using Antlr.Runtime.Tree;
using Microsoft.AspNet.Identity;
using Purchases.Models;
using Purchases.Models.ViewModel;
using Purchases.MyLogic;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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

        public ActionResult LoadDataOriginal(int? bankId)
        {
            try
            {
                SharedClass shClass = new SharedClass();
                var userId = User.Identity.GetUserId();
                int financialCycleId = shClass.GetUserCurrentFinancialCycleId(userId);
                string FinancialCyclYear = shClass.GetUserCurrentFinancialCycleYear(userId);
                int FinancialCyclYeare = Convert.ToInt32(FinancialCyclYear);

                List<int> TransactionIdsList = new List<int>();

                if (bankId != null)
                {
                    TransactionIdsList = db.Transactions
                        .Where(t => t.FinancialCycleId == financialCycleId - 1 &&
                                    t.TransactionDetails.Any(td => td.AccTreeId == bankId))
                        .Select(q => q.Id)
                        .ToList();
                }
                else
                {
                    TransactionIdsList = db.Transactions
                        .Where(t => t.FinancialCycleId == financialCycleId - 1)
                        .Select(q => q.Id)
                        .ToList();
                }

                var data = db.TransactionDetails
                        .Where(q => TransactionIdsList.Contains(q.TransactionId))
                                .Include(q => q.Transaction)
                                .Include(q => q.Transaction.FinancialCycle)
                                .Include(q => q.AccountTree)
                                .Include(q => q.Balance)
                                .ToList();

                int? financialCycleId1 = financialCycleId - 1;
                var data1 = data.GroupBy(q => new
                {
                    financialCycleId1,
                    q.AccTreeId,
                    q.AccountTree.AccName,
                    q.Transaction.FinancialCycle.Year
                })
                            .Select(g => new OpeningBalanceVM()
                            {
                                Debit = g.Sum(x => x.Debit),
                                Credit = g.Sum(x => x.Credit),
                                Id = g.Key.AccTreeId,
                                AccTreeName = g.Key.AccName,
                                AccTreeId = g.Key.AccTreeId,
                                FinancialCycle = g.Key.Year,
                                FinancialCycleId = g.Key.financialCycleId1,
                            })
                            .OrderBy(q => q.Debit)
                            .ToList();

                TreeClass tree = new TreeClass();
                data1 = data1
                    .Where(item =>
                    {
                        var rootParentId = tree.getRootParentId(item.AccTreeId);
                        return rootParentId != 3 && rootParentId != 4;
                    })
                    .ToList();

                data1.ForEach(q => q.Diff = q.Debit >= q.Credit ? q.Debit - q.Credit : q.Credit - q.Debit);

                return Json(data1, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطا في جلب البيانات", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoadData11(int? bankId)
        {
            try
            {
                SharedClass shClass = new SharedClass();
                TreeClass tree = new TreeClass();
                var userId = User.Identity.GetUserId();
                int financialCycleId = shClass.GetUserCurrentFinancialCycleId(userId);
                string FinancialCyclYear = shClass.GetUserCurrentFinancialCycleYear(userId);
                int FinancialCyclYeare = Convert.ToInt32(FinancialCyclYear);

                List<int> TransactionIdsList = new List<int>();

                var AccountTreesList = db.AccountTrees
                    .AsEnumerable()
                    .Where(item =>
                    {
                        var rootParentId = tree.getRootParentId(item.AccParent);
                        return rootParentId != 3 && rootParentId != 4;
                    }).ToList();

                if (bankId != null)
                {
                    TransactionIdsList = db.Transactions
                        .Where(t => t.FinancialCycleId == financialCycleId - 1 &&
                                    t.TransactionDetails.Any(td => td.AccTreeId == bankId))
                        .Select(q => q.Id)
                        .ToList();
                }
                else
                {
                    TransactionIdsList = db.Transactions
                        .Where(t => t.FinancialCycleId == financialCycleId - 1)
                        .Select(q => q.Id)
                        .ToList();
                }

                var data = db.TransactionDetails
                        .Where(q => TransactionIdsList.Contains(q.TransactionId) && AccountTreesList.Any(t => t.Id == q.AccTreeId))
                                .Include(q => q.Transaction)
                                .Include(q => q.Transaction.FinancialCycle)
                                .Include(q => q.AccountTree)
                                .Include(q => q.Balance)
                                .ToList();

                int? financialCycleId1 = financialCycleId - 1;
                var data1 = data
                          .GroupBy(q => new { q.AccountTree.AccParent, q.AccTreeId })
                          .Select(g => new OpeningBalanceVM
                          {
                              AccPrentId = g.Key.AccParent, // هذا هو الـ Parent ID
                              //AccPrentName = AccountTreesList.FirstOrDefault(a => a.Id == g.Key.AccParent)?.AccName,
                              Debit = g.Sum(x => x.Debit),
                              Credit = g.Sum(x => x.Credit),
                              AccTreeId = g.Key.AccTreeId,
                              // هنا بنعمل تجميع الأطفال
                              Children = g.Select(x => new OpeningBalanceVM
                              {
                                  Id = x.AccTreeId,
                                  AccTreeId = x.AccTreeId,
                                  AccTreeName = x.AccountTree.AccName,
                                  Debit = x.Debit,
                                  Credit = x.Credit,
                                  FinancialCycle = x.Transaction.FinancialCycle.Year,
                                  FinancialCycleId = financialCycleId1
                              }).Distinct().ToList()
                          }).Distinct()
                          .OrderBy(q => q.AccPrentId)
                          .ToList();

                // ✅ حساب الفرق
                //data1.ForEach(q => q.Diff = Math.Abs(q.Debit - q.Credit));

                data1.ForEach(parent =>
                {
                    parent.AccPrentName = AccountTreesList
                        .FirstOrDefault(at => at.Id == parent.AccPrentId)?
                        .AccName;
                });

                data1.ForEach(q => q.Diff = q.Debit >= q.Credit ? q.Debit - q.Credit : q.Credit - q.Debit);
                data1.OrderBy(q => q.AccPrentId).ToList();
                data1 = data1
                    .Where(item =>
                    {
                        var rootParentId = tree.getRootParentId(item.AccTreeId);
                        return rootParentId != 3 && rootParentId != 4;
                    })
                    .ToList();

                return Json(data1, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطا في جلب البيانات", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoadData22(int? bankId)
        {
            try
            {
                SharedClass shClass = new SharedClass();
                TreeClass tree = new TreeClass();
                var userId = User.Identity.GetUserId();
                int financialCycleId = shClass.GetUserCurrentFinancialCycleId(userId);
                string FinancialCyclYear = shClass.GetUserCurrentFinancialCycleYear(userId);
                int FinancialCyclYeare = Convert.ToInt32(FinancialCyclYear);

                List<TransactionDetail> TransactionDetailsList = new List<TransactionDetail>();
                List<int> SelectedAccountTreeIdsList = new List<int>();
                List<int> AccountTreesIdsForPrincipleAccounts = new List<int>(); // الاصول - Principle Accounts
                List<int> AccountTreesIdsListForLiabilitiesAccounts = new List<int>(); // الالتزامات - الخصوم - Liabilities

                AccountTreesIdsForPrincipleAccounts = tree.getAllItemsByParentId(1); // الاصول - Principle Accounts
                AccountTreesIdsListForLiabilitiesAccounts = tree.getAllItemsByParentId(2); // الالتزامات - الخصوم - Liabilities
                AccountTreesIdsForPrincipleAccounts.AddRange(AccountTreesIdsListForLiabilitiesAccounts); // Concatenate both lists
                List<int> TotalAccountTreesIdsList = AccountTreesIdsForPrincipleAccounts;
                var AccountTreesList = db.AccountTrees.ToList();
                var AccountTreeIdsList = db.AccountSubs.Select(q => q.AccTreeId).ToList(); //accounts that have subs

                // accouts do not in subs (main accounts)
                TotalAccountTreesIdsList = TotalAccountTreesIdsList.Where(id => !AccountTreeIdsList.Contains(id)).Distinct().OrderByDescending(q => q).ToList();

                var AccountTreesForLevel2 = db.AccountTrees.Where(q => q.TheLevel == 2).Select(q => q.Id).ToList();
                SelectedAccountTreeIdsList = TotalAccountTreesIdsList.Where(id => AccountTreesForLevel2.Contains(id)).ToList();

                TransactionDetailsList = db.TransactionDetails
                    .Where(t => t.Transaction.FinancialCycleId == financialCycleId - 1 && t.AccountTree.TheLevel > 2)
                    .ToList();

                if (bankId != null)
                {
                    TransactionDetailsList = TransactionDetailsList.Where(q => q.AccTreeId == bankId).ToList();
                }

                var SelectedTransactions = TransactionDetailsList.Select(q => q.TransactionId).ToList();

                var NewTransactionDetailsList = db.TransactionDetails
                   .Where(t => SelectedTransactions.Contains(t.TransactionId))
                   .Include(q => q.Transaction)
                   .Include(q => q.Transaction.FinancialCycle)
                   .Include(q => q.AccountTree)
                   .Include(q => q.Balance)
                   //.Select(q => q.Id)
                   .ToList();

                var data1 = NewTransactionDetailsList
                          .GroupBy(q => q.AccountTree.AccParent)
                          .Select(g => new OpeningBalanceVM
                          {
                              AccPrentId = g.Key, // هذا هو الـ Parent ID
                              AccPrentName = AccountTreesList.FirstOrDefault(a => a.Id == g.Key)?.AccName,
                              Debit = g.Sum(x => x.Debit),
                              Credit = g.Sum(x => x.Credit),
                              // هنا بنعمل تجميع الأطفال
                              Children = g.Select(x => new OpeningBalanceVM
                              {
                                  Id = x.AccTreeId,
                                  AccTreeId = x.AccTreeId,
                                  AccTreeName = x.AccountTree.AccName,
                                  Debit = x.Debit,
                                  Credit = x.Credit,
                                  FinancialCycle = x.Transaction.FinancialCycle.Year,
                                  FinancialCycleId = financialCycleId - 1
                              }).Distinct().ToList()
                          }).Distinct()
                          .OrderBy(q => q.AccPrentId)
                          .ToList();

                //  ✅ حساب الفرق
                data1.ForEach(q => q.Diff = q.Debit >= q.Credit ? q.Debit - q.Credit : q.Credit - q.Debit);
                data1.OrderBy(q => q.AccPrentId).ToList();

                return Json(data1, JsonRequestBehavior.AllowGet);
                //return Json(SelectedAccountTreeIdsList, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطا في جلب البيانات", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult LoadData1()
        {
            try
            {
                SharedClass shClass = new SharedClass();
                var userId = User.Identity.GetUserId();
                int financialCycleId = shClass.GetUserCurrentFinancialCycleId(userId);

                List<OpeningBalanceVM> data = new List<OpeningBalanceVM>();

                if (db.OpeningBalances.Any(x => x.FinancialCycleId == financialCycleId))
                {
                    var dlist = db.OpeningBalances.Where(x => x.FinancialCycleId == financialCycleId).Select(f => f.Id).ToList();
                    foreach (var item in db.OpeningBalanceDetails.Where(x => dlist.Any(c => c == x.OpeningBalanceId)))
                    {
                        OpeningBalanceVM obj = new OpeningBalanceVM();

                        obj.Id = item.Id;
                        obj.OpeningBalanceId = item.OpeningBalanceId;
                        obj.AccTreeId = Convert.ToInt32(item.AccTreeId);
                        obj.AccTreeName = item.AccountTree.AccName;
                        obj.Credit = item.Credit;
                        obj.Debit = item.Debit;
                        obj.Note = item.Note;
                        obj.FinancialCycle = item.OpeningBalance.FinancialCycle.Year;

                        data.Add(obj);
                    }

                    return Json(data, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Message = "لا توجد بيانات لعرضها", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطا في جلب البيانات", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Create(OpeningBalanceVM data)
        {
            if (data == null)
            {
                return Json(new { Message = "لم تتم العملية بنجاح", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }

            var userId = User.Identity.GetUserId();
            var currentCycle = db.FinancialCycles.FirstOrDefault(q => q.CurrentYear == true);
            if (currentCycle == null)
            {
                return Json(new { Message = "لا يوجد سنة مالية حالية", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }

            int currentYearId = currentCycle.Id;

            // الحصول على OpeningBalance الحالي أو إنشاء جديد
            var ob = db.OpeningBalances
                       .Include(q => q.OpeningBalanceDetails)
                       .FirstOrDefault(q => q.FinancialCycleId == currentYearId);

            if (ob == null)
            {
                ob = new OpeningBalance
                {
                    FinancialCycleId = currentYearId,
                    CreatedBy = userId,
                    CreationDate = DateTime.Now,
                    OpeningBalanceDetails = new List<OpeningBalanceDetail>()
                };
                db.OpeningBalances.Add(ob);
            }

            // تحديث أو إضافة OpeningBalanceDetail
            var obd = ob.OpeningBalanceDetails.FirstOrDefault(q => q.AccTreeId == data.AccTreeId);
            if (obd == null)
            {
                obd = new OpeningBalanceDetail
                {
                    AccTreeId = data.AccTreeId,
                    Note = "رصيد أول المدة",
                    CreatedBy = userId,
                    CreationDate = DateTime.Now
                };
                ob.OpeningBalanceDetails.Add(obd);
            }

            // تحديث القيم
            obd.Debit = data.Debit;
            obd.Credit = data.Credit;
            obd.OpeningBalanceId = ob.Id;
            obd.UpdatedBy = userId;
            obd.UpdatingDate = DateTime.Now;

            // تحديث OpeningBalance الإجمالي
            ob.Debit = ob.OpeningBalanceDetails.Sum(q => q.Debit) ?? 0;
            ob.Credit = ob.OpeningBalanceDetails.Sum(q => q.Credit) ?? 0;
            ob.Difference = Math.Abs((ob.Credit ?? 0) - (ob.Debit ?? 0));
            ob.UpdatedBy = userId;
            ob.UpdatingDate = DateTime.Now;

            db.SaveChanges();

            return Json(new { Message = "تمت عملية الإضافة بنجاح", Title = "عملية الإضافة", Status = "success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadData(int? bankId)
        {
            try
            {
                var shClass = new SharedClass();
                var tree = new TreeClass();
                var userId = User.Identity.GetUserId();

                int financialCycleId = shClass.GetUserCurrentFinancialCycleId(userId);
                int previousFinancialCycleId = financialCycleId - 1;
                string financialCycleYear = db.FinancialCycles.Find(previousFinancialCycleId).Year;

                var AccountTreesIdsForPrincipleAccounts = tree.getAllItemsByParentId(1); // الاصول - Principle Accounts
                var AccountTreesIdsListForLiabilitiesAccounts = tree.getAllItemsByParentId(2); // الالتزامات - الخصوم - Liabilities

                // 🔹 تحميل الشجرة وكل الحسابات
                var accountTrees = db.AccountTrees.Where(q => AccountTreesIdsForPrincipleAccounts.Contains(q.Id)
                                                                                     || AccountTreesIdsListForLiabilitiesAccounts.Contains(q.Id)).ToList();
                var transactionDetails = db.TransactionDetails
                    .Include(t => t.Transaction)
                    .Include(t => t.AccountTree)
                    .Where(t => t.Transaction.FinancialCycleId == previousFinancialCycleId)
                    .ToList();

                if (bankId.HasValue)
                {
                    transactionDetails = transactionDetails
                        .Where(t => t.AccTreeId == bankId.Value)
                        .ToList();
                }

                // 🔹 بناء قاموس لتجميع الأرصدة لكل حساب
                var balanceByAccount = transactionDetails
                    .GroupBy(t => t.AccTreeId)
                    .ToDictionary(
                        g => g.Key,
                        g => new
                        {
                            Debit = g.Sum(x => x.Debit),
                            Credit = g.Sum(x => x.Credit)
                        });

                // 🔹 دالة تكرارية لحساب المجاميع
                Func<int, AccountBalance> GetTotalBalanceRecursive = null;

                GetTotalBalanceRecursive = (accountId) =>
                {
                    // الأبناء المباشرين
                    var children = accountTrees.Where(a => a.AccParent == accountId).Select(a => a.Id).ToList();

                    decimal? totalDebit = 0;
                    decimal? totalCredit = 0;

                    // أضف رصيد الحساب نفسه (إن وجد)
                    if (balanceByAccount.ContainsKey(accountId))
                    {
                        totalDebit += balanceByAccount[accountId].Debit;
                        totalCredit += balanceByAccount[accountId].Credit;
                    }

                    // أضف مجاميع الأبناء (بغض النظر عن المستوى)
                    foreach (var childId in children)
                    {
                        var childBalance = GetTotalBalanceRecursive(childId);
                        totalDebit += childBalance.Debit;
                        totalCredit += childBalance.Credit;
                    }

                    return new AccountBalance
                    {
                        Debit = totalDebit,
                        Credit = totalCredit
                    };
                };

                // 🔹 استهداف حسابات المستوى 2 فقط
                var level2Accounts = accountTrees.Where(a => a.TheLevel == 1 || a.TheLevel == 2).ToList();

                var result = new List<OpeningBalanceVM>();

                foreach (var acc in level2Accounts)
                {
                    var totals = GetTotalBalanceRecursive(acc.Id);

                    // تجميع الأبناء (المستوى 3 و 4)
                    var children = accountTrees
                        .Where(a => a.AccParent == acc.Id)
                        .Select(a =>
                        {
                            var childTotals = GetTotalBalanceRecursive(a.Id);
                            return new OpeningBalanceVM
                            {
                                Id = a.Id,
                                AccPrentId = acc.Id,
                                AccTreeId = a.Id,
                                AccTreeName = a.AccName,
                                Debit = childTotals.Debit,
                                Credit = childTotals.Credit,
                                Diff = Math.Abs((childTotals.Debit ?? 0) - (childTotals.Credit ?? 0)),
                            };
                        })
                        .Where(a => a.Diff > 0)
                        .ToList();

                    result.Add(new OpeningBalanceVM
                    {
                        AccPrentId = acc.Id,
                        AccPrentName = acc.AccName,
                        Debit = totals.Debit,
                        Credit = totals.Credit,
                        FinancialCycle = financialCycleYear,
                        FinancialCycleId = previousFinancialCycleId,
                        Diff = Math.Abs((totals.Debit ?? 0) - (totals.Credit ?? 0)),
                        Children = children
                    });
                }

                result = result.Where(q => q.Children.Any(c => c.Diff != 0)).ToList();
                return Json(result.OrderBy(r => r.AccPrentId).ToList(), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Message = "حدث خطأ أثناء جلب البيانات",
                    Title = "خطأ",
                    Status = "error",
                    Exception = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

    }

    public class AccountBalance
    {
        public decimal? Debit { get; set; }
        public decimal? Credit { get; set; }
    }

}