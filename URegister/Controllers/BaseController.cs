using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using URegister.Infrastructure.Constants;

namespace URegister.Controllers
{
    [Authorize]
    public class BaseController : Controller
    {
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
    }
}
