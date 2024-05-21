using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class BankViewmodel
    {
        public int Id { get; set; }
        public string AccName { get; set; }
        public string Number { get; set; }
        public string accParentName { get; set; }
        public Nullable<int> BankAccountTypeId { get; set; }
        public Nullable<int> CurrencyTypeId { get; set; }
        public Nullable<int> AccountSubId { get; set; }
        public Nullable<int> AccId { get; set; }
        public int AccCategoryId { get; set; }
        public DateTime OpenDate { get; set; }
        public string IBan { get; set; }
    }
}