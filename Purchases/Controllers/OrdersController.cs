using Purchases.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Purchases.Models.ViewModal;
using System.IO;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;

namespace Purchases.Controllers
{
    public class OrdersController : Controller
    {
        private Entities db = new Entities();
        // GET: Orders;
        public ActionResult Index()
        {
            //القيمة الافتراضية للضريبة
            ViewBag.Vat = db.Vats.Find(1).Value;
            return View();
        }

        public ActionResult Index2()
        {
            //القيمة الافتراضية للضريبة
            ViewBag.Vat = db.Vats.Find(1).Value;
            return View();
        }

        public ActionResult LoadData()
        {
            var data = db.Orders.Select(p => new
            {
                Id = p.Id,
                Department = p.DepartmentName,
                DepartmentId = p.DepartmentId,
                OrderDate = p.OrderDate.Value.Day + "/" + p.OrderDate.Value.Month + "/" + p.OrderDate.Value.Year,
                Description = p.Description,
                OrderTypeId = p.OrderTypeId ?? 1,
                IsCircued = db.Circus.Any(x => x.OrderId == p.Id) ? "مسرك" : "غير مسرك"

            }).Where(f=>f.OrderTypeId == 1);
           
            return Json(data,  JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadDataForAuction()
        {
            var data = db.Orders.Select(p => new
            {
                Id = p.Id,
                Department = p.DepartmentName,
                DepartmentId = p.DepartmentId,
                OrderDate = p.OrderDate.Value.Day + "/" + p.OrderDate.Value.Month + "/" + p.OrderDate.Value.Year,
                Description = p.Description,
                OrderTypeId = p.OrderTypeId ?? 1

            }).Where(f => f.OrderTypeId == 2);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetItem(string q)
        {
            var data = db.Items.Select(p => new
            {
                id = p.Id,
                text = p.Name
            }).Where(f => f.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetItemDetail(string q ,int fk)
        {
            var data = db.ItemDetails.Select(p => new
            {
                id = p.Id,
                text = p.Name,
                itemid = p.ItemId
            }).Where(f=>f.text.Contains(q) & f.itemid == fk);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult GetSupplier(string q)
        {
            var data = db.Suppliers.Select(p => new
            {
                id = p.Id,
                text = p.Name

            }).Where(f => f.text.Contains(q));
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        //تسجيل طلب جديد 
        [HttpPost]
        public ActionResult Create()
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                var userid = User.Identity.GetUserId();

                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;

                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            return Json(new { Message = " الرجاء تغير المتصفح", Title = "خطأ", Status = "error" });
                        }
                        else
                        {
                        //    var format = "dd/mm/yyyy";
                        //    var dateconver = new IsoDateTimeConverter { DateTimeFormat = format };
                        //    JsonConvert.DeserializeObject<object>(HttpContext.Request.Form["OrderDate"], dateconver);

                            Order o = new Order();
                            o.DepartmentId = Convert.ToInt32(HttpContext.Request.Form["DepartmentId"]);
                            o.DepartmentName = HttpContext.Request.Form["DepartmentName"];
                            o.Description = HttpContext.Request.Form["Description"];
                            o.OrderDate =  Convert.ToDateTime(HttpContext.Request.Form["OrderDate"]);
                            o.CreatedDate = DateTime.Now;
                            o.OrderTypeId = 1;
                            o.CreatedBy = userid;
                            o.CreatedDate = DateTime.Now;

                            //o.SuggestPrice = Convert.ToDecimal(HttpContext.Request.Form["SuggestPrice"]);
                            db.Orders.Add(o);

                            fname = o.Id + Path.GetExtension(file.FileName);
                            OrderImage oi = new OrderImage();
                            oi.OrderId = o.Id;
                            oi.Path = Path.Combine(Server.MapPath("~/OrderImage/"), fname);
                            oi.CreatedBy = userid;
                            oi.CreationDate = DateTime.Now;
                            db.OrderImages.Add(oi);
                            
                            var ItemId = HttpContext.Request.Form["ItemId"].Split(',');
                            var ItemDetailId = HttpContext.Request.Form["ItemDetailId"].Split(',');
                            var RequierCount = HttpContext.Request.Form["RequierCount"].Split(',');

                            List<OrderDetiail> odlist = new List<OrderDetiail>();
                            OrderDetiail orderdetail = new OrderDetiail();

                            for (int x = 0; x <= ItemId.Length - 1; x++)
                            {
                                if (ItemId[x] != "0" & ItemDetailId[x] != "0" & RequierCount[x] != "0")
                                {
                                    OrderDetiail od = new OrderDetiail();

                                    od.OrderId = o.Id;
                                    od.ItemId = Convert.ToInt32(ItemId[i]);
                                    od.ItemDetialId = Convert.ToInt32(ItemDetailId[i]);
                                    od.RequierCount = Convert.ToInt32(RequierCount[i]);
                                    od.CreatedBy = userid;
                                    od.CreationDate = DateTime.Now;
                                    odlist.Add(od);
                                }//--if end

                            }//-- for end

                            db.OrderDetiails.AddRange(odlist);

                        }

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/OrderImage/"), fname);
                        file.SaveAs(fname);
                        db.SaveChanges();
                    }
                    // Returns message that successfully uploaded  
                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
                catch (Exception ex)
                {
                    return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
                }
            }
            else
            {
                return Json(new { Message = "لم يتم إختيار ملف", Title = "خطأ", Status = "error" });
            }


        }

        public ActionResult CreateNewInvoice(int OrderId , int SupplierId)
        {
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                var userid = User.Identity.GetUserId();

                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    
                    for (int i = 0; i < files.Count ; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  


                        //string path = "E/ASD/";

                        HttpPostedFileBase file = files[i];
                        string fname;

                        //if (Path.GetExtension(file.FileName) == ".pdf")
                        //{

                        //}
                        //else { }



                            // Checking for Internet Explorer  
                            if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            //string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            //fname = testfiles[testfiles.Length - 1];
                            //fname = file.FileName;
                            return Json(new { Message = " الرجاء تغير المتصفح", Title = "خطأ", Status = "error" });
                        }
                        else
                        {
                            fname = OrderId.ToString() + SupplierId.ToString() + Path.GetExtension(file.FileName);
                            //التاكد لا فاتورة لنفس المورد لنفس الطلب 
                            if (!db.InvoiceImages.Any(f => f.OrderId == OrderId & f.SupplierId == SupplierId))
                            {
                                InvoiceImage ii = new InvoiceImage();
                                ii.OrderId = OrderId;
                                ii.SupplierId = SupplierId;
                                ii.CreatedBy = userid;
                                ii.CreationDate = DateTime.Now;
                                ii.Path = Path.Combine(Server.MapPath("~/InvoiceImage/"), fname);
                                db.InvoiceImages.Add(ii);
                            }
                            else
                            {
                                InvoiceImage fd = db.InvoiceImages.FirstOrDefault(z => z.OrderId == OrderId & z.SupplierId == SupplierId);
                                fd.OrderId = OrderId;
                                fd.SupplierId = SupplierId;
                                fd.UpdatedBy = userid;
                                fd.UpdatingDate = DateTime.Now;
                                fd.Path = Path.Combine(Server.MapPath("~/InvoiceImage/"), fname);
                                db.Entry(fd).State = EntityState.Modified;
                            }
                        }

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/InvoiceImage/"), fname);
                        file.SaveAs(fname);
                        db.SaveChanges();
                    }
                    // Returns message that successfully uploaded  
                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
                catch (Exception ex)
                {
                    return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
                }
            }
            else
            {
                return Json(new { Message = "لم يتم إختيار ملف", Title = "خطأ", Status = "error" });
            }
        }

        public ActionResult GetInvoiceOrder(int orderid)
        {
            var data = db.InvoiceImages.Select(p => new
            {
                Id = p.Id,
                OrderId = p.OrderId,
                SupplierId = p.SupplierId,
                Supplier = p.Supplier.Name,
                Path = p.Path,
                IsChoosed = p.IsChoosed

            }).Where(f=>f.OrderId == orderid);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        public ActionResult viewOrderImage(int id)
        {
            string path = db.OrderImages.FirstOrDefault(f => f.OrderId == id).Path;

            string ReportURL = path;
            byte[] FileBytes = System.IO.File.ReadAllBytes(ReportURL);
            return File(FileBytes, "application/pdf");
        }

        public FileResult GetReport(int orderid ,int supplierid)
        {
            string path  = db.InvoiceImages.FirstOrDefault(f => f.OrderId == orderid & f.SupplierId == supplierid).Path;

            string ReportURL = path;
            byte[] FileBytes = System.IO.File.ReadAllBytes(ReportURL);
            return File(FileBytes, "application/pdf");
        }
        
        public ActionResult CheckInvoice(int orderid)
        {
            if(!db.InvoiceImages.Any(f=>f.OrderId == orderid & f.IsChoosed == true))
            {
                return Json(new { Status = "success" });
            }
            return Json(new { Status = "error" });
        }

        //عرض تفاصيل الفاتورة لاجراء التكملة
        public ActionResult GetOrderDetail(int orderid)
        {
            var data = db.OrderDetiails.Select(p => new
            {
                OrderDetailId = p.Id,
                OrderId = p.OrderId,
                RequierCount = p.RequierCount,
                ItemDetailId = p.ItemDetialId,
                ItemDetial = p.ItemDetail.Name,
            }).Where(f=>f.OrderId == orderid);
            
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //تكملة بيانات الفاتورة بعد الاختيار
        public ActionResult InvoiceChoosed(OrderVM data , int orderid , int supplierid)
        {
            List<OrderDetiail> odlist = new List<OrderDetiail>();
            var userid = User.Identity.GetUserId();

            for (int i = 0; i <= data.OrderDetailId.Length - 1; i++)
            {
                if (data.ItemPrice[i] != 0)
                {

                    int Id = data.OrderDetailId[i];
                    OrderDetiail od = db.OrderDetiails.Find(Id);

                    od.ItemPrice = data.ItemPrice[i];
                    od.Price = data.ItemPrice[i] * od.RequierCount;
                    od.UpdatedBy = userid;
                    od.UpdatingDate = DateTime.Now;

                    odlist.Add(od);
                    db.Entry(od).State = EntityState.Modified;
                } else
                {
                    return Json(new { Message = " الرجاء  إدخال كل القيم", Title = "خطأ", Status = "error" });
                }
            }

            List<InvoiceImage> ini = db.InvoiceImages.Where(f => f.OrderId == orderid).ToList();
            
            foreach(var i in ini)
            {
                i.IsChoosed = null;
                db.Entry(i).State = EntityState.Modified;
            }


            InvoiceImage ii = db.InvoiceImages.Single(f => f.OrderId == orderid & f.SupplierId == supplierid);
            ii.IsChoosed = true;
            db.Entry(ii).State = EntityState.Modified;

            Vat vat = db.Vats.FirstOrDefault(f => f.Value == data.VatType);
            Order o = db.Orders.Find(orderid);
            o.SupplierId = supplierid;
            o.CurrencyTypeId = data.CurrencyType;
            o.VatId = vat.Id;
            o.ActualPrice = odlist.Sum(f => f.Price);
            o.Vat = odlist.Sum(f => f.Price) * data.VatType/100;
            o.PriceWithVat = odlist.Sum(f => f.Price) + (odlist.Sum(f => f.Price) * data.VatType / 100);
            o.UpdatedBy = userid;
            o.UpdatingDate = DateTime.Now;
            //o.CurrencyTypeId = data.CurrencyType;

            db.Entry(o).State = EntityState.Modified;

            db.SaveChanges();
            return Json(new { Message = "  تم تكملة البيانات بنجاح ", Title = "نجاح", Status = "success" });
        }



        ///**********************  ***  **************************///
        /// 



        //public ActionResult FinaicalRequest(int orderid)
        //{

        //    ReportDocument rd = new ReportDocument();
        //    rd.Load(Path.Combine(Server.MapPath("~/Report/Order/FinaicalRequest.rpt")));
        //    IQueryable<Order> Order;
        //    Order = db.Orders;
        //    //IQueryable<Employee> Employee;
        //    //Employee = db.Employees;

        //    BaseClass b = new BaseClass();
        //    decimal o = db.Orders.FirstOrDefault(f => f.Id == orderid).ActualPrice??0;
        //    string total = b.ChangeNumberToText(o.ToString(), 0);
        //    rd.SetDataSource(Order.Select(p => new
        //    {
        //        Id = p.Id,
        //        Body = "بالاشارة للموضوع اعلاه نرجو من سيادتكم التكرم بسداد مبلغ وقدره " + p.ActualPrice.Value + "("  +total + ")"
        //        + "عبارة عن " + p.Description 
        //    }).Where(f=>f.Id == orderid).ToList());


        //    Response.Buffer = false;
        //    Response.ClearContent();
        //    Response.ClearHeaders();
        //    Stream Stream = rd.ExportToStream
        //        //WordForWindows
        //        (CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //    Stream.Seek(0, SeekOrigin.Begin);//application/doc

        //    rd.Close();
        //    rd.Dispose();
        //    GC.Collect();

        //    return File(Stream, "application/pdf");
        //}
    }
}