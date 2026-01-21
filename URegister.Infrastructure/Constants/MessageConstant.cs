using System.ComponentModel.DataAnnotations;

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
        public const string NotCyrillic = "Стойността не е на кирилица или започва/завършва с интервал";
        
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

        /// <summary>
        /// Невалиден шаблон за идентификация
        /// </summary>
        public const string InvalidIDTemplate = "Невалиден шаблон за идентификация";

        /// <summary>
        /// Въведете място на раждане
        /// </summary>
        public const string EnterPlaceOfBirth = "Въведете място на раждане";

        /// <summary>
        /// Невалиден формат на стойността
        /// </summary>
        public const string InvalidValueFormat = "Невалиден формат на стойността";

        /// <summary>
        /// Невалиден тип идентификатор
        /// </summary>
        public const string InvalidIdentifierType = "Невалиден тип идентификатор";

        /// <summary>
        /// Неуспешна валидация, проблем с връзката, опитайте пак
        /// </summary>
        public const string ValidationFailConnectionIssue = "Неуспешна валидация, проблем с връзката, опитайте пак";

        /// <summary>
        /// Непознат тип идентификатор
        /// </summary>
        public const string UnknownIdentifierType = "Непознат тип идентификатор";

        /// <summary>
        /// Невалиден идентификатор
        /// </summary>
        public const string InvalidIdentifier = "Невалиден идентификатор";

        /// <summary>
        /// Невалиден шаблон за поле
        /// </summary>
        public const string InvalidFieldConfig = "Невалиден шаблон за поле";

        /// <summary>
        /// Непозната улица за населеното място
        /// </summary>
        public const string UnknownStreetForSettlement = "Непозната улица за населеното място";

        /// <summary>
        /// Непознат район за населеното място
        /// </summary>
        public const string UnknownDistrictForSettlement = "Непознат район за населеното място";

        /// <summary>
        /// Непознат квартал за населеното място
        /// </summary>
        public const string UnknownNeighborhoodForSettlement = "Непознат квартал за населеното място";

        /// <summary>
        /// Изборът е задължителен
        /// </summary>
        public const string SelectionIsRequired = "Изборът е задължителен";

        /// <summary>
        /// Изберете отминала дата
        /// </summary>
        public const string SelectPastDate = "Изберете отминала дата";

        /// <summary>
        /// Изберете не отминала дата
        /// </summary>
        public const string SelectFutureDate = "Изберете неотминала дата";

        /// <summary>
        /// Стойността е по-малка от минимума
        /// </summary>
        public const string ValueBelowMinimum = "Стойността е по-малка от минимума";

        /// <summary>
        /// Стойността е по-голяма от максимума
        /// </summary>
        public const string ValueExceedsMaximum = "Стойността е по-голяма от максимума";

        /// <summary>
        /// Невалидна стойност за номенклатура
        /// </summary>
        public const string InvalidNomenclatureValue = "Невалидна стойност за номенклатура";

        /// <summary>
        /// Невалиден тип на EKATTE
        /// </summary>
        public const string InvalidEKATTEType = "Невалиден тип на EKATTE";

        /// <summary>
        /// Успешна валидация на данните
        /// </summary>
        public const string SuccessfulValidation = "Успешна валидация на данните";

        /// <summary>
        /// Подсказка за формат на телефон
        /// </summary>
        public const string PhonePlaceholder = "+359xxxxxxxxx";

        /// <summary>
        /// Имейл
        /// </summary>
        public const string EmailLabel = "Имейл";

        /// <summary>
        /// Телефон
        /// </summary>
        public const string PhoneLabel = "Телефон";

        /// <summary>
        /// Проблем с връзката, презаредете страницата
        /// </summary>
        public const string NetworkProblemReload = "Проблем с връзката, презаредете страницата";

        /// <summary>
        /// Номенклатурата не е достъпна
        /// </summary>
        public const string NomenclatureNotAvailable = "Номенклатурата {0} не е достъпна";

        /// <summary>
        /// Необходим е един идентификатор за партида
        /// </summary>
        public const string MPRIdRequired = "Необходим е един валиден идентификатор за партида";

        public static class Values
        {
            /// <summary>
            /// Записът премина успешно.
            /// </summary>
            public const string SaveOK = "Записът премина успешно.";

            /// <summary>
            /// Проблем по време на запис.
            /// </summary>
            public const string SaveFailed = "Проблем по време на запис.";

            /// <summary>
            /// Обновяването премина успешно.
            /// </summary>
            public const string UpdateOK = "Обновяването премина успешно.";

            /// <summary>
            /// Проблем при обновяването на данните.
            /// </summary>
            public const string UpdateFailed = "Проблем при обновяването на данните.";

            /// <summary>
            /// Файлът не е намерен!
            /// </summary>
            public const string FileNotFound = "Файлът не е намерен!";

            /// <summary>
            /// Проблем при получаване на данните!
            /// </summary>
            public const string BindError = "Проблем при получаване на данните!";

            /// <summary>
            /// Нямате права върху този ресурс!
            /// </summary>
            public const string Unauthorized = "Нямате права върху този ресурс!";

            /// <summary>
            /// Грешка при обработка на данните.
            /// </summary>
            public const string ErrorProcessingData = "Грешка при обработка на данните.";

            /// <summary>
            /// Записът не е намерен!
            /// </summary>
            public const string EntityNotFound = "Записът не е намерен!";

            /// <summary>
            /// Проблем с входните данни!
            /// </summary>
            public const string InputDataError = "Проблем с входните данни!";

            /// <summary>
            /// Неуспешно качване на файл.
            /// </summary>
            public const string FileUploadFailed = "Неуспешно качване на файл";

            /// <summary>
            /// Изтриването премина успешно.
            /// </summary>
            public const string DeleteOK = "Изтриването премина успешно.";

            /// <summary>
            /// Проблем по време на изтриване.
            /// </summary>
            public const string DeleteFailed = "Проблем по време на изтриване.";

            /// <summary>
            /// Грешка по време на валидация на подпис.
            /// </summary>
            public const string ErrorValidatingSignature = "Грешка по време на валидация на подпис";

            /// <summary>
            /// Невалиден подпис.
            /// </summary>
            public const string InvalidSignature = "Невалиден подпис";

            /// <summary>
            /// Неуспешно подписване.
            /// </summary>
            public const string UnsuccessfulSigning = "Неуспешно подписване";

            /// <summary>
            /// Успешно подписване.
            /// </summary>
            public const string SuccessfulSigning = "Успешно подписване";

            /// <summary>
            /// Файлът е празен.
            /// </summary>
            public const string FileIsEmpty = "Файлът е празен";

            /// <summary>
            /// Разширението на файла не отговаря на съдържанието му.
            /// </summary>
            public const string FileTypeMismatch = "Разширението на файла не отговаря на съдържанието му";

            /// <summary>
            /// Форматът на файла е неприемлив. Изберете {0}.
            /// </summary>
            public const string FileTypeRejected = "Форматът на файла е неприемлив. Изберете {0}";

            /// <summary>
            /// Файлът е по-голям от {0} MB! Файлът не е записан.
            /// </summary>
            public const string FileExceedsLimit = "Файлът е по-голям от {0} MB! Файлът не е записан";
        }
    }
}