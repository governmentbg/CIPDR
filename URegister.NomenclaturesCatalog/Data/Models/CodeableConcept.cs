using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace URegister.NomenclaturesCatalog.Data.Models
{
    /// <summary>
    /// Номенклатура
    /// </summary>
    [Table("codeable_concepts")]
    [Comment("Номенклатура")]
    [Index(nameof(Type), nameof(Code), nameof(DateFrom), IsUnique = true)]
    public class CodeableConcept
    {
        [Key]
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Comment("Идентификатор")]
        public long Id { get; set; }

        [StringLength(20, MinimumLength = 1, ErrorMessage = "Максималната дължина на полето {0} е {1}")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        [Comment("Код")]
        public string Code { get; set; } = null!;


        /// <summary>
        /// Стойност
        /// </summary>
        [StringLength(255, MinimumLength = 1, ErrorMessage = "Максималната дължина на полето {0} е {1}")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        [Comment("Стойност")]
        public string Value { get; set; } = null!;

        [StringLength(255, ErrorMessage = "Максималната дължина на полето {0} е {1}")]
        [Comment("Стойност ЕН")]
        public string? ValueEn { get; set; }

        /// <summary>
        /// Валидна от дата
        /// </summary>
        [Column(TypeName = "date")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        [Comment("Валидна от дата")]
        public DateTime DateFrom { get; set; } 

        /// <summary>
        /// Валидна до дата
        /// </summary>
        [Column(TypeName = "date")]
        [Comment("Валидна до дата")]
        public DateTime? DateTo { get; set; } 


        /// <summary>
        /// Тип на номенклатура
        /// </summary>
        [Required(ErrorMessage = "Полето {0} е задължително")]
        [StringLength(6, MinimumLength = 5, ErrorMessage = "Дължината на полето {0} трябва да е {1}")]
        [Comment("Тип на номенклатура")]
        public string Type { get; set; } = null!;

        /// <summary>
        /// Код не горно ниво при дървовидна номенклатура
        /// </summary>
        [Comment("Код не горно ниво при дървовидна номенклатура")]
        public string? ParentCode { get; set; } 

        /// <summary>
        /// Създаден от
        /// </summary>
        [StringLength(255)]
        [Comment("Създаден от")]
        public string? CreatedBy { get; set; } = null!;

        /// <summary>
        /// Дата и час на записа
        /// </summary>
        [Column(TypeName = "timestamptz")]
        [Comment("Дата и час на записа")]
        public DateTime? CreatedOn { get; set; } = null!;


        /// <summary>
        /// Горно ниво в друг номенклатурен тип
        /// </summary>
        [Comment("Горно ниво в друг номенклатурен тип")]
        public string? HolderCode { get; set; }

        /// <summary>
        /// Допълнителни данни
        /// </summary>
        public ICollection<AdditionalColumn>? AdditionalColumns { get; set; }

        public void AdditionalColumn(string name, string value, string valueEn = null)
        {
            if (AdditionalColumns == null) 
                AdditionalColumns = new List<AdditionalColumn>();
            AdditionalColumns.Add(new AdditionalColumn
            {
                Name = name,
                Value = value,
                ValueEn = valueEn
            });
        }
    }
}
