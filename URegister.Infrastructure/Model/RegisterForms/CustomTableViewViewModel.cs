using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Model.RegisterForms
{
    public class CustomTableViewViewModel
    {
        private string? _searchPattern;
        private string? _fieldName;
        private string? _incomingNumber;
        private string? _registerNumber;
        private string? _mprId;
        private string _title;
        private string _submitterId;

        [DisplayName("Търсене по поле:")]
        [MaxLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? FieldName
        {
            get => _fieldName;
            set => _fieldName = value?.Trim();
        }

        [DisplayName("Низ за търсене")]
        [MaxLength(500, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? SearchPattern
        {
            get => _searchPattern;
            set => _searchPattern = value?.Trim();
        }

        [Required(ErrorMessage = MessageConstant.FieldIsRequired)]
        public int ServiceId { get; set; } = 0;

        public int CustomViewId { get; set; } = 0;

        public string Title
        {
            get => _title;
            set => _title = value?.Trim();
        }

        [DisplayName("Входящ номер")]
        [StringLength(20, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? IncomingNumber
        {
            get => _incomingNumber;
            set => _incomingNumber = value?.Trim();
        }

        [DisplayName("Номер на вписване")]
        [StringLength(20, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? RegisterNumber
        {
            get => _registerNumber;
            set => _registerNumber = value?.Trim();
        }

        [DisplayName("Дата на вписване от")]
        public DateTime? IncomingDateFrom { get; set; } = null;

        [DisplayName("Дата на вписване до")]
        public DateTime? IncomingDateTo { get; set; } = null;

        [DisplayName("Идентификатор партида")]
        [StringLength(20, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string? MprId
        {
            get => _mprId;
            set => _mprId = value?.Trim();
        }

        [DisplayName("Идентификатор заявител")]
        [StringLength(20, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string SubmitterId
        {
            get => _submitterId;
            set => _submitterId = value?.Trim();
        }
    }
}
