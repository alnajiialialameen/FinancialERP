using Microsoft.AspNet.Identity;
using Purchases.Models;
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
            Transaction obj = db.Transactions.Find(Id);

            decimal Amount = 0;
            if (obj.HasTax == true & obj.HasAddedTax == true)
            {
                Amount = Convert.ToDecimal(obj.Amount) - Convert.ToDecimal(obj.Amount * (decimal)0.01) + Convert.ToDecimal(obj.Amount * (decimal)0.17);
            }
            else if (obj.HasTax == false & obj.HasAddedTax == true)
            {
                Amount = Convert.ToDecimal(obj.Amount) + Convert.ToDecimal(obj.Amount * (decimal)0.17);
            }
            else if (obj.HasTax == true & obj.HasAddedTax == false)
            {
                Amount = Convert.ToDecimal(obj.Amount) - Convert.ToDecimal(obj.Amount * (decimal)0.01);
            }
            else
            {
                Amount = Convert.ToDecimal(obj.Amount);
            }

            ViewBag.IsCheckPrinted = db.PrintChecks.Any(x => x.TransactionId == Id)? "true" : "false";
            ViewBag.TransactionId = Id;
            ViewBag.dueDate = obj.TransactionDate.Value.ToShortDateString();
            ViewBag.amount = Amount;
            ViewBag.Recipient = obj.Recipient;

            return View();
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

            ViewBag.dueDate = dueDate;
            ViewBag.payNo = payNo;
            ViewBag.amount = amount;
            ViewBag.textAmount = textAmount;
            ViewBag.checkNo = checkNo;
            ViewBag.recipient = recipient;

            Transaction tarnsObj = db.Transactions.Find(Id);
            int AccountSubsId = (int)tarnsObj.TransactionDetails.FirstOrDefault(x => x.Credit > 0).AccountTree.AccountSubs.FirstOrDefault().Id;
            var bankObj = db.BankAccounts.FirstOrDefault(x => x.AccountSubId == AccountSubsId);

            if (!db.PrintChecks.Any(x => x.TransactionId == Id))
            {
                PrintCheck obj = new PrintCheck();

                obj.TransactionId = Id;
                obj.Amount = Convert.ToDecimal(amount);
                obj.CheckNo = Convert.ToInt32(checkNo);
                obj.PayNo = Convert.ToInt32(payNo);
                obj.DueDate = Convert.ToDateTime(dueDate);
                obj.Recipient = recipient;
                obj.BankAccountId = bankObj.Id;
                obj.CreatedBy = userid;
                obj.CreationDate = DateTime.Now;

                db.PrintChecks.Add(obj);
                db.SaveChanges();

                //هنا مفترض نعمل ترحيل طوالي عشان ما تاني يدخلوا ليهو اي تعديل بعد ما يطلع الشيك
                tarnsObj.Recipient = recipient;
                tarnsObj.UpdatedBy = userid;
                tarnsObj.UpdatingDate = DateTime.Now;
                db.Entry(tarnsObj).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                PrintCheck obj = db.PrintChecks.FirstOrDefault(x=>x.TransactionId == Id);

                obj.TransactionId = Id;
                obj.Amount = Convert.ToDecimal(amount);
                obj.CheckNo = Convert.ToInt32(checkNo);
                obj.PayNo = Convert.ToInt32(payNo);
                obj.DueDate = Convert.ToDateTime(dueDate);
                obj.Recipient = recipient;
                obj.BankAccountId = bankObj.Id;
                obj.UpdatedBy = userid;
                obj.UpdatingDate = DateTime.Now;
                
                //هنا مفترض نعمل ترحيل طوالي عشان ما تاني يدخلوا ليهو اي تعديل بعد ما يطلع الشيك
                tarnsObj.Recipient = recipient;
                tarnsObj.UpdatedBy = userid;
                tarnsObj.UpdatingDate = DateTime.Now;
                db.Entry(obj).State = EntityState.Modified;
                db.Entry(tarnsObj).State = EntityState.Modified;
                db.SaveChanges();
            }

            return View();
        }
    }
}