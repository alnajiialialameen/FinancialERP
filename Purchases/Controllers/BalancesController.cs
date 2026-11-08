using Microsoft.AspNet.Identity;
using Purchases.MyLogic;
using Purchases.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class BalancesController : Controller
    {
        private Entities db = new Entities();
        private TreeClass trcls = new TreeClass();
        private SharedClass shared = new SharedClass();
        // GET: Balances
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult OverLimitItems()
        {
            return View();
        }

        public ActionResult LoadOverLimitItemsData()
        {
            var userId = User.Identity.GetUserId();
            var data = shared.GetOverloadBalanceItems(userId);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadData()
        {
            List<object> data = new List<object>();
            var userId = User.Identity.GetUserId();

            int financeCycleId = shared.GetUserCurrentFinancialCycleId(userId);
            //var cycles = db.UserWorkDetails
            //                            .AsNoTracking()
            //                            .Where(x => x.UserId == userId)
            //                            .Select(x => new
            //                            {
            //                                x.FinancialCycleId,
            //                                Year = x.FinancialCycle.Year
            //                            })
            //                            .ToList();

            var balances = db.Balances
                .Where(x => x.FinanceCycleId == financeCycleId)
                .ToList();

            foreach (var b in balances)
            {
                var item = new
                {
                    Id = b.Id,
                    Year = b.FinancialCycle.Year,
                    Name = b.AccountTree.AccName,
                    ActualExchange = b.ActualExchange,
                    RelativeDeviation = b.RelativeDeviation ?? 0,
                    DeviationRatio = b.DeviationRatio,
                    Credint = b.Credint,
                    Retainer = b.Retainer,
                    BalanceId = b.Id,
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
            var userId = User.Identity.GetUserId();
            int financeCycleId = shared.GetUserCurrentFinancialCycleId(userId);

            // 1. Get account trees
            var accountTrees = db.AccountTrees
                .AsNoTracking()
                .Where(g => g.AccParent == type)
                .Select(g => new
                {
                    g.Id,
                    g.AccName,
                    g.AccParent
                })
                .ToList();

            var treeIds = accountTrees.Select(x => x.Id).ToList();

            // 2. Get balances once
            var balances = db.Balances
                .AsNoTracking()
                .Where(b => b.FinanceCycleId == financeCycleId)
                .Select(b => new
                {
                    b.Id,
                    b.AccountTreeId,
                    Credint = b.Credint ?? 0,
                    ActualExchange = b.ActualExchange ?? 0
                })
                .ToList()
                .GroupBy(b => b.AccountTreeId)
                .ToDictionary(g => g.Key, g => g.First());

            // 3. Get account subs with parent names
            var accountSubs = (
                from p in db.AccountSubs.AsNoTracking()
                join tree in db.AccountTrees.AsNoTracking()
                    on p.AccTreeId equals tree.Id
                join parent in db.AccountTrees.AsNoTracking()
                    on tree.AccParent equals parent.Id into parentGroup
                from parent in parentGroup.DefaultIfEmpty()
                where treeIds.Contains(tree.Id)
                select new
                {
                    Id = p.Id,
                    AccTreeId = p.AccTreeId,
                    Name = tree.AccName,
                    AccParent = tree.AccParent,
                    AccParentName = parent != null ? parent.AccName : ""
                }
            ).ToList();

            // 4. Prepare account data in memory
            var accSub = accountSubs.Select(p =>
            {
                var balance = p.AccTreeId.HasValue &&
                              balances.ContainsKey(p.AccTreeId.Value)
                    ? balances[p.AccTreeId.Value]
                    : null;

                return new
                {
                    p.Id,
                    p.AccTreeId,
                    p.Name,
                    p.AccParent,
                    p.AccParentName,
                    Credint = balance?.Credint ?? 0,
                    ActualExchange = balance?.ActualExchange ?? 0
                };
            }).ToList();

            // 5. Group by parent account
            if(type == 3)
                return Json(accSub, JsonRequestBehavior.AllowGet);

            var newAccSub = accSub
                .GroupBy(g => g.AccParent)
                .Select(q => new
                {
                    Id = q.First().Id,
                    AccParent = q.Key,
                    AccParentName = q.First().AccParentName,
                    Name = q.First().AccParentName,

                    Credint = q.Sum(d => d.Credint),
                    ActualExchange = q.Sum(d => d.ActualExchange),

                    Details = q.Select(d => new
                    {
                        d.Id,
                        d.AccTreeId,
                        d.AccParent,
                        d.AccParentName,
                        d.Name,
                        d.Credint,
                        d.ActualExchange
                    }).ToList()
                })
                .OrderBy(x => x.Credint)
                .ToList();

            return Json(newAccSub, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateOUpdate(List<Balance> model)
        {
            List<Balance> createList = new List<Balance>();
            List<Balance> updateList = new List<Balance>();

            if (model.Count > 0)
            {
                var userId = User.Identity.GetUserId();
                int financeCycleId = shared.GetUserCurrentFinancialCycleId(userId);

                var userid = User.Identity.GetUserId();

                createList = model.Where(d => d.Credint > 0 & !db.Balances.Any(m => m.AccountTreeId == d.AccountTreeId && m.FinanceCycleId == financeCycleId)).ToList();
                updateList = model.Where(d => db.Balances.Any(m => m.AccountTreeId == d.AccountTreeId && m.FinanceCycleId == financeCycleId)).ToList();

                foreach (var item in updateList)
                {
                    item.FinanceCycleId = financeCycleId;
                    Balance obj = db.Balances.Find(item.Id);

                    trcls.updateFinancialCycleCredint(financeCycleId, obj.Credint, item.Credint, userid);

                    obj.Credint = item.Credint;
                    obj.FinanceCycleId = financeCycleId;
                    obj.UpdatedBy = userid;
                    obj.CreationDate = DateTime.Now;

                    db.Entry<Balance>(obj).State = EntityState.Modified;
                    db.SaveChanges();
                }

                trcls.updateFinancialCycleCredint(financeCycleId, 0, createList.Sum(x => x.Credint), userid);

                createList.ForEach(x => x.FinanceCycleId = financeCycleId);
                createList.ForEach(x => x.CreatedBy = userid);
                createList.ForEach(x => x.CreationDate = DateTime.Now);

                db.Balances.AddRange(createList);
                db.SaveChanges();

                return Json(new { Message = "تمت عملية الاضافة بنجاح", Title = "نجاح", Status = "success" });
            }

            return Json(new { Message = "خطأ في عملية الاضافة", Title = "خطأ", Status = "error" });
        }

        // دي تست لي تقرير موقف الموازنه - برسل ليها رقم السنة المالية المحددة وبترجع لي تقرير عن الموقف المالي للموازنة
        public ActionResult BalancePosition(int ParentId)
        {
            var val = trcls.BalancePosition(1, ParentId);

            return Json(val, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult BalancePosition()
        //{
        //    var val = trcls.BalancePositionFi(1);

        //    return Json(val, JsonRequestBehavior.AllowGet);
        //}

        // دي تست لي تقرير موقف الموازنه - برسل ليها رقم السنة المالية المحددة وبترجع لي تقرير عن الموقف المالي للموازنة
        public ActionResult BalancePositionByDate()
        {
            var val = trcls.BalancePositionByDate(1, DateTime.Now);

            return Json(val, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getSubaccountForAddUpdateOld(int type)
        {
            var userId = User.Identity.GetUserId();

            int financeCycleId = shared.GetUserCurrentFinancialCycleId(userId);

            string typeCode = type.ToString();

            // 1. Get account name
            var account = db.AccountTrees
                .FirstOrDefault(g => g.AccCode.StartsWith(typeCode));

            if (account == null)
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            string accName = account.AccName;

            // 2. Get account tree IDs
            var treeIds = db.AccountTrees
                .Where(g => g.AccCode.StartsWith(typeCode))
                .Select(g => g.Id)
                .ToList();

            // 3. Load balances once
            var balances = db.Balances
                .Where(b => b.FinanceCycleId == financeCycleId)
                .ToList()
                .GroupBy(b => b.AccountTreeId)
                .ToDictionary(g => g.Key, g => g.First());

            // 4. Get sub accounts
            var accountSubs = db.AccountSubs
                .Where(f => treeIds.Contains(f.AccTreeId ?? 0))
                .Select(p => new
                {
                    Id = p.Id,
                    AccTreeId = p.AccTreeId,
                    Name = p.AccountTree.AccName
                })
                .ToList();

            // 5. Prepare account data
            var accSub = accountSubs.Select(p =>
            {
                var balance = p.AccTreeId.HasValue &&
                              balances.ContainsKey(p.AccTreeId.Value)
                    ? balances[p.AccTreeId.Value]
                    : null;

                return new
                {
                    p.Id,
                    p.AccTreeId,
                    p.Name,
                    AccParentName = accName,
                    Credint = balance?.Credint ?? 0,
                    ActualExchange = balance?.ActualExchange ?? 0,
                    BalanceId = balance?.Id ?? 0
                };
            })
            .OrderBy(x => x.Credint)
            .ToList();

            // 6. Type 4: Group accounts and include details
            if (type == 4)
            {
                var groupedAccounts = accSub
                    .GroupBy(x => x.AccParentName)
                    .Select(g => new
                    {
                        AccTreeId = g.First().AccTreeId,
                        Name = g.First().Name,
                        AccParentName = g.Key,

                        Credint = g.Sum(x => x.Credint),

                        ActualExchange = g.Sum(x => x.ActualExchange),

                        BalanceId = 0,

                        Details = g.Select(p => new
                        {
                            p.Id,
                            p.AccTreeId,
                            p.Name,
                            p.AccParentName,
                            p.Credint,
                            p.ActualExchange,
                            p.BalanceId
                        }).ToList()
                    })
                    .ToList();

                return Json(groupedAccounts, JsonRequestBehavior.AllowGet);
            }

            return Json(accSub, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(int id, decimal Credint)
        {
            var obj = db.Balances.Find(id);

            if (obj == null)
            {
                return Json(new { Message = "خطأ في عملية التعديل", Title = "خطأ", Status = "error" });
            }

            obj.Credint = Credint;

            db.Entry(obj).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success" });
        }

        public ActionResult GetAccountTree(int type)
        {
            var userId = User.Identity.GetUserId();
            int financeCycleId = shared.GetUserCurrentFinancialCycleId(userId);

            // جلب الحسابات
            var accounts = db.AccountTrees
                .Where(a => a.IsActive != false)
                .AsNoTracking()
                .Select(a => new
                {
                    a.Id,
                    a.AccParent,
                    a.AccCode,
                    a.AccName
                })
                .ToList();

            // جلب أرصدة الحسابات للدورة المالية الحالية
            var balances = db.Balances
                .AsNoTracking()
                .Where(b => b.FinanceCycleId == financeCycleId)
                .ToList()
                .GroupBy(b => b.AccountTreeId)
                .ToDictionary(g => g.Key, g => g.First());

            // تجميع الحسابات حسب الحساب الأب
            var children = accounts
                .Where(a => a.AccParent.HasValue)
                .GroupBy(a => a.AccParent.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            var visited = new HashSet<int> { type };

            Func<int, List<object>> getDetails = null;

            getDetails = currentParentId =>
            {
                var details = new List<object>();

                if (!children.ContainsKey(currentParentId))
                    return details;

                foreach (var account in children[currentParentId])
                {
                    if (!visited.Add(account.Id))
                        continue;

                    // البحث عن رصيد الحساب
                    var balance = balances.ContainsKey(account.Id)
                        ? balances[account.Id]
                        : null;

                    details.Add(new
                    {
                        Id = account.Id,
                        Name = account.AccName,
                        AccCode = account.AccCode,
                        AccParent = account.AccParent,

                        Credint = balance != null ? balance.Credint ?? 0 : 0,
                        ActualExchange = balance != null ? balance.ActualExchange ?? 0 : 0,
                        BalanceId = balance != null ? balance.Id : 0,

                        Details = getDetails(account.Id)
                    });
                }

                return details;
            };

            // إرجاع الأبناء فقط
            var result = getDetails(type);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}