using Microsoft.AspNet.Identity;
using Purchases.Class;
using Purchases.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class BalancesController : Controller
    {
        private Entities db = new Entities();
        private TreeClass trcls = new TreeClass();
        // GET: Balances
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadData()
        {
            List<object> data = new List<object>();
            foreach (var b in db.Balances.ToList())
            {
                var item = new
                {
                    Id = b.Id,
                    Year = b.FinancialCycle.Year,
                    Name = b.AccountTree.AccName,
                    ActualExchange = b.ActualExchange,
                    RelativeDeviation = b.RelativeDeviation,
                    DeviationRatio = b.DeviationRatio,
                    Credint = b.Credint,
                    Retainer = b.Retainer,
                    BalanceId = b.BalanceId,
                    rootParentId = trcls.getRootParentId(b.AccountTreeId),
                    Count = db.Balances.Where(h => h.BalanceId == b.Id).Count()
                };

                data.Add(item);
            }

            //var data = db.Balances.Select(b => new
            //{
            //    Id = b.Id,
            //    Year = b.FinancialCycle.Year,
            //    Name = b.AccountTree.AccName,
            //    ActualExchange = b.ActualExchange,
            //    RelativeDeviation = b.RelativeDeviation,
            //    DeviationRatio = b.DeviationRatio,
            //    Credint = b.Credint,
            //    accParentId = 1,
            //    accTreeId = b.AccountTreeId,
            //    Retainer = b.Retainer,
            //    BalanceId = b.BalanceId,
            //   Count = db.Balances.Where(h => h.BalanceId == b.Id).Count()
            //}).Where(p=>p.BalanceId==null);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getSubaccountForAddUpdate(int type)
        {
            string AccName = db.AccountTrees.Where(g => g.AccCode.Substring(0, 1) == type.ToString()).FirstOrDefault().AccName;
            List<int> treeid = db.AccountTrees.Where(g => g.AccCode.Substring(0, 1) == type.ToString()).Select(f => f.Id).ToList();
            var accSub = db.AccountSubs.Where(f => treeid.Contains(f.AccTreeId ?? 0)).Select(p => new
            {
                Id = p.AccTreeId,
                Name = p.AccountTree.AccName,
                AccParentName = AccName,
                Credint = db.Balances.FirstOrDefault(x => x.AccountTreeId == p.AccTreeId).Credint ?? 0,
                ActualExchange = db.Balances.FirstOrDefault(x => x.AccountTreeId == p.AccTreeId).ActualExchange ?? 0,
                BalanceId = db.Balances.Any(x => x.AccountTreeId == p.AccTreeId) ? db.Balances.FirstOrDefault(x => x.AccountTreeId == p.AccTreeId).Id : 0
            }).OrderBy(h => h.Credint);
            return Json(accSub, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult CreateOUpdate(List<Balance> model)
        {
            List<Balance> createList = new List<Balance>();
            List<Balance> updateList = new List<Balance>();

            if (model.Count > 0)
            {
                var userid = User.Identity.GetUserId();

                createList = model.Where(d => d.Credint > 0 & !db.Balances.Any(m => m.AccountTreeId == d.AccountTreeId)).ToList();
                updateList = model.Where(d => db.Balances.Any(m => m.AccountTreeId == d.AccountTreeId)).ToList();

                foreach (var item in updateList)
                {
                    item.FinanceCycleId = 1;
                    Balance obj = db.Balances.Find(item.Id);

                    trcls.updateFinancialCycleCredint(1, obj.Credint, item.Credint, userid);

                    obj.Credint = item.Credint;
                    obj.UpdatedBy = userid;
                    obj.CreationDate = DateTime.Now;

                    db.Entry<Balance>(obj).State = EntityState.Modified;
                }

                trcls.updateFinancialCycleCredint(1, 0, createList.Sum(x=>x.Credint), userid);
                db.Balances.AddRange(createList);
                db.SaveChanges();

                return Json(new { Message = "تمت عملية الاضافة بنجاح", Title = "نجاح", Status = "success" });
            }

            return Json(new { Message = "خطأ في عملية الاضافة", Title = "خطأ", Status = "error" });
        }
     
        public ActionResult test()
        {
            var val = trcls.test(1);

            return Json(val, JsonRequestBehavior.AllowGet);
        }
    }
}