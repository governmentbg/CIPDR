namespace URegister.Infrastructure.Model.EDelivery
{
    public class ImportResultVM
    {
        public string? Status { get; set; }
        public string? IncomingNumber { get; set; }
        public DateTime? IncomingDate { get; set; }
        public Guid? ProcessId { get; set; }
        public DateTime? Timestamp { get; set; }
     }
}
