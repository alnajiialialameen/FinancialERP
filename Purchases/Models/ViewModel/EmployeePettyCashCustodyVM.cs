using System;

namespace Purchases.Models.ViewModel
{
    public class EmployeePettyCashCustodyVM
    {
        public int Id { get; set; }
        public Nullable<int> EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public Nullable<decimal> TotalDeductionAmount { get; set; }
        public Nullable<decimal> PaiedAmount { get; set; }
        public Nullable<int> FinancialCycleId { get; set; }
        public Nullable<int> AccTreeId { get; set; }
        public string AccTreeName { get; set; }
    }
}