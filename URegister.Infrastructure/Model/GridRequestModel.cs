namespace URegister.Infrastructure.Model
{
    public class GridRequestModel
    {
        public int page { get; set; }
        public int size { get; set; }
        public string? filter { get; set; }
        public string? exportFormat { get; set; }
        public string? data { get; set; }
    }
}
