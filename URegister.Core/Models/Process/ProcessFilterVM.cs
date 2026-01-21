using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;
using URegister.Core.Models.Common;

namespace URegister.Core.Models.Process
{
    /// <summary>
    /// Модел на филтър за процеси
    /// </summary>
    public class ProcessFilterVM
    {
        /// <summary>
        /// От дата на входиране
        /// </summary>
        [Display(Name = "От дата на входиране")]
        public DateTime? IncomingDateFrom { get; set; }

        /// <summary>
        /// До дата на входиране
        /// </summary>
        [Display(Name = "До дата на входиране")]
        public DateTime? IncomingDateTo { get; set; }

        /// <summary>
        /// Входящ номер
        /// </summary>
        [MaxLength(20, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Входящ номер")]
        public string? IncomingNumber
        {
            get => _incomingNumber;
            set => _incomingNumber = value?.Trim();
        }
        private string? _incomingNumber;

        /// <summary>
        /// Номер на вписване 
        /// </summary>
        [Display(Name = "Рег. на вписване ")]
        public string? RegisterNumber
        {
            get => _registerNumber;
            set => _registerNumber = value?.Trim();
        }
        private string? _registerNumber;

        /// <summary>
        /// История по ид
        /// </summary>
        public Guid? FromProcessId { get; set; }
        

        /// <summary>
        /// Идентификатор на услуга
        /// </summary>
        [Display(Name = "Услуга")]
        public int ServiceId { get; set; }

        /// <summary>
        /// Идентификатор на стъпка
        /// </summary>
        [Display(Name = "Стъпка")]
        public int StepId { get; set; }

        /// <summary>
        /// Идентификатор на статус
        /// </summary>
        [Display(Name = "Статус")]
        public int StatusId { get; set; }

        /// <summary>
        /// Идентификатор на партида
        /// </summary>
        [Display(Name = "Идентификатор на партида")]
        public PersonIdentifierVM PersonIdentifier { get; set; } = new();

        /// <summary>
        /// Идентификатор на заявител
        /// </summary>
        [Display(Name = "Идентификатор на заявител")]
        public PersonIdentifierVM PersonIdentifierApplicant { get; set; } = new();

        public Guid? AssignedToUserId { get; set; }

        public bool ForDeAssignUser { get; set; }

        public bool RegisterServiceHasJustOneStep { get; set; } = false;
    }
}
