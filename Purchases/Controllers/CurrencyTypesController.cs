using Purchases.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class CurrencyTypesController : Controller
    {
        private Entities db = new Entities();
        // GET: CurrencyTypes;
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadData()
        {
            var data = db.CurrencyTypes.Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetCurrencyType(string q)
        {
            var data = db.CurrencyTypes.Select(p => new
            {
                id = p.Id,
                text = p.Name
            }).Where(x=>x.text.Contains(q));

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(CurrencyType data)
        {
            if (data.Name != null)
            {

                if (!db.CurrencyTypes.Any(x => x.Name == data.Name))
                {
                    CurrencyType s = new CurrencyType();

                    s.Name = data.Name;

                    db.CurrencyTypes.Add(s);
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
                return Json(new { Message = "عذرا هذه العملة موجودة سلفا", Title = "تنبيه", Status = "warning" });
            }
            return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });

        }

        public ActionResult Edit(CurrencyType data)
        {
            if (data.Id != 0 && data.Name != null)
            {

                if (!db.CurrencyTypes.Any(x => x.Id != data.Id & x.Name == data.Name))
                {
                    CurrencyType f = db.CurrencyTypes.Find(data.Id);
                    f.Name = data.Name;
                    db.Entry(f).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success" });
                }
                return Json(new { Message = "عذرا هذه العملة موجودة سلفا", Title = "تنبيه", Status = "warning" });
            }
            return Json(new { Message = "حدث خطأ اثناء التعديل", Title = "خطأ", Status = "error" });
        }
    }
}