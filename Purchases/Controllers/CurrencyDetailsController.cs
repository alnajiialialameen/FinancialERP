using Purchases.Models;
using Purchases.MyLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace Purchases.Controllers
{
    public class CurrencyDetailsController : Controller
    {
        private Entities db = new Entities();
        private TreeClass trcls = new TreeClass();

        // GET: CurrencyDetails
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult LoadData()
        {
            List<object> data = new List<object>();
            foreach (var c in db.CurrencyDetails.ToList())
            {
                var item = new
                {
                    Id = c.Id,
                    Year = c.Year,
                    Month = c.Month,
                    ExchangeRate = c.ExchangeRate,
                    CurrencyTypeId = c.CurrencyTypeId,
                    CurrencyType = c.CurrencyType.Name,
                };

                data.Add(item);
            }
            
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost]
        public ActionResult Create(int CurrencyTypeId, decimal ExchangeRate, DateTime ExchangeDate)
        {
            try
            {
                if (!db.CurrencyTypes.Any(x => x.Id == CurrencyTypeId && x.IsLocalCurrency == true))
                {
                    if (!db.CurrencyDetails.Any(x => x.CurrencyTypeId == CurrencyTypeId && x.Month == ExchangeDate.Month && x.Year == ExchangeDate.Year))
                    {
                        CurrencyDetail obj = new CurrencyDetail();

                        obj.CurrencyTypeId = CurrencyTypeId;
                        obj.ExchangeRate = ExchangeRate;
                        obj.Month = ExchangeDate.Month;
                        obj.Year = ExchangeDate.Year;
                        obj.IsActive = true;

                        db.CurrencyDetails.Add(obj);
                        db.SaveChanges();

                        return Json(new { Message = " تمت عملية الحفظ بنجاح ", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Message = "يوجد سعر صرف لهذه العملة في هذا الشهر...يمكنك التعديل فقط", Title = "خطأ", Status = "warning" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Message = "لا يمكن تحديد سعر الصرف للعملة المحلية", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
                }
            }catch(Exception e)
            {
                return Json(new { Message = " عفوا حدث خطأ اثناء العملية (Exception)", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult Update(int Id, int CurrencyTypeId, decimal ExchangeRate, DateTime ExchangeDate)
        {
            try
            {
                if (!db.CurrencyTypes.Any(x => x.Id == CurrencyTypeId && x.IsLocalCurrency == true))
                {
                    if (db.CurrencyDetails.Any(x => x.Id == Id))
                    {
                        CurrencyDetail obj = db.CurrencyDetails.Find(Id);

                        obj.CurrencyTypeId = CurrencyTypeId;
                        obj.ExchangeRate = ExchangeRate;
                        obj.Month = ExchangeDate.Month;
                        obj.Year = ExchangeDate.Year;
                        obj.IsActive = true;

                        db.Entry(obj).State = EntityState.Modified;
                        db.SaveChanges();

                        return Json(new { Message = " تمت عملية الحفظ بنجاح ", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { Message = "حدث خطا اثناء عملية التعديل", Title = "خطأ", Status = "warning" }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json(new { Message = "لا يمكن تحديد سعر الصرف للعملة المحلية", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = " عفوا حدث خطأ اثناء العملية (Exception)", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult Delete(int Id)
        {
            try
            {
                if(db.CurrencyDetails.Any(x=>x.Id == Id))
                {
                    CurrencyDetail Obj =  db.CurrencyDetails.Find(Id);

                    db.CurrencyDetails.Remove(Obj);
                    db.SaveChanges();

                    return Json(new { Message = " تمت عملية الحذف بنجاح ", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Message = "لا يمكن حذف بيانات غير موجودة", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                return Json(new { Message = " عفوا حدث خطأ اثناء العملية (Exception) ", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}