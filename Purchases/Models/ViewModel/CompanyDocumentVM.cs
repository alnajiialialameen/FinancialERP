using System;

namespace Purchases.Models.ViewModel
{
    public class CompanyDocumentVM : BaseVM
    {
        public Nullable<int> CompetingCompanyId { get; set; }
        public string ImagePath { get; set; }
        public string Title { get; set; }
        public string Note { get; set; }
    }
}