using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class TreeAccVM
    {
        public int Id { get; set; }
        public string AccName { get; set; }
        public Nullable<int> AccParent { get; set; }
        public string AccCode { get; set; }
        public Nullable<int> TheLevel { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> AccTypeId { get; set; }
        public Nullable<int> AccNatureId { get; set; }
        public Nullable<int> AccFinalId { get; set; }
        public Nullable<int> AccSubCategory { get; set; }
    }
}