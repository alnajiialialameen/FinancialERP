using System;
using System.Collections.Generic;
using System.Linq;
using Purchases.Models.ViewModal;
using Purchases.Models.ViewModel;
using System.Web.Mvc;
using Purchases.Models;
using System.Data.Entity;

namespace Purchases.Controllers
{
    // حسابات الموظفين والعملاء
    public class RecipientsController : Controller
    {
        private Entities db = new Entities();

        // GET: Recipients
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadData()
        {
            var data = db.Recipients.Select(q=> 
            new {
                q.Id,
                q.RecipientName,
                q.BankName,
                q.BranchName,
                q.AccountNumber
            }).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBanks(string q)
        {
            var data = db.Recipients.Select(x=> new { id = x.BankName, text = x.BankName}).Distinct().ToList();
            data = data.Where(x => x.text.Contains(q)).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBranches(string q)
        {
            var data = db.Recipients.Select(x => new { id = x.BranchName, text = x.BranchName }).Distinct().ToList();
            data = data.Where(x => x.text.Contains(q)).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(RecipientVM model)
        {
            //if(!ModelState.IsValid)
            //{
            //    return Json(new { Message = "يجب التاكد من صحة البيانات", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            //}

            if(db.Recipients.Any(x=> x.AccountNumber == model.AccountNumber && x.BankName == model.BankName && x.BranchName == model.BranchName))
            {
                return Json(new { Message = "هذا الحساب موجود مسبقا", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                Recipient Obj = new Recipient();

                Obj.AccountNumber = model.AccountNumber;
                Obj.BankName = model.BankName;
                Obj.BranchName = model.BranchName;
                Obj.RecipientName = model.RecipientName;

                db.Recipients.Add(Obj);
                db.SaveChanges();

                return Json(new { Message = "تمت عملية الاضافة بي نجاح", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Update(RecipientVM model)
        {
            if (!ModelState.IsValid || model.Id <= 0)
            {
                return Json(new { Message = "يجب التاكد من صحة البيانات", Title = "خطأ", Status = "error" });
            }

            if (db.Recipients.Any(x => x.Id != model.Id && (x.AccountNumber == model.AccountNumber && x.BankName == model.BankName && x.BranchName == model.BranchName)))
            {
                return Json(new { Message = "هذا الحساب موجود مسبقا", Title = "خطأ", Status = "error" });
            }
            else
            {
                Recipient Obj = db.Recipients.Find(model.Id);

                Obj.AccountNumber = model.AccountNumber;
                Obj.BankName = model.BankName;
                Obj.BranchName = model.BranchName;
                Obj.RecipientName = model.RecipientName;

                db.Entry(Obj).State = EntityState.Modified;
                db.SaveChanges();

                return Json(new { Message = "تمت عملية التعديل بي نجاح", Title = "نجاح", Status = "success" });
            }
        }

        public ActionResult Delete(int Id)
        {
            if (Id <= 0)
            {
                return Json(new { Message = "يجب التاكد من صحة البيانات", Title = "خطأ", Status = "error" });
            }

            if (!db.Recipients.Any(x => x.Id == Id))
            {
                return Json(new { Message = "هذا الحساب غير موجود", Title = "خطأ", Status = "error" });
            }
            else
            {
                Recipient Obj = db.Recipients.Find(Id);

                db.Recipients.Remove(Obj);
                db.SaveChanges();

                return Json(new { Message = "تمت عملية الحذف بي نجاح", Title = "نجاح", Status = "success" });
            }
        }

    }
}