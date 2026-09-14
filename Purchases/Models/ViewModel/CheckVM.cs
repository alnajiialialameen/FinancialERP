using System;
namespace Purchases.Models.ViewModel
{
    public class CheckVM : BaseVM
    {
        public int BankAccountId { get; set; }
        public string BooKNumber { get; set; }
        public Nullable<int> StartFromNumber { get; set; }
        public Nullable<int> EndToNumber { get; set; }
        public Nullable<bool> IsFinished { get; set; }

    }
}