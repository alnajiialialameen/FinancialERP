using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Purchases.Models;
using Purchases.Models.ViewModal;

namespace Purchases.Controllers
{
    public class OpeningBalancesController : Controller
    {
        Entities db = new Entities();
        // GET: OpeningBalances
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create(List<OpeningBalanceVM> data)
        {
            OpeningBalance ob = new OpeningBalance();
            OpeningBalanceDetail obd = new OpeningBalanceDetail();
            ob.FinancialCycleId = 1;
            List<OpeningBalanceDetail> obdlist = new List<OpeningBalanceDetail>();
            foreach(var d in data)
            {
                OpeningBalanceDetail o = new OpeningBalanceDetail();
                o.AccTreeId = d.AccTreeId;
                o.Debit = d.Debit;
                o.Credit = d.Credit;
                o.Note = d.Note;
                obdlist.Add(o);
            }

            ob.Debit = obdlist.Sum(d => d.Debit);
            ob.OpeningBalanceDetails = obdlist;
            db.OpeningBalances.Add(ob);

            db.SaveChanges();
            return Json(new { Message = "هذا البند موجود  مسبقا في الشجرة المحاسبية", Title = "عملية الاضافة", Status = "error" });
        }
        

    }
}