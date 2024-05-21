using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class CompanyDocumentViewModel
    {
        public int Id { get; set; }
        public Nullable<int> CompetingCompanyId { get; set; }
        public string ImagePath { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
    }
}