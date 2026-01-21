using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Models.Register
{
    public class RegisterVM
    {
        // Backing fields for string properties
        private string _code = null!;
        private string _name = null!;
        private string? _description;
        private string _legalBasis = null!;
        private string _type = null!;
        private string? _identitySecurityLevel;
        private string _typeEntry = null!;

        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дали се редактира администрация
        /// </summary>
        public bool IsEditAdministration { get; set; }
        /// <summary>
        /// Код на регистър
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(10, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Код на регистър")]
        public string Code
        {
            get => _code;
            set => _code = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Име на регистър
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(500, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Име на регистър")]
        public string Name
        {
            get => _name;
            set => _name = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Описание
        /// </summary>
        [MaxLength(1000, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Описание")]
        //[RegularExpression(RegexPatterns.CyrillicTextPattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string? Description
        {
            get => _description;
            set => _description = value?.Trim();
        }

        /// <summary>
        /// Правно основание
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(1000, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Правно основание")]
        //[RegularExpression(RegexPatterns.CyrillicTextPattern, ErrorMessage = MessageConstant.NotCyrillic)]
        public string LegalBasis
        {
            get => _legalBasis;
            set => _legalBasis = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Вид на регистъра
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(5, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Вид на регистъра")]
        public string Type
        {
            get => _type;
            set => _type = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Ниво на осигуреност на средствата за електронна идентификация при заявяване на ЕАУ
        /// </summary>
        [Display(Name = "Ниво на осигуреност на средствата за електронна идентификация при заявяване на ЕАУ")]
        public string? IdentitySecurityLevel
        {
            get => _identitySecurityLevel;
            set => _identitySecurityLevel = value?.Trim();
        }
       

        /// <summary>
        /// Начин на вписване
        /// </summary>
        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        [MaxLength(5, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Display(Name = "Начин на вписване")]
        public string TypeEntry
        {
            get => _typeEntry;
            set => _typeEntry = value?.Trim() ?? throw new ArgumentNullException(nameof(value));
        }


        /// <summary>
        /// Ръководител
        /// </summary>
        public PersonVM Manager { get; set; } = new PersonVM
        {
            Type = PersonTypeValue.Manager,
        };

        /// <summary>
        /// Оторозирани служители
        /// </summary>
        public List<PersonVM> ContactPersons { get; set; } = new List<PersonVM>
        {
            new PersonVM
            {
                Type = PersonTypeValue.AuthorizedPerson,
            }
        };

        /// <summary>
        /// Администрация на заявител
        /// </summary>
        public AdministrationVM Administration { get; set; } = new();

        public int StatusId { get; set; }

        public RegisterFileListVM RegisterFiles { get; set; } = new()
        {
            FilesLabel = "Прикачени файлове"
        };

        public RegisterFileListVM AdministrationFiles { get; set; } = new();

        /// <summary>
        /// Историята не е публична
        /// </summary>
        [DisplayName("Историята не е публична")]
        public bool HistoryNotPublic { get; set; } = false;
    }
}