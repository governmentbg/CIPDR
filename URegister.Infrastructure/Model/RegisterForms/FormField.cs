using System.Text.Json.Serialization;
using URegister.Infrastructure.Model.JsonConverters;

namespace URegister.Infrastructure.Model.RegisterForms
{
    /// <summary>
    /// Полета на форма
    /// </summary>
    public class FormField
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Identifier { get; set; }

        /// <summary>
        /// Тип на поле
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// Име на поле
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Етикет на поле
        /// </summary>
        public string Label { get; set; } = null!;

        /// <summary>
        /// Подсказка
        /// </summary>
        public string? Placeholder { get; set; }

        /// <summary>
        /// Стойност
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Брой колони
        /// </summary>
        [JsonConverter(typeof(IntConverter))]
        public int Columns { get; set; } = 1;

        /// <summary>
        /// Клас на поле
        /// </summary>
        public string? ClassName { get; set; }

        /// <summary>
        /// Шаблон за валидация
        /// </summary>
        public string? Pattern { get; set; }

        /// <summary>
        /// Съобщение за валидация
        /// </summary>
        public string? ValidationMessage { get; set; }

        /// <summary>
        /// Полето е задължително
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Полето е публично
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Полето е само за четене
        /// </summary>
        public bool IsReadonly { get; set; }

        /// <summary>
        /// Подсказка
        /// </summary>
        public string? Tooltip { get; set; }

        /// <summary>
        /// Тип на номенклатура
        /// </summary>
        public string? NomenclatureType { get; set; }

        /// <summary>
        /// Подполета
        /// </summary>
        public List<FormField>? Fields { get; set; } = new List<FormField>();

        /// <summary>
        /// Максимална стойност за числови полета
        /// </summary>
        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? MaxValue { get; set; }

        /// <summary>
        /// Минимална стойност за числови полета
        /// </summary>
        [JsonConverter(typeof(NullableDecimalConverter))]
        public decimal? MinValue { get; set; }

        /// <summary>
        /// Позволени ли са отминали дати за поле за дата
        /// </summary>
        public bool AllowPastDates { get; set; }

        /// <summary>
        /// Позволени ли са отминали дати за поле за дата
        /// </summary>
        public bool AllowFutureDates { get; set; }

        /// <summary>
        /// Брой цифри след десетичната запетая
        /// </summary>
        [JsonConverter(typeof(NullableIntConverter))]
        public int? NumberOfDigitsAfterDelimiter { get; set; } = 0;

        /// <summary>
        /// Скрито ли е полето
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// Грешка при валидация в контролера
        /// </summary>
        public string ValidationError { get; set; }

        /// <summary>
        /// Допустим размер на файл в MB
        /// </summary>
        [JsonConverter(typeof(NullableIntConverter))]
        public int? AllowedFileSizeInMB { get; set; }

        /// <summary>
        /// Допустими разширения на файлове
        /// </summary>
        public string[]? AllowedFileExtensions { get; set; }

        /// <summary>
        /// Потребителят може да добавя повторения на полето
        /// </summary>
        public bool CanBeRepeated { get; set; } = false;

        /// <summary>
        /// Полето последно ли е на реда
        /// </summary>
        public bool IsLastInRow { get; set; } = false;


        /// <summary>
        /// Физическото или юридическо лице собственик ли е на партидата
        /// </summary>
        public bool IsBatchOwner { get; set; } = false;

        /// <summary>
        /// Лицето заявител ли е
        /// </summary>
        public bool IsSubmitter { get; set; } = false;

        /// <summary>
        /// Повторения на полето, заявени от потребителя
        /// </summary>
        public List<FormField>? Repetitions { get; set; } = new List<FormField>();

        /// <summary>
        /// Форматиран статичен текст
        /// </summary>
        public string FormattedText { get; set; } = string.Empty;

        /// <summary>
        /// Дали полето ще бъде колона при визуализиране на данните в таблица
        /// </summary>
        public bool IsColumnInDataTable { get; set; } = false;

        /// <summary>
        /// Път за стойността в еформата. Ползва се при импорт на данни
        /// </summary>
        public string EFormImportPath { get; set; } = string.Empty;
    }
}
