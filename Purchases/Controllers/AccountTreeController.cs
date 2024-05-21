using Purchases.Class;
using Purchases.Models;
using Purchases.Models.ViewModal;
using Purchases.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class AccountTreeController : Controller
    {
        // GET: AccountTree
        Entities db = new Entities();
        TreeClass ClsTree = new TreeClass();
        // GET: AccountTree
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult LoadTree()
        {
            IQueryable<AccountTree> AccountTree = db.AccountTrees;

            var data = new List<TreeModel>();
            foreach (var item in AccountTree)
            {
                TreeModel r = new TreeModel();
                r.Id = item.Id;
                r.AccName = item?.AccName;
                data.Add(r);
            }


             data.Insert(0, new TreeModel() { AccName = "-- اختار البند --", Id = 0 });

          //var data = db.AccountTrees.Select(p => new
          //  {
          //      AccName = p.AccName,
          //      AccCode = p.AccCode,
          //      Id = p.Id,
          //  });

           
            ///data.Insert(0, new AccountTree() { AccName = "-- اختار البند --", Id = 0 });



            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult LoadTreeUpdate()
        {

            var data = db.AccountTrees.Select(p => new
            {
                AccName = p.AccName,
                AccCode = p.AccCode,
                Id = p.Id,
            });


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadTreeAdd(string q)
        {

            var data = db.AccountTrees.Select(p => new
            {
                text = p.AccName,
                code = p.AccCode,
                id = p.Id,
            }).Where(f => f.text.Contains(q));


            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateNaji(AccountTree data)
        {
            int AccId = Convert.ToInt32(db.SpLastTreeAccountId().FirstOrDefault());
             int AccParentCode = ClsTree.GetAccParentCode(Convert.ToInt32(data.AccParent));

           /// int AccParentCode = Convert.ToInt32(data.AccParent);

            //int TheLastLevelCheck = Convert.ToInt32(db.SpLastLevel(AccParentCode).FirstOrDefault());

            //if (TheLastLevelCheck >= 9)
            //{
            //    return Json(new { Message = "لقت وصلت لاقصي  حد في الشجرة المحاسبية", Title = "عملية الاضافة", Status = "error" });
            //}
            string AccCode = db.GetLastAccNumber(AccParentCode).FirstOrDefault().ToString();


            AccountTree AccountTree = db.AccountTrees.Find(data.AccParent);

            if (data.AccName != null)
            {

                if (db.AccountTrees.Any(x => x.AccName == data.AccName))
                {
                    return Json(new { Message = "هذا البند موجود  مسبقا في الشجرة المحاسبية", Title = "عملية الاضافة", Status = "error" });
                }


                if (!db.AccountTrees.Any(f => f.AccName == data.AccName))
                {
                    AccountTree d = new AccountTree();
                    d.Id = AccId;
                    d.AccCode = AccCode;
                    d.AccName = data.AccName;
                    if (AccountTree == null)
                    {
                        d.AccParent = 0;
                    }
                    else
                    {
                        d.AccParent = AccountTree.Id;
                    }
                    if (data.AccParent == null || data.AccParent == 0)
                    {
                        d.TheLevel = 0;
                    }
                    else
                    {
                        d.TheLevel = Convert.ToInt32(AccountTree.TheLevel) + 1;
                    }

                    db.AccountTrees.Add(d);
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
            }
            return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });

        }
        
        //دالة الحفظ بعد التعديل
        public ActionResult Create(List<TreeAccVM> dataList, string accParentName)
        {
            string result = ClsTree.AddToTree(dataList, accParentName);

            if(result == "success")
            {
                return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
            }
            else
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }

            //if (dataList.Count() > 0)
            //{
            //    var addAccParentId = db.AccountTrees.FirstOrDefault(x => x.AccName.Trim() == addAccParentName.Trim()).Id;

            //    foreach (var data in dataList)
            //    {
            //        data.AccParent = addAccParentId;
            //        int AccId = Convert.ToInt32(db.SpLastTreeAccountId().FirstOrDefault());
            //        int AccParentCode = ClsTree.GetAccParentCode(Convert.ToInt32(data.AccParent));
            //        string AccCode = db.GetLastAccNumber(AccParentCode).FirstOrDefault().ToString();

            //        //get parent
            //        AccountTree accountTree = db.AccountTrees.Find(data.AccParent);

            //        if (data.AccName != null)
            //        {
            //            if (db.AccountTrees.Any(x => x.AccName == data.AccName))
            //            {
            //                return Json(new { Message = "هذا البند موجود  مسبقا في الشجرة المحاسبية", Title = "عملية الاضافة", Status = "error" });
            //            }


            //            if (!db.AccountTrees.Any(f => f.AccName == data.AccName))
            //            {
            //                AccountTree d = new AccountTree();
            //                d.Id = AccId;
            //                d.AccCode = AccCode;
            //                d.AccName = data.AccName;
            //                d.AccTypeId = data.AccTypeId;
            //                d.AccNatureId = data.AccNatureId;
            //                d.AccFinalId = data.AccFinalId;

            //                if (data.AccTypeId == 2)//حساب فرعي
            //                {
            //                    AccountSub sub = new AccountSub();
            //                    sub.AccCategoryId = data.AccSubCategory;
            //                    sub.AccTreeId = AccId;
            //                    db.AccountSubs.Add(sub);
            //                }

            //                if (accountTree == null)
            //                {
            //                    d.AccParent = 0;
            //                }
            //                else
            //                {
            //                    d.AccParent = accountTree.Id;
            //                }
            //                if (data.AccParent == null || data.AccParent == 0)
            //                {
            //                    d.TheLevel = 0;
            //                }
            //                else
            //                {
            //                    d.TheLevel = Convert.ToInt32(accountTree.TheLevel) + 1;
            //                }

            //                db.AccountTrees.Add(d);
            //                db.SaveChanges();
            //            }

            //        }
            //    }
            //    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
            //    //return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            //}
            //else
            //{
            //    return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            //}
        }




        public ActionResult GetAllItem()
        {

            IQueryable<AccountTree> CoursesNames = db.AccountTrees;


            var ItemList = new List<TreeViewModelItem>();
            foreach (var item in CoursesNames)
            {
                //AirPortsList.Add(new AirPortsViewMosels
                //{
                TreeViewModelItem r = new TreeViewModelItem();
                r.Id = item.Id;
                r.AccName = item?.AccName;
                r.AccParentName = ClsTree.GetParentName(item.AccParent);
                r.GetParentNameList = ClsTree.GetParentNameList(item.AccParent);

                //});
                ItemList.Add(r);
            }

            return Json(new { data = ItemList.ToList() }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetDataForUpdate(int id)
        {
            AccountTree AccountTree = db.AccountTrees.Find(id);

            TreeModel Model = new TreeModel();
            Model.AccName = AccountTree.AccName;
            Model.AccCode = Convert.ToDecimal(AccountTree.AccCode);
            Model.AccParent = Convert.ToInt32(ClsTree.GetParentId(Convert.ToDecimal(AccountTree.AccParent)));
            Model.Id = AccountTree.Id;
            //  Model.AccParentId = Convert.ToInt32(ClsTree.GetParentId(Convert.ToDecimal(AccountTree.AccParent)));

            return Json(Model, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Edit(TreeModel model)
        {

            if (model.Id >= 0)
            {

                if (db.AccountTrees.Any(x => x.AccName == model.AccName && x.Id != model.Id))
                {
                    return Json(new { Message = "هذا البند موجود  مسبقا في الشجرة المحاسبية", Title = "عملية الاضافة", Status = "error" });

                }

                AccountTree AccountTree = db.AccountTrees.Find(model.Id);

                AccountTree.AccName = model.AccName;



                db.Entry(AccountTree).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success" });
            }
            else
            {

                return Json(new { Message = "حدث خطأ اثناء التعديل", Title = "خطأ", Status = "error" });

            }

        }





        public ActionResult GetTreeParial(int? id)
        {

            List<AccountTree> Model = db.AccountTrees.ToList();
            return PartialView("_Tree");

        }



        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return Json(new { Status = "error", Message = "رقم العنصر غير موجود", Title = "تعذر الحذف" }, JsonRequestBehavior.AllowGet);
            }

            AccountTree AccountTree = db.AccountTrees.Find(id);
            if (AccountTree == null)
            {
                return Json(new { Status = "error", Message = "العنصر غير موجود", Title = "تعذر الحذف" }, JsonRequestBehavior.AllowGet);
            }

            if (db.Balances.Any(x => x.AccountTreeId == AccountTree.Id))
            {
                return Json(new { Status = "error", Message = "لايمكن حذف هذا العنصر", Title = "تعذر الحذف" }, JsonRequestBehavior.AllowGet);
            }

            //check is Has child
            if (db.AccountTrees.Any(x => x.AccParent == AccountTree.Id))
            {
                return Json(new { Status = "error", Message = "لايمكن حذف هذا العنصر", Title = "تعذر الحذف" }, JsonRequestBehavior.AllowGet);
            }
           
            //هنا يجب حذف البند من جدول ال AccountSubs اولا 
            if (AccountTree.AccTypeId == 2)
            {
                AccountSub AccSubObj = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == AccountTree.Id);
                db.AccountSubs.Remove(AccSubObj);
            }
            
            var AccountTreeDelete = db.AccountTrees.Find(id);
            db.AccountTrees.Remove(AccountTreeDelete);
            db.SaveChanges();
            return Json(new { Status = "success", Message = "تم حذف البند", Title = " الحذف" }, JsonRequestBehavior.AllowGet);
        }




        /*Get Data For Select2 Inputs*/
        public ActionResult GetAccountType(string q)
        {
            var data = db.AccountTypes.Select(p => new
            {
                id = p.Id,
                text = p.Name
            }).Where(f => f.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAccountNature(string q)
        {
            var data = db.AccountNatures.Select(p => new
            {
                id = p.Id,
                text = p.Name
            }).Where(f => f.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAccountFinal(string q)
        {
            var data = db.AccountFinals.Select(p => new
            {
                id = p.Id,
                text = p.Name
            }).Where(f => f.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAccountSubCategory(string q)
        {
            var data = db.AccountSubCategories.Select(p => new
            {
                id = p.Id,
                text = p.Name
            }).Where(f => f.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

    }
}