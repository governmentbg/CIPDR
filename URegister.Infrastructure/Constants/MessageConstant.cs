namespace URegister.Infrastructure.Constants
{
    public static class MessageConstant
    {
        /// <summary>
        /// Грешка
        /// </summary>
        public const string ErrorMessage = "ErrorMessage";

        /// <summary>
        /// Внимание
        /// </summary>
        public const string WarningMessage = "WarningMessage";

        /// <summary>
        /// Успех
        /// </summary>
        public const string SuccessMessage = "SuccessMessage";

        /// <summary>
        /// Не е избран (за Display Template-ите)
        /// </summary>
        public const string NotSelected = "Не е избран";

        /// <summary>
        /// Да (за Display Template-ите)
        /// </summary>
        public const string Yes = "Да";

        /// <summary>
        /// Не (за Display Template-ите)
        /// </summary>
        public const string No = "Не";

        /// <summary>
        /// Полето е задължително без параметри
        /// </summary>
        public const string FieldIsRequiredNoParam = "Полето е задължително";

        /// <summary>
        /// Полето е задължително (за Data Annotations)
        /// </summary>
        public const string FieldIsRequired = "Полето '{0}' е задължително"; 
        
        /// <summary>
        /// Максимална дължина на низ в поле (за Data Annotations)
        /// </summary>
        public const string StringMaxLengthValidation = "Полето '{0}' не трябва да надвишава {1} символа";

        /// <summary>
        /// swal Грешка
        /// </summary>
        public const string SwalErrorMessage = "SwalErrorMessage";

        /// <summary>
        /// swal Внимание
        /// </summary>
        public const string SwalWarningMessage = "SwalWarningMessage";

        /// <summary>
        /// swal Успех
        /// </summary>
        public const string SwalSuccessMessage = "SwalSuccessMessage";


        /// <summary>
        /// Максимална дължина на низ в поле без параметри
        /// </summary>
        public const string StringMaxLengthValidationNoParam = "Стойността надвишава допустимата дължина";

        /// <summary>
        /// Фиксирана дължина на низ в поле (за Data Annotations)
        /// </summary>
        public const string StringExactLengthValidation = "Полето '{0}' трябва да е {1} символа";

        /// <summary>
        /// Грешен формат на стойност (за Data Annotations)
        /// </summary>
        public const string RegexFail = "Грешен формат";

        /// <summary>
        /// Стойността не е на кирилица (за Data Annotations)
        /// </summary>
        public const string NotCyrillic = "Стойността не е на кирилица";
        
        /// <summary>
        /// Стойността не е на латиница (за Data Annotations)
        /// </summary>
        public const string NotLatin = "Приемат се латински букви и цифри. Започва се с буква";

        /// <summary>
        /// Въведете стойност (за Data Annotations)
        /// </summary>
        public const string EnterValue = "Въведете стойност";

        /// <summary>
        /// Невалиден e-mail формат
        /// </summary>
        public const string InvalidEmail = "Невалиден e-mail формат";

        /// <summary>
        /// Нескриваща се грешка
        /// </summary>
        public const string PersistentErrorMessage = "PersistentErrorMessage";

        /// <summary>
        /// Неприемлива стойност
        /// </summary>
        public const string InvalidValue = "Неприемлива стойност";

        public static class Values
        {
            public const string SaveOK = "Записът премина успешно.";
            public const string SaveFailed = "Проблем по време на запис.";
            public const string UpdateOK = "Обновяването премина успешно.";
            public const string UpdateFailed = "Проблем при обновяването на данните.";
            public const string FileNotFound = "Файлът не е намерен!";
            public const string BindError = "Проблем при получаване на данните!";
            public const string Unauthorized = "Нямате права върху този ресурс!";
            public const string ErrorProcessingData = "Грешка при обработка на данните.";
            public const string EntityNotFound = "Записът не е намерен!";
            public const string InputDataError = "Проблем с входните данни!";
            public const string FileUploadFailed = "Неуспешно качване на файл";
            public const string DeleteOK = "Изтриването премина успешно.";
            public const string DeleteFailed = "Проблем по време на изтриване.";
            public const string ErrorValidatingSignature = "Грешка по време на валидация на подпис";
            public const string InvalidSignature = "Невалиден подпис";
            public const string UnsuccessfulSigning = "Неуспешно подписване";
            public const string SuccessfulSigning = "Успешно подписване";
        }
    }
}
