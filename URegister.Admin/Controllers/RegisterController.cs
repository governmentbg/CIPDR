using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using URegister.Core.Models.Register;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Infrastructure.Extensions;
using URegister.NomenclaturesCatalog;
using URegister.RegistersCatalog;
using static URegister.NomenclaturesCatalog.NomenclatureGrpc;
using URegister.Infrastructure.Constants;

namespace URegister.Admin.Controllers
{
    /// <summary>
    /// Регистри
    /// </summary>
    /// <param name="nomenclatureClient"></param>
    /// <param name="registerClient"></param>
    public class RegisterController(
        INomenclatureClientService nomenclatureClient,
        IRegisterClientService registerClient
        ) : BaseController
    {
        /// <summary>
        /// Списък регистри
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var filter = new RegisterFilterVM();
            return View(filter);
        }

        /// <summary>
        /// Списък на регистри
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetRegisterList(IDataTablesRequest request, RegisterFilterVM filter)
        {
            return await registerClient.GetRegisterFullList(request, filter);
        }

        /// <summary>
        /// Списък регистри
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexAdministration(int registerId)
        {
            var filter = new AdministrationFilterVM
            {
                RegisterId = registerId
            };
            return View(filter);
        }

        /// <summary>
        /// Списък на администрации към регистър
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetAdministrationList(IDataTablesRequest request, AdministrationFilterVM filter)
        {
            return await registerClient.GetAdministrationList(request, filter);
        }


        /// <summary>
        /// Списък от оторозирани лица
        /// </summary>
        /// <returns></returns>
        public IActionResult IndexPerson(Guid administrationId, int registerId)
        {
            var filter = new PersonFilterVM
            {
                AdministrationId = administrationId,
                RegisterId = registerId
            };
            return View(filter);
        }

        /// <summary>
        /// Списък на оторозирани лица към администрация
        /// </summary>
        /// <param name="request"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> GetPersonList(IDataTablesRequest request, PersonFilterVM filter)
        {
            return await registerClient.GetPersonList(request, filter);
        }

        /// <summary>
        /// Добавяне на регистър
        /// </summary>
        /// <param name="nomenclatureType"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Add()
        {
            await nomenclatureClient.SetViewBagRegister(ViewData);
            var model = await registerClient.CreateRegister();
            return View(nameof(Add), model);
        }

        /// <summary>
        /// Запис на добавен регистър 
        /// </summary>
        /// <param name="model">Модел на регистър</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                (var result, var errMsg) = await registerClient.AddRegister(model);
                if (result)
                {
                    SetSuccessMessage("Успешно добавен регистър");
                    return RedirectToAction("Index");
                }
                else
                {
                    SetErrorMessage($"Проблем при запис!{Environment.NewLine}{errMsg}");
                }
            }
            await nomenclatureClient.SetViewBagRegister(ViewData);
            return View(nameof(Add) , model);
        }

        /// <summary>
        /// Форма за добавяне на администрация към регистър
        /// </summary>
        /// <param name="registerId">Идентификатор на регистъра</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> AddAdministration(int registerId)
        {
            await nomenclatureClient.SetViewBagRegister(ViewData);
            var model = await registerClient.GetRegisterForAddAdministration(registerId);
            return View(nameof(Add), model);
        }

        /// <summary>
        /// Добавяне на администрация към регистър
        /// </summary>
        /// <param name="model">Модел на администрацията</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAdministration(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                (var result, var errMsg) = await registerClient.AddRegister(model);
                if (result)
                {
                    SetSuccessMessage("Успешно добавена администрация");
                    return RedirectToAction("Index");
                }
                else
                {
                    SetErrorMessage($"Проблем при запис!{Environment.NewLine}{errMsg}");
                }
            }
            await nomenclatureClient.SetViewBagRegister(ViewData);
            return View(nameof(Add), model);
        }

        /// <summary>
        /// Partial за оторозиране лице
        /// </summary>
        /// <param name="index"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public IActionResult AddContactPerson(int index, string prefix)
        {
            var model = new PersonVM
            {
                Index = index,
                Type = PersonTypeValue.AuthorizedPerson,
            };
            ViewData.TemplateInfo.HtmlFieldPrefix = string.IsNullOrEmpty(prefix) ? $"ContactPersons[{index}]" : $"{prefix}.ContactPersons[{index}]";
            return PartialView("_Person", model);
        }
    }
}
