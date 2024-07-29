using Microsoft.AspNet.Identity;
using Purchases.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class SuppliersController : Controller
    {
        private Entities db = new Entities();
        // GET: Suppliers;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadData()
        {
            var data = db.Suppliers.Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                Phone = p.Phone,
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(Supplier data)
        {
            if (data.Name != null)
            {
                var userid = User.Identity.GetUserId();

                if (!db.Suppliers.Any(x=>x.Name == data.Name))
                {
                    Supplier s = new Supplier();

                    s.Name = data.Name;
                    s.Phone = data.Phone;
                    s.CreatedBy = userid;
                    s.CreationDate = DateTime.Now;
                    db.Suppliers.Add(s);
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
            }
            return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });

        }

        public ActionResult Edit(Supplier data)
        {
            if (data.Id != 0 && data.Name != null)
            {
                var userid = User.Identity.GetUserId();

                if (!db.Suppliers.Any(x => x.Name == data.Name & x.Id != data.Id))
                {
                    Supplier f = db.Suppliers.Find(data.Id);

                    f.Name = data.Name;
                    f.Phone = data.Phone;
                    f.UpdatedBy = userid;
                    f.UpdatingDate = DateTime.Now;

                    db.Entry(f).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success" });
                }
                return Json(new { Message = "هذا الاسم مكرر", Title = "تنبيه", Status = "warning" });
            }
            
            return Json(new { Message = "حدث خطأ اثناء التعديل", Title = "خطأ", Status = "error" });
        }
    }
}