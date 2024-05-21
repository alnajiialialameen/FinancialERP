using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net.Http;
using Purchases.Models;
using Purchases.MyLogic;
using Purchases.Models.ViewModal;

namespace Purchases.Controllers
{
    public class ConsumeHrApiController : Controller
    {
        // GET: ConsumeHrApi
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult GetDepartment(string q)
        {
            try
            {
                //http://localhost:57677/api/
                List<DepartmentVM> department = null;
                HttpClient HC = new HttpClient();
                HC.BaseAddress = new Uri(Constant.BaseAPIURL);
                var ConsumeApi = HC.GetAsync("Departments");
                ConsumeApi.Wait();
                var ReadData = ConsumeApi.Result;
                if (ReadData.IsSuccessStatusCode)
                {
                    var DisplayRecord = ReadData.Content.ReadAsAsync<List<DepartmentVM>>();
                    DisplayRecord.Wait();
                    department = DisplayRecord.Result;
                }


                var data = department.Select(p => new
                {
                    id = p.Id,
                    text = p.Name
                }).Where(f => f.text.Contains(q));

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}