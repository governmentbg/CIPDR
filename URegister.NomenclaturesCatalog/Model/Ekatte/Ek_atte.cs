namespace URegister.NomenclaturesCatalog.Model.Ekatte
{
    public partial class Ek_atte
    {
        /// <summary>
        /// Код ЕКАТТЕ
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("ekatte")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(5)]
        public string Ekatte { get; set; } = null!;

        /// <summary>
        /// Тип населено място
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("t_v_m")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(5)]
        public string T_v_m { get; set; } = null!;

        /// <summary>
        /// Наименование
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(30)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Област
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("oblast")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(3)]
        public string Oblast { get; set; } = null!;

        /// <summary>
        /// Община
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("obshtina")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(5)]
        public string Obshtina { get; set; } = null!;

        /// <summary>
        /// Кметство
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("kmetstvo")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(8)]
        public string Kmetstvo { get; set; } = null!;

        /// <summary>
        /// Вид
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("kind")]
        public int Kind { get; set; }

        /// <summary>
        /// Категория
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("category")]
        public int Category { get; set; }

        /// <summary>
        /// Надморска височина
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("altitude")]
        public int Altitude { get; set; }

        /// <summary>
        /// Документ
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("document")]
        public int Document { get; set; }

        /// <summary>
        /// Подредба
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("abc")]
        public int Abc { get; set; }

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
