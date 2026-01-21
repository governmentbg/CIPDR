using DataTables.AspNet.Core;
using IO.SignTools.Contracts;
using IO.SignTools.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Linq.Expressions;
using URegister.Infrastructure.Constants;
using URegister.Core.Models.Previewer;
using URegister.Core.Utils;
using URegister.Core.Models;
using URegister.Core.Contracts;
using System.ComponentModel.DataAnnotations;

namespace URegister.Controllers
{
    /// <summary>
    /// Контролер за файловете
    /// </summary>
    public class FilesController : BaseController
    {
        private readonly ICommonFileService commonFileService;
        private readonly ILogger<FilesController> logger;
        private readonly IIOSignToolsService signToolsService;
        private readonly IUserContext userContext;
        private readonly IConfiguration config;

        /// <summary>
        /// Контролер за файловете
        /// </summary>
        /// <param name="_commonService"></param>
        /// <param name="_nomenclatureService"></param>
        /// <param name="_logger"></param>
        /// <param name="_signToolsService"></param>
        /// <param name="_config"></param>
        public FilesController(ICommonFileService _commonFileService,
                               ILogger<FilesController> _logger,
                               IIOSignToolsService _signToolsService,
                               IUserContext _userContext,
                               IConfiguration _config)
        {
            commonFileService = _commonFileService;
            logger = _logger;
            signToolsService = _signToolsService;
            config = _config;
            userContext = _userContext;
        }

        /// <summary>
        /// Преглед на документ
        /// </summary>
        /// <param name="id">Идентификатор на файл</param>
        /// <param name="returnUrl">URL за връщане след прегледа</param>
        /// <param name="returnSignUrl">URL за връщане</param>
        /// <returns></returns>
        [Display(Name = "Преглед")]
        public async Task<IActionResult> Preview(string id, bool clearautosave = false, string? returnUrl = null, string? returnSignUrl = null)
        {

            string url = returnUrl ?? Url.Action("Index", "Home", null, Request.Scheme, Request.Host.Value) ?? "/";
            string signUrl = returnSignUrl ?? Url.Action("Index", "Home", null, Request.Scheme, Request.Host.Value) ?? "/";
            FileInfoModel fileItem = await commonFileService.GetFileInfo(id);
            var model = new PreviewInfoViewModel(id, url, signUrl);

            model.ShowPreview = fileItem.Size < PreviewConstants.MaxFileSize;
            model.IsSigned = fileItem.IsSigned;

            if (fileItem.ContentType?.ToLower() == PreviewConstants.ContentType.PDF)
            {
                model.SignatureType = PreviewConstants.SignatureType.Embeded;
                model.Reason = "Signature demo application";
            }
            else
            {
                model.SignatureType = PreviewConstants.SignatureType.Detached;
            }

            model.ContentType = fileItem.ContentType;
            model.FileName = fileItem.Name;

            return View(model);
        }

        [Display(Name = "Подготви PDF за подпис")]
        [HttpPost]
        public async Task<IActionResult> PreparePdfForSignature([FromBody] PdfVisualSignatureData data)
        {
            string hash;
            string tempPdfId;
            if (data?.FileId == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            //var position = new
            //{
            //    x = config.GetValue<float?>("VisualSignature:Position:X") ?? 400,
            //    y = config.GetValue<float?>("VisualSignature:Position:Y") ?? 750,
            //    width = config.GetValue<float?>("VisualSignature:Position:Width") ?? 150,
            //    height = config.GetValue<float?>("VisualSignature:Position:Height") ?? 60,
            //};

            (var pdfBytes, _, _) = await commonFileService.GetFileById(data.FileId);
            VisualSignaturOptions options = new VisualSignaturOptions()
            {
                Cordinates = new iText.Kernel.Geom.Rectangle(data.Position.X, data.Position.X, data.Position.Width, data.Position.Height),
                Location = "ИСЦИПР",
                PageNumber = data.Position.Page,
                Reason = data.Reason,
                Signer = data.SignerName,
                SignerCertificate = data.SignerCert
            };

            using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
            {
                (hash, tempPdfId) = await signToolsService
                        .GetPdfHashForVisualSignature(pdfStream, options);
            }

            return Json(new { hash, tempPdfId });
        }

        [Display(Name = "Подписване")]
        [HttpPost]
        public async Task<IActionResult> SignPdf(PreviewInfoViewModel model)
        {
            byte[] signedPdf;

            if (string.IsNullOrEmpty(model.TempPdfId) || string.IsNullOrEmpty(model.Signature))
            {
                TempData[MessageConstant.ErrorMessage] = "Неуспешно подписване";

                return Redirect(model.ReturnUrl.AbsoluteUri);
            }

            try
            {
                (var cert, var time) = CryptoHelper.GetSignatureInfoFromCMS(Convert.FromBase64String(model.Signature));

                if (userContext.CertificateNumber.ToUpper() != cert.SerialNumber.ToUpper())
                {
                    TempData[MessageConstant.ErrorMessage] = "Подписът не отговаря на подписа на активния потребител";

                    return Redirect(model.ReturnUrl.AbsoluteUri);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FilesController/Sign");
                TempData[MessageConstant.ErrorMessage] = "Грешка по време на валидация на подпис";

                return Redirect(model.ReturnUrl.AbsoluteUri);
            }

            try
            {
                (signedPdf, _) = await signToolsService.EmbedPdfSignature(model.TempPdfId, model.Signature);
                using (MemoryStream ms = new(signedPdf))
                {
                    signedPdf = signToolsService.AddLTV(ms);
                }
                await commonFileService.ChangeFile(model.FileId, signedPdf, null);
            }
            catch (SignatureValidationException sve)
            {
                logger.LogError(sve, "Files/SignVisual");
                TempData[MessageConstant.ErrorMessage] = "Невалиден подпис";

                return Redirect(model.ReturnUrl.AbsoluteUri);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Files/SignVisual");
                TempData[MessageConstant.ErrorMessage] = "Неуспешно подписване";

                return Redirect(model.ReturnUrl.AbsoluteUri);
            }

            TempData[MessageConstant.SuccessMessage] = "Успешно подписване";

            return Redirect(model.ReturnSignUrl.AbsoluteUri);
        }
        /// <summary>
        /// Вграждане на файл
        /// </summary>
        /// <param name="id">Идентификатор на фаил</param>
        /// <returns></returns>
        [Display(Name = "Преглеждане на файл")]
        public async Task<FileResult> GetFileInline(string id)
        {
            (byte[] fileBytes,  var model, var contentType) = await commonFileService.GetFileById(id);

            var contentDispositionHeader = new ContentDisposition
            {
                Inline = true,
                FileName = HttpUtility.UrlPathEncode(model.FileName).Replace(",", "%2C")
            };

            Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());

          
            return File(fileBytes, contentType);
        }
    }
}
