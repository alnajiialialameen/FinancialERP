using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models
{
    public class UserVM
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string AirportName { get; set; }
        public string Phone { get; set; }
        public int EmployeeId { get; set; }
        public string Password { get; set; }


        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public int? FinancialCycleId { get; set; }
        public string CompanyInfoName { get; set; }
        public int? CompanyInfoId { get; set; }
    }
}