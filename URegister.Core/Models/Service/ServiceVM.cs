using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using URegister.Core.Models.Common;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Service
{
    public class ServiceVM
    {
        private string _name = null!;

        /// <summary>
        /// Идентификатор на тип на услуга
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Име на тип на услуга
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Име на услуга")]
        public string Name
        {
            get => _name;
            set => _name = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Идентификатор на тип услуга
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [Display(Name = "Тип услуга")]
        public int ServiceTypeId { get; set; }

        /// <summary>
        /// Идентификатор на тип форма
        /// </summary>        
        [Display(Name = "Форма")]
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public int FormParentId { get; set; }

        /// <summary>
        /// Стъпки
        /// </summary>
        [Display(Name = "Стъпки")]
        public List<ServiceStepVM> Steps { get; set; } = new();

        public bool IsInsert => Id <= 0;


        /// <summary>
        /// Номер на услуга в е-формите
        /// </summary>
        [Display(Name = "Номер на услуга в е-формите")]
        public string? EFormCode { get; set; }


        public bool IsForCertificateOnRegister()
        {
            return ServiceTypeId == (int)ServiceTypes.Register ||
                   ServiceTypeId == (int)ServiceTypes.Change ||
                   ServiceTypeId == (int)ServiceTypes.AskForCorrectionError;

        }

        public bool IsCertificate()
        {
            return ServiceTypeId == (int)ServiceTypes.Document;

        }
    }
}
