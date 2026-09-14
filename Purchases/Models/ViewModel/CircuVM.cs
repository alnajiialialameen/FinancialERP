using System;

namespace Purchases.Models.ViewModel
{
    public class CircuVM : BaseVM
    {
        public DateTime CircusDate { get; set; }
        public string FNo { get; set; }        
        public string Subject { get; set; }
        public int OrderId { get; set; }
        public int DepartmentSenderId { get; set; }
        public int DepartmentRecipientId { get; set; }
        public string DepartmentSenderName { get; set; }
        public string DepartmentRecipientName { get; set; }
        public int ReceiverId { get; set; }  
        public string ReceiverEmployeeName { get; set; }
        public string Signatur { get; set; }
    }
}