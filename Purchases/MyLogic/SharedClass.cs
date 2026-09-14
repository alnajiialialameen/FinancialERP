using Microsoft.Ajax.Utilities;
using Purchases.Models;
using Purchases.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Purchases.MyLogic
{
    // الكلاس ده لي الدوال المستخدمة في مختلف الدوال في النظام بغرض توحيد الدوال
    // مفترض كلها تكون في كونترولر واحدة ح نسميها بي نفس الاسم ونخلي كل الشاشات تنادي من ال كونترولر ده 
    public class SharedClass
    {
        private Entities db = new Entities();

        // تحميل البيانات في شاشة سندات الصرف
        public List<TransactionVM> LoadData(string userId)
        {
            try
            {
                int financialCycleId = GetUserCurrentFinancialCycleId(userId);
                var data = db.Transactions.Where(x => x.FinancialCycleId == financialCycleId && x.IsPosted != true)
                .Select(p => new TransactionVM()
                {
                    transactionId = p.Id,
                    currency = p.CurrencyType.Name,
                    documentType = p.DocumentType.Name,
                    total = p.Amount,
                    documentTypeId = p.DocumentTypeId,
                    exchangeRate = p.ExchangeRate != null ? p.ExchangeRate : 0,
                    transactionDateStr = p.TransactionDate.Value.Year + "/" + p.TransactionDate.Value.Month + "/" + p.TransactionDate.Value.Day,
                    note = p.Note,
                    hasAddedTaxTxt = p.HasAddedTax == true ? "مضمن 17%" : "غير مضمنة",
                    hasAddedTax = p.HasAddedTax,
                    hasTax = p.HasTax,
                    recipient = p.Recipient??""
                }).OrderByDescending(d => d.transactionId).ToList();

                //data = documentTypeId > 0 ? data.Where(x => x.documentTypeId == documentTypeId).ToList() : data;
                return data;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<DDListObj> GetAccTrees(string q)
        {
            //var data = db.AccountTrees.Where(x => db.AccountSubs.Any(s => s.AccTreeId == x.Id))
            //            .Select(p => new DDListObj() { id = p.Id, text = p.AccName }).Where(f => f.text.Contains(q)).ToList();

            // تم التعديل ليعرض IsActive فقط
            var data = db.AccountTrees.Where(x => db.AccountSubs.Any(s => s.AccTreeId == x.Id) && x.IsActive ==true)
                        .Select(p => new DDListObj() { id = p.Id, text = p.AccName }).Where(f => f.text.Contains(q)).ToList();
            return data;
        }

        public List<DDListObj> GetAccTreesAll(string q)
        {
            var data = db.AccountTrees
                        .Select(p => new DDListObj() { id = p.Id, text = p.AccName })
                        .Where(f => f.text.Contains(q))
                        .ToList();

            return data;
        }

        public List<DDListObj> getBalanceIds(string q, string userId)
        {
            int FinanceCycleId = GetUserCurrentFinancialCycleId(userId);
            var data = db.Balances.Where(x => x.FinanceCycleId == FinanceCycleId)
                        .Select(p => new DDListObj { id = p.Id, text = p.AccountTree.AccName }).Where(f => f.text.Contains(q)).ToList();

            return data;
        }
        // جلب العمللات
        public List<DDListObj> GetCurrency(string q)
        {
            var data = db.CurrencyTypes
                        .Select(p => new DDListObj() { id = p.Id, text = p.Name }).Where(f => f.text.Contains(q)).ToList();

            return data;
        }
        // جلب نوع السند
        public List<DDListObj> GetDocumentType(string q)
        {
            var data = db.DocumentTypes
                        .Select(p => new DDListObj() { id = p.Id, text = p.Name }).Where(f => f.text.Contains(q)).ToList();

            return data;
        }

        // لمن اختار البنك يرجع لي نوع العملة
        public int GetCurrencyId(int bankId)
        {
            var subAccId = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == bankId).Id;
            var obj = db.BankAccounts.FirstOrDefault(x => x.AccountSubId == subAccId);

            int CurrencyTypeId = Convert.ToInt32(obj.CurrencyTypeId);

            return CurrencyTypeId;
        }

        // اختبار هل الحركة دي فيها حساب من نوع بنك
        public int IsTransactionHasBankAccount(int TransactionId)
        {
            try
            {
                var dataList = db.TransactionDetails.Where(x => x.TransactionId == TransactionId).ToList();
                var accountsList = db.AccountTrees.Where(x => x.AccParent == 7 & x.AccountSubs.Any(s => s.AccTreeId == x.Id));
                foreach (var item in dataList)
                {
                    // رقم الحساب الاب حق البنوك كلها هو 7
                    if (accountsList.Any(x => x.Id == item.AccTreeId)) //هل هو فعلا حساب فرعي و هل هو من نوع بنوك
                    {
                        var obj = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == item.AccTreeId);

                        // كده رجعنا رقم البنك
                        return db.BankAccounts.FirstOrDefault(x => x.AccountSubId == obj.Id).Id;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        // اختبار هل الحركة دي فيها حساب من نوع بنك اذا متوفر رجع مبلغ البنك
        public decimal? GetAmountForBankAccountInTransaction(int TransactionId)
        {
            try
            {
                var dataList = db.TransactionDetails.Where(x => x.TransactionId == TransactionId).ToList();
                var accountsList = db.AccountTrees.Where(x => x.AccParent == 7 & x.AccountSubs.Any(s => s.AccTreeId == x.Id));
                foreach (var item in dataList)
                {
                    // رقم الحساب الاب حق البنوك كلها هو 7
                    if (accountsList.Any(x => x.Id == item.AccTreeId)) //هل هو فعلا حساب فرعي و هل هو من نوع بنوك
                    {
                        // كده رجعنا مبلغ البنك
                        return item.Credit + item.Debit;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }
        // اختبار هل الحركة دي فيها حساب من نوع بنك
        public bool IsItBankAccount(int AccTreeId)
        {
            try
            {
                // var dataList = db.TransactionDetails.Where(x => x.TransactionId == TransactionId).ToList();
                var accountsList = db.AccountTrees.Where(x => x.AccParent == 7 &&
                                    x.AccountSubs.Any(s => s.AccTreeId == AccTreeId));

                return accountsList.Any();
            }
            catch (Exception e)
            {
                return false;
            }
        }

        // اختبار هل الحركة دي فيها حساب بنكي ام لا
        public int IsItBankAccount(int TransactionId, decimal? CreditVal = 0)
        {
            try
            {
                var dataList = db.TransactionDetails.Where(x => x.TransactionId == TransactionId && x.Credit == CreditVal).ToList();

                var accountsList = db.AccountTrees.Where(x => x.AccParent == 7 &&  x.AccountSubs.Any(s => s.AccTreeId == x.Id));

                foreach (var item in dataList)
                {
                    // رقم الحساب الاب حق البنوك كلها هو 7
                    if (accountsList.Any(x => x.Id == item.AccTreeId)) //هل هو فعلا حساب فرعي و هل هو من نوع بنوك
                    {
                        var obj = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == item.AccTreeId);

                        // كده رجعنا رقم البنك
                        return db.BankAccounts.FirstOrDefault(x => x.AccountSubId == obj.Id).Id;
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        // جلب اخر رقم شيك لادخالة تلقائيا في العمليات عند طباعة الشيك 
        public int GetLastUsedCheckNo(int Id)
        {
            try
            {
                if (this.IsTransactionHasBankAccount(Id) > 0)
                {
                    var AccountSubObj = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == Id);
                    var BankAccountObj = db.BankAccounts.FirstOrDefault(x => x.AccountSubId == AccountSubObj.Id);

                    if (db.Checks.Any(x => x.BankAccountId == BankAccountObj.Id & x.IsFinished != true))
                    {
                        if (db.Checks.Any(x => x.BankAccountId == BankAccountObj.Id & x.LastUsedCheckNo != x.EndToNumber & x.IsFinished != true))
                        {
                            var currenCeckOtbj = db.Checks.Where(x => x.BankAccountId == BankAccountObj.Id & x.LastUsedCheckNo != x.EndToNumber & x.IsFinished != true).OrderBy(o => o.StartFromNumber).FirstOrDefault();
                            return currenCeckOtbj.Id;
                        }
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        // دي بجيب بيها السنة المالية الحالية نفسها
        public string GetCurrentFinancialCycleYear()
        {
            try
            {
                string Year = "";
                Year = db.FinancialCycles.FirstOrDefault(x => x.CurrentYear == true).Year.ToString();

                return Year;
            }
            catch (Exception e)
            {
                throw new NullReferenceException(e.Message);
            }
        }

        // دي بجيب بيها السنة المالية الحالية للمستخدم الحالي
        public string GetUserCurrentFinancialCycleYear(string userId)
        {
            try
            {
                string Year = db.AspNetUsers.Find(userId).FinancialCycle.Year.ToString();

                return Year;
            }
            catch (Exception e)
            {
                throw new NullReferenceException(e.Message);
            }
        }

        // دي بجيب بيها السنة المالية الحالية للمستخدم الحالي
        public string GetCurrentTransactionFinancialCycleYear(int transactionId)
        {
            try
            {
                string Year = db.Transactions.Find(transactionId).FinancialCycle.Year.ToString();

                return Year;
            }
            catch (Exception e)
            {
                throw new NullReferenceException(e.Message);
            }
        }

        // دي بجيب بيها رقم السنة المالية الحالية للمستخدم الحالي
        public int GetUserCurrentFinancialCycleId(string userId)
        {
            try
            {
                int? Year = 0;
                Year = db.AspNetUsers.Find(userId).FinancialCycleId;

                return Convert.ToInt32(Year);
            }
            catch (Exception e)
            {
                throw new NullReferenceException(e.Message);
            }
        }

        // دي بجيب بيها سعر الصرف للشهر الحالي
        public decimal GetCurrentMonthExchangeRate()
        {
            try
            {
                DateTime now = DateTime.Now;
                decimal res = 0;
                res = db.CurrencyDetails.Where(x => x.Year == now.Year & x.Month == now.Month & x.IsActive != false).FirstOrDefault().ExchangeRate;

                return res;
            }
            catch (Exception e)
            {
                // throw new NullReferenceException(e.Message);

                return 0;
            }
        }

        // دي بجيب بيها سعر الصرف للشهر الحالي
        public decimal GetCurrentMonthExchangeRate(DateTime transactionDate, int? currncyId)
        {
            try
            {
                decimal res = 1;
                if (db.CurrencyDetails.Any(x => x.CurrencyTypeId == currncyId &&
                x.Year == transactionDate.Year & x.Month == transactionDate.Month & x.IsActive != false))
                {
                    res = db.CurrencyDetails.FirstOrDefault(x => x.CurrencyTypeId == currncyId && x.Year == transactionDate.Year & x.Month == transactionDate.Month & x.IsActive != false).ExchangeRate;
                }
                return res;
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        /// <summary>
        /// دي برسل ليها كل البنود ال دخلت في العملية لو كانت حساب بنك او ليست حساب بنك وبترجع لي رقم البنك
        /// (رقم البنك في جدول البنوك بختلف من رقم البنك في الشجرة) اذا هو حساب بنكي ولو ما حساب بنكي بتجاهل القيمه
        /// </summary>
        /// <param name="accTreeIdList"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<int> getBankIds(List<int?> accTreeIdList)
        {
            try
            {
                if (accTreeIdList == null)
                {
                    return new List<int>(); // null data
                }

                var accountSubIdList = db.AccountSubs.Where(x => accTreeIdList.Contains(x.AccTreeId) & x.AccCategoryId == 1).Select(s => s.Id).ToList();

                if (accountSubIdList == null)
                {
                    new List<int?>(); // null data
                }

                return db.BankAccounts.Where(x => accountSubIdList.Contains(x.AccountSubId ?? 0)).Select(s => s.Id).ToList(); // return bank Ids
            }
            catch (Exception e)
            {
                throw new Exception(e.Message); // an exception
            }
        }

        // اضافة قيد 
        public int Create(List<TransactionVM> modal, string userid)
        {
            db.Database.CommandTimeout = 220;
            using (var scope = db.Database.BeginTransaction())
            {
                try
                {
                    if (modal.Sum(x => x.credit) != modal.Sum(x => x.debit))
                    {
                        return 0;
                    }

                    // ده عشان اجيب السنة المالية لي تاريخ العملية
                    string currentTransactionYear = Convert.ToDateTime(modal[0].transactionDate).Year.ToString();

                    // لو العام المالي لي تاريخ العملية  مغلق ما ينفذ العملية
                    //if (db.FinancialCycles.Any(x => x.Year == currentTransactionYear && x.CurrentYear == false))
                    //{
                    //    return -1000;
                    //}

                    // اذا السنة المالية مقفولة ما يعمل اي حاجه
                    //int FinancialCycleId = this.GetFinancialCycleIdForSpecificDate(currentTransactionYear); // هنا دي مفترض تتحول حسب العام المالي للمستخدم
                    int FinancialCycleId = this.GetUserCurrentFinancialCycleId(userid); // جلب العام المالي للمستخدم الحالي
                    if (db.FinancialCycles.Any(x => x.Id == FinancialCycleId && x.IsClosed == true))
                    {
                        return -1000;
                    }

                    DateTime now = DateTime.Now;
                    Transaction t = new Transaction();
                    List<TransactionDetail> tdList = new List<TransactionDetail>();

                    decimal? bankAmount = 0;
                    decimal? totalWithoutbankAmount = 0;
                    foreach (var item in modal)
                    {
                        if(IsItBankAccount(item.accTrreId??0))
                        {
                            bankAmount = item.credit;
                            break;
                        }

                        totalWithoutbankAmount += item.credit;
                    }

                    decimal? amount = bankAmount - totalWithoutbankAmount;

                    t.CurrencyId = modal[0].currencyId;
                    t.RecipientId = modal[0].recipientId;
                    t.TransactionDate = Convert.ToDateTime(modal[0].transactionDate);
                    t.CreatedDate = now;
                    t.Note = modal[0].note;
                    t.DocumentTypeId = modal[0].documentTypeId; 
                    t.Amount = modal.Sum(x => x.debit);
                    // t.Amount = amount;
                    t.Recipient = modal[0].recipient;
                    t.ExchangeRate =  this.GetCurrentMonthExchangeRate((DateTime)t.TransactionDate, t.CurrencyId);
                    t.FinancialCycleId = FinancialCycleId;
                    t.CreatedBy = userid;
                    t.CreatedDate = now;
                    t.CreationDate = now;

                    var balances = db.Balances.Where(x => x.FinanceCycleId == FinancialCycleId);

                    foreach (var item in modal)
                    {
                        TransactionDetail td = new TransactionDetail();

                        td.AccTreeId = Convert.ToInt32(item.accTrreId);
                        td.TransactionId = t.Id;
                        td.Debit = item.debit;
                        td.Credit = item.credit;
                        td.Note = modal.FirstOrDefault().note;
                        td.CreatedBy = userid;
                        td.CreationDate = now;
                        
                        if (balances.Any(x => x.AccountTreeId == item.accTrreId))
                        {
                            decimal Amount = Convert.ToDecimal(item.debit + item.credit);
                            int BalanceId = balances.FirstOrDefault(x => x.AccountTreeId == item.accTrreId).Id;

                            myExtention.UpdateActualExchange(BalanceId, 0, Amount, userid);

                            td.BalanceId = BalanceId;
                        }

                        tdList.Add(td);
                    }

                    // اذا تم اضافة بند ضريبة ال 1% في الادخال
                    // عدل العملية لتكون قيمة HasTax هي true
                    if (modal.Any(x => x.accTrreId == 228))
                    {
                        t.HasTax = true;
                    }

                    db.Transactions.Add(t);
                    db.TransactionDetails.AddRange(tdList);
                    db.SaveChanges();


                    // اضافة مستلم الي جدول المستلمين ان وجد
                    //if(modal.Any(x => x.recipientId > 0))
                    //{
                    //    var recipient = db.Recipients.Find(modal.FirstOrDefault().recipientId);

                    //    var bankAccountId = this.IsTransactionHasBankAccount(t.Id);
                    //    //var bankAmount = this.IsTransactionHasBankAccount(t.Id);

                    //    var transcationReceipts = new TransactionRecipient();

                    //    transcationReceipts.TransactionId = t.Id;
                    //    transcationReceipts.RecipientName = recipient.RecipientName;
                    //    transcationReceipts.BankName = recipient.BankName;
                    //    transcationReceipts.BranchName = recipient.BranchName;
                    //    transcationReceipts.AccountNumber = recipient.AccountNumber;
                    //    transcationReceipts.BankAccountId = bankAccountId;
                    //    transcationReceipts.Amount = bankAmount;

                    //    db.TransactionRecipients.Add(transcationReceipts);
                    //    db.SaveChanges();
                    //}

                    scope.Commit();
                    return t.Id;
                }
                catch (Exception e)
                {
                    scope.Rollback();
                    return -1;
                }
            }
        }

        public string configDate(DateTime? date)
        {
            if(date == null)
            {
                return "";
            }

            return date.Value.Year + "/" + date.Value.Month + "/" + date.Value.Day;
        }

        public List<dynamic> GetTransactionBankAccounts(int id)
        {
            var data = db.TransactionDetails.Where(q => q.TransactionId == id).ToList();
            var accTree = db.AccountTrees.Where(q => q.AccParent == 7).ToList();

            List<dynamic> result = new List<dynamic>();
            foreach (var trans in data)
            {
                if (this.IsItBankAccount(trans.AccTreeId) && accTree.Any(q => q.Id == trans.AccTreeId))
                {
                    var obj = accTree.FirstOrDefault(q => q.Id == trans.AccTreeId);
                    result.Add(new { id = obj.Id, text = obj.AccName, amount = trans.Credit + trans.Debit });
                }
            }

            return result;
        }

        public BalanceVM GetOpenBalanceForSpecificAccTree(int financeCycleYear, int? financeCycleId, int accTreeId, int? bankId = null)
        {
            try
            {
                var date = new DateTime(financeCycleYear, 1, 1);
                string tarnsactionDateStr = this.configDate(date);

                // تجهيز الاستعلام الأساسي
                List<int> TransactionIdsList = new List<int>();

                if (bankId != null)
                {
                    TransactionIdsList = db.Transactions
                        .Where(t => t.FinancialCycleId == financeCycleId - 1 &&
                                    t.TransactionDetails.Any(td => td.AccTreeId == bankId))
                        .Select(q => q.Id)
                        .ToList();
                }
                else
                {
                    TransactionIdsList = db.Transactions
                        .Where(t => t.FinancialCycleId == financeCycleId - 1)
                        .Select(q => q.Id)
                        .ToList();
                }

                var data = db.TransactionDetails
                    .Where(q => TransactionIdsList.Contains(q.TransactionId) && q.AccTreeId == accTreeId).ToList();

                var res = new BalanceVM()
                {
                    debit = data.Sum(d => d.Debit),
                    credit = data.Sum(d => d.Credit),
                    accTreeId = accTreeId,
                    financeCycleId = financeCycleId??0,
                    note = "رصيد أول المدة",
                    transactionDate = date,
                    transactionDateStr = tarnsactionDateStr,
                };

                res.Diff = res.debit >= res.credit? res.debit - res.credit : res.credit - res.debit;
                res.debit = res.debit > res.credit? res.Diff:0;
                res.credit = res.credit > res.debit? res.Diff:0;

                return res;
            }
            catch (Exception e)
            {
                return new BalanceVM();
            }
        }


        public BalanceVM GetOpenBalanceForBank(int financeCycleYear, int? financeCycleId, int bankId)
        {
            try
            {
                var date = new DateTime(financeCycleYear, 1, 1);
                string tarnsactionDateStr = this.configDate(date);

                // تجهيز الاستعلام الأساسي
                var TransactionIdsList = db.Transactions
                    .Where(t => t.FinancialCycleId == financeCycleId - 1 &&
                                t.TransactionDetails.Any(td => td.AccTreeId == bankId))
                    .Select(q => q.Id)
                    .ToList();

                var data = db.TransactionDetails
                    .Where(q => TransactionIdsList.Contains(q.TransactionId)).ToList();

                var res = new BalanceVM()
                {
                    debit = data.Sum(d => d.Debit),
                    credit = data.Sum(d => d.Credit),
                    accTreeId = bankId,
                    financeCycleId = financeCycleId ?? 0,
                    note = "رصيد أول المدة",
                    transactionDate = date,
                    transactionDateStr = tarnsactionDateStr,
                };

                return res;
            }
            catch (Exception e)
            {
                return new BalanceVM();
            }
        }


        public List<BalanceVM> GetOpenBalanceForSpecificAccTree(int financeCycleYear, int? financeCycleId, int accTreeId)
        {
            var date = new DateTime(financeCycleYear, 1, 1);
            string tarnsactionDateStr = this.configDate(date);
            return db.OpeningBalanceDetails.Where(q=> q.OpeningBalance.FinancialCycleId == financeCycleId && q.AccTreeId ==   accTreeId).Include(q=>q.OpeningBalance)
                .Select(q=> new BalanceVM
                {
                    accTreeId = accTreeId,
                    credit = q.Credit,
                    debit = q.Debit,
                    transactionDate = date,
                    transactionDateStr = tarnsactionDateStr,
                    accTreeName = q.AccountTree.AccName,
                    financeCycleId = financeCycleId??0,
                    note = "رصيد أول المدة",
                    Id = q.Id
                })
                .ToList();
        }

        public string FormatDecimal(decimal? value)
        {
            decimal val = (decimal)value * 100;

            value = Math.Round(val, 2);
            // إذا الجزء العشري صفر، يظهر بدون كسور
            if (val % 1 == 0)
                return ((int)val).ToString();
            else
                return val.ToString("0.##"); // تظهر حتى خانتين عشريتين فقط
        }
    }
}