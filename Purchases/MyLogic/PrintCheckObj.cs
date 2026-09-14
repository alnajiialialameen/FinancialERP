using Purchases.Models;
using Purchases.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Purchases.CurencyOperation;

namespace Purchases.MyLogic
{
    public class PrintCheckObj
    {
        private Entities db = new Entities();
        
        // الشيكات الصادرة
        public int SaveData(int Id, string dueDate, int payNo, string amount, string textAmount, string checkNo, string recipient, string userid)
        {
            // ال 3 سطور دي عشان اجيب اخر رقم شيك متاح في دفتر شيكات الحساب ده
            Transact tr = new Transact(db);
            //int Ids = tr.IsItBankAccount(Id);
            //int CheckNo = tr.GetLastUsedCheckNo(Id);

            Transaction tarnsObj = db.Transactions.Find(Id);
            var AccountSubObj = tarnsObj.TransactionDetails.FirstOrDefault(x => x.Credit > 0).AccountTree.AccountSubs.FirstOrDefault();
            int AccountSubsId = Convert.ToInt32(AccountSubObj.Id);
            var bankObj = db.BankAccounts.FirstOrDefault(x => x.AccountSubId == AccountSubsId);
            int CheckTypeOut = Convert.ToInt32(ConstValEnum.CheckTypeOut); // نوع الشيك انه يكون صادر

            if (!db.PrintChecks.Any(x => x.TransactionId == Id))
            {
                PrintCheck obj = new PrintCheck();

                obj.TransactionId = Id;
                obj.Amount = Convert.ToDecimal(amount);
                obj.CheckNo = Convert.ToInt32(checkNo);
                obj.PayNo = payNo;
                obj.DueDate = Convert.ToDateTime(dueDate);
                obj.Recipient = recipient;
                obj.CheckType = CheckTypeOut;
                obj.BankAccountId = bankObj.Id;
                obj.CreatedBy = userid;
                obj.CreationDate = DateTime.Now;

                db.PrintChecks.Add(obj);
                db.SaveChanges();

                ////هنا مفترض نعمل ترحيل طوالي عشان ما تاني يدخلوا ليهو اي تعديل بعد ما يطلع الشيك
                //tarnsObj.Recipient = recipient;
                //tarnsObj.UpdatedBy = userid;
                //tarnsObj.UpdatingDate = DateTime.Now;
                //db.Entry(tarnsObj).State = EntityState.Modified;
                //db.SaveChanges();
            }
            else
            {
                PrintCheck obj = db.PrintChecks.FirstOrDefault(x => x.TransactionId == Id);

                obj.TransactionId = Id;
                obj.Amount = Convert.ToDecimal(amount);
                obj.CheckNo = Convert.ToInt32(checkNo);
                obj.PayNo = payNo;
                obj.DueDate = Convert.ToDateTime(dueDate);
                obj.Recipient = recipient;
                obj.CheckType = CheckTypeOut;
                obj.BankAccountId = bankObj.Id;
                obj.UpdatedBy = userid;
                obj.UpdatingDate = DateTime.Now;

                //هنا مفترض نعمل ترحيل طوالي عشان ما تاني يدخلوا ليهو اي تعديل بعد ما يطلع الشيك
                //tarnsObj.Recipient = recipient;
                //tarnsObj.UpdatedBy = userid;
                //tarnsObj.UpdatingDate = DateTime.Now;
                //db.Entry(tarnsObj).State = EntityState.Modified;
                db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();
            }

            return 1;
        }
        
        // الشيكات الصادرة
        public int SaveDataNew(List<TransactionVM> modal, string userid)
        {
            db.Database.CommandTimeout = 220;
            using (var scope = db.Database.BeginTransaction())
            {
                // التحضير للحفظ في الشيكات
                SharedClass sharedCls = new SharedClass();
                var accTreeIdList = modal.Select(x => x.accTrreId).ToList();
                List<int> bankAccountIdList = sharedCls.getBankIds(accTreeIdList);

                BaseClass baseClass = new BaseClass();

                string textAmount = baseClass.ChangeNumberToText(modal.Sum(x => x.debit).ToString(), 0);
                var dueDate = modal.FirstOrDefault().transactionDate;
                int? checkNo = modal.FirstOrDefault().CheckNo;
                int CheckTypeOut = Convert.ToInt32(ConstValEnum.CheckTypeOut); // نوع الشيك انه يكون صادر
                int TransactionId = Convert.ToInt32(modal.FirstOrDefault().transactionId);
                decimal? amount = modal.Sum(x => x.debit);

                foreach (var item in bankAccountIdList)
                {
                    if (!db.PrintChecks.Any(x => x.TransactionId == TransactionId && x.BankAccountId == item))
                    {
                        PrintCheck obj = new PrintCheck();

                        obj.TransactionId = TransactionId;
                        obj.Amount = amount;
                        obj.CheckNo = checkNo;
                        obj.PayNo = TransactionId;
                        obj.DueDate = dueDate;
                        obj.Recipient = modal.FirstOrDefault().recipient;
                        obj.CheckType = CheckTypeOut;
                        obj.BankAccountId = item;
                        obj.CreatedBy = userid;
                        obj.CreationDate = DateTime.Now;

                        db.PrintChecks.Add(obj);
                    }
                    else
                    {
                        PrintCheck obj = db.PrintChecks.FirstOrDefault(x => x.TransactionId == TransactionId);

                        obj.TransactionId = TransactionId;
                        obj.Amount = amount;
                        obj.CheckNo = checkNo;
                        obj.PayNo = TransactionId;
                        obj.DueDate = dueDate;
                        obj.Recipient = modal.FirstOrDefault().recipient;
                        obj.CheckType = CheckTypeOut;
                        obj.BankAccountId = item;
                        obj.UpdatedBy = userid;
                        obj.UpdatingDate = DateTime.Now;

                        db.Entry(obj).State = EntityState.Modified;
                    }
                }

                db.SaveChanges();
                scope.Commit();
                return 1;
            }
        }

        // الشيكات الوارده 
        public int SaveDataRevenue(int Id, string dueDate, string payNo, string amount, string textAmount, int? checkNo, string recipient, string userid)
        {
            // ال 3 سطور دي عشان اجيب اخر رقم شيك متاح في دفتر شيكات الحساب ده
            //Transact tr = new Transact(db);
            //int Ids = tr.IsItBankAccount(Id);
            //int CheckNo = tr.GetLastUsedCheckNo(Id);

            Transaction tarnsObj = db.Transactions.Find(Id);
            var AccountSubObj = tarnsObj.TransactionDetails.FirstOrDefault(x => x.Debit > 0).AccountTree.AccountSubs.FirstOrDefault();
            int AccountSubsId = Convert.ToInt32(AccountSubObj.Id);
            var bankObj = db.BankAccounts.FirstOrDefault(x => x.AccountSubId == AccountSubsId);
            int CheckTypeIn = Convert.ToInt32(ConstValEnum.CheckTypeIn); // نوع الشيك انه يكون وارد

            if (!db.PrintChecks.Any(x => x.TransactionId == Id))
            {
                PrintCheck obj = new PrintCheck();

                obj.TransactionId = Id;
                obj.Amount = Convert.ToDecimal(amount);
                obj.CheckNo = checkNo;
                obj.PayNo = Convert.ToInt32(payNo);
                obj.DueDate = Convert.ToDateTime(dueDate);
                obj.Recipient = recipient;
                obj.BankAccountId = bankObj.Id;
                obj.CheckType = CheckTypeIn;
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
                PrintCheck obj = db.PrintChecks.FirstOrDefault(x => x.TransactionId == Id);

                obj.TransactionId = Id;
                obj.Amount = Convert.ToDecimal(amount);
                obj.CheckNo = Convert.ToInt32(checkNo);
                obj.PayNo = Convert.ToInt32(payNo);
                obj.DueDate = Convert.ToDateTime(dueDate);
                obj.Recipient = recipient;
                obj.CheckType = CheckTypeIn;
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

            return 1;
        }

        //  جديدة و غير مستخدمة
        public int SaveDataNew(int Id, string dueDate, string payNo, string amount, string textAmount, string checkNo, string recipient, string userid)
        {
            SharedClass sharedCls = new SharedClass();
            int Ids = sharedCls.IsTransactionHasBankAccount(Id);
            int CheckNo = sharedCls.GetLastUsedCheckNo(Id);

            Transaction tarnsObj = db.Transactions.Find(Id);
            var AccountSubObj = tarnsObj.TransactionDetails.FirstOrDefault(x => x.Credit > 0).AccountTree.AccountSubs.FirstOrDefault();
            int AccountSubsId = Convert.ToInt32(AccountSubObj.Id);
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
                //obj.BankAccountId = bankObj.Id;
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
                PrintCheck obj = db.PrintChecks.FirstOrDefault(x => x.TransactionId == Id);

                obj.TransactionId = Id;
                obj.Amount = Convert.ToDecimal(amount);
                obj.CheckNo = Convert.ToInt32(checkNo);
                obj.PayNo = Convert.ToInt32(payNo);
                obj.DueDate = Convert.ToDateTime(dueDate);
                obj.Recipient = recipient;
                //obj.BankAccountId = bankObj.Id;
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

            return 1;
        }
    }
}