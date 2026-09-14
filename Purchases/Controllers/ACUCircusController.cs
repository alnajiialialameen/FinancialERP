using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Purchases.Models;
using Purchases.MyLogic;
using Purchases.Models.ViewModel;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace Purchases.Controllers
{
    public class ACUCircusController : Controller
    {
        private Entities db = new Entities();

        // GET: Circus
        public ActionResult Index()
        {
            //var circus = db.Circus.Include(c => c.Order);
            return View();
        }

        //get All orders to be added to the circus
        public ActionResult Orders()
        {
            var data = db.Orders.Select(p => new
            {
                Id = p.Id,
                Department = p.DepartmentName,
                DepartmentId = p.DepartmentId,
                OrderDate = p.OrderDate.Value.Day + "/" + p.OrderDate.Value.Month + "/" + p.OrderDate.Value.Year,
                Description = p.Description,

            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //get All orders to be added to the circus
        public ActionResult LoadCircuedData()
        {
            var data = db.Orders.Select(p => new
            {
                Id = p.Id,
                Department = p.DepartmentName,
                DepartmentId = p.DepartmentId,
                OrderDate = p.OrderDate.Value.Day + "/" + p.OrderDate.Value.Month + "/" + p.OrderDate.Value.Year,
                Description = p.Description,

            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        // GET: Circus/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Circu circu = db.Circus.Find(id);
            if (circu == null)
            {
                return HttpNotFound();
            }
            return View(circu);
        }

        // GET: Circus/Create
        public async Task<ActionResult> Create(int Id)
        {
            ConsumeHRAPI api = new ConsumeHRAPI();

            var obj = db.Orders.Find(Id);

            ViewBag.OrderId = Id;
            ViewBag.Subject = obj.Description;
            ViewBag.DepartmentSenderId = obj.DepartmentId;
            ViewBag.DepartmentSenderName = obj.DepartmentName;
            ViewBag.DepartmentRecipientId = 5;//ده الرقم الخاص بالادارة العامة للشؤون االمالية والمحاسبية
            ViewBag.DepartmentRecipientName = api.getDepartments().Result.FirstOrDefault(x => x.id == 5).text;
            ViewBag.OrderDate = obj.OrderDate.Value.Day + " - " + obj.OrderDate.Value.Month + " - " + obj.OrderDate.Value.Year;
            ViewBag.CircusDate = DateTime.Today.Day + " - " + DateTime.Today.Month + " - " + DateTime.Today.Year;
            ViewBag.DepartmentRecipientName = api.getDepartments().Result.FirstOrDefault(x=>x.id == 5).text;
            string ReceiverEmployeeName = api.getAllEmployees().FirstOrDefault(x => x.Id == 1721).Name;
            ViewBag.ReceiverEmployeeName = ReceiverEmployeeName;
            ViewBag.ReceiverId = 1721;
            ViewBag.Signatur = ReceiverEmployeeName.Substring(0, ReceiverEmployeeName.IndexOf(' '));

            if (db.Circus.Any(x => x.OrderId == Id))
            {
                ViewBag.FNo = db.Circus.FirstOrDefault(x => x.OrderId == Id).FNo;
                ViewBag.Id = db.Circus.FirstOrDefault(x => x.OrderId == Id).Id;
            }
            
                return View();
        }

        // POST: Circus/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult CreateOrUpdate(CircuVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                if (model.Id > 0) //update
                {
                    if (!db.Circus.Any(x => x.FNo == model.FNo & x.Id != model.Id)) //نتاكد انه رقم الملف غير موجود في طلب اخر
                    {
                        Circu obj = db.Circus.Find(model.Id);

                        obj.OrderId = model.OrderId;
                        obj.DepartmentRecipientId = model.DepartmentRecipientId;
                        obj.DepartmentSenderId = model.DepartmentSenderId;
                        obj.DepartmentRecipientName = model.DepartmentRecipientName;
                        obj.DepartmentSenderName = model.DepartmentSenderName;
                        obj.ReceiverId = model.ReceiverId;
                        obj.ReceiverEmployeeName = model.ReceiverEmployeeName;
                        obj.FNo = model.FNo;
                        obj.CircusDate = DateTime.Today;
                        obj.Subject = model.Subject;
                        obj.Signatur = model.Signatur;
                        obj.UpdatedBy = userid;
                        obj.UpdatingDate = DateTime.Now;

                        db.Entry(obj).State = EntityState.Modified;
                        db.SaveChanges();

                        return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success" });
                    }
                    else
                    {
                        return Json(new { Message = "عفوا يوجد خطأ في بيانات السيرك المرسلة(رقم الملف موجود مسبقا)", Title = "تنبيه", Status = "warning" });
                    }
                }
                else // Add OR Create
                {
                    if (!db.Circus.Any(x => x.FNo == model.FNo))
                    {
                        Circu obj = new Circu();

                        obj.OrderId = model.OrderId;
                        obj.DepartmentRecipientId = model.DepartmentRecipientId;
                        obj.DepartmentSenderId = model.DepartmentSenderId;
                        obj.DepartmentRecipientName = model.DepartmentRecipientName;
                        obj.DepartmentSenderName = model.DepartmentSenderName;
                        obj.ReceiverId = model.ReceiverId;
                        obj.ReceiverEmployeeName = model.ReceiverEmployeeName;
                        obj.FNo = model.FNo;
                        obj.CircusDate = DateTime.Today;
                        obj.Subject = model.Subject;
                        obj.Signatur = model.Signatur;
                        obj.CreatedBy = userid;
                        obj.CreationDate = DateTime.Now;

                        db.Circus.Add(obj);
                        db.SaveChanges();

                        return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                    }
                    else
                    {
                        return Json(new { Message = "عفوا يوجد خطأ في بيانات السيرك المرسلة(رقم الملف موجود مسبقا)", Title = "تنبيه", Status = "warning" });
                    }
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }

        // GET: Circus/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Circu circu = db.Circus.Find(id);
            if (circu == null)
            {
                return HttpNotFound();
            }
            ViewBag.OrderId = new SelectList(db.Orders, "Id", "DepartmentName", circu.OrderId);
            return View(circu);
        }

        // POST: Circus/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult Update(CircuVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                if (db.Circus.Any(x => x.Id == model.Id))
                {
                    Circu obj = db.Circus.Find(model.Id);

                    obj.OrderId = model.OrderId;
                    obj.DepartmentRecipientId = model.DepartmentRecipientId;
                    obj.DepartmentSenderId = model.DepartmentSenderId;
                    obj.DepartmentRecipientName = model.DepartmentRecipientName;
                    obj.DepartmentSenderName = model.DepartmentSenderName;
                    obj.ReceiverId = model.ReceiverId;
                    obj.ReceiverEmployeeName = model.ReceiverEmployeeName;
                    obj.FNo = model.FNo;
                    obj.CircusDate = DateTime.Today;
                    obj.Subject = model.Subject;
                    obj.Signatur = model.Signatur;
                    obj.UpdatedBy = userid;
                    obj.UpdatingDate = DateTime.Now;

                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "عفوا يوجد خطأ بيانات الطلب في السيرك المرسلة", Title = "تنبيه", Status = "warning" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية التعديل", Title = "خطأ", Status = "error" });
            }
        }

        // GET: Circus/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Circu circu = db.Circus.Find(id);
            if (circu == null)
            {
                return HttpNotFound();
            }
            return View(circu);
        }

        // POST: Circus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Circu circu = db.Circus.Find(id);
            db.Circus.Remove(circu);
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