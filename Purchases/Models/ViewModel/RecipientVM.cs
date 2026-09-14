using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModel
{
    public class RecipientVM
    {
        public int? Id { get; set; } = 0;
        public string RecipientName { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }
    }
}