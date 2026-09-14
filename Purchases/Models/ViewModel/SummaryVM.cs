using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Purchases.Models.ViewModel
{

    public class SummaryVM
    {
        public SummaryVM(int id, string name, decimal? total)
        {
            Id = id;
            accTreeName = name;
            this.total = total;
        }
        public int Id { get; set; }
        public string accTreeName { get; set; }
        public decimal? total { get; set; }
    }
}
