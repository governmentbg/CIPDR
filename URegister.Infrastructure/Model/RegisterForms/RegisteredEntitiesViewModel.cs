using System.ComponentModel;

namespace URegister.Infrastructure.Model.RegisterForms
{
    public class RegisteredEntitiesViewModel
    {
        [DisplayName("Изберете услуга")]
        public int ServiceId { get; set; }
    }
}
