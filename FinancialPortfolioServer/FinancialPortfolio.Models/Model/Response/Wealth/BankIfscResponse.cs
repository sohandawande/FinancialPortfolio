namespace FinancialPortfolio.Models.Model.Response.Wealth
{
    public class BankIfscResponse
    {
        public string Ifsc { get; set; } = string.Empty;
        public string Bank { get; set; } = string.Empty;
        public string? BankCode { get; set; }
        public string? Branch { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? State { get; set; }
        public string? Contact { get; set; }
        public string? Micr { get; set; }
        public bool? Rtgs { get; set; }
        public bool? Neft { get; set; }
        public bool? Imps { get; set; }
        public bool? Upi { get; set; }
    }
}
