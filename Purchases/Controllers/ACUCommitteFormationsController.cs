using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Purchases.Models;
using Purchases.MyLogic;
using Purchases.Models.ViewModel;
using Purchases.Models.ViewModal;
using Microsoft.AspNet.Identity;

namespace Purchases.Controllers
{
    public class ACUCommitteFormationsController : Controller
    {
        private Entities db = new Entities();
        private ConsumeHRAPI hrAPIObj = new ConsumeHRAPI();


        public ActionResult getAllEmployees(int Id)
        {
            var data = hrAPIObj.getAllEmployees();
            if (db.CommitteFormations.Any(x => x.OrderId == Id))
            {
                var committeeFormationId = db.CommitteFormations.FirstOrDefault(x => x.OrderId == Id).Id;
                foreach (var item in db.CommitteeMembers.Where(x => x.CommitteeFormationId == committeeFormationId))
                {
                    var obj = data.FirstOrDefault(x=>x.Id == item.EmployeeId);
                    data.Remove(obj);
                }
                    
            }
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AllEmployees(int Id)
        {
            ViewBag.OrderId = Id;
            if (db.CommitteFormations.Any(x => x.OrderId == Id))
            {
                var committeeFormations = db.CommitteFormations.FirstOrDefault(x => x.OrderId == Id);
                var order = db.Orders.Find(Id);
                ViewBag.OrderId = Id;
                ViewBag.DepartmentName = db.Orders.Find(Id).DepartmentName;
                ViewBag.Description = db.Orders.Find(Id).Description;
                ViewBag.OrderDate = db.Orders.Find(Id).OrderDate;
                ViewBag.OrderDate = db.Orders.Find(Id).OrderDate.Value.Day.ToString() + "-" + db.Orders.Find(Id).OrderDate.Value.Month.ToString() + "-" + db.Orders.Find(Id).OrderDate.Value.Year;


                ViewBag.ACUCommitteFormationId = committeeFormations.Id;
                ViewBag.ResolutionNo = committeeFormations.ResolutionNo;
                ViewBag.ResolutionName = committeeFormations.ResolutionName;
                ViewBag.ResolutionDate = committeeFormations.ResolutionDate.Value.Day.ToString() + "-" + committeeFormations.ResolutionDate.Value.Month.ToString() + "-" + committeeFormations.ResolutionDate.Value.Year;
                ViewBag.Subject = committeeFormations.Subject;
            }
            else
            {
                ViewBag.OrderId = Id;
            }

            return View();
        }

        public ActionResult getAllCommitteMemebers(int Id)
        {
            List<object> employeeList = new List<object>();
            if (db.CommitteFormations.Any(x => x.OrderId == Id))
            {
                var committeeFormationId = db.CommitteFormations.FirstOrDefault(x => x.OrderId == Id).Id;
                var data = db.CommitteeMembers.Where(x => x.CommitteeFormationId == committeeFormationId).ToList();
                var employeesList = hrAPIObj.getAllEmployees();

                
                foreach (var item in data)
                {
                    employeeList.Add(
                        new
                        {
                            Id = item.Id,
                            EmployeeId = employeesList.FirstOrDefault(x => x.Id == item.EmployeeId).Id,
                            Name = employeesList.FirstOrDefault(x => x.Id == item.EmployeeId).Name,                            
                            Department = employeesList.FirstOrDefault(x => x.Id == item.EmployeeId).Department,
                            Grade = employeesList.FirstOrDefault(x => x.Id == item.EmployeeId).Grade,
                            Gender = employeesList.FirstOrDefault(x => x.Id == item.EmployeeId).Gender,
                            EmploymetType = employeesList.FirstOrDefault(x => x.Id == item.EmployeeId).EmploymetType,
                            Job = item.CommitteeJob.Name,
                        });
                }

                return Json(employeeList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(employeeList, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult Add(CommitteFormationsVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                if (!db.CommitteFormations.Any(x=>x.OrderId == model.OrderId))
                {
                    CommitteFormation obj = new CommitteFormation();
                    obj.OrderId = model.OrderId;
                    obj.ResolutionNo = model.ResolutionNo;
                    obj.ResolutionDate = model.ResolutionDate;
                    obj.ResolutionName = model.ResolutionName;
                    obj.Subject = model.Subject;
                    obj.CommitteeTypeId = model.CommitteeTypeId;
                    obj.CreatedBy = userid;
                    obj.CreationDate = DateTime.Now;

                    db.CommitteFormations.Add(obj);
                    int Id = db.SaveChanges();

                    ViewBag.ACUCommitteFormationId = Id;
                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "تم إصدار أمر تشكيل من قبل", Title = "تنبيه", Status = "warning" });
                }
            }
            catch(Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }

        [HttpPost]
        public ActionResult Update(CommitteFormationsVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                CommitteFormation obj = db.CommitteFormations.Find(model.Id);
                obj.OrderId = model.OrderId;
                obj.ResolutionNo = model.ResolutionNo;
                obj.ResolutionDate = model.ResolutionDate;
                obj.ResolutionName = model.ResolutionName;
                obj.Subject = model.Subject;
                obj.CommitteeTypeId = model.CommitteeTypeId;
                obj.UpdatedBy = userid;
                obj.UpdatingDate = DateTime.Now;

                db.Entry(obj).State = EntityState.Modified;
                int Id = db.SaveChanges();

                ViewBag.ACUCommitteFormationId = Id;
                return Json(new { Message = "تمت عملية التعديل  بنجاح", Title = "نجاح", Status = "success" });
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية التعديل", Title = "خطأ", Status = "error" });
            }
        }
        
        // GET: ACUCommitteFormations
        public ActionResult Index(int Id)
        {
            if (db.CommitteFormations.Any(c => c.OrderId == Id))
            {
                var committeFormations = db.CommitteFormations.FirstOrDefault(c => c.OrderId == Id);

                ViewBag.OrderId = Id;
                ViewBag.ResolutionNo = committeFormations.ResolutionNo;
                ViewBag.ResolutionName = committeFormations.ResolutionName;
                ViewBag.OrderDate = committeFormations.Order.OrderDate.Value.Day.ToString() + "-" +committeFormations.Order.OrderDate.Value.Month.ToString() + "-" + committeFormations.Order.OrderDate.Value.Year;
                ViewBag.ResolutionDate = committeFormations.ResolutionDate.Value.Day.ToString() + "-" +committeFormations.ResolutionDate.Value.Month.ToString() + "-" + committeFormations.ResolutionDate.Value.Year;
                ViewBag.Subject = committeFormations.Subject;
                ViewBag.DepartmentName = db.Orders.Find(Id).DepartmentName;
                ViewBag.Description = db.Orders.Find(Id).Description;
                ViewBag.ACUCommitteFormationId = committeFormations.Id;
                ViewBag.CommitteeTypeId = committeFormations.CommitteeTypeId;
            }
            else
            {
                var orderObj = db.Orders.Find(Id);

                ViewBag.OrderId = Id;
                ViewBag.OrderDate = orderObj.OrderDate.Value.Day.ToString() + "-" + orderObj.OrderDate.Value.Month.ToString() + "-" + orderObj.OrderDate.Value.Year;
            }
            return View();
        }

        [ActionName("AddMember")]
        [HttpPost]
        public ActionResult AddCommitteeMembers(CommitteeMemberVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                if (!db.CommitteeMembers.Any(x => x.CommitteeFormationId == model.CommitteFormationId & x.EmployeeId == model.EmployeeId))
                {
                    CommitteeMember obj = new CommitteeMember();
                    obj.CommitteeFormationId = model.CommitteFormationId;
                    obj.EmployeeId = model.EmployeeId;
                    obj.CommitteeJobId = model.CommitteeJobId;
                    obj.CreatedBy = userid;
                    obj.CreationDate = DateTime.Now;

                    db.CommitteeMembers.Add(obj);
                    int Id = db.SaveChanges();
                    
                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "تم إصدار أمر تشكيل من قبل", Title = "تنبيه", Status = "warning" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }
        
        [ActionName("UpdateMember")]
        [HttpPost]
        public ActionResult UpdateCommitteeMembers(CommitteeMemberVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                if (db.CommitteeMembers.Any(x => x.Id == model.Id & x.EmployeeId == model.EmployeeId))
                {
                    CommitteeMember obj = db.CommitteeMembers.Find(model.Id);
                    
                    obj.CommitteeJobId = model.CommitteeJobId;
                    obj.UpdatedBy = userid;
                    obj.UpdatingDate = DateTime.Now;

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
        
        [ActionName("DeleteMember")]
        [HttpPost]
        public ActionResult DeleteCommitteeMembers(CommitteeMemberVM model)
        {
            try
            {
                if (db.CommitteeMembers.Any(x => x.Id == model.Id))
                {
                    CommitteeMember obj = db.CommitteeMembers.Find(model.Id);

                    db.CommitteeMembers.Remove(obj);
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
        
        /*----------------------------------------Companies Related Operations -----------------------------------------*/
        [ActionName("AddCompany")]
        [HttpPost]
        public ActionResult AddCommitteeCompany(CommitteeMemberVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                if (!db.CompetingCompanies.Any(x => x.OrderId == model.OrderId & x.CompanyRegisterationId == model.CompanyRegisterationId))
                {
                    CompetingCompany obj = new CompetingCompany();
                    obj.CommitteeFormationId = model.CommitteFormationId;
                    obj.OrderId = model.OrderId;
                    obj.CompanyRegisterationId = model.CompanyRegisterationId;
                    obj.CreatedBy = userid;
                    obj.CreationDate = DateTime.Now;

                    db.CompetingCompanies.Add(obj);
                    int Id = db.SaveChanges();

                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "تم إدخال هذا الشركة من قبل ك منافس في هذا العطاء", Title = "تنبيه", Status = "warning" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }

        [ActionName("UpdateCompany")]
        [HttpPost]
        public ActionResult UpdateCommitteeCompanies(CommitteeMemberVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                if (!db.CompetingCompanies.Any(x => x.Id == model.Id & x.CompanyRegisterationId == model.CompanyRegisterationId))
                {
                    CompetingCompany obj = db.CompetingCompanies.Find(model.Id);

                    obj.CompanyRegisterationId = model.CompanyRegisterationId;
                    obj.CreatedBy = userid;
                    obj.UpdatingDate = DateTime.Now;

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

        [ActionName("DeleteCompany")]
        [HttpPost]
        public ActionResult DeleteCommitteeCompany(int Id)
        {
            try
            {
                if (db.CompetingCompanies.Any(x => x.Id == Id))
                {
                    CompetingCompany obj = db.CompetingCompanies.Find(Id);

                    db.CompetingCompanies.Remove(obj);
                    db.SaveChanges();

                    return Json(new { Message = "تمت العملية بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "عفوا لا يوجد عنصر مسجل ", Title = "تنبيه", Status = "warning" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء العملية ", Title = "خطأ", Status = "error" });
            }
        }

        [ActionName("GetCompaniesForList")]
        [HttpGet]
        public ActionResult GetCompaniesForList(int CommitteeTypeId)
        {
            var data = new List<object>();
            try
            {
                if (CommitteeTypeId > 0)
                {
                    //CommitteeTypeId == 1 جلب كل الشركات
                    if (CommitteeTypeId == 1 & db.CompanyRegisterations.Any(x => x.Status == 1))//مناقصة عامة
                    {
                        data = new List<object>();
                        foreach (var obj in db.CompanyRegisterations.Where(x => x.Status == 1).ToList())
                        {
                            data.Add(new { Id = obj.Id, Name = obj.Name });
                        }

                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                    else if (CommitteeTypeId == 2 & db.CompanyRegisterations.Any(x => x.Status == 1 & x.IsQualified == true))//مناقصة محدودة
                    { //CommitteeTypeId == 2 جلب الشركات المؤهلة فقط

                        data.Clear();
                        foreach (var obj in db.CompanyRegisterations.Where(x => x.Status == 1 & x.IsQualified == true).ToList())
                        {
                            data.Add(new { Id = obj.Id, Name = obj.Name });
                        }

                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                    else if (CommitteeTypeId == 3 & db.CompanyRegisterations.Any(x => x.Status == 1 & x.IsAgentCompany == true))//شراء مباشر من الوكيل
                    { //CommitteeTypeId == 3 جلب الشركات الوكلاء فقط

                        data.Clear();
                        foreach (var obj in db.CompanyRegisterations.Where(x => x.Status == 1 & x.IsAgentCompany == true).ToList())
                        {
                            data.Add(new { Id = obj.Id, Name = obj.Name });
                        }

                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        data.Clear();
                        return Json(new { Message = "حدث خطأ أثناء العملية1111 ", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    data.Clear();
                    return Json(new { Message = "حدث خطأ أثناء العملية222 ", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                data.Clear();
                return Json(new { Message = "حدث خطأ أثناء العملية ", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        /*----------------------------------------End Companies Related Operations -----------------------------------------*/


        // GET: ACUCommitteFormations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CommitteFormation committeFormation = db.CommitteFormations.Find(id);
            if (committeFormation == null)
            {
                return HttpNotFound();
            }
            return View(committeFormation);
        }

        // GET: ACUCommitteFormations/Create
        public ActionResult Create()
        {
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "DepartmentName");
            return View();
        }

        // POST: ACUCommitteFormations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,ResolutionNo,ResolutionName,ResolutionDate,Subjec,OrderId,CreationDate,CreatedBy,UpdatingDate,UpdatedBy")] CommitteFormation committeFormation)
        {
            if (ModelState.IsValid)
            {
                db.CommitteFormations.Add(committeFormation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.OrderId = new SelectList(db.Orders, "Id", "DepartmentName", committeFormation.OrderId);
            return View(committeFormation);
        }

        // GET: ACUCommitteFormations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CommitteFormation committeFormation = db.CommitteFormations.Find(id);
            if (committeFormation == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "DepartmentName", committeFormation.OrderId);
            return View(committeFormation);
        }

        // POST: ACUCommitteFormations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,ResolutionNo,ResolutionName,ResolutionDate,Subjec,OrderId,CreationDate,CreatedBy,UpdatingDate,UpdatedBy")] CommitteFormation committeFormation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(committeFormation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "DepartmentName", committeFormation.OrderId);
            return View(committeFormation);
        }

        // GET: ACUCommitteFormations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CommitteFormation committeFormation = db.CommitteFormations.Find(id);
            if (committeFormation == null)
            {
                return HttpNotFound();
            }
            return View(committeFormation);
        }

        // POST: ACUCommitteFormations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CommitteFormation committeFormation = db.CommitteFormations.Find(id);
            db.CommitteFormations.Remove(committeFormation);
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
