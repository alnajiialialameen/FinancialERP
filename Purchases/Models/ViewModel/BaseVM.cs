using System;

namespace Purchases.Models.ViewModel
{
    public class BaseVM
    {
        public int Id { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatingDate { get; set; }
    }
}