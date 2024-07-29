using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Purchases.Models;
using Purchases.Models.ViewModal;
using System.Data.Entity;
using Microsoft.AspNet.Identity;

namespace Purchases.Controllers

{
    public class BankTransfersController : Controller
    {
        Entities db = new Entities();
        // GET: BankTransfers
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadData()
        {
            var data = db.Transactions.Where(x=>x.IsPosted != true && x.DocumentTypeId == 5).Select(p => new
            {
                Id = p.Id,
                FromAccTree = db.TransactionDetails.FirstOrDefault(f=>f.TransactionId == p.Id & f.Debit > 0).AccountTree.AccName,
                ToAccTree = db.TransactionDetails.FirstOrDefault(f=>f.TransactionId == p.Id & f.Credit > 0).AccountTree.AccName,
                Amount = db.TransactionDetails.FirstOrDefault(f=>f.TransactionId == p.Id & f.Debit > 0).Debit,
                Date = p.TransactionDate.Value.Day + "/" + p.TransactionDate.Value.Month + "/" + p.TransactionDate.Value.Year,
                Note = p.Note??"لا يوجد"
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult Create(BankTransferVM data)
        {
            if(data.FromAccTreeId != data.ToAccTreeId)
            {
                var userid = User.Identity.GetUserId();

                if (ModelState.IsValid)
                {
                    int currencyFrom = (int)db.BankAccounts.FirstOrDefault(d => d.AccountSub.AccTreeId == data.FromAccTreeId).CurrencyTypeId;
                    int currencyTo = (int)db.BankAccounts.FirstOrDefault(d => d.AccountSub.AccTreeId == data.ToAccTreeId).CurrencyTypeId;

                    if(currencyFrom == currencyTo)
                    {
                        Transaction t = new Transaction();
                        t.Amount = data.Amount;
                        t.CurrencyId = currencyFrom;
                        t.DocumentTypeId = 5;
                        t.TransactionDate = data.TransactionDate;
                        t.CreatedDate = DateTime.Now;
                        t.Note = data.Note;
                        t.CreatedBy = userid;
                        t.CreatedDate = DateTime.Now;

                        db.Transactions.Add(t);


                        TransactionDetail tdDebit = new TransactionDetail();
                        TransactionDetail tdCredit = new TransactionDetail();
                        
                        tdDebit.AccTreeId = data.FromAccTreeId;
                        tdDebit.Debit = data.Amount;
                        tdDebit.Credit = 0;
                        tdDebit.TransactionId = t.Id;
                        tdDebit.CreatedBy = userid;
                        tdDebit.CreationDate = DateTime.Now;
                        db.TransactionDetails.Add(tdDebit);
                        
                        tdCredit.AccTreeId = data.ToAccTreeId;
                        tdCredit.Credit = data.Amount;
                        tdCredit.Debit = 0;
                        tdCredit.TransactionId = t.Id;
                        tdCredit.CreatedBy = userid;
                        tdCredit.CreationDate = DateTime.Now;
                        db.TransactionDetails.Add(tdCredit);

                        db.SaveChanges();

                        return Json(new { Message = " تمت عملية التحويل بنجاح ", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { Message = "  عملة الحسابات لا تتطابق ", Title = "تنبيه", Status = "warning" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { Message = " يجب ان لا تتشابهة ارقام الحسابات   ", Title = "تنبيه", Status = "warning" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Message = " عذرا حدث خطأ اثناء عملية الاضافة ", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult Update(BankTransferVM data)
        {
            if (data.FromAccTreeId != data.ToAccTreeId)
            {
                var userid = User.Identity.GetUserId();

                if (ModelState.IsValid)
                {
                    int currencyFrom = (int)db.BankAccounts.FirstOrDefault(d => d.AccountSub.AccTreeId == data.FromAccTreeId).CurrencyTypeId;
                    int currencyTo = (int)db.BankAccounts.FirstOrDefault(d => d.AccountSub.AccTreeId == data.ToAccTreeId).CurrencyTypeId;

                    if (currencyFrom == currencyTo)
                    {
                        Transaction t = db.Transactions.Find(data.Id);
                        t.Amount = data.Amount;
                        t.CurrencyId = currencyFrom;
                        t.DocumentTypeId = 5;
                        t.TransactionDate = data.TransactionDate;
                        t.CreatedDate = DateTime.Now;
                        t.UpdatedBy = userid;
                        t.UpdatingDate = DateTime.Now;
                        t.Note = data.Note;

                        db.Entry(t).State = EntityState.Modified;

                        if (db.TransactionDetails.Any(x => x.TransactionId == data.Id))
                        {
                            var obj = db.TransactionDetails.Where(x => x.TransactionId == data.Id).ToList();
                            db.TransactionDetails.RemoveRange(obj);

                            TransactionDetail tdDebit = new TransactionDetail();
                            TransactionDetail tdCredit = new TransactionDetail();
                            
                            tdDebit.AccTreeId = data.FromAccTreeId;
                            tdDebit.Debit = data.Amount;
                            tdDebit.Credit = 0;
                            tdDebit.TransactionId = t.Id;
                            tdDebit.CreatedBy = userid;
                            tdDebit.CreationDate = DateTime.Now;
                            db.TransactionDetails.Add(tdDebit);

                            tdCredit.AccTreeId = data.ToAccTreeId;
                            tdCredit.Credit = data.Amount;
                            tdCredit.Debit = 0;
                            tdCredit.TransactionId = t.Id;
                            tdCredit.CreatedBy = userid;
                            tdCredit.CreationDate = DateTime.Now;
                            db.TransactionDetails.Add(tdCredit);

                            db.SaveChanges();
                        }

                        return Json(new { Message = " تمت عملية التعديل بنجاح ", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new { Message = "  عملة الحسابات لا تتطابق ", Title = "تنبيه", Status = "warning" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { Message = " يجب ان لا تتشابهة ارقام الحسابات   ", Title = "تنبيه", Status = "warning" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Message = " عذرا حدث خطأ اثناء عملية التعديل ", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
        }
        
        //populat List 
        public ActionResult GetAccTreeId (string q)
        {
            var data = db.BankAccounts.Select(p => new
            {
                id = p.AccountSub.AccTreeId,
                text = p.AccountSub.AccountTree.AccName
            }).Where(d=>d.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //print Transfer to another account 
        public ActionResult PrintData(int Id)
        {
            var data = db.Transactions.Where(x=>x.Id == Id).Select(p => new
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

    }
}