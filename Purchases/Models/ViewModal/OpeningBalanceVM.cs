using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class OpeningBalanceVM
    {
        public int AccTreeId { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public string Note { get; set; }
    }
}