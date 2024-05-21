using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class AllViewMode
    {
        public int id { get; set; }// this is TransactionDetail Id
        public int accTreeId { get; set; }
        public string accName { get; set; }
        public string accLevel { get; set; }
        public string accParent { get; set; }
        public string accCode { get; set; }
        public string accType { get; set; }
        public string accNature { get; set; }
        public string accFinal { get; set; }
        public Nullable<int> countOfCridet { get; set; }
        public Nullable<int> countOfDebit { get; set; }
        public Nullable<int> rowsCount { get; set; }
        public Nullable<decimal> sumOfCridet { get; set; }
        public Nullable<decimal> sumOfDebit { get; set; }
        public Nullable<decimal> balance { get; set; }
        public string note { get; set; }

        public List<TransactionViewMode> transDetails { get; set; }
        public List<TransVM> lastFivetrans { get; set; }
    }

    public class TransVM
    {
        public Nullable<int> transactionId { get; set; }
        public Nullable<DateTime> TransactionDate { get; set; }
        public string transDate { get; set; }
        public string DocumentType { get; set; }
        public string CurrencyType { get; set; }
        public Nullable<decimal> ExchangeRate { get; set; }
        public Nullable<decimal> Amount { get; set; }

    }
}