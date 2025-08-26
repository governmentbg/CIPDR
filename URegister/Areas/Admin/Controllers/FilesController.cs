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
using MHRegistries.Core.Utils;
using URegister.Core.Models;
using URegister.Core.Contracts;

namespace URegister.Areas.Admin.Controllers
{
    /// <summary>
    /// Контролер за файловете
    /// </summary>
    public class FilesController : BaseController
    {
        private readonly ICommonFileService commonFileService;
        private readonly ILogger<FilesController> logger;
        private readonly IIOSignToolsService signToolsService;
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
                               IConfiguration _config)
        {
            commonFileService = _commonFileService;
            logger = _logger;
            signToolsService = _signToolsService;
            config = _config;
        }
/*
        /// <summary>
        /// Метод свалящ файла
        /// </summary>
        /// <param name="sourceType">Тип на файла</param>
        /// <param name="sourceId">Идентификатор на обекта</param>
        /// <returns></returns>
        public async Task<FileResult?> DownloadFile(int sourceType, Guid sourceId)
        {
            FileViewModel? fileViewModel = await commonFileService.DownloadFile(sourceType, sourceId);

            if (fileViewModel != null)
            {
                return File(Convert.FromBase64String(fileViewModel.FileContentBase64 ?? string.Empty), fileViewModel.ContentType, fileViewModel.FileName);
            }

            return null;
        }

        /// <summary>
        /// Метод свалящ файла
        /// </summary>
        /// <param name="sourceType">Тип на файла</param>
        /// <param name="sourceId">Идентификатор на обекта</param>
        /// <param name="fileId">Идентификатор на файла</param>
        /// <returns></returns>
        public async Task<FileResult?> DownloadFileByFileId(int sourceType, Guid sourceId, string fileId)
        {
            FileViewModel? fileViewModel = await commonService.DownloadFileByFileId(sourceType, sourceId, fileId);

            if (fileViewModel != null)
            {
                return File(Convert.FromBase64String(fileViewModel.FileContentBase64 ?? string.Empty), fileViewModel.ContentType, fileViewModel.FileName);
            }

            return null;
        }

      

        /// <summary>
        /// Добавяне на файл
        /// </summary>
        /// <param name="sourceType">Тип на файла</param>
        /// <param name="sourceId">Идентификатор на обекта</param>
        /// <param name="backUrl">Линк за връщане</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AddFile(int sourceType, Guid sourceId, string backUrl)
        {
            FileEditViewModel model = new()
            {
                Id = Guid.Empty,
                SourceType = sourceType,
                SourceId = sourceId,
                BackUrl = backUrl
            };
            
            return View(nameof(EditFile), model);
        }

        /// <summary>
        /// Запис на файл
        /// </summary>
        /// <param name="model">Модел за запис</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> EditFile(FileEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                SetErrorMessage("Възникна проблем при валидация на данните.");
                return View(nameof(EditFile), model);
            }

            string _isvalid = IsValidateFile(model);
            if (_isvalid != string.Empty)
            {
                SetErrorMessage(_isvalid);
                return View(nameof(EditFile), model);
            }

            Guid? id = await commonService.SaveFile(model);
            if (id != null)
            {
                SetSuccessMessage("Записът премина успешно.");
                return Redirect(model.BackUrl);
            }
            else
            {
                SetErrorMessage("Проблем по време на запис");
            }

            return View(nameof(EditFile), model);
        }

       
        /// <summary>
        /// Валидация на данни преди запис
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private string IsValidateFile(FileEditViewModel model)
        {
            if (model.FileUpload.File == null)
                return "Изберете файл";

            if (model.FileTypeId == null || model.FileTypeId < 1)
                return "Изберете тип файл";

            //if (string.IsNullOrEmpty(model.FileTitle))
            //    return "Въведете кратко описание";

            return string.Empty;
        }

        /// <summary>
        /// Сторниране на файл
        /// </summary>
        /// <param name="id">Идентификатор на файл</param>
        /// <param name="backUrl">Линк за връщане</param>
        /// <returns></returns>
        public async Task<IActionResult> ExpiredFile(Guid id, string backUrl)
        {
            bool isSave = await commonService.ExpiredFile(id);

            if (isSave)
            {
                SetSuccessMessage("Заличаването премина успешно.");
            }
            else
            {
                SetErrorMessage("Проблем по време на заличаване");
            }

            return Json(new { redirectUrl = backUrl });
        }

        #region FilePreview

        /// <summary>
        /// Истегляне на подпис
        /// </summary>
        /// <param name="id">Идентификатор на файл</param>
        /// <returns></returns>
        public async Task<IActionResult> DownloadSignature(string id)
        {
            FileInfoModel info = await commonService.GetFileInfo(id);
            string name = $"{info.Name}.p7b";

            if (!info.IsSigned || info.Signature == null)
            {
                return NotFound();
            }

            return File(Convert.FromBase64String(info.Signature), "application/pkcs7-mime", name);
        }

        /// <summary>
        /// Вграждане на файл
        /// </summary>
        /// <param name="id">Идентификатор на фаил</param>
        /// <returns></returns>
        public async Task<FileResult> GetFileInline(string id)
        {
            FileViewModel file = await commonService.GetFileDataByFileId(id);

            var contentDispositionHeader = new ContentDisposition
            {
                Inline = true,
                FileName = HttpUtility.UrlPathEncode(file.FileName).Replace(",", "%2C")
            };

            Response.Headers.Add("Content-Disposition", contentDispositionHeader.ToString());

            return File(file.FileByteArray, file.ContentType);
        }

        /// <summary>
        /// Преглед на документ
        /// </summary>
        /// <param name="id">Идентификатор на файл</param>
        /// <param name="returnUrl">URL за връщане след прегледа</param>
        /// <param name="returnSignUrl">URL за връщане</param>
        /// <returns></returns>
        public async Task<IActionResult> Preview(string id, string? returnUrl = null, string? returnSignUrl = null)
        {

            string url = returnUrl ?? Url.Action("Index", "Home", null, Request.Scheme, Request.Host.Value) ?? "/";
            string signUrl = returnSignUrl ?? Url.Action("Index", "Home", null, Request.Scheme, Request.Host.Value) ?? "/";
            FileInfoModel fileItem = await commonService.GetFileInfo(id);
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

        /// <summary>
        /// Подписване на файл
        /// </summary>
        /// <param name="model">Информация от прегледа</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Sign(PreviewInfoViewModel model)
        {
            FileViewModel file = await commonService.GetFileDataByFileId(model.FileId);
            bool isValid = false;

            try
            {
                isValid = await signToolsService.ValidateFile(file.FileByteArray, model.Signature);
            }
            catch (SignatureValidationException sve)
            {
                logger.LogError(sve, "FilesController/Sign");
                TempData[MessageConstant.ErrorMessage] = "Невалиден подпис";

                return Redirect(model.ReturnUrl.AbsoluteUri);
            }

            try
            {
                (var cert, var time) = CryptoHelper.GetSignatureInfoFromCMS(Convert.FromBase64String(model.Signature));

                // Тук можете да проверите дали сертификата на подписващия е сертификата на в
                // логнатия потребител или някаква друга проверка, която ви е необходима преди подписване
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FilesController/Sign");
                TempData[MessageConstant.ErrorMessage] = MessageConstant.Values.ErrorValidatingSignature;

                return Redirect(model.ReturnUrl.AbsoluteUri);
            }

            if (!isValid)
            {
                TempData[MessageConstant.ErrorMessage] = MessageConstant.Values.InvalidSignature;

                return Redirect(model.ReturnUrl.AbsoluteUri);
            }

            bool success = await commonService.SetSignature(model.FileId, model.Signature);

            if (!success)
            {
                TempData[MessageConstant.ErrorMessage] = MessageConstant.Values.UnsuccessfulSigning;

                return Redirect(model.ReturnUrl.AbsoluteUri);
            }

            TempData[MessageConstant.SuccessMessage] = MessageConstant.Values.SuccessfulSigning;

            return Redirect(model.ReturnSignUrl.AbsoluteUri);
        }

        /// <summary>
        /// Проверка на подпис
        /// </summary>
        /// <param name="fileId">Идентификатор на файл</param>
        /// <returns></returns>
        public async Task<IActionResult> VerifySignature(string fileId)
        {
            FileViewModel file = await commonService.GetFileDataByFileId(fileId);
            bool isValid = false;
            CertificateInfoResponse? certificateInfoResponse = null;
            DateTime? signingTime = null;
            string? name = null;

            try
            {
                
                isValid = await signToolsService.ValidateFile(file.FileByteArray, file.Signature);
                (var certificate, signingTime) =
                    CryptoHelper.GetSignatureInfoFromCMS(Convert.FromBase64String(file.Signature ?? string.Empty));

                if (config.GetValue<string>("VerificationService:Token") != null)
                {
                    certificateInfoResponse = await signToolsService.GetCertificateInfo(certificate, true);
                    name = certificateInfoResponse.Name;
                    isValid &= certificateInfoResponse.Valid;
                }
                else
                {
                    name = certificate.GetNameInfo(X509NameType.SimpleName, false);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FilesController/Sign");
                isValid = false;
            }

            return Ok(new
            {
                isValid,
                date = signingTime?.ToString("dd.MM.yyyy"),
                time = signingTime?.ToString("HH:mm"),
                name
            });
        }

        /// <summary>
        /// Подписване на PDF
        /// </summary>
        /// <param name="model">Информация от прегледа</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SignPdf(PreviewInfoViewModel model)
        {
            try
            {
                byte[] signedPdf;

                if (string.IsNullOrEmpty(model.TempPdfId) || string.IsNullOrEmpty(model.Signature))
                {
                    TempData[MessageConstant.ErrorMessage] = MessageConstant.Values.UnsuccessfulSigning;

                    return Redirect(model.ReturnUrl.AbsoluteUri);
                }

                try
                {
                    (var cert, var time) = CryptoHelper.GetSignatureInfoFromCMS(Convert.FromBase64String(model.Signature));

                    // Тук можете да проверите дали сертификата на подписващия е сертификата на в
                    // логнатия потребител или някаква друга проверка, която ви е необходима преди подписване
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "FilesController/Sign");
                    TempData[MessageConstant.ErrorMessage] = "Грешка по време на валидация на подпис";

                    return Redirect(model.ReturnUrl.AbsoluteUri);
                }

                (signedPdf, _) = await signToolsService.EmbedPdfSignature(model.TempPdfId, model.Signature);

                using (MemoryStream ms = new MemoryStream(signedPdf))
                {
                    // Добавяне на LTV (Long Time Verification) 
                    // Това е валидация на подписа въз основа на времето, в което е направен
                    signedPdf = signToolsService.AddLTV(ms);
                }

                string? fileId = await commonService.ReplaceFile(signedPdf, model.FileId);
                if (string.IsNullOrEmpty(fileId))
                {
                    TempData[MessageConstant.ErrorMessage] = MessageConstant.Values.UnsuccessfulSigning;
                    return Redirect(model.ReturnUrl.AbsoluteUri);
                }

                bool uploadResult = await commonService.SetSignature(fileId);

                if (!uploadResult)
                {
                    TempData[MessageConstant.ErrorMessage] = MessageConstant.Values.UnsuccessfulSigning;

                    return Redirect(model.ReturnUrl.AbsoluteUri);
                }

                TempData[MessageConstant.SuccessMessage] = MessageConstant.Values.SuccessfulSigning;

                return Redirect(model.ReturnSignUrl.AbsoluteUri);
            }
            catch (Exception ex)
            {
                return Redirect(model.ReturnUrl.AbsoluteUri);
            }
        }

        /// <summary>
        /// Проверка на подпис в PDF
        /// </summary>
        /// <param name="fileId">Идентификатор на файл</param>
        /// <returns></returns>
        public async Task<IActionResult> VerifyPdfSignature(string fileId)
        {
            FileViewModel file = await commonService.GetFileDataByFileId(fileId);
            bool isValid = false;
            IOSignerInfo? signer = null;

            try
            {
                isValid = await signToolsService.ValidatePdf(file.FileByteArray);

                using (MemoryStream ms = new MemoryStream(file.FileByteArray))
                {
                    signer = signToolsService.GetSignerInfo(ms).FirstOrDefault();
                }

                isValid &= signer != null;

                if (!isValid)
                {
                    return Ok(new { isValid });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FilesController/Sign");
                isValid = false;
            }

            return Ok(new
            {
                isValid,
                date = signer != null ? signer.SignedOn.ToString("dd.MM.yyyy") : string.Empty,
                time = signer != null ? signer.SignedOn.ToString("HH:mm") : string.Empty,
                name = signer != null ? signer.Name : string.Empty
            });
        }

        /// <summary>
        /// Подготвяне на PDF за подпис
        /// </summary>
        /// <param name="data">Информация за подпис</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [HttpPost]
        public async Task<IActionResult> PreparePdfForSignature([FromBody] PdfVisualSignatureData data)
        {
            try
            {
                string hash;
                string tempPdfId;
                if (data?.FileId == null)
                {
                    throw new ArgumentNullException(nameof(data));
                }

                FileViewModel file = await commonService.GetFileDataByFileId(data.FileId);
                byte[] pdfBytes = file.FileByteArray;

                // Ако е избран визуален подпис
                if (data.Visual)
                {
                    VisualSignaturOptions options = new VisualSignaturOptions()
                    {
                        Cordinates = new iText.Kernel.Geom.Rectangle(
                            data.Position.X,
                            data.Position.Y,
                            data.Position.Width,
                            data.Position.Height),
                        Location = "НЗИС",
                        PageNumber = data.Position.Page,
                        Reason = data.Reason,
                        Signer = data.SignerName,
                        SignerCertificate = data.SignerCert
                    };

                    using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
                    {
                        try
                        {
                            (hash, tempPdfId) = await signToolsService
                                .GetPdfHashForVisualSignature(pdfStream, options);
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "FilesController/PreparePdfForSignature");
                            throw;
                        }

                    }
                }
                // Ако подписа не се визуализира в PDF-а
                else
                {
                    using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
                    {
                        (hash, tempPdfId) = await signToolsService
                                .GetPdfHash(pdfStream, data.Reason, "НЗИС");
                    }
                }

                return Json(new { hash, tempPdfId });
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException(nameof(data));
            }
        }

        #endregion
*/
    }
}
