using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class RecieptVM
    {
        public int BalanceAccTreeId { get; set; }
        public int DebitAccTreeId { get; set; }
        public int CreditAccTreeId { get; set; }
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public int Currency { get; set; }
        public string checkNo { get; set; }
        public string DebitNote { get; set; }
        public string CreditNote { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Recipient { get; set; }
    }
}