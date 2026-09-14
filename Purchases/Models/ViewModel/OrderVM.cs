using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class OrderVM
    {
        public int Id { get; set; }
        public int[] OrderDetailId { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int SupplierId { get; set; }
        public int CurrencyType { get; set; }
        public decimal VatType { get; set; }

        //[DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime OrderDate { get; set; }
        public string Description { get; set; }
        public decimal SuggestPrice { get; set; }
        public int[] RequierCount { get; set; }
        public int[] ItemId { get; set; }
        public int[] ItemDetailId { get; set; }
        public decimal[] ItemPrice { get; set; }

    }
}