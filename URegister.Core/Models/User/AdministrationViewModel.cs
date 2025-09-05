using Microsoft.AspNetCore.Mvc.Rendering;

namespace URegister.Core.Models.User
{
    public class AdministrationViewModel
    {
        public string SelectedAdministrationId { get; set; }
        public string RegisterCode { get; set; }
        public List<SelectListItem> Administrations { get; set; }
    }
}
