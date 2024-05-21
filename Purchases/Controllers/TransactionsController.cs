using Purchases.Models;
using Purchases.Models.ViewModal;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Purchases.MyLogic;

namespace Purchases.Controllers
{
    public class TransactionsController : Controller
    {
        private Entities db = new Entities();

        // GET: Transactions
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PostedTransactions()
        {
            return View();
        }

        public ActionResult LoadDataAll()
        {
            var data = db.Transactions
                .Select(p => new
                {
                    TransactionId = p.Id,
                    Currency = p.CurrencyType.Name,
                    DocumentType = p.DocumentType.Name,
                    TotalAmount = p.Amount,
                    ExchangeRate = p.ExchangeRate,
                    TransactionDate = p.TransactionDate.Value.Year + "/" + p.TransactionDate.Value.Month + "/" + p.TransactionDate.Value.Day,
                    Note = p.Note,

                }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult LoadData()
        {
            var data = db.Transactions.Where(x => x.IsPosted != true & x.DocumentTypeId == 1)
                .Select(p => new
                {
                    TransactionId = p.Id,
                    Currency = p.CurrencyType.Name,
                    DocumentType = p.DocumentType.Name,
                    TotalAmount = p.Amount,
                    ExchangeRate = p.ExchangeRate,
                    TransactionDate = p.TransactionDate.Value.Year + "/" + p.TransactionDate.Value.Month + "/" + p.TransactionDate.Value.Day  ,
                    Note = p.Note,
                }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult LoadDataPosted()
        {
            var data = db.Transactions.Where(x => x.IsPosted == true)
                .Select(p => new
                {
                    TransactionId = p.Id,
                    Currency = p.CurrencyType.Name,
                    DocumentType = p.DocumentType.Name,
                    TotalAmount = p.Amount,
                    ExchangeRate = p.ExchangeRate,
                    TransactionDate = p.TransactionDate.Value.Year + "/" + p.TransactionDate.Value.Month + "/" + p.TransactionDate.Value.Day,
                    Note = p.Note,
                }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult getAccTrees(string q)
        {
            var data = db.AccountTrees.Where(x => db.AccountSubs.Any(s => s.AccTreeId == x.Id))
                        .Select(p => new { id = p.Id, text = p.AccName }).Where(f => f.text.Contains(q)).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getBalanceIds(string q)
        {
            var data = db.Balances.Where(x => db.AccountTrees.Any(s => s.Id == x.AccountTreeId))
                        .Select(p => new { id = p.Id, text = p.AccountTree.AccName }).Where(f => f.text.Contains(q)).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getCurrency(string q)
        {
            var data = db.CurrencyTypes
                        .Select(p => new { id = p.Id, text = p.Name }).Where(f => f.text.Contains(q)).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult Create(List<TransactionViewMode> data)
        {
            //var userId = User.Identity.getUserId();
            try
            {
                if (data.Sum(x => x.credit) == data.Sum(x => x.debit))
                {
                    Transaction jou = new Transaction();

                    jou.DocumentTypeId = 1;
                    jou.CurrencyId = data.FirstOrDefault().currencyId;
                    jou.Amount = data.Sum(x => x.credit);
                    jou.TransactionDate = data.FirstOrDefault().transactionDate;
                    jou.ExchangeRate = data.FirstOrDefault().exchangeRate;
                    jou.Note = data.FirstOrDefault().note;
                    jou.IsPosted = false;
                    jou.CreatedDate = DateTime.Today;

                    db.Transactions.Add(jou);
                    db.SaveChanges();

                    List<TransactionDetail> jouDetList = new List<TransactionDetail>();
                    foreach (var item in data)
                    {
                        TransactionDetail jouDet = new TransactionDetail();

                        jouDet.AccTreeId = item.accTrreId;
                        jouDet.TransactionId = jou.Id;
                        jouDet.Debit = item.debit;
                        jouDet.Credit = item.credit;
                        jouDet.Note = item.note;
                        jouDet.BalanceId = item.balanceId;
                        int type = item.credit > 0 ? 1 : 0;

                        decimal newAmount = Convert.ToDecimal(item.debit + item.credit);
                        int balanceId = Convert.ToInt32(item.balanceId);
                        myExtention.UpdateActualExchange(balanceId, 0, newAmount, type);

                        jouDetList.Add(jouDet);
                    }

                    db.TransactionDetails.AddRange(jouDetList);
                    db.SaveChanges();

                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "يجب أن يتساوي الجانب المدين مع الجانب الدائن", Title = "خطأ", Status = "error" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }

        [HttpPost]
        public ActionResult Posting(int Id)
        {
            try
            {
                Transaction transaction = db.Transactions.Find(Id);

                transaction.IsPosted = true;

                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { Message = "تمت عملية الترحيل  بنجاح", Title = "نجاح", Status = "success" });
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الترحيل", Title = "خطأ", Status = "error" });
            }
        }

        [HttpPost]
        public ActionResult Add17Tax(int Id, bool HasAddedTax)
        {
            try
            {
                Transaction transaction = db.Transactions.Find(Id);

                transaction.HasAddedTax = HasAddedTax;

                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { Message = "تمت عملية الترحيل  بنجاح", Title = "نجاح", Status = "success" });
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الترحيل", Title = "خطأ", Status = "error" });
            }
        }
        
        [HttpPost]
        public ActionResult AddTax(int Id, bool HasTax)
        {
            try
            {
                Transaction transaction = db.Transactions.Find(Id);

                transaction.HasTax = HasTax;

                if (HasTax)
                {
                    TransactionDetail transDet = new TransactionDetail();
                    transDet.TransactionId = Id;
                    //transDet.BalanceId = ;
                    transDet.AccTreeId = 228;
                    transDet.Debit = transaction.Amount * (decimal)0.01;
                    transDet.Credit = 0;
                    transDet.Note = transaction.Note;

                    db.TransactionDetails.Add(transDet);
                }
                else
                {
                    var taxObj = db.TransactionDetails.Where(x => x.TransactionId == Id & x.AccTreeId == 228).FirstOrDefault();

                    db.TransactionDetails.Remove(taxObj);
                }

                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { Message = "تمت عملية الترحيل  بنجاح", Title = "نجاح", Status = "success" });
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الترحيل", Title = "خطأ", Status = "error" });
            }
        }
        
        public ActionResult Edit(int Id)
        {
            var trans = db.Transactions.Find(Id);
            ViewBag.TransactionId = Id;

            string TransactionDate = trans.TransactionDate != null? trans.TransactionDate.Value.Date.ToString("dd/MM/yyyy"):"غير مدخل";
            
            ViewBag.TransactionDate = TransactionDate;
            ViewBag.Note = trans.Note != null? trans.Note :"غير مدخل";
            ViewBag.CurrencyType = trans.CurrencyType.Name != null? trans.CurrencyType.Name : "غير مدخل";
            ViewBag.ExchangeRate = trans.ExchangeRate != null? trans.ExchangeRate.ToString() : "غير مدخل";
            ViewBag.Note = trans.Note != null ? trans.Note :"غير مدخل";


            return View(trans);
        }

        public ActionResult getDetailData(int Id)
        {
            List<TransactionViewMode> data = new List<TransactionViewMode>();
            foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == Id).ToList())
            {
                TransactionViewMode obj = new TransactionViewMode();

                obj.id = item.Id;
                obj.accTrreId = item.AccTreeId;
                obj.balanceId = item.BalanceId;
                obj.balanceAccName = item.Balance.AccountTree.AccName;
                obj.accName = item.AccountTree.AccName;
                obj.credit = item.Credit;
                obj.debit = item.Debit;
                obj.transDate = item.Transaction.TransactionDate != null ? item.Transaction.TransactionDate.Value.ToShortDateString() : "غير مدخل";
                obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

                data.Add(obj);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult Detail(int Id)
        {
            if (db.TransactionDetails.Any(x => x.AccTreeId == Id))
            {
                var data = new List<TransactionViewMode>();
                var trans = db.TransactionDetails.Where(x => x.AccTreeId == Id).ToList();
                var accTree = db.AccountTrees.Find(Id);

                var dList = db.TransactionDetails.Where(x => x.AccTreeId == Id).Select(x => x.TransactionId).ToList();

                var lastFivetrans = db.Transactions.Where(x => dList.Contains(x.Id)).OrderByDescending(s => s.Id).Take(5)
                    .Select(x => new TransVM()
                    {
                        transactionId = x.Id,
                        CurrencyType = x.CurrencyType.Name,
                        Amount = x.Amount,
                        DocumentType = x.DocumentType.Name,
                        ExchangeRate = x.ExchangeRate,
                        TransactionDate = x.TransactionDate
                    })
                    .ToList();

                foreach (var item1 in dList)
                {
                    foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == item1))
                    {
                        TransactionViewMode obj = new TransactionViewMode();

                        obj.transactionId = Convert.ToInt32(item.TransactionId);
                        obj.accName = item.AccountTree.AccName;
                        obj.credit = item.Credit;
                        obj.debit = item.Debit;
                        obj.transDate = item.Transaction.TransactionDate != null ? item.Transaction.TransactionDate.Value.ToShortDateString() : "";
                        obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

                        data.Add(obj);
                    }
                }

                var myObj = new AllViewMode()
                {
                    accTreeId = Id,
                    accName = accTree.AccName,
                    accLevel = "المستوي " + accTree.TheLevel.Value,
                    accParent = accTree.AccParent > 0 ? db.AccountTrees.Find(accTree.AccParent).AccName : "هو حساب رئيسي",
                    accCode = accTree.AccCode,
                    accType = accTree.AccountType.Name,
                    accNature = accTree.AccountNature.Name,
                    accFinal = accTree.AccountFinal.Name,

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
                
                AllViewMode myObj = new AllViewMode();

                myObj.accTreeId = Id;
                myObj.accName = accTree.AccName;
                myObj.accLevel = "المستوي " + accTree.TheLevel.Value;
                myObj.accParent = accTree.AccParent > 0 ? db.AccountTrees.Find(accTree.AccParent).AccName : "هو حساب رئيسي";
                myObj.accCode = accTree.AccCode;
                myObj.accType = accTree.AccountType.Name;
                myObj.accNature = accTree.AccountNature.Name;
                myObj.accFinal = accTree.AccountFinal.Name;

                return View(myObj);
            }
        }
        
        public ActionResult getTransDetailsData(int Id)
        {
            List<TransactionViewMode> data = new List<TransactionViewMode>();
            foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == Id))
            {
                TransactionViewMode obj = new TransactionViewMode();

                obj.id = Convert.ToInt32(item.Id);
                obj.transactionId = Convert.ToInt32(item.TransactionId);
                obj.accName = item.AccountTree.AccName;
                obj.credit = item.Credit;
                obj.debit = item.Debit;
                obj.transDate = item.Transaction.TransactionDate != null ? item.Transaction.TransactionDate.Value.ToShortDateString() : "غير مدخل";
                obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

                data.Add(obj);
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Update(List<TransactionViewMode> data)
        {
            try
            {
                if (data.Sum(x => x.credit) == data.Sum(x => x.debit))
                {
                    //Firstly update Transaction data 
                    var transactionId = data.FirstOrDefault().transactionId;
                    var currencyId = data.FirstOrDefault().currencyId;
                    var transactionDate1 = data.FirstOrDefault().transactionDate;
                    DateTime transactionDate = Convert.ToDateTime(transactionDate1);
                    var ExchangeRate = db.CurrencyDetails.FirstOrDefault(x => x.CurrencyTypeId == currencyId & x.Month == transactionDate.Month & x.Year == transactionDate.Year).ExchangeRate;
                    var transObj = db.Transactions.Find(transactionId);

                    transObj.DocumentTypeId = 1;
                    transObj.CurrencyId = data.FirstOrDefault().currencyId;
                    transObj.Amount = data.Sum(x => x.credit);
                    transObj.TransactionDate = transactionDate;
                    transObj.ExchangeRate = ExchangeRate;
                    transObj.Note = data.FirstOrDefault().note;
                    transObj.IsPosted = false;
                    transObj.CreatedDate = DateTime.Today;

                    db.Entry(transObj).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    //update Transaction Details data 
                    foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == transactionId).ToList())
                    {
                        if (data.Any(x => x.accTrreId == item.AccTreeId))//update
                        {
                            var obj = data.FirstOrDefault(x => x.accTrreId == item.AccTreeId);
                            item.AccTreeId = obj.accTrreId;
                            item.BalanceId = obj.balanceId;
                            item.Credit = obj.credit;
                            item.Debit = obj.debit;
                            item.TransactionId = transactionId;
                            item.Note = obj.note;

                            int type = obj.credit > 0 ? 1 : 0;

                            decimal oldAmount = Convert.ToDecimal(item.Debit + item.Credit);
                            int oldBalanceId = Convert.ToInt32(item.BalanceId);
                            myExtention.UpdateActualExchange(oldBalanceId, oldAmount, 0, type);

                            decimal newAmount = Convert.ToDecimal(obj.debit + obj.credit);
                            int newBalanceId = Convert.ToInt32(obj.balanceId);
                            myExtention.UpdateActualExchange(newBalanceId, 0, newAmount, type);

                            db.Entry(item).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        //اذا البند ده موجود في قاعدة البيانات و ما موجود في البيانات المدخلة يبقى المستخدم عمل ليهو حذف في الشاشة لكن فعليا ما اتحذف من قاعدة البيانات
                        else if (!data.Any(x => x.accTrreId == item.AccTreeId)) //Delete
                        {

                            int type = item.Credit > 0 ? 1 : 0;

                            decimal oldAmount = Convert.ToDecimal(item.Debit + item.Credit);
                            int oldBalanceId = Convert.ToInt32(item.BalanceId);
                            myExtention.UpdateActualExchange(oldBalanceId, oldAmount, 0, type);

                            db.TransactionDetails.Remove(item);
                            db.SaveChanges();
                        }
                    }

                    //ده لو البند تم ادخاله في عملية التعديل ولم يكن موجود مسبقا
                    foreach(var item in data)
                    {
                        if(!db.TransactionDetails.Any(x=>x.TransactionId == transactionId & x.AccTreeId == item.accTrreId))
                        {
                            TransactionDetail transDet = new TransactionDetail();
                            transDet.AccTreeId = item.accTrreId;
                            transDet.BalanceId = item.balanceId;
                            transDet.Credit = item.credit;
                            transDet.Debit = item.debit;
                            transDet.TransactionId = transactionId;
                            transDet.Note = item.note;

                            int type = item.credit > 0 ? 1 : 0;

                            decimal Amount = Convert.ToDecimal(item.debit + item.credit);
                            int BalanceId = Convert.ToInt32(item.balanceId);
                            myExtention.UpdateActualExchange(BalanceId, 0, Amount, type);
                            
                            db.TransactionDetails.Add(transDet);
                            db.SaveChanges();
                        }
                    }

                    return Json(new { Message = "تمت عملية التعديل  بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "يجب أن يتساوي الجانب المدين مع الجانب الدائن", Title = "خطأ", Status = "error" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية التعديل", Title = "خطأ", Status = "error" });
            }
        }
        
        public ActionResult PrintCheck()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DeleteTransactionDetails(int Id)
        {
            try
            {
                var transDetObj = db.TransactionDetails.Find(Id);
                var balanceId = Convert.ToInt32(transDetObj.BalanceId);
                var Credit = transDetObj.Credit;
                var Debit = transDetObj.Debit;
                
                int type = transDetObj.Credit > 0 ? 1 : 0;

                var oldAmount = Convert.ToDecimal(Credit + Debit);
                myExtention.UpdateActualExchange(balanceId, oldAmount, 0, type);

                db.TransactionDetails.Remove(transDetObj);
                db.SaveChanges();

                return Json(1, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
        }
        
        /*------------------------------------------------------------------------------------*/

        public ActionResult printAllData()
        {
            List<TransactionViewMode> data = new List<TransactionViewMode>();
            foreach (var item in db.TransactionDetails.ToList())
            {
                TransactionViewMode obj = new TransactionViewMode();

                obj.transactionId = Convert.ToInt32(item.TransactionId);
                obj.accName = item.AccountTree.AccName;
                obj.credit = item.Credit;
                obj.debit = item.Debit;
                obj.transDate = item.Transaction.TransactionDate != null ? item.Transaction.TransactionDate.Value.ToShortDateString() : "";
                obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

                data.Add(obj);
            }
            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = DateTime.Today.ToShortDateString();

            return View(data);
        }

        public ActionResult printData(int Id)
        {
            List<TransactionViewMode> data = new List<TransactionViewMode>();
            var dList = db.TransactionDetails.Where(x => x.AccTreeId == Id).Select(x=>x.TransactionId).ToList();
            foreach (var item1 in dList)
            {
                foreach(var item in db.TransactionDetails.Where(x=>x.TransactionId == item1))
                {
                    TransactionViewMode obj = new TransactionViewMode();

                    obj.transactionId = Convert.ToInt32(item.TransactionId);
                    obj.accName = item.AccountTree.AccName;
                    obj.credit = item.Credit;
                    obj.debit = item.Debit;
                    obj.transDate = item.Transaction.TransactionDate != null? item.Transaction.TransactionDate.Value.ToShortDateString():"";
                    obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

                    data.Add(obj);
                }
            }
            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = DateTime.Today.ToShortDateString();

            return View(data);
        }
        
        public ActionResult printTransData(int transId)
        {
            List<TransactionViewMode> data = new List<TransactionViewMode>();
            foreach (var item1 in db.TransactionDetails.Where(x => x.TransactionId == transId).ToList())
            {
                foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == item1.TransactionId))
                {
                    TransactionViewMode obj = new TransactionViewMode();

                    obj.transactionId = Convert.ToInt32(item.TransactionId);
                    obj.accName = item.AccountTree.AccName;
                    obj.credit = item.Credit;
                    obj.debit = item.Debit;
                    obj.transDate = item.Transaction.TransactionDate != null ? item.Transaction.TransactionDate.Value.ToShortDateString() : "";
                    obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

                    data.Add(obj);
                }
            }
            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = DateTime.Today.ToShortDateString();

            return View(data);
        }
        
        public ActionResult printDatas(DateTime myDate)
        {
            List<TransactionViewMode> data = new List<TransactionViewMode>();
            foreach (var item1 in db.Transactions.ToList())
            {
                if(Convert.ToDateTime(item1.TransactionDate).Date == myDate.Date)
                {
                    foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == item1.Id))
                    {
                        TransactionViewMode obj = new TransactionViewMode();

                        obj.transactionId = Convert.ToInt32(item.TransactionId);
                        obj.accName = item.AccountTree.AccName;
                        obj.credit = item.Credit;
                        obj.debit = item.Debit;
                        obj.transactionDate = item.Transaction.TransactionDate;
                        obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";
                        
                        data.Add(obj);
                    }
                }
            }
            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = myDate.ToShortDateString();

            return View(data);
        }

        public ActionResult printLedger(int Id)
        {
            List<TransactionViewMode> data = new List<TransactionViewMode>();
            string AccName = db.AccountTrees.Find(Id).AccName;
            ViewBag.AccName = AccName;

            var dList = db.TransactionDetails.Where(x => x.AccTreeId == Id).Select(x => x.TransactionId).ToList();
            foreach (var item1 in dList)
            {
                foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == item1 & x.AccTreeId != Id))
                {
                    TransactionViewMode obj = new TransactionViewMode();

                    obj.transactionId = Convert.ToInt32(item.TransactionId);
                    obj.accName = item.AccountTree.AccName;
                    obj.credit = item.Credit;
                    obj.debit = item.Debit;
                    obj.transDate = item.Transaction.TransactionDate != null ? item.Transaction.TransactionDate.Value.ToShortDateString() : "";
                    obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

                    data.Add(obj);
                }
            }
            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = DateTime.Today.ToShortDateString();

            return View(data);
        }

        public ActionResult printOrnik17()
        {
            //List<TransactionViewMode> data = new List<TransactionViewMode>();
            //string AccName = db.AccountTrees.Find(Id).AccName;
            //ViewBag.AccName = AccName;

            //var dList = db.TransactionDetails.Where(x => x.AccTreeId == Id).Select(x => x.TransactionId).ToList();
            //foreach (var item1 in dList)
            //{
            //    foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == item1 & x.AccTreeId != Id))
            //    {
            //        TransactionViewMode obj = new TransactionViewMode();

            //        obj.transactionId = Convert.ToInt32(item.TransactionId);
            //        obj.accName = item.AccountTree.AccName;
            //        obj.credit = item.Credit;
            //        obj.debit = item.Debit;
            //        obj.transDate = item.Transaction.TransactionDate != null ? item.Transaction.TransactionDate.Value.ToShortDateString() : "";
            //        obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

            //        data.Add(obj);
            //    }
            //}
            //ViewBag.SumOfAmount = data.Sum(x => x.credit);
            //ViewBag.SelectedDate = DateTime.Today.ToShortDateString();

            return View();
        }
        
    }
}