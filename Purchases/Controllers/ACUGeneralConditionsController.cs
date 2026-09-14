using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Purchases.Models;
using Purchases.Models.ViewModel;
using Microsoft.AspNet.Identity;

namespace Purchases.Controllers
{
    public class ACUGeneralConditionsController : Controller
    {
        private Entities db = new Entities();
        
        // GET: ACUGeneralConditions
        public ActionResult Index()
        {
            return View(db.GeneralConditions.ToList());
        }

        public ActionResult getAllConditions()
        {
            List<GeneralConditionsVM> conditionList = new List<GeneralConditionsVM>();
            foreach (var item in db.GeneralConditions.Where(x=>x.IsActive == true).ToList())
            {
                conditionList.Add(
                    new GeneralConditionsVM()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        MaxValue = item.MaxValue,
                        IsActive = item.IsActive,
                    });
            }

            return Json(conditionList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getConditionById(int Id)
        {
            GeneralConditionsVM obj = new GeneralConditionsVM();

            if (db.GeneralConditions.Any(x => x.Id == Id))
            {
                GeneralCondition item = db.GeneralConditions.Find(Id);
                obj.Id = item.Id;
                obj.Name = item.Name;
                obj.MaxValue = item.MaxValue;
                //obj.IsActive = item.IsActive;

            }

            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        
        // POST: ACUGeneralConditions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ActionName("Add")]
        public ActionResult AddNewCondition(GeneralConditionsVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                if (!db.GeneralConditions.Any(x => x.Id== model.Id))
                {
                    GeneralCondition obj = new GeneralCondition();
                    obj.Name = model.Name;
                    obj.MaxValue = model.MaxValue;
                    obj.IsActive = model.IsActive;
                    obj.CreatedBy = userid;
                    obj.CreationDate = DateTime.Now;

                    db.GeneralConditions.Add(obj);
                    db.SaveChanges();

                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "عفوا تم إدخال بيانات رقم الترخيص من قبل", Title = "تنبيه", Status = "warning" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }

        //POST: ACUGeneralConditions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Update")]
        //[ValidateAntiForgeryToken]
        public ActionResult Update(GeneralConditionsVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                if (db.GeneralConditions.Any(x => x.Id == model.Id))
                {
                    GeneralCondition obj = db.GeneralConditions.Find(model.Id);
                    obj.Name = model.Name;
                    obj.MaxValue = model.MaxValue;
                    obj.IsActive = model.IsActive;
                    obj.UpdatedBy = userid;
                    obj.UpdatingDate = DateTime.Now;
                    
                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "عفوا تم إدخال بيانات رقم الترخيص من قبل", Title = "تنبيه", Status = "warning" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }

        //POST: ACUGeneralConditions/Delete/5


        [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int Id)
        {
            try
            {
                if (db.GeneralConditions.Any(x => x.Id == Id))
                {
                    GeneralCondition obj = db.GeneralConditions.Find(Id);
                    obj.IsActive = false;

                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { Message = "تمت العملية بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "تم إصدار أمر تشكيل من قبل", Title = "تنبيه", Status = "warning" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء العملية ", Title = "خطأ", Status = "error" });
            }
        }







        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
