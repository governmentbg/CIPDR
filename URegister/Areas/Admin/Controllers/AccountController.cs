using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Infrastructure.Constants;
using URegister.Models;
using URegister.Users;
using URegister.RegistersCatalog;
using NuGet.Packaging;
using URegister.Core.Models.CurrentRegister;
using System.ComponentModel.DataAnnotations;

namespace URegister.Areas.Admin.Controllers
{
    [Display(Name = "Акаунт")]
    public class AccountController(
        IAuthenticationSchemeProvider schemes,
        ILogger<AccountController> logger,
        AppUserManager.AppUserManagerClient appUserManager,
        RegistersCatalogGrpc.RegistersCatalogGrpcClient registersCatalogGrpcClient,
        IRegisterService registerService,
        IUserContext userContext) : BaseController
    {
        private const string LoginProviderKey = "LoginProvider";

        [HttpGet]
        [AllowAnonymous]
        [Display(Name = "Отказан достъп")]
        public IActionResult AccessDenied(string errorMessage)
        {
            TempData[MessageConstant.ErrorMessage] = string.IsNullOrEmpty(errorMessage) ? MessageConstant.Values.Unauthorized : errorMessage;
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        [Display(Name = "Опит за вход в системата с невалиден сертификат")]
        public IActionResult LoginCertError(string error)
        {
            //logger.LogError(error);
            return RedirectToAction("Error", "Home", new { area = "", error = "Моля изберете валиден сертификат." });
        }

        private async Task StartRegister()
        {
            try
            {
                var registerId = await registerService.GetCurrentRegisterId();
            }
            catch (Exception)
            {
                var code = Request.HttpContext.Request.Host.Value;
                logger.LogError(code);
                var pos = code.IndexOf('.');
                code = code.Substring(0, pos).ToUpper();
                logger.LogError(code);
                var register = await registerService.StartRegister(code);
            }

        }

        [HttpGet]
        [AllowAnonymous]
        [Display(Name = "Вход в системата")]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            await StartRegister();
            ViewBag.ReturnUrl = returnUrl;
            var model = new LoginViewModel();
            model.ReturnUrl = returnUrl;
            model.ExternalLogins = (await schemes.GetAllSchemesAsync())
                .Where(s => !string.IsNullOrEmpty(s.DisplayName))
                .ToList();

            return View("FomanticUILogin", model);
        }

        [HttpGet]
        [AllowAnonymous]
        [Display(Name = "Зареждане на интерфейс за вход с Fomantic UI")]
        public async Task<IActionResult> FomanticUILogin() 
        {
            await StartRegister();
            return View(); 
        }  

        [HttpGet]
        [AllowAnonymous]
        [Display(Name = "Изход от системата")]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return LocalRedirect("/");
        }


        [HttpPost]
        [AllowAnonymous]
        [Display(Name = "Иницииране на външно влизане чрез доставчик на идентичност")]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {

            returnUrl = returnUrl ?? Url.Action("Index", "Home", new { Area = string.Empty });

            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action("ExternalLoginCallback", new { returnUrl });
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            properties.Items[LoginProviderKey] = provider;
            var result = new ChallengeResult(provider, properties);

            return result;
        }

        [AllowAnonymous]
        [Display(Name = "Обработка на обратна връзка от външен доставчик на идентичност и авторизация на потребител")]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl = returnUrl ?? "/";

            if (remoteError != null)
            {
                logger.LogError($"Error from external provider: {remoteError}");

                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }

            var info = await GetExternalLoginInfoAsync();

            if (info == null)
            {
                logger.LogError("Error loading external login information.");

                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }

            RegisterVM? register = null;

            try
            {
                register = await registerService.GetCurrentRegister();
            }
            catch (Exception)
            {
                var code = Request.HttpContext.Request.Host.Value;
                logger.LogError(code);
                var pos = code.IndexOf('.');
                code = code.Substring(0, pos).ToUpper();
                logger.LogError(code);
                register = await registerService.StartRegister(code);
            }
            
            AppUser appUser = await appUserManager
                .AuthorizeUserAsync(new AuthorizeUserData()
                {
                    Pid = info.ProviderKey,
                    RegisterCode = register.Code
                });

            if (appUser.Status.Code != ResultCodes.Ok)
            {
                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }

            var additionalClaims = await GetClaimsAsync(appUser, register.Id, info);
            if (additionalClaims == null)
            {
                SetErrorMessage("Няма активна администрация за потребителя");
                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }
            var claimsIdentity = new ClaimsIdentity(
                    additionalClaims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false
            };
           
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return LocalRedirect(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Display(Name = "Смяна на администрация")]
        public async Task<IActionResult> ChangeAdministrationAsync([FromForm]string administrationId, [FromForm]string? returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Action("Index", "Home", new { Area = string.Empty });
            
            var identity = (ClaimsIdentity?)User.Identity;

            if (identity == null)
            {
                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }

            string administrationName = userContext.AvailableAdministrations
                .FirstOrDefault(a => a.Id == administrationId)?.Name ?? string.Empty;

            string currentAdministrationName = userContext.AvailableAdministrations
                .FirstOrDefault(a => a.Id == userContext.AdministrationId.ToString())?.Name ?? string.Empty;

            bool hasAdministration = (await appUserManager.HasAdministrationAsync(new HasAdministrationRequest 
            {
                AdministrationName = administrationName,
                AdministrationId = administrationId,
                UserId = User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? string.Empty
            })).Code == ResultCodes.Ok;

            if (hasAdministration)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                Claim? administrationClaim = identity.Claims.FirstOrDefault(c => c.Type == CustomClaimType.AdministrationId);

                if (administrationClaim != null)
                {
                    identity.RemoveClaim(administrationClaim);
                    administrationClaim = new Claim(CustomClaimType.AdministrationId, administrationId);
                    identity.AddClaim(administrationClaim);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(identity),
                        new AuthenticationProperties
                        {
                            IsPersistent = false
                        });
                }
                
                SetSuccessMessage($"Успешно се прехвърлихте от администрация '{currentAdministrationName}' в '{administrationName}'");
            }

            return LocalRedirect(returnUrl);
        }

        private async Task<IList<Claim>?> GetClaimsAsync(AppUser appUser, int registerId, ExternalLoginInfo? info)
        {
            IList<Claim> claims = new List<Claim>();
            IList<string> availableAdministrations = new List<string>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, appUser.Id));
            claims.Add(new Claim(ClaimTypes.Name, $"{appUser.FirstName} {appUser.LastName}"));
            claims.Add(new Claim(ClaimTypes.Email, appUser.Email));
            claims.Add(new Claim(CustomClaimType.FirstName, appUser.FirstName));
            claims.Add(new Claim(CustomClaimType.LastName, appUser.LastName));

            foreach (var claim in appUser.Claims)
            {
                if (claim.Type != CustomClaimType.AvailableAdministration)
                {
                    claims.Add(new Claim(claim.Type, claim.Value));
                }
                else
                {
                    availableAdministrations.Add(claim.Value);
                }
            }

            foreach (var role in appUser.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            
             AdministrationIds ids = new AdministrationIds();
            ids.Ids.AddRange(availableAdministrations);
            ids.RegisterId = registerId;
            var administrations = await registersCatalogGrpcClient.GetAdministrationsByIdsAsync(ids);
            if (administrations.Status.Code != ResultCodes.Ok || !administrations.Administrations.Any())
                return null;
            //if (administrations.Administrations.Count > 1)
            //{
            //    claims.AddRange(administrations.Administrations
            //          .Select(a => new Claim(CustomClaimType.AvailableAdministration, $"{a.Id}!{a.Name}")));
            //}
            claims.AddRange(administrations.Administrations
                  .Select(a => new Claim(CustomClaimType.AvailableAdministration, $"{a.Id}!{a.Name}")));
            if (!administrations.Administrations.Any(a => a.Id == appUser.AdministrationId.ToString()))
            {
                var administration = administrations.Administrations.First();
                appUser.AdministrationId = administration.Id;
            }
            claims.Add(new Claim(CustomClaimType.AdministrationId, appUser.AdministrationId));
            if (info != null)
            {
                var currentCertNoClaim = info.Principal.Claims.FirstOrDefault(c => c.Type == CustomClaimType.IdStampit.CertificateNumber);
                if (currentCertNoClaim != null)
                {
                    claims.Add(new Claim(CustomClaimType.IdStampit.CertificateNumber, currentCertNoClaim.Value));
                }
            }

            return claims;
        }

        private async Task<ExternalLoginInfo?> GetExternalLoginInfoAsync()
        {
            var auth = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
            var items = auth?.Properties?.Items;
            if (auth?.Principal == null || items == null || !items.TryGetValue(LoginProviderKey, out var provider))
            {
                return null;
            }

            var providerKey = auth.Principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? auth.Principal.FindFirstValue("sub");
            if (providerKey == null || provider == null)
            {
                return null;
            }

            var providerDisplayName = (await schemes.GetAllSchemesAsync())
                .FirstOrDefault(p => p.Name == provider)?.DisplayName ?? provider;

            return new ExternalLoginInfo(auth.Principal, provider, providerKey, providerDisplayName)
            {
                AuthenticationTokens = auth.Properties?.GetTokens(),
                AuthenticationProperties = auth.Properties
            };
        }
    }
}
