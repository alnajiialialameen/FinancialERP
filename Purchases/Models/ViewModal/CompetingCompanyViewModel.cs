using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class CompetingCompanyViewModel
    {
        public int Id { get; set; }
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