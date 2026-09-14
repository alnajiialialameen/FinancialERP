using Microsoft.AspNet.Identity;
using Purchases.Models;
using Purchases.MyLogic;
using Purchases.CurencyOperation;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class PrintChecsController : Controller
    {
        Entities db = new Entities();

        // GET: PrintChecs
        public ActionResult Index(int Id)
        {
            decimal? Amount = 0;
            SharedClass sharedCls = new SharedClass();
            int bankId = sharedCls.IsTransactionHasBankAccount(Id);

            if (bankId > 0)
            {
                foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == Id && x.Credit > 0))
                {
                    int AccountSubId = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == item.AccTreeId).Id;

                    if (db.BankAccounts.Any(x=> x.AccountSubId == AccountSubId))
                    {
                        Amount = item.Credit + item.Debit;
                    }
                }

                Transaction obj = db.Transactions.Find(Id);

                //if (obj.HasTax == true & obj.HasAddedTax == true)
                //{
                //    Amount = Convert.ToDecimal(obj.Amount) - Convert.ToDecimal(obj.Amount * (decimal)0.01) + Convert.ToDecimal(obj.Amount * (decimal)0.17);
                //}
                //else if (obj.HasTax == false & obj.HasAddedTax == true)
                //{
                //    Amount = Convert.ToDecimal(obj.Amount) + Convert.ToDecimal(obj.Amount * (decimal)0.17);
                //}
                //else if (obj.HasTax == true & obj.HasAddedTax == false)
                //{
                //    Amount = Convert.ToDecimal(obj.Amount) - Convert.ToDecimal(obj.Amount * (decimal)0.01);
                //}
                //else
                //{
                //    Amount = Convert.ToDecimal(obj.Amount);
                //}

                BaseClass b = new BaseClass();
                
                ViewBag.IsCheckPrinted = db.PrintChecks.Any(x => x.TransactionId == Id) ? "true" : "false";
                ViewBag.TransactionId = Id;
                ViewBag.dueDate = obj.TransactionDate.Value.ToShortDateString();
                ViewBag.amount = Amount;
                ViewBag.Recipient = obj.Recipient;
                ViewBag.CheckNo = db.PrintChecks.Any(x => x.TransactionId == Id) ? db.PrintChecks.FirstOrDefault(x => x.TransactionId == Id).CheckNo : null;

                string textAmount = b.ChangeNumberToText(Amount.ToString(), 0);
                ViewBag.textAmount = textAmount;

                return View();
            }
            else
            {
                return Json(new { Message = "لا يوجد في هذه العملية حساب بنكي لطباعة الشيك", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
            
        }
        public ActionResult CheckTheCheck(int Id)
        {
            decimal? Amount = 0;
            SharedClass sharedCls = new SharedClass();
            int bankId = sharedCls.IsTransactionHasBankAccount(Id);

            if (bankId > 0)
            {
                foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == Id && x.Credit > 0))
                {
                    int AccountSubId = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == item.AccTreeId).Id;

                    if (db.BankAccounts.Any(x => x.AccountSubId == AccountSubId))
                    {
                        Amount = item.Credit + item.Debit;
                    }
                }

                return Json(new { Message = "1", Title = "تم", Status = "success" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Message = "لا يوجد في هذه العملية حساب بنكي لطباعة الشيك", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult PrintChecsOld(string dueDate, string payNo, string amount, string textAmount, string checkNo, string recipient)
        {
            ViewBag.dueDate = dueDate;
            ViewBag.payNo = payNo;
            ViewBag.amount = amount;
            ViewBag.textAmount = textAmount;
            ViewBag.checkNo = checkNo;
            ViewBag.recipient = recipient;

            //PrintCheck obj = new PrintCheck();

            //obj.TransactionId = Id;
            //obj.Amount = Convert.ToDecimal(amount);
            //obj.CheckNo = checkNo;
            //obj.PayNo = payNo;
            //obj.DueDate = dueDate;
            //obj.Recipient = recipient;

            //db.PrintChecks.Add(obj);
            //db.SaveChanges();


            return View();
        }
        
        public ActionResult PrintChecs(int Id, string dueDate , string payNo , string amount, string textAmount, string checkNo, string recipient)
        {
            var userid = User.Identity.GetUserId();
            PrintCheckObj obj = new PrintCheckObj();

            ViewBag.dueDate = dueDate;
            ViewBag.payNo = payNo;
            ViewBag.amount = amount;
            ViewBag.textAmount = textAmount;
            ViewBag.checkNo = checkNo;
            ViewBag.recipient = recipient;
            int myPayNo = Convert.ToInt32(payNo);

            int res = obj.SaveData(Id, dueDate, myPayNo, amount, textAmount, checkNo, recipient, userid);
            if(res > 0)
            {
                return View();
            }
            else
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة(Exception)", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}