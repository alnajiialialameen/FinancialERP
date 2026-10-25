using FuzzySharp.Utils;
using Microsoft.AspNet.Identity;
using Purchases.Models;
using Purchases.Models.ViewModal;
using Purchases.Models.ViewModel;
using Purchases.MyLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class EmpPettyCashController : Controller
    {
        private EmployeePettyCashCustodyObj obj = new EmployeePettyCashCustodyObj();
        private ConsumeHRAPI hrAPI = new ConsumeHRAPI();
        private Entities db = new Entities();
        private SharedClass sh = new SharedClass();

        // GET: EmpPettyCash
        public ActionResult Index()
        {
            ViewBag.GetPettyCachItems = obj.GetPettyCachItems();

            return View();
        }

        public ActionResult LoadDataFromHrAPI()
        {
            var hrApiData = hrAPI.getAllEmployees();

            if (hrApiData == null)
                return Json(new { Message = "حدث خطأ في عملية جلب بيانات الموظفين", Title = "خطأ", Status = "error" }
                                        , JsonRequestBehavior.AllowGet);

            var data = hrApiData.Where(q => q.EmploymetTypeId == 1).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult LoadData(int id)
        {
            var userId = User.Identity.GetUserId();
            var myData = obj.LoadData(userId, id);

            if (myData == null)
                return Json(new { Message = "حدث خطأ في عملية جلب بيانات الموظفين", Title = "خطأ", Status = "error" }
                                        , JsonRequestBehavior.AllowGet);

            return Json(myData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveData(EmployeePettyCashCustodyVM data)
        {
            var userId = User.Identity.GetUserId();
            var year = sh.GetUserCurrentFinancialCycleYear(userId);
            int myYear = Convert.ToInt32(year);

            var deductionIds = db.AccountTreeHrDeductions.Where(q=> q.AccTreeId == data.AccTreeId)
                .Select(q=> q.DeductionIds).FirstOrDefault();

            List<int> newList = string.IsNullOrWhiteSpace(deductionIds)? new List<int>()
                                                     : deductionIds
                                                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(id => int.Parse(id.Trim()))
                                                        .ToList();

            decimal? TotalDeductions = hrAPI.GetEmployeeTotalDeductions(data.EmployeeId ?? 0, newList, myYear);

            data.PaiedAmount = TotalDeductions;

            var res = obj.SaveDataSingleRow(data, userId);

            if (res > 0)
                return Json(new { Message = "تمت العملية بنجاح", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { Message = "حدث خطأ في عملية الحفظ", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
        }

    }
}