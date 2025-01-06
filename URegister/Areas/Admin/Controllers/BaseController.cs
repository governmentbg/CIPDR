using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using URegister.Infrastructure.Constants;

namespace URegister.Areas.Admin.Controllers
{
    //[Authorize]
    public class BaseController : Controller
    {
        //private IUserContext _userContext;

        //protected IUserContext userContext
        //{
        //    get
        //    {
        //        if (_userContext == null)
        //        {
        //            _userContext = (IUserContext)HttpContext
        //                 .RequestServices
        //                 .GetService(typeof(IUserContext));
        //        }

        //        return _userContext;
        //    }
        //}

        //private IValidateService _validateService;

        ///// <summary>
        ///// Сервиз за валидация на данни
        ///// </summary>
        //public IValidateService ValidateService
        //{
        //    get
        //    {
        //        if (_validateService == null)
        //        {
        //            _validateService = (IValidateService)HttpContext.RequestServices.GetService(typeof(IValidateService));
        //        }

        //        return _validateService;
        //    }
        //    set
        //    {
        //        _validateService = value;
        //    }
        //}

        /// <summary>
        /// Изпраща съобщение за успех към front end-а
        /// </summary>
        /// <param name="message"></param>
        protected void SetSuccessMessage(string message = MessageConstant.Values.SaveOK)
        {
            TempData[MessageConstant.SuccessMessage] = message;
        }

        /// <summary>
        /// Изпраща съобщение за грешка към front end-а
        /// </summary>
        /// <param name="message"></param>
        protected void SetErrorMessage(string message = MessageConstant.Values.SaveFailed)
        {
            TempData[MessageConstant.ErrorMessage] = message;
        }

        /// <summary>
        /// Изпраща съобщение за предупреждение към front end-а
        /// </summary>
        /// <param name="message"></param>
        protected void SetWarningMessage(string message = MessageConstant.Values.SaveFailed)
        {
            TempData[MessageConstant.WarningMessage] = message;
        }

        /// <summary>
        /// Изпраща нескриващо се съобщение за грешка към front end-а
        /// </summary>
        /// <param name="message"></param>
        protected void SetPersistentErrorMessage(string message = MessageConstant.Values.SaveFailed)
        {
            TempData[MessageConstant.PersistentErrorMessage] = message;
        }
    }
}