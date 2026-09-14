using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModel
{
    public class OpeningBalanceVM
    {
        public int Id { get; set; }
        public int? OpeningBalanceId { get; set; }
        public int AccTreeId { get; set; }
        public int? AccPrentId { get; set; }
        public string AccPrentName { get; set; }
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
        public decimal? Diff { get; set; }
        public string Note { get; set; }
        public string AccTreeName { get; set; }
        public string FinancialCycle { get; set; }
        public int? FinancialCycleId { get; set; }
        public List<OpeningBalanceVM> Children { get; set; }
    }
}