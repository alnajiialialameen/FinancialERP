using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using Purchases.Models;

namespace Purchases.Controllers
{
    public class VatsController : Controller
    {
        
        Entities db = new Entities();
        // GET: Vats
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult LoadData()
        {
            var data = db.Vats.Select(b => new {

                Id = b.Id,
                Name = b.Name,
                Value = "%" + b.Value
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetVat(string q)
        {
            var data = db.Vats.Select(p => new
            {
                id = p.Value,
                text = p.Name
            }).Where(f=>f.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(Vat data)
        {
            Vat t = new Vat();
            t.Name = data.Name;
            t.Value = data.Value;
            db.Vats.Add(t);
            db.SaveChanges();

            return Json(new { Message = "تمت عملية الاضافة بنجاح", Title = "نجاح", Status = "success" });
        }

        [HttpPost]
        public ActionResult Edit(Vat data)
        {
            if (data.Id != 0 && data.Name != null && data.Value != null)
            {
                if (db.Vats.Any(f => f.Name == data.Name && f.Id == data.Id))
                {
                    Vat t = db.Vats.Find(data.Id);
                    t.Name = data.Name;
                    t.Value = data.Value;
                    db.Entry(t).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success" });
                }

            }
            return Json(new { Message = "حدث خطأ اثناء التعديل", Title = "خطأ", Status = "error" });
        }
    }
}