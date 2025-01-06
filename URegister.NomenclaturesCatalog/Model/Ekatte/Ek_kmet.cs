namespace URegister.NomenclaturesCatalog.Model.Ekatte
{
    public class Ek_kmet
    {
        // <summary>
        /// Кметство
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("kmetstvo")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(8)]
        public string Kmetstvo { get; set; } = null!;

        /// <summary>
        /// Наименование на български
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(30)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Код
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("ekatte")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(5)]
        public string Ekatte { get; set; } = null!;

        /// <summary>
        /// Документ
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("document")]
        public int Document { get; set; }

        /// <summary>
        /// Категория
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("category")]
        public int? Category { get; set; }

        /// <summary>
        /// Наименование на английски
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("name_en")]
        [System.ComponentModel.DataAnnotations.StringLength(30)]
        public string Name_en { get; set; } = null!;

        /// <summary>
        /// NUTS1
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("nuts1")]
        [System.ComponentModel.DataAnnotations.StringLength(3)]
        public string Nuts1 { get; set; } = null!;

        /// <summary>
        /// NUTS2
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("nuts2")]
        [System.ComponentModel.DataAnnotations.StringLength(4)]
        public string Nuts2 { get; set; } = null!;

        /// <summary>
        /// NUTS3
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("nuts3")]
        [System.ComponentModel.DataAnnotations.StringLength(5)]
        public string Nuts3 { get; set; } = null!;
    }
}
