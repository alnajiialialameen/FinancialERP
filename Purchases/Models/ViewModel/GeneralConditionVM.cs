using System;

namespace Purchases.Models.ViewModal
{
    public class GeneralConditionVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> MaxValue { get; set; }
        public Nullable<bool> IsActive { get; set; }
    }
}