namespace Purchases.Models.ViewModel
{
    public class CompetingCompanyVM : BaseVM
    {
        public int CompanyId { get; set; }
        public int? AccountTreeId { get; set; }
        public string Name { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string StatusText { get; set; }
        public int Status { get; set; }
        public string LicenseNumber { get; set; }
        public bool IsQualified { get; set; }
        public bool IsAgentComapny { get; set; }

    }
}