using Microsoft.AspNet.Identity;
using Purchases.CurencyOperation;
using Purchases.Models;
using Purchases.Models.ViewModel;
using Purchases.MyLogic;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace Purchases.Controllers
{
    public class ReportsController : Controller
    {
        private Entities db = new Entities();
        private Reports reportObject = new Reports();
        private SharedClass sh = new SharedClass();

        /*---------------------------------------دفتر الاستاذ---------------------------------*/

        // طباعة بيانات الحركة لي بند محدد بين تاريخين  
        // دفتر الاستاذ لي بند معين بين تاريخين
        [HttpGet]
        public ActionResult printLedgerByDatesOld(int? bankId, int Id, DateTime? dateFrom, DateTime? dateTo)
        {
            AccountTree AccTree = db.AccountTrees.Find(Id);
            var userId = User.Identity.GetUserId();
            SharedClass sc = new SharedClass();

            ViewBag.AccName = AccTree.AccName;
            ViewBag.AccountNature = AccTree.AccountNature.Name;
            ViewBag.AccountNatureId = AccTree.AccNatureId;
            int FinancialCycleId = sc.GetUserCurrentFinancialCycleId(userId);
            string FinancialCyclYear = sc.GetUserCurrentFinancialCycleYear(userId);
            int FinancialCyclYeare = Convert.ToInt32(FinancialCyclYear);

            // var data1 = reportObject.BalancePositionForBanks(FinancialCycleId, bankId, dateFrom, dateTo);
            List<BalanceVM> data = new List<BalanceVM>();

            // جلب البيانات
            var res = reportObject.GetLedgerForAccountAndBank(Id, bankId, dateFrom, dateTo, userId);

            data.AddRange(res);

            // جلب الرصيد المرحل ان وجد
            TreeClass tree = new TreeClass();
            // الارصدة المرحلة تكون في الاصول والخصوم فقط اما الايرادات وبعض المصروفات مافيها ارصد مرحلة
            var rootParentId = tree.getRootParentId(Id); // دالة بتجيب لي الحساب الرئيسي 
            if (rootParentId != 3 && rootParentId != 4) // نستثني الايرادات والمصروفات 
            {
                var openBalance = sc.GetOpenBalanceForSpecificAccTree(FinancialCyclYeare, FinancialCycleId, Id, bankId);

                if (openBalance.debit >= openBalance.credit)
                {
                    openBalance.debit -= openBalance.credit;
                    openBalance.credit = 0;
                }
                else
                {
                    openBalance.credit -= openBalance.debit;
                    openBalance.debit = 0;
                }

                data.Add(openBalance);
            }

            data = data.Where(x => x.debit > 0 || x.credit > 0).ToList();
            data = data.OrderBy(q => q.transactionDate).ToList();
            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = sh.configDate(DateTime.Today);
            ViewBag.dateFrom = dateFrom?.ToShortDateString();
            ViewBag.dateTo = dateTo?.ToShortDateString();
            //ViewBag.bankName = bankId.HasValue? db.AccountTrees.Find(bankId).AccName : "";
            ViewBag.bankName = bankId != null ? db.AccountTrees.Find(bankId).AccName : "";

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.credit).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            return View(data);
        }
        [HttpGet]
        public ActionResult printLedgerByDates(int? bankId, int Id, DateTime? dateFrom, DateTime? dateTo)
        {
            AccountTree AccTree = db.AccountTrees.Find(Id);
            var userId = User.Identity.GetUserId();
            SharedClass sc = new SharedClass();

            ViewBag.AccName = AccTree.AccName;
            ViewBag.AccountNature = AccTree.AccountNature.Name;
            ViewBag.AccountNatureId = AccTree.AccNatureId;
            int FinancialCycleId = sc.GetUserCurrentFinancialCycleId(userId);
            string FinancialCyclYear = sc.GetUserCurrentFinancialCycleYear(userId);
            int FinancialCyclYeare = Convert.ToInt32(FinancialCyclYear);

            // var data1 = reportObject.BalancePositionForBanks(FinancialCycleId, bankId, dateFrom, dateTo);
            List<BalanceVM> data = new List<BalanceVM>();

            var dataList = db.AccountSubs.Include(q=>q.AccountTree).ToList();


            List<BalanceVM> res;
            if (dataList.Any(q => q.AccTreeId == Id))
            // جلب البيانات
            {
                res = reportObject.GetLedgerForAccountAndBank(Id, bankId, dateFrom, dateTo, userId);
            }
            else
            {
                res = reportObject.GetLedgerForAccountAndBankForParents(Id, bankId, dateFrom, dateTo, userId);
            }

            data.AddRange(res);

            // جلب الرصيد المرحل ان وجد
            TreeClass tree = new TreeClass();
            // الارصدة المرحلة تكون في الاصول والخصوم فقط اما الايرادات وبعض المصروفات مافيها ارصد مرحلة
            var rootParentId = tree.getRootParentId(Id); // دالة بتجيب لي الحساب الرئيسي 
            if (rootParentId != 3 && rootParentId != 4) // نستثني الايرادات والمصروفات 
            {
                //var openBalance = sc.GetOpenBalanceForSpecificAccTree(FinancialCyclYeare, FinancialCycleId, Id, bankId);
                //if (openBalance != null)
                //{
                //    if (openBalance.debit >= openBalance.credit)
                //    {
                //        openBalance.debit -= openBalance.credit;
                //        openBalance.credit = 0;
                //    }
                //    else
                //    {
                //        openBalance.credit -= openBalance.debit;
                //        openBalance.debit = 0;
                //    }

                //data.Add(openBalance);
                //}
            }

            //data = data.Where(x => x.debit > 0 || x.credit > 0).ToList();
            data = data.OrderBy(q => q.transactionDate).ToList();
            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = sh.configDate(DateTime.Today);
            ViewBag.dateFrom = dateFrom?.ToShortDateString();
            ViewBag.dateTo = dateTo?.ToShortDateString();
            //ViewBag.bankName = bankId.HasValue? db.AccountTrees.Find(bankId).AccName : "";
            ViewBag.bankName = bankId != null ? db.AccountTrees.Find(bankId).AccName : "";

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.credit).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            data= data.Where(q=>q.debit > 0 || q.credit > 0).ToList();
            if (dataList.Any(q => q.AccTreeId == Id))
                return View(data);
            else
                return View("printLedgerReportForParent", data);
        }

        public ActionResult printLedgerReportForParent()
        {
            return View();
        }
        [HttpGet]
        public ActionResult printLedgerByDatesForSearch(int? bankId, int Id, DateTime? dateFrom, DateTime? dateTo)
        {
            string AccName = db.AccountTrees.Find(Id).AccName;
            ViewBag.AccName = AccName;
            var userId = User.Identity.GetUserId();

            // var data1 = reportObject.printLedgerByDatesForSearch(Id, dateFrom, dateTo);
            //var data = reportObject.GetLedgerForAccountAndBank(Id, bankId, dateFrom, dateTo, userId);

            List<BalanceVM> data = new List<BalanceVM>();

            var dataList = db.AccountSubs.Include(q => q.AccountTree).ToList();

            List<BalanceVM> res;
            bool IsParent = dataList.Any(q => q.AccTreeId == Id);
            if (IsParent)
            // جلب البيانات
            {
                res = reportObject.GetLedgerForAccountAndBank(Id, bankId, dateFrom, dateTo, userId);
            }
            else
            {
                res = reportObject.GetLedgerForAccountAndBankForParents(Id, bankId, dateFrom, dateTo, userId);
                res.ForEach(q => { q.note = q.accTreeName; q.Diff = q.debit - q.credit; q.transactionDateStr = q.transactionDateStr ?? "#####"; });
            }

            data.AddRange(res);
            data = data.Where(q=> q.debit> 0 || q.credit > 0).ToList();
            return Json(new { data, IsParent }, JsonRequestBehavior.AllowGet);
        }

        // شاشة طباعة دفتر الاستاذ
        public ActionResult printLedgerReport()
        {
            return View();
        }

        // عمل كشف حساب لي بند معين من غير تاريخ
        [HttpGet]
        public ActionResult printLedger(int Id)
        {
            var data = reportObject.printLidger(Id);

            string AccName = db.AccountTrees.Find(Id).AccName;
            ViewBag.AccName = AccName;

            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = sh.configDate(DateTime.Today);

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.credit).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            return View(data);
        }

        /*------------------------------------------- قيود اليومية --------------------------------------------*/

        // طباعة بيانات الحركة بين تاريخين  
        [HttpGet]
        public ActionResult printDataInDateRange(int? bankId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var userId = User.Identity.GetUserId();
            // var dataOld = reportObject.printDataByDates(dateFrom, dateTo).OrderBy(x=>x.transactionDate);
            var data = reportObject.printDataByDatesForBank(bankId, dateFrom, dateTo, userId  ).OrderBy(x => x.transactionDate);
            //var data = reportObject.printAllData();

            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.dateFrom = dateFrom == null ? null : dateFrom.Value.Date.ToShortDateString();
            ViewBag.dateTo = dateTo == null ? null : dateTo.Value.Date.ToShortDateString();

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.credit).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            return View(data);
            //return Json(data.Select(o=> o.transactionId).Distinct(), JsonRequestBehavior.AllowGet);
        }

        //  قيد البيومية في ذر البحث
        [HttpGet]
        public ActionResult GetDataInDateRangeForSearch(int? bankId, DateTime? dateFrom, DateTime? dateTo)
        {
            var dataOld = reportObject.printDataByDatesForSearch(dateFrom, dateTo);
            var data = reportObject.printDataByDatesForSearchForBank(bankId, dateFrom, dateTo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // ده شاشة تقرير قيد اليومية في الشاشة الرئيسية
        public ActionResult printDataReport()
        {
            return View();
        }

        /*------------------------------------------- تقارير الضريبة --------------------------------------------*/

        // جلب البيانات لطباعة ضريبة ال 1% الخاصة بالمصروفات
        // مستخدمة في تقرير printBudgetReport
        [HttpGet]
        public ActionResult printTaxReport()
        {
            List<TransactionVM> data = reportObject.printTaxReport();

            ViewBag.sumOfTax = data.Sum(x => x.tax);

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.credit).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            return View(data);
        }

        // ذر البحث في شاشة طباعة تقرير ضريبة ال 1% مصروفات
        [HttpGet]
        public ActionResult GetTaxInDateRangeForSearch(DateTime? DateFrom, DateTime? DateTo, int? bankId)
        {
            var userId = User.Identity.GetUserId();
            var data = reportObject.GetTaxInDateRangeForSearch(DateFrom, DateTo, bankId, userId).OrderBy(x => x.transactionDate);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // ذر الطباعة في شاشة طباعة تقرير ضريبة ال 1% مصروفات
        [HttpGet]
        public ActionResult printTaxReportInRange(DateTime? DateFrom, DateTime? DateTo, int? bankId)
        {
            var userId = User.Identity.GetUserId();

            //var currentUserYear = sh.GetUserCurrentFinancialCycleYear(userId);

            //if (DateFrom.Value.Year.ToString() != currentUserYear || DateTo.Value.Year.ToString() != currentUserYear)
            //{
            //    return Json(new { Message = "العام المالي للمستخدم لا يتوافق مع العام المالي للتواريخ المطلوبة", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            //}

            List<TransactionVM> data = reportObject.GetTaxInDateRangeForSearch(DateFrom, DateTo, bankId, userId).OrderBy(x => x.transactionDate).ToList();

            ViewBag.sumOfTax = data.Sum(x => x.tax);
            ViewBag.BankName = bankId > 0 ? db.AccountTrees.Find(bankId).AccName : "";
            ViewBag.dateFrom = DateFrom?.ToShortDateString();
            ViewBag.dateTo = DateTo?.ToShortDateString();

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.tax).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            return View(data);
        }

        // شاشة طباعة ضريبة ال 1% الخاصة بالمصروفات
        public ActionResult TaxReport()
        {
            return View();
        }

        // ده التقرير ال بيمشي لي سلطة الضرائب
        [HttpGet]
        public ActionResult printTaxAuthorityReport(DateTime? DateFrom, DateTime? DateTo, int? bankId)
        {
            var userId = User.Identity.GetUserId();

            //var currentUserYear = sh.GetUserCurrentFinancialCycleYear(userId);

            //if(DateFrom.Value.Year.ToString() != currentUserYear || DateTo.Value.Year.ToString() != currentUserYear)
            //{
            //    return Json(new { Message = "العام المالي للمستخدم لا يتوافق مع العام المالي للتواريخ المطلوبة", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            //}

            List<TransactionVM> data = reportObject.GetTaxInDateRangeForSearch(DateFrom, DateTo, bankId, userId);

            if (data != null)
            {
                ViewBag.sumOfTax = data.Sum(x => x.tax);
                ViewBag.BankName = bankId > 0 ? db.AccountTrees.Find(bankId).AccName : "";
                ViewBag.dateFrom = DateFrom?.ToShortDateString();
                ViewBag.dateTo = DateTo?.ToShortDateString();

                BaseClass baseClass = new BaseClass();
                string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.tax).ToString(), 0);
                ViewBag.txtSumOfBous = txtSumOfBous;

                return View("TaxAuthorityReport", data);
            }
            else
            {
                return Json("لا توجد بيانات لعرضها", JsonRequestBehavior.AllowGet);
            }
        }




        //   ده التقرير ال بيمشي لي سلطة الضرائب بخصوص ضريبة الدخل
        [HttpGet]
        public ActionResult printIncomeTaxAuthorityReport(DateTime? DateFrom, DateTime? DateTo, int? bankId)
        {
            var userId = User.Identity.GetUserId();

            //var currentUserYear = sh.GetUserCurrentFinancialCycleYear(userId);

            //if(DateFrom.Value.Year.ToString() != currentUserYear || DateTo.Value.Year.ToString() != currentUserYear)
            //{
            //    return Json(new { Message = "العام المالي للمستخدم لا يتوافق مع العام المالي للتواريخ المطلوبة", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            //}

            List<TransactionVM> data = reportObject.GetIncomeTaxInDateRangeForSearch(DateFrom, DateTo, bankId, userId);

            if (data != null)
            {
                ViewBag.sumOfTax = data.Sum(x => x.tax);
                ViewBag.BankName = bankId > 0 ? db.AccountTrees.Find(bankId).AccName : "";
                //ViewBag.dateFrom = DateFrom?.ToShortDateString();
                //ViewBag.dateTo = DateTo?.ToShortDateString();
                // عشان يستعمل التاريخ الميلادي
                ViewBag.dateFrom = DateFrom?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                ViewBag.dateTo = DateTo?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                BaseClass baseClass = new BaseClass();
                string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.tax).ToString(), 0);
                ViewBag.txtSumOfBous = txtSumOfBous;

                return View("IncomeTaxAuthorityReport", data);
            }
            else
            {
                return Json("لا توجد بيانات لعرضها", JsonRequestBehavior.AllowGet);
            }
        }














        /*-------------------------------------------------------- تقارير الشجرة المحاسبية -------------------------------------------------------------*/

        // طباعة كشف حساب لي بند محدد 
        [HttpGet]
        public ActionResult printAccountStatement(int Id)
        {
            var userId = User.Identity.GetUserId();
            var financialCycleId = sh.GetUserCurrentFinancialCycleId(userId);

            List<TransactionVM> data = reportObject.printsAccountStatement(Id, financialCycleId);
            string AccName = db.AccountTrees.Find(Id).AccName;
            ViewBag.AccName = AccName;

            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = sh.configDate(DateTime.Today);

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.credit).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            return View(data);
        }

        // طباعة جميع الحركات
        [HttpGet]
        public ActionResult printAllData()
        {
            var userId = User.Identity.GetUserId();
            var financialCycleId = sh.GetUserCurrentFinancialCycleId(userId);

            var data = reportObject.printAllData(financialCycleId);

            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = sh.configDate(DateTime.Today);

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.credit).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            return View(data);
        }

        // طباعة قيد اليومية لي بند محدد
        [HttpGet]
        public ActionResult printData(int Id)
        {
            var userId = User.Identity.GetUserId();
            var financialCycleId = sh.GetUserCurrentFinancialCycleId(userId);

            var data = reportObject.printDataByAccountId(Id, financialCycleId);
            ViewBag.AccName = db.AccountTrees.Find(Id).AccName;

            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = sh.configDate(DateTime.Today);

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.credit).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            return View(data);
        }

        // طباعة ضريبة ال 5% الخاصة بالايرادات
        [HttpGet]
        public ActionResult printFiveTaxReport()
        {
            var data = reportObject.printFiveTaxReport();
            ViewBag.sumOfTax = data.Sum(x => x.tax);

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.tax).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            return View(data);
        }

        // طباعة أورنيك 17
        [HttpGet]
        public ActionResult PrintOrnik17Original(int id)
        {
            try
            {
                string pattren = @"^\d{10,}$"; // بالنسبة لي رقم فاتورة ايصالي
                // الحصول على المعاملة
                var objTrans = db.Transactions.Find(id);

                if (objTrans == null)
                {
                    return Json(new { Message = "لم يتم العثور على المعاملة.", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
                }

                var userId = User.Identity.GetUserId();
                // الحصول على الحسابات المرتبطة
                var TransactionDetailsData = db.TransactionDetails
                    .Where(x => x.TransactionId == id).ToList();

                var AccTreeIdsList = new List<int>();
                foreach (var item in TransactionDetailsData)
                {
                    // نجيب كل البنود ما عدا البنود البنكية او ما عدا البنك
                    if (sh.IsItBankAccount(item.AccTreeId)) // اذا البند هو حساب بنكي اتجاهله
                    {
                        continue;
                    }

                    AccTreeIdsList.Add(item.AccTreeId);
                }

                // حساب ضريبة ال 1% من البيانات
                decimal? taxVal = 0;
                if (TransactionDetailsData.Any(q => q.AccTreeId == 228))
                {
                    taxVal = TransactionDetailsData.FirstOrDefault(q => q.AccTreeId == 228).Credit;
                    AccTreeIdsList.Remove(228); // احذف ضريبة ال 1% من القائمة حقت البيانات
                }

                // نحسب اصل المبلغ
                decimal? sumOfDebit = TransactionDetailsData.Sum(q => q.Debit);
                decimal? baseAmount = objTrans.HasAddedTax == true ? sumOfDebit - (sumOfDebit / 1.17m) * 0.17m : sumOfDebit;

                int AccTrreId = AccTreeIdsList.FirstOrDefault();
                string AccTrreName = TransactionDetailsData.Where(q => q.Debit > 0).Count() > 1 ? "حساب مذكورين" : TransactionDetailsData.FirstOrDefault(q => q.Debit > 0).AccountTree.AccName ?? "غير معروف";

                SharedClass shared = new SharedClass();
                // البيانات الأساسية
                string currentYear = shared.GetCurrentTransactionFinancialCycleYear(objTrans.Id);
                decimal? total = baseAmount - taxVal;

                // تمرير البيانات إلى العرض
                ViewBag.CurrentYear = currentYear;
                ViewBag.Note = objTrans.Note ?? "غير مدخل";
                ViewBag.BalanceName = AccTrreName;
                ViewBag.TransactionDate = sh.configDate(objTrans.TransactionDate);
                ViewBag.recipient = Regex.IsMatch(objTrans.Recipient, pattren) ? $"فاتورة إيصالي بالرقم {objTrans.Recipient}" : objTrans.Recipient;

                ViewBag.transactionId = objTrans.Id;
                ViewBag.CheckNo = objTrans.PrintChecks.Any()
                           ? objTrans.PrintChecks.FirstOrDefault(x => x.TransactionId == id).CheckNo
                           : null;

                var data = TransactionDetailsData
                   .Where(x => AccTreeIdsList.Any(q => q == x.AccTreeId))
                   .Select(item => new TransactionVM
                   {
                       transactionId = id,
                       note = item.Note,
                       accName = item.AccountTree.AccName,
                       amount = item.Debit > 0 ?
                                objTrans.HasAddedTax == true ? baseAmount : item.Debit
                                : item.Credit,
                       tax = item.Debit > 0 && objTrans.HasTax == true ? baseAmount * 0.01m : 0,
                       total = item.Debit > 0 ?  // ضريبة ال 17% فقط
                       objTrans.HasTax == true ? baseAmount - taxVal : baseAmount  // ضريبة ال 1% فقط
                       : item.Credit, // حساب دائن فقط
                       isCredit = item.Credit > 0 ? true : false
                   })
                   .ToList();

                // إضافة ضريبة القيمة المضافة 17%
                if (objTrans.HasAddedTax == true)
                {
                    decimal? vat = baseAmount * 0.17m;
                    data.Add(new TransactionVM
                    {
                        transactionId = id,
                        accName = "ضريبة القيمة المضافة (17%)",
                        note = "ضريبة القيمة المضافة (17%)",
                        amount = vat,
                        tax = 0,
                        total = vat,
                        isCredit = false
                    });
                }

                decimal? totalAmount = data.Where(q => q.isCredit == false).Sum(x => x.total) - data.Where(q => q.isCredit == true).Sum(x => x.total);
                ViewBag.AmountText = new BaseClass().ChangeNumberToText(totalAmount?.ToString() ?? "0", 0);
                ViewBag.TotalAmount = totalAmount;

                return View(data);
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء معالجة الطلب.", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        // طباعة أورنيك 17
        [HttpGet]
        public ActionResult PrintOrnik17(int id)
        {
            try
            {
                string pattren = @"^\d{10,}$"; // بالنسبة لي رقم فاتورة ايصالي
                decimal? AddedTaxPercent = 0;
                // الحصول على المعاملة
                var objTrans = db.Transactions.Find(id);

                if (objTrans == null)
                {
                    return Json(new { Message = "لم يتم العثور على المعاملة.", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
                }

                var userId = User.Identity.GetUserId();
                // الحصول على الحسابات المرتبطة
                var TransactionDetailsData = db.TransactionDetails
                    .Where(x => x.TransactionId == id).ToList();
                //AddedTaxPercent = objTrans.AddedTaxPercent!=null? objTrans.AddedTaxPercent * 100: 0;
                var AccTreeIdsList = new List<int>();
                foreach (var item in TransactionDetailsData)
                {
                    // نجيب كل البنود ما عدا البنود البنكية او ما عدا البنك
                    if (sh.IsItBankAccount(item.AccTreeId)) // اذا البند هو حساب بنكي اتجاهله
                    {
                        continue;
                    }

                    AccTreeIdsList.Add(item.AccTreeId);
                }

                // حساب ضريبة ال 1% من البيانات
                decimal? taxVal = 0;
                if (TransactionDetailsData.Any(q => q.AccTreeId == 228))
                {
                    taxVal = TransactionDetailsData.FirstOrDefault(q => q.AccTreeId == 228).Credit;
                    AccTreeIdsList.Remove(228); // احذف ضريبة ال 1% من القائمة حقت البيانات
                }

                AddedTaxPercent = objTrans.AddedTaxPercent;
                decimal? AddedTaxPercentNew = AddedTaxPercent + 1;
                // نحسب اصل المبلغ
                decimal? sumOfDebit = TransactionDetailsData.Sum(q => q.Debit);
                //decimal? baseAmount = objTrans.HasAddedTax == true ? sumOfDebit - (sumOfDebit / 1.17m) * 0.17m : sumOfDebit;
                decimal? baseAmount = objTrans.HasAddedTax == true ? sumOfDebit - (sumOfDebit / AddedTaxPercentNew) * AddedTaxPercent : sumOfDebit;

                int AccTrreId = AccTreeIdsList.FirstOrDefault();
                string AccTrreName = TransactionDetailsData.Where(q => q.Debit > 0).Count() > 1 ? "حساب مذكورين" : TransactionDetailsData.FirstOrDefault(q => q.Debit > 0).AccountTree.AccName ?? "غير معروف";

                SharedClass shared = new SharedClass();
                // البيانات الأساسية
                string currentYear = shared.GetCurrentTransactionFinancialCycleYear(objTrans.Id);
                decimal? total = baseAmount - taxVal;

                // تمرير البيانات إلى العرض
                ViewBag.CurrentYear = currentYear;
                ViewBag.Note = objTrans.Note ?? "غير مدخل";
                ViewBag.BalanceName = AccTrreName;
                ViewBag.TransactionDate = sh.configDate(objTrans.TransactionDate);
                ViewBag.recipient = Regex.IsMatch(objTrans.Recipient, pattren) ? $"فاتورة إيصالي بالرقم {objTrans.Recipient}" : objTrans.Recipient;

                ViewBag.transactionId = objTrans.Id;
                ViewBag.CheckNo = objTrans.PrintChecks.Any()
                           ? objTrans.PrintChecks.FirstOrDefault(x => x.TransactionId == id).CheckNo
                           : null;

                var data = TransactionDetailsData
                   .Where(x => AccTreeIdsList.Any(q => q == x.AccTreeId))
                   .Select(item => new TransactionVM
                   {
                       transactionId = id,
                       note = item.Note,
                       accName = item.AccountTree.AccName,
                       amount = item.Debit > 0 ?
                                objTrans.HasAddedTax == true ? baseAmount : item.Debit
                                : item.Credit,
                       tax = item.Debit > 0 && objTrans.HasTax == true ? baseAmount * 0.01m : 0,
                       total = item.Debit > 0 ?  // ضريبة ال 17% فقط
                       objTrans.HasTax == true ? baseAmount - taxVal : item.Debit  // ضريبة ال 1% فقط
                       : item.Credit, // حساب دائن فقط
                       isCredit = item.Credit > 0 ? true : false
                   })
                   .ToList();

                // إضافة ضريبة القيمة المضافة 17%
                if (objTrans.HasAddedTax == true)
                {
                    string AddedTaxPercentText = shared.FormatDecimal(AddedTaxPercent);
                    decimal? vat = baseAmount * AddedTaxPercent;
                    data.Add(new TransactionVM
                    {
                        transactionId = id,
                        accName = $"ضريبة القيمة المضافة ({AddedTaxPercentText}%)",
                        note = $"ضريبة القيمة المضافة ({AddedTaxPercentText}%)",
                        amount = vat,
                        tax = 0,
                        total = vat,
                        isCredit = false
                    });
                }

                decimal? totalAmount = data.Where(q => q.isCredit == false).Sum(x => x.total) - data.Where(q => q.isCredit == true).Sum(x => x.total);
                ViewBag.AmountText = new BaseClass().ChangeNumberToText(totalAmount?.ToString() ?? "0", 0);
                ViewBag.TotalAmount = totalAmount;

                return View(data);
            }
            catch (Exception e)
            {
                return Json(new { Message = "حدث خطأ أثناء معالجة الطلب.", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        /*---------------------------------------دي التقارير الخاصة بي الحركات المحاسبية--------------------------------*/

        // طباعة بيانات حركة محددة
        [HttpGet]
        public ActionResult printTransData(int transId)
        {
            var data = reportObject.printDataByTransactionId(transId);

            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = sh.configDate(DateTime.Today);

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.credit).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            return View(data);
        }

        // طباعة الحركات في شهر محدد
        [HttpGet]
        public ActionResult printDatas(DateTime myDate)
        {
            var data = reportObject.printDataByDate(myDate);

            ViewBag.SumOfAmount = data.Sum(x => x.credit);
            ViewBag.SelectedDate = myDate.ToShortDateString();

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.credit).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            return View(data);
        }

        /*------------------------------------------دي التقارير الخاصة بي الحركات بين البنوك------------------------------------------*/

        // شاشة تقرير حركات البنوك
        public ActionResult printBankTransReport(DateTime? DateFrom, DateTime? DateTo)
        {
            List<BankTransferVM> data = reportObject.GetTransfDateRangeForSearch(DateFrom, DateTo);

            if (data != null)
            {
                ViewBag.sumOfAmount = data.Sum(x => x.Amount);
                ViewBag.DateFrom = DateFrom != null ? DateFrom.Value.ToShortDateString() : sh.configDate(new DateTime(DateTime.Now.Year, 1, 1));
                ViewBag.DateTo = DateTo != null ? DateTo.Value.ToShortDateString() : sh.configDate(DateTime.Now);

                BaseClass baseClass = new BaseClass();
                string txtSumOfBous = baseClass.ChangeNumberToText(data.Sum(x => x.Amount).ToString() ?? "0", 0);
                ViewBag.txtSumOfBous = txtSumOfBous;

                return View(data);
            }
            else
            {
                return Json("لا توجد بيانات لعرضها", JsonRequestBehavior.AllowGet);
            }
        }

        // شاشة تقرير حركات البنوك
        public ActionResult BankTransReport()
        {
            return View();
        }

        // ذر البحث في شاشة طباعة تقرير حركات البنوك
        [HttpGet]
        public ActionResult GetBankTransDateRangeForSearch(DateTime? DateFrom, DateTime? DateTo)
        {
            var data = reportObject.GetTransfDateRangeForSearch(DateFrom, DateTo);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /*---------------دي التقارير الخاصة بي قسم الموازنة ---------------------------------*/

        // دي تست لي تقرير موقف الموازنه - برسل ليها رقم السنة المالية المحددة وبترجع لي تقرير عن الموقف المالي للموازنة
        public ActionResult printBalancesReport(DateTime? DateFrom, DateTime? DateTo, int ParentId)
        {
            SharedClass sc = new SharedClass();
            if (DateFrom != null && DateTo != null)
            {
                ViewBag.DateFrom = DateFrom.Value.ToShortDateString();
                ViewBag.DateTo = DateTo.Value.ToShortDateString();
            }
            var userId = User.Identity.GetUserId();
            ViewBag.AccName = db.AccountTrees.Find(ParentId).AccName;
            int CurrentFinancialCycleId = sc.GetUserCurrentFinancialCycleId(userId);

            var CurrentFinancialCycle = db.FinancialCycles.Find(CurrentFinancialCycleId).Year;
            ViewBag.CurrentFinancialCycle = CurrentFinancialCycle;
            var data = reportObject.BalancePosition(CurrentFinancialCycleId, ParentId, DateFrom, DateTo);

            return View(data);
            //return Json(data, JsonRequestBehavior.AllowGet);
        }

        // دي حسب البنك رئاسة او بورتسودان
        public ActionResult BalancePositionForBanks(DateTime? DateFrom, DateTime? DateTo, int ParentId)
        {
            SharedClass sc = new SharedClass();
            if (DateFrom != null && DateTo != null)
            {
                ViewBag.DateFrom = DateFrom.Value.ToShortDateString();
                ViewBag.DateTo = DateTo.Value.ToShortDateString();
            }
            var userId = User.Identity?.GetUserId();
            ViewBag.AccName = db.AccountTrees.Find(ParentId).AccName;
            int CurrentFinancialCycleId = sc.GetUserCurrentFinancialCycleId(userId);

            var CurrentFinancialCycle = db.FinancialCycles.Find(CurrentFinancialCycleId).Year;
            ViewBag.CurrentFinancialCycle = CurrentFinancialCycle;
            var val = reportObject.BalancePositionForBanks(CurrentFinancialCycleId, ParentId, DateFrom, DateTo);

            return View("printBalancesReportSumm", val);
            //return Json(val, JsonRequestBehavior.AllowGet);
        }

        // دي تست لي تقرير موقف الموازنه - برسل ليها رقم السنة المالية المحددة وبترجع لي تقرير عن الموقف المالي للموازنة
        public ActionResult printBalancesReportRevenue(DateTime? DateFrom, DateTime? DateTo, int ParentId)
        {
            SharedClass sc = new SharedClass();
            if (DateFrom != null && DateTo != null)
            {
                ViewBag.DateFrom = DateFrom.Value.ToShortDateString();
                ViewBag.DateTo = DateTo.Value.ToShortDateString();
            }
            var userId = User.Identity?.GetUserId();
            ViewBag.AccName = db.AccountTrees.Find(ParentId).AccName;
            int CurrentFinancialCycleId = sc.GetUserCurrentFinancialCycleId(userId);

            var CurrentFinancialCycle = db.FinancialCycles.Find(CurrentFinancialCycleId).Year;
            ViewBag.CurrentFinancialCycle = CurrentFinancialCycle;
            var val = reportObject.BalancePositionRevenue(CurrentFinancialCycleId, ParentId, DateFrom, DateTo);

            return View(val);
            //return Json(val, JsonRequestBehavior.AllowGet);
        }

        //  شاشة تقرير  مراقبة الصرف دي القدييييمة اول واحده وقالوا ما عاوزين التقرير كده
        public ActionResult printBalancesReportOld(DateTime? DateFrom, DateTime? DateTo, int AccountTopType)
        {
            var data = reportObject.BalancePositionByDate(DateFrom, DateTo, AccountTopType);

            if (data != null)
            {
                ViewBag.DateFrom = DateFrom != null ? DateFrom.Value.ToShortDateString() : sh.configDate(new DateTime(DateTime.Now.Year, 1, 1));
                ViewBag.DateTo = DateTo != null ? DateTo.Value.ToShortDateString() : DateTime.Now.ToShortDateString();

                if (AccountTopType > 0)
                {
                    ViewBag.RptTitle = "(" + db.AccountTrees.Find(AccountTopType).AccName + ")";
                }

                return View(data);
            }
            else
            {
                return Json("لا توجد بيانات لعرضها", JsonRequestBehavior.AllowGet);
            }
        }

        // شاشة تقرير مراقبة الصرف
        public ActionResult BalancesReport()
        {
            return View();
        }

        // شاشة تقرير مراقبة الصرف
        public ActionResult BalancesReportForBanks()
        {
            return View();
        }

        // ذر البحث في شاشة طباعة تقرير  مراقبة الصرف
        [HttpGet]
        public ActionResult GetBalancesDateRangeForSearch(DateTime? DateFrom, DateTime? DateTo, int AccountTopType)
        {
            var data = reportObject.BalancePositionByDate(DateFrom, DateTo, AccountTopType);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // دي حسب البنك رئاسة او بورتسودان
        public ActionResult BalancePositionForBanksSearch(DateTime? DateFrom, DateTime? DateTo, int ParentId)
        {
            SharedClass sc = new SharedClass();
            if (DateFrom != null && DateTo != null)
            {
                ViewBag.DateFrom = DateFrom.Value.ToShortDateString();
                ViewBag.DateTo = DateTo.Value.ToShortDateString();
            }
            var userId = User.Identity.GetUserId();
            ViewBag.AccName = db.AccountTrees.Find(ParentId).AccName;
            int CurrentFinancialCycleId = sc.GetUserCurrentFinancialCycleId(userId);

            var CurrentFinancialCycle = db.FinancialCycles.Find(CurrentFinancialCycleId).Year;
            ViewBag.CurrentFinancialCycle = CurrentFinancialCycle;
            var val = reportObject.BalancePositionForBanks(CurrentFinancialCycleId, ParentId, DateFrom, DateTo);

            var data = val.SelectMany(q => q.details).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /*---------------------------------------------------------------تقارير الحسابات الختامية--------------------------------------------------*/
        // تقرير ميزان المراجعة بالارصدة - يعني كل حساب  والرصيد ال فيهو
        public ActionResult printTrailBalance(int financeCycleId)
        {
            var lists = new List<dynamic>() {
                     new { parentId = 245, isExpense = true }, // الفصل الاول
                    new { parentId = 12,  isExpense = true }, // التشغيلي
                    new { parentId = 13,  isExpense = true }, // الاداري
                    new { parentId = 172, isExpense = true }, // الراسمالي
                    new { parentId = 235, isExpense = true }, // التنمية 
                    new { parentId = 7,   isExpense = true }, // البنك
                    new { parentId = 9,   isExpense = true }, // المدينون
                    new { parentId = 3,   isExpense = false }, // الايرادات
                    new { parentId = 11,  isExpense = false }, // راس المال
                    new { parentId = 10,  isExpense = false }  // الدائنون
            };

            List<TransactionDetail> RetrivedDataList = db.TransactionDetails.Where(x => x.Transaction.FinancialCycleId == financeCycleId).ToList();
            RetrivedDataList = RetrivedDataList.Where(x => x.Transaction.DocumentTypeId != 5).ToList(); // طلع التحاويل البنكية بره

            var dataList = new List<BalanceVM>();
            foreach (var parent in lists)
            {
                BalanceVM data = reportObject.TrailBalanceOutBalance(financeCycleId, parent.parentId, RetrivedDataList, new List<int>(), parent.isExpense);
                dataList.Add(data);
            }

            decimal totalExpenses = dataList.Where(x => x.isExpense == true).Sum(s => Math.Abs(s.actualExchange ?? 0));
            decimal totalRevenues = dataList.Where(x => x.isExpense == false).Sum(s => Math.Abs(s.actualExchange ?? 0));

            decimal Profit = Math.Abs(totalExpenses - totalRevenues); // مجمل الربح
            decimal netDifference = Profit * (decimal)0.05; // "m" يرمز إلى decimal (ده الاحتياطي القانوني)
            decimal netProfit = Profit - netDifference; // صافي الربح = مجمل الربح - الاحتياطي القانوني

            var obj = dataList.FirstOrDefault(x => x.Id == 10);
            dataList.Remove(obj);

            dataList.Add(new BalanceVM()
            {
                accTreeName = "الــربــح",
                sumOfDebit = 0,
                sumOfCredit = netProfit,
                actualExchange = netProfit,
                isExpense = false,
            });

            dataList.Add(new BalanceVM()
            {
                accTreeName = "الاحتياطي القانوني(5%)",
                sumOfDebit = 0,
                sumOfCredit = netDifference,
                actualExchange = netDifference,
                isExpense = false
            });

            obj.isExpense = false;
            dataList.Add(obj);

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(dataList.Sum(x => x.actualExchange).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            ViewBag.CurrentFinancialCycle = db.FinancialCycles.Find(financeCycleId).Year; // العام المالي الحالي
            return View(dataList);
        }

        // تقرير ميزان المراجعة بالمجاميع - يعني كل حساب  المدين و الدائن ال فيهو
        public ActionResult printTrailBalanceSummation(int financeCycleId)
        {
            var lists = new List<dynamic>() {
                    new { parentId = 245, isExpense = true }, // الفصل الاول
                    new { parentId = 12,  isExpense = true }, // التشغيلي
                    new { parentId = 13,  isExpense = true }, // الاداري
                    new { parentId = 172, isExpense = true }, // الراسمالي
                    new { parentId = 235, isExpense = true }, // التنمية 
                    new { parentId = 7,   isExpense = true }, // البنك
                    new { parentId = 9,   isExpense = true }, // المدينون
                    new { parentId = 3,   isExpense = false }, // الايرادات
                    new { parentId = 11,  isExpense = false }, // راس المال
                    new { parentId = 10,  isExpense = false }  // الدائنون
            };

            List<TransactionDetail> RetrivedDataList = db.TransactionDetails.Where(x => x.Transaction.FinancialCycleId == financeCycleId).ToList();
            RetrivedDataList = RetrivedDataList.Where(x => x.Transaction.DocumentTypeId != 5).ToList(); // طلع التحاويل البنكية بره
            var dataList = new List<BalanceVM>();
            foreach (var parent in lists)
            {
                BalanceVM data = reportObject.TrailBalanceOutBalance(financeCycleId, parent.parentId, RetrivedDataList, new List<int>(), parent.isExpense);
                dataList.Add(data);
            }

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(dataList.Sum(x => x.actualExchange).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;


            ViewBag.CurrentFinancialCycle = db.FinancialCycles.Find(financeCycleId).Year; // العام المالي الحالي
            return View(dataList);
        }

        // تقرير ميزان المراجعة بالمجاميع - يعني كل حساب  المدين و الدائن ال فيهو
        public ActionResult printTrailBalanceExplanation(int financeCycleId)
        {
            var lists = new List<dynamic>() {
                    new { parentId = 245, isExpense = true }, // الفصل الاول
                    new { parentId = 12,  isExpense = true }, // التشغيلي
                    new { parentId = 13,  isExpense = true }, // الاداري
                    new { parentId = 172, isExpense = true }, // الراسمالي
                    new { parentId = 235, isExpense = true }, // التنمية 
                    new { parentId = 7,   isExpense = true }, // البنك
                    new { parentId = 9,   isExpense = true }, // المدينون
                    new { parentId = 3,   isExpense = false }, // الايرادات
                    new { parentId = 11,  isExpense = false }, // راس المال
                    new { parentId = 10,  isExpense = false }  // الدائنون
            };

            List<TransactionDetail> RetrivedDataList = db.TransactionDetails.Where(x => x.Transaction.FinancialCycleId == financeCycleId).ToList();
            RetrivedDataList = RetrivedDataList.Where(x => x.Transaction.DocumentTypeId != 5).ToList(); // طلع التحاويل البنكية بره
            var dataList = new List<BalanceVM>();
            foreach (var parent in lists)
            {
                BalanceVM data = reportObject.TrailBalanceOutBalance(financeCycleId, parent.parentId, RetrivedDataList, new List<int>(), parent.isExpense);
                dataList.Add(data);
            }

            dataList = dataList.Where(q => q.sumOfDebit > 0 || q.sumOfCredit > 0).ToList();
            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(dataList.Sum(x => x.actualExchange).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            ViewBag.CurrentFinancialCycle = db.FinancialCycles.Find(financeCycleId).Year; // العام المالي الحالي
            return View(dataList.OrderBy(x => x.accTreeCode));
        }

        // دي شاشة لاذرار طباعة تقارير الحسابات الختماية
        public ActionResult FinalAccountingReports()
        {
            SharedClass sh = new SharedClass();
            var userId = User.Identity.GetUserId();
            ViewBag.financeCycleId = sh.GetUserCurrentFinancialCycleId(userId);

            return View();
        }

        // تقرير ميزان المراجعة بالمجاميع - يعني كل حساب  المدين و الدائن ال فيهو
        public ActionResult printTrailBalanceIncomeStatement(int financeCycleId)
        {
            var lists = new List<dynamic>() {
                    new { parentId = 3,   isExpense = false }, // الايرادات
                    new { parentId = 245, isExpense = true }, // الفصل الاول
                    new { parentId = 12,  isExpense = true }, // التشغيلي
                    new { parentId = 13,  isExpense = true }, // الاداري
            };

            List<TransactionDetail> RetrivedDataList = db.TransactionDetails.Where(x => x.Transaction.FinancialCycleId == financeCycleId).ToList();
            RetrivedDataList = RetrivedDataList.Where(x => x.Transaction.DocumentTypeId != 5).ToList(); // طلع التحاويل البنكية بره

            var dataList = new List<BalanceVM>();
            foreach (var parent in lists)
            {
                BalanceVM data = reportObject.TrailBalanceOutBalance(financeCycleId, parent.parentId, RetrivedDataList, new List<int>(), parent.isExpense);
                dataList.Add(data);
            }

            decimal totalExpenses = dataList.Where(x => x.Id != 3).Sum(s => Math.Abs(s.actualExchange ?? 0));
            decimal totalRevenues = dataList.Where(x => x.Id == 3).Sum(s => Math.Abs(s.actualExchange ?? 0));

            decimal Profit = Math.Abs(totalRevenues - totalExpenses); // مجمل الربح
            decimal netDifference = Profit * (decimal)0.05; // "m" يرمز إلى decimal (ده الاحتياطي القانوني)
            decimal netProfit = Profit - netDifference; // صافي الربح = مجمل الربح - الاحتياطي القانوني

            dataList.Add(new BalanceVM()
            {
                accTreeName = "مجمل الــربــح",
                sumOfDebit = 0,
                sumOfCredit = Profit,
                actualExchange = Profit,
                isExpense = false,
            });

            dataList.Add(new BalanceVM()
            {
                accTreeName = "الاحتياطي القانوني(5%)",
                sumOfDebit = 0,
                sumOfCredit = netDifference,
                actualExchange = netDifference,
                isExpense = false
            });

            dataList.Add(new BalanceVM()
            {
                accTreeName = "صافي الربح",
                sumOfDebit = netProfit,
                sumOfCredit = netProfit,
                actualExchange = netProfit,
                isExpense = false
            });

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(dataList.Sum(x => x.actualExchange).ToString(), 0);
            ViewBag.txtSumOfBous = txtSumOfBous;

            ViewBag.CurrentFinancialCycle = db.FinancialCycles.Find(financeCycleId).Year; // العام المالي الحالي
            return View(dataList.OrderBy(x => x.accTreeCode));
        }

        // طباعة خطاب البنك
        public ActionResult printBankLetterOld(int Id)
        {
            SharedClass sh = new SharedClass();
            var transactionObj = db.Transactions.Find(Id);

            ViewBag.Id = Id;
            ViewBag.TransactionDate = transactionObj.TransactionDate.Value.ToShortDateString();

            // بيانات الحساب المدفوع له
            // اذا العملية تحويل بنكي يبقى المدفوع له حساب بنكي ايضا
            if (transactionObj.DocumentTypeId == 5)
            {
                int debitAccTreeId = db.TransactionDetails.FirstOrDefault(x => x.Debit > 0 && x.TransactionId == transactionObj.Id).AccTreeId;
                int AccountSubId = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == debitAccTreeId).Id;
                var BankObj = db.BankAccounts.FirstOrDefault(x => x.AccountSubId == AccountSubId);
                ViewBag.RecipientDetails = " حساب شركة مطارات السودان المحدودة طرف  " + BankObj.AccountSub.AccountTree.AccName + " بالرقم " + BankObj.Number;
            }
            else // اذا العملية ما تحويل بنكي
            {
                // المستلمين مجموعة (الكشف المرفق)
                if (transactionObj.Recipient.Trim().Contains("كشف"))
                {
                    ViewBag.RecipientDetails = "أرقام الحسابات طرفكم حسب الكشف المرفق.";
                }
                else
                {
                    // المستلم فرد واحد
                    if (transactionObj.RecipientId > 0) // الشرط ده عشان لو المستلم عنده اكتر من حساب بنكي يجيب لي المحدد في العملية ما يجيب لي اول واحد بي
                    {
                        var Recipient = db.Recipients.FirstOrDefault(x => x.Id == transactionObj.RecipientId);
                        ViewBag.RecipientDetails = Recipient != null ?
                                                " حساب السيد / " + Recipient.RecipientName + " - طرف بنك " + Recipient.BankName + " - فرع " + Recipient.BranchName + " - بالرقم " + Recipient.AccountNumber : "";
                    }
                    else
                    {
                        var Recipient = db.Recipients.FirstOrDefault(x => x.RecipientName == transactionObj.Recipient);
                        ViewBag.RecipientDetails = Recipient != null ?
                                                " حساب السيد / " + Recipient.RecipientName + " - طرف بنك " + Recipient.BankName + " - فرع " + Recipient.BranchName + " - بالرقم " + Recipient.AccountNumber : "";
                    }
                }
            }

            // بيانات البنك الدافع او المسدد
            foreach (var item in db.TransactionDetails.Where(x => x.TransactionId == Id && x.Credit > 0).ToList())
            {
                int bankId = sh.IsItBankAccount(Id, item.Credit);
                // int bankId = sh.IsTransactionHasBankAccount(Id); // دي الاصح ال مفترض تشتغل لكن ما اتاكدت منها

                if (bankId > 0)
                {
                    var BankObj = db.BankAccounts.Find(bankId);
                    ViewBag.BankLabel = BankObj.BankLabel;
                    ViewBag.SendBank = BankObj.AccountSub.AccountTree.AccName + " بالرقم " + BankObj.Number;

                    ViewBag.FirstSignature = BankObj.FirstSignature; // التوقيع الاول
                    ViewBag.SecondSignature = BankObj.SecondSignature; // التوقيع الثاني

                    ViewBag.TotlaAmount = item.Credit.ToString();

                    BaseClass baseClass = new BaseClass();
                    string txtSumOfBous = baseClass.ChangeNumberToText(item.Credit.ToString(), 0);
                    ViewBag.TotalAmountTxt = txtSumOfBous;
                }
            }

            return View();
        }

        // طباعة خطاب البنك
        public ActionResult printBankLetterOld1(int Id)
        {
            SharedClass sh = new SharedClass();
            var transactionObj = db.Transactions.Find(Id);

            ViewBag.Id = Id;
            ViewBag.TransactionDate = transactionObj.TransactionDate.Value.ToShortDateString();

            // بيانات الحساب المدفوع له
            // اذا العملية تحويل بنكي يبقى المدفوع له حساب بنكي ايضا
            if (transactionObj.DocumentTypeId == 5)
            {
                int debitAccTreeId = db.TransactionDetails.FirstOrDefault(x => x.Debit > 0 && x.TransactionId == transactionObj.Id).AccTreeId;
                int AccountSubId = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == debitAccTreeId).Id;
                var BankObj = db.BankAccounts.FirstOrDefault(x => x.AccountSubId == AccountSubId);
                ViewBag.RecipientDetails = " حساب شركة مطارات السودان المحدودة طرف  " + BankObj.AccountSub.AccountTree.AccName + " بالرقم " + BankObj.Number;
            }
            else // اذا العملية ما تحويل بنكي
            {
                // المستلمين مجموعة (الكشف المرفق)
                if (transactionObj.Recipient.Trim().Contains("كشف"))
                {
                    ViewBag.RecipientDetails = "أرقام الحسابات طرفكم حسب الكشف المرفق.";
                }
                else
                {
                    // المستلم فرد واحد
                    if (transactionObj.RecipientId > 0) // الشرط ده عشان لو المستلم عنده اكتر من حساب بنكي يجيب لي المحدد في العملية ما يجيب لي اول واحد بي
                    {
                        var Recipient = db.Recipients.FirstOrDefault(x => x.Id == transactionObj.RecipientId);
                        ViewBag.RecipientDetails = Recipient != null ?
                                                " حساب السيد / " + Recipient.RecipientName + " - طرف بنك " + Recipient.BankName + " - فرع " + Recipient.BranchName + " - بالرقم " + Recipient.AccountNumber : "";
                    }
                    else
                    {
                        var Recipient = db.Recipients.FirstOrDefault(x => x.RecipientName == transactionObj.Recipient);
                        ViewBag.RecipientDetails = Recipient != null ?
                                                " حساب السيد / " + Recipient.RecipientName + " - طرف بنك " + Recipient.BankName + " - فرع " + Recipient.BranchName + " - بالرقم " + Recipient.AccountNumber : "";
                    }
                }
            }

            // بيانات البنك الدافع او المسدد
            var credits = db.TransactionDetails.Where(x => x.TransactionId == Id && x.Credit > 0).ToList();
            var transactionRecipientsList = db.TransactionRecipients.Where(x => x.TransactionId == Id).ToList();
            foreach (var item in credits)
            {
                int bankId = sh.IsItBankAccount(Id, item.Credit);
                // int bankId = sh.IsTransactionHasBankAccount(Id); // دي الاصح ال مفترض تشتغل لكن ما اتاكدت منها

                if (bankId > 0)
                {

                    var transactionRecipientsLetters = transactionRecipientsList.Where(q => q.BankAccountId == bankId).ToList();

                    decimal? bankAmount = 0;

                    if (transactionRecipientsLetters.Any())
                    {
                        bankAmount = transactionRecipientsLetters.Sum(q => q.Amount);
                    }

                    var BankObj = db.BankAccounts.Find(bankId);
                    ViewBag.BankLabel = BankObj.BankLabel;
                    ViewBag.SendBank = BankObj.AccountSub.AccountTree.AccName + " بالرقم " + BankObj.Number;

                    ViewBag.FirstSignature = BankObj.FirstSignature; // التوقيع الاول
                    ViewBag.SecondSignature = BankObj.SecondSignature; // التوقيع الثاني

                    decimal? amount = item.Credit - bankAmount;

                    ViewBag.TotlaAmount = amount.ToString();

                    BaseClass baseClass = new BaseClass();
                    string txtSumOfBous = baseClass.ChangeNumberToText(amount.ToString(), 0);
                    ViewBag.TotalAmountTxt = txtSumOfBous;
                }
            }

            return View();
        }

        public ActionResult printBankLetter(int Id, int? bankAccountId = null)
        {
            SharedClass sh = new SharedClass();
            var transactionObj = db.Transactions.Find(Id);
            string pattren = @"^\d{10,}$"; // بالنسبة لي رقم فاتورة ايصالي

            ViewBag.Id = Id;
            var transactionDate = transactionObj.TransactionDate.Value;
            ViewBag.TransactionDate = $"{transactionDate.Day}-{transactionDate.Month}-{transactionDate.Year}";

            // بيانات الحساب المدفوع له
            // اذا العملية تحويل بنكي يبقى المدفوع له حساب بنكي ايضا
            if (transactionObj.DocumentTypeId == 5)
            {
                int debitAccTreeId = db.TransactionDetails.FirstOrDefault(x => x.Debit > 0 && x.TransactionId == transactionObj.Id).AccTreeId;
                int AccountSubId = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == debitAccTreeId).Id;
                var BankObj = db.BankAccounts.FirstOrDefault(x => x.AccountSubId == AccountSubId);
                ViewBag.RecipientDetails = " حساب شركة مطارات السودان المحدودة طرف  " + BankObj.AccountSub.AccountTree.AccName + " بالرقم " + BankObj.Number;
            }
            else // اذا العملية ما تحويل بنكي
            {
                // المستلمين مجموعة (الكشف المرفق)
                if (transactionObj.Recipient.Trim().Contains("كشف"))
                {
                    ViewBag.RecipientDetails = "أرقام الحسابات طرفكم حسب الكشف المرفق.";
                }
                else
                {
                    // المستلم فرد واحد
                    if (transactionObj.RecipientId > 0) // الشرط ده عشان لو المستلم عنده اكتر من حساب بنكي يجيب لي المحدد في العملية ما يجيب لي اول واحد بي
                    {
                        var Recipient = db.Recipients.FirstOrDefault(x => x.Id == transactionObj.RecipientId);
                        ViewBag.RecipientDetails = Recipient != null ?
                                                " حساب السيد / " + Recipient.RecipientName + " - طرف بنك " + Recipient.BankName + " - فرع " + Recipient.BranchName + " - بالرقم " + Recipient.AccountNumber : "";
                    }
                    else if (Regex.IsMatch(transactionObj.Recipient.Trim(), pattren))
                    {
                        ViewBag.RecipientDetails = "نظام الدفع الالكتروني(إيصالي) برقم الفاتورة " + transactionObj.Recipient.Trim();
                    }
                    else
                    {
                        var Recipient = db.Recipients.FirstOrDefault(x => x.RecipientName == transactionObj.Recipient);
                        ViewBag.RecipientDetails = Recipient != null ?
                                                " حساب السيد / " + Recipient.RecipientName + " - طرف بنك " + Recipient.BankName + " - فرع " + Recipient.BranchName + " - بالرقم " + Recipient.AccountNumber : "";
                    }
                }
            }

            // بيانات البنك الدافع او المسدد
            var credits = db.TransactionDetails.Where(x => x.TransactionId == Id && x.Credit > 0).ToList();
            var transactionRecipientsList = db.TransactionRecipients.Where(x => x.TransactionId == Id).ToList();

            foreach (var item in credits)
            {
               //int bankId = sh.IsItBankAccount(Id, item.Credit);

                 int bankId = sh.IsTransactionHasBankAccount(Id); // دي الاصح ال مفترض تشتغل لكن ما اتاكدت منها

                //if (bankId > 0)
                decimal? bankAmount = 0;
                if (bankId == bankAccountId)
                {
                    
                   //if (bankId == bankAccountId)
                    //{
                        var transactionRecipientsLetters = transactionRecipientsList.Where(q => q.BankAccountId == bankId).ToList();

                        if (transactionRecipientsLetters.Any())
                        {
                            bankAmount = transactionRecipientsLetters.Sum(q => q.Amount);
                        }
                    }

                    var BankObj = db.BankAccounts.Find(bankId);
                    ViewBag.BankLabel = BankObj.BankLabel;
                    ViewBag.SendBank = BankObj.AccountSub.AccountTree.AccName + " بالرقم " + BankObj.Number;

                    ViewBag.FirstSignature = BankObj.FirstSignature; // التوقيع الاول
                    ViewBag.SecondSignature = BankObj.SecondSignature; // التوقيع الثاني

                    decimal? amount = item.Credit - bankAmount;

                    ViewBag.TotlaAmount = amount.ToString();
                    ViewBag.CurrencyType = transactionObj.CurrencyType.Name;

                    BaseClass baseClass = new BaseClass();
                    string txtSumOfBous = baseClass.ChangeNumberToText(amount.ToString(), transactionObj.CurrencyId-1??1);
                    ViewBag.TotalAmountTxt = txtSumOfBous;
                }
           // }

            return View();
        }

        // طباعة خطاب لبينك لكل مستلم علي حدى
        [HttpPost]
        public ActionResult printRecipientBankLetter(int Id, int TransactionId, decimal Amount, string RecipientName = null, string BankName = null, string BranchName = null, string AccountNumber = null)
        {
            SharedClass sh = new SharedClass();
            var transactionRecipientsObj = db.TransactionRecipients.Find(Id);
            var BankAccountObj = new BankAccount();

            if (transactionRecipientsObj == null)
            {
                var transactionObj = db.Transactions.Find(TransactionId);

                TransactionRecipient obj = new TransactionRecipient();
                obj.TransactionId = TransactionId;
                obj.RecipientName = transactionObj.Recipient;
                obj.Amount = Amount;

                int bankAccountId = sh.IsTransactionHasBankAccount(TransactionId);

                if (transactionObj.RecipientId > 0)
                {
                    obj.BankName = transactionObj.Recipient1.BankName;
                    obj.BranchName = transactionObj.Recipient1.BranchName;
                    obj.AccountNumber = transactionObj.Recipient1.AccountNumber;
                    obj.BankAccountId = bankAccountId;
                }

                db.TransactionRecipients.Add(obj);
                db.SaveChanges();

                transactionRecipientsObj = obj;
                BankAccountObj = db.BankAccounts.Where(q => q.Id == bankAccountId)
                    .Include(q => q.AccountSub)
                    .Include(q => q.AccountSub.AccountTree)
                    .Include(q => q.CurrencyType)
                    .FirstOrDefault();
            }

            ViewBag.Id = TransactionId;

            // المستلم فرد واحد
            if (transactionRecipientsObj != null)
            {
                ViewBag.RecipientDetails =
                                            " حساب السيد / " + transactionRecipientsObj.RecipientName +
                                            " - طرف بنك " + transactionRecipientsObj.BankName +
                                            " - فرع " + transactionRecipientsObj.BranchName +
                                            " - بالرقم " + transactionRecipientsObj.AccountNumber;
            }

            ViewBag.BankLabel = BankAccountObj.BankLabel;
            ViewBag.SendBank = BankAccountObj.AccountSub.AccountTree.AccName +
                               " بالرقم " + BankAccountObj.Number;
            ViewBag.FirstSignature = BankAccountObj.FirstSignature; // التوقيع الاول
            ViewBag.SecondSignature = BankAccountObj.SecondSignature; // التوقيع الثاني
            ViewBag.TotlaAmount = BankAccountObj.ToString();

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(BankAccountObj.ToString(), 0);
            ViewBag.TotalAmountTxt = txtSumOfBous;

            return View();
        }

        public ActionResult printRecipientBankLetter(int Id)
        {
            SharedClass sh = new SharedClass();
            var transactionRecipientsObj = db.TransactionRecipients.Find(Id);

            ViewBag.Id = Id;

            // المستلم فرد واحد
            if (transactionRecipientsObj != null)
            {
                ViewBag.RecipientDetails =
                                            " حساب السيد / " + transactionRecipientsObj.RecipientName +
                                            " - طرف بنك " + transactionRecipientsObj.BankName +
                                            " - فرع " + transactionRecipientsObj.BranchName +
                                            " - بالرقم " + transactionRecipientsObj.AccountNumber;
            }

            ViewBag.BankLabel = transactionRecipientsObj.BankAccount.BankLabel;
            ViewBag.SendBank = transactionRecipientsObj.BankAccount.AccountSub.AccountTree.AccName +
                               " بالرقم " + transactionRecipientsObj.BankAccount.Number;
            ViewBag.FirstSignature = transactionRecipientsObj.BankAccount.FirstSignature; // التوقيع الاول
            ViewBag.SecondSignature = transactionRecipientsObj.BankAccount.SecondSignature; // التوقيع الثاني
            ViewBag.TotlaAmount = transactionRecipientsObj.Amount.ToString();

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(transactionRecipientsObj.Amount.ToString(), 0);
            ViewBag.TotalAmountTxt = txtSumOfBous;

            return View();
        }

        public ActionResult printBalancesReportSummary(DateTime? DateFrom, DateTime? DateTo)
        {
            Dictionary<int, string> parentIds = new Dictionary<int, string>() { { 3, "جملة الايرادات" }, { 245, "الفصل الاول" }, { 13, "الفصل الثاني/ الاداري" },
                { 12, "الفصل الثاني/ التشغيلي" },{10001, "جملة الفصل الثاني" }, {10002, "الاستخدامات(الاول + الثاني)" }, {10003, "جملة الفائض(الايرادات - الاستخدامات)" },
                { 172, "المصروفات الراسمالية" },{ 235, "مصروفات التنمية" }, {10004, "جملة الراسمالية والتنمية" },  {10005, "الفائض(اجمالي الفائض - جملة الراسمالية والتنمية)" },
                {10006, "الربط السنوي" }};

            List<SummaryVM> result = new List<SummaryVM>();

            SharedClass sc = new SharedClass();
            if (DateFrom != null && DateTo != null)
            {
                ViewBag.DateFrom = DateFrom.Value.ToShortDateString();
                ViewBag.DateTo = DateTo.Value.ToShortDateString();
            }
            var userId = User.Identity.GetUserId();
            int CurrentFinancialCycleId = sc.GetUserCurrentFinancialCycleId(userId);
            var CurrentFinancialCycle = db.FinancialCycles.Find(CurrentFinancialCycleId).Year;
            ViewBag.CurrentFinancialCycle = CurrentFinancialCycle;
            foreach (var item in parentIds.Where(q => q.Key < 10000))
            {
                if (item.Key == 3 || item.Key == 235)
                {
                    var valRev = reportObject.BalancePositionRevenue(CurrentFinancialCycleId, item.Key, DateFrom, DateTo);
                    var objRev = new SummaryVM(item.Key, item.Value, valRev.Sum(q => q.actualExchange) ?? 0);
                    result.Add(objRev);
                    continue;
                }
                else
                {
                    var val = reportObject.BalancePosition(CurrentFinancialCycleId, item.Key, DateFrom, DateTo);
                    var obj = new SummaryVM(item.Key, item.Value, val.Sum(q => q.actualExchange) ?? 0);
                    result.Add(obj);
                }
            }
            return View(result);
            //return Json(result, JsonRequestBehavior.AllowGet);
        }

        // 757770
        public ActionResult PrintCovenantLetter(decimal? debit, decimal? credit, int accTreeId, decimal diff, string note)
        {
            //var AccName = db.AccountTrees.Find(accTreeId).AccName;
            ViewBag.Debit = debit;
            ViewBag.credit = credit;
            ViewBag.accTreeId = accTreeId;
            ViewBag.diff = diff;
            ViewBag.AccName = note;
            ViewBag.note = note;

            BaseClass baseClass = new BaseClass();
            string txtSumOfBous = baseClass.ChangeNumberToText(diff.ToString(), 0);
            ViewBag.TotalAmountTxt = txtSumOfBous;

            return View();
        }
  
    }
}