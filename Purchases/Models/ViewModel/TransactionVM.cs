using System;

namespace Purchases.Models.ViewModel
{
    public class TransactionVM 
    {
        public Nullable<int> transactionId { get; set; }
        public Nullable<DateTime> transactionDate { get; set; }
        //public string transDate { get; set; }
        public int documentTypeId { get; set; }
        public string documentType { get; set; }
        public int currencyId { get; set; }
        public int? recipientId { get; set; }
        public int? companyInfoId { get; set; }
        public string currency { get; set; }
        public Nullable<decimal> exchangeRate { get; set; }
        public Nullable<decimal> amount { get; set; }
        public Nullable<bool> isPosted { get; set; }
        public string transactionDateStr { get; set; }
        public string userId { get; set; }
        public string createdBy { get; set; }
        public string updatedBy { get; set; }

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
        public bool? hasTax { get; set; }
        public int? CheckNo { get; set; }
        public int? CheckType { get; set; }
        public string topParent { get; set; }
        public string hasAddedTaxTxt { get; set; }
        public Nullable<bool> isCredit { get; set; }

    }
}