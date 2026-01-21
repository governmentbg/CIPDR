namespace URegister.Infrastructure.Model.EDelivery
{
    public class ImportJsonVM
    {
        public string? JsonFromFile { get; set; }
        public string? AdministrationUic { get; set; }
        public string? RegisterCode { get; set; }

        public string? RegisterNumber { get; set; }
        public int ServiceId { get; set; }
        public List<EDeliveryFileVM> EDeliveryFiles { get; set; } = new();
    }
}
