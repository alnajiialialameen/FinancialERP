using FuzzySharp;
using Microsoft.AspNet.Identity;
using PagedList;
using Purchases.CurencyOperation;
using Purchases.Models;
using Purchases.Models.ViewModal;
using Purchases.Models.ViewModel;
using Purchases.MyLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class SharedController : Controller
    {
        private readonly Entities db;
        private SharedClass obj;
        private TreeClass tre;

        // constructor
        public SharedController()
        {
            db = new Entities();
            obj = new SharedClass();
            tre = new TreeClass();
        }

        // جلب جميع الحسابات في الشجرة
        public ActionResult getAccTrees(string q)
        {
            var data = obj.GetAccTrees(q);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // جلب جميع الحسابات في الشجرة
        public ActionResult getAccTreesAll(string q)
        {
            var data = obj.GetAccTreesAll(q);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // جلب جميع بنود الموازنة
        public ActionResult getBalanceIds(string q)
        {
            var userId = User.Identity.GetUserId();
            var data = obj.getBalanceIds(q, userId);

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // جلب جميع العملات
        public ActionResult getCurrency(string q)
        {
            var data = obj.GetCurrency(q);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // جلب جميع العملات
        public ActionResult getDocumentTypes(string q)
        {
            var data = obj.GetDocumentType(q);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        //جلب جميع الدائنون
        public ActionResult getAllCreditors(string q)
        {
            TreeClass tr = new TreeClass();
            int CreditorsId = Convert.ToInt32(ConstValEnum.CreditorsId);
            var data = tr.getAllItemsByParentId(CreditorsId).Where(x => db.AccountSubs.Any(b => b.AccTreeId == x)).Select(p =>
                new
                {
                    id = p,
                    text = db.AccountTrees.Find(p).AccName
                }).Where(p => p.text.Contains(q));

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // جلب جميع المدينون
        public ActionResult getAllDebtors(string q)
        {
            TreeClass tr = new TreeClass();

            int DebtorsId = Convert.ToInt32(ConstValEnum.DebtorsId);
            var data = tr.getAllItemsByParentId(DebtorsId).Where(x => db.AccountSubs.Any(b => b.AccTreeId == x)).Select(p =>
                new
                {
                    id = p,
                    text = db.AccountTrees.Find(p).AccName
                }).Where(p => p.text.Contains(q));

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // جلب جميع البنوك
        public ActionResult getAllBanks(string q)
        {
            TreeClass tr = new TreeClass();
            int BanksId = Convert.ToInt32(ConstValEnum.BanksId);
            var data = tr.getAllItemsByParentId(BanksId).Select(p =>
                new
                {
                    id = p,
                    text = db.AccountTrees.Find(p).AccName
                }).Where(p => p.text.Contains(q));

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // جلب جميع بنود الايرادات
        public ActionResult getAllRevenue(string q)
        {
            TreeClass tr = new TreeClass();
            int RevenueId = Convert.ToInt32(ConstValEnum.RevenueId);
            var data = tr.getAllItemsByParentId(RevenueId).Where(x => db.Balances.Any(b => b.AccountTreeId == x)).Select(p =>
                     new
                     {
                         id = p,
                         text = db.AccountTrees.Find(p).AccName
                     }).ToList();

            // اضافة ضريبة ال 5% عشان تظهر معاي في الخيارات
            int TaxRevenueId = Convert.ToInt32(ConstValEnum.TaxRevenueId);
            var AccName = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == TaxRevenueId).AccountTree.AccName;
            data.Add(new
            {
                id = TaxRevenueId,
                text = AccName,
            });

            // اضافة رسوم الملاحة الجوية عشان تظهر معاي في الخيارات
            int UserChargeFeesId = Convert.ToInt32(ConstValEnum.UserChargeFeesId);
            var UserChargeAccName = db.AccountSubs.FirstOrDefault(x => x.AccTreeId == UserChargeFeesId).AccountTree.AccName;
            data.Add(new
            {
                id = UserChargeFeesId,
                text = UserChargeAccName,
            });

            data = data.Where(p => p.text.Contains(q)).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // لمن اختار البنك يرجع لي نوع العملة
        public ActionResult getCurrencyTypeId(int bankId)
        {
            // الدالة دي عشان الموظف لمن يختار البنك ما يختار تاني العملة - يجلب العملة مباشرة


            int CurrenyTypeId = obj.GetCurrencyId(bankId);

            return Json(CurrenyTypeId, JsonRequestBehavior.AllowGet);
        }

        //جلب جميع الدائنون لازالة الامانات
        public ActionResult getAllDepositsCreditor(string q)
        {
            TreeClass tr = new TreeClass();
            int CreditorsId = Convert.ToInt32(ConstValEnum.CreditorsId);
            var data = tr.getAllItemsByParentId(CreditorsId).Where(x => db.AccountSubs.Any(b => b.AccTreeId == x)).Select(p =>
                new
                {
                    id = p,
                    text = db.AccountTrees.Find(p).AccName
                }).ToList();

            // رسوم خدمات الركاب فيها امانات تبع شركة رايت لازم اظهرها عشان اقدر ادخل بيها--- ح اضيفها تحت هنا

            var CUPPSObj = db.AccountTrees.Find(114);
            data.Add(
                new
                {
                    id = CUPPSObj.Id,
                    text = CUPPSObj.AccName
                });

            data = data.Where(p => p.text.Contains(q)).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        // جلب جميع المستلمين(دي القديمة)
        public ActionResult getAllRecipientOld(string q)
        {
            ConsumeHRAPI obj = new ConsumeHRAPI();
            // جلب كل الموظفين
            var AllEmployees = obj.getAllEmployees();

            // تصفية الموظفين بناءً على قيمة البحث
            var empoList = AllEmployees
                .Where(x => string.IsNullOrEmpty(q) || x.Name.Contains(q))
                .Select(x => new { text = x.Name })
                .ToList();

            // جلب كل المستلمين غير الموجودين ضمن قائمة الموظفين وتصفية النتائج بناءً على قيمة البحث
            var list1 = db.Transactions
                .Where(x => x.Recipient != null && (string.IsNullOrEmpty(q) || x.Recipient.Contains(q)))
                .Select(x => new { text = x.Recipient })
                .Distinct()
                .ToList();

            // دمج القائمتين معاً بدون تكرار
            var combinedList = empoList.Union(list1).ToList();

            return Json(combinedList, JsonRequestBehavior.AllowGet);
        }

        // جلب جميع المستلمين
        // دي كانت شغالة لكن اتعدلت تحت
        public ActionResult getAllRecipient2(string q)
        {
            var recipientsList = db.Recipients.ToList();

            // تصفية الموظفين بناءً على قيمة البحث
            var empoList = recipientsList.Where(x => string.IsNullOrEmpty(q) || x.RecipientName.Contains(q))
                .Select(x => new { text = x.RecipientName + " - " + x.BankName.Split(' ')[0] })
                .ToList();

            // جلب كل المستلمين غير الموجودين ضمن قائمة الموظفين وتصفية النتائج بناءً على قيمة البحث
            var list1 = db.Transactions
                .Where(x => x.Recipient != null && (string.IsNullOrEmpty(q) || x.Recipient.Contains(q)))
                .Select(x => new { text = x.Recipient })
                .Distinct()
                .ToList();

            // دمج القائمتين معاً بدون تكرار
            var combinedList = empoList.Union(list1).ToList();

            return Json(combinedList, JsonRequestBehavior.AllowGet);
        }

        //public List<string> Main(List<string> list1, List<string> list2)
        //{
        //    // القائمتين تحتويان على أسماء مكونة من أجزاء
        //    //List<string> list1 = new List<string> { "أحمد آدم إبراهيم", "محمد عثمان علي", "طه أحمد علي" };
        //    //List<string> list2 = new List<string> { "احمد ادم ابراهيم", "محمد عثمان على", "محمود أحمد حسن" };

        //    List<string> listResult = new List<string>();

        //    // تطبيع الحروف العربية للتخلص من الهمزة والياء
        //    list1 = list1.Select(NormalizeArabic).ToList();
        //    list2 = list2.Select(NormalizeArabic).ToList();

        //    // مقارنة الأسماء المكونة من أجزاء بين القائمتين
        //    foreach (var item1 in list1)
        //    {
        //        //Console.WriteLine($"النتائج المشابهة ل {item1}:");

        //        var similarNames = list2
        //            .Where(item2 => !AreNamesSimilar(item1, item2)) // استبعاد الأسماء المتشابهة
        //            .ToList();

        //        // عرض النتائج المتبقية
        //        foreach (var result in similarNames)
        //        {
        //            listResult.Add(result);
        //        }

        //    }

        //    return listResult;
        //}

        // دالة لتطبيع الحروف العربية (إزالة الهمزات واستبدال الياء)
        static string NormalizeArabic(string text)
        {
            return text
                .Replace("أ", "ا")
                .Replace("إ", "ا")
                .Replace("آ", "ا")
                .Replace("ى", "ي")
                .Replace("ء", "")
                .Replace("ئ", "ي")
                .Replace("ؤ", "و");
        }

        // دالة للتحقق مما إذا كانت الأسماء متشابهة
        static bool AreNamesSimilar(string name1, string name2)
        {
            // تقسيم الأسماء إلى أجزائها
            var parts1 = name1.Split(' ');
            var parts2 = name2.Split(' ');

            // مقارنة كل جزء من الاسم الأول مع كل جزء من الاسم الثاني
            foreach (var part1 in parts1)
            {
                foreach (var part2 in parts2)
                {
                    if (Fuzz.TokenSortRatio(part1, part2) > 90) // إذا كانت النسبة أكبر من 90%، نعتبرها متطابقة
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        // Shared/Create
        [HttpPost]
        public ActionResult CreateOriginal(List<TransactionVM> modal)
        {
            try
            {
                int? checkNo = modal.FirstOrDefault().CheckNo;
                //int CheckType = modal.FirstOrDefault().documentTypeId == 2 ? Convert.ToInt32(ConstValEnum.CheckTypeIn): Convert.ToInt32(ConstValEnum.CheckTypeOut); // نوع الشيك انه يكون صادر ولا وارد
                int? CheckType = modal.FirstOrDefault().CheckType;
                int CurrentCheckNo = modal.FirstOrDefault().CheckNo != null ? Convert.ToInt32(modal.FirstOrDefault().CheckNo) : 0;

                // نختبر هل الشيك مدخل قبل كده ولا لا
                // اذا تم الحفظ بي نجاح يبقى نضيف في الشيكات
                if (checkNo == null | !db.PrintChecks.Any(x => x.CheckNo == CurrentCheckNo & x.CheckType == CheckType & x.CheckNo != null))
                {
                    var userid = User.Identity.GetUserId();
                    int res = obj.Create(modal, userid);

                    if (res == -1000)
                    {
                        return Json(new { Message = "العام المالي مقفول‘ لا يمكن اجراء اي اعملية", Title = "خطأ", Status = "error" });
                    }

                    if (res > 0)
                    {
                        PrintCheckObj printCheckObj = new PrintCheckObj();
                        BaseClass baseClass = new BaseClass();
                        var TransactionDate = modal[0].transactionDate;
                        string textAmount = baseClass.ChangeNumberToText(modal.Sum(x => x.debit).ToString(), 0);
                        string recipient = modal.FirstOrDefault().recipient;
                        int bankId = obj.IsTransactionHasBankAccount(res);

                        if (CurrentCheckNo > 0 && bankId > 0)
                        {
                            //الاضافة في الشيكات
                            int res1 = printCheckObj.SaveData(res, TransactionDate.Value.ToShortDateString(), res, modal.Sum(x => x.debit).ToString(),
                                                              textAmount, CurrentCheckNo.ToString(), recipient, userid);

                            if (res1 > 0)
                            {
                                return Json(new { Message = " تمت عملية الحفظ بنجاح ", Title = "نجاح", Status = "success" });
                            }
                            else
                            {
                                return Json(new { Message = "تم الحفظ بنجاح - لكن بيانات الشيك غير محفوظة", Title = "تنبيه", Status = "warning" });
                            }
                        }

                        return Json(new { Message = " تمت عملية الحفظ بنجاح ", Title = "نجاح", Status = "success" });
                    }
                    else
                    {
                        return Json(new { Message = " عفوا لم تتم عملية الحفظ بنجاح ", Title = "خطأ", Status = "error" });
                    }
                }
                else
                {
                    return Json(new { Message = " عفوا يوجد شيك صادر بهذا الرقم ", Title = "خطأ", Status = "error" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = " عفوا حدث خطأ اثناء العملية (Exception) ", Title = "خطأ", Status = "error" });
            }
        }


        // جلب جميع الحسابات في الشجرة
        public ActionResult loadData()
        {
            var userId = User.Identity.GetUserId();
            var data = obj.LoadData(userId);
            return new JsonResult { Data = data, MaxJsonLength = 50000000, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        // Shared/Create
        [HttpPost]
        public ActionResult Create(List<TransactionVM> modal)
        {
            try
            {
                var userid = User.Identity.GetUserId();
                int res = obj.Create(modal, userid);

                if (res == -1000)
                {
                    return Json(new { Message = "العام المالي مقفول او غير موجود‘ لا يمكن اجراء اي اعملية", Title = "خطأ", Status = "error" });
                }

                if (res > 0)
                {
                    return Json(new { Message = " تمت عملية الحفظ بنجاح ", Title = "نجاح", Status = "success" });
                }
                else
                {
                    return Json(new { Message = " عفوا لم تتم عملية الحفظ بنجاح ", Title = "خطأ", Status = "error" });
                }
            }
            catch (Exception e)
            {
                return Json(new { Message = " عفوا حدث خطأ اثناء العملية (Exception) ", Title = "خطأ", Status = "error" });
            }
        }

        // دي اخر واحده شغالة
        public ActionResult getAllRecipientNew(string q)
        {
            var recipientsList = db.Recipients.ToList();

            // قائمة الموظفين الرسمية (بالشكل الكامل: "الاسم - البنك")
            var empListWithBankName = recipientsList
                .Where(x => string.IsNullOrEmpty(q) || x.RecipientName.Contains(q))
                .Select(x => new { id = x.Id, text = x.RecipientName + " - " + x.BankName.Split(' ')[0] })
                .ToList();

            //// قائمة المستلمين من Transactions (أسماء فقط، بدون بنك)
            //var transactionsRecipients = db.Transactions
            //    .Where(x => x.Recipient != null && (string.IsNullOrEmpty(q) || x.Recipient.Contains(q)))
            //    .Select(x => new { id = x.RecipientId??0, text = x.Recipient.Trim()})
            //    .Distinct()
            //    .ToList();

            //// استبعاد أي اسم موجود في قائمة الموظفين
            //var filteredRecipients = transactionsRecipients
            //    .Where(recipient => !empListWithBankName.Any(r=> r.text.Split('-')[0].Trim() == recipient.text))
            //    .Select(x => new { id = x.id, text= x.text})
            //    .ToList();

            //// دمج القائمتين
            //var combinedList = empListWithBankName.Union(filteredRecipients).Select(qq => new {id=qq.id, text = qq.text }).ToList();


            return Json(empListWithBankName, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getAllRecipient(string q)
        {
            string pattren = @"^\d{10,}$"; // بالنسبة لي رقم فاتورة ايصالي
            var recipientsList = db.Recipients.ToList();

            // قائمة الموظفين الرسمية (بالشكل الكامل: "الاسم - البنك")
            var empListWithBankName = recipientsList
                .Where(x => string.IsNullOrEmpty(q) || x.RecipientName.Contains(q))
                .Select(x => new { id = x.Id, text = x.RecipientName + " - " + x.BankName.Split(' ')[0] })
                .ToList();

            // قائمة المستلمين من Transactions (أسماء فقط، بدون بنك)
            var transactionsRecipients = db.Transactions
                .Where(x => x.Recipient != null && (string.IsNullOrEmpty(q) || x.Recipient.Contains(q)))
                .Select(x => new { id = x.RecipientId ?? 0, text = x.Recipient.Trim() })
                .Distinct()
                .ToList();

            // اذا فيهم رقم فاتورة ما تظهره مع الخيارات عشان ما يتم اختياره بالغلط لانه رقم الفاتورة تبع ايصالي مفترض ما يتكرر
            transactionsRecipients = transactionsRecipients.Where(x => !Regex.IsMatch(x.text, pattren)).ToList();


            // استبعاد أي اسم موجود في قائمة الموظفين
            var filteredRecipients = transactionsRecipients
                .Where(recipient => !empListWithBankName.Any(r => recipient.text.Contains(r.text.Split('-')[0].Trim())))
                .Select(x => x)
                .ToList();

            transactionsRecipients = transactionsRecipients.Select(t => new { id = t.id, text = NormalizeArabicName(t.text) }).ToList();
            empListWithBankName = empListWithBankName.Select(r => new { id = r.id, text = NormalizeArabicName(r.text) }).ToList();

            //var missingInRecipients = transactionsRecipients
            //    .Where(t => !empListWithBankName.Any(r=> r.text == t.text))
            //    .Distinct()
            //    .ToList();

            // دمج القائمتين
            var combinedList = empListWithBankName.Union(filteredRecipients).Distinct().ToList();



            return Json(combinedList, JsonRequestBehavior.AllowGet);
        }


        public ActionResult UpdateRecipientsNames()
        {
            var recipientsList = db.Recipients.ToList();

            foreach (var item in recipientsList)
            {
                item.RecipientName = NormalizeArabicName(item.RecipientName);
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateTransactionsNames()
        {
            var transactionsList = db.Transactions.ToList();

            foreach (var item in transactionsList)
            {
                item.Recipient = NormalizeArabicName(item.Recipient);
                db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }

            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public static string NormalizeArabicName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            var normalized = name.Trim();

            // توحيد الحروف
            normalized = normalized.Replace("أ", "ا")
                                   .Replace("إ", "ا")
                                   .Replace("آ", "ا")
                                   .Replace("ى", "ي")
                                   //.Replace("ئ", "ي")
                                   //.Replace("ؤ", "و")
                                   .Replace("ة", "ه");

            // إزالة التشكيل
            string diacritics = new string[] {
        "\u064B", // tanwin fath
        "\u064C", // tanwin damm
        "\u064D", // tanwin kasr
        "\u064E", // fatha
        "\u064F", // damma
        "\u0650", // kasra
        "\u0651", // shadda
        "\u0652"  // sukun
    }.Aggregate(normalized, (current, d) => current.Replace(d, ""));

            // إزالة المسافات الزائدة بين الكلمات
            normalized = Regex.Replace(diacritics, @"\s+", " ");

            return normalized;
        }

        public ActionResult GetDepartment(string q)
        {
            ConsumeHRAPI api = new ConsumeHRAPI();

            var data = api.getDepartments().Result.ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

    }
}