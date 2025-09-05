using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace URegister.ModelBinders
{
    /// <summary>
    /// Author: Stamo Petkov
    /// Created: 07.01.2017
    /// Description: Корекция на формата на датата
    /// </summary>
    public class DateOnlyModelBinderProvider : IModelBinderProvider
    {
        private readonly string _customFormat;


        public DateOnlyModelBinderProvider(string dateFormat)
        {
            _customFormat = dateFormat;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Metadata.ModelType == typeof(DateOnly) || context.Metadata.ModelType == typeof(DateOnly?))
            {
                return new DateOnlyModelBinder(_customFormat);
            }

            return null;
        }
    }
}
