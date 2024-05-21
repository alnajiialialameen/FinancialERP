using Purchases.Models;
using Purchases.Models.ViewModal;
using Purchases.MyLogic;
using SACLERP.CurencyOperation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class RecieptsController : Controller
    {
        Entities db = new Entities();
        // GET: Reciepts
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadDataOld()
        {
            var data = db.Transactions.Select(p => new
            {
                Id = p.Id,
                FromAccTree = db.TransactionDetails.FirstOrDefault(f => f.TransactionId == p.Id & f.Debit > 0).AccountTree.AccName,
                ToAccTree = db.TransactionDetails.FirstOrDefault(f => f.TransactionId == p.Id & f.Credit > 0).AccountTree.AccName,
                Amount = db.TransactionDetails.FirstOrDefault(f => f.TransactionId == p.Id & f.Debit > 0).Debit,
                Date = p.TransactionDate.Value.Day + "/" + p.TransactionDate.Value.Month + "/" + p.TransactionDate.Value.Year,
                Note = p.Note ?? "لا يوجد"
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadData()
        {
            var data = db.Transactions.Where(x => x.IsPosted != true & x.DocumentTypeId == 3)
                .Select(p => new
                {
                    TransactionId = p.Id,
                    Currency = p.CurrencyType.Name,
                    DocumentType = p.DocumentType.Name,
                    TotalAmount = p.Amount,
                    ExchangeRate = p.ExchangeRate,
                    TransactionDate = p.TransactionDate.Value.Year + "/" + p.TransactionDate.Value.Month + "/" + p.TransactionDate.Value.Day,
                    Note = p.Note,
                    //HasAddedTax = p.HasAddedTax == true?  "مضمن 17%" : "غير مضمنة",
                    HasAddedTax = p.HasAddedTax,
                    HasTax = p.HasTax,

                }).OrderByDescending(d=>d.TransactionId).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create(List<RecieptVM> modal)
        {
            DateTime now = DateTime.Now;
            Transaction t = new Transaction();
            List<TransactionDetail> tdList = new List<TransactionDetail>();
            t.CurrencyId = modal[0].Currency;
            t.TransactionDate = Convert.ToDateTime(modal[0].TransactionDate);
            t.CreatedDate = now;
            t.Note = modal[0].CreditNote;
            t.DocumentTypeId = 3;
            t.Amount = modal.Sum(x => x.DebitAmount);
            t.Recipient = modal[0].Recipient;

            foreach (var data in modal)
            {
                TransactionDetail td = new TransactionDetail();
                td.TransactionId = t.Id;
                if (data.CreditAccTreeId != 0)
                {
                    td.AccTreeId = data.CreditAccTreeId;
                    td.Credit = modal.Sum(x => x.DebitAmount);
                    td.BalanceId = data.BalanceAccTreeId;
                    td.Debit = 0;
                    td.Note = data.CreditNote;

                    int type = data.CreditAmount > 0 ? 1 : 0;

                    decimal newAmount = Convert.ToDecimal(td.Credit);
                    int balanceId = Convert.ToInt32(data.BalanceAccTreeId);
                    myExtention.UpdateActualExchange(balanceId, 0, newAmount, type);
                }
                else
                {
                    td.AccTreeId = data.DebitAccTreeId;
                    td.Debit = data.DebitAmount;
                    td.BalanceId = data.BalanceAccTreeId;
                    td.Credit = 0;
                    td.Note = data.DebitNote;

                    int type = data.CreditAmount > 0 ? 1 : 0;

                    decimal newAmount = Convert.ToDecimal(data.DebitAmount + data.CreditAmount);
                    int balanceId = Convert.ToInt32(data.BalanceAccTreeId);
                    myExtention.UpdateActualExchange(balanceId, 0, newAmount, type);
                }

                tdList.Add(td);
            }

            db.Transactions.Add(t);
            db.TransactionDetails.AddRange(tdList);
            db.SaveChanges();

            return Json(new { Message = " تمت عملية التحويل بنجاح ", Title = "نجاح", Status = "success" });
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
        
        public ActionResult printOrnik17(int Id)
        {
            var objTrans = db.Transactions.Find(Id);
            var objDet = db.TransactionDetails.FirstOrDefault(x => x.TransactionId == Id);
            int BalanceId = Convert.ToInt32(objDet.BalanceId);

            string currentYear = db.FinancialCycles.FirstOrDefault().Year;
            string BalanceName = db.Balances.Find(BalanceId).AccountTree.AccName;
            decimal Amount = Convert.ToDecimal(objTrans.Amount);
            decimal Tax = objTrans.HasTax == true? Amount * Convert.ToDecimal(0.01) : 0;
            decimal Total = Amount - Tax;


            //return Json(new { currentYear = currentYear, BalanceName = BalanceName, Amount = Amount, Tax = Tax, Total = Total }, JsonRequestBehavior.AllowGet);
            ViewBag.currentYear = currentYear;
            ViewBag.Note = objTrans.Note;
            ViewBag.BalanceName = BalanceName;
            ViewBag.Amount = Amount;
            ViewBag.Tax = Tax;
            ViewBag.Total = Total;
            ViewBag.SelectedDate = DateTime.Today.ToShortDateString();

            List<TransactionViewMode> data = new List<TransactionViewMode>();

            foreach(var item in db.TransactionDetails.Where(x=>x.TransactionId == Id && x.Debit > 0 & x.BalanceId != null))
            {
                TransactionViewMode obj = new TransactionViewMode();
                
                data.Add(new TransactionViewMode()
                {
                    transactionId = Convert.ToInt32(item.TransactionId),
                    accName = item.AccountTree.AccName,
                    balanceAccName = item.BalanceId != null? item.Balance.AccountTree.AccName : "لا توجد",
                    amount = item.Credit + item.Debit,
                    tax = item.Transaction.HasTax == true ? (item.Credit + item.Debit) * (decimal)0.01 : 0,
                    total = (item.Credit + item.Debit) - (item.Transaction.HasTax == true ? (item.Credit + item.Debit) * (decimal)0.01 : 0),
                    transDate = item.Transaction.TransactionDate != null ? item.Transaction.TransactionDate.Value.ToShortDateString() : "",
                    note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل",
                    recipient = item.Transaction.Recipient,
                });
            }

            if (Convert.ToBoolean(objTrans.HasAddedTax))
            {
                data.Add(new TransactionViewMode()
                {
                    transactionId = Id,
                    balanceAccName = data[0].balanceAccName,
                    note = "ضريبة القيمة المضافة (17%)",
                    amount = data.Sum(x=>x.amount) * (decimal)0.17,
                    tax = 0,
                    total = data.Sum(x => x.amount) * (decimal)0.17,
                    recipient = data[0].recipient,
                });
            }


            BaseClass b = new BaseClass();

            string textAmount = b.ChangeNumberToText(data.Sum(x=>x.total).ToString(), 0);
            ViewBag.AmountText = textAmount;

            return View(data);
        }

    }
}