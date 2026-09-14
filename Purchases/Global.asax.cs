using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Purchases
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        //protected void Session_Start(object sender, EventArgs e)
        //{
        //    // يمكن تستخدم Session_Start لو حابب تهيّئ بيانات معينة
        //}

        //protected void Application_AcquireRequestState(object sender, EventArgs e)
        //{
        //    // لو المستخدم غير مسجّل الدخول (FormsAuth cookie انتهت)
        //    if (HttpContext.Current.Session != null &&
        //        !HttpContext.Current.Request.Url.AbsolutePath.Contains("/Account/Login"))
        //    {
        //        if (HttpContext.Current.User == null || !HttpContext.Current.User.Identity.IsAuthenticated)
        //        {
        //            HttpContext.Current.Response.Redirect("~/Account/Login");
        //        }
        //    }
        //}
    }
}
