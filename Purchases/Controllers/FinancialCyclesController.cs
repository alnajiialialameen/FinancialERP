using Purchases.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class FinancialCyclesController : Controller
    {
        private Entities db = new Entities();
        // GET: FinancialCycles
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadData()
        {
            var data = db.FinancialCycles.Select(b=> new {

                Id=b.Id,
                Year = b.Year,
                RelativeDeviation = b.RelativeDeviation,
                Credint = b.Credint,
                RelativeRatio = b.RelativeRatio,
                CurrentYear = b.CurrentYear,
                ActualExchange = b.ActualExchange,
                IsClosed = b.IsClosed,
            });

           

            
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Save(FinancialCycle ob)
        {
            FinancialCycle f = new FinancialCycle();

            f.Year = ob.Year;
            f.ActualExchange = 0;
            f.RelativeDeviation = 0;
            f.RelativeRatio = 0;
            f.CurrentYear = true;

            db.FinancialCycles.Add(f);
            db.SaveChanges();

            return Json(new { Message = "تمت عملية الاضافة بنجاح", Title = "نجاح", Status = "success" });
        }

        public ActionResult Edit(FinancialCycle data)
        {
            if (data.Id != 0 && data.Year != null)
            {
                int c = db.FinancialCycles.Where(f => f.Id != data.Id && f.Year == data.Year).Count();
                if (c == 0)
                {
                    FinancialCycle t = db.FinancialCycles.Find(data.Id);
                    // TeacherMaterial tm = db.TeacherMaterials.Single(f => f.TeacherId == data.Id && f.IsSpecialtyMaterial == true);

                    t.Year = data.Year;


                    db.Entry(t).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success" });
                }

            }

            return Json(new { Message = "حدث خطأ اثناء التعديل", Title = "خطأ", Status = "error" });
        }


        public ActionResult StopBalances(FinancialCycle data)
        {
            if (data.Id != 0)
            {
                FinancialCycle O = db.FinancialCycles.Find(data.Id);
                if (O.IsClosed == true)
                {

                    O.IsClosed = false;
                }
                else
                {
                    O.IsClosed = true;
                }
                db.Entry(O).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new { Message = "تمت تكملة الاجراء بنجاح شكرا ", Title = "نجاح", Status = "success" });

            }
            return Json(new { Message = " عذرا حدث خطأ أثناء العملية", Title = "خطأ", Status = "error" });
        }

    }
}