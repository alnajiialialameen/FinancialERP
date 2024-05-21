using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class JournalEnteryViewMode
    {
        public int journalEnteryId { get; set; }
        public string statement { get; set; }
        public DateTime journalDate { get; set; }


        public int id { get; set; }// this is JournalEnteryDetail Id
        public int accTrreId { get; set; }
        public string accName { get; set; }
        public Nullable<decimal> amount { get; set; }
        public Nullable<bool> isCredit { get; set; }
    }
}