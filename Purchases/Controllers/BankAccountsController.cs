using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Purchases.Models;
using System.Data.Entity;
using Purchases.Models.ViewModal;
using Purchases.Models.ViewModel;
using Purchases.MyLogic;
using Microsoft.AspNet.Identity;
using WebGrease.Css.Extensions;

namespace Purchases.Controllers
{
    public class BankAccountsController : Controller
    {
        private Entities db = new Entities();
        private TreeClass treecls = new TreeClass();

        // GET: BankAccounts
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadData()
        {
            var data = db.AccountSubs.Where(x => x.AccountTree.AccParent == 7).Select(p => new
            {
                Id = p.Id,
                Name = p.AccountTree.AccName,
                AccTreeId = p.AccTreeId,
                BankId = db.BankAccounts.Any(x => x.AccountSubId == p.Id) ? db.BankAccounts.FirstOrDefault(x => x.AccountSubId == p.Id).Id : 0,
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getBankData(int accountSubId)
        {
            var data = new object();
            if (db.BankAccounts.Any(x => x.AccountSubId == accountSubId))
            {
                var p = db.BankAccounts.FirstOrDefault(x => x.AccountSubId == accountSubId);
                data = new
                {
                    Id = p.Id,
                    AccTreeId = p.AccountSub.AccTreeId,
                    Name = p.AccountSub.AccountTree.AccName,
                    Number = p.Number,
                    CurrencyTypeId = p.CurrencyTypeId,
                    CurrencyType = p.CurrencyType.Name,
                    OpenDate = p.OpenDate?.Month + "/" + p.OpenDate?.Day + "/" + p.OpenDate?.Year,
                    IBan = p.IBan,
                    BankLabel = p.BankLabel,
                    BankAccountTypeId = p.BankAccountTypeId,
                    BankAccountType = p.BankAccountType.Name,
                    FirstSignature = p.FirstSignature,
                    SecondSignature = p.SecondSignature
                };
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(BankVM data)
        {
            try
            {
                var userid = User.Identity.GetUserId();
                //int res = treecls.AddToTree(data.accParentName, data.AccName, 2, data.AccCategoryId);

                if (data.Id > 0)
                {
                    BankAccount b = db.BankAccounts.Find(data.Id);

                    b.BankAccountTypeId = data.BankAccountTypeId;
                    b.Number = data.Number;
                    b.OpenDate = Convert.ToDateTime(data.OpenDate);
                    b.CurrencyTypeId = data.CurrencyTypeId;
                    b.IBan = data.IBan;
                    b.BankLabel = data.BankLabel;
                    b.UpdatedBy = userid;
                    b.UpdatingDate = DateTime.Now;
                    b.FirstSignature = data.FirstSignature;
                    b.SecondSignature = data.SecondSignature;

                    db.Entry(b).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { Message = "تم التعديل بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    BankAccount b = new BankAccount();

                    b.AccountSubId = Convert.ToInt32(data.AccountSubId);
                    b.BankAccountTypeId = data.BankAccountTypeId;
                    b.Number = data.Number;
                    b.OpenDate = Convert.ToDateTime(data.OpenDate);
                    b.CurrencyTypeId = data.CurrencyTypeId;
                    b.IBan = data.IBan;
                    b.FirstSignature = data.FirstSignature;
                    b.SecondSignature = data.SecondSignature;
                    b.CreatedBy = userid;
                    b.CreationDate = DateTime.Now;

                    db.BankAccounts.Add(b);
                    db.SaveChanges();

                    return Json(new { Message = "تمت الاضافة بنجاح", Title = "نجاح", Status = "success" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }

        [HttpPost]
        public ActionResult Update(BankVM data)
        {
            try
            {
                var userid = User.Identity.GetUserId();
                BankAccount bank = db.BankAccounts.Find(data.Id);
                AccountTree tree = db.AccountTrees.Find(data.AccId);

                tree.AccName = data.AccName;

                bank.BankAccountTypeId = data.BankAccountTypeId;
                bank.Number = data.Number;
                bank.BankLabel = data.BankLabel;
                bank.OpenDate = Convert.ToDateTime(data.OpenDate);
                bank.CurrencyTypeId = data.CurrencyTypeId;
                bank.IBan = data.IBan;
                bank.FirstSignature = data.FirstSignature;
                bank.SecondSignature = data.SecondSignature;
                bank.UpdatedBy = userid;
                bank.UpdatingDate = DateTime.Now;

                db.Entry(bank).State = EntityState.Modified;
                db.Entry(tree).State = EntityState.Modified;

                db.SaveChanges();

                return Json(new { Message = "تمت الاضافة بنجاح", Title = "نجاح", Status = "success" });
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }

        public ActionResult GetBankName(string q)
        {
            var data = db.AccountSubs.Where(f => f.AccCategoryId == 1 & !f.BankAccounts.Any(g => g.AccountSubId == f.Id))
                .Select(p => new
                {
                    id = p.Id,
                    text = p.AccountTree.AccName
                }).Where(x => x.text.Contains(q));

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCurrencyType(string q)
        {
            var data = db.CurrencyTypes.Select(p => new
            {
                id = p.Id,
                text = p.Name
            }).Where(x => x.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBankAccountType(string q)
        {
            var data = db.BankAccountTypes.Select(p => new
            {
                id = p.Id,
                text = p.Name
            }).Where(x => x.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult checkCode(int Id)
        {
            string data = treecls.GetLastAccCode(Id);

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetAllBankTransactionByBankId(int accountSubId)
        {
            var userId = User.Identity.GetUserId();
            SharedClass sh = new SharedClass();
            var financialCycleId = sh.GetUserCurrentFinancialCycleId(userId);

            int accTreeId = 0;
            if (db.BankAccounts.Any(x => x.AccountSubId == accountSubId))
            {
                var AccTreeIdBank = db.BankAccounts.FirstOrDefault(x => x.AccountSubId == accountSubId).AccountSub.AccTreeId;
                accTreeId = Convert.ToInt32(AccTreeIdBank);
            }

            var transactionDataList = db.TransactionDetails
                .Where(q => q.Transaction.FinancialCycleId == financialCycleId && q.AccTreeId == accTreeId).ToList();
            var deletedtransactionDataList = db.DeletedTransactionDetails
                .Where(q => q.DeletedTransaction.FinancialCycleId == financialCycleId && q.AccTreeId == accTreeId).ToList();

            var data = new List<dynamic>();

            // Transactions
            foreach (var item in transactionDataList)
            {
                data.Add(new
                {
                    transactionId = item.TransactionId,
                    note = item.Transaction.Note,
                    TransactionDate = item.Transaction.TransactionDate,
                    transactionDateStr =
                        item.Transaction.TransactionDate?.Year.ToString() + "-" +
                        item.Transaction.TransactionDate?.Month.ToString() + "-" +
                        item.Transaction.TransactionDate?.Day.ToString(),
                    DocumentType = item.Transaction.DocumentType.Name,
                    Amount = item.Debit + item.Credit,
                    Currency = item.Transaction.CurrencyType.Name,
                    ExchangeRate = item.Transaction.ExchangeRate,
                    HasAddedTax = item.Transaction.HasAddedTax,
                    HasTax = item.Transaction.HasTax,
                    IsPosted = item.Transaction.IsPosted,
                    IsDeleted = false,

                    CheckNo = item.Transaction.PrintChecks.Any(q=> q.TransactionId == item.TransactionId)?
                    item.Transaction.PrintChecks.FirstOrDefault(q => q.TransactionId == item.TransactionId).CheckNo.ToString(): "No Check",

                    Id = item.Id,
                    AccountName = item.AccountTree.AccName,
                    Debit = item.Debit,
                    Credit = item.Credit,
                    BalanceId = item.BalanceId,
                });
            }

            // Delete Transactions
            foreach (var item in deletedtransactionDataList)
            {
                data.Add(new
                {
                    transactionId = item.DeletedTransactionId,
                    note = item.DeletedTransaction.Note,
                    TransactionDate = item.DeletedTransaction.TransactionDate,
                    transactionDateStr = 
                        item.DeletedTransaction.TransactionDate?.Year.ToString() + "-" +
                        item.DeletedTransaction.TransactionDate?.Month.ToString() + "-" +
                        item.DeletedTransaction.TransactionDate?.Day.ToString(),
                    DocumentType = item.DeletedTransaction.DocumentType.Name,
                    Amount = item.Debit + item.Credit,
                    Currency = item.DeletedTransaction.CurrencyType.Name,
                    ExchangeRate = item.DeletedTransaction.ExchangeRate,

                    HasAddedTax = item.DeletedTransaction.HasAddedTax,
                    HasTax = item.DeletedTransaction.HasTax,
                    IsPosted = item.DeletedTransaction.IsPosted,
                    IsDeleted = true,

                    CheckNo = !item.DeletedTransaction.CheckNo.Contains("0")? item.DeletedTransaction.CheckNo : "No Check",

                    Id = item.Id,
                    AccountName = item.AccountTree.AccName,
                    Debit = item.Debit,
                    Credit = item.Credit,
                    BalanceId = item.BalanceId,
                });
            }

            data = data.OrderByDescending(q => q.TransactionDate).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BankTransactions(int Id)
        {
            var AccName = db.AccountSubs.Find(Id).AccountTree.AccName;
            ViewBag.accountSubId = Id;
            ViewBag.AccName = AccName;

            return View();
        }
        
        /*---------------------------------------------- Checks ----------------------------------------------------*/

        public ActionResult Check(int Id)
        {
            if (Id > 0)
            {
                var bank = db.BankAccounts.Find(Id);
                var bankName = db.AccountTrees.FirstOrDefault(x => x.Id == bank.AccountSub.AccTreeId).AccName;

                ViewBag.bankName = bankName;
                ViewBag.bankId = Id;

                return View();
            }
            else
            {
                return Json(new { Message = "حدث خطأ أثناء إجراء العملية", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult LoadCheckData(int Id)
        {
            var data = db.Checks.Where(x => x.BankAccountId == Id).Select(p => new
            {
                Id = p.Id,
                StartFromNumber = p.StartFromNumber,
                EndToNumber = p.EndToNumber,
                BooKNumber = p.BooKNumber,
                IsFinished = p.IsFinished == true ? "منتهي" : "غير منتهي",
                BankAccountId = p.BankAccountId
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateOrUpdate(CheckVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                if (model.Id > 0)
                {
                    var obj = db.Checks.Find(model.Id);

                    obj.StartFromNumber = model.StartFromNumber;
                    obj.EndToNumber = model.EndToNumber;
                    obj.BankAccountId = model.BankAccountId;
                    obj.BooKNumber = model.BooKNumber;
                    obj.IsFinished = model.IsFinished;
                    obj.UpdatedBy = userid;
                    obj.UpdatingDate = DateTime.Now;

                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { Message = "تمت عملية التعديل  بنجاح", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Check obj = new Check();

                    obj.StartFromNumber = model.StartFromNumber;
                    obj.EndToNumber = model.EndToNumber;
                    obj.BankAccountId = model.BankAccountId;
                    obj.BooKNumber = model.BooKNumber;
                    obj.IsFinished = false;
                    obj.CreatedBy = userid;
                    obj.CreationDate = DateTime.Now;

                    db.Checks.Add(obj);
                    db.SaveChanges();

                    return Json(new { Message = "تمت عملية الاضافة  بنجاح", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء إجراء العملية", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteCheck(int Id)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                var data = db.Checks.Find(Id);
                data.IsFinished = true;
                data.UpdatedBy = userid;
                data.UpdatingDate = DateTime.Now;

                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { Message = "تمت عملية الايقاف  بنجاح", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الايقاف", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}