using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Globalization;

namespace URegister.ModelBinders
{
    /// <summary>
    /// Author: Stamo Petkov
    /// Created: 07.01.2017
    /// Description: Корекция на формата на датата
    /// </summary>
    public class DateOnlyModelBinder : IModelBinder
    {
        #region Fields

        private readonly string _customFormat;

        #endregion

        public DateOnlyModelBinder(string customFormat)
        {
            this._customFormat = customFormat;
        }

        Task IModelBinder.BindModelAsync(ModelBindingContext bindingContext)
        {
            ValueProviderResult value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (value != ValueProviderResult.None && !String.IsNullOrEmpty(value.FirstValue))
            {
                string valueAsString = value.FirstValue;

                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);
                DateOnly result = DateOnly.MinValue;
                bool success = false;

                try
                {
                    result = DateOnly.ParseExact(valueAsString, this._customFormat, CultureInfo.InvariantCulture);
                    success = true;
                }
                catch (FormatException)
                {
                    try
                    {
                        result = DateOnly.Parse(valueAsString, new CultureInfo("bg-bg").DateTimeFormat);
                        success = true;
                    }
                    catch (Exception e)
                    {
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, e, bindingContext.ModelMetadata);
                    }
                }
                catch (Exception e)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, e, bindingContext.ModelMetadata);
                }

                if (success)
                {
                    bindingContext.Result = ModelBindingResult.Success(result);
                    return Task.CompletedTask;
                }
            }

            return Task.CompletedTask;
        }
    }
}
