using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class CircuViewModel
    {
        public int Id { get; set; }
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
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime UpdatingDate { get; set; }
        public string UpdatedBy { get; set; }

    }
}