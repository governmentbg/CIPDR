using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace URegister.Infrastructure.Model.AuditLog
{
    public class AuditLogFilterVM
    {
        private string _actionType;
        private string _ipAddress;
        private string _userName;
        /// <summary>
        /// От дата на извършване на действие
        /// </summary>
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        /// <summary>
        /// До дата на извършване на действие
        /// </summary>
        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Тип на действие")]
        public string ActionType
        {
            get => _actionType;
            set => _actionType = value?.Trim();
        }

        [Display(Name = "IP адрес")]
        public string IpAddress
        {
            get => _ipAddress;
            set => _ipAddress = value?.Trim();
        }

        [Display(Name = "Потребител")]
        public string UserName
        {
            get => _userName;
            set => _userName = value?.Trim();
        }
    }
}
