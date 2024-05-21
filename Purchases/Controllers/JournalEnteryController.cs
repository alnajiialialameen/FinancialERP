using Purchases.Models;
using Purchases.Models.ViewModal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class JournalEnteryController : Controller
    {
        private Entities db = new Entities();
        // GET: JournalEntery

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadData()
        {
            var data = db.JournalEnteries.ToList().Select(p => new
            {
                Id = p.Id,
                TotalCridetAmount = p.JournalEnteryDetails.Where(x=>x.IsCredit == true).Sum(x=>x.Amount),
                TotalDebitAmount = p.JournalEnteryDetails.Where(x=>x.IsCredit != true).Sum(x=>x.Amount),
                Statement = p.Statement,
                JournalDate = p.JournalDate.Value.Day + "/" + p.JournalDate.Value.Month + "/" + p.JournalDate.Value.Year
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAccTrees(string q)
        {
            var data = db.AccountTrees.Where(x => db.AccountSubs.Any(s => s.AccTreeId == x.Id))
                        .Select(p => new { id = p.Id, text = p.AccName }).Where(f => f.text.Contains(q)).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(List<JournalEnteryViewMode> data)
        {
            try
            {
                JournalEntery jou = new JournalEntery();
                jou.Statement = data.FirstOrDefault().statement;
                jou.JournalDate = data.FirstOrDefault().journalDate;

                db.JournalEnteries.Add(jou);

                List<JournalEnteryDetail> jouDetList = new List<JournalEnteryDetail>();
                foreach (var item in data)
                {
                    JournalEnteryDetail jouDet = new JournalEnteryDetail();

                    jouDet.AccTrreId = item.accTrreId;
                    jouDet.JournalEnteryId = jou.Id;
                    jouDet.Amount = item.amount;
                    jouDet.IsCredit = item.isCredit;

                    jouDetList.Add(jouDet);
                }

                db.JournalEnteryDetails.AddRange(jouDetList);
                db.SaveChanges();

                return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
            }
            catch(Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
            return View();
        }
       
        public ActionResult printData()
        {
            List<JournalEnteryViewMode> data = new List<JournalEnteryViewMode>();
            foreach (var item in db.JournalEnteryDetails.ToList())
            {
                JournalEnteryViewMode obj = new JournalEnteryViewMode();

                obj.accName = item.AccountTree.AccName;
                obj.isCredit = item.IsCredit;
                obj.amount = item.Amount;
                obj.journalEnteryId = item.JournalEntery.Id;
                obj.statement = item.JournalEntery.Statement;

                data.Add(obj);
            }
            ViewBag.SelectedDate = DateTime.Today.ToShortDateString();

            return View(data);
        }
    }
}