using System;

namespace Purchases.Models.ViewModel
{
    public class GeneralConditionsVM
    {
        public int Id { get; set; }
        public int GeneralConditionId { get; set; }
        public int CompetingCompanyId { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> MaxValue { get; set; }
        public Nullable<decimal> InputValue { get; set; }
        public Nullable<bool> IsActive { get; set; }
    }
}