using Microsoft.AspNet.Identity;
using Purchases.MyLogic;
using Purchases.Models;
using Purchases.Models.ViewModal;
using Purchases.Models.ViewModel;
using Purchases.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class AccountTreeController : Controller
    {
        // GET: AccountTree
        Entities db = new Entities();
        TreeClass ClsTree = new TreeClass();

        // GET: AccountTree
        public ActionResult Index()
        {
            var currentYear = db.FinancialCycles.FirstOrDefault(x => x.CurrentYear == true);
            ViewBag.FinancialCycleId = currentYear.Id;
            ViewBag.Year = currentYear.Year;
            ViewBag.UserId = Session["userId"].ToString();

            return View();
        }
       
        // GET: AccountTree
        public ActionResult AccTreeFinancialCycle()
        {
            var currentYear = db.FinancialCycles.FirstOrDefault(x => x.CurrentYear == true);
            ViewBag.FinancialCycleId = currentYear.Id;
            ViewBag.Year = currentYear.Year;

            return View();
        }
        
        // مستخدمة في ال AccTreeFinancialCycle
        public ActionResult LoadData()
        {
            var userId = User.Identity.GetUserId();
            var data = ClsTree.LoadDataForAccTreeFinancialCycle(userId);

            return Json(new { data = data.ToList() }, JsonRequestBehavior.AllowGet);
        }

        // مستخدمة في ال Index
        public ActionResult LoadTree()
        {
            var data = ClsTree.GetAllTreeData();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // مستخدمة في ال Index
        public ActionResult LoadTreeUpdate()
        {
            var data = db.AccountTrees.Select(p => new
            {
                AccName = p.AccName,
                AccCode = p.AccCode,
                Id = p.Id,
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //دالة الحفظ بعد التعديل
        public ActionResult Create(List<AccountTreeVM> dataList, string accParentName)
        {
            var userid = User.Identity.GetUserId();

            string result = ClsTree.AddToTree(dataList, accParentName, userid);

            if (result == "success")
            {
                return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
            }
            else
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }

            //if (dataList.Count() > 0)
            //{
            //    var addAccParentId = db.AccountTrees.FirstOrDefault(x => x.AccName.Trim() == addAccParentName.Trim()).Id;

            //    foreach (var data in dataList)
            //    {
            //        data.AccParent = addAccParentId;
            //        int AccId = Convert.ToInt32(db.SpLastTreeAccountId().FirstOrDefault());
            //        int AccParentCode = ClsTree.GetAccParentCode(Convert.ToInt32(data.AccParent));
            //        string AccCode = db.GetLastAccNumber(AccParentCode).FirstOrDefault().ToString();

            //        //get parent
            //        AccountTree accountTree = db.AccountTrees.Find(data.AccParent);

            //        if (data.AccName != null)
            //        {
            //            if (db.AccountTrees.Any(x => x.AccName == data.AccName))
            //            {
            //                return Json(new { Message = "هذا البند موجود  مسبقا في الشجرة المحاسبية", Title = "عملية الاضافة", Status = "error" });
            //            }


            //            if (!db.AccountTrees.Any(f => f.AccName == data.AccName))
            //            {
            //                AccountTree d = new AccountTree();
            //                d.Id = AccId;
            //                d.AccCode = AccCode;
            //                d.AccName = data.AccName;
            //                d.AccTypeId = data.AccTypeId;
            //                d.AccNatureId = data.AccNatureId;
            //                d.AccFinalId = data.AccFinalId;

            //                if (data.AccTypeId == 2)//حساب فرعي
            //                {
            //                    AccountSub sub = new AccountSub();
            //                    sub.AccCategoryId = data.AccSubCategory;
            //                    sub.AccTreeId = AccId;
            //                    db.AccountSubs.Add(sub);
            //                }

            //                if (accountTree == null)
            //                {
            //                    d.AccParent = 0;
            //                }
            //                else
            //                {
            //                    d.AccParent = accountTree.Id;
            //                }
            //                if (data.AccParent == null || data.AccParent == 0)
            //                {
            //                    d.TheLevel = 0;
            //                }
            //                else
            //                {
            //                    d.TheLevel = Convert.ToInt32(accountTree.TheLevel) + 1;
            //                }

            //                db.AccountTrees.Add(d);
            //                db.SaveChanges();
            //            }

            //        }
            //    }
            //    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
            //    //return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            //}
            //else
            //{
            //    return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            //}
        }

        // جلب جميع البيانات
        public ActionResult GetAllItem()
        {
            IQueryable<AccountTree> CoursesNames = db.AccountTrees;

            var ItemList = new List<TreeViewModelItem>();
            foreach (var item in CoursesNames)
            {
                TreeViewModelItem r = new TreeViewModelItem();
                r.Id = item.Id;
                r.AccName = item?.AccName;
                r.AccParentName = ClsTree.GetParentName(item.AccParent);
                r.GetParentNameList = ClsTree.GetParentNameList(item.AccParent);
                r.IsActive = Convert.ToBoolean(item.IsActive);
                ItemList.Add(r);
            }

            return Json(new { data = ItemList.ToList() }, JsonRequestBehavior.AllowGet);
        }

        // جلب البيانات قبل التعديل
        public ActionResult GetDataForUpdate(int id)
        {
            AccountTree AccountTree = db.AccountTrees.Find(id);

            TreeModel Model = new TreeModel();
            Model.AccName = AccountTree.AccName;
            Model.AccCode = Convert.ToDecimal(AccountTree.AccCode);
            Model.AccParent = Convert.ToInt32(ClsTree.GetParentId(Convert.ToDecimal(AccountTree.AccParent)));
            Model.Id = AccountTree.Id;
            //  Model.AccParentId = Convert.ToInt32(ClsTree.GetParentId(Convert.ToDecimal(AccountTree.AccParent)));

            return Json(Model, JsonRequestBehavior.AllowGet);

        }

        // شاشة عرض التفاصيل
        public ActionResult Detail(int Id)
        {
            if (db.TransactionDetails.Any(x => x.AccTreeId == Id))
            {
                var data = new List<TransactionVM>();
                var trans = db.TransactionDetails.Where(x => x.AccTreeId == Id).ToList();
                var accTree = db.AccountTrees.Find(Id);

                var dList = db.TransactionDetails.Where(x => x.AccTreeId == Id).Select(x => x.TransactionId).ToList();

                var lastFivetrans = db.Transactions.Where(x => dList.Contains(x.Id)).OrderByDescending(s => s.Id).Take(5)
                    .Select(x => new TransactionVM()
                    {
                        transactionId = x.Id,
                        currency = x.CurrencyType.Name,
                        amount = x.Amount,
                        documentType = x.DocumentType.Name,
                        exchangeRate = x.ExchangeRate,
                        transactionDate = x.TransactionDate,
                        transactionDateStr = x.TransactionDate.ToString()
                    })
                    .ToList();

                foreach (var item1 in dList)
                {
                    foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == item1))
                    {
                        TransactionVM obj = new TransactionVM();

                        obj.transactionId = Convert.ToInt32(item.TransactionId);
                        obj.accName = item.AccountTree.AccName;
                        obj.credit = item.Credit;
                        obj.debit = item.Debit;
                        obj.transactionDateStr = item.Transaction.TransactionDate != null ? item.Transaction.TransactionDate.Value.ToShortDateString() : "";
                        obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

                        data.Add(obj);
                    }
                }

                var myObj = new AllViewMode()
                {
                    AccTree =
                    {
                        Id = Id,
                        AccName = accTree.AccName,
                        AccLevel = "المستوي " + accTree.TheLevel.Value,
                        AccParent = accTree.AccParent > 0 ? db.AccountTrees.Find(accTree.AccParent).AccName : "هو حساب رئيسي",
                        AccCode = accTree.AccCode,
                        AccType = accTree.AccountType.Name,
                        AccNature = accTree.AccountNature.Name,
                        AccFinal = accTree.AccountFinal.Name,

                    },
                    //accName = accTree.AccName,
                    //accLevel = "المستوي " + accTree.TheLevel.Value,
                    //accParent = accTree.AccParent > 0 ? db.AccountTrees.Find(accTree.AccParent).AccName : "هو حساب رئيسي",
                    //accCode = accTree.AccCode,
                    //accType = accTree.AccountType.Name,
                    //accNature = accTree.AccountNature.Name,
                    //accFinal = accTree.AccountFinal.Name,

                    countOfCridet = trans.Where(x => x.Credit > 0).Count(),
                    countOfDebit = trans.Where(x => x.Debit > 0).Count(),
                    sumOfCridet = trans.Sum(x => x.Credit),
                    sumOfDebit = trans.Sum(x => x.Debit),
                    balance = trans.Sum(x => x.Credit) - trans.Sum(x => x.Debit),
                    rowsCount = trans.Count(),

                    transDetails = data,
                    lastFivetrans = lastFivetrans,
                };
                return View(myObj);
            }
            else
            {
                var accTree = db.AccountTrees.Find(Id);

                AccountTreeVM myObj = new AccountTreeVM();

                myObj.Id = Id;
                myObj.AccName = accTree.AccName;
                myObj.AccLevel = "المستوي " + accTree.TheLevel.Value;
                myObj.AccParent = accTree.AccParent > 0 ? db.AccountTrees.Find(accTree.AccParent).AccName : "هو حساب رئيسي";
                myObj.AccCode = accTree.AccCode;
                myObj.AccType = accTree.AccountType.Name;
                myObj.AccNature = accTree.AccountNature.Name;
                myObj.AccFinal = accTree.AccountFinal.Name;

                return View(myObj);
            }
        }

        // تعديل بيانات الحساب
        public ActionResult Edit(TreeModel model)
        {
            var userid = User.Identity.GetUserId();

            if (model.Id >= 0)
            {
                if (db.AccountTrees.Any(x => x.AccName == model.AccName && x.Id != model.Id))
                {
                    return Json(new { Message = "هذا البند موجود  مسبقا في الشجرة المحاسبية", Title = "عملية الاضافة", Status = "error" });
                }

                AccountTree AccountTree = db.AccountTrees.Find(model.Id);
                AccountTree.AccName = model.AccName;
                AccountTree.CreatedBy = userid;
                AccountTree.CreationDate = DateTime.Now;

                db.Entry(AccountTree).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success" });
            }
            else
            {
                return Json(new { Message = "حدث خطأ اثناء التعديل", Title = "خطأ", Status = "error" });
            }
        }

        // جلب شاشة الاضافة والتعديل
        public ActionResult GetTreeParial(int? id)
        {

            List<AccountTree> Model = db.AccountTrees.ToList();
            var userId = User.Identity.GetUserId();
            ViewBag.UserId = userId;

            return PartialView("_Tree");
        }

        // حذف حساب
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return Json(new { Status = "error", Message = "رقم العنصر غير موجود", Title = "تعذر الحذف" }, JsonRequestBehavior.AllowGet);
            }

            AccountTree AccountTree = db.AccountTrees.Find(id);
            if (AccountTree == null)
            {
                return Json(new { Status = "error", Message = "العنصر غير موجود", Title = "تعذر الحذف" }, JsonRequestBehavior.AllowGet);
            }

            if (db.Balances.Any(x => x.AccountTreeId == AccountTree.Id))
            {
                return Json(new { Status = "error", Message = "لايمكن حذف هذا العنصر", Title = "تعذر الحذف" }, JsonRequestBehavior.AllowGet);
            }

            //check is Has child
            if (db.AccountTrees.Any(x => x.AccParent == AccountTree.Id))
            {
                return Json(new { Status = "error", Message = "لايمكن حذف هذا العنصر", Title = "تعذر الحذف" }, JsonRequestBehavior.AllowGet);
            }

            //هنا يجب حذف البند من جدول ال AccountSubs اولا 
            if (AccountTree.AccTypeId == 2)
            {
                AccountSub AccSubObj = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == AccountTree.Id);
                db.AccountSubs.Remove(AccSubObj);
            }

            var AccountTreeDelete = db.AccountTrees.Find(id);
            db.AccountTrees.Remove(AccountTreeDelete);
            db.SaveChanges();
            return Json(new { Status = "success", Message = "تم حذف البند", Title = " الحذف" }, JsonRequestBehavior.AllowGet);
        }
        
        /*Get Data For Select2 Inputs*/
        public ActionResult GetAccountType(string q)
        {
            var data = ClsTree.GetAccountType(q);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountNature(string q)
        {
            var data = ClsTree.GetAccountNature(q);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountFinal(string q)
        {
            var data = ClsTree.GetAccountFinal(q);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountSubCategory(string q)
        {
            var data = ClsTree.GetAccountSubCategory(q);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult GetAccountNoneSub(string q)
        {
            var data = ClsTree.GetAccountNoneSub(q);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateNaji(AccountTree data)
        {
            int AccId = Convert.ToInt32(db.SpLastTreeAccountId().FirstOrDefault());
            int AccParentCode = ClsTree.GetAccParentCode(Convert.ToInt32(data.AccParent));

            /// int AccParentCode = Convert.ToInt32(data.AccParent);

            //int TheLastLevelCheck = Convert.ToInt32(db.SpLastLevel(AccParentCode).FirstOrDefault());

            //if (TheLastLevelCheck >= 9)
            //{
            //    return Json(new { Message = "لقت وصلت لاقصي  حد في الشجرة المحاسبية", Title = "عملية الاضافة", Status = "error" });
            //}
            string AccCode = db.GetLastAccNumber(AccParentCode).FirstOrDefault().ToString();


            AccountTree AccountTree = db.AccountTrees.Find(data.AccParent);

            if (data.AccName != null)
            {

                if (db.AccountTrees.Any(x => x.AccName == data.AccName))
                {
                    return Json(new { Message = "هذا البند موجود  مسبقا في الشجرة المحاسبية", Title = "عملية الاضافة", Status = "error" });
                }


                if (!db.AccountTrees.Any(f => f.AccName == data.AccName))
                {
                    AccountTree d = new AccountTree();
                    d.Id = AccId;
                    d.AccCode = AccCode;
                    d.AccName = data.AccName;
                    if (AccountTree == null)
                    {
                        d.AccParent = 0;
                    }
                    else
                    {
                        d.AccParent = AccountTree.Id;
                    }
                    if (data.AccParent == null || data.AccParent == 0)
                    {
                        d.TheLevel = 0;
                    }
                    else
                    {
                        d.TheLevel = Convert.ToInt32(AccountTree.TheLevel) + 1;
                    }

                    db.AccountTrees.Add(d);
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
            }
            return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });

        }

        
        // دي لي تعديل الحساب الاب في الشجرة لكن بكون في سنة مالية محدده
        [HttpPost]
        public ActionResult UpdateAccTreeFinancialCycle(int Id, int FinancialCycleId, int AccParentId)
        {
            SharedClass sh = new SharedClass();
            var userId = User.Identity.GetUserId();

            int CurrentFinancialCycleId = sh.GetUserCurrentFinancialCycleId(userId);

            if (CurrentFinancialCycleId != FinancialCycleId)
            {
                return Json(new { Message = "العام المالي الذي تعمل عليه هو ليس العام المالي الحالي", Title = "خطأ", Status = "error" });
            }

            try
            {
                if(db.FinancialCycleAccountTrees.Any(x=> x.FinancialCycleId == FinancialCycleId && x.AccTreeId == Id))
                {
                    var item = db.FinancialCycleAccountTrees.FirstOrDefault(x => x.FinancialCycleId == FinancialCycleId && x.AccTreeId == Id);

                    item.FinancialCycleId = FinancialCycleId;
                    item.AccTreeId = Id;
                    item.ParentId = AccParentId;

                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                    db.Entry(item).Reload();

                    return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    FinancialCycleAccountTree item = new FinancialCycleAccountTree();

                    item.FinancialCycleId = FinancialCycleId;
                    item.AccTreeId = Id;
                    item.ParentId = AccParentId;

                    db.FinancialCycleAccountTrees.Add(item);
                    db.SaveChanges();

                    db.Entry(item).Reload();

                    return Json(new { Message = "تمت عملية الاضافة بنجاح", Title = "نجاح", Status = "success" });
                }
            }catch(Exception e)
            {
                return Json(new { Message = "خطا في عملية التعديل (Exception)", Title = "خطأ", Status = "error" });
            }
        }




        public ActionResult Active(int? id, bool? IsActive)
        {
            if (id == null)
            {
                return Json(new { Status = "error", Message = "رقم العنصر غير موجود", Title = "خطأ" }, JsonRequestBehavior.AllowGet);
            }

            var accountTree = db.AccountTrees.Find(id);

            if (accountTree == null)
            {
                return Json(new { Status = "error", Message = "العنصر غير موجود", Title = "خطأ" }, JsonRequestBehavior.AllowGet);
            }

            if (IsActive == null)
            {
                return Json(new { Status = "error", Message = "لم يتم تحديد الحالة", Title = "خطأ" }, JsonRequestBehavior.AllowGet);
            }

            accountTree.IsActive = IsActive.Value;

            db.Entry(accountTree).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new
            {
                Status = "success",
                Message = IsActive.Value ? "تم التفعيل بنجاح" : "تم إلغاء التفعيل بنجاح",
                Title = "نجاح"
            }, JsonRequestBehavior.AllowGet);
        }







    }
}