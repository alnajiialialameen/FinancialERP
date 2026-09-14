 using Purchases.MyLogic;
using PagedList;
using Purchases.Models;
using Purchases.Models.ViewModal;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Purchases.Models.ViewModel;

namespace Purchases.Controllers
{
    public class ACUCompetingCompanyController : Controller
    {
        private static Entities db = new Entities();
        
        // GET: CompetingCompany
        public ActionResult Index()
        {
            var data = db.CompetingCompanies.ToList();
            return View();
        }

        public ActionResult getAllCompanies()
        {
            List<CompetingCompanyVM> employeeList = new List<CompetingCompanyVM>();
            foreach (var item in db.AccountTrees.Where(x=>x.AccParent == 46 & !db.CompanyRegisterations.Any(c=>c.AccountTreeId == x.Id)).ToList())
            {
                employeeList.Add(
                    new CompetingCompanyVM()
                    {
                        Id = item.Id,
                        Name = item.AccName,
                        AccountTreeId = item.Id,
                        Phone1 = "Not Found",
                        Phone2 = "Not Found",
                        Email = "Not Found",
                        Address = "Not Found",
                        StatusText = "Not Found",
                        LicenseNumber = "Not Found",
                    });
            }
            foreach(var item in db.CompanyRegisterations.ToList())
            {
                employeeList.Add(
                    new CompetingCompanyVM()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Phone1 = item.Phone1,
                        Phone2 = item.Phone2,
                        Email = item.Email,
                        Address = item.Address,
                        AccountTreeId = item.AccountTreeId,
                        StatusText = item.Status == 1?"مفعلة":"غير مفعلة",
                        LicenseNumber = item.LicenseNumber,
                    });
            }

            return Json(employeeList, JsonRequestBehavior.AllowGet);
        }


        public ActionResult getAllCompaniesOld()
        {
            List<CompetingCompanyVM> employeeList = new List<CompetingCompanyVM>();
            foreach (var item in db.CompanyRegisterations.ToList())
            {
                employeeList.Add(
                    new CompetingCompanyVM()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Phone1 = item.Phone1,
                        Phone2 = item.Phone2,
                        Email = item.Email,
                        Address = item.Address,
                        StatusText = item.Status == 1 ? "مفعل" : "غير مفعل",
                        LicenseNumber = item.LicenseNumber,
                    });
            }

            return Json(employeeList, JsonRequestBehavior.AllowGet);
        }


        //دي متعلقة 
        public ActionResult getAllCompaniesInOrder(int Id)
        {
            List<CompetingCompanyVM> employeeList = new List<CompetingCompanyVM>();
            foreach (var item in db.CompetingCompanies.Where(x=>x.OrderId == Id).ToList())
            {
                employeeList.Add(
                    new CompetingCompanyVM()
                    {
                        Id = item.Id,
                        CompanyId = item.CompanyRegisteration.Id,
                        Name = item.CompanyRegisteration.Name,
                        Phone1 = item.CompanyRegisteration.Phone1,
                        Phone2 = item.CompanyRegisteration.Phone2,
                        Email = item.CompanyRegisteration.Email,
                        Address = item.CompanyRegisteration.Address,
                        StatusText = item.CompanyRegisteration.Status == 1 ? "مفعل" : "غير مفعل",
                        LicenseNumber = item.CompanyRegisteration.LicenseNumber,
                    });
            }

            return Json(employeeList, JsonRequestBehavior.AllowGet);
        }


        public ActionResult getCompanyById(int Id)
        {
            CompetingCompanyVM obj = new CompetingCompanyVM();

            if (db.CompanyRegisterations.Any(x=>x.Id==Id))
            {
                CompanyRegisteration item = db.CompanyRegisterations.Find(Id);
                obj.Id = item.Id;
                obj.Name = item.Name;
                obj.Phone1 = item.Phone1;
                obj.Phone2 = item.Phone2;
                obj.Email = item.Email;
                obj.Address = item.Address;
                obj.StatusText = item.Status == 1 ? "مفعل" : "غير مفعل";
                obj.LicenseNumber = item.LicenseNumber;
                    
            }

            return Json(obj, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Add(CompetingCompanyVM model)
        {
            var userid = User.Identity.GetUserId();

            try
            {
                TreeClass ClsTree = new TreeClass();
                if (!db.CompanyRegisterations.Any(x => x.AccountTreeId == model.AccountTreeId))
                {
                    int AccountTreeId = ClsTree.AddToTree("شركات العطاءات", model.Name, 2, 4, userid);

                    CompanyRegisteration obj = new CompanyRegisteration();
                    obj.Name = model.Name;
                    obj.Phone1 = model.Phone1;
                    obj.Phone2 = model.Phone2;
                    obj.Address = model.Address;
                    obj.Email = model.Email;
                    obj.Status = model.Status;
                    obj.AccountTreeId = AccountTreeId;
                    obj.IsAgentCompany = model.IsAgentComapny;
                    obj.IsQualified = model.IsQualified;
                    obj.LicenseNumber = model.LicenseNumber;
                    obj.CreatedBy = userid;
                    obj.CreationDate = DateTime.Now;

                    db.CompanyRegisterations.Add(obj);
                    db.SaveChanges();

                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    CompanyRegisteration obj = db.CompanyRegisterations.FirstOrDefault(x=>x.AccountTreeId == model.AccountTreeId);

                    obj.Name = model.Name;
                    obj.Phone1 = model.Phone1;
                    obj.Phone2 = model.Phone2;
                    obj.Address = model.Address;
                    obj.Email = model.Email;
                    obj.Status = model.Status;
                    obj.IsAgentCompany = model.IsAgentComapny;
                    obj.IsQualified = model.IsQualified;
                    obj.LicenseNumber = model.LicenseNumber;

                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية التعديل بنجاح", Title = "تنبيه", Status = "warning" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }

        [HttpPost]
        public ActionResult AddOld(CompetingCompanyVM model)
        {
            var userid = User.Identity.GetUserId();

            try
            {
                TreeClass ClsTree = new TreeClass();
                if (!db.CompanyRegisterations.Any(x => x.LicenseNumber == model.LicenseNumber))
                {
                    int AccountTreeId = ClsTree.AddToTree("شركات العطاءات", model.Name, 2, 4, userid);

                    CompanyRegisteration obj = new CompanyRegisteration();
                    obj.Name = model.Name;
                    obj.Phone1 = model.Phone1;
                    obj.Phone2 = model.Phone2;
                    obj.Address = model.Address;
                    obj.Email = model.Email;
                    obj.Status = model.Status;
                    obj.AccountTreeId = AccountTreeId;
                    obj.IsAgentCompany = model.IsAgentComapny;
                    obj.IsQualified = model.IsQualified;
                    obj.LicenseNumber = model.LicenseNumber;

                    db.CompanyRegisterations.Add(obj);
                    db.SaveChanges();

                    return Json(new { Message = "تمت عملية الإضافة  بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "عفوا تم إدخال بيانات رقم الترخيص من قبل", Title = "تنبيه", Status = "warning" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء عملية الإضافة", Title = "خطأ", Status = "error" });
            }
        }

        [ActionName("UpdateCompany")]
        [HttpPost]
        public ActionResult UpdateCompetingCompany(CompetingCompanyVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                if (db.CompanyRegisterations.Any(x => x.Id == model.Id))
                {
                    CompanyRegisteration obj = db.CompanyRegisterations.Find(model.Id);
                    
                    obj.Name = model.Name;
                    obj.Phone1 = model.Phone1;
                    obj.Phone2 = model.Phone2;
                    obj.Address = model.Address;
                    obj.Email = model.Email;
                    obj.Status = model.Status;
                    obj.LicenseNumber = model.LicenseNumber;
                    obj.IsAgentCompany = model.IsAgentComapny;
                    obj.IsQualified = model.IsQualified;
                    obj.UpdatedBy = userid;
                    obj.UpdatingDate = DateTime.Now;

                    AccountTree treeObj = db.AccountTrees.Find(obj.AccountTreeId);
                    treeObj.AccName = model.Name;

                    db.Entry(obj).State = EntityState.Modified;
                    db.Entry(treeObj).State = EntityState.Modified;
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

        [HttpPost]
        public ActionResult Delete(int Id)
        {
            try
            {
                if (db.CompanyRegisterations.Any(x => x.Id == Id))
                {
                   var obj = db.CompanyRegisterations.Find(Id);

                    obj.Status = 0;

                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { Message = "تمت عملية الحذف بنجاح ", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "حدث خطأ أثناء العملية ", Title = "خطأ", Status = "error" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء العملية ", Title = "خطأ", Status = "error" });
            }
        }

        /****************************************************************************************/
        public ActionResult CompanyDocument(int Id, int? page)
        {
            int pageSize = 9;
            CompanyRegisteration obj = db.CompanyRegisterations.Find(Id);
            ViewBag.CompanyId = Id;

            ViewBag.CompanyName = obj.Name;
            ViewBag.Phones = obj.Phone1 + " - " + obj.Phone2;
            ViewBag.Email = obj.Email;
            ViewBag.Address = obj.Address;
            ViewBag.LicenseNumber = obj.LicenseNumber;
            ViewBag.Status = obj.Status == 1 ? "الشركة / المؤسسة ما زالت موجودة في سوق العمل وتعمل حاليا يرجي مراجعة بياناتها بالتواصل المباشر او عن طريق المناديب" :
                "الشركة / المؤسسة غير موجودة في سوق العمل وبالتالي هي خارج اطار المنافسة في جميع المناقصات والعطاءات المطروحة حاليا او التي ستطرح مستقبلا";
            
            List<CompanyDocumentVM> data = new List<CompanyDocumentVM>();
            if (db.CompanyDocuments.Any(x => x.CompanyRegisterationId == Id))
            {
                data = db.CompanyDocuments.Where(x => x.CompanyRegisterationId == Id)
                            .Select(p => new CompanyDocumentVM()
                            {
                                Id = p.Id,
                                ImagePath = p.ImagePath,
                                Title = p.Title,
                                Note = p.Note,
                                CompetingCompanyId = Id,
                            }).ToList();
            }

            @ViewBag.ItemCount = data.Count();
            return View(data.ToPagedList(page??1, pageSize));
        }

        //public ActionResult CompanyDocument(int Id, int? page)
        //{
        //    CompanyRegisteration obj = db.CompanyRegisterations.Find(Id);
        //    ViewBag.CompanyId = Id;

        //    @ViewBag.CompanyName = obj.Name;
        //    @ViewBag.Phones = obj.Phone1 + " - " + obj.Phone2;
        //    @ViewBag.Email = obj.Email;
        //    @ViewBag.Address = obj.Address;
        //    @ViewBag.LicenseNumber = obj.LicenseNumber;
        //    @ViewBag.Status = obj.Status == 1? "الشركة / المؤسسة ما زالت موجودة في سوق العمل وتعمل حاليا يرجي مراجعة بياناتها بالتواصل المباشر او عن طريق المناديب" :
        //        "الشركة / المؤسسة غير موجودة في سوق العمل وبالتالي هي خارج اطار المنافسة في جميع المناقصات والعطاءات المطروحة حاليا او التي ستطرح مستقبلا";

        //    List<CompanyDocumentVM> data = new List<CompanyDocumentVM>();
        //    if (db.CompanyDocuments.Any(x => x.CompetingCompanyId == Id))
        //    {
        //        data = db.CompanyDocuments.Where(x => x.CompetingCompanyId == Id)
        //                    .Select(p => new CompanyDocumentVM()
        //                    {
        //                        Id = p.Id,
        //                        ImagePath = p.ImagePath,
        //                        Title = p.Title,
        //                        Note = p.Note,
        //                        CompetingCompanyId = Id,
        //                    }).ToList();
        //    }
        //    int pageSize = 9;
        //    int PageNumber = (page ?? 1);

        //    return View(data.ToPagedList(PageNumber, pageSize));
        //}

        //public ActionResult GetCompanyDocument(int? page, int? Get, int? Search, int? Count, int? Id)
        //{
        //    List<CompanyDocumentVM> data = new List<CompanyDocumentVM>();
        //    if (db.CompanyDocuments.Any(x => x.CompetingCompanyId == Id))
        //    {
        //        data = db.CompanyDocuments.Where(x => x.CompetingCompanyId == Id)
        //                    .Select(p => new CompanyDocumentVM()
        //                    {
        //                        Id = p.Id,
        //                        ImagePath = p.ImagePath,
        //                        Title = p.Title,
        //                        Note = p.Note,
        //                        CompetingCompanyId = Id,
        //                    }).ToList();
        //    }
        //    int pageSize = 9;
        //    int PageNumber = (page ?? 1);
        //    return PartialView("_GetCompanyDocument", data.ToPagedList(PageNumber, pageSize));
        //}

        public ActionResult _GetCompanyDocument()
        {
            return PartialView();
        }

        [HttpPost]
        [ActionName("DeleteDocument")]
        public ActionResult DeleteDoc(int Id, string Title, int CompanyId)
        {
            try
            {
                if (db.CompanyDocuments.Any(x => x.Id == Id))
                {
                    CompanyDocument obj = db.CompanyDocuments.Find(Id);

                    db.CompanyDocuments.Remove(obj);
                    db.SaveChanges();

                    myExtention.DeleteUploadedFile(CompanyId, Title);
                    //هنا مفترض برضه نمسح الملف نفسه من السيرفر
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

        [HttpPost]
        [ActionName("AddDocument")]
        public ActionResult AddCompanyDocument(CompanyDocumentVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                string FilePath = myExtention.UploadFile(model.CompetingCompanyId, model.Title);

                CompanyDocument obj = new CompanyDocument();

                obj.Title = model.Title;
                obj.ImagePath = FilePath;
                obj.CompanyRegisterationId = model.CompetingCompanyId;
                obj.Note = model.Note;
                obj.CreatedBy = userid;
                obj.CreationDate = DateTime.Now;
                    
                db.CompanyDocuments.Add(obj);
                db.SaveChanges();

                if (FilePath != null)
                {
                    UpdateImagePath(obj.Id, FilePath);
                }
                
                return Json(new { Message = "تمت العملية بنجاح", Title = "نجاح", Status = "success" });
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء العملية ", Title = "خطأ", Status = "error" });
            }
        }

        [HttpPost]
        [ActionName("UpdateDocument")]
        public ActionResult UpdateCompanyDocument(CompanyDocumentVM model)
        {
            try
            {
                var userid = User.Identity.GetUserId();

                if (db.CompanyDocuments.Any(x => x.Id == model.Id))
                {
                    string FilePath = myExtention.UploadFile(model.CompetingCompanyId, model.Title);
                    if (FilePath != null)
                    {
                        //UpdateImagePath(obj.Id, FilePath);
                        model.ImagePath = FilePath;
                    }

                    CompanyDocument obj = db.CompanyDocuments.Find(model.Id);
                    
                    obj.Title = model.Title;
                    obj.ImagePath = model.ImagePath;                    
                    obj.Note = model.Note;
                    obj.CreatedBy = userid;
                    obj.CreationDate = DateTime.Now;

                    db.Entry(obj).State = EntityState.Modified;
                    db.SaveChanges();
                    //هنا مفترض نحدث مسار الملف في السيرفر
                    return Json(new { Message = "تمت العملية بنجاح", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = "تم إدخال هذا المستند من قبل", Title = "تنبيه", Status = "warning" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء العملية ", Title = "خطأ", Status = "error" });
            }
        }

        [HttpGet]
        [ActionName("getDocument")]
        public ActionResult GetDocumentForUpdate(int Id)
        {
            try
            {
                if (db.CompanyDocuments.Any(x => x.Id == Id))
                {
                    CompanyDocument obj = db.CompanyDocuments.Find(Id);
                    CompanyDocumentVM dto = new CompanyDocumentVM();
                    dto.Id = Id;
                    dto.ImagePath = obj.ImagePath;
                    dto.Title = obj.Title;
                    dto.Note = obj.Note;
                    dto.CompetingCompanyId = obj.CompanyRegisterationId;
                    
                    //هنا مفترض برضه نمسح الملف نفسه من السيرفر
                    return Json(dto, JsonRequestBehavior.AllowGet);
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

        /*-----------------------------------------------------------------------------*/
        [HttpPost]
        public ActionResult UploadPdfFileOriginal()
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

                            Order o = new Order();
                            o.DepartmentId = Convert.ToInt32(HttpContext.Request.Form["DepartmentId"]);
                            o.DepartmentName = HttpContext.Request.Form["DepartmentName"];
                            o.Description = HttpContext.Request.Form["Description"];
                            o.OrderDate = DateTime.Now; //Convert.ToDateTime(HttpContext.Request.Form["OrderDate"]);
                            o.CreatedDate = DateTime.Now;
                            o.SuggestPrice = Convert.ToDecimal(HttpContext.Request.Form["SuggestPrice"]);
                            o.OrderTypeId = 1;
                            o.CreatedBy = userid;
                            o.CreatedDate = DateTime.Now;

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

        public int UpdateImagePath(int Id, string profileImagePath)
        {
            int res = 0;
            try
            {
                var userid = User.Identity.GetUserId();

                CompanyDocument obj = db.CompanyDocuments.Find(Id);

                obj.ImagePath = profileImagePath;
                obj.UpdatedBy = userid;
                obj.UpdatingDate = DateTime.Now;

                db.Entry(obj).State = EntityState.Modified;
                res = db.SaveChanges();
            }
            catch (Exception ex)
            {
                res = -1;
            }

            return res;
        }

    }
}