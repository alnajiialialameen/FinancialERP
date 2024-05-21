using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Purchases.Models;
using System.Data.Entity;

namespace Purchases.Controllers
{
    public class UnitsController : Controller
    {
        Entities db = new Entities();
        // GET: Units
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadData()
        {
            var data = db.Units.Select(b => new {

                Id = b.Id,
                Name = b.Name,
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Create(Unit data)
        {
            Unit t = new Unit();
            t.Name = data.Name;
            db.Units.Add(t);
            db.SaveChanges();

            return Json(new { Message = "تمت عملية الاضافة بنجاح", Title = "نجاح", Status = "success" });
        }

        public ActionResult Edit(Unit data)
        {
            if (data.Id != 0 && data.Name != null)
            {
                if(!db.Units.Any(f=>f.Name == data.Name))
                {
                    Unit t = db.Units.Find(data.Id);
                    t.Name = data.Name;
                    db.Entry(t).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success" });
                }

            }
            return Json(new { Message = "حدث خطأ اثناء التعديل", Title = "خطأ", Status = "error" });
        }

    }
}