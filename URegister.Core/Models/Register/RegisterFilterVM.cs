using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace URegister.Core.Models.Register
{
    /// <summary>
    /// Филтър за регистри
    /// </summary>
    public class RegisterFilterVM
    {
        /// <summary>
        /// Код
        /// </summary>
        [Display(Name = "Код")]
        public string? Code
        {
            get => _code;
            set => _code = value?.Trim();
        }
        private string? _code;

        /// <summary>
        /// Име
        /// </summary>
        [Display(Name = "Име")]
        public string? Name
        {
            get => _name;
            set => _name = value?.Trim();
        }
        private string? _name;

        /// <summary>
        /// Описание
        /// </summary>
        [Display(Name = "Описание")]
        public string? Description
        {
            get => _description;
            set => _description = value?.Trim();
        }
        private string? _description;

        [DisplayName("Дата на създаване от")]
        public DateTime? DateFrom { get; set; } = null;

        [DisplayName("Дата на създаване до")]
        public DateTime? DateTo { get; set; } = null;

        [DisplayName("Администрация")]
        public Guid? AdministrationId { get; set; } = null;

        /// <summary>
        /// Вид на регистъра
        /// </summary>
        [MaxLength(5)]
        [DisplayName("Вид на регистъра")]
        public string? Type { get; set; } = null;

        /// <summary>
        /// Ниво на осигуреност на средствата за електронна идентификация
        /// </summary>
        [MaxLength(5)]
        [DisplayName("Ниво на осигуреност на средствата за електронна идентификация")]
        public string? IdentitySecurityLevel { get; set; } = null;

        /// <summary>
        /// Начин на вписване
        /// </summary>
        [MaxLength(5)]
        [DisplayName("Начин на вписване")]
        public string? TypeEntry { get; set; } = null;

        /// <summary>
        /// Статус
        /// </summary>
        [DisplayName("Статус")]
        public int StatusId { get; set; } 

        [DisplayName("Активен")] 
        public bool IsActive { get; set; } = true;
    }
}
