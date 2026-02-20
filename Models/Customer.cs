namespace TariffSwitch.Processor.Models
{
    public class Customer
    {
        public string CustomerId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool HasUnpaidInvoice { get; set; }
        public string Sla { get; set; } = string.Empty;       // "Premium" or "Standard"
        public string MeterType { get; set; } = string.Empty; // "Smart" or "Classic"
    }
}