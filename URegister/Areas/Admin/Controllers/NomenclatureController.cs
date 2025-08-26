using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Models.Nomenclature;
using URegister.Infrastructure.Constants;
using URegister.NomenclaturesCatalog;

namespace URegister.Areas.Admin.Controllers
{
    /// <summary>
    /// Контролер за операциите с номенклатури
    /// </summary>
    [Area("Admin")]
    [Display(Name = "Номенклатури")]
    public class NomenclatureController(NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
                                        IRegisterService registerService,
                                        INomenclatureClientService nomenclatureClientService,
                                        ILogger<DesignerController> logger) : BaseController
    {

        /// <summary>
        /// Връща номенклатурните стойности за тип по низ от символи
        /// </summary>
        /// <param name="query">Низ от символи за търсене</param>
        /// <param name="nomenclatureType">Номенклатурен тип</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Извличане на номенклатурни стойности за автодопълване")]
        public async Task<JsonResult> GetAutocompleteValues(string query, string nomenclatureType)
        {
            NomenclaturePublicRequest getNomenclaturesRequest = new NomenclaturePublicRequest
            {
                RegisterId = await registerService.GetCurrentRegisterId(),
                NomenclatureTypes = { nomenclatureType },
                FilterValue = query
            };

            try
            {
                URegister.NomenclaturesCatalog.NomenclaturePublicResponse nomenclatureResult =
                    await nomenclatureGrpcClient.GetNomenclaturePublicAsync(getNomenclaturesRequest);

                if (nomenclatureResult.ResultStatus.Code != ResultCodes.Ok)
                {
                    logger.LogError($"GetNomenclaturePublicAsync неуспешен в {nameof(GetAutocompleteValues)}");
                    return new JsonResult(string.Empty);
                }

                if (!nomenclatureResult.NomenclatureTypes.Any())
                {
                    logger.LogWarning($"Не са върнати резултати за номенклатура {nomenclatureType} в {nameof(GetAutocompleteValues)}");
                    return new JsonResult(string.Empty);
                }

                var result = nomenclatureResult.NomenclatureTypes.First().CodeableConcepts
                    .Select(c => new
                    {
                        title = c.Value,
                        id = c.Code
                    });

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Проблем при зареждане на номенклатурите в {nameof(GetAutocompleteValues)}");
                return Json(string.Empty);
            }
        }

        /// <summary>
        /// Връща населените места по низ за търсене
        /// </summary>
        /// <param name="query">Низ за търсене</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Извличане на населени места")]
        public async Task<JsonResult> GetEkatteValues(string query)
        {
            EkattePublicRequest getEkatteRequest = new EkattePublicRequest
            {
                RegisterId = await registerService.GetCurrentRegisterId(),
                FilterValue = query
            };

            try
            {
                EkattePublicResponse ekattePublicResponse =
                    await nomenclatureGrpcClient.GetEkattePublicAsync(getEkatteRequest);

                if (ekattePublicResponse.ResultStatus.Code != ResultCodes.Ok)
                {
                    logger.LogError($"GetEkattePublicAsync неуспешен в {nameof(GetEkatteValues)}");
                    return new JsonResult(string.Empty);
                }

                // Use ExpandoObject for dynamic structure
                dynamic result = new ExpandoObject();
                result.results = new ExpandoObject();

                foreach (var category in ekattePublicResponse.Categories)
                {
                    dynamic categoryObj = new ExpandoObject();
                    categoryObj.name = category.Category;

                    // Add city details in the required format
                    var cityList = new List<dynamic>();
                    foreach (var city in category.Cities)
                    {
                        dynamic cityObj = new ExpandoObject();
                        cityObj.title = city.Name;
                        cityObj.value = city.Code;
                        cityList.Add(cityObj);
                    }

                    categoryObj.results = cityList;

                    // Add each category to the results
                    ((IDictionary<string, object>)result.results).Add(category.Category, categoryObj);
                }

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Проблем при зареждане на EKATTE данни в {nameof(GetEkatteValues)}");
                return Json(string.Empty);
            }
        }
        /// <summary>
        /// Връща списък за падащо меню от номенклатурни стойности според номенклатурен код
        /// </summary>
        /// <param name="nomenclatureCode">код на номенклатура</param>
        /// <param name="holderCode">код на подстойност</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Извличане на номенклатурни стойности за падащо меню")]
        public async Task<IActionResult> GetNomenclatureValues(string nomenclatureCode, string holderCode, string query = "")
        {
            try
            {
                var request = new NomenclatureHolderRequest
                {
                    RegisterId = await registerService.GetCurrentRegisterId(),
                    NomenclatureType = nomenclatureCode,
                    Holder = holderCode,
                    FilterValue = query
                };

                NomenclaturePublicResponse response = await nomenclatureGrpcClient.GetNomenclatureOnHolderPublicAsync(request);

                if (response.ResultStatus.Code != ResultCodes.Ok)
                {
                    logger.LogWarning($"Проблем при извличане на данни в {nameof(GetNomenclatureValues)}");
                    return Json(new List<SelectListItem>());
                }

                var list = response.NomenclatureTypes.First().CodeableConcepts.Select(c => new SelectListItem
                {
                    Text = c.Value,
                    Value = c.Code
                });

                return Json(list);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Проблем при зареждане на данни в {nameof(GetNomenclatureValues)}");
                return Json(new List<SelectListItem>());
            }

        }

        /// <summary>
        /// Връща списък за падащо меню от номенклатурни стойности според номенклатурен код
        /// </summary>
        /// <param name="nomenclatureCode">код на номенклатура</param>
        /// <param name="holderCode">код на подстойност</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Извличане на номенклатурни стойности за автодопълване")]
        public async Task<IActionResult> GetNomenclatureValuesForAutocomplete(string nomenclatureCode, string holderCode, string query = "")
        {
            try
            {
                var request = new NomenclatureHolderRequest
                {
                    RegisterId = await registerService.GetCurrentRegisterId(),
                    NomenclatureType = nomenclatureCode,
                    Holder = holderCode,
                    FilterValue = query
                };

                NomenclaturePublicResponse response = await nomenclatureGrpcClient.GetNomenclatureOnHolderPublicAsync(request);

                if (response.ResultStatus.Code != ResultCodes.Ok)
                {
                    logger.LogWarning($"Проблем при извличане на данни в {nameof(GetNomenclatureValues)}");
                    return Json(new List<SelectListItem>());
                }

                var result = response.NomenclatureTypes.First().CodeableConcepts
                   .Select(c => new
                   {
                       title = c.Value,
                       id = c.Code
                   });

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Проблем при зареждане на данни в {nameof(GetNomenclatureValuesForAutocomplete)}");
                return Json(string.Empty);
            }
        }
       
        /// <summary>
        /// Връща населените места по низ за търсене
        /// </summary>
        /// <param name="query">Низ за търсене</param>
        /// <param name="nomenclatureType">Тип номенклатура</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Извличане на номенклатурни стойности с категории за автодопълване")]
        public async Task<JsonResult> GetAutocompleteWithCategoryValues(string query, string nomenclatureType)
        {
            NomenclatureWithHolderValuesRequest request = new NomenclatureWithHolderValuesRequest
            {
                RegisterId = await registerService.GetCurrentRegisterId(),
                FilterValue = query,
                NomenclatureCode = nomenclatureType
            };
            try
            {
                URegister.NomenclaturesCatalog.NomenclatureWithHolderResponse response =
                    await nomenclatureGrpcClient.GetNomenclatureWithHolderValuesAsync(request);

                if (response.ResultStatus.Code != ResultCodes.Ok)
                {
                    logger.LogError($"GetNomenclatureWithHolderValuesAsync неуспешен в {nameof(GetAutocompleteWithCategoryValues)}");
                    return new JsonResult(string.Empty);
                }

                dynamic result = new ExpandoObject();
                result.results = new ExpandoObject();

                foreach (var category in response.Categories)
                {
                    dynamic categoryObj = new ExpandoObject();
                    categoryObj.name = category.Category;

                    var cityList = new List<dynamic>();
                    foreach (var city in category.Entities)
                    {
                        dynamic cityObj = new ExpandoObject();
                        cityObj.title = city.Name;
                        cityObj.value = city.Code;
                        cityList.Add(cityObj);
                    }

                    categoryObj.results = cityList;

                    ((IDictionary<string, object>)result.results).Add(category.Category, categoryObj);
                }

                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Проблем при зареждане на данни за номенклатура в {nameof(GetAutocompleteWithCategoryValues)}");
                return Json(string.Empty);
            }
        }
        #region edit admin

        /// <summary>
        /// Списък номенклатурни типове за регистър
        /// </summary>
        /// <returns></returns>
        [Display(Name = "Зареждане на списък с номенклатурни типове за регистър")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> IndexTypeRegister()
        {
            var filter = new NomenclatureTypeRegisterFilterVM
            {
                RegisterId = await registerService.GetCurrentRegisterId(),
            };
            return View(filter);
        }


        /// <summary>
        /// Списък на номенклатурнани стойности за регистър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с номенклатурни стойности за регистър")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetCodeableConceptRegisterList(IDataTablesRequest request, NomenclatureTypeRegisterFilterVM filter)
        {
            filter.RegisterId = await registerService.GetCurrentRegisterId();
            return await nomenclatureClientService.GetCodeableConceptRegisterList(request, filter);
        }


        /// <summary>
        /// Списък на номенклатурнани типове за регистър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Извличане на списък с номенклатурни типове за регистър")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> GetNomenclatureTypeRegisterList(IDataTablesRequest request, NomenclatureTypeRegisterFilterVM filter)
        {
            filter.RegisterId = await registerService.GetCurrentRegisterId();
            return await nomenclatureClientService.GetNomenclatureTypeRegisterList(request, filter);
        }


        /// <summary>
        /// Запис на влогове за номенклатурен тип
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Актуализиране на номенклатурен тип")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<JsonResult> UpdateNomenclatureTypeRegister([FromBody] NomenclatureTypeRegisterUpdateVM model)
        {
            model.RegisterId = await registerService.GetCurrentRegisterId();
            var result = await nomenclatureClientService.UpdateNomenclatureTypeRegister(model);
            return Json(result);
        }

        /// <summary>
        /// Запис допустими номенклатурни стойности
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Актуализиране на номенклатурна стойност")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<JsonResult> UpdateCodeableConceptRegister([FromBody] CodeableConceptRegisterUpdateVM model)
        {
            model.RegisterId = await registerService.GetCurrentRegisterId();
            var result = await nomenclatureClientService.UpdateCodeableConceptRegister(model);
            return Json(result);
        }


        /// <summary>
        /// Списък на номенклатурни стойности за регистър
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        [Display(Name = "Зареждане на списък с номенклатурни стойности за регистър")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> IndexRegister(string nomenclatureType)
        {
            var registerId = await registerService.GetCurrentRegisterId();
            var response = await nomenclatureGrpcClient.GetNomenclatureTypeRegisterOnTypeAsync(
                new GetNomenclatureTypeRegisterOnTypeRequest
                {
                    Type = nomenclatureType,
                    RegisterId = registerId
                });
            var model = new NomenclatureTypeRegisterFilterVM
            {
                Type = response.Type,
                Name = response.Name,
                RegisterId = registerId,
                IsValidAllType = response.IsValidAll
            };
            return View(model);
        }

        /// <summary>
        /// Добавяне на номенклатурна стойност 
        /// </summary>
        /// <param name="nomenclatureType">Име на новия тип стойност</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за добавяне на номенклатурна стойност")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Add(string nomenclatureType)
        {
            var response = await nomenclatureGrpcClient.CreateNewCodeableConceptAsync(new CreateNewCodeableConceptRequest { Type = nomenclatureType });
            var model = nomenclatureClientService.GrpcCodeableConceptToModel(response);
            return View(nameof(Edit), model);
        }

        /// <summary>
        /// Запис на добавена номенклатурна стойност 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Добавяне на номенклатурна стойност")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Add(CodeableConceptVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            CodeableConceptRequest request = nomenclatureClientService.CodeableConceptToGrpcModel(model, (int)CodeableConceptStatus.New);
            var result = await nomenclatureGrpcClient.AddCodeableConceptAsync(request);
            if (result?.Code == ResultCodes.Ok)
            {
                SetSuccessMessage("Успешно добавен номенклатурна стойност");
                return RedirectToAction("IndexRegister", new { nomenclatureType = model.Type });
            }
            else
            {
                SetErrorMessage(result?.Message ?? "Проблем при запис!");
            }
            return View(nameof(Edit), model);
        }


        /// <summary>
        /// Редакция на номенклатурна стойност 
        /// </summary>
        /// <param name="id">Идентификатор на стойността</param>
        /// <returns></returns>
        [HttpGet]
        [Display(Name = "Зареждане на форма за редакция на номенклатурна стойност")]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await nomenclatureGrpcClient.GetCodeableConceptAsync(new GetCodeableConceptRequest { Id = id });
            var model = nomenclatureClientService.GrpcCodeableConceptToModel(response);
            return View(model);
        }

        /// <summary>
        /// Запис на редактирана номенклатурна стойност 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Display(Name = "Редактиране на номенклатурна стойност")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = UserRoles.Admin)]
        public async Task<IActionResult> Edit(CodeableConceptVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            CodeableConceptRequest request = nomenclatureClientService.CodeableConceptToGrpcModel(model, (int)CodeableConceptStatus.New);
            var result = await nomenclatureGrpcClient.EditCodeableConceptAsync(request);
            if (result?.Code == ResultCodes.Ok)
            {
                SetSuccessMessage("Успешно редактирана номенклатурна стойност");
                return RedirectToAction("IndexRegister", new { nomenclatureType = model.Type });
            }
            else
            {
                SetErrorMessage(result?.Message ?? "Проблем при запис!");
            }
            return View(model);
        }

        /// <summary>
        /// Partial за допълнителна колона
        /// </summary>
        /// <param name="index"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        [Display(Name = "Добавяне на допълнителна колона за номенклатура")]
        [Authorize(Roles = UserRoles.Admin)]
        public IActionResult AddAdditionalColumn(int index, string prefix)
        {
            var model = new AdditionalColumnVM
            {
                Index = index,
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = string.IsNullOrEmpty(prefix) ? $"AdditionalColumns[{index}]" : $"{prefix}.AdditionalColumns[{index}]";
            return PartialView("_AdditionalColumn", model);
        }
        #endregion edit admin
    }
}
