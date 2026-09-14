using Purchases.Models;
using Purchases.Models.ViewModal;
using Purchases.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Purchases.MyLogic
{
    public class Reports
    {
        private Entities db = new Entities();
        private TreeClass trc = new TreeClass();
        private SharedClass sh = new SharedClass();
        /*transactions */

        // طباعة جميع الحركات
        public List<TransactionVM> printAllData(int financialCycleId)
        {
            List<TransactionVM> data = new List<TransactionVM>();
            try
            {
                var transactionsList = db.TransactionDetails.Where(x=> x.Transaction.FinancialCycleId == financialCycleId).ToList();

                foreach (var item in transactionsList)
                {
                    TransactionVM obj = new TransactionVM();

                    obj.transactionId = Convert.ToInt32(item.TransactionId);
                    obj.accName = item.AccountTree.AccName;
                    obj.credit = item.Credit;
                    obj.debit = item.Debit;
                    obj.transactionDate = item.Transaction.TransactionDate;
                    obj.transactionDateStr = item.Transaction.TransactionDate != null ? sh.configDate(item.Transaction.TransactionDate.Value) : "";
                    obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

                    data.Add(obj);
                }

                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.debit).ToList();
            }catch(Exception e)
            {
                return data;
            }
        }
        // طباعة بيانات الحركات لي بند محدد
        public List<TransactionVM> printDataByAccountId(int AccTreeId, int financialCycleId)
        {
            List<TransactionVM> data = new List<TransactionVM>();
            try
            {
                var dList = db.TransactionDetails
                    .Where(x => x.Transaction.FinancialCycleId == financialCycleId 
                    && x.AccTreeId == AccTreeId).Select(x => x.TransactionId).ToList();

                foreach (var item1 in dList)
                {
                    foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == item1))
                    {
                        TransactionVM obj = new TransactionVM();

                        obj.transactionId = Convert.ToInt32(item.TransactionId);
                        obj.accName = item.AccountTree.AccName;
                        obj.credit = item.Credit;
                        obj.debit = item.Debit;
                        obj.transactionDate = item.Transaction.TransactionDate;
                        obj.transactionDateStr = item.Transaction.TransactionDate != null ? item.Transaction.TransactionDate.Value.ToShortDateString() : "";
                        obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

                        data.Add(obj);
                    }
                }

                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.debit).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }
        // طباعة بيانات حركة محددة
        public List<TransactionVM> printDataByTransactionId(int transId)
        {
            List<TransactionVM> data = new List<TransactionVM>();
            try
            {
                foreach (var item1 in db.TransactionDetails.Where(x => x.TransactionId == transId).ToList())
                {
                    foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == item1.TransactionId))
                    {
                        TransactionVM obj = new TransactionVM();

                        obj.transactionId = Convert.ToInt32(item.TransactionId);
                        obj.accName = item.AccountTree.AccName;
                        obj.credit = item.Credit;
                        obj.debit = item.Debit;
                        obj.transactionDate = item.Transaction.TransactionDate;
                        obj.transactionDateStr = item.Transaction.TransactionDate != null ? item.Transaction.TransactionDate.Value.ToShortDateString() : "";
                        obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";

                        data.Add(obj);
                    }
                }

                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.debit).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }
        // طباعة الحركات في شهر محدد
        public List<TransactionVM> printDataByDate(DateTime myDate)
        {
            List<TransactionVM> data = new List<TransactionVM>();
            try
            {
                foreach (var item1 in db.Transactions.ToList())
                {
                    if (Convert.ToDateTime(item1.TransactionDate).Month == myDate.Month & Convert.ToDateTime(item1.TransactionDate).Year == myDate.Year)
                    {
                        foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == item1.Id))
                        {
                            TransactionVM obj = new TransactionVM();

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
                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.debit).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }

        // طباعة بيانات الحركة لي بند محدد
        public List<TransactionVM> printLidger(int AccTreeId)
        {
            List<TransactionVM> data = new List<TransactionVM>();
            try
            {
                var dList = db.TransactionDetails.Where(x => x.AccTreeId == AccTreeId).Select(x => x.TransactionId).ToList();
                
                foreach (var item in db.TransactionDetails.Where(x => dList.Contains(x.TransactionId)))
                {
                    if (item.AccTreeId == AccTreeId & item.Debit > 0) //cridet
                    {
                        //we must get debit
                        foreach (var item2 in db.TransactionDetails.Where(x => x.TransactionId == item.TransactionId & x.AccTreeId != AccTreeId & x.Credit > 0))
                        {
                            TransactionVM objCridet = new TransactionVM();

                            objCridet.transactionId = Convert.ToInt32(item2.TransactionId);
                            objCridet.accTrreId = item2.AccTreeId;
                            objCridet.accName = item2.AccountTree.AccName;
                            objCridet.credit = item2.Credit;
                            objCridet.debit = item2.Debit;
                            objCridet.transactionDate = item2.Transaction.TransactionDate;
                            objCridet.transactionDateStr = item2.Transaction.TransactionDate != null ? sh.configDate(item2.Transaction.TransactionDate.Value) : "";
                            objCridet.note = item2.Transaction.Note != null ? item2.Transaction.Note : "غير مدخل";

                            data.Add(objCridet);
                        }
                    }
                    else if (item.AccTreeId == AccTreeId & item.Credit > 0)
                    {
                        //we must get credit
                        foreach (var item2 in db.TransactionDetails.Where(x => x.TransactionId == item.TransactionId & x.AccTreeId != AccTreeId & x.Debit > 0))
                        {
                            TransactionVM objCridet = new TransactionVM();

                            objCridet.transactionId = Convert.ToInt32(item2.TransactionId);
                            objCridet.accTrreId = item2.AccTreeId;
                            objCridet.accName = item2.AccountTree.AccName;
                            objCridet.credit = item2.Credit;
                            objCridet.debit = item2.Debit;
                            objCridet.transactionDate = item2.Transaction.TransactionDate;
                            objCridet.transactionDateStr = item2.Transaction.TransactionDate != null ? sh.configDate(item2.Transaction.TransactionDate.Value) : "";
                            objCridet.note = item2.Transaction.Note != null ? item2.Transaction.Note : "غير مدخل";

                            data.Add(objCridet);
                        }
                    }
                }

                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.debit).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }

        // طباعة بيانات الحركة لي بند محدد بين تاريخين  
        //public List<TransactionVM> printLedgerByDates(int Id, DateTime? dateFrom, DateTime? dateTo)
        //{
        //    List<TransactionVM> data = new List<TransactionVM>();
        //    try
        //    {
        //        List<int> dList1 = db.TransactionDetails.Where(x => x.AccTreeId == Id).Select(d => d.TransactionId).ToList();
        //        var dList2 = db.TransactionDetails.Where(x => x.AccTreeId == Id).ToList();
        //        List<int> dList = new List<int>();

        //        if (dateFrom != null & dateTo != null)
        //        {
        //            dList = db.Transactions.Where(x => DbFunctions.TruncateTime(x.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) & DbFunctions.TruncateTime(x.TransactionDate) <= DbFunctions.TruncateTime(dateTo) & dList1.Contains(x.Id)).Select(x => x.Id).Distinct().ToList();
        //        }
        //        else
        //        {
        //            dList = db.Transactions.Where(x => dList1.Contains(x.Id)).Select(x => x.Id).Distinct().ToList();
        //        }


        //        foreach (var item in db.Transactions.Where(x => DbFunctions.TruncateTime(x.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) & DbFunctions.TruncateTime(x.TransactionDate) <= DbFunctions.TruncateTime(dateTo) & dList1.Contains(x.Id)))
        //            {
        //                TransactionVM obj = new TransactionVM();
        //                var myObj = item.TransactionDetails.FirstOrDefault(x => x.AccTreeId == Id);

        //                obj.note = item.Note;
        //                obj.transactionDate = item.TransactionDate;
        //                obj.transactionDateStr = item.TransactionDate.Value.Day + "/" + item.TransactionDate.Value.Month + "/" + item.TransactionDate.Value.Year;

        //                if (myObj.Credit > 0)
        //                {
        //                    obj.debit = db.TransactionDetails.Where(x => x.TransactionId == item.Id).Sum(x => x.Debit) - item.TransactionDetails.Where(x => x.TransactionId == item.Id).Sum(x => x.Credit) + myObj.Credit;
        //                    obj.credit = 0;
        //                }
        //                else
        //                {
        //                    obj.credit = db.TransactionDetails.Where(x => x.TransactionId == item.Id).Sum(x => x.Credit) - item.TransactionDetails.Where(x => x.TransactionId == item.Id).Sum(x => x.Debit) + myObj.Debit;
        //                    obj.debit = 0;
        //                }

        //                data.Add(obj);
        //            }
        //      ///  }
        //        //else
        //        //{
        //        //    foreach (var item in db.Transactions.Where(x => DbFunctions.TruncateTime(x.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) & DbFunctions.TruncateTime(x.TransactionDate) <= DbFunctions.TruncateTime(dateTo) & dList1.Contains(x.Id)))
        //        //    {
        //        //        TransactionVM obj = new TransactionVM();
        //        //        var myObj = item.TransactionDetails.FirstOrDefault(x => x.AccTreeId == Id);

        //        //        obj.note = item.Note;
        //        //        obj.transactionDate = item.TransactionDate;
        //        //        obj.transactionDateStr = item.TransactionDate.Value.Day + "/" + item.TransactionDate.Value.Month + "/" + item.TransactionDate.Value.Year;

        //        //        if (myObj.Credit > 0)
        //        //        {
        //        //            obj.debit = db.TransactionDetails.Where(x => x.TransactionId == item.Id).Sum(x => x.Debit) - item.TransactionDetails.Where(x => x.TransactionId == item.Id).Sum(x => x.Credit) + myObj.Credit;
        //        //            obj.credit = 0;
        //        //        }
        //        //        else
        //        //        {
        //        //            obj.credit = db.TransactionDetails.Where(x => x.TransactionId == item.Id).Sum(x => x.Credit) - item.TransactionDetails.Where(x => x.TransactionId == item.Id).Sum(x => x.Debit) + myObj.Debit;
        //        //            obj.debit = 0;
        //        //        }

        //        //        data.Add(obj);
        //        //    }
        //        //}
        //        return data.OrderBy(x => x.transactionDate).ThenBy(x => x.debit).ToList();
        //    }
        //    catch (Exception e)
        //    {
        //        return data;
        //    }
        //}

        // طباعة بيانات الحركة لي بند محدد بين تاريخين  
        public List<TransactionVM> printLedgerByDatesForSearch(int Id, DateTime? dateFrom, DateTime? dateTo, string userId)
        {
            List<TransactionVM> data = new List<TransactionVM>();
            SharedClass cs = new SharedClass();
            int financialCycleId = cs.GetUserCurrentFinancialCycleId(userId);

            try
            {
                List<int> dList = new List<int>();
                if (dateFrom != null & dateTo != null)
                {
                    dList = db.Transactions.Where(x => x.FinancialCycleId == financialCycleId && 
                    DbFunctions.TruncateTime(x.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) & DbFunctions.TruncateTime(x.TransactionDate) <= DbFunctions.TruncateTime(dateTo)).Select(x => x.Id).ToList();
                }
                else
                {
                    dList = db.Transactions.Where(x => x.FinancialCycleId == financialCycleId).Select(x => x.Id).ToList();
                }

                foreach (var item in db.TransactionDetails.Where(x => dList.Contains(x.TransactionId) & x.AccTreeId == Id))
                {
                    TransactionVM obj = new TransactionVM
                    {
                        transactionId = Convert.ToInt32(item.TransactionId),
                        accTrreId = item.AccTreeId,
                        accName = item.AccountTree.AccName,
                        credit = item.Credit,
                        debit = item.Debit,
                        amount = item.Credit + item.Debit,
                        transactionDate = item.Transaction.TransactionDate,
                        transactionDateStr = item.Transaction.TransactionDate != null ? sh.configDate(item.Transaction.TransactionDate.Value) : "",
                        note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل"
                    };

                    data.Add(obj);
                }

                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.debit).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }

        // دي دالة دفتر الاستاذ الجديدة وشغالة تمام - فيها التواريخ واسم البنك (اسم البنك عشان نفصل مصروفات بورتسودان من الرئاسة) واسم البند
        public List<BalanceVM> GetLedgerForAccountAndBank(int accountId, int? bankId, DateTime? dateFrom, DateTime? dateTo, string userId)
        {
            var result = new List<BalanceVM>();
            try
            {
                int financialCycleId = new SharedClass().GetUserCurrentFinancialCycleId(userId);

                var query = db.Transactions
                   .Where(t => t.FinancialCycleId == financialCycleId &&
                               t.TransactionDetails.Any(td => td.AccTreeId == accountId));

                // تطبيق الفلاتر حسب التواريخ
                if (dateFrom.HasValue && dateTo.HasValue)
                {
                    var fromDate = dateFrom.Value.Date;
                    var toDate = dateTo.Value.Date;
                    query = query.Where(t =>
                        DbFunctions.TruncateTime(t.TransactionDate) >= fromDate &&
                        DbFunctions.TruncateTime(t.TransactionDate) <= toDate);
                }

                // فلترة حسب البنك إذا موجود
                if (bankId.HasValue)
                {
                    query = query.Where(t => t.TransactionDetails.Any(td => td.AccTreeId == bankId));
                }

                var transactions = query
                    .Select(t => new
                    {
                        t.Id,
                        t.Note,
                        t.TransactionDate,
                        Detail = t.TransactionDetails.FirstOrDefault(td => td.AccTreeId == accountId),
                        AccName = t.TransactionDetails.FirstOrDefault(td => td.AccTreeId == accountId).AccountTree.AccName
                    })
                    .Where(t => t.Detail != null)
                    .OrderBy(t => t.TransactionDate)
                    .ToList();

                // بناء النتائج
                result = transactions.Select(t => new BalanceVM
                {
                    Id = t.Id,
                    transactionId = t.Id,
                    note = t.Note,
                    accTreeName = t.AccName,
                    transactionDate = t.TransactionDate,
                    transactionDateStr = sh.configDate(t.TransactionDate),
                    credit = t.Detail.Credit,
                    debit = t.Detail.Debit
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
                // يمكنك تسجيل الخطأ هنا مثلاً باستخدام logging
            }

            return result;
        }
        // دي دالة دفتر الاستاذ الجديدة وشغالة تمام - فيها التواريخ واسم البنك (اسم البنك عشان نفصل مصروفات بورتسودان من الرئاسة) واسم البند
        public List<BalanceVM> GetLedgerForAccountAndBankForParents(int accountId, int? bankId, DateTime? dateFrom, DateTime? dateTo, string userId)
        {
            var finalResult = new List<BalanceVM>();
            int financialCycleId = new SharedClass().GetUserCurrentFinancialCycleId(userId);
            try
            {
                var directChildernList = db.AccountTrees.Where(q => q.AccParent == accountId).ToList();
                foreach (var directChild in directChildernList)
                {
                    var result = new List<BalanceVM>();
                    List<int> AllKeys = trc.getAllItemsByParentId(directChild.Id);

                    //var AllKeysAfterFliter = db.AccountSubs.Where(x => AllKeys.Contains(x.AccTreeId ?? 0)).Select(s => new { AccTreeId= s.AccTreeId ?? 0, AccParentId = s.AccountTree.AccParent}).ToList();
                    var AllKeysAfterFliter = db.AccountSubs.Where(x => AllKeys.Contains(x.AccTreeId ?? 0)).Select(s => s.AccTreeId).ToList();

                    var isOk = AllKeysAfterFliter.Count() > 0? true : false;
                    //// old code
                    var query = isOk?  db.Transactions
                       .Where(t => t.FinancialCycleId == financialCycleId &&
                                    t.TransactionDetails.Any(td => AllKeysAfterFliter.Any(c => c == td.AccTreeId))).ToList()
                                    :
                                     db.Transactions
                       .Where(t => t.FinancialCycleId == financialCycleId &&
                                    t.TransactionDetails.Any(td => td.AccTreeId == directChild.Id)).ToList();

                    // تطبيق الفلاتر حسب التواريخ
                    if (dateFrom.HasValue && dateTo.HasValue)
                    {
                        var fromDate = dateFrom.Value.Date;
                        var toDate = dateTo.Value.Date;
                        query = query.Where(t =>
                            DbFunctions.TruncateTime(t.TransactionDate) >= fromDate &&
                            DbFunctions.TruncateTime(t.TransactionDate) <= toDate).ToList();
                    }

                    // فلترة حسب البنك إذا موجود
                    if (bankId.HasValue)
                    {
                        query = query.Where(t => t.TransactionDetails.Any(td => td.AccTreeId == bankId)).ToList();
                    }

                    var transactionDetails = isOk ? query
                        .SelectMany(t => t.TransactionDetails
                        .Where(td => AllKeysAfterFliter.Contains(td.AccTreeId)),
                            (t, td) => new BalanceVM
                            {
                                Id = td.Id,
                                transactionId = t.Id,
                                note = t.Note,
                                accTreeName = td.AccountTree != null ? td.AccountTree.AccName : "",
                                transactionDate = t.TransactionDate,
                                transactionDateStr = sh.configDate(t.TransactionDate),
                                accTreeId = td.AccTreeId,
                                parentId = td.AccountTree != null ? td.AccountTree.AccParent : (int?)null,
                                credit = td.Credit,
                                debit = td.Debit
                            })
                        .OrderBy(x => x.transactionDate)
                        .ToList()
                        :
                        query
                        .SelectMany(t => t.TransactionDetails
                        .Where(td => td.AccTreeId == directChild.Id),
                            (t, td) => new BalanceVM
                            {
                                Id = td.Id,
                                transactionId = t.Id,
                                note = t.Note,
                                accTreeName = td.AccountTree != null ? td.AccountTree.AccName : "",
                                transactionDate = t.TransactionDate,
                                transactionDateStr = sh.configDate(t.TransactionDate),
                                accTreeId = td.AccTreeId,
                                parentId = td.AccountTree != null ? td.AccountTree.AccParent : (int?)null,
                                credit = td.Credit,
                                debit = td.Debit
                            })
                        .OrderBy(x => x.transactionDate)
                        .ToList();

                    // الآن نجمع حسب parentId (أو حسب accTreeId لو تفضل)
                    var grouped = transactionDetails
                        .Where(x => x.parentId.HasValue) // نتجنّب nulls لو صارت
                        .GroupBy(x => x.parentId.Value)
                        .Select(g => new BalanceVM
                        {
                            parentId = g.Key,
                            accTreeName = g.FirstOrDefault()?.accTreeName ?? "",
                            debit = g.Sum(x => x.debit),
                            credit = g.Sum(x => x.credit)
                        })
                        .ToList();

                    var data = new BalanceVM()
                    {
                        accTreeId = directChild.Id,
                        accTreeName = directChild.AccName,
                        debit = grouped.Sum(s => s.debit),
                        credit = grouped.Sum(s => s.credit)
                    };
                    finalResult.Add(data);
                }

                return finalResult;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ex.Message);
                // يمكنك تسجيل الخطأ هنا مثلاً باستخدام logging
            }

        }

        // طباعة بيانات الحركة بين تاريخين 
        public List<TransactionVM> printDataByDates(DateTime? dateFrom, DateTime? dateTo)
        {
            List<TransactionVM> data = new List<TransactionVM>();

            try
            {
                List<int> dList = new List<int>();

                if (dateFrom != null & dateTo != null)
                {
                    dList = db.Transactions.Where(x => x.DocumentTypeId != 5 & DbFunctions.TruncateTime(x.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) & DbFunctions.TruncateTime(x.TransactionDate) <= DbFunctions.TruncateTime(dateTo)).Select(s => s.Id).ToList();
                }
                else
                {
                    dList = db.Transactions.Where(x => x.DocumentTypeId != 5).Select(s => s.Id).ToList();
                }

                foreach (var item in db.TransactionDetails.Where(x => dList.Distinct().Contains(x.TransactionId)))
                {
                    TransactionVM obj = new TransactionVM();

                    if (item.Credit > 0)
                    {
                        obj.transactionId = Convert.ToInt32(item.TransactionId);
                        obj.accName = item.AccountTree.AccName;
                        obj.debit = 0;
                        obj.credit = item.Credit;
                        obj.topParent = db.AccountTrees.Find(trc.getTopParentId(item.AccTreeId)).AccName;
                        obj.transactionDate = item.Transaction.TransactionDate;
                        obj.transactionDateStr = item.Transaction.TransactionDate != null ? sh.configDate(item.Transaction.TransactionDate.Value) : "";
                        obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";
                    }
                    else
                    {
                        obj.transactionId = Convert.ToInt32(item.TransactionId);
                        obj.accName = item.AccountTree.AccName;
                        obj.debit = item.Debit;
                        obj.credit = 0;
                        obj.topParent = db.AccountTrees.Find(trc.getTopParentId(item.AccTreeId)).AccName;
                        obj.transactionDate = item.Transaction.TransactionDate;
                        obj.transactionDateStr = item.Transaction.TransactionDate != null ? sh.configDate(item.Transaction.TransactionDate.Value) : "";
                        obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";
                    }

                    data.Add(obj);
                }

                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.credit).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }
        // طباعة بيانات الحركة بين تاريخين 
        public List<TransactionVM> printDataByDatesForBank(int? bankId, DateTime? dateFrom, DateTime? dateTo)
        {
            List<TransactionVM> data = new List<TransactionVM>();

            try
            {
                List<int> dList = new List<int>();
                List<Transaction> transactionsList = new List<Transaction>();

                if (dateFrom != null & dateTo != null)
                {
                    transactionsList = db.Transactions
                        .Where(x => x.DocumentTypeId != 5 &&
                               DbFunctions.TruncateTime(x.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) &&
                               DbFunctions.TruncateTime(x.TransactionDate) <= DbFunctions.TruncateTime(dateTo))
                        .ToList();
                }
                else
                {
                    transactionsList = db.Transactions.Where(x => x.DocumentTypeId != 5).ToList();
                }

                if (bankId != null) {
                    transactionsList = transactionsList.Where(q => q.TransactionDetails.Any(x => x.AccTreeId == bankId)).ToList();
                }

                dList = transactionsList.Select(x => x.Id).ToList();

                foreach (var item in db.TransactionDetails.Where(x => dList.Distinct().Contains(x.TransactionId)))
                {
                    TransactionVM obj = new TransactionVM();

                    if (item.Credit > 0)
                    {
                        obj.transactionId = Convert.ToInt32(item.TransactionId);
                        obj.accName = item.AccountTree.AccName;
                        obj.debit = 0;
                        obj.credit = item.Credit;
                        obj.topParent = db.AccountTrees.Find(trc.getTopParentId(item.AccTreeId)).AccName;
                        obj.transactionDate = item.Transaction.TransactionDate;
                        obj.transactionDateStr = item.Transaction.TransactionDate != null ? sh.configDate(item.Transaction.TransactionDate.Value) : "";
                        obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";
                    }
                    else
                    {
                        obj.transactionId = Convert.ToInt32(item.TransactionId);
                        obj.accName = item.AccountTree.AccName;
                        obj.debit = item.Debit;
                        obj.credit = 0;
                        obj.topParent = db.AccountTrees.Find(trc.getTopParentId(item.AccTreeId)).AccName;
                        obj.transactionDate = item.Transaction.TransactionDate;
                        obj.transactionDateStr = item.Transaction.TransactionDate != null ? sh.configDate(item.Transaction.TransactionDate.Value) : "";
                        obj.note = item.Transaction.Note != null ? item.Transaction.Note : "غير مدخل";
                    }

                    data.Add(obj);
                }
                
                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.credit).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }

        // طباعة بيانات الحركة بين تاريخين  
        public List<TransactionVM> printDataByDatesForSearch(DateTime? dateFrom, DateTime? dateTo)
        {
            List<TransactionVM> data = new List<TransactionVM>();
            try
            {
                List<Transaction> dList = new List<Transaction>();
                if (dateFrom != null & dateTo != null)
                {
                    dList = db.Transactions.Where(x => DbFunctions.TruncateTime(x.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) & DbFunctions.TruncateTime(x.TransactionDate) <= DbFunctions.TruncateTime(dateTo)).ToList();
                }
                else
                {
                    dList = db.Transactions.ToList();
                }

                foreach (var item in dList)
                {
                    TransactionVM obj = new TransactionVM();

                    obj.transactionId = Convert.ToInt32(item.Id);
                    obj.amount = item.Amount;
                    obj.transactionDate = item.TransactionDate;
                    obj.transactionDateStr = item.TransactionDate != null ? sh.configDate(item.TransactionDate.Value) : "";
                    obj.note = item.Note != null ? item.Note : "غير مدخل";

                    data.Add(obj);
                }

                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.debit).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }

        // طباعة بيانات الحركة بين تاريخين  
        public List<TransactionVM> printDataByDatesForSearchForBank(int? bankId, DateTime? dateFrom, DateTime? dateTo)
        {
            List<TransactionVM> data = new List<TransactionVM>();
            try
            {
                List<Transaction> dList = new List<Transaction>();
                if (dateFrom != null & dateTo != null)
                {
                    dList = db.Transactions.Where(x => x.DocumentTypeId != 5 &&
                    DbFunctions.TruncateTime(x.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) &&
                    DbFunctions.TruncateTime(x.TransactionDate) <= DbFunctions.TruncateTime(dateTo)).ToList();
                }
                else
                {
                    dList = db.Transactions.Where(x => x.DocumentTypeId != 5).ToList();
                }

                if(bankId != null)
                {
                    dList = dList.Where(x=> x.TransactionDetails.Any(q=> q.AccTreeId == bankId)).ToList();
                }

                foreach (var item in dList)
                {
                    TransactionVM obj = new TransactionVM();

                    obj.transactionId = Convert.ToInt32(item.Id);
                    obj.amount = item.Amount;
                    obj.transactionDate = item.TransactionDate;
                    obj.transactionDateStr = item.TransactionDate != null ? sh.configDate(item.TransactionDate.Value) : "";
                    obj.note = item.Note != null ? item.Note : "غير مدخل";

                    data.Add(obj);
                }

                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.debit).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }

        public List<TransactionVM> printsAccountStatement(int Id, int financialCycleId)
        {
            List<TransactionVM> data = new List<TransactionVM>();

            try
            {
                var dList = db.TransactionDetails.Where(x => x.Transaction.FinancialCycleId == financialCycleId && x.AccTreeId == Id).Select(x => x.TransactionId).ToList();

                foreach (var item in db.TransactionDetails.Where(x => dList.Contains(x.TransactionId)))
                {
                    if (item.AccTreeId == Id & item.Debit > 0) //cridet
                    {
                        //we must get debit
                        foreach (var item2 in db.TransactionDetails.Where(x => x.TransactionId == item.TransactionId & x.AccTreeId != Id & x.Credit == item.Debit))
                        {
                            TransactionVM objCridet = new TransactionVM();

                            objCridet.transactionId = Convert.ToInt32(item2.TransactionId);
                            objCridet.accTrreId = item2.AccTreeId;
                            objCridet.accName = item2.AccountTree.AccName;
                            objCridet.credit = item2.Credit;
                            objCridet.debit = item2.Debit;
                            objCridet.transactionDate = item2.Transaction.TransactionDate;
                            objCridet.transactionDateStr = item2.Transaction.TransactionDate != null ? sh.configDate(item2.Transaction.TransactionDate.Value) : "";
                            objCridet.note = item2.Transaction.Note != null ? item2.Transaction.Note : "غير مدخل";

                            data.Add(objCridet);
                        }
                    }
                    else if (item.AccTreeId == Id & item.Credit > 0)
                    {
                        //we must get credit
                        foreach (var item2 in db.TransactionDetails.Where(x => x.TransactionId == item.TransactionId & x.AccTreeId != Id & x.Debit == item.Credit))
                        {
                            TransactionVM objCridet = new TransactionVM();

                            objCridet.transactionId = Convert.ToInt32(item2.TransactionId);
                            objCridet.accTrreId = item2.AccTreeId;
                            objCridet.accName = item2.AccountTree.AccName;
                            objCridet.credit = item2.Credit;
                            objCridet.debit = item2.Debit;
                            objCridet.transactionDate = item2.Transaction.TransactionDate;
                            objCridet.transactionDateStr = item2.Transaction.TransactionDate != null ? sh.configDate(item2.Transaction.TransactionDate.Value) : "";
                            objCridet.note = item2.Transaction.Note != null ? item2.Transaction.Note : "غير مدخل";

                            data.Add(objCridet);
                        }
                    }
                }
                
                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.debit).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }

        // طباعة ضريبة ال 5% الخاصة بالايرادات
        public List<TransactionVM> printFiveTaxReport()
        {
            List<TransactionVM> data = new List<TransactionVM>();

            try
            {
                int TaxRevenueId = Convert.ToInt32(ConstValEnum.TaxRevenueId);

                if (db.TransactionDetails.Any(x => x.AccTreeId == TaxRevenueId))
                {
                    foreach (var item in db.TransactionDetails.Where(x => x.AccTreeId == 229))
                    {
                        data.Add(new TransactionVM()
                        {
                            id = item.Id,
                            note = item.Note,
                            amount = item.Transaction.Amount,
                            tax = item.Debit + item.Credit,
                            transactionDate = item.Transaction.TransactionDate
                        });
                    }
                }

                return data.OrderBy(x => x.transactionDate).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }
        
        // طباعة ضريبة ال 1% الخاصة بالمصروفات
        public List<TransactionVM> printTaxReport()
        {
            List<TransactionVM> data = new List<TransactionVM>();

            try
            {
                if (db.Transactions.Any(x => x.HasTax == true))
                {
                    foreach (var item in db.Transactions.Where(x => x.HasTax == true))
                    {
                        data.Add(new TransactionVM()
                        {
                            id = item.Id,
                            note = item.Recipient,
                            amount = item.Amount,
                            tax = item.Amount * (decimal)0.01,
                            total = item.Amount - (item.Amount * (decimal)0.01),
                            documentType = db.PrintChecks.Any(x => x.TransactionId == item.Id) ? db.PrintChecks.FirstOrDefault(x => x.TransactionId == item.Id).Id.ToString() : "غير مدخل",
                            transactionDateStr = sh.configDate(item.TransactionDate.Value),
                            transactionDate = item.TransactionDate,
                        });
                    }
                }

                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.amount).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }

       // ----- خاص بضربية الدخل تم الانشاء بواسطة ناجي 
        public List<TransactionVM> GetIncomeTaxInDateRangeForSearch(DateTime? dateFrom, DateTime? dateTo, int? bankId, string userId)
        {
            List<TransactionVM> data = new List<TransactionVM>();
            SharedClass cs = new SharedClass();

            try
            {
                List<Transaction> dList = new List<Transaction>();
                int financialCycleId = cs.GetUserCurrentFinancialCycleId(userId);

                if (dateFrom != null & dateTo != null)
                {
                    dList = db.Transactions.Where(x =>
                    x.FinancialCycleId == financialCycleId &&
                    x.TransactionDetails.Any(t => t.AccTreeId == 317 && t.Credit > 0) &&
                    DbFunctions.TruncateTime(x.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) &&
                    DbFunctions.TruncateTime(x.TransactionDate) <= DbFunctions.TruncateTime(dateTo)).ToList();
                }
                else
                {
                    dList = db.Transactions.Where(x =>
                    x.FinancialCycleId == financialCycleId &&
                    x.TransactionDetails.Any(t => t.AccTreeId == 317 && t.Credit > 0)).ToList();
                }

                if (bankId.HasValue)
                {
                    dList = dList.Where(x => x.TransactionDetails.Any(t => t.AccTreeId == bankId)).ToList();
                }

                var PrintCheckList = db.PrintChecks.ToList();

                foreach (var item in dList)
                {                                                                  // كود ضربية الدخل في الشجرة الحسبية 317
                    var myObj = item.TransactionDetails.FirstOrDefault(x => x.TransactionId == item.Id && x.AccTreeId == 317);

                    TransactionVM Obj = new TransactionVM();

                    Obj.id = item.Id;
                    Obj.transactionId = item.Id;
                    Obj.recipient = item.Recipient;
                    Obj.note = item.Note;
                    Obj.CheckNo = PrintCheckList.Any(x => x.TransactionId == item.Id) ? PrintCheckList.FirstOrDefault(x => x.TransactionId == item.Id).CheckNo : 0;
                    Obj.amount = item.Amount;
                    Obj.tax = myObj.Credit + myObj.Debit;
                    Obj.transactionDate = item.TransactionDate;
                    Obj.documentType = PrintCheckList.Any(x => x.TransactionId == item.Id) ? PrintCheckList.FirstOrDefault(x => x.TransactionId == item.Id).Id.ToString() : "غير مدخل";
                    Obj.transactionDateStr = sh.configDate(item.TransactionDate.Value);

                    data.Add(Obj);
                }

                return data.OrderBy(x => x.transactionDate).ToList();
            }
            catch (Exception e)
            {
                // يمكن تسجيل الخطأ هنا إذا لزم الأمر
                return data;
            }
        }












        public List<TransactionVM> GetTaxInDateRangeForSearch(DateTime? dateFrom, DateTime? dateTo, int? bankId, string userId)
        {
            List<TransactionVM> data = new List<TransactionVM>();
            SharedClass cs = new SharedClass();

            try
            {
                List<Transaction> dList = new List<Transaction>();
                int financialCycleId = cs.GetUserCurrentFinancialCycleId(userId);

                if (dateFrom != null & dateTo != null)
                {
                    dList = db.Transactions.Where(x =>
                    x.FinancialCycleId == financialCycleId && 
                    x.TransactionDetails.Any(t => t.AccTreeId == 228 && t.Credit > 0) &&
                    DbFunctions.TruncateTime(x.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) &&
                    DbFunctions.TruncateTime(x.TransactionDate) <= DbFunctions.TruncateTime(dateTo)).ToList();
                }
                else
                {
                    dList = db.Transactions.Where(x =>
                    x.FinancialCycleId == financialCycleId &&
                    x.TransactionDetails.Any(t => t.AccTreeId == 228 && t.Credit > 0)).ToList();
                }

                if (bankId.HasValue)
                {
                    dList = dList.Where(x => x.TransactionDetails.Any(t => t.AccTreeId == bankId)).ToList();
                }

                var PrintCheckList = db.PrintChecks.ToList();

                foreach (var item in dList)
                {
                    var myObj = item.TransactionDetails.FirstOrDefault(x => x.TransactionId == item.Id && x.AccTreeId == 228);

                    TransactionVM Obj = new TransactionVM();

                    Obj.id = item.Id;
                    Obj.transactionId = item.Id;
                    Obj.recipient = item.Recipient;
                    Obj.note = item.Note;
                    Obj.CheckNo = PrintCheckList.Any(x => x.TransactionId == item.Id) ? PrintCheckList.FirstOrDefault(x => x.TransactionId == item.Id).CheckNo : 0;
                    Obj.amount = item.Amount;
                    Obj.tax = myObj.Credit + myObj.Debit;
                    Obj.transactionDate = item.TransactionDate;
                    Obj.documentType = PrintCheckList.Any(x => x.TransactionId == item.Id) ? PrintCheckList.FirstOrDefault(x => x.TransactionId == item.Id).Id.ToString() : "غير مدخل";
                    Obj.transactionDateStr = sh.configDate(item.TransactionDate.Value);

                    data.Add(Obj);
                }

                return data.OrderBy(x => x.transactionDate).ToList();
            }
            catch (Exception e)
            {
                // يمكن تسجيل الخطأ هنا إذا لزم الأمر
                return data;
            }
        }

        // طباعة كشف الضريبة ال 1% الخاصة بالمصروفات الكشف ال بيمشي مصلحة الضرائب
        public List<TransactionVM> printTaxAuthorityReport()
        {
            List<TransactionVM> data = new List<TransactionVM>();

            try
            {
                if (db.Transactions.Any(x => x.HasTax == true))
                {
                    foreach (var item in db.Transactions.Where(x => x.HasTax == true))
                    {
                        data.Add(new TransactionVM()
                        {
                            id = item.Id,
                            note = item.Recipient,
                            amount = item.Amount,
                            tax = item.Amount * (decimal)0.01,
                            total = item.Amount - (item.Amount * (decimal)0.01),
                            documentType = db.PrintChecks.Any(x => x.TransactionId == item.Id) ? db.PrintChecks.FirstOrDefault(x => x.TransactionId == item.Id).Id.ToString() : "غير مدخل",
                            transactionDateStr = sh.configDate(item.TransactionDate.Value),
                            transactionDate = item.TransactionDate,
                        });
                    }
                }

                return data.OrderBy(x => x.transactionDate).ThenBy(x => x.debit).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }
        
        // ذر البحث في شاشة طباعة تقرير ضريبة ال 1% مصروفات
        public List<BankTransferVM> GetTransfDateRangeForSearch(DateTime? dateFrom, DateTime? dateTo)
        {
            List<BankTransferVM> data = new List<BankTransferVM>();

            try
            {
                List<int> dList = new List<int>();

                if (dateFrom != null & dateTo != null)
                {
                    dList = db.Transactions.Where(x => x.DocumentTypeId == 5 & DbFunctions.TruncateTime(x.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) & DbFunctions.TruncateTime(x.TransactionDate) <= DbFunctions.TruncateTime(dateTo)).Select(s => s.Id).ToList();
                }
                else
                {
                    dList = db.Transactions.Where(x => x.DocumentTypeId == 5).Select(s=> s.Id).ToList();
                }

                foreach (var item in dList)
                {
                    BankTransferVM obj = new BankTransferVM();
                    var itemDebit = db.TransactionDetails.FirstOrDefault(x => x.TransactionId == item & x.Debit > 0 );

                    obj.TransactionId = itemDebit.TransactionId;
                    obj.Note = itemDebit.Transaction.Note;
                    obj.Amount = itemDebit.Transaction.Amount;
                    obj.TransactionDate = Convert.ToDateTime(itemDebit.Transaction.TransactionDate);
                    obj.TransactionDateStr = sh.configDate(itemDebit.Transaction.TransactionDate.Value);

                    obj.ToAccTreeId = itemDebit.AccTreeId;
                    obj.DebitAccTreeName = itemDebit.AccountTree.AccName;

                    var itemCredit = db.TransactionDetails.FirstOrDefault(x => x.TransactionId == item & x.Credit > 0);
                    
                    obj.FromAccTreeId = itemCredit.AccTreeId;
                    obj.CreditAccTreeName = itemCredit.AccountTree.AccName;
                    
                    data.Add(obj);
                }

                return data.OrderBy(x => x.TransactionDate).ThenBy(x => x.Amount).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }

        // موقف بنود الموازنة حسب تاريخ معين
        //دي حقت الموازنة كل بند اب(رئيسي) و التفاصيل حقته في الشجرة وضعها والصرف الفعلي ليها في الموازنة كيف حسب تاريخ معين.......ممكن نستخدمة في التقارير
        public List<BalanceVM> BalancePositionByDate(DateTime? dateFrom, DateTime? dateTo, int AccountTopType)
        {
            // AccountTopType ==> نوع الحساب ايرادات ولا مصروفات ولا الكل
            List<BalanceVM> data = new List<BalanceVM>();
            try
            {
                List<int> dList = new List<int>();
                List<TransactionDetail> RetrivedDataList = new List<TransactionDetail>();

                if (dateFrom != null & dateTo != null)
                {
                    RetrivedDataList = db.TransactionDetails.Where(x => x.BalanceId != null && x.Balance.Credint > 0 && DbFunctions.TruncateTime(x.Transaction.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) & DbFunctions.TruncateTime(x.Transaction.TransactionDate) <= DbFunctions.TruncateTime(dateTo)).ToList();
                }
                else
                {
                    RetrivedDataList = db.TransactionDetails.Where(x => x.BalanceId != null && x.Balance.Credint > 0).ToList();
                }

                var balanceIdList = RetrivedDataList.OrderBy(x=>x.AccountTree.AccParent).Select(b => b.BalanceId).ToList();
                double deviationRelative = 0;
                foreach (int item in balanceIdList)
                {
                    BalanceVM obj = new BalanceVM();

                    obj.balanceId = item;
                    obj.credint = RetrivedDataList.FirstOrDefault(x => x.BalanceId == item).Balance.Credint;
                    obj.sumOfDebit = RetrivedDataList.Where(x=>x.BalanceId == item).Sum(x => x.Debit);
                    obj.sumOfCredit = RetrivedDataList.Where(x=>x.BalanceId == item).Sum(x => x.Credit);
                    obj.actualExchange =  obj.sumOfDebit + obj.sumOfCredit;
                    obj.accTreeName = RetrivedDataList.FirstOrDefault(x => x.BalanceId == item).Balance.AccountTree.AccName;
                    
                    int? accTreeId = RetrivedDataList.FirstOrDefault(x => x.BalanceId == item).Balance.AccountTreeId;

                    obj.relativeDeviation = AccountTopType == 3 || trc.getTopParentId(accTreeId) == 3? obj.credint + obj.actualExchange : obj.credint - obj.actualExchange;
                    deviationRelative = Math.Round(Convert.ToDouble((obj.actualExchange / obj.credint) * 100));
                    obj.deviationRelative = Convert.ToDecimal(deviationRelative);

                    if (AccountTopType > 0) // حسب نوع التقرير ايرادات فقط او مصروفات فقط 
                    {
                        if (trc.getTopParentId(accTreeId) == AccountTopType)
                        {
                            if (!data.Any(x => x.balanceId == item))
                            {
                                data.Add(obj);
                            }
                        }
                    }
                    else // كل الموازنة
                    {
                        if (!data.Any(x => x.balanceId == item))
                        {
                            data.Add(obj);
                        }
                    }
                }

                return data.OrderBy(x => x.actualExchange).ToList();
            }
            catch (Exception e)
            {
                return data;
            }
        }

        /*-----------------------------------------تقارير موقف الموازنة----------------------------------------*/
        // موقف الموازنة
     
        public List<BalanceVM> BalancePosition(int financeCycleId, int ParentId, DateTime? dateFrom, DateTime? dateTo)
        {
            var parentRes = new List<BalanceVM>();
            var newData = new List<int>();
            var FinancialCycleAccountTreeIdList = new List<dynamic>();
            List<TransactionDetail> RetrivedDataList = new List<TransactionDetail>();

           // SharedClass sh = new SharedClass();
           // int FinancialCycleId = sh.GetCurrentFinancialCycleId();

            // دي عشان اقسم مبلغ الموازنة علي عدد الشهور المطلوب ليها التقرير
            //int MonthsPercentage = 1;
            //if (dateFrom != null & dateTo != null)
            //{
            //    MonthsPercentage = ((dateFrom.Value.Year - dateTo.Value.Year) * 12) + (dateTo.Value.Month - dateFrom.Value.Month) + 1;
            //    if (dateTo.Value.Day < dateFrom.Value.Day)
            //    {
            //        MonthsPercentage--;
            //    }

            //    RetrivedDataList = db.TransactionDetails.Where(x => x.BalanceId != null && x.Balance.Credint > 0 && DbFunctions.TruncateTime(x.Transaction.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) & DbFunctions.TruncateTime(x.Transaction.TransactionDate) <= DbFunctions.TruncateTime(dateTo)).ToList();
            //}
            //else
            //{
            //    RetrivedDataList = db.TransactionDetails.Where(x => x.BalanceId != null && x.Balance.Credint > 0).ToList();
            //}

           // decimal MonthsPercentage = 100; // نفرض مبدئياً إنها 100%
            int monthsDiff = 0;
            if (dateFrom != null && dateTo != null)
            {
                monthsDiff =((dateTo.Value.Year - dateFrom.Value.Year) * 12) + (dateTo.Value.Month - dateFrom.Value.Month) + 1;

                if (dateTo.Value.Day < dateFrom.Value.Day)
                {
                    monthsDiff--; // لو يوم النهاية أصغر من يوم البداية، ننقص شهر
                }

                // نحسب النسبة
               // MonthsPercentage = Math.Round((decimal)(12.0 / monthsDiff), 0);

                // باقي استعلام الداتا
                RetrivedDataList = db.TransactionDetails.Where(x =>
                    x.Transaction.FinancialCycleId == financeCycleId &&
                    x.BalanceId != null && x.Balance.Credint > 0 &&
                    DbFunctions.TruncateTime(x.Transaction.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) &&
                    DbFunctions.TruncateTime(x.Transaction.TransactionDate) <= DbFunctions.TruncateTime(dateTo)).ToList();
            }
            else
            {
                RetrivedDataList = db.TransactionDetails.Where(x =>
                    x.Transaction.FinancialCycleId == financeCycleId &&
                    x.BalanceId != null && x.Balance.Credint > 0).ToList();
            }

            var data = trc.getAllItemsByParentId(ParentId);

            /*------start--------*/
            // هنا بختبر هل الحساب ده تم تغيير الحساب الاب ليهو خلال العام الحالي ولا لا
            var relevantItems = db.FinancialCycleAccountTrees.Where(s => s.FinancialCycleId == financeCycleId).ToList();

            foreach (var item in relevantItems)
            {
                int accParent = item.ParentId ?? 0;
                int accTreeId = item.AccTreeId ?? 0;

                if (data.Contains(accParent)) // && !data.Contains(accTreeId)
                {
                    FinancialCycleAccountTreeIdList.Add(new {accTreeId, accParent});
                }
            }
            /*------end--------*/
            
            var balances = db.Balances.Where(x => x.FinanceCycleId == financeCycleId).ToList();
            newData = data.Where(x => !balances.Any(b => b.AccountTreeId == x)).ToList();

            foreach (var item in newData)
            {
                var res = new List<BalanceVM>();

                BalanceVM objPrent = new BalanceVM();

                objPrent.accTreeName = db.AccountTrees.Find(item).AccName;
                objPrent.parentName = db.AccountTrees.Find(ParentId).AccName;
                objPrent.parentId = ParentId;
                objPrent.Id = item;
                objPrent.financeCycleId = financeCycleId;
                objPrent.credint = balances.Where(x => x.AccountTree.AccParent == item).Sum(x => x.Credint);
                objPrent.credintPercent = (balances.Where(x => x.AccountTree.AccParent == item).Sum(x => x.Credint) / 12) * monthsDiff;
                objPrent.credint = Math.Round((decimal)objPrent.credint, 2);
                objPrent.credintPercent = Math.Round((decimal)objPrent.credintPercent, 2);

                var additionalList = FinancialCycleAccountTreeIdList.Where(x=> x.accParent == item).Select(x=> x.accTreeId).ToList();
                var newBalances = balances.Where(x => x.AccountTree.AccParent == item || additionalList.Contains(x.AccountTreeId)).ToList();

                foreach (var item1 in newBalances)
                {
                    if(relevantItems.Any(x=> x.AccTreeId == item1.AccountTreeId && x.ParentId != item))
                    {
                        continue;
                    }
                    BalanceVM obj = new BalanceVM();

                    obj.Id = item1.AccountTreeId ?? 0;
                    obj.accTreeName = item1.AccountTree.AccName;
                    obj.credint = item1.Credint;
                    obj.credintPercent = (item1.Credint / 12) * monthsDiff;
                    obj.parentId = item;
                    obj.actualExchange = RetrivedDataList.Where(x => x.BalanceId == item1.Id).Sum(x => x.Debit) - RetrivedDataList.Where(x => x.BalanceId == item1.Id).Sum(x => x.Credit);

                    res.Add(obj);
                }
                
                objPrent.sumOfCredint = res.Sum(x => x.credint);
                objPrent.actualExchange = res.Sum(x => x.actualExchange);
                objPrent.details = res;
                // objPrent.details = res.Where(x=> x.credint > 0).ToList();
                parentRes.Add(objPrent);
            }

            return parentRes.Where(x => x.credint > 0).ToList();
        }

        public List<BalanceVM> BalancePositionRevenue(int financeCycleId, int ParentId, DateTime? dateFrom, DateTime? dateTo)
        {
            var parentRes = new List<BalanceVM>();
            var newData = new List<int>();
            List<TransactionDetail> RetrivedDataList = new List<TransactionDetail>();

            // دي عشان اقسم مبلغ الموازنة علي عدد الشهور المطلوب ليها التقرير
           // decimal MonthsPercentage = 100; // نفرض مبدئياً إنها 100%
            int monthsDiff = 0;
            if (dateFrom != null & dateTo != null)
            {
                monthsDiff = ((dateTo.Value.Year - dateFrom.Value.Year) * 12) + (dateTo.Value.Month - dateFrom.Value.Month) + 1;

                if (dateTo.Value.Day < dateFrom.Value.Day)
                {
                    monthsDiff--; // لو يوم النهاية أصغر من يوم البداية، ننقص شهر
                }

                // نحسب النسبة
                //MonthsPercentage = Math.Round((decimal)(12.0 / monthsDiff), 2);


                RetrivedDataList = db.TransactionDetails.Where(x => 
                x.Transaction.FinancialCycleId == financeCycleId &&
                x.BalanceId != null && x.Balance.Credint > 0 
                && DbFunctions.TruncateTime(x.Transaction.TransactionDate) >= DbFunctions.TruncateTime(dateFrom) & DbFunctions.TruncateTime(x.Transaction.TransactionDate) <= DbFunctions.TruncateTime(dateTo)).ToList();
            }
            else
            {
                RetrivedDataList = db.TransactionDetails.Where(x =>
                    x.Transaction.FinancialCycleId == financeCycleId && 
                    x.BalanceId != null && x.Balance.Credint > 0).ToList();
            }

            var data = trc.getAllItemsByParentId(ParentId);
            var balances = db.Balances.Where(x => x.FinanceCycleId == financeCycleId).ToList();
            
            newData = data.Where(x => balances.Any(b => b.AccountTreeId == x)).ToList();

            foreach (var item in newData)
            {
                var res = new List<BalanceVM>();

                BalanceVM objPrent = new BalanceVM();

                objPrent.accTreeName = db.AccountTrees.Find(item).AccName;
                objPrent.parentName = db.AccountTrees.Find(ParentId).AccName;
                objPrent.parentId = ParentId;
                objPrent.Id = item;
                objPrent.financeCycleId = financeCycleId;
                objPrent.credint = balances.Where(x => x.AccountTreeId == item).Sum(x => x.Credint);
                objPrent.credint = Math.Round((decimal)objPrent.credint, 2);
                objPrent.credintPercent = (balances.Where(x => x.AccountTreeId == item).Sum(x => x.Credint) / 12) * monthsDiff;
                objPrent.credintPercent = Math.Round((decimal)objPrent.credintPercent, 2);

                objPrent.sumOfCredint = res.Sum(x => x.credint);
                objPrent.actualExchange = RetrivedDataList.Where(q=> q.Transaction.CurrencyId == 1).Where(x => x.AccTreeId == item).Sum(x => Math.Abs((x.Credit - x.Debit)??0));

                foreach (var item1 in 
                    RetrivedDataList.Where(q => q.Transaction.CurrencyId != 1).Where(x => x.AccTreeId == item).ToList())
                {
                    var CurrencyId = item1.Transaction.CurrencyId;
                    var TransactionDate = item1.Transaction.TransactionDate;
                    var Amount = item1.Credit + item1.Debit;

                    decimal? actualExchange = CalculateActualExchange(CurrencyId, TransactionDate, Amount);
                    objPrent.actualExchange += actualExchange;
                }

                parentRes.Add(objPrent);
            }

            //parentRes = parentRes.Where(x => x.details.Where(d => d.actualExchange > 0)).ToList();

            return parentRes.Where(x => x.credint > 0).ToList();
        }

        private decimal? CalculateActualExchange(int? currencyId, DateTime? transactionDate, decimal? amount)
        {
            if(currencyId == null || transactionDate == null)
                return 0;

            var CurrencyDetailObj = db.CurrencyDetails.FirstOrDefault(q => q.CurrencyTypeId == currencyId && q.Year == transactionDate.Value.Year && q.Month == transactionDate.Value.Month);

            if (CurrencyDetailObj == null)
                return 0;

           return CurrencyDetailObj.ExchangeRate* amount;
        }

        // دي حسب البنك رئاسة او بورتسودان
        public List<BalanceVM> BalancePositionForBanksOld(int financeCycleId, int? accTreeIdForBank, DateTime? dateFrom, DateTime? dateTo)
        {
            var parentRes = new List<BalanceVM>();
            var newData = new List<int>();
            var DocTypeIds = new List<int> { 1, 3, 7, 8, 9 };
            List<int> RetrivedDataListIds = new List<int>();
            List<TransactionDetail> RetrivedDataList = new List<TransactionDetail>();
            List<TransactionDetail> TransactionDetailList = new List<TransactionDetail>();


            // دي عشان اقسم مبلغ الموازنة علي عدد الشهور المطلوب ليها التقرير
            int MonthsPercentage = 1;
            if (dateFrom != null & dateTo != null)
            {
                MonthsPercentage = ((dateFrom.Value.Year - dateTo.Value.Year) * 12) + (dateTo.Value.Month - dateFrom.Value.Month) + 1;
                if (dateTo.Value.Day < dateFrom.Value.Day)
                {
                    MonthsPercentage--;
                }

                TransactionDetailList = db.TransactionDetails
                    .Where(x => DbFunctions.TruncateTime(x.Transaction.TransactionDate) >= DbFunctions.TruncateTime(dateFrom)
                            & DbFunctions.TruncateTime(x.Transaction.TransactionDate) <= DbFunctions.TruncateTime(dateTo)).ToList();
            }
            else
            {
                TransactionDetailList = db.TransactionDetails.ToList();
            }

            if (accTreeIdForBank.HasValue)
            {
                TransactionDetailList = TransactionDetailList.Where(q => q.AccTreeId == accTreeIdForBank).ToList();
            }

            TransactionDetailList = TransactionDetailList.Where(x => DocTypeIds.Contains(x.Transaction.DocumentTypeId)).ToList();
            RetrivedDataListIds = TransactionDetailList.Where(q => q.Transaction.FinancialCycleId == financeCycleId).Select(q=> q.TransactionId).ToList();

            RetrivedDataList = db.TransactionDetails.Where(q => RetrivedDataListIds.Contains(q.TransactionId)).ToList();
            var data = trc.getAllItemsByParentId(0);
            var balanvcesIds = RetrivedDataList.Select(x => x.BalanceId).Distinct().ToList();
            var balances = db.Balances.Where(x => x.FinanceCycleId == financeCycleId && balanvcesIds.Contains(x.Id)).ToList();
            newData = data.Where(x => !balances.Any(b => b.AccountTreeId == x)).ToList();

            foreach (var item in newData)
            {
                var res = new List<BalanceVM>();

                BalanceVM objPrent = new BalanceVM();
                var acctreeObj = db.AccountTrees.Find(item);
                var acctreeParentObj = db.AccountTrees.Find(acctreeObj.AccParent);
                objPrent.accTreeName = acctreeObj.AccName;

                if (!parentRes.Any(x => x.accTreeName == objPrent.accTreeName))
                {    // objPrent.parentName = acctreeParentObj.AccName;
                    objPrent.parentId = acctreeObj.AccParent;
                    objPrent.Id = item;
                    objPrent.financeCycleId = financeCycleId;
                    objPrent.credint = balances.Where(x => x.AccountTree.AccParent == item).Sum(x => x.Credint);
                    objPrent.credintPercent = balances.Where(x => x.AccountTree.AccParent == item).Sum(x => x.Credint) / MonthsPercentage;
                    objPrent.credint = Math.Round((decimal)objPrent.credint, 2);
                    objPrent.credintPercent = Math.Round((decimal)objPrent.credintPercent, 2);

                    foreach (var item1 in balances.Where(x => x.AccountTree.AccParent == item))
                    {
                        BalanceVM obj = new BalanceVM();

                        obj.Id = item1.AccountTreeId ?? 0;
                        obj.accTreeName = item1.AccountTree.AccName;
                        obj.credint = item1.Credint;
                        obj.credintPercent = item1.Credint / MonthsPercentage;
                        obj.parentId = item;
                        obj.actualExchange = RetrivedDataList.Where(x => x.BalanceId == item1.Id).Sum(x => x.Debit + x.Credit);

                        res.Add(obj);
                    }

                    objPrent.sumOfCredint = res.Sum(x => x.credint);
                    objPrent.actualExchange = res.Sum(x => x.actualExchange);
                    objPrent.details = res;
                    // objPrent.details = res.Where(x=> x.credint > 0).ToList();
                    parentRes.Add(objPrent);
                }

            }
            return parentRes.Where(x => x.credint > 0).ToList();
        }

        public List<BalanceVM> BalancePositionForBanks(int financeCycleId, int? accTreeIdForBank, DateTime? dateFrom, DateTime? dateTo)
        {
            var parentRes = new List<BalanceVM>();
            // var docTypeIds = new List<int> { 1, 3, 7, 8, 9 };

            // حساب عدد الشهور
            int monthsPercentage = 1;
            if (dateFrom.HasValue && dateTo.HasValue)
            {
                monthsPercentage = ((dateTo.Value.Year - dateFrom.Value.Year) * 12) + (dateTo.Value.Month - dateFrom.Value.Month) + 1;
                if (dateTo.Value.Day < dateFrom.Value.Day)
                    monthsPercentage--;
            }

            // تجهيز الاستعلام لفلترة تفاصيل المعاملات
            var transactionDetailsQuery = db.TransactionDetails.AsQueryable();

            if (dateFrom.HasValue && dateTo.HasValue)
            {
                var from = dateFrom.Value.Date;
                var to = dateTo.Value.Date;
                transactionDetailsQuery = transactionDetailsQuery
                    .Where(x => DbFunctions.TruncateTime(x.Transaction.TransactionDate) >= from &&
                                DbFunctions.TruncateTime(x.Transaction.TransactionDate) <= to);
            }

            if (accTreeIdForBank.HasValue)
            {
                transactionDetailsQuery = transactionDetailsQuery
                    .Where(x => x.AccTreeId == accTreeIdForBank.Value);
            }

            //transactionDetailsQuery = transactionDetailsQuery
            //    .Where(x => docTypeIds.Contains(x.Transaction.DocumentTypeId));

            var transactionDetailsList = transactionDetailsQuery
                .Include(x => x.Transaction)
                .ToList();

            var transactionIds = transactionDetailsList
                .Where(q => q.Transaction.FinancialCycleId == financeCycleId)
                .Select(q => q.TransactionId)
                .Distinct()
                .ToList();

            var retrievedDataList = db.TransactionDetails
                .Where(q => transactionIds.Contains(q.TransactionId))
                .ToList();

            var data = trc.getAllItemsByParentId(0);

            var balanceIds = retrievedDataList
                .Select(x => x.BalanceId)
                .Distinct()
                .ToList();

            var balances = db.Balances
                .Include(b => b.AccountTree)
                .Where(x => x.FinanceCycleId == financeCycleId && balanceIds.Contains(x.Id))
                .ToList();

            var newData = data
                .Where(x => !balances.Any(b => b.AccountTreeId == x))
                .ToList();

            foreach (var item in newData)
            {
                var acctreeObj = db.AccountTrees.Find(item);
                if (acctreeObj == null) continue;

                if (parentRes.Any(x => x.accTreeName == acctreeObj.AccName)) continue;

                var childBalances = balances
                    .Where(x => x.AccountTree.AccParent == item)
                    .ToList();

                var details = childBalances.Select(item1 => new BalanceVM
                {
                    Id = item1.AccountTreeId ?? 0,
                    accTreeName = item1.AccountTree.AccName,
                    credint = Math.Round(item1.Credint ?? 0, 2),
                    credintPercent = Math.Round(item1.Credint ?? 0 / monthsPercentage, 2),
                    parentId = item,
                    actualExchange = retrievedDataList
                                        .Where(x => x.BalanceId == item1.Id)
                                        .Sum(x => x.Debit + x.Credit)
                }).ToList();

                var parentDto = new BalanceVM
                {
                    Id = item,
                    accTreeName = acctreeObj.AccName,
                    parentId = acctreeObj.AccParent,
                    financeCycleId = financeCycleId,
                    credint = Math.Round(childBalances.Sum(x => x.Credint ?? 0), 2),
                    credintPercent = Math.Round(childBalances.Sum(x => x.Credint ?? 0) / monthsPercentage, 2),
                    sumOfCredint = details.Sum(x => x.credint),
                    actualExchange = details.Sum(x => x.actualExchange),
                    details = details
                };

                parentRes.Add(parentDto);
            }

            return parentRes.Where(x => x.credint > 0).ToList();
        }

        /*---------------------------------------------------------------تقارير الحسابات الختامية--------------------------------------------------*/
        // تقرير ميزان المراجعة بالارصدة- يعني كل حساب  والرصيد ال فيهو

        public List<BalanceVM> TrailBalance1(int financeCycleId)
        {
            var parentRes = new List<BalanceVM>();
            var newData = new List<int>();
            var DocTypeIds = new List<int> { 1, 3, 7, 8, 9 };
            List<int> RetrivedDataListIds = new List<int>();
            List<TransactionDetail> RetrivedDataList = new List<TransactionDetail>();

            // دي عشان اقسم مبلغ الموازنة علي عدد الشهور المطلوب ليها التقرير
           
            RetrivedDataList = db.TransactionDetails.Where(x => x.Transaction.FinancialCycleId == financeCycleId).ToList();

            if(RetrivedDataList == null)
            {
                return null;
            }

            foreach(var item in RetrivedDataList)
            {
                BalanceVM obj =new BalanceVM();
                obj.credint = item.Credit;
                obj.actualExchange = item.Debit;
                obj.accTreeName = item.BalanceId != null ? item.Balance.AccountTree.AccName: "";
                obj.Id = item.BalanceId != null ? (int)item.BalanceId : 0;

                parentRes.Add(obj);
            }

            //parentRes = RetrivedDataList.Select(p =>
            //     new BalanceVM()
            //     {
            //         credint = p.Credit,
            //         actualExchange = p.Debit,
            //         accTreeName = p.Balance.AccountTree.AccName,
            //         Id = p.BalanceId != null ? (int)p.BalanceId : 0,
            //     }).ToList();

            return parentRes;



            //var data = trc.getAllItemsByParentId(0);
            //var balanvcesIds = RetrivedDataList.Select(x => x.BalanceId).Distinct().ToList();
            //var balances = db.Balances.Where(x => x.FinanceCycleId == financeCycleId && balanvcesIds.Contains(x.Id)).ToList();
            //newData = data.Where(x => !balances.Any(b => b.AccountTreeId == x)).ToList();

            //foreach (var item in newData)
            //{
            //    var res = new List<BalanceVM>();

            //    BalanceVM objPrent = new BalanceVM();
            //    var acctreeObj = db.AccountTrees.Find(item);
            //    var acctreeParentObj = db.AccountTrees.Find(acctreeObj.AccParent);
            //    objPrent.accTreeName = acctreeObj.AccName;

            //    if (!parentRes.Any(x => x.accTreeName == objPrent.accTreeName))
            //    {    // objPrent.parentName = acctreeParentObj.AccName;
            //        objPrent.parentId = acctreeObj.AccParent;
            //        objPrent.Id = item;
            //        objPrent.financeCycleId = financeCycleId;
            //        objPrent.credint = balances.Where(x => x.AccountTree.AccParent == item).Sum(x => x.Credint);
            //        // objPrent.credintPercent = balances.Where(x => x.AccountTree.AccParent == item).Sum(x => x.Credint) / MonthsPercentage;
            //        objPrent.credint = Math.Round((decimal)objPrent.credint, 2);
            //        objPrent.credintPercent = Math.Round((decimal)objPrent.credintPercent, 2);

            //        foreach (var item1 in balances.Where(x => x.AccountTree.AccParent == item))
            //        {
            //            BalanceVM obj = new BalanceVM();

            //            obj.Id = item1.AccountTreeId ?? 0;
            //            obj.accTreeName = item1.AccountTree.AccName;
            //            obj.credint = item1.Credint;
            //            // obj.credintPercent = item1.Credint / MonthsPercentage;
            //            obj.parentId = item;
            //            obj.actualExchange = RetrivedDataList.Where(x => x.BalanceId == item1.Id).Sum(x => x.Debit + x.Credit);

            //            res.Add(obj);
            //        }

            //        objPrent.sumOfCredint = res.Sum(x => x.credint);
            //        objPrent.actualExchange = res.Sum(x => x.actualExchange);
            //        objPrent.details = res;
            //        // objPrent.details = res.Where(x=> x.credint > 0).ToList();
            //        parentRes.Add(objPrent);
            //    }

            //}
            // return parentRes.Where(x => x.credint > 0).ToList();
        }

        public BalanceVM TrailBalanceOld(int financeCycleId, int ParentId, List<TransactionDetail> RetrivedDataList, bool isExpense)
        {
            var parentRes = new List<BalanceVM>();
            var newData = new List<int>();
            // List<TransactionDetail> RetrivedDataList = new List<TransactionDetail>();
            // RetrivedDataList = db.TransactionDetails.Where(x => x.Transaction.FinancialCycleId == financeCycleId && x.BalanceId != null && x.Balance.Credint > 0).ToList();

            var data = trc.getAllItemsByParentId(ParentId);
            var tobObjPrentAccName = db.AccountTrees.Find(ParentId).AccName;
            var balances = db.Balances.Where(x => x.FinanceCycleId == financeCycleId).ToList();
            newData = data.Where(x => !balances.Any(b => b.AccountTreeId == x)).ToList();            
            // newData = data.Where(x => !db.AccountSubs.Any(s => s.AccTreeId == x)).ToList();
            // newData = isExpense == true ? data.Where(x => !balances.Any(b => b.AccountTreeId == x)).ToList() : data.ToList();
            BalanceVM tobObjPrent = new BalanceVM();

            foreach (var item in newData)
            {
                var res = new List<BalanceVM>();

                BalanceVM objPrent = new BalanceVM();

                objPrent.accTreeName = db.AccountTrees.Find(item).AccName;
                objPrent.parentName = tobObjPrentAccName;
                objPrent.parentId = ParentId;
                objPrent.Id = item;
                objPrent.financeCycleId = financeCycleId;
                objPrent.credint = balances.Where(x => x.AccountTree.AccParent == item).Sum(x => x.Credint);
                objPrent.credint = Math.Round((decimal)objPrent.credint, 2);

                foreach (var item1 in balances.Where(x => x.AccountTree.AccParent == item))
                {
                    BalanceVM obj = new BalanceVM();

                    obj.Id = item1.AccountTreeId ?? 0;
                    obj.accTreeName = item1.AccountTree.AccName;
                    obj.credint = item1.Credint;
                    obj.credintPercent = objPrent.credint;
                    obj.parentId = item;
                    obj.sumOfCredit = RetrivedDataList.Where(x => x.BalanceId == item1.Id).Sum(x => x.Credit);
                    obj.sumOfDebit = RetrivedDataList.Where(x => x.BalanceId == item1.Id).Sum(x => x.Debit);
                    obj.actualExchange = RetrivedDataList.Where(x => x.BalanceId == item1.Id).Sum(x => x.Debit) - RetrivedDataList.Where(x => x.BalanceId == item1.Id).Sum(x => x.Credit);

                    res.Add(obj);
                }

                objPrent.sumOfCredint = res.Sum(x => x.credint);
                objPrent.actualExchange = res.Sum(x => x.actualExchange);
                objPrent.sumOfCredit = res.Sum(x => x.sumOfCredit);
                objPrent.sumOfDebit = res.Sum(x => x.sumOfDebit);
                objPrent.details = res.Where(x => x.credint > 0).ToList();
                parentRes.Add(objPrent);
            }

            tobObjPrent.sumOfDebit = parentRes.Sum(x => x.sumOfDebit);
            tobObjPrent.sumOfCredit = parentRes.Sum(x => x.sumOfCredit);
            tobObjPrent.sumOfCredint = parentRes.Sum(x => x.sumOfCredint);
            tobObjPrent.accTreeName = tobObjPrentAccName;
            tobObjPrent.actualExchange = tobObjPrent.sumOfDebit - tobObjPrent.sumOfCredit;
            tobObjPrent.details = parentRes.Where(x => x.sumOfCredint > 0).ToList();

            // return parentRes.Where(x => x.credint > 0).ToList();
            return tobObjPrent;
        }

        public BalanceVM TrailBalance(int financeCycleId, int ParentId, List<TransactionDetail> RetrivedDataList, bool isExpense)
        {
            if (isExpense)
            {
                return TrailBalanceInBalance(financeCycleId, ParentId, RetrivedDataList);
            }
            else
            {
                return TrailBalanceOutBalance(financeCycleId, ParentId, RetrivedDataList, new List<int>(), isExpense);
            }
        }

        public BalanceVM TrailBalanceInBalance(int financeCycleId, int ParentId, List<TransactionDetail> RetrivedDataList)
        {
            var parentRes = new List<BalanceVM>();
            BalanceVM tobObjPrent = new BalanceVM();

            var data = trc.getAllItemsByParentId(ParentId, true);
            var tobObjPrentAccName = db.AccountTrees.Find(ParentId).AccName;
            var balances = db.Balances.Where(x => x.FinanceCycleId == financeCycleId).ToList();
            List<int> newData = data.Where(x => !balances.Any(b => b.AccountTreeId == x)).ToList();
            
            foreach (var item in newData)
            {
                var res = new List<BalanceVM>();

                BalanceVM objPrent = new BalanceVM();

                objPrent.accTreeName = db.AccountTrees.Find(item).AccName;
                objPrent.parentName = tobObjPrentAccName;
                objPrent.parentId = ParentId;
                objPrent.Id = item;
                objPrent.financeCycleId = financeCycleId;
                objPrent.credint = balances.Where(x => x.AccountTree.AccParent == item).Sum(x => x.Credint);
                objPrent.credint = Math.Round((decimal)objPrent.credint, 2);

                foreach (var item1 in balances.Where(x => x.AccountTree.AccParent == item))
                {
                    BalanceVM obj = new BalanceVM();

                    obj.Id = item1.AccountTreeId ?? 0;
                    obj.accTreeName = item1.AccountTree.AccName;
                    obj.credint = item1.Credint;
                    obj.credintPercent = objPrent.credint;
                    obj.parentId = item;
                    obj.sumOfCredit = RetrivedDataList.Where(x => x.BalanceId == item1.Id).Sum(x => x.Credit);
                    obj.sumOfDebit = RetrivedDataList.Where(x => x.BalanceId == item1.Id).Sum(x => x.Debit);
                    obj.actualExchange = RetrivedDataList.Where(x => x.BalanceId == item1.Id).Sum(x => x.Debit) - RetrivedDataList.Where(x => x.BalanceId == item1.Id).Sum(x => x.Credit);

                    res.Add(obj);
                }

                objPrent.sumOfCredint = res.Sum(x => x.credint);
                objPrent.actualExchange = res.Sum(x => x.actualExchange);
                objPrent.sumOfCredit = res.Sum(x => x.sumOfCredit);
                objPrent.sumOfDebit = res.Sum(x => x.sumOfDebit);
                objPrent.details = res.Where(x => x.credint > 0).ToList();
                parentRes.Add(objPrent);
            }

            tobObjPrent.Id = ParentId;
            tobObjPrent.financeCycleId = financeCycleId;
            tobObjPrent.sumOfDebit = parentRes.Sum(x => x.sumOfDebit);
            tobObjPrent.sumOfCredit = parentRes.Sum(x => x.sumOfCredit);
            tobObjPrent.sumOfCredint = parentRes.Sum(x => x.sumOfCredint);
            tobObjPrent.accTreeName = tobObjPrentAccName;
            tobObjPrent.actualExchange = tobObjPrent.sumOfDebit - tobObjPrent.sumOfCredit;
            tobObjPrent.details = parentRes.Where(x => x.sumOfCredint > 0).ToList();

            return tobObjPrent;
        }

        public BalanceVM TrailBalanceOutBalance(int financeCycleId, int parentId, List<TransactionDetail> retrievedDataList, List<int> oldList, bool isExpense)
        {
            // إنشاء كائن الحساب الأب
            var tobObjParent = new BalanceVM
            {
                Id = parentId,
                financeCycleId = financeCycleId,
                accTreeName = db.AccountTrees.Find(parentId)?.AccName ?? "Unknown",
                sumOfDebit = 0,
                sumOfCredit = 0,
                sumOfCredint = 0,
                actualExchange = 0,
                details = new List<BalanceVM>()
            };
            
            // إضافة العنصر الحالي إلى قائمة العناصر المعالجة
            oldList.Add(parentId);

            // جلب جميع الأبناء المباشرين
            var childItems = trc.getAllItemsByParentId(parentId, true);

            foreach (var child in childItems)
            {
                if (!oldList.Contains(child))
                {
                    // استدعاء الوظيفة بشكل متكرر لمعالجة الأبناء
                    var childBalance = TrailBalanceOutBalance(financeCycleId, child, retrievedDataList, oldList, isExpense);

                    // إضافة التفاصيل إلى الحساب الأب
                    tobObjParent.details.Add(childBalance);

                    // تحديث المجاميع بناءً على البيانات المسترجعة
                    tobObjParent.sumOfDebit += childBalance.sumOfDebit;
                    tobObjParent.sumOfCredit += childBalance.sumOfCredit;
                    tobObjParent.isExpense = isExpense;
                    tobObjParent.accTreeCode = childBalance.accTreeCode;
                }
            }
            tobObjParent.isExpense = isExpense;

            // جمع القيم المالية الخاصة بالحساب الحالي فقط
            tobObjParent.sumOfDebit += retrievedDataList
                .Where(x => x.AccTreeId == parentId)
                .Sum(x => x.Debit);

            tobObjParent.sumOfCredit += retrievedDataList
                .Where(x => x.AccTreeId == parentId)
                .Sum(x => x.Credit);
            // tobObjParent.actualExchange = tobObjParent.sumOfCredit - tobObjParent.sumOfDebit;

            // حساب الفرق
            if (isExpense)
            {
                tobObjParent.actualExchange = tobObjParent.sumOfDebit - tobObjParent.sumOfCredit;
            }
            else
            {
                tobObjParent.actualExchange = tobObjParent.sumOfCredit - tobObjParent.sumOfDebit;
            }
            return tobObjParent;
        }

    }
}

