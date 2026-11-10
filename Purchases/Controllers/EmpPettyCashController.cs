using FuzzySharp.Utils;
using Microsoft.AspNet.Identity;
using Purchases.Models;
using Purchases.Models.ViewModal;
using Purchases.Models.ViewModel;
using Purchases.MyLogic;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
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

        public ActionResult GetPettyCachItems()
        {
            var data = obj.GetPettyCachItems();

            return Json(data, JsonRequestBehavior.AllowGet);
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

        public ActionResult LoadData(int id, int? EmpId)
        {
            var userId = User.Identity.GetUserId();
            var financialCycleId = sh.GetUserCurrentFinancialCycleId(userId);
            var myData = obj.LoadData(userId, EmpId, id);
            //var myData1 = db.EmployeePettyCashCustodies
            //                                        .Where(q => q.FinancialCycleId == financialCycleId);

            //if (EmpId.HasValue && EmpId.Value > 0)
            //    myData1 = myData1.Where(q => q.EmployeeId == EmpId.Value);

            //if (id > 0)
            //    myData1 = myData1.Where(q => q.AccTreeId == id);

            //var myData = myData1
            //                        .Select(q => new EmployeePettyCashCustodyVM
            //                        {
            //                            EmployeeId = q.EmployeeId,
            //                            EmployeeName = "EmplName",

            //                            Id = q == null ? 0 : q.Id,
            //                            AccTreeId = q == null ? 0 : q.AccTreeId,
            //                            AccTreeName = q == null ? "" : q.AccountTree.AccName,

            //                            PaiedAmount = q == null ? 0 : (q.PaiedAmount ?? 0),
            //                            TotalDeductionAmount = q == null
            //                                ? 0
            //                                : q.TotalDeductionAmount
            //                        })
            //                        .ToList();

            if (myData == null)
                return Json(new { Message = "حدث خطأ في عملية جلب بيانات الموظفين", Title = "خطأ", Status = "error" }
                                        , JsonRequestBehavior.AllowGet);

            return Json(myData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SaveData(EmployeePettyCashCustodyVM data)
        {
            var userId = User.Identity.GetUserId();
            var year = sh.GetUserCurrentFinancialCycleYear(userId);
            var financialCycleId = sh.GetUserCurrentFinancialCycleId(userId);
            int myYear = Convert.ToInt32(year);

            if (!db.EmployeePettyCashCustodies.Any(q => q.EmployeeId == data.EmployeeId
                                                                                && q.AccTreeId == data.AccTreeId
                                                                                && q.FinancialCycleId == financialCycleId))
            {
                var deductionIds = db.AccountTreeHrDeductions.Where(q => q.AccTreeId == data.AccTreeId)
                    .Select(q => q.DeductionIds).FirstOrDefault();

                List<int> newList = string.IsNullOrWhiteSpace(deductionIds) ? new List<int>()
                                                         : deductionIds
                                                            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                            .Select(id => int.Parse(id.Trim()))
                                                            .ToList();

                decimal? TotalDeductions = hrAPI.GetEmployeeTotalDeductions(data.EmployeeId ?? 0, newList, myYear);

                if (TotalDeductions <= 0)
                {
                    data.PaiedAmount = TotalDeductions;

                    var res = obj.SaveDataSingleRow(data, userId);

                    if (res > 0)
                        return Json(new { Message = "تمت العملية بنجاح", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
                    else
                        return Json(new { Message = "حدث خطأ في عملية الحفظ", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Message = "لا يوجد خصم علي المرتب في السنة التي تعمل عليها", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Message = "تم الادخال مسبقا يمكنك التعديل فقط", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult UpdateData(int id, decimal? amount)
        {
            var userId = User.Identity.GetUserId();
            var year = sh.GetUserCurrentFinancialCycleYear(userId);
            int myYear = Convert.ToInt32(year);
            var data = db.EmployeePettyCashCustodies.Find(id);

            var deductionIds = db.AccountTreeHrDeductions.Where(q => q.AccTreeId == data.AccTreeId)
                .Select(q => q.DeductionIds).FirstOrDefault();

            List<int> newList = string.IsNullOrWhiteSpace(deductionIds) ? new List<int>()
                                                     : deductionIds
                                                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(Id => int.Parse(Id.Trim()))
                                                        .ToList();

            decimal? TotalDeductions = hrAPI.GetEmployeeTotalDeductions(data.EmployeeId ?? 0, newList, myYear);

            if (TotalDeductions <= 0)
            {
                data.PaiedAmount = TotalDeductions;
                data.TotalDeductionAmount = amount;

                db.Entry(data).State = EntityState.Modified;
                var res = db.SaveChanges();

                //var res = obj.SaveDataSingleRow(data, userId);

                if (res > 0)
                    return Json(new { Message = "تمت العملية بنجاح", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { Message = "حدث خطأ في عملية الحفظ", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { Message = "لا يوجد خصم علي المرتب في السنة التي تعمل عليها", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
        }

    }
}