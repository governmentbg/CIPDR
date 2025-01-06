namespace URegister.NomenclaturesCatalog.Model.Ekatte
{
    public partial class Ek_doc
    {
        /// <summary>
        /// Note:
        /// <br/>This is a Primary Key.&lt;pk/&gt;
        /// </summary>

        [System.Text.Json.Serialization.JsonPropertyName("document")]
        public int? Document { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("doc_kind")]
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string Doc_kind { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("doc_name")]
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Doc_name { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("doc_name_en")]
        [System.ComponentModel.DataAnnotations.StringLength(200)]
        public string Doc_name_en { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("doc_inst")]
        [System.ComponentModel.DataAnnotations.StringLength(255)]
        public string Doc_inst { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("doc_num")]
        [System.ComponentModel.DataAnnotations.StringLength(20)]
        public string Doc_num { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("doc_date")]
        [System.Text.Json.Serialization.JsonConverter(typeof(DateFormatConverter))]
        public System.DateTimeOffset? Doc_date { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("doc_act")]
        [System.Text.Json.Serialization.JsonConverter(typeof(DateFormatConverter))]
        public System.DateTimeOffset? Doc_act { get; set; } 

        [System.Text.Json.Serialization.JsonPropertyName("dv_danni")]
        [System.ComponentModel.DataAnnotations.StringLength(20)]
        public string Dv_danni { get; set; } = null!;

        [System.Text.Json.Serialization.JsonPropertyName("dv_date")]
        [System.Text.Json.Serialization.JsonConverter(typeof(DateFormatConverter))]
        public System.DateTimeOffset? Dv_date { get; set; }

    }
}
