using Amazon.S3.Model;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Contracts;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.Report;
using URegister.Users;
using static URegister.Users.AppUserManager;

namespace URegister.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = $"{UserRoles.Admin},{UserRoles.Manager}, {UserRoles.Editor}")]
    [Display(Name = "Статистика")]
    public class ReportController : BaseController
    {
        ILogger<ReportController> _logger;
        private readonly IReportService _reportService;
        private readonly IRegisterService _registerService;
        private readonly AppUserManagerClient _appUserManagerClient;
        public ReportController(ILogger<ReportController> logger, IReportService reportService, IRegisterService registerService, AppUserManagerClient appUserManagerClient)
        {
            _logger = logger;
            _reportService = reportService;
            _registerService = registerService;
            _appUserManagerClient = appUserManagerClient;
        }

        [Display(Name = "Зареждане на страница Статистика")]
        public IActionResult Index()
        {
            return View();
        }

        [Display(Name = "Генериране на статистическа справка")]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> GenerateStatistics(StatisticalReportViewModel model)
        {
            var register = await _registerService.GetCurrentRegister();

            if (register == null)
            {
                _logger.LogError("Не е намерен регистър.");
                throw new InvalidOperationException("Не е намерен регистър.");
            }

            model.RegisterTypeEntry = register.TypeEntry;

            // Validate date range
            if (model.DateFrom.HasValue && model.DateTo.HasValue && model.DateFrom > model.DateTo)
            {
                ModelState.AddModelError(nameof(model.DateFrom), "'От дата' не може да бъде по-късна от 'До дата'.");
                return View("Index", model);
            }

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // Use provided dates or null for unrestricted queries
            DateTime? dateFrom = model.DateFrom;
            DateTime? dateTo = model.DateTo;
           
            var usersCountRequest = new UserCountRequest
            {
                RegisterCode = register.Code,
                DateFrom = dateFrom.HasValue ? Timestamp.FromDateTime(dateFrom.Value.ToUniversalTime()) : null,
                DateTo = dateTo.HasValue ? Timestamp.FromDateTime(dateTo.Value.ToUniversalTime()) : null
            };
            
            var usersCount = await _appUserManagerClient.GetCurrentRegisterUsersCountAsync(usersCountRequest);

            var report = await _reportService.GenerateStatisticalReport(dateFrom, dateTo, register.TypeEntry);
            report.CreatedUsers = usersCount.CreatedUsers;
            report.ActiveUsers = usersCount.ActiveUsers;
            report.InactiveUsers = usersCount.InactiveUsers;

            return View("Index", report);
        }
    }
}
