using Microsoft.AspNet.Identity;
using Purchases.Models;
using Purchases.Models.ViewModal;
using Purchases.Models.ViewModel;
using Purchases.MyLogic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly Entities db;
        private readonly Transact obj;

        // constructor
        public TransactionsController()
        {
            db = new Entities();
            obj = new Transact(db);
        }

        // سندات القيد
        public ActionResult Index()
        {
            return View();
        }

        // سندات القيد
        public ActionResult IndexAll()
        {
            return View();
        }

        //تسويات المرتبات
        public ActionResult SalariesSettlement()
        {
            return View();
        }

        //بحث بي اسم المستلم
        public ActionResult SearchByRecipient()
        {
            return View();
        }

        //  شاشة القيود المرحلة
        public ActionResult PostedTransactions()
        {
            return View();
        }

        // جلب جميع البيانات بغض النظر عن نوع القيد هو قبض او صرف او تحويل او غيره
        [HttpGet]
        public ActionResult LoadDataAll()
        {
            var data = obj.GetAllData();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // جلب بيانات سندات القيد
        [HttpGet]
        public ActionResult LoadData()
        {
            var data = obj.GetData(1);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // جلب القيود المرحلة
        [HttpGet]
        public ActionResult LoadDataPosted()
        {
            var userid = User.Identity.GetUserId();
            var data = obj.GetDataPosted(userid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // تحميل البيانات في شاشة تسوية المرتبات
        [HttpGet]
        public ActionResult LoadDataForSettlement()
        {
            var data = obj.GetData(8);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // جلب البيانات  في شاشة البحث باسم المستلم
        [HttpGet]
        public ActionResult LoadDataForSearchByRecipient()
        {
            var data = obj.GetData(-1); // القيمة دي عشان يجيب لي كل البيانات بدون فرز
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // حفظ بيانات سندات القيد + تسويات المرتبات
        [HttpPost]
        public ActionResult Create(List<TransactionVM> data)
        {
            var userid = User.Identity.GetUserId();
            int res = obj.PostData(data, userid);

            if (res > 0)
            {
                return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
            }
            else if (res == 0)
            {
                return Json(new { Message = "يجب أن يتساوي الجانب المدين مع الجانب الدائن", Title = "خطأ", Status = "error" });
            }
            else
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة(Exception)", Title = "خطأ", Status = "error" });
            }
        }

        // حفظ بيانات الحركة في سندات القبض
        [HttpPost]
        public ActionResult CreateRec(List<TransactionVM> data)
        {
            var userid = User.Identity.GetUserId();
            int res = obj.PostData(data, userid);

            if (res > 0)
            {
                return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
            }
            else if (res == 0)
            {
                return Json(new { Message = "يجب أن يتساوي الجانب المدين مع الجانب الدائن", Title = "خطأ", Status = "error" });
            }
            else
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة(Exception)", Title = "خطأ", Status = "error" });
            }
        }

        // ترحيل القيد
        [HttpPost]
        public ActionResult Posting(int Id)
        {
            try
            {
                var userid = User.Identity.GetUserId();
                int res = obj.PostingTransaction(Id, userid);

                if (res > 0)
                {
                    return Json(new { Message = "تمت عملية الترحيل  بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "حدث خطأ أثناء عملية الترحيل", Title = "خطأ", Status = "error" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الترحيل", Title = "خطأ", Status = "error" });
            }
        }

        // اضافة / حذف ضريبة 17 % للحركة
        [HttpPost]
        public ActionResult Add17Tax(int Id, bool HasAddedTax, decimal? AddedTaxPercent)
        {
            try
            {
                var userid = User.Identity.GetUserId();
                int res = obj.Add17Tax(Id, HasAddedTax, AddedTaxPercent, userid);

                if (res > 0)
                {
                    return Json(new { Message = "تمت العملية بنجاح", Title = "نجاح", Status = "success" });
                }
                else if (res == -100)
                {
                    return Json(new { Message = "لا توجد بنود لتحميل الضريبة عليها", Title = "خطأ", Status = "error" });
                }
                else
                {
                    return Json(new { Message = "حدث خطأ أثناء العملية", Title = "خطأ", Status = "error" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء العملية (Exception)", Title = "خطأ", Status = "error" });
            }
        }
        // اضافة / حذف ضريبة 1 % للحركة
        [HttpPost]
        public ActionResult AddTax(int Id, bool HasTax)
        {
            try
            {
                var userid = User.Identity.GetUserId();
                int res = obj.AddTax(Id, HasTax, userid);

                if (res > 0)
                {
                    return Json(new { Message = "تمت العملية بنجاح", Title = "نجاح", Status = "success" });
                }
                else if (res == -100)
                {
                    return Json(new { Message = "لا توجد بنود لتحميل الضريبة عليها", Title = "خطأ", Status = "error" });
                }
                else if (res == -200)
                {
                    return Json(new { Message = "ضريبة ال 1% مدخلة مسبقا", Title = "خطأ", Status = "error" });
                }
                else
                {
                    return Json(new { Message = "حدث خطأ أثناء العملية", Title = "خطأ", Status = "error" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء العملية (Exception)", Title = "خطأ", Status = "error" });
            }
        }
        // اضافة / حذف الدمغة للحركة
        [HttpPost]
        public ActionResult AddStamp(int Id, decimal stampValue)
        {
            try
            {
                var userid = User.Identity.GetUserId();
                int res = obj.AddStamp(Id, stampValue, userid);

                if (res > 0)
                {
                    return Json(new { Message = "تمت العملية بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "حدث خطأ أثناء العملية", Title = "خطأ", Status = "error" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء العملية", Title = "خطأ", Status = "error" });
            }
        }

        // اضافة / حذف الدمغة للحركة
        [HttpPost]
        public ActionResult AddIncomeTax(int Id, decimal IncomeTaxValue)
        {
            try
            {
                var userid = User.Identity.GetUserId();
                int res = obj.AddIncomeTax(Id, IncomeTaxValue, userid);

                if (res > 0)
                {
                    return Json(new { Message = "تمت العملية بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "حدث خطأ أثناء العملية", Title = "خطأ", Status = "error" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء العملية", Title = "خطأ", Status = "error" });
            }
        }

        // تعديل بيانات الحركة
        public ActionResult Edit(int Id)
        {
            var trans = db.Transactions.Find(Id);
            ViewBag.TransactionId = Id;

            //string TransactionDate = trans.TransactionDate != null ? trans.TransactionDate.Value.Date.ToString("dd/MM/yyyy") : "غير مدخل";
            string TransactionDate = trans.TransactionDate != null ? trans.TransactionDate.Value.Date.ToShortDateString() : "غير مدخل";

            ViewBag.TransactionDate = TransactionDate;
            ViewBag.Recipient = trans.Recipient;
            ViewBag.Note = trans.Note != null ? trans.Note.Trim() : "غير مدخل";
            ViewBag.CurrencyType = trans.CurrencyType.Name != null ? trans.CurrencyType.Name : "غير مدخل";
            ViewBag.ExchangeRate = trans.ExchangeRate != null ? trans.ExchangeRate.ToString() : "غير مدخل";
            ViewBag.Note = trans.Note != null ? trans.Note : "غير مدخل";

            return View(trans);
        }

        // جلب البيانات لعرض تفاصيل الحركة
        [HttpGet]
        public ActionResult getDetailData(int Id)
        {
            var data = obj.GetDetailsData(Id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // دي تقريبا لمن اضغط علي ايقونة الاستعلام بجيب بيانات تفاصيل الحركة من هنا ... مفترض نوحد الدالة تكون نفس الفوق دي
        public ActionResult getTransDetailsData_NotUsed(int Id)
        {
            var data = obj.GetDetailsData(Id);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // تعديل الحركة
        [HttpPost]
        public ActionResult Update(List<TransactionVM> data)
        {
            var userid = User.Identity.GetUserId();
            SharedClass sh = new SharedClass();
            var first = data.FirstOrDefault();
            var currentTransactionYear = first.transactionDate.Value.Year.ToString();
            // var financialCycleId = sh.GetCurrentFinancialCycleId();// اذا السنة المالية مقفولة ما يعمل اي حاجه
            //int FinancialCycleId = sh.GetFinancialCycleIdForSpecificDate(currentTransactionYear); // هنا دي مفترض تتحول حسب العام المالي للمستخدم
            int FinancialCycleId = sh.GetUserCurrentFinancialCycleId(userid); // جلب العام المالي للمستخدم الحالي

            int res = obj.UpdateData(data, userid, FinancialCycleId);

            if (res == -1000)
            {
                return Json(new { Message = "العام المالي مقفول او غير موجود‘ لا يمكن اجراء اي اعملية", Title = "خطأ", Status = "error" });
            }

            if (res > 0)
            {
                return Json(new { Message = "تمت عملية التعديل  بنجاح", Title = "نجاح", Status = "success" });
            }
            else if (res == 0)
            {
                return Json(new { Message = "يجب أن يتساوي الجانب المدين مع الجانب الدائن", Title = "خطأ", Status = "error" });
            }
            else // -1 means transaction NotFound
            {
                return Json(new { Message = "حدث خطأ أثناء عملية التعديل", Title = "خطأ", Status = "error" });
            }
        }

        // حذف صف واحد في تفاصيل الحركة
        // مستخدمة في سندات القيد - تقريبا في عملية التعديل قد احتاج احذف
        [HttpPost]
        public ActionResult DeleteTransactionDetails(int Id)
        {
            var userid = User.Identity.GetUserId();
            int res = obj.DeleteDetailsData(Id, userid);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        // دي ما شغالة متاكد منها بس قد احتاج ارجع ليها
        public ActionResult printDataByDatesOld(DateTime dateFrom, DateTime dateTo)
        {
            List<TransactionVM> data = new List<TransactionVM>();
            var dList = db.TransactionDetails.Where(x => DbFunctions.TruncateTime(x.Transaction.TransactionDate) >= dateFrom & DbFunctions.TruncateTime(x.Transaction.TransactionDate) <= dateTo).ToList();

            foreach (var item in dList.OrderBy(x => x.Transaction.TransactionDate))
            {
                TransactionVM obj = new TransactionVM();

                obj.transactionId = Convert.ToInt32(item.TransactionId);
                obj.accName = item.AccountTree.AccName;
                obj.transactionDate = item.Transaction.TransactionDate;
                obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

                if (item.Transaction.HasTax == true & item.Transaction.HasAddedTax == true) // ضريبة 1% + ضريبة 17% معا
                {
                    /*
                    TransactionId = 22
                    amount = 2400000
                    tax = 24000
                    addTax = 408000
                    itemAmount = amount - tax + addtax
                    itemAmount = 2400000 - 24000 + 408000
                    */
                    if (item.BalanceId == null)
                    {
                        obj.credit = item.Credit;
                        obj.debit = item.Debit + (item.Debit * (decimal)0.17);
                    }
                    else
                    {
                        obj.debit = item.Debit + (item.Debit * (decimal)0.17);
                        obj.credit = item.Credit + (item.Credit * (decimal)0.17) - (item.Credit * (decimal)0.01);
                    }
                }
                else if (item.Transaction.HasTax == true & item.Transaction.HasAddedTax != true) // ضريبة 1% فقط
                {
                    /*
                    TransactionId = 22
                    amount = 2400000
                    tax = 24000
                    addTax = 0
                    itemAmount = amount - tax + addtax
                    itemAmount = 2400000 - 24000 + 0
                    */
                    if (item.BalanceId == null)
                    {
                        obj.credit = item.Credit;
                        obj.debit = item.Debit;
                    }
                    else
                    {
                        obj.credit = item.Credit - (item.Credit * (decimal)0.01);
                        obj.debit = item.Debit;
                    }
                }
                else if (item.Transaction.HasTax != true & item.Transaction.HasAddedTax == true) // ضريبة 17% فقط
                {
                    /*
                    TransactionId = 22
                    amount = 2400000
                    tax = 0
                    addTax = 408000
                    itemAmount = amount - tax + addtax
                    itemAmount = 2400000 - 0 + 408000
                    */
                    if (item.BalanceId == null)
                    {
                        obj.credit = item.Credit + (item.Credit * (decimal)0.17);
                        obj.debit = item.Debit;
                    }
                    else
                    {
                        obj.credit = item.Credit + (item.Credit * (decimal)0.17);
                        obj.debit = item.Debit + (item.Debit * (decimal)0.17);
                    }
                }
                else
                {
                    //obj.credit = item.Credit;
                    //obj.debit = item.Debit;

                    if (item.BalanceId == null)
                    {
                        obj.credit = item.Credit;
                        obj.debit = item.Debit;
                    }
                    else
                    {
                        obj.credit = item.Credit;
                        obj.debit = item.Debit - item.Credit;
                    }

                }

                data.Add(obj);
            }

            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.dateFrom = dateFrom.ToShortDateString();
            ViewBag.dateTo = dateTo.ToShortDateString();

            return View(data);
        }

        public ActionResult getTransactionBanks(int id)
        {
            if (id <= 0)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية التعديل", Title = "خطأ", Status = "error" });
            }

            SharedClass sh = new SharedClass();
            var result = sh.GetTransactionBankAccounts(id);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getTransactionRecipients(int Id)
        {
            if (Id <= 0)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية التعديل", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }

            SharedClass sh = new SharedClass();
            var transactionObj = db.Transactions.Find(Id);

            // قائمة المستلمين الأصلية
            var transactionRecipients = db.TransactionRecipients
                .Where(q => q.TransactionId == Id)
                .Select(p => new
                {
                    TransactionId = Id,
                    Id = p.Id,
                    RecipientName = p.RecipientName,
                    BankName = p.BankName,
                    BranchName = p.BranchName,
                    AccountNumber = p.AccountNumber,
                    BankAccountId = p.BankAccountId ?? 0,
                    Amount = (decimal)p.Amount
                })
                .ToList();

            var data = db.TransactionDetails.Where(q => q.TransactionId == Id && q.Credit > 0).ToList();

            foreach (var item in data)
            {
                int bankId = sh.IsItBankAccount(Id, item.Credit);
                if (bankId > 0)
                {
                    var bankAccountObj = db.BankAccounts.Find(bankId);
                    if (transactionRecipients.Any(q => q.BankAccountId == bankId))
                    {
                       var Aomunt = transactionRecipients.Where(q => q.BankAccountId == bankId).Sum(q=>q.Amount);

                        transactionRecipients.Add(
                            new
                            {
                                TransactionId = Id,
                                Id = 0,
                                RecipientName = "الكشف المرفق",
                                BankName = bankAccountObj.BankLabel,
                                BranchName = "",
                                AccountNumber = "",
                                BankAccountId = bankId,
                                Amount = item.Credit - Aomunt?? 0
                            });
                    }
                    else
                    {
                        transactionRecipients.Add(
                            new
                            {
                                TransactionId = Id,
                                Id = 0,
                                RecipientName = "الكشف المرفق",
                                BankName = bankAccountObj.BankLabel,
                                BranchName = "",
                                AccountNumber = "",
                                BankAccountId = bankId,
                                Amount = item.Credit ?? 0
                            });
                    }
                }
            }

            return Json(transactionRecipients.Where(q=>q.Amount > 0), JsonRequestBehavior.AllowGet);
        }

        // دي لمن يختار المستلم من القائمة ويعمل حفظ
        [HttpPost]
        public ActionResult SaveTransactionRecipients(int id, int recipientNameId, int recipientBankId, decimal amount)
        {
            try
            {
                if (id > 0 && recipientNameId > 0 && recipientBankId > 0 && amount > 0)
                {
                    var recipient = db.Recipients.Find(recipientNameId);
                    var AccountSubId = db.AccountSubs.FirstOrDefault(q => q.AccTreeId == recipientBankId).Id;
                    var bankAccountId = db.BankAccounts.FirstOrDefault(q => q.AccountSubId == AccountSubId).Id;
                    var transactionRecipients = db.TransactionRecipients.Where(q => q.TransactionId == id).ToList();

                    if (transactionRecipients.Any(q => q.RecipientName == recipient.RecipientName && q.Amount == amount))
                    {
                        return Json(new { Message = "هذا المستلم تم تسجيله مسبقا", Title = "خطأ", Status = "error" });
                    }

                    TransactionRecipient transactionRecipient = new TransactionRecipient();

                    transactionRecipient.TransactionId = id;
                    transactionRecipient.BankAccountId = bankAccountId;
                    transactionRecipient.RecipientName = recipient.RecipientName;
                    transactionRecipient.BankName = recipient.BankName;
                    transactionRecipient.BranchName = recipient.BranchName;
                    transactionRecipient.AccountNumber = recipient.AccountNumber;
                    transactionRecipient.Amount = amount;

                    db.TransactionRecipients.Add(transactionRecipient);
                    db.SaveChanges();

                    var redirectToUrl = Url.Action("printRecipientBankLetter", "Reports", new { Id = transactionRecipient.Id });

                    //return RedirectToAction("printRecipientBankLetter", "Reports", new { Id = transactionRecipient.Id });
                    return Json(new { redirectToUrl });
                }

                return Json(new { Message = "حدث خطأ أثناء عملية الاضافة, الرجاء مراجعة البيانات", Title = "خطأ", Status = "error" });
            }
            catch
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الاضافة(Exception)", Title = "خطأ", Status = "error" });
            }
        }


        // دي لمن يضغط علي ذر طباعة مباشرة
        [HttpGet]
        public ActionResult SaveTransactionRecipients(int id, string recipientName, string bankName, decimal amount)
        {
            try
            {
                if (id > 0 && !string.IsNullOrEmpty(recipientName) && !string.IsNullOrEmpty(bankName) && amount > 0)
                {
                    var recipient = db.Recipients.FirstOrDefault(q => q.RecipientName == recipientName && q.BankName == bankName);


                    var AccountTreeObj = db.AccountTrees.FirstOrDefault(q => q.AccName == bankName
                    );

                    var AccountSubId = db.AccountSubs.FirstOrDefault(q => q.AccTreeId == AccountTreeObj.Id).Id;
                    var bankAccountId = db.BankAccounts.FirstOrDefault(q => q.AccountSubId == AccountSubId).Id;
                    var transactionRecipients = db.TransactionRecipients.Where(q => q.TransactionId == id).ToList();

                    if (transactionRecipients.Any(q => q.RecipientName == recipient.RecipientName))
                    {
                        return Json(new { Message = "هذا المستلم تم تسجيله مسبقا", Title = "خطأ", Status = "error" });
                    }

                    TransactionRecipient transactionRecipient = new TransactionRecipient();

                    transactionRecipient.TransactionId = id;
                    transactionRecipient.BankAccountId = bankAccountId;
                    transactionRecipient.RecipientName = recipientName;
                    transactionRecipient.BankName = bankName;
                    transactionRecipient.BranchName = recipient.BranchName;
                    transactionRecipient.AccountNumber = recipient.AccountNumber;
                    transactionRecipient.Amount = amount;

                    db.TransactionRecipients.Add(transactionRecipient);
                    db.SaveChanges();

                    var redirectToUrl = Url.Action("printRecipientBankLetter", "Reports", new { Id = transactionRecipient.Id });

                    //return RedirectToAction("printRecipientBankLetter", "Reports", new { Id = transactionRecipient.Id });
                    return Json(new { redirectToUrl });
                }

                return Json(new { Message = "حدث خطأ أثناء عملية الاضافة, الرجاء مراجعة البيانات", Title = "خطأ", Status = "error" });
            }
            catch
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الاضافة(Exception)", Title = "خطأ", Status = "error" });
            }
        }
    }
}