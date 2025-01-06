namespace URegister.NomenclaturesCatalog.Model.Ekatte
{
    public class Ek_raion
    {
        [System.Text.Json.Serialization.JsonPropertyName("raion")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(15)]
        public string Raion { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(150)]
        public string Name { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("name_en")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        [System.ComponentModel.DataAnnotations.StringLength(150)]
        public string Name_en { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("document")]
        [System.ComponentModel.DataAnnotations.Required(AllowEmptyStrings = true)]
        public int? Document { get; set; }
    }
}
