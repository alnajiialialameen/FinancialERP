using System;
namespace Purchases.Models.ViewModal
{
    public class BankVM
    {
        public int Id { get; set; }
        public string AccName { get; set; }
        public string Number { get; set; }
        public string BankLabel { get; set; }
        public string accParentName { get; set; }
        public Nullable<int> BankAccountTypeId { get; set; }
        public Nullable<int> CurrencyTypeId { get; set; }
        public Nullable<int> AccountSubId { get; set; }
        public Nullable<int> AccId { get; set; }
        public int AccCategoryId { get; set; }
        public DateTime OpenDate { get; set; }
        public string IBan { get; set; }
        public string FirstSignature { get; set; }
        public string SecondSignature { get; set; }
    }
}