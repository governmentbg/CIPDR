using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using URegister.Core.Data.Models.Register;
using URegister.Infrastructure.Data.Common;

namespace URegister.Core.Data.Models.Process
{
    /// <summary>
    /// Вписвания
    /// </summary>
    [Comment("Вписвания")]
    public class RegisterItem : SoftDeletable
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        [Comment("Идентификатор")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Идентификатор на MasterPersonIndex
        /// </summary>
        [Comment("Идентификатор на MasterPersonIndex")]
        public Guid MpriId { get; set; }


        /// <summary>
        /// Идентификатор на процес
        /// </summary>
        [Comment("Идентификатор на процес")]
        public Guid ProcessId { get; set; }

        /// <summary>
        /// Идентификатор на стъпка от процес
        /// </summary>
        [Comment("Идентификатор на стъпка от процес")]
        public Guid? ProcessStepId { get; set; }
        /// <summary>
        /// Идентификатор на администрация
        /// </summary>
        [Comment("Идентификатор на администрация")]
        public Guid TenantId { get; set; }

        /// <summary>
        /// Номер на вписване 
        /// </summary>
        [Required]
        [Comment("Номер на вписване ")]
        [StringLength(16)]
        public string RegisterNumber { get; set; } = null!;

        /// <summary>
        /// Идентификатор на поле
        /// </summary>
        [Comment("Идентификатор на поле")]
        public Guid FieldId { get; set; }

        /// <summary>
        /// Идентификатор на главно поле
        /// </summary>
        [Comment("Идентификатор на главно поле")]
        public Guid ParentFieldId { get; set; }

        /// <summary>
        /// Комплексно поле
        /// </summary>
        [Required]
        [Comment("Комплексно поле")]
        public bool IsComplex { get; set; }

        /// <summary>
        /// Име
        /// </summary>
        [Required]
        [Comment("Име")]
        [StringLength(255)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Име
        /// </summary>
        [Comment("Индекс на повтарящо се поле")]
        public int Index { get; set; }

        /// <summary>
        /// Стойност
        /// </summary>
        [Comment("Стойност")]
        public string? Value { get; set; }
        /// <summary>
        /// Тип номенклатура
        /// </summary>
        [Comment("Тип номенклатура")]
        public string? NomenclatureType { get; set; }

        /// <summary>
        /// Стойност на номенклатура
        /// </summary>
        [Comment("Стойност на номенклатура")]
        public string? ClValue { get; set; }

        /// <summary>
        /// Публично поле
        /// </summary>
        [Required]
        [Comment("Публично поле")]
        public bool IsPublic { get; set; }

        /// <summary>
        /// Стойност на номенклатура
        /// </summary>
        [Comment("Етикет на полето")]
        public string? Label { get; set; }

        /// <summary>
        /// Булева стойност
        /// </summary>
        [Comment("Булева стойност")]
        public bool? BoolValue { get; set; }

        /// <summary>
        /// Числова стойност
        /// </summary>
        [Comment("Числова стойност")]
        public decimal? DecimalValue { get; set; }

        /// <summary>
        /// Дата стойност
        /// </summary>
        [Comment("Дата стойност")]
        public DateTime? DateTimeValue { get; set; }

        /// <summary>
        /// Тип на поле
        /// </summary>
        [Comment("Тип на поле")]
        [Required]
        public int FieldTypeId { get; set; }

        /// <summary>
        /// Процес
        /// </summary>
        [ForeignKey(nameof(ProcessId))]
        public Process Process { get; set; } = null!;

        /// <summary>
        /// Стъпка от процес
        /// </summary>
        [ForeignKey(nameof(ProcessStepId))]
        public ProcessStep ProcessStep { get; set; } = null!;

    }
}
