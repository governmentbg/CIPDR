using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Data.Models.Register;

namespace URegister.Core.Data.Models.Process
{
    /// <summary>
    /// Вписвания
    /// </summary>
    [Comment("Вписвания")]
    public class RegisterItem
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
        /// Идентификатор на администрация
        /// </summary>
        [Comment("Идентификатор на администрация")]
        public Guid TennantId { get; set; }

        /// <summary>
        /// Номер на вписване 
        /// </summary>
        [Required]
        [Comment("Номер на вписване ")]
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
        public string Name { get; set; } = null!;

        /// <summary>
        /// Стойност
        /// </summary>
        [Comment("Стойност")]
        public string? Value { get; set; }

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
        /// Заличено поле
        /// </summary>
        [Required]
        [Comment("Заличено поле")]
        public bool IsDelеted { get; set; }

        /// <summary>
        /// Процес
        /// </summary>
        [ForeignKey(nameof(ProcessId))]
        public Process Process { get; set; } = null!;

        /// <summary>
        /// Администрация
        /// </summary>
        [ForeignKey(nameof(TennantId))]
        public Administration Administration { get; set; } = null!;
    }
}
