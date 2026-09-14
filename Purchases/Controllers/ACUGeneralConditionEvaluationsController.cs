using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Purchases.Models;
using Purchases.Models.ViewModel;
using Microsoft.AspNet.Identity;

namespace Purchases.Controllers
{
    public class ACUGeneralConditionEvaluationsController : Controller
    {
        private Entities db = new Entities();

        // GET: ACUGeneralConditionEvaluations
        public ActionResult Index(int Id, int OrderId)
        {
            CompetingCompany CompetingCompanyObj = db.CompetingCompanies.Find(Id);
            int CompanyRegisterationId = Convert.ToInt32(CompetingCompanyObj.CompanyRegisterationId);
            CompanyRegisteration regObj = db.CompanyRegisterations.Find(CompanyRegisterationId);
           
            //كل ما يتعلق بي بيانات الشركة
            @ViewBag.CompanyRegisterationId = regObj.Id;
            @ViewBag.CompanyName = regObj.Name;
            @ViewBag.Phones = regObj.Phone1 + " - " + regObj.Phone2;
            @ViewBag.Email = regObj.Email;
            @ViewBag.Address = regObj.Address;
            @ViewBag.LicenseNumber = regObj.LicenseNumber;
            @ViewBag.Status = regObj.Status == 1 ? "الشركة / المؤسسة ما زالت موجودة في سوق العمل وتعمل حاليا يرجي مراجعة بياناتها بالتواصل المباشر او عن طريق المناديب" :
                "الشركة / المؤسسة غير موجودة في سوق العمل وبالتالي هي خارج اطار المنافسة في جميع المناقصات والعطاءات المطروحة حاليا او التي ستطرح مستقبلا";

            // كل ما يتعلق بي بيانات العطاء
            var orderObj = db.Orders.Find(CompetingCompanyObj.OrderId);
            ViewBag.DepartmentName = orderObj.DepartmentName;
            ViewBag.Description = orderObj.Description;
            ViewBag.OrderDate = orderObj.OrderDate.Value.Day.ToString() + "-" + orderObj.OrderDate.Value.Month.ToString() + "-" + orderObj.OrderDate.Value.Year;
            
            ViewBag.CompetingCompanyId = Id;
            
            return View();
        }

        public ActionResult getAllGeneralConditions(int Id)
        {
            if (db.GeneralConditions.Any(x => x.IsActive == true))
            {
                var data = db.GeneralConditions.Where(x => x.IsActive == true)
                    .Select(par => new
                    {
                        Id = par.Id,
                        Name = par.Name,
                        MaxValue = par.MaxValue,
                        IsActive = par.IsActive,
                        InputValue = db.GeneralConditionEvaluations.Any(x => x.GeneralConditionId == par.Id & x.CompetingCompanyId == Id) ?
                        db.GeneralConditionEvaluations.FirstOrDefault(x => x.GeneralConditionId == par.Id & x.CompetingCompanyId == Id).Value : null
                    });

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Evaluate(GeneralConditionsVM model)
        {
            try {
                var userid = User.Identity.GetUserId();

                if (!db.GeneralConditionEvaluations.Any(x => x.GeneralConditionId == model.GeneralConditionId & x.CompetingCompanyId == model.CompetingCompanyId))
                {
                    GeneralConditionEvaluation obj = new GeneralConditionEvaluation();
                    obj.CompetingCompanyId = model.CompetingCompanyId;
                    obj.GeneralConditionId = model.GeneralConditionId;
                    obj.Value = model.InputValue;
                    obj.CreatedBy = userid;
                    obj.CreationDate = DateTime.Now;

                    db.GeneralConditionEvaluations.Add(obj);
                    db.SaveChanges();

                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    GeneralConditionEvaluation obj = db.GeneralConditionEvaluations.FirstOrDefault(x => x.GeneralConditionId == model.GeneralConditionId & x.CompetingCompanyId == model.CompetingCompanyId);
                    obj.CompetingCompanyId = model.CompetingCompanyId;
                    obj.GeneralConditionId = model.GeneralConditionId;
                    obj.Value = model.InputValue;
                    obj.UpdatedBy = userid;
                    obj.UpdatingDate = DateTime.Now;

                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { Message = "تمت عملية التعديل  بنجاح", Title = "نجاح", Status = "success" });
                }
            }
            catch(Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }


        /*-----------------------------------------------------------------------------------------------------------*/
        // GET: ACUGeneralConditionEvaluations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GeneralConditionEvaluation generalConditionEvaluation = db.GeneralConditionEvaluations.Find(id);
            if (generalConditionEvaluation == null)
            {
                return HttpNotFound();
            }
            return View(generalConditionEvaluation);
        }

        // GET: ACUGeneralConditionEvaluations/Create
        public ActionResult Create()
        {
            ViewBag.GeneralConditionId = new SelectList(db.GeneralConditions, "Id", "Name");
            ViewBag.CompetingCompanyId = new SelectList(db.CompetingCompanies, "Id", "Id");
            return View();
        }

        // POST: ACUGeneralConditionEvaluations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CompetingCompanyId,GeneralConditionId,Value,CreationDate,CreatedBy,UpdatingDate,UpdatedBy")] GeneralConditionEvaluation generalConditionEvaluation)
        {
            if (ModelState.IsValid)
            {
                db.GeneralConditionEvaluations.Add(generalConditionEvaluation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.GeneralConditionId = new SelectList(db.GeneralConditions, "Id", "Name", generalConditionEvaluation.GeneralConditionId);
            ViewBag.CompetingCompanyId = new SelectList(db.CompetingCompanies, "Id", "Id", generalConditionEvaluation.CompetingCompanyId);
            return View(generalConditionEvaluation);
        }

        // GET: ACUGeneralConditionEvaluations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GeneralConditionEvaluation generalConditionEvaluation = db.GeneralConditionEvaluations.Find(id);
            if (generalConditionEvaluation == null)
            {
                return HttpNotFound();
            }
            ViewBag.GeneralConditionId = new SelectList(db.GeneralConditions, "Id", "Name", generalConditionEvaluation.GeneralConditionId);
            ViewBag.CompetingCompanyId = new SelectList(db.CompetingCompanies, "Id", "Id", generalConditionEvaluation.CompetingCompanyId);
            return View(generalConditionEvaluation);
        }

        // POST: ACUGeneralConditionEvaluations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CompetingCompanyId,GeneralConditionId,Value,CreationDate,CreatedBy,UpdatingDate,UpdatedBy")] GeneralConditionEvaluation generalConditionEvaluation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(generalConditionEvaluation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.GeneralConditionId = new SelectList(db.GeneralConditions, "Id", "Name", generalConditionEvaluation.GeneralConditionId);
            ViewBag.CompetingCompanyId = new SelectList(db.CompetingCompanies, "Id", "Id", generalConditionEvaluation.CompetingCompanyId);
            return View(generalConditionEvaluation);
        }

        // GET: ACUGeneralConditionEvaluations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GeneralConditionEvaluation generalConditionEvaluation = db.GeneralConditionEvaluations.Find(id);
            if (generalConditionEvaluation == null)
            {
                return HttpNotFound();
            }
            return View(generalConditionEvaluation);
        }

        // POST: ACUGeneralConditionEvaluations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            GeneralConditionEvaluation generalConditionEvaluation = db.GeneralConditionEvaluations.Find(id);
            db.GeneralConditionEvaluations.Remove(generalConditionEvaluation);
            db.SaveChanges();
            return RedirectToAction("Index");
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
