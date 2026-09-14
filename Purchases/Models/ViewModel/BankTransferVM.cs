using System;

namespace Purchases.Models.ViewModel
{
    public class BankTransferVM
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public int FromAccTreeId { get; set; } // credit
        public int ToAccTreeId { get; set; } // debit
        public string CreditAccTreeName { get; set; }
        public string DebitAccTreeName { get; set; }
        public decimal? Amount { get; set; }
        public decimal? ExchangeRate { get; set; }
        public int Currency { get; set; }
        public string Note { get; set; }
        public string TransactionDateStr { get; set; }
        public DateTime? TransactionDate { get; set; }
    }
}