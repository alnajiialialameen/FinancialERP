using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Purchases.Models;
using System.Data.Entity;
using Purchases.Models.ViewModal;
using Purchases.Class;

namespace Purchases.Controllers
{
    public class BankAccountsController : Controller
    {
        Entities db = new Entities();
        TreeClass treecls = new TreeClass();
        // GET: BankAccounts
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult LoadData()
        {
            var data = db.AccountSubs.Where(x => x.AccountTree.AccParent == 7).Select(p => new
            {
                Id = p.Id,
                Name = p.AccountTree.AccName,
                BankId = db.BankAccounts.Any(x=>x.AccountSubId == p.Id)? db.BankAccounts.FirstOrDefault(x => x.AccountSubId == p.Id).Id:0,
            });
            
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult getBankData(int accountSubId)
        {
            var data = new object();
            if(db.BankAccounts.Any(x=>x.AccountSubId == accountSubId))
            {
                var p = db.BankAccounts.FirstOrDefault(x => x.AccountSubId == accountSubId);
                data = new
                {
                    Id = p.Id,
                    Name = p.AccountSub.AccountTree.AccName,
                    Number = p.Number,
                    CurrencyTypeId = p.CurrencyTypeId,
                    CurrencyType = p.CurrencyType.Name,
                    OpenDate = p.OpenDate.ToString(),
                    IBan = p.IBan,
                    BankAccountTypeId = p.BankAccountTypeId,
                    BankAccountType = p.BankAccountType.Name
                };
            }
            
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Create(BankViewmodel data)
        {
            try
            {
                //int res = treecls.AddToTree(data.accParentName, data.AccName, 2, data.AccCategoryId);

                if (data.Id > 0)
                {
                    BankAccount b = db.BankAccounts.Find(data.Id);

                    b.BankAccountTypeId = data.BankAccountTypeId;
                    b.Number = data.Number;
                    b.OpenDate = Convert.ToDateTime(data.OpenDate);
                    b.CurrencyTypeId = data.CurrencyTypeId;
                    b.IBan = data.IBan;

                    db.Entry(b).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { Message = "تم التعديل بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    BankAccount b = new BankAccount();

                    b.AccountSubId = data.AccountSubId;
                    b.BankAccountTypeId = data.BankAccountTypeId;
                    b.Number = data.Number;
                    b.OpenDate = Convert.ToDateTime(data.OpenDate);
                    b.CurrencyTypeId = data.CurrencyTypeId;
                    b.IBan = data.IBan;

                    db.BankAccounts.Add(b);
                    db.SaveChanges();

                    return Json(new { Message = "تمت الاضافة بنجاح", Title = "نجاح", Status = "success" });
                }
            }catch(Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }

        [HttpPost]
        public ActionResult Update(BankViewmodel data)
        {
            try
            {
                BankAccount bank = db.BankAccounts.Find(data.Id);
                AccountTree tree = db.AccountTrees.Find(data.AccId);

                tree.AccName = data.AccName;

                bank.BankAccountTypeId = data.BankAccountTypeId;
                bank.Number = data.Number;
                bank.OpenDate = Convert.ToDateTime(data.OpenDate);
                bank.CurrencyTypeId = data.CurrencyTypeId;
                bank.IBan = data.IBan;

                db.Entry(bank).State = EntityState.Modified;
                db.Entry(tree).State = EntityState.Modified;

                db.SaveChanges();

                return Json(new { Message = "تمت الاضافة بنجاح", Title = "نجاح", Status = "success" }); 
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }

        public ActionResult GetBankName(string q)
        {
            var data = db.AccountSubs.Where(f=>f.AccCategoryId == 1 & !f.BankAccounts.Any(g=>g.AccountSubId == f.Id) )
                .Select(p => new
                {
                    id = p.Id,
                    text = p.AccountTree.AccName
                }).Where(x => x.text.Contains(q));

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCurrencyType(string q)
        {
            var data = db.CurrencyTypes.Select(p => new
            {
                id = p.Id,
                text = p.Name
            }).Where(x => x.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBankAccountType(string q)
        {
            var data = db.BankAccountTypes.Select(p => new
            {
                id = p.Id,
                text = p.Name
            }).Where(x => x.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        


        public ActionResult checkCode(int Id)
        {
            string data = treecls.GetLastAccCode(Id);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        /*---------------------------------------------- Checks ----------------------------------------------------*/

        public ActionResult Check(int Id)
        {
            if(Id > 0)
            {

                var bank = db.BankAccounts.Find(Id);
                var bankName = db.AccountTrees.FirstOrDefault(x => x.Id == bank.AccountSub.AccTreeId).AccName;

                ViewBag.bankName = bankName;
                ViewBag.bankId = Id;

                return View();
            }
            else
            {
                return Json(new { Message = "حدث خطأ أثناء إجراء العملية", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
            
        }

        public ActionResult LoadCheckData(int Id)
        {
            var data = db.Checks.Where(x=>x.BankAccountId == Id).Select(p => new
            {
                Id = p.Id,
                StartFromNumber = p.StartFromNumber,
                EndToNumber = p.EndToNumber,
                BooKNumber = p.BooKNumber,
                IsFinished = p.IsFinished,
                BankAccountId = p.BankAccountId
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateOrUpdate(Check model)
        {
            try
            {
                if(model.Id > 0)
                {
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { Message = "تمت عملية التعديل  بنجاح", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    db.Checks.Add(model);
                    db.SaveChanges();

                    return Json(new { Message = "تمت عملية الاضافة  بنجاح", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء إجراء العملية", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteCheck(int Id)
        {
            try
            {
                var data = db.Checks.Find(Id);
                data.IsFinished = true;

                db.Entry(data).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { Message = "تمت عملية الايقاف  بنجاح", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الايقاف", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}