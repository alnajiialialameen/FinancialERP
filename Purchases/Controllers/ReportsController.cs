using Purchases.Models;
using Purchases.Models.ViewModal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class ReportsController : Controller
    {
        Entities db = new Entities();
   
        public ActionResult printFiveTaxReport()
        {
            List<TransactionViewMode> data = new List<TransactionViewMode>();
            if (db.TransactionDetails.Any(x => x.AccTreeId == 229))
            {
                foreach (var item in db.TransactionDetails.Where(x => x.AccTreeId == 229))
                {
                    data.Add(new TransactionViewMode()
                    {
                        id = item.Id,
                        note = item.Note,
                        amount = item.Transaction.Amount,
                        tax = item.Debit + item.Credit,
                        //total = item.Transaction.Amount + (item.Debit + item.Credit)
                    });
                }
            }

            ViewBag.sumOfTax = data.Sum(x => x.tax);
            return View(data);
        }

        public ActionResult TaxReport()
        {
            return View();
        }

        public ActionResult printLedgerReport()
        {
            return View();
        }

        public ActionResult printDataReport()
        {
            return View();
        }

        public ActionResult printBudgetReport()
        {
            return View();
        }

        // GET: Reports
        public ActionResult printTaxReport()
        {
            List<TransactionViewMode> data = new List<TransactionViewMode>();
            if (db.Transactions.Any(x => x.HasTax == true))
            {
                foreach (var item in db.Transactions.Where(x => x.HasTax == true))
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