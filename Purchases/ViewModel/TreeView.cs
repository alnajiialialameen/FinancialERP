using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.ViewModel
{
    public class TreeModel
    {
        public int Id { get; set; }
        public decimal AccCode { get; set; }
        public string AccName { get; set; }
        public decimal AccParent { get; set; }
        public int AccParentId { get; set; }
    }
    
    public class TreeViewModelItem
    {
        public int Id { get; set; }
        public int? AccParent { get; set; }
        public string AccName { get; set; }

        public string AccParentName { get; set; }
        public bool IsActive { get; set; }
        
        public string GetParentNameList { get; set; }
    }


}