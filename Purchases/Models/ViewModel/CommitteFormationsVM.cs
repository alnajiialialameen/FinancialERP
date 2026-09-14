using System;

namespace Purchases.Models.ViewModel
{
    public class CommitteFormationsVM : BaseVM
    {
        public int OrderId { get; set; }
        public string ResolutionName { get; set; }
        public int ResolutionNo { get; set; }
        public int CommitteeTypeId { get; set; }
        public DateTime ResolutionDate { get; set; }
        public string Subject { get; set; }
    }
}