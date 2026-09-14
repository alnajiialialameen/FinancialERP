using Purchases.CurencyOperation;
using Purchases.Models;
using Purchases.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;

namespace Purchases.MyLogic
{
    public class Transact
    {
        private Entities db;
        private SharedClass sharedCls = new SharedClass();

        // constructor
        public Transact(Entities _db)
        {
            db = _db;
        }

        // جلب جميع بيانات الحركة
        public List<TransactionVM> GetAllData()
        {
            var data = db.Transactions
               .Select(p => new TransactionVM()
               {
                   transactionId = p.Id,
                   currency = p.CurrencyType.Name,
                   documentType = p.DocumentType.Name,
                   total = p.Amount,
                   exchangeRate = p.ExchangeRate,
                   transactionDateStr = p.TransactionDate.Value.Year + "/" + p.TransactionDate.Value.Month + "/" + p.TransactionDate.Value.Day,
                   note = p.Note
               }).ToList();

            return data;
        }

        // جلب بيانات الحركة الخاصة بي سندات القيد
        public List<TransactionVM> GetData(int DocTypeId)
        {
            if (DocTypeId > 0)
            {
                List<TransactionVM> data = db.Transactions.Where(x => x.IsPosted != true & x.DocumentTypeId == DocTypeId)
                .Select(p => new TransactionVM()
                {
                    transactionId = p.Id,
                    currency = p.CurrencyType.Name,
                    documentType = p.DocumentType.Name,
                    total = p.Amount,
                    exchangeRate = p.ExchangeRate == null ? 0 : p.ExchangeRate,
                    transactionDateStr = p.TransactionDate.Value.Year.ToString() + "/" + p.TransactionDate.Value.Month.ToString() + "/" + p.TransactionDate.Value.Day.ToString(),
                    note = p.Note,
                    recipient = p.Recipient==null? "غير مدخل" : p.Recipient,
                }).ToList();

                return data;
            }
            else
            {
                List<TransactionVM> data = db.Transactions.Where(x => x.IsPosted != true)
                .Select(p => new TransactionVM()
                {
                    transactionId = p.Id,
                    currency = p.CurrencyType.Name,
                    documentType = p.DocumentType.Name,
                    total = p.Amount,
                    exchangeRate = p.ExchangeRate == null ? 0 : p.ExchangeRate,
                    recipient = p.Recipient == null ? "غير مدخل" : p.Recipient,
                    transactionDateStr = p.TransactionDate.Value.Year.ToString() + "/" + p.TransactionDate.Value.Month.ToString() + "/" + p.TransactionDate.Value.Day.ToString(),
                    note = p.Note,
                }).ToList();

                return data;
            }
        }

        // جلب القيود او الحركات المرحلة
        public List<TransactionVM> GetDataPosted(string userid)
        {
            int financialCycleId = sharedCls.GetUserCurrentFinancialCycleId(userid);
            var data = db.Transactions.Where(x => x.IsPosted == true && x.FinancialCycleId == financialCycleId)
                .Select(p => new TransactionVM()
                {
                    transactionId = p.Id,
                    currency = p.CurrencyType.Name,
                    documentType = p.DocumentType.Name,
                    total = p.Amount,
                    exchangeRate = p.ExchangeRate,
                    transactionDateStr = p.TransactionDate.Value.Year.ToString() + "/" + p.TransactionDate.Value.Month.ToString() + "/" + p.TransactionDate.Value.Day.ToString(),
                    note = p.Note,
                    recipient = p.Recipient == null ? "غير مدخل" : p.Recipient,
                }).ToList();

            return data;
        }

        // حفظ بيانات الحركة في سندات القيد + تسويات المرتبات 
        public int PostData(List<TransactionVM> data, string userid)
        {
            try
            {
                if (data.Sum(x => x.credit) != data.Sum(x => x.debit))
                {
                    return 0;
                }

                Transaction tra = new Transaction();

                tra.DocumentTypeId = data.FirstOrDefault().documentTypeId;
                tra.CurrencyId = data.FirstOrDefault().currencyId;
                tra.Amount = data.Sum(x => x.debit);
                tra.TransactionDate = data.FirstOrDefault().transactionDate;
                tra.ExchangeRate = data.FirstOrDefault().exchangeRate;
                tra.Note = data.FirstOrDefault().note;
                tra.Recipient = data[0].recipient;
                tra.IsPosted = false;
                tra.ExchangeRate = sharedCls.GetCurrentMonthExchangeRate((DateTime)tra.TransactionDate, tra.CurrencyId);
                tra.FinancialCycleId = sharedCls.GetUserCurrentFinancialCycleId(userid);
                tra.CreatedDate = DateTime.Today;
                tra.CreatedBy = userid;

                db.Transactions.Add(tra);
                db.SaveChanges();

                List<TransactionDetail> jouDetList = new List<TransactionDetail>();
                var balances = db.Balances.Where(x => x.FinanceCycleId == tra.FinancialCycleId);

                foreach (var item in data)
                {
                    TransactionDetail transDet = new TransactionDetail();

                    transDet.AccTreeId = Convert.ToInt32(item.accTrreId);
                    transDet.TransactionId = tra.Id;
                    transDet.Debit = item.debit;
                    transDet.Credit = item.credit;
                    transDet.Note = data.FirstOrDefault().note;
                    transDet.CreatedBy = userid;
                    transDet.CreationDate = DateTime.Today;

                    if (balances.Any(x => x.AccountTreeId == item.accTrreId))
                    {
                        decimal Amount = Convert.ToDecimal(item.debit + item.credit);
                        int BalanceId = balances.FirstOrDefault(x => x.AccountTreeId == item.accTrreId).Id;

                        myExtention.UpdateActualExchange(BalanceId, 0, Amount, userid);

                        transDet.BalanceId = BalanceId;
                    }

                    jouDetList.Add(transDet);
                }

                db.TransactionDetails.AddRange(jouDetList);
                db.SaveChanges();

                /*-------------- الحفظ في الشيكات ---------------*/
                int bankId = sharedCls.IsTransactionHasBankAccount(tra.Id);
                string checkNo = data.FirstOrDefault().CheckNo.ToString();
                int CheckTypeOut = Convert.ToInt32(ConstValEnum.CheckTypeOut); // نوع الشيك انه يكون صادر
                int CurrentCheckNo = Convert.ToInt32(checkNo);

                if (checkNo != null && !db.PrintChecks.Any(x => x.CheckNo == CurrentCheckNo & x.CheckType == CheckTypeOut))
                {
                    PrintCheckObj obj = new PrintCheckObj();
                    BaseClass b = new BaseClass();

                    string textAmount = b.ChangeNumberToText(data.Sum(x => x.debit).ToString(), 0);
                    string recipient = data.FirstOrDefault().recipient;

                    int res = obj.SaveData(tra.Id, tra.TransactionDate.Value.ToShortDateString(), tra.Id, data.Sum(x => x.credit).ToString(), textAmount, checkNo, recipient, userid);

                    if (res > 0)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                }

                return 1;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        // ترحيل الحركة
        public int PostingTransaction(int Id, string userid)
        {
            try
            {
                Transaction transaction = db.Transactions.Find(Id);

                transaction.IsPosted = true;
                transaction.UpdatedBy = userid;
                transaction.UpdatingDate = DateTime.Today;

                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                return 1; //success
            }
            catch (Exception e)
            {
                return -1; // error
            }
        }

        // اضافة ضريبة 17 % للحركة
        public int Add17Tax(int Id, bool HasAddedTax, string userid)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    Transaction transaction = db.Transactions.Find(Id);
                    var debitData = db.TransactionDetails.Where(x => x.TransactionId == Id & x.BalanceId != null);
                    
                    if (debitData.Any())
                    { 
                        transaction.HasAddedTax = HasAddedTax;
                        transaction.UpdatedBy = userid;
                        transaction.UpdatingDate = DateTime.Today;

                        db.Entry(transaction).State = EntityState.Modified;
                    }
                    else
                    {
                        //  لا توجد بنود لتحميل الضريبة عليها
                        return -100;
                    }

                    // الكود ده يشتغل لمن 
                    foreach (var item in debitData)
                    {
                        var BalanceObj = db.Balances.Find(item.BalanceId);

                        //هنا التاثير علي الموازنة زيادة او نقصان
                        if (HasAddedTax) // مفترض نزيد نسبة 17% من اصل المبلغ لي بند الموازنة
                        {
                            double? debitAmountDouble = Math.Round(((double)item.Debit * 0.17), 2);
                            decimal? debitAmount = Convert.ToDecimal(debitAmountDouble);


                            //BalanceObj.ActualExchange = BalanceObj.ActualExchange + (item.Debit * (decimal)0.17);
                            //BalanceObj.RelativeDeviation = BalanceObj.RelativeDeviation - (item.Debit * (decimal)0.17);
                            //item.Debit = item.Debit + (item.Debit * (decimal)0.17);

                            BalanceObj.ActualExchange += debitAmount;
                            BalanceObj.RelativeDeviation -= debitAmount;
                            item.Debit += debitAmount;
                        }
                        else // مفترض ننقص نسبة 17% من اصل المبلغ من بند الموازنة
                        {
                            //    BalanceObj.ActualExchange = BalanceObj.ActualExchange - ((item.Debit / (decimal)1.17)* (decimal)0.17);
                            //    BalanceObj.RelativeDeviation = BalanceObj.RelativeDeviation + (item.Debit / (decimal)1.17) * (decimal)0.17;
                            //    item.Debit = (item.Debit / (decimal)1.17);

                            double? debitAmountDouble = Math.Round((((double)item.Debit / 1.17) * 0.17), 2);
                            decimal? debitAmount = Convert.ToDecimal(debitAmountDouble);

                            BalanceObj.ActualExchange -= debitAmount;
                            BalanceObj.RelativeDeviation += debitAmount;
                            item.Debit = Convert.ToDecimal(Math.Round((double)(item.Debit / (decimal)1.17), 2));
                        }
                        
                        db.Entry(item).State = EntityState.Modified;
                        db.Entry(BalanceObj).State = EntityState.Modified;
                        db.SaveChanges();
                    }


                    decimal? sumOfDebit = db.TransactionDetails.Where(x => x.TransactionId == Id & x.Debit > 0).Sum(s=>s.Debit);

                    // الكود ده يشتغل لمن 
                    List<int> NotAppliabaleItems = new List<int>();

                    int TaxId = Convert.ToInt32(ConstValEnum.TaxId);
                    int StampId = Convert.ToInt32(ConstValEnum.StampId);
                    NotAppliabaleItems.Add(TaxId);
                    NotAppliabaleItems.Add(StampId);

                    var NotAppliabaleData = db.TransactionDetails.Where(x => x.TransactionId == Id & !NotAppliabaleItems.Contains(x.AccTreeId) & x.Credit > 0);
                    var AppliabaleData = db.TransactionDetails.Where(x => x.TransactionId == Id & NotAppliabaleItems.Contains(x.AccTreeId) & x.Credit > 0);
                    foreach (var item in NotAppliabaleData)
                    {
                        decimal? newCredit = Convert.ToDecimal(Math.Round((double)item.Debit * 0.17));
                        //هنا التاثير علي الموازنة زيادة او نقصان
                        if (HasAddedTax) // مفترض نزيد نسبة 17% من اصل المبلغ لي بند الموازنة
                        {   
                            //item.Credit = item.Credit + (item.Debit * (decimal)0.17);
                            //decimal? taxs = db.TransactionDetails.Any(x => x.TransactionId == Id & NotAppliabaleItems.Contains(x.AccTreeId) & x.Credit > 0)? 
                            //    db.TransactionDetails.Where(x => x.TransactionId == Id & NotAppliabaleItems.Contains(x.AccTreeId) & x.Credit > 0).Sum(s => s.Credit):0;

                            item.Credit += newCredit;
                            decimal? taxs = AppliabaleData.Any() ? AppliabaleData.Sum(x => x.Credit) : 0;
                            item.Credit = sumOfDebit - taxs;
                        }
                        else // مفترض ننقص نسبة 17% من اصل المبلغ من بند الموازنة
                        {
                            //item.Credit = item.Credit - (item.Debit * (decimal)0.17);
                            //item.Credit = item.Credit - (newSumOfDebit * (decimal)0.17);

                            var newSumOfDebit = Convert.ToDecimal(Math.Round((double)sumOfDebit * 0.17, 2));
                            item.Credit -= newSumOfDebit;
                        }

                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    dbContextTransaction.Commit();
                    return 1;
                }
                catch (Exception e)
                {
                    return -1;
                }
            }
        }

        // اضافة ضريبة 17 % للحركة
        public int Add17Tax(int Id, bool HasAddedTax, decimal? addedTaxPercent, string userid)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    decimal? AddedTaxPercentPlus = 0;
                    decimal? AddedTaxPercent = 0;

                    Transaction transaction = db.Transactions.Find(Id);

                    if (!HasAddedTax)
                    { 
                        AddedTaxPercent = transaction.AddedTaxPercent;
                        AddedTaxPercentPlus =transaction.AddedTaxPercent + 1; // for example 0.17 ==> 1.17 
                    }
                    else
                    {
                        AddedTaxPercent = addedTaxPercent;
                        AddedTaxPercentPlus = addedTaxPercent + 1; // for example 0.17 ==> 1.17 
                    }

                        var debitData = db.TransactionDetails.Where(x => x.TransactionId == Id & x.BalanceId != null);

                    if (debitData.Any())
                    {
                        transaction.HasAddedTax = HasAddedTax;
                        transaction.AddedTaxPercent = addedTaxPercent;
                        transaction.UpdatedBy = userid;
                        transaction.UpdatingDate = DateTime.Today;

                        db.Entry(transaction).State = EntityState.Modified;
                    }
                    else
                    {
                        //  لا توجد بنود لتحميل الضريبة عليها
                        return -100;
                    }

                    // الكود ده يشتغل لمن 
                    foreach (var item in debitData)
                    {
                        var BalanceObj = db.Balances.Find(item.BalanceId);

                        //هنا التاثير علي الموازنة زيادة او نقصان
                        if (HasAddedTax) // مفترض نزيد نسبة 17% من اصل المبلغ لي بند الموازنة
                        {
                            double? debitAmountDouble = Math.Round(((double)item.Debit * (double)AddedTaxPercent), 2);
                            decimal? debitAmount = Convert.ToDecimal(debitAmountDouble);


                            //BalanceObj.ActualExchange = BalanceObj.ActualExchange + (item.Debit * (decimal)0.17);
                            //BalanceObj.RelativeDeviation = BalanceObj.RelativeDeviation - (item.Debit * (decimal)0.17);
                            //item.Debit = item.Debit + (item.Debit * (decimal)0.17);

                            BalanceObj.ActualExchange += debitAmount;
                            BalanceObj.RelativeDeviation -= debitAmount;
                            item.Debit += debitAmount;
                        }
                        else // مفترض ننقص نسبة 17% من اصل المبلغ من بند الموازنة
                        {
                            //    BalanceObj.ActualExchange = BalanceObj.ActualExchange - ((item.Debit / (decimal)1.17)* (decimal)0.17);
                            //    BalanceObj.RelativeDeviation = BalanceObj.RelativeDeviation + (item.Debit / (decimal)1.17) * (decimal)0.17;
                            //    item.Debit = (item.Debit / (decimal)1.17);

                            double? debitAmountDouble = Math.Round((((double)item.Debit / (double)AddedTaxPercentPlus) * (double)AddedTaxPercent), 2);
                            decimal? debitAmount = Convert.ToDecimal(debitAmountDouble);

                            BalanceObj.ActualExchange -= debitAmount;
                            BalanceObj.RelativeDeviation += debitAmount;
                            item.Debit = Convert.ToDecimal(Math.Round((double)(item.Debit / AddedTaxPercentPlus), 2));
                        }

                        db.Entry(item).State = EntityState.Modified;
                        db.Entry(BalanceObj).State = EntityState.Modified;
                        db.SaveChanges();
                    }


                    decimal? sumOfDebit = db.TransactionDetails.Where(x => x.TransactionId == Id & x.Debit > 0).Sum(s => s.Debit);

                    // الكود ده يشتغل لمن 
                    List<int> NotAppliabaleItems = new List<int>();

                    int TaxId = Convert.ToInt32(ConstValEnum.TaxId);
                    int StampId = Convert.ToInt32(ConstValEnum.StampId);
                    NotAppliabaleItems.Add(TaxId);
                    NotAppliabaleItems.Add(StampId);

                    var NotAppliabaleData = db.TransactionDetails.Where(x => x.TransactionId == Id & !NotAppliabaleItems.Contains(x.AccTreeId) & x.Credit > 0);
                    var AppliabaleData = db.TransactionDetails.Where(x => x.TransactionId == Id & NotAppliabaleItems.Contains(x.AccTreeId) & x.Credit > 0);
                    foreach (var item in NotAppliabaleData)
                    {
                        decimal? newCredit = Convert.ToDecimal(Math.Round((double)item.Debit * (double)AddedTaxPercent));
                        //هنا التاثير علي الموازنة زيادة او نقصان
                        if (HasAddedTax) // مفترض نزيد نسبة 17% من اصل المبلغ لي بند الموازنة
                        {
                            //item.Credit = item.Credit + (item.Debit * (decimal)0.17);
                            //decimal? taxs = db.TransactionDetails.Any(x => x.TransactionId == Id & NotAppliabaleItems.Contains(x.AccTreeId) & x.Credit > 0)? 
                            //    db.TransactionDetails.Where(x => x.TransactionId == Id & NotAppliabaleItems.Contains(x.AccTreeId) & x.Credit > 0).Sum(s => s.Credit):0;

                            item.Credit += newCredit;
                            decimal? taxs = AppliabaleData.Any() ? AppliabaleData.Sum(x => x.Credit) : 0;
                            item.Credit = sumOfDebit - taxs;
                        }
                        else // مفترض ننقص نسبة 17% من اصل المبلغ من بند الموازنة
                        {
                            //item.Credit = item.Credit - (item.Debit * (decimal)0.17);
                            //item.Credit = item.Credit - (newSumOfDebit * (decimal)0.17);

                            var newSumOfDebit = Convert.ToDecimal(Math.Round((double)sumOfDebit * (double)AddedTaxPercent, 2));
                            item.Credit -= newSumOfDebit;
                        }

                        db.Entry(item).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    dbContextTransaction.Commit();
                    return 1;
                }
                catch (Exception e)
                {
                    return -1;
                }
            }
        }

        // اضافة ضريبة 1% للحركة
        public int AddTax(int Id, bool HasTax, string userid)
        {
            using (var dbContextTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    // نفس الكود الخاص بك لتحديث المعاملات والبيانات
                    Transaction transaction = db.Transactions.Find(Id);
                    var TransactionDetailsList = db.TransactionDetails.Where(x => x.TransactionId == Id).ToList();

                    if(TransactionDetailsList.Any(q=> q.AccTreeId == 228 ) && HasTax == true)
                        return -200;

                    decimal ? AddedTaxPercent = 0;
                    var debitData = TransactionDetailsList.Where(x => x.TransactionId == Id & x.BalanceId != null);

                    if (debitData.Any())
                    {
                        transaction.HasTax = HasTax;
                        transaction.UpdatedBy = userid;
                        transaction.UpdatingDate = DateTime.Today;

                        db.Entry(transaction).State = EntityState.Modified;
                    }
                    else
                    {
                        // لا توجد بنود لتحميل الضريبة عليها او خصم الضريبة منها
                        return -100;
                    }

                    int taxId = Convert.ToInt32(ConstValEnum.TaxId);

                    //اجمع كل القيم ال مدينه عشان اشيل منها الضريبة كلها
                    decimal? sumOfDebits = debitData.Where(x => x.Debit > 0).Sum(x => x.Debit);

                    // نسبة الضريبة من اصل المبلغ
                    // decimal? sumOfDebits = transaction.Amount;

                    if (HasTax) // خصم الضريبة من اصل المبلغ
                    {
                        decimal? taxValue = 0;
                        if (transaction.HasAddedTax == true)
                        {
                            AddedTaxPercent = transaction.AddedTaxPercent;
                            decimal? AddedTaxPercentPlus = AddedTaxPercent + 1; // for example 0.17 ==> 1.17, 0.0425 ==> 1.0425....etc

                            decimal? Amount = sumOfDebits / AddedTaxPercentPlus;
                            taxValue = Amount * (decimal)0.01;
                        }
                        else
                        {
                            taxValue = sumOfDebits * (decimal)0.01;
                            //taxValue = Convert.ToDecimal(Math.Round((double)sumOfDebits * 0.01, 2));
                        }

                        taxValue = Convert.ToDecimal(Math.Round((double)taxValue, 2));
                        TransactionDetail transDetCredit = new TransactionDetail();

                        transDetCredit.TransactionId = Id;
                        transDetCredit.AccTreeId = taxId;
                        transDetCredit.Debit = 0;
                        transDetCredit.Credit = taxValue;
                        transDetCredit.Note = transaction.Note;
                        transDetCredit.CreatedBy = userid;
                        transDetCredit.CreationDate = DateTime.Today;

                        db.TransactionDetails.Add(transDetCredit);

                        // خصم الضريبة من حساب البنك
                        foreach (var creditItem in db.TransactionDetails.Where(x => x.TransactionId == Id && x.Credit > 0).ToList())
                        {
                            if (sharedCls.IsItBankAccount(creditItem.AccTreeId))
                            {
                                creditItem.Credit -= taxValue;

                                db.Entry(creditItem).State = EntityState.Modified;
                            }
                        }
                    }
                    else  // الغاء خصم الضريبة 
                    {
                        var taxObj = db.TransactionDetails.FirstOrDefault(x => x.TransactionId == Id && x.AccTreeId == taxId);
                        var creditDataList = db.TransactionDetails.Where(x => x.TransactionId == Id && x.Credit > 0 && x.AccTreeId != taxId).ToList();

                        foreach (var creditItem in creditDataList)
                        {
                            if (sharedCls.IsItBankAccount(creditItem.AccTreeId))
                            {
                                creditItem.Credit += taxObj.Credit;

                                db.Entry(creditItem).State = EntityState.Modified;
                            }
                        }
                        db.TransactionDetails.Remove(taxObj);
                        db.Entry(transaction).State = EntityState.Modified;

                        db.SaveChanges();
                    }
                    var data = db.TransactionDetails.Where(q => q.TransactionId == Id);
                    decimal? sumOfDebit = data.Sum(x => x.Debit);
                    decimal? sumOfCredit = data.Sum(x => x.Credit);

                    if (sumOfCredit == sumOfDebit)
                    {
                        db.SaveChanges();
                        dbContextTransaction.Commit();

                        return 1;
                    }

                    return 0;
                }
                catch (Exception e)
                {
                    dbContextTransaction.Rollback();
                    return -1;
                }
            }
        }

        // اضافة الدمغة للحركة
        public int AddStamp(int Id, decimal StampValue, string userid)
        {
            try
            {
                Transaction transaction = db.Transactions.Find(Id);
                
                transaction.UpdatedBy = userid;
                transaction.UpdatingDate = DateTime.Today;

                int stampId = Convert.ToInt32(ConstValEnum.StampId);
                var creditAccount = db.TransactionDetails.Where(x => x.TransactionId == Id & x.Credit > 0).FirstOrDefault();
                
                //هنا معناها انه الدمغعة غير مضافة من قبل لانه لو بقت مضافة من قبل مفترض نحذفها في الجزء الاسفل
                if (!db.TransactionDetails.Any(x=>x.TransactionId == Id & x.AccTreeId == stampId))
                {
                    // هنا نضيف صف للدمغة في الجانب الدائن
                    TransactionDetail transDetCredit = new TransactionDetail();

                    transDetCredit.TransactionId = Id;
                    transDetCredit.AccTreeId = stampId;
                    transDetCredit.Debit = 0;
                    transDetCredit.Credit = StampValue;
                    transDetCredit.Note = transaction.Note;
                    transDetCredit.CreatedBy = userid;
                    transDetCredit.CreationDate = DateTime.Today;
                    db.TransactionDetails.Add(transDetCredit);

                    //هنا طرحنا قيمة الدمغة من الجانب الدائن 
                    creditAccount.Credit = creditAccount.Credit - StampValue;
                }
                else
                {
                    //هنا بنعدل في قيمة الدمغة حتي لو كانت القيمة الجديدة صفر
                    var stampObj = db.TransactionDetails.Where(x => x.TransactionId == Id & x.AccTreeId == stampId).FirstOrDefault();

                    if (stampObj.Credit > 0)
                    {
                        creditAccount.Credit = creditAccount.Credit + stampObj.Credit - StampValue;
                    }
                    else
                    {
                        //هنا طرحنا قيمة الدمغة من الجانب الدائن 
                        creditAccount.Credit = creditAccount.Credit - StampValue;
                    }
                    stampObj.Credit = StampValue;
                    stampObj.Debit = 0;
                }
                
                db.Entry(creditAccount).State = EntityState.Modified;
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                return 1;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        // اضافة الدمغة للحركة
        public int AddIncomeTax(int Id, decimal IncomeTaxValue, string userid)
        {
            try
            {
                Transaction transaction = db.Transactions.Find(Id);

                transaction.UpdatedBy = userid;
                transaction.UpdatingDate = DateTime.Today;

                int IncomeTaxId = Convert.ToInt32(ConstValEnum.IncomeTaxId);
                var creditAccount = db.TransactionDetails.Where(x => x.TransactionId == Id & x.Credit > 0).FirstOrDefault();

                //هنا معناها انه الدمغعة غير مضافة من قبل لانه لو بقت مضافة من قبل مفترض نحذفها في الجزء الاسفل
                if (!db.TransactionDetails.Any(x => x.TransactionId == Id & x.AccTreeId == IncomeTaxId))
                {
                    // هنا نضيف صف للدمغة في الجانب الدائن
                    TransactionDetail transDetCredit = new TransactionDetail();

                    transDetCredit.TransactionId = Id;
                    transDetCredit.AccTreeId = IncomeTaxId;
                    transDetCredit.Debit = 0;
                    transDetCredit.Credit = IncomeTaxValue;
                    transDetCredit.Note = transaction.Note;
                    transDetCredit.CreatedBy = userid;
                    transDetCredit.CreationDate = DateTime.Today;
                    db.TransactionDetails.Add(transDetCredit);

                    //هنا طرحنا قيمة الدمغة من الجانب الدائن 
                    creditAccount.Credit = creditAccount.Credit - IncomeTaxValue;
                }
                else
                {
                    //هنا بنعدل في قيمة الدمغة حتي لو كانت القيمة الجديدة صفر
                    var stampObj = db.TransactionDetails.Where(x => x.TransactionId == Id & x.AccTreeId == IncomeTaxId).FirstOrDefault();

                    if (stampObj.Credit > 0)
                    {
                        creditAccount.Credit = creditAccount.Credit + stampObj.Credit - IncomeTaxValue;
                    }
                    else
                    {
                        //هنا طرحنا قيمة الدمغة من الجانب الدائن 
                        creditAccount.Credit = creditAccount.Credit - IncomeTaxValue;
                    }
                    stampObj.Credit = IncomeTaxValue;
                    stampObj.Debit = 0;
                }

                db.Entry(creditAccount).State = EntityState.Modified;
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();

                return 1;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        // جلب تفاصيل حركة محددة
        public List<TransactionVM> GetDetailsData(int Id)
        {
            List<TransactionVM> data = new List<TransactionVM>();
            foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == Id).ToList())
            {
                TransactionVM obj = new TransactionVM();

                obj.id = item.Id;
                obj.accTrreId = item.AccTreeId;
                obj.balanceId = item.BalanceId ?? item.BalanceId;
                obj.balanceAccName = item.BalanceId != null ? item.Balance.AccountTree.AccName : "";
                obj.accName = item.AccountTree.AccName;
                obj.credit = item.Credit;
                obj.debit = item.Debit;
                obj.transactionDateStr = item.Transaction.TransactionDate != null ? item.Transaction.TransactionDate.Value.ToShortDateString() : "غير مدخل";
                obj.note = item.Transaction.Note ?? "غير مدخل";

                data.Add(obj);
            }

            return data.OrderBy(x=> x.debit).ToList();
        }
        
        // حذف صف واحد في تفاصيل الحركة
        public int DeleteDetailsData(int Id, string userid)
        {
            try
            {
                var transDetObj = db.TransactionDetails.Find(Id);
                var balanceId = Convert.ToInt32(transDetObj.BalanceId);
                var Credit = transDetObj.Credit;
                var Debit = transDetObj.Debit;

                int type = transDetObj.Credit > 0 ? 1 : 0;

                if (balanceId > 0)
                {
                    var oldAmount = Convert.ToDecimal(Credit + Debit);
                    // بعد حذف العنصر تعدل في الموانه
                    myExtention.UpdateActualExchange(balanceId, oldAmount, 0, type, userid);
                }

                db.TransactionDetails.Remove(transDetObj);
                db.SaveChanges();

                return 1;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        // تعديل بيانات الحركة مهمه جدا الدالة دي
        public int UpdateData(List<TransactionVM> data, string userid, int financialCycleId)
        {
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    if (data.Sum(x => x.credit) != data.Sum(x => x.debit))
                        return 0;

                    var firstItem = data.First();

                    // ده عشان اجيب السنة المالية لي تاريخ العملية
                    string currentTransactionYear = Convert.ToDateTime(firstItem.transactionDate).Year.ToString();

                    // لو العام المالي لي تاريخ العملية  مغلق ما ينفذ العملية
                    //if (db.FinancialCycles.Any(x => x.Year == currentTransactionYear && x.CurrentYear == false))
                    //{
                    //    return -1000;
                    //}

                    var transactionId = firstItem.transactionId;
                    var currencyId = firstItem.currencyId;
                    var recipient = firstItem.recipient;
                    var recipientId = firstItem.recipientId;
                    var transactionDate = Convert.ToDateTime(firstItem.transactionDate);
                    var exchangeRate = sharedCls.GetCurrentMonthExchangeRate(transactionDate, currencyId);

                    //var exchangeRate = db.CurrencyDetails
                    //    .Where(x => x.CurrencyTypeId == currencyId && x.Month == transactionDate.Month && x.Year == transactionDate.Year)
                    //    .Select(x => x.ExchangeRate)
                    //    .FirstOrDefault();

                    var transObj = db.Transactions.Find(transactionId);

                    if (transObj == null) return -1;

                    transObj.DocumentTypeId = firstItem.documentTypeId;
                    transObj.CurrencyId = currencyId;
                    transObj.RecipientId = recipientId;
                    transObj.FinancialCycleId = financialCycleId;
                    transObj.Recipient = recipient;
                    transObj.Amount = data.Sum(x => x.credit);
                    transObj.TransactionDate = transactionDate;
                    transObj.ExchangeRate = exchangeRate;
                    transObj.Note = firstItem.note?.Trim();
                    transObj.IsPosted = false;
                    transObj.UpdatingDate = DateTime.Now;
                    transObj.UpdatedBy = userid;

                    db.Entry(transObj).State = EntityState.Modified;

                    var balancesData = db.Balances
                        .Where(x => x.FinanceCycleId == financialCycleId)
                        .ToDictionary(x => x.AccountTreeId, x => x.Id);

                    var existingDetails = db.TransactionDetails
                        .Where(x => x.TransactionId == transactionId)
                        .ToList();

                    // تحديث أو حذف البنود القديمة
                    foreach (var item in existingDetails)
                    {
                        var match = data.FirstOrDefault(x => x.accTrreId == item.AccTreeId);
                        item.BalanceId = balancesData.ContainsKey(item.AccTreeId) ? balancesData[item.AccTreeId] : (int?)null;

                        if (match != null)
                        {
                            // تحديث
                            decimal oldAmount = Convert.ToDecimal(item.Debit + item.Credit);
                            decimal newAmount = Convert.ToDecimal(match.debit + match.credit);

                            item.Credit = match.credit;
                            item.Debit = match.debit;
                            item.Note = match.note;
                            item.UpdatedBy = userid;
                            item.UpdatingDate = DateTime.Now;
                            //var newAmount = Convert.ToDecimal(match.credit + match.debit);
                            //myExtention.UpdateActualExchange(item.BalanceId ?? 0, 0, newAmount, userid);

                            // بعد تعديل العنصر تعدل في الموانه
                            myExtention.UpdateActualExchange(item.BalanceId ?? 0, oldAmount, newAmount, userid);

                            db.Entry(item).State = EntityState.Modified;
                        }
                        else
                        {
                            // حذف
                            var oldAmount = Convert.ToDecimal(item.Debit + item.Credit);
                            myExtention.UpdateActualExchange(item.BalanceId ?? 0, oldAmount, 0, userid);
                            db.TransactionDetails.Remove(item);
                        }
                    }

                    // إضافة البنود الجديدة
                    var existingAccIds = existingDetails.Select(x => x.AccTreeId).ToList();
                    var newItems = data.Where(x => !existingAccIds.Contains(x.accTrreId??0));
                    foreach (var item in newItems)
                    {
                        var newDetail = new TransactionDetail
                        {
                            AccTreeId = item.accTrreId??0,
                            BalanceId = item.balanceId,
                            Credit = item.credit,
                            Debit = item.debit,
                            TransactionId = transactionId??0,
                            Note = item.note,
                            CreatedBy = userid,
                            CreationDate = DateTime.Now
                        };

                        var amount = Convert.ToDecimal(item.credit + item.debit);
                        myExtention.UpdateActualExchange(item.balanceId ?? 0, 0, amount, userid);

                        db.TransactionDetails.Add(newDetail);
                    }

                    db.SaveChanges();
                    
                    transaction.Commit();

                    // اذا العملية فيها 17% قيمة مضافة عدل المبالغ حسب المبالغ الجديدة
                    if(transObj.HasAddedTax == true)
                    {
                        this.Add17Tax(transObj.Id, true, userid);
                    }

                    if (!data.Any(x=> x.accTrreId == 228) && transObj.HasTax == true)
                    {
                        this.AddTax(transObj.Id, true, userid);
                    }


                    // اضافة مستلم الي جدول المستلمين ان وجد
                    //var transactionRecipients = db.TransactionRecipients.Where(q=> q.TransactionId == transObj.Id).ToList();

                    //string RecipientName = data.FirstOrDefault().recipient;

                    //if (data.Any(x => x.recipientId > 0) && !transactionRecipients.Any(q=> q.RecipientName == RecipientName))
                    //{
                    //    var recipientObj = db.Recipients.Find(data.FirstOrDefault().recipientId);

                    //    var bankAccountId = sharedCls.IsTransactionHasBankAccount(transObj.Id);
                    //    var bankAmount = sharedCls.GetAmountForBankAccountInTransaction(transObj.Id);

                    //    var transcationReceipts = new TransactionRecipient();

                    //    transcationReceipts.TransactionId = transObj.Id;
                    //    transcationReceipts.RecipientName = recipientObj.RecipientName;
                    //    transcationReceipts.BankName = recipientObj.BankName;
                    //    transcationReceipts.BranchName = recipientObj.BranchName;
                    //    transcationReceipts.AccountNumber = recipientObj.AccountNumber;
                    //    transcationReceipts.BankAccountId = bankAccountId;
                    //    transcationReceipts.Amount = bankAmount;

                    //    db.TransactionRecipients.Add(transcationReceipts);
                    //    db.SaveChanges();
                    //}


                    return 1;
                }
                catch
                {
                    transaction.Rollback();
                    return -1;
                }
            }
        }

    }
}