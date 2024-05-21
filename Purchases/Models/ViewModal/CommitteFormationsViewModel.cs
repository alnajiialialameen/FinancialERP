using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class CommitteFormationsViewModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string ResolutionName { get; set; }
        public int ResolutionNo { get; set; }
        public DateTime ResolutionDate { get; set; }
        public string Subject { get; set; }
    }
}