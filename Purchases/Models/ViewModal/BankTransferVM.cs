using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class BankTransferVM
    {
        public int Id { get; set; }
        public int FromAccTreeId { get; set; }
        public int ToAccTreeId { get; set; }
        public decimal Amount { get; set; }
        public int Currency { get; set; }
        public string Note { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}