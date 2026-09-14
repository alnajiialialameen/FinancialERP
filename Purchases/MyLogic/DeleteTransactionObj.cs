using Purchases.Models;
using Purchases.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Purchases.MyLogic
{
    public class DeleteTransactionObj
    {
        private readonly Entities db = new Entities();

        public int DeleteTransactionOld(int transId, string userid)
        {
            try
            {
                var data = db.TransactionDetails
                            .Where(q => q.TransactionId == transId)
                            .Include(q => q.Transaction)
                            .ToList();

                if (data == null)
                {
                    return -100; // data not found
                }

                var Item = new DeletedTransaction
                {
                    Amount = data[0].Transaction.Amount,
                    CurrencyId = data[0].Transaction.CurrencyId,
                    DocumentTypeId = data[0].Transaction.DocumentTypeId,
                    ExchangeRate = data[0].Transaction.ExchangeRate,
                    FinancialCycleId = data[0].Transaction.FinancialCycleId,
                    HasAddedTax = data[0].Transaction.HasAddedTax,
                    HasTax = data[0].Transaction.HasTax,
                    IsPosted = data[0].Transaction.IsPosted,
                    Recipient = data[0].Transaction.Recipient,
                    Note = data[0].Transaction.Note,
                    TransactionDate = data[0].Transaction.TransactionDate,
                    TransactionId = data[0].TransactionId,
                    CreatedBy = userid,
                    CreationDate = DateTime.Now
                };

                db.DeletedTransactions.Add(Item);
                db.SaveChanges();

                var itemDetailsList = new List<DeletedTransactionDetail>();

                // delete Transaction Details Objects
                foreach (var obj in data)
                {

                    var ItemDetails = new DeletedTransactionDetail
                    {
                        BalanceId = obj.BalanceId,
                        AccTreeId = obj.AccTreeId,
                        Debit = obj.Debit,
                        Credit = obj.Credit,
                        Note = obj.Note,
                        TransactionDetailId = obj.Id,
                        DeletedTransactionId = Item.Id,
                        CreatedBy = userid,
                        CreationDate = DateTime.Now
                    };
                    // Update Balance Object
                    if (obj.BalanceId > 0)
                    {
                        int BalanceId = Convert.ToInt32(obj.BalanceId);

                        var oldAmount = Convert.ToDecimal(obj.Credit + obj.Debit);

                        int type = obj.Credit > 0 ? 1 : 0;

                        // بعد حذف العنصر تعدل في الموانه
                        myExtention.UpdateActualExchange(BalanceId, oldAmount, 0, type, userid);
                        // myExtention.UpdateActualExchange(BalanceId, oldAmount, 0, userid);
                    }
                    // add object to list for removing all of them together
                    itemDetailsList.Add(ItemDetails);

                }
                var transObj = data[0].Transaction;

                // delete Transaction Details List
                db.TransactionDetails.RemoveRange(data);

                // delete Transaction Object

                if (transObj.PrintChecks.Any())
                {
                    db.PrintChecks.Remove(transObj.PrintChecks.FirstOrDefault(x => x.TransactionId == transObj.Id));
                }
                db.Transactions.Remove(transObj);
                db.SaveChanges();

                return 1;
            }
            catch
            {
                return -1;
            }

        }

        public int DeleteTransaction(int transId, string userid)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var data = db.TransactionDetails
                                .Where(q => q.TransactionId == transId)
                                .Include(q => q.Transaction)
                                .ToList();

                    if (!data.Any())
                    {
                        return -100; // data not found
                    }

                    var firstTransactionDetail = data.FirstOrDefault();
                    if (firstTransactionDetail?.Transaction == null)
                    {
                        return -100; // transaction not found
                    }

                    var transObj = firstTransactionDetail.Transaction;
                    int? checkNo = 0;

                    // حذف PrintChecks إن وجد
                    if (transObj.PrintChecks != null && transObj.PrintChecks.Any())
                    {
                        var printCheckToRemove = transObj.PrintChecks.FirstOrDefault(x => x.TransactionId == transObj.Id);
                        if (printCheckToRemove != null)
                        {
                            checkNo = printCheckToRemove.CheckNo;
                            db.PrintChecks.Remove(printCheckToRemove);
                        }
                    }

                    var deletedTransaction = new DeletedTransaction
                    {
                        Amount = transObj.Amount,
                        CurrencyId = transObj.CurrencyId,
                        DocumentTypeId = transObj.DocumentTypeId,
                        ExchangeRate = transObj.ExchangeRate,
                        FinancialCycleId = transObj.FinancialCycleId,
                        HasAddedTax = transObj.HasAddedTax,
                        HasTax = transObj.HasTax,
                        IsPosted = transObj.IsPosted,
                        Recipient = transObj.Recipient,
                        Note = transObj.Note,
                        TransactionDate = transObj.TransactionDate,
                        TransactionId = firstTransactionDetail.TransactionId,
                        CreatedBy = userid,
                        CheckNo = checkNo.ToString(),
                        CreationDate = DateTime.Now
                    };

                    db.DeletedTransactions.Add(deletedTransaction);
                    db.SaveChanges();

                    // إنشاء DeletedTransactionDetails
                    var deletedDetails = data.Select(obj =>
                    {
                        // تحديث رصيد الحساب إذا وجد
                        if (obj.BalanceId > 0)
                        {
                            int balanceId = obj.BalanceId.Value;
                            decimal oldAmount = Convert.ToDecimal(obj.Credit + obj.Debit);
                            int type = obj.Credit > 0 ? 1 : 0;
                            // بعد حذف العنصر تعدل في الموانه
                            myExtention.UpdateActualExchange(balanceId, oldAmount, 0, type, userid);
                        }

                        return new DeletedTransactionDetail
                        {
                            BalanceId = obj.BalanceId,
                            AccTreeId = obj.AccTreeId,
                            Debit = obj.Debit,
                            Credit = obj.Credit,
                            Note = obj.Note,
                            TransactionDetailId = obj.Id,
                            DeletedTransactionId = deletedTransaction.Id,
                            CreatedBy = userid,
                            CreationDate = DateTime.Now
                        };
                    }).ToList();

                    db.DeletedTransactionDetails.AddRange(deletedDetails);


                    // حذف TransactionDetails و Transaction
                    db.TransactionDetails.RemoveRange(data);
                    db.Transactions.Remove(transObj);

                    db.SaveChanges();
                    transaction.Commit();

                    return 1;
                }
                catch
                {
                    transaction.Rollback();
                    return -1;
                }
            }
        }

        public List<TransactionVM> getDeletedDetailData(int Id)
        {
            var dataList = db.DeletedTransactionDetails.Where(q => q.DeletedTransactionId == Id).ToList();

            //return data.Select(q => new
            //{
            //    q.Id,
            //    q.BalanceId,
            //    q.AccTreeId,
            //    q.Debit,
            //    q.Credit,
            //    q.Note,
            //    q.TransactionDetailId,
            //    q.DeletedTransactionId
            //});


            List<TransactionVM> data = new List<TransactionVM>();
            foreach (var item in dataList)
            {
                TransactionVM obj = new TransactionVM();

                obj.id = item.Id;
                obj.accTrreId = item.AccTreeId;
                obj.balanceId = item.BalanceId ?? item.BalanceId;
                obj.balanceAccName = item.BalanceId != null ? item.Balance.AccountTree.AccName : "";
                obj.accName = item.AccountTree.AccName;
                obj.credit = item.Credit;
                obj.debit = item.Debit;
                obj.transactionDateStr = item.DeletedTransaction.TransactionDate != null ? item.DeletedTransaction.TransactionDate.Value.ToShortDateString() : "غير مدخل";
               // obj.transactionDateStr = item.DeletedTransaction.TransactionDate != null ? item.DeletedTransaction.TransactionDate.Value.ToShortDateString() : "غير مدخل";
                obj.note = item.DeletedTransaction.Note ?? "غير مدخل";

                data.Add(obj);
            }

            return data.OrderBy(x => x.debit).ToList();
        }
    }
}