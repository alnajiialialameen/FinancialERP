namespace Purchases.Models.ViewModel
{
    public enum ConstValEnum
    {
        TaxId = 228, // رقم الضريبة في الشجرة المحاسبية
        TaxRevenueId = 229, // رقم ضريبة الايرادات 5% في الشجرة المحاسبية
        AddTaxId = 0,// رقم الضريبة المضافة في الشجرة المحاسبية
        AddTaxVal = 17,// قيمة الضريبة المضافة
        StampId = 244, // رقم الدمغة في الشجرة المحاسبية
        IncomeTaxId = 317, // رقم ضؤيبة الدخل في الشجرة المحاسبية
        DebtorsId = 9, // المدينون
        CreditorsId = 10, // الدائنون
        BanksId = 7, // البنوك
        RevenueId = 3, // الايرادات
        CheckTypeOut = 1, // نوع الشيك صادر
        CheckTypeIn = 2, // نوع الشيك وارد
        UserChargeFeesId = 291, // بند رسوم الملاحة الجوية بتخش في الفاتورة لكن هي تبع الامانات

    }
}