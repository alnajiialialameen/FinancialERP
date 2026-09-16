using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Purchases.Models;
using Purchases.CurencyOperation;
using Purchases.MyLogic;
using Microsoft.AspNet.Identity;

namespace Purchases.Controllers
{
    public class HomeController : Controller
    {
        DateTime now = DateTime.Now;
        Entities db = new Entities();

        public ActionResult Index()
        {
            //الصنف الاكثر طلبا
            ViewBag.RushItem =  db.OrderDetiails.GroupBy(f => f.ItemDetail).OrderByDescending(a => a.Count()).First().Select(f=>f.ItemDetail.Name).FirstOrDefault();
            ViewBag.RushItemCount =  db.OrderDetiails.GroupBy(f => f.ItemDetail).OrderByDescending(a => a.Count()).First().Sum(f=>f.RequierCount);

            //  الصنف الاكثر طلبا ف الشهر الحالي
            //ViewBag.RushItemThisMonth = db.OrderDetiails.Where(f=>f.Order.OrderDate.Value.Month == now.Month & f.Order.OrderDate.Value.Year == now.Year).GroupBy(f => f.ItemDetail).OrderByDescending(a => a.Count()).First().Select(f => f.ItemDetail.Name).FirstOrDefault();
            //ViewBag.RushItemCountThisMonth = db.OrderDetiails.Where(f => f.Order.OrderDate.Value.Month == now.Month & f.Order.OrderDate.Value.Year == now.Year).GroupBy(f => f.ItemDetail).OrderByDescending(a => a.Count()).First().Sum(f => f.RequierCount);


            //الادارة الاكثر طلبا
            ViewBag.RushDepartment = db.Orders.GroupBy(f => f.DepartmentId).OrderByDescending(f => f.Count()).FirstOrDefault().Select(f => f.DepartmentName).FirstOrDefault();
            ViewBag.RushDepartmentCount = db.Orders.GroupBy(f => f.DepartmentId).OrderByDescending(f => f.Count()).FirstOrDefault().Count();


            //الادارة الاكثر طلبا في الشهر الحالي
            //ViewBag.RushDepartmentThisMonth = db.Orders.Where(f => f.OrderDate.Value.Month == now.Month & f.OrderDate.Value.Year == now.Year).GroupBy(f => f.DepartmentId).OrderByDescending(f => f.Count()).FirstOrDefault().Select(f => f.DepartmentName).FirstOrDefault();
            //ViewBag.RushDepartmentCountThisMonth = db.Orders.Where(f => f.OrderDate.Value.Month == now.Month & f.OrderDate.Value.Year == now.Year).GroupBy(f => f.DepartmentId).OrderByDescending(f => f.Count()).FirstOrDefault().Count();



            //طلبيات الشهر الحالي
            ViewBag.ThisMonthOrder = db.Orders.Where(f=>f.OrderDate.Value.Month == now.Month & f.OrderDate.Value.Year == now.Year).Count();





            //Chart 

            //مقارنة بين اكثر الادارات طلبا






            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult GetNotifications()
        {
            var shared = new SharedClass();
            var userId = User.Identity.GetUserId();
            var data = shared.GetNotifications(userId);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        //الدالة دي برسل ليها المبلغ بتحول لي نص(التفقيطة)
        public ActionResult ConvertToText(string amount)
        {
            BaseClass b = new BaseClass();

            string textAmount = b.ChangeNumberToText(amount.ToString(), 0);

            return Json(textAmount, JsonRequestBehavior.AllowGet);
        }

    }
}