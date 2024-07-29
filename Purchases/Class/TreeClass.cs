using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Purchases.Models;
using Purchases.Class;
using Purchases.Models.ViewModal;
using System.Data.Entity;

namespace Purchases.Class
{
    public class TreeClass
    {
        public string ParentNameList = "";
        public string AccCodee;
        public int AccCodeInt;
        public Decimal AccId;
        Entities db = new Entities();
        int rootId = 0;
        //TeeView tee = new TeeView();

        public object myOb { get; set; }


        /*
            1. get parent code, latter we will concat it with the item generated code.
            2. get item level, the code digits depend on level + 1.
            3. get conut of items in the current level, latter we will set the 
            4. generate the code,
        */
        public string GetLastAccCode(int ParentAccId)
        {
            string Code;

            AccountTree accountTree = db.AccountTrees.Find(ParentAccId);
            
            int newItemLevel = Convert.ToInt32(accountTree.TheLevel + 1);
            string parentCode = accountTree.AccCode;
            int countOfChildernInThisParent = db.AccountTrees.Where(x=>x.AccParent == ParentAccId).Count();
            
            var newItemCodeDegits = string.Concat(Enumerable.Repeat("0", newItemLevel - countOfChildernInThisParent.ToString().Length + 1));

            int newItemSuffexCode = countOfChildernInThisParent + 1;

            Code = parentCode + newItemCodeDegits + newItemSuffexCode.ToString();
            
            return Code;
        }

        public string GetLastAccCodeNaji(int ParentAccId)
        {
            string Code;

            AccountTree AccountTree = db.AccountTrees.Where(x => x.Id == ParentAccId).FirstOrDefault();
            if (AccountTree == null)
            {
                Code = "1";
            }
            else
            {
                Code = AccountTree.AccCode.ToString() + "01";
            }
            return Code;

        }

        public string GetParentName(int? ParentId)
        {
            string ParentName = "";
            if (ParentId != 0 || ParentId == null)
            {
                AccountTree tree = db.AccountTrees.Where(x => x.Id == ParentId).FirstOrDefault();
                if (tree != null)
                {
                    ParentName = tree.AccName;
                    return ParentName;
                }
                else
                {
                    return ParentName;
                }
            }
            return ParentName;
        }
        
        public string GetParentNameList(decimal? AccParentId)
        {
            ParentNameList = "";
            if (AccParentId != 0 || AccParentId == null)
            {
                AccountTree tree = db.AccountTrees.Where(x => x.Id == AccParentId).FirstOrDefault();
               
                if (tree != null)
                {
                    int TreeLevel = Convert.ToInt32(tree.TheLevel);
                    AccId = Convert.ToDecimal(tree.AccParent);
                    for (int i = 0; i < TreeLevel; i++) 
                    {
                      //  AccId = tree.Id;
                        AccountTree t = db.AccountTrees.Where(x=>x.Id== AccId).FirstOrDefault();
                        ParentNameList += " <img src='/Temp/Image/Arrow.png' height='30' /> " + t.AccName.ToString();

                        AccId = Convert.ToDecimal(t.AccParent);
                         //  AccountTree P = db.AccountTrees.Find(t.Id);
                    }
                }
                else
                {
                    return ParentNameList;
                }
            }
            return ParentNameList;
        }
        
        public string GetAccCode(int AccId)
        {
            string AccCode;
            if (AccId == 0)
            {
                return AccCode = "0";
            }
            else
            {
                return AccCode = db.AccountTrees.Find(AccId).AccCode;
            }
        }
        
        public int GetAccParentCode(int AccId)
        {
            int AccCode;
            if (AccId == 0)
            {
                return AccCode = 0;
            }
            else
            {
                return AccCode = Convert.ToInt32(db.AccountTrees.Find(AccId).AccCode);
            }
        }
        
        public decimal GetParentId(decimal ParentID)
        {
            int Id;
            if (ParentID == 0)
            {
                return 0;
            }
            else
            {
                return Id = Convert.ToInt32(db.AccountTrees.Where(x => x.Id == ParentID).FirstOrDefault().Id);
            }
        }

        public int getTopParentId(int accTreeId)
        {
            try
            {
                if(db.AccountTrees.Any(x=>x.Id == accTreeId))
                {
                    var accParentRow = db.AccountTrees.Find(accTreeId);
                    int accParentId = Convert.ToInt32(accParentRow.AccParent);
                    if (accParentId > 0)
                    {
                        rootId = accParentId;
                        return this.getTopParentId(accParentId);
                    }
                    else
                    {
                        return rootId;
                    }
                }
                else
                {
                    return rootId;
                }
            }
            catch(Exception e)
            {
                return -1;
            }
        }
        
        public int getRootParentId(int? accTreeId)
        {
            try
            {
                if (db.AccountTrees.Any(x => x.Id == accTreeId))
                {
                    var accParentRow = db.AccountTrees.Find(accTreeId);
                    int accParentId = Convert.ToInt32(accParentRow.AccParent);
                    if (accParentId > 0)
                    {
                        rootId = accParentId;
                        return this.getRootParentId(accParentId);
                    }
                    else
                    {
                        return rootId;
                    }
                }
                else
                {
                    return rootId;
                }
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public void updateFinancialCycleCredint(int Id, decimal? oldAmount, decimal? newAmount, string userid)
        {
            FinancialCycle obj = db.FinancialCycles.Find(Id);

            obj.Credint = obj.Credint == null | obj.Credint <= 0 ? newAmount : obj.Credint - oldAmount + newAmount;
            obj.UpdatedBy = userid;
            obj.UpdatingDate = DateTime.Now;

            db.Entry(obj).State = EntityState.Modified;
            db.SaveChanges();
        }

        public void updateParentFinancialCycleCredint(int financeCycleId, int Id, decimal? oldAmount, decimal? newAmount, string userid)
        {
            try
            {
                if(db.Balances.Any(x=>x.AccountTreeId == Id & x.FinanceCycleId == financeCycleId))
                {
                    var obj = db.Balances.FirstOrDefault(x => x.AccountTreeId == Id & x.FinanceCycleId == financeCycleId);

                    obj.Credint = obj.Credint - oldAmount + newAmount;
                    obj.UpdatedBy = userid;
                    obj.UpdatingDate = DateTime.Now;

                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    Balance obj = new Balance();

                    obj.AccountTreeId = Id;
                    obj.FinanceCycleId = financeCycleId;
                    obj.Credint = newAmount;
                    obj.ActualExchange = 0;
                    obj.DeviationRatio = 0;
                    obj.RelativeDeviation = 0;

                    db.Balances.Add(obj);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                
            }
        }



        //دي حقت الموازنة كل بند اب(رئيسي) و التفاصيل حقته في الشجرة وضعها والصرف الفعلي ليها في الموازنة كيف.......ممكن نستخدمة في التقارير
        public object test(int financeCycleId)
        {
            var data = db.AccountTrees.Where(x => db.Balances.Any(b => b.AccountTreeId == x.Id & b.FinanceCycleId == financeCycleId))
                .Select(s=> new { parentId = s.AccParent}).Distinct().ToList();

            List<object> vaList = new List<object>();
            foreach (var item1 in data)
            {
                List<object> varList = new List<object>();
                decimal? sumOfCredint = 0;
                decimal? sumOfActualExchange = 0;
                string parentName = db.AccountTrees.Find(item1.parentId).AccName;
                foreach (var item in db.AccountTrees.Where(x => db.Balances.Any(b => b.AccountTreeId == x.Id & b.FinanceCycleId == financeCycleId) & x.AccParent == item1.parentId))
                {
                    sumOfCredint = sumOfCredint + db.Balances.Where(x => x.AccountTreeId == item.Id & x.FinanceCycleId == financeCycleId).Sum(d => d.Credint);
                    sumOfActualExchange = sumOfCredint + db.Balances.Where(x => x.AccountTreeId == item.Id & x.FinanceCycleId == financeCycleId).Sum(d => d.ActualExchange);

                    var obj = db.Balances.FirstOrDefault(x => x.AccountTreeId == item.Id & x.FinanceCycleId == financeCycleId);
                    string accName = db.AccountTrees.Find(item.Id).AccName;

                    myOb = new { Id = item.Id, accName = accName, Credint = obj.Credint, ActualExchange = obj.ActualExchange };
                    varList.Add(myOb);
                }

                vaList.Add(new { parentId = item1.parentId, sumOfCredint = sumOfCredint, parentName = parentName, details = varList });
            }

            return vaList;
        }
        
        //دي حقت الموازنة كل بند اب(رئيسي) و التفاصيل حقته في الشجرة وضعها والصرف الفعلي ليها في الموازنة كيف حسب تاريخ معين.......ممكن نستخدمة في التقارير
        public object test2(int financeCycleId, DateTime dateFrom)
        {
            var data = db.Transactions.Where(d => d.TransactionDate != null & DbFunctions.TruncateTime(d.TransactionDate) >= DbFunctions.TruncateTime(dateFrom))
                        .Select(x=> x.TransactionDetails
                            .Where(b=>db.Balances.Any(d=>d.AccountTreeId == b.AccTreeId))
                            .Select(f => new { f.Id, f.AccTreeId, f.AccountTree.AccName, f.Debit, f.Credit })).ToList();


            //var data1 = data.Where(x => x..Where(d => db.Balances.Any(f => f.AccountTreeId == d.AccTreeId))).ToList();

            //List<Transaction> data = db.Transactions.Where(t => t.TransactionDate.Value.Year == transactionDate.Year/* & t.TransactionDate.Value.Month == transactionDate.Month &*/
            /*t.TransactionDate.Value.Day == transactionDate.Day).ToList();*/

            //var data = db.AccountTrees.Where(x => db.TransactionDetails.Any(b => b.AccountTreeId == x.Id & b.FinanceCycleId == financeCycleId))
            //    .Select(s => new { parentId = s.AccParent }).Distinct().ToList();

            //List<object> vaList = new List<object>();
            //foreach (var item1 in data)
            //{
            //    List<object> varList = new List<object>();
            //    decimal? sumOfCredint = 0;
            //    decimal? sumOfActualExchange = 0;
            //    string parentName = db.AccountTrees.Find(item1.parentId).AccName;
            //    foreach (var item in db.AccountTrees.Where(x => db.Balances.Any(b => b.AccountTreeId == x.Id & b.FinanceCycleId == financeCycleId) & x.AccParent == item1.parentId))
            //    {
            //        sumOfCredint = sumOfCredint + db.Balances.Where(x => x.AccountTreeId == item.Id & x.FinanceCycleId == financeCycleId).Sum(d => d.Credint);
            //        sumOfActualExchange = sumOfCredint + db.Balances.Where(x => x.AccountTreeId == item.Id & x.FinanceCycleId == financeCycleId).Sum(d => d.ActualExchange);

            //        var obj = db.Balances.FirstOrDefault(x => x.AccountTreeId == item.Id & x.FinanceCycleId == financeCycleId);
            //        string accName = db.AccountTrees.Find(item.Id).AccName;

            //        myOb = new { Id = item.Id, accName = accName, Credint = obj.Credint, ActualExchange = obj.ActualExchange };
            //        varList.Add(myOb);
            //    }

            //    vaList.Add(new { parentId = item1.parentId, sumOfCredint = sumOfCredint, parentName = parentName, details = varList });
            //}

            //return vaList;
            return data;
        }
        
        //دي حقت الموازنة لكل بند في الشجرة وضعه والصرف الفعلي ليهو في الموازنة وصل الحدي وين.......ممكن نستخدمة في التقارير
        // دي ما شغالة
        public List<int> test1(int Id, int financeCycleId)
        {
            List<int> ids = new List<int>();

            if(db.AccountTrees.Any(x => x.Id == Id))
            {
                var accParentRow = db.AccountTrees.Find(Id);
                int accParentId = Convert.ToInt32(accParentRow.AccParent);
                if (accParentId > 0)
                {
                    ids.Add(accParentId);
                    rootId = accParentId;
                    return this.test1(accParentId, financeCycleId);
                }
                else
                {
                    return ids;
                }
            }
            else
            {
                return ids;
            }
        }


        //for all tree
        public string AddToTree(List<TreeAccVM> dataList, string accParentName, string userid)
        {
            if (dataList.Count() > 0)
            {
                var addAccParentId = db.AccountTrees.FirstOrDefault(x => x.AccName.Trim() == accParentName.Trim()).Id;

                foreach (var data in dataList)
                {
                    data.AccParent = addAccParentId;
                    int AccId = Convert.ToInt32(db.SpLastTreeAccountId().FirstOrDefault());
                    int AccParentCode = GetAccParentCode(Convert.ToInt32(data.AccParent));
                    //string AccCode = db.GetLastAccNumber(AccParentCode).FirstOrDefault().ToString();
                    string AccCode = GetLastAccCode(addAccParentId);

                    //get parent
                    AccountTree accountTree = db.AccountTrees.Find(data.AccParent);

                    if (data.AccName != null)
                    {
                        if (db.AccountTrees.Any(x => x.AccName == data.AccName))
                        {
                            return "error";
                            //return Json(new { Message = "هذا البند موجود  مسبقا في الشجرة المحاسبية", Title = "عملية الاضافة", Status = "error" });
                        }


                        if (!db.AccountTrees.Any(f => f.AccName == data.AccName))
                        {
                            AccountTree d = new AccountTree();
                            d.Id = AccId;
                            d.AccCode = AccCode;
                            d.AccName = data.AccName;
                            d.AccTypeId = data.AccTypeId;
                            d.AccNatureId = data.AccNatureId;
                            d.AccFinalId = data.AccFinalId;
                            d.CreatedBy = userid;
                            d.CreationDate = DateTime.Now;

                            if (data.AccTypeId == 2)//حساب فرعي
                            {
                                AccountSub sub = new AccountSub();
                                sub.AccCategoryId = data.AccSubCategory;
                                sub.AccTreeId = AccId;
                                sub.CreatedBy = userid;
                                sub.CreationDate = DateTime.Now;

                                db.AccountSubs.Add(sub);
                            }

                            if (accountTree == null)
                            {
                                d.AccParent = 0;
                            }
                            else
                            {
                                d.AccParent = accountTree.Id;
                            }
                            if (data.AccParent == null || data.AccParent == 0)
                            {
                                d.TheLevel = 0;
                            }
                            else
                            {
                                d.TheLevel = Convert.ToInt32(accountTree.TheLevel) + 1;
                            }

                            db.AccountTrees.Add(d);
                            db.SaveChanges();
                        }
                    }
                }
                return "success";
                //return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
            }
            else
            {
                return "error";
               //return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }
        
        //for bankAccount
        public int AddToTree(string accParentName, string accName, int accTypeId, int accSubCategory, string userid)
        {
            int res = 0;
            //يجب اختبار اسم الحساب الاب اولا
            if (accParentName != null)
            {
                //get parent object from database
                AccountTree parentAccountTree = db.AccountTrees.FirstOrDefault(x => x.AccName.Trim() == accParentName.Trim());
                
                //get Id
                int AccId = Convert.ToInt32(db.SpLastTreeAccountId().FirstOrDefault());
                
                //prent code
                //int AccParentCode = GetAccParentCode(parentAccountTree.Id);
                
                //currnet code
                //string AccCode = db.GetLastAccNumber(AccParentCode).FirstOrDefault().ToString();

                string AccCode = GetLastAccCode(parentAccountTree.Id);

                if (accName != null)// no error in inserted data
                {
                    if (db.AccountTrees.Any(x => x.AccName == accName))//already inserted in database table
                    {
                        var obj = db.AccountTrees.FirstOrDefault(f => f.AccName == accName);
                        res = Convert.ToInt32(obj.Id);
                    }
                    
                    //add to tree(accName not inserted before into database table)
                    if (!db.AccountTrees.Any(f => f.AccName == accName))
                    {
                        AccountTree d = new AccountTree();
                        d.Id = AccId;
                        d.AccCode = AccCode;
                        d.AccName = accName;
                        d.AccTypeId = accTypeId;
                        d.AccNatureId = parentAccountTree.AccNatureId;
                        d.AccFinalId = parentAccountTree.AccFinalId;
                        d.IsActive = true;
                        d.CreatedBy = userid;
                        d.CreationDate = DateTime.Now;

                        //parent
                        if (parentAccountTree == null)
                        {
                            d.AccParent = 0;
                        }
                        else
                        {
                            d.AccParent = parentAccountTree.Id;
                        }

                        //level
                        if (parentAccountTree.Id == null || parentAccountTree.Id == 0)
                        {
                            d.TheLevel = 0;
                        }
                        else
                        {
                            d.TheLevel = Convert.ToInt32(parentAccountTree.TheLevel) + 1;
                        }

                        //type
                        if (accTypeId == 2)//حساب فرعي
                        {
                            AccountSub sub = new AccountSub();
                            //sub.AccCategoryId = data.AccSubCategory;
                            sub.AccCategoryId = accSubCategory;
                            sub.AccTreeId = AccId;
                            sub.CreatedBy = userid;
                            sub.CreationDate = DateTime.Now;
                            db.AccountSubs.Add(sub);

                            db.AccountTrees.Add(d);
                            db.SaveChanges();

                            res = sub.Id;
                        }
                        else
                        {
                            db.AccountTrees.Add(d);
                            db.SaveChanges();

                            res = d.Id;
                        }
                    }
                }
            }
            else
            {
                res = -1;
            }

            return res;
        }
        




        /*[
            {"parentId":28,"sumOfCredint":739000000.00,"parentName":"عمليات الحركة الجوية",
                "details":[
                            {"Id":33,"accName":"رسوم  الهبوط","Credint":5000000.00,"ActualExchange":0.00},
                            {"Id":34,"accName":"رسوم خدمات الركاب","Credint":45000000.00,"ActualExchange":0.00},
                            {"Id":35,"accName":"رسوم البضائع","Credint":689000000.00,"ActualExchange":0.00}
                          ]},
            {"parentId":28,"sumOfCredint":739000000.00,"parentName":"عمليات الحركة الجوية",
                "details":[
                            {"Id":33,"accName":"رسوم  الهبوط","Credint":5000000.00,"ActualExchange":0.00},
                            {"Id":34,"accName":"رسوم خدمات الركاب","Credint":45000000.00,"ActualExchange":0.00},
                            {"Id":35,"accName":"رسوم البضائع","Credint":689000000.00,"ActualExchange":0.00}
                        ]},
            {"parentId":28,"sumOfCredint":739000000.00,"parentName":"عمليات الحركة الجوية",
                "details":[
                            {"Id":33,"accName":"رسوم  الهبوط","Credint":5000000.00,"ActualExchange":0.00},
                            {"Id":34,"accName":"رسوم خدمات الركاب","Credint":45000000.00,"ActualExchange":0.00},
                            {"Id":35,"accName":"رسوم البضائع","Credint":689000000.00,"ActualExchange":0.00}
                        ]},
            {"parentId":29,"sumOfCredint":8800000.00,"parentName":"رسوم الخدمات الأرضية",
                "details":[
                            {"Id":42,"accName":"شركات الطيران","Credint":8800000.00,"ActualExchange":0.00}
                          ]},
            {"parentId":7,"sumOfCredint":50000000.00,"parentName":"البنوك",
                "details":[
                            {"Id":43,"accName":"امدرمان مصروفات","Credint":30000000.00,"ActualExchange":0.00},
                            {"Id":44,"accName":"امدرمان ايرادات","Credint":20000000.00,"ActualExchange":0.00}
                        ]},
            {"parentId":7,"sumOfCredint":50000000.00,"parentName":"البنوك",
                "details":[
                            {"Id":43,"accName":"امدرمان مصروفات","Credint":30000000.00,"ActualExchange":0.00},
                            {"Id":44,"accName":"امدرمان ايرادات","Credint":20000000.00,"ActualExchange":0.00}
                          ]}
    ]*/


    }
}