using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Purchases.Models.ViewModal
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Grade { get; set; }
        public string Department { get; set; }
        public string EmploymetType { get; set; }
        public int EmploymetTypeId { get; set; }
        public string CommitteJob { get; set; }
    }
}