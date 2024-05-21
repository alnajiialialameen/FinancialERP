using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class GeneralConditionviewmodel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Nullable<decimal> MaxValue { get; set; }
        public Nullable<bool> IsActive { get; set; }
    }
}