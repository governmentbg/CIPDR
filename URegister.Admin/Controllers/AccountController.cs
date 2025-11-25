using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using URegister.Admin.Models;
using URegister.Common;
using URegister.Infrastructure.Constants;
using URegister.Users;

namespace URegister.Admin.Controllers
{
    [Authorize]
    [Display(Name = "Акаунт")]
    public class AccountController(
        IAuthenticationSchemeProvider schemes,
        ILogger<AccountController> logger,
        AppUserManager.AppUserManagerClient appUserManager) : BaseController
    {
        private const string LoginProviderKey = "LoginProvider";

        [HttpGet]
        [Display(Name = "Отказан достъп")]
        [AllowAnonymous]
        public IActionResult AccessDenied(string errorMessage)
        {
            TempData[MessageConstant.ErrorMessage] = string.IsNullOrEmpty(errorMessage) ? MessageConstant.Values.Unauthorized : errorMessage;
            return View();
        }

        [HttpGet]
        [Display(Name = "Опит за вход в системата с невалиден сертификат")]
        [AllowAnonymous]
        public IActionResult LoginCertError(string error)
        {
            //logger.LogError(error);
            return RedirectToAction("Error", "Home", new { area = "", error = "Моля изберете валиден сертификат." });
        }

        [HttpGet]
        [Display(Name = "Вход в системата")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new LoginViewModel();
            model.ReturnUrl = returnUrl;
            model.ExternalLogins = (await schemes.GetAllSchemesAsync())
                .Where(s => !string.IsNullOrEmpty(s.DisplayName))
                .ToList();

            return View("FomanticUILogin", model);
        }

        [HttpGet]
        [Display(Name = "Зареждане на интерфейс за вход")]
        [AllowAnonymous]
        public IActionResult FomanticUILogin()
        {
            return View();
        }

        [HttpGet]
        [Display(Name = "Изход от системата")]
        [AllowAnonymous]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return LocalRedirect("/");
        }


        [HttpPost]
        [Display(Name = "Иницииране на външно влизане чрез доставчик на идентичност")]
        [AllowAnonymous]
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

            if (string.IsNullOrWhiteSpace(info.ProviderKey))
            {
                logger.LogError($"Липсва ProviderKey в {nameof(ExternalLoginCallback)}");

                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }

            AppUser appUser = await appUserManager
                .AuthorizeUserAsync(new AuthorizeUserData()
                {
                    Pid = info.ProviderKey,
                    RegisterCode = "R00000"
                });

            if (appUser.Status.Code != ResultCodes.Ok)
            {
                return RedirectToAction("Login", new { ReturnUrl = returnUrl });
            }

            var additionalClaims = GetClaims(appUser);

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

        private IList<Claim> GetClaims(AppUser appUser)
        {
            IList<Claim> claims = new List<Claim>();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, appUser.Id));
            claims.Add(new Claim(ClaimTypes.Name, $"{appUser.FirstName} {appUser.LastName}"));
            claims.Add(new Claim(ClaimTypes.Email, appUser.Email));
            claims.Add(new Claim(CustomClaimType.FirstName, appUser.FirstName));
            claims.Add(new Claim(CustomClaimType.LastName, appUser.LastName));
            claims.Add(new Claim(CustomClaimType.AdministrationId, appUser.AdministrationId));

            foreach (var claim in appUser.Claims)
            {
                claims.Add(new Claim(claim.Type, claim.Value));
            }

            foreach (var role in appUser.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
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
