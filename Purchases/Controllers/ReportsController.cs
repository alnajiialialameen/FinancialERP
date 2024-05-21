using Purchases.Models;
using Purchases.Models.ViewModal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class ReportsController : Controller
    {
        Entities db = new Entities();
        // GET: Reports
        public ActionResult printTaxReport()
        {
            List<TransactionViewMode> data = new List<TransactionViewMode>();
            if(db.Transactions.Any(x=>x.HasTax == true))
            {
                foreach(var item in db.Transactions.Where(x=>x.HasTax == true))
                {
                    data.Add(new TransactionViewMode()
                    {
                        id = item.Id,
                        note = item.Note,
                        amount = item.Amount,
                        tax = item.Amount * (decimal)0.01,
                        total = item.Amount - (item.Amount * (decimal)0.01)
                    });
                }
            }

            ViewBag.sumOfTax = data.Sum(x => x.tax);
            return View(data);
        }
    }
}