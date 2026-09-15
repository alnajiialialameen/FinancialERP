using System;
using System.Collections.Generic;

namespace Purchases.Models.ViewModel
{
    public class BalanceVM
    {
        public int Id { get; set; }
        public int accTreeId { get; set; }
        public int transactionId { get; set; }
        public string accTreeName { get; set; }
        public Nullable<int> balanceId { get; set; }
        public Nullable<int> currencyId { get; set; }
        public string accTreeCode { get; set; }
        public Nullable<decimal> actualExchange { get; set; }
        public Nullable<decimal> credint { get; set; }
        public Nullable<decimal> credintPercent { get; set; }
        public Nullable<decimal> sumOfCredint { get; set; }
        public Nullable<decimal> deviationRelative { get; set; }
        public string parentName { get; set; }
        public int financeCycleId { get; set; }
        public int? parentId { get; set; }
        public Nullable<decimal> sumOfCredit { get; set; }
        public Nullable<decimal> sumOfDebit { get; set; }
        public Nullable<decimal> Diff { get; set; }
        public Nullable<bool> isExpense { get; set; }
        public Nullable<DateTime> transactionDate { get; set; }
        public string note { get; set; }
        public string transactionDateStr { get; set; }
        public Nullable<decimal> credit { get; set; }
        public Nullable<decimal> debit { get; set; }
        public Nullable<decimal> relativeDeviation { get; set; }
        public string year { get; set; }
        public List<BalanceVM> details { get; set; }
        public TransactionDetail detail { get; set; }
    }
}