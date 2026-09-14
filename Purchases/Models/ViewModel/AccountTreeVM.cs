using System;

namespace Purchases.Models.ViewModel
{
    public class AccountTreeVM
    {
        public int Id { get; set; }
        public string AccName { get; set; }
        public Nullable<int> AccParentId { get; set; }
        public string AccParent { get; set; }
        public string AccCode { get; set; }
        public string AccLevel { get; set; }
        public string AccType { get; set; }
        public string AccNature { get; set; }
        public string AccFinal { get; set; }
        public Nullable<int> TheLevel { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public Nullable<int> AccTypeId { get; set; }
        public Nullable<int> AccNatureId { get; set; }
        public Nullable<int> AccFinalId { get; set; }
        public Nullable<int> AccSubCategory { get; set; }
    }
}