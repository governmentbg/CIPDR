namespace URegister.NomenclaturesCatalog.Model.Ekatte
{
    public class Ek_sobr
    {
        /// <summary>
        /// Код
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("ekatte")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(5)]
        public string Ekatte { get; set; } = null!;

        /// <summary>
        /// Тип
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("kind")]
        public int Kind { get; set; }

        /// <summary>
        /// Наименование на български
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(70)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Находящо се в землище
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("area1")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string Area1 { get; set; } = null!;

        /// <summary>
        /// Находящо се в землище
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("area2")]
        [System.ComponentModel.DataAnnotations.StringLength(100)]
        public string Area2 { get; set; } = null!;

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
        [System.ComponentModel.DataAnnotations.StringLength(70)]
        public string Name_en { get; set; } = null!;

    }
}
