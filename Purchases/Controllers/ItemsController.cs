using Purchases.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class ItemsController : Controller
    {
        private Entities db = new Entities();

        // GET: Items
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult LoadData()
        {
            var data = db.Items.Select(b => new {

                Id = b.Id,
                Name = b.Name,
                Count = db.ItemDetails.Where(h => h.ItemId == b.Id).Count()

            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult Save(Item data)
        {
            if(!db.Items.Any(f=>f.Name == data.Name))
            {
                Item t = new Item();
                t.Name = data.Name;
                db.Items.Add(t);
                db.SaveChanges();
                return Json(new { Message = "تمت عملية الاضافة بنجاح", Title = "نجاح", Status = "success" });
            }
            return Json(new { Message = "هذا الصنف موجود سلفا", Title = "تنبيه", Status = "warning" });
        }

        public ActionResult Edit(Item data)
        {
            if (data.Id != 0 && data.Name != null)
            {
                //int c = db.Items.Where(f => f.Id != data.Id && f.Name == data.Name).Count();
                if (!db.Items.Any(f => f.Id != data.Id && f.Name == data.Name))
                
                    //if (c == 0)
                {
                    Item t = db.Items.Find(data.Id);
                    // TeacherMaterial tm = db.TeacherMaterials.Single(f => f.TeacherId == data.Id && f.IsSpecialtyMaterial == true);

                    t.Name = data.Name;


                    db.Entry(t).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success" });
                }
                return Json(new { Message = "هذا الصنف موجود سلفا", Title = "تنبيه", Status = "warning" });
            }

            return Json(new { Message = "حدث خطأ اثناء التعديل", Title = "خطأ", Status = "error" });
        }




        public ActionResult LoadDataItemDetail(int? id)
        {
            var datad = db.ItemDetails.Select(p => new
            {
                Id = p.Id,
                Name = p.Name,
                ItemId = p.ItemId
            }).Where(p => p.ItemId == id);
            return Json(datad, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult saveItemDetails(ItemDetail data)
        {
            if(!db.ItemDetails.Any(f=>f.Name == data.Name))
            {
                ItemDetail t = new ItemDetail();
                t.Name = data.Name;
                t.ItemId = data.Id;
                db.ItemDetails.Add(t);
                db.SaveChanges();
                return Json(new { Message = "تمت عملية الاضافة بنجاح", Title = "نجاح", Status = "success" });
            }
            return Json(new { Message = "هذا العنصر موجود سلفا", Title = "تنبيه", Status = "warning" });
        }

        public ActionResult EditItemDetails(ItemDetail data)
        {
            if (data.Id != 0 && data.Name != null)
            {
                if(!db.ItemDetails.Any(f => f.Id != data.Id && f.Name == data.Name))
                {
                    ItemDetail h = db.ItemDetails.Find(data.Id);
                    h.Name = data.Name;
                    db.Entry(h).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "نجاح", Status = "success", JsonRequestBehavior.AllowGet });
                }
            }
            return Json(new { Message = "هذا العنصر موجود سلفا", Title = "تنبيه", Status = "warning" });
        }



    }
    }