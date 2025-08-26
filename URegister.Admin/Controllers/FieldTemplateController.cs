using Azure.Core;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Models.Service;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.ObjectsCatalog;
using static FastExpressionCompiler.ExpressionCompiler;
using static URegister.ObjectsCatalog.ObjectsCatalogGrpc;

namespace URegister.Admin.Controllers
{
    [Display(Name = "Шаблони за полета")]
    public class FieldTemplateController(ObjectsCatalogGrpcClient objectCatalogGrpcClient) : BaseController
    {
        [Display(Name = "Зареждане на списък с шаблони за полета")]
        public IActionResult Index()
        {
            return View();
        }

        [Display(Name = "Задаване на падащ списък с типове полета за шаблони")]
        public async Task SetViewBag()
        {
            var response = await objectCatalogGrpcClient.GetFieldsListAsync(new Google.Protobuf.WellKnownTypes.Empty());
            ViewBag.FieldTypeId_ddl = response.FieldTypes
                .Where(x => x.IsComplex)
                .Select(x => new SelectListItem { 
                   Value = x.FieldTypeId.ToString(),
                   Text = x.Label} )
                .ToList();
        }

        [Display(Name = "Зареждане на форма за добавяне на нов шаблон за поле")]
        public async Task<IActionResult> Add()
        {
            await SetViewBag();
            var model = new FieldTemplateVM();
            return View(nameof(Edit), model);
        }

        [Display(Name = "Зареждане на форма за редакция на шаблон за поле")]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await objectCatalogGrpcClient.GetFieldTemplateAsync(new FieldTemplateRequest { Id = id });
            var model = new FieldTemplateVM
            {
                Id = response.FieldTemplate.Id,
                FieldTypeId = response.FieldTemplate.FieldTypeId,
                Name = response.FieldTemplate.Name,
                FieldType = response.FieldTemplate.FieldType,
                FieldTypeName = response.FieldTemplate.FieldTypeName,
            };
            await SetViewBag();
            return View(model);
        }

        [Display(Name = "Зареждане на форма за редакция на съдържание на шаблон за поле")]
        public async Task<IActionResult> EditContent(int id)
        {
            var response = await objectCatalogGrpcClient.GetFieldTemplateContentAsync(new FieldTemplateRequest { Id = id });
            var model = new FieldTemplateContentVM
            {
                Id = response.FieldTemplate.Id,
                FieldTypeId = response.FieldTemplate.FieldTypeId,
                Name = response.FieldTemplate.Name,
                Content = response.FieldTemplate.Content,
                ContentText = response.FieldTemplate.ContentText,
                FieldType = response.FieldTemplate.FieldType,
                FieldTypeName = response.FieldTemplate.FieldTypeName,
            };
            await SetViewBag();
            return View(model);
        }

        /// <summary>
        /// Запис на  тип услуга
        /// </summary>
        /// <param name="model">Модел на услуга</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис или редакция на шаблон за поле")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FieldTemplateVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await objectCatalogGrpcClient.AppendUpdateFieldTemplateAsync(new FieldTemplateMessage
                    {
                        Id = model.Id,
                        Name = model.Name,
                        FieldTypeId = model.FieldTypeId,
                        
                    });
                    SetSuccessMessage(model.Id <= 0 ? "Успешно добавена бланка" : "Успешна редакция на бланка");
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    SetErrorMessage("Проблем при запис!");
                }
            }
            await SetViewBag();
            return View(model);
        }
        /// <summary>
        /// Запис на  тип услуга
        /// </summary>
        /// <param name="model">Модел на услуга</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Запис на съдържание на шаблон за поле")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditContent(FieldTemplateContentVM model)
        {
            try
            {
                await objectCatalogGrpcClient.UpdateFieldTemplateContentAsync(new FieldTemplateContentMessage
                {
                    Id = model.Id,
                    Name = model.Name,
                    FieldTypeId = model.FieldTypeId,
                    Content = model.Content,
                    ContentText = model.ContentText
                });
                SetSuccessMessage("Успешна записана бланка");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                SetErrorMessage("Проблем при запис!");
            }
            await SetViewBag();
            return View(model);
        }


        /// <summary>
        /// Списък на  услуги
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с шаблони за полета")]
        public async Task<IActionResult> GetFieldTemplateList(IDataTablesRequest request)
        {
            var response = await objectCatalogGrpcClient.GetFieldTemplateListAsync(new FieldTemplateListRequest{
                DataTableRequest = request!.GetDataTablesRequestProto()
            });
            return request.GetResponseServerPaging(response.FieldTemplates, response.CountAll);
        }

        [Display(Name = "Извличане на вътрешни полета за тип поле")]
        public async Task<JsonResult> GetInnerFields(string fieldType)
        {
            var reply = await objectCatalogGrpcClient.GetFieldAsync(new CatalogFieldRequest { FieldType = fieldType });

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
            var formField = JsonSerializer.Deserialize<FormField>(reply.Data, options);
            var paramList = formField.Fields.Select(x => new BlanksTemplateParamVM
            {
                Label = x.Label,
                Name = x.Name,
            })
            .ToList();
            return Json(paramList);
        }

        // <summary>
        /// Изтриване на номенклатура
        /// </summary>
        /// <param name="id">Идентификатор на номенклатура</param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Изтриване на шаблон за поле")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteFieldTemplate(int id)
        {
            ResultStatus result = await objectCatalogGrpcClient.DeleteFieldTemplateAsync(new FieldTemplateRequest
            {
                Id = id
            });

            if (result.Code == ResultCodes.Ok)
            {
                SetSuccessMessage("Бланката е изтрита успешно");
            }
            else
            {
                SetErrorMessage(result.Message);
            }

            return Json(null);
        }
    }
}
