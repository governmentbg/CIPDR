namespace URegister.NomenclaturesCatalog.Model.Ekatte
{
    public class Ek_reg2
    {

        [System.Text.Json.Serialization.JsonPropertyName("region")]
        [System.ComponentModel.DataAnnotations.StringLength(15)]
        public string Region { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("nuts1")]
        [System.ComponentModel.DataAnnotations.StringLength(15)]
        public string Nuts1 { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("nuts2")]
        [System.ComponentModel.DataAnnotations.StringLength(15)]
        public string Nuts2 { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("name")]
        [System.ComponentModel.DataAnnotations.StringLength(150)]
        public string Name { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("name_en")]
        [System.ComponentModel.DataAnnotations.StringLength(150)]
        public string Name_en { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("document")]
        public int? Document { get; set; }

    }
}
