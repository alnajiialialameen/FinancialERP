using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Purchases.Models;
using Purchases.ViewModel;
using Purchases.Models.ViewModal;
using Purchases.MyLogic;
using Microsoft.AspNet.Identity;

namespace Purchases.Controllers
{
    public class CatchReceiptsController : Controller
    {
        Entities db = new Entities();
        // GET: CatchReceipts
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadData()
        {
            var data = db.Transactions.Where(x => x.IsPosted != true & x.DocumentTypeId == 2)
                .Select(p => new
                {
                    TransactionId = p.Id,
                    Currency = p.CurrencyType.Name,
                    DocumentType = p.DocumentType.Name,
                    TotalAmount = p.Amount,
                    ExchangeRate = p.ExchangeRate,
                    TransactionDate = p.TransactionDate.Value.Year + "/" + p.TransactionDate.Value.Month + "/" + p.TransactionDate.Value.Day,
                    Note = p.Note,
                }).OrderByDescending(d => d.TransactionId).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(List<RecieptVM> modal)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                DateTime now = DateTime.Now;
                Transaction t = new Transaction();
                List<TransactionDetail> tdList = new List<TransactionDetail>();
                t.CurrencyId = modal[0].Currency;
                t.TransactionDate = Convert.ToDateTime(modal[0].TransactionDate);
                t.CreatedDate = now;
                t.Note = modal[0].CreditNote;
                t.DocumentTypeId = 2;
                t.Amount = modal.Sum(x => x.CreditAmount);
                t.CreatedBy = userid;
                t.CreationDate = DateTime.Now;
                int balanceAccTreeId = modal[0].BalanceAccTreeId;

                foreach (var data in modal)
                {
                    TransactionDetail td = new TransactionDetail();
                    td.TransactionId = t.Id;
                    if (data.CreditAccTreeId != 0)
                    {
                        td.AccTreeId = data.CreditAccTreeId;
                        td.Credit = data.CreditAmount;
                        td.BalanceId = data.BalanceAccTreeId;
                        td.Debit = 0;
                        td.Note = data.CreditNote;
                        td.CreatedBy = userid;
                        td.CreationDate = DateTime.Now;
                    }
                    else
                    {
                        td.AccTreeId = data.DebitAccTreeId;
                        td.Debit = modal.Sum(x => x.CreditAmount);
                        td.BalanceId = data.BalanceAccTreeId;
                        td.Credit = 0;
                        td.Note = data.DebitNote;
                        td.CreatedBy = userid;
                        td.CreationDate = DateTime.Now;
                    }

                    tdList.Add(td);
                }

                decimal newAmount = Convert.ToDecimal(t.Amount);
                int balanceId = Convert.ToInt32(balanceAccTreeId);
                int res = myExtention.UpdateActualExchange(balanceId, 0, newAmount, userid);

                db.Transactions.Add(t);
                db.TransactionDetails.AddRange(tdList);
                db.SaveChanges();

                return Json(new { Message = " تمت عملية الحجفظ بنجاح ", Title = "نجاح", Status = "success" });
                //if (res > 0)
                //{
                //    db.Transactions.Add(t);
                //    db.TransactionDetails.AddRange(tdList);
                //    db.SaveChanges();

                //    return Json(new { Message = " تمت عملية الحجفظ بنجاح ", Title = "نجاح", Status = "success" });
                //}

                //return Json(new { Message = " عفوا لم تتم عملية الحفظ بنجاح ", Title = "خطأ", Status = "error" });
            }catch(Exception e)
            {
                return Json(new { Message = "خطأ في عملية الحفظ(Exception)", Title = "خطأ", Status = "error" });
            }
        }
        
        public ActionResult PrintChecs()
        {
            return View();
        }
        //populate list

        public ActionResult GetBankAccount(string q)
        {
            var data = db.BankAccounts.Select(f => new
            {
                id = f.AccountSub.AccTreeId,
                text = f.AccountSub.AccountTree.AccName
            }).Where(p => p.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSubAccount(string q)
        {
            var data = db.AccountSubs.Select(f => new
            {
                id = f.AccTreeId,
                text = f.AccountTree.AccName
            }).Where(p => p.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }
    }
}