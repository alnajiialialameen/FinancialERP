using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class TransactionViewMode
    {
        public Nullable<int> transactionId { get; set; }
        public Nullable<DateTime> transactionDate { get; set; }
        public string transDate { get; set; }
        public int documentTypeId { get; set; }
        public int currencyId { get; set; }
        public Nullable<decimal> exchangeRate { get; set; }
        public Nullable<decimal> amount { get; set; }
        public Nullable<bool> isPosted { get; set; }


        public int id { get; set; }// this is TransactionDetail Id
        public Nullable<int> accTrreId { get; set; }
        public Nullable<int> balanceId { get; set; }
        public string accName { get; set; }
        public string balanceAccName { get; set; }
        public Nullable<decimal> debit { get; set; }
        public Nullable<decimal> credit { get; set; }
        public Nullable<decimal> tax { get; set; }
        public Nullable<decimal> total { get; set; }
        public string note { get; set; }
        public string recipient { get; set; }
        public bool? hasAddedTax { get; set; }

    }
}