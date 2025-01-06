namespace URegister.NomenclaturesCatalog.Model.Ekatte
{
    public class Ek_street
    {
        /// <summary>
        /// Код на населено място
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("city_code")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(5)]
        public string City_code { get; set; } = null!;

        /// <summary>
        /// Код на улица
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("street_code")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(5)]
        public string Street_code { get; set; } = null!;

        /// <summary>
        /// Наименование на български
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("name")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(40)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Код ЕКАТТЕ
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("actual_city")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(5)]
        public string Actual_city { get; set; } = null!;

        /// <summary>
        /// Тип на улица
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("street_type")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public int? Street_type { get; set; } = null!;

        /// <summary>
        /// Валидно от
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("valid_from")]
        [System.Text.Json.Serialization.JsonConverter(typeof(DateFormatConverter))]
        public System.DateTimeOffset? Valid_from { get; set; } = null!;

        /// <summary>
        /// Валидно до
        /// </summary>
        [System.Text.Json.Serialization.JsonPropertyName("valid_to")]
        [System.Text.Json.Serialization.JsonConverter(typeof(DateFormatConverter))]
        public System.DateTimeOffset? Valid_to { get; set; } = null!;
    }
}
