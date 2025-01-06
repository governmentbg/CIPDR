using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using URegister.Infrastructure.Constants;
using Google.Protobuf.WellKnownTypes;

namespace URegister.Infrastructure.Model.KeyCloak
{
    public class UserViewModel
    {
        private DateTime _createdAt;
        private long _createdTimestamp;

        public string Id { get; set; }

        [DisplayName("Потребителско име")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        [Required]
        public string Username { get; set; }

        [DisplayName("Име")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string FirstName { get; set; }

        [DisplayName("Фамилия")]
        [StringLength(50, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string LastName { get; set; }

        [DisplayName("Email")]
        [StringLength(100, ErrorMessage = MessageConstant.StringMaxLengthValidation)]
        public string Email { get; set; }

        [DisplayName("Администрация(число)")]
        public string? AdministrationId { get; set; }

        [DisplayName("Email верификация")]
        public bool EmailVerified { get; set; }

        [DisplayName("Групи")]
        public string GroupList{ get; set; }

        public string GroupIds{ get; set; }

        public long CreatedTimestamp
        {
            get => _createdTimestamp;
            set
            {
                _createdTimestamp = value;
                _createdAt = DateTimeOffset.FromUnixTimeMilliseconds(_createdTimestamp).DateTime;
                CreatedAtStr = _createdAt.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set
            {
                _createdAt = value;
                CreatedAtStr = _createdAt.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }

        [DisplayName("Дата на създаване")]
        public string CreatedAtStr { get; private set; }

        [DisplayName("Активиран")]
        public bool Enabled { get; set; }
    }
}
