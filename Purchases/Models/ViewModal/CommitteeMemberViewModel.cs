using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class CommitteeMemberViewModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }        
        public int EmployeeId { get; set; }
        public int CommitteeJobId { get; set; }
        public int CommitteFormationId { get; set; }        
        public int CompanyRegisterationId { get; set; }
        public DateTime CreationDate { get; set; }        
    }
}