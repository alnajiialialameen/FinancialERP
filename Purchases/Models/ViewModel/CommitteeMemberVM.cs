namespace Purchases.Models.ViewModel
{
    public class CommitteeMemberVM : BaseVM
    {
        public int OrderId { get; set; }        
        public int EmployeeId { get; set; }
        public int CommitteeJobId { get; set; }
        public int CommitteFormationId { get; set; }        
        public int CompanyRegisterationId { get; set; }  
    }
}