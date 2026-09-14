using System;
using System.Collections.Generic;

namespace Purchases.Models.ViewModel
{
    public class AllViewMode
    {
        public int id { get; set; }// this is TransactionDetail Id
        //public int accTreeId { get; set; }
        //public string accName { get; set; }
        //public string accLevel { get; set; }
        //public string accParent { get; set; }
        //public string accCode { get; set; }
        //public string accType { get; set; }
        //public string accNature { get; set; }
        //public string accFinal { get; set; }
        public Nullable<int> countOfCridet { get; set; }
        public Nullable<int> countOfDebit { get; set; }
        public Nullable<int> rowsCount { get; set; }
        public Nullable<decimal> sumOfCridet { get; set; }
        public Nullable<decimal> sumOfDebit { get; set; }
        public Nullable<decimal> balance { get; set; }
        public string note { get; set; }

        public AccountTreeVM AccTree { get; set; }
        public List<TransactionVM> transDetails { get; set; }
        public List<TransactionVM> lastFivetrans { get; set; }
    }

}