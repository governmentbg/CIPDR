using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.NomenclaturesCatalog.Model.Ekatte;

namespace URegister.NomenclaturesCatalog.Infrastructure.Data.Models.Nomenclatures
{
    /// <summary>
    /// Импрортирани документи от nrnm.nsi.bg
    /// </summary>
    [Comment("Импрортирани документи от nrnm.nsi.bg")]
    [Table("ek_doc")]
    public class EkDoc
    {
        [Key]
        public int Document { get; set; }

        [StringLength(255)]
        public string Doc_kind { get; set; } = null!;

        [StringLength(200)]
        public string Doc_name { get; set; } = null!;

        [StringLength(200)]
        public string Doc_name_en { get; set; } = null!;

        [StringLength(255)]
        public string Doc_inst { get; set; } = null!;

        [StringLength(20)]
        public string Doc_num { get; set; } = null!;

        [Column(TypeName = "date")]
        public DateTime? Doc_date { get; set; }

        [Column(TypeName = "date")]
        public DateTime? Doc_act { get; set; }

        [StringLength(20)]
        public string Dv_danni { get; set; } = null!;

        [Column(TypeName = "date")]
        public DateTime? Dv_date { get; set; }

        public int Status { get; set; }
    }
}

