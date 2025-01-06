using DataTables.AspNet.Core;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Dynamic;
using URegister.Core.Models.Nomenclature;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Extensions;
using URegister.NomenclaturesCatalog;
using URegister.RegistersCatalog;
using URegister.Core.Contracts;

namespace URegister.Admin.Controllers
{
    /// <summary>
    /// Управление на номенклатури
    /// </summary>
    /// <param name="nomenclatureGrpcClient"></param>
    public class NomenclatureController(
        NomenclatureGrpc.NomenclatureGrpcClient nomenclatureGrpcClient,
        RegistersCatalogGrpc.RegistersCatalogGrpcClient registerGrpcClient,
        INomenclatureClientService nomenclatureClientService,
        ILogger<NomenclatureController> logger) : BaseController
    {
        /// <summary>
        /// Списък номенклатурни типове
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexType()
        {
            var filter = new NomenclatureTypeFilterVM();
            return View(filter);
        }

        /// <summary>
        /// Списък номенклатурни типове за регистър
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> IndexTypeRegister(int registerId)
        {
            var registers = await registerGrpcClient.GetRegisterListAsync(new Empty());
            ViewBag.RegisterId_ddl = registers.Data.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Label,
            });
            var filter = new NomenclatureTypeRegisterFilterVM
            {
                RegisterId = registerId
            };
            return View(filter);
        }

        /// <summary>
        /// Добавяне на номенклатурен тип
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AddNomenclatureType()
        {
            var response = await nomenclatureGrpcClient.CreateNewNomenclatureTypeAsync(new Empty());
            var model = nomenclatureClientService.GrpcNomenclatureTypeToModel(response);
            return View(nameof(EditNomenclatureType), model);
        }
        /// <summary>
        /// Запис на номенклатурен тип
        /// </summary>
        /// <param name="model">Модел на номенклатурен тип</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNomenclatureType(NomenclatureTypeVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(EditNomenclatureType), model);
            }
            NomenclatureTypeRequest request = nomenclatureClientService.NomenclatureTypeToGrpcModel(model);
            var result = await nomenclatureGrpcClient.AddNomenclatureTypeAsync(request);
            if (result?.Code == ResultCodes.Ok)
            {
                SetSuccessMessage("Успешно добавен номенклатурен тип");
                return RedirectToAction(nameof(IndexType));
            }
            else
            {
                SetErrorMessage(result?.Message ?? "Проблем при запис!");
            }
            return View(nameof(EditNomenclatureType), model);
        }

        /// <summary>
        /// Редакция на номенклатурен тип
        /// </summary>
        /// <param name="id">идентификатор</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> EditNomenclatureType(int id)
        {
            var response = await nomenclatureGrpcClient.GetNomenclatureTypeAsync(new GetNomenclatureTypeRequest { Id = id });
            var model = nomenclatureClientService.GrpcNomenclatureTypeToModel(response);
            return View(model);
        }

        /// <summary>
        /// Запис редакция на номенклатурен тип
        /// </summary>
        /// <param name="model">данни за тип</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNomenclatureType(NomenclatureTypeVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            NomenclatureTypeRequest request = nomenclatureClientService.NomenclatureTypeToGrpcModel(model);
            var result = await nomenclatureGrpcClient.EditNomenclatureTypeAsync(request);

            if (result?.Code == ResultCodes.Ok)
            {
                SetSuccessMessage("Успешно редактиран номенклатурен тип");
                return RedirectToAction("IndexType");
            }
            else
            {
                SetErrorMessage(result?.Message ?? "Проблем при запис!");
            }
            return View(model);
        }
        

        /// <summary>
        /// Добавяне на номенклатурна стойност 
        /// </summary>
        /// <param name="nomenclatureType">Име на новия тип стойност</param>
        /// <returns></returns>
        [HttpGet]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CodeableConceptVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Edit), model);
            }
            CodeableConceptRequest request = nomenclatureClientService.CodeableConceptToGrpcModel(model);
            var result = await nomenclatureGrpcClient.AddCodeableConceptAsync(request);
            if (result?.Code == ResultCodes.Ok)
            {
                SetSuccessMessage("Успешно добавен номенклатуррна стойност");
                return RedirectToAction("Index", new { nomenclatureType = model.Type });
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CodeableConceptVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            CodeableConceptRequest request = nomenclatureClientService.CodeableConceptToGrpcModel(model);
            var result = await nomenclatureGrpcClient.EditCodeableConceptAsync(request);
            if (result?.Code == ResultCodes.Ok)
            {
                SetSuccessMessage("Успешно редактирана номенклатурна стойност");
                return RedirectToAction("Index", new { nomenclatureType = model.Type });
            }
            else
            {
                SetErrorMessage(result?.Message ?? "Проблем при запис!");
            }
            return View(model);
        }

        /// <summary>
        /// Списък на номенклатурнани стойности
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetCodeableConceptList(IDataTablesRequest request, NomenclatureTypeFilterVM filter)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await nomenclatureGrpcClient.GetCodeableConceptListAsync(
                new CodeableConceptListRequest
                {
                    DataTableRequest = protoRequest,
                    Type = filter.Type
                });
            return request.GetResponseServerPaging(result.Data, result.CountAll);
        }

        /// <summary>
        /// Списък на номенклатурнани стойности за регистър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetCodeableConceptRegisterList(IDataTablesRequest request, NomenclatureTypeRegisterFilterVM filter)
        {
            return await nomenclatureClientService.GetCodeableConceptRegisterList(request, filter);
        }

        /// <summary>
        /// Списък на номенклатурнани типове
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetNomenclatureTypeList(IDataTablesRequest request, NomenclatureTypeFilterVM filter)
        {
            var protoRequest = request!.GetDataTablesRequestProto();
            var result = await nomenclatureGrpcClient.GetNomenclatureTypeListAsync(new NomenclatureTypeListRequest
            {
                DataTableRequest = protoRequest,
                Type = filter.Type ?? string.Empty,
                Name = filter.Name ?? string.Empty,
            });
            return request.GetResponseServerPaging(result.Data, result.CountAll);
        }

        /// <summary>
        /// Списък на номенклатурнани типове за регистър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetNomenclatureTypeRegisterList(IDataTablesRequest request, NomenclatureTypeRegisterFilterVM filter)
        {
            return await nomenclatureClientService.GetNomenclatureTypeRegisterList(request, filter);
        }


        /// <summary>
        /// Списък на номенклатурнани стойности
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        public async Task<IActionResult> Index(string nomenclatureType)
        {
            var response = await nomenclatureGrpcClient.GetNomenclatureTypeOnTypeAsync(new GetNomenclatureTypeOnTypeRequest { Type = nomenclatureType });
            var model = new NomenclatureTypeFilterVM
            {
                Type = response.Type,
                Name = response.Name
            };
            return View(model);
        }

        /// <summary>
        /// Списък на номенклатурнани стойности за регистър
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        public async Task<IActionResult> IndexRegister(string nomenclatureType, int registerId)
        {
            var response = await nomenclatureGrpcClient.GetNomenclatureTypeRegisterOnTypeAsync(
                new GetNomenclatureTypeRegisterOnTypeRequest { 
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
        /// Partial за допълнителна колона
        /// </summary>
        /// <param name="index"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IActionResult AddAdditionalColumn(int index, string prefix)
        {
            var model = new AdditionalColumnVM
            {
                Index = index,
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = string.IsNullOrEmpty(prefix) ? $"AdditionalColumns[{index}]" : $"{prefix}.AdditionalColumns[{index}]";
            return PartialView("_AdditionalColumn", model);
        }

        public async Task<IActionResult> TestImport()
        {
            // var r = nomenclatureGrpcClient.ImportNrnmNsi(new Common.EmptyRequest(), deadline: DateTime.UtcNow.AddMinutes(5));
            var r = await nomenclatureGrpcClient.ImportNrnmNsiStreetAsync(new Empty(), deadline: DateTime.UtcNow.AddMinutes(15));
            return RedirectToAction("IndexType");
        }

        public async Task<IActionResult> TestPublic()
        {
            var request = new NomenclaturePublicRequest
            {
                RegisterId = 0,
            };
            var r = await nomenclatureGrpcClient.GetNomenclaturePublicAsync(request);

            var rType = await nomenclatureGrpcClient.GetNomenclatureTypeListPublicAsync(request);
            return RedirectToAction("IndexType");
        }

        public async Task<IActionResult> TestPublicHolder()
        {
            var request = new NomenclatureHolderRequest
            {
                RegisterId = 0,
                NomenclatureType = NomenclatureTypes.EkStreet,
                Holder = "00014",
                FilterValue = "Отец"
            };
            var r = await nomenclatureGrpcClient.GetNomenclatureOnHolderPublicAsync(request);

            return RedirectToAction("IndexType");
        }

        public async Task<IActionResult> TestCheckNomenclature()
        {
            var request = new CheckNomenclatureRequest
            {
                RegisterId = 0
            };
            request.Data.Add(new CheckNomenclatureItem
            {
              Type = NomenclatureTypes.Ekatte,
              Code = "00012",
            });
            var r = await nomenclatureGrpcClient.CheckNomenclatureAsync(request);

            return RedirectToAction("IndexType");
        }
        public async Task<IActionResult> TestEkatte()
        {
            var request = new EkattePublicRequest
            {
                RegisterId = 0
            };
            var r = nomenclatureGrpcClient.GetEkattePublic(request);

            return RedirectToAction("IndexType");
        }

        /// <summary>
        /// Запис на влогове за номенклатурен тип
        /// </summary>
        /// <param name="model">Модел на типа</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateNomenclatureTypeRegister([FromBody] NomenclatureTypeRegisterUpdateVM model)
        {
            var result = await nomenclatureClientService.UpdateNomenclatureTypeRegister(model);
            return Json(result);
        }

        /// <summary>
        /// Запис допустими номенклатурни стойности
        /// </summary>
        /// <param name="model">Модел на допустимите стойности</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateCodeableConceptRegister([FromBody] CodeableConceptRegisterUpdateVM model)
        {
            var result = await nomenclatureClientService.UpdateCodeableConceptRegister(model);
            return Json(result);
        }

        /// <summary>
        /// Връща номенклатурните стойности за тип по низ от символи
        /// </summary>
        /// <param name="query">Низ от символи за търсене</param>
        /// <param name="nomenclatureType">Номенклатурен тип</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<JsonResult> GetAutocompleteValues(string query, string nomenclatureType)
        {
            NomenclaturePublicRequest getNomenclaturesRequest = new NomenclaturePublicRequest
            {
                RegisterId = 0,
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
        public async Task<JsonResult> GetEkatteValues(string query)
        {
            EkattePublicRequest getEkatteRequest = new EkattePublicRequest
            {
                RegisterId = 0,
                FilterValue = query
            };
            try
            {
                URegister.NomenclaturesCatalog.EkattePublicResponse ekattePublicResponse =
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
        public async Task<IActionResult> GetNomenclatureValues(string nomenclatureCode, string holderCode)
        {
            try
            {
                var request = new NomenclatureHolderRequest
                {
                    RegisterId = 0,
                    NomenclatureType = nomenclatureCode,
                    Holder = holderCode
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
        public async Task<IActionResult> GetNomenclatureValuesForAutocomplete(string nomenclatureCode, string holderCode, string query = "")
        {
            try
            {
                var request = new NomenclatureHolderRequest
                {
                    RegisterId = 0,
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
    }
}
