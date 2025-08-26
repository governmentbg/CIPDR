using URegister.Admin.Models.Service;

namespace URegister.Admin.Models
{
    public class CodeableConceptListExportVM
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public List<CodeableConceptItemExportVM> Values { get; set; } = new();
    }
}
