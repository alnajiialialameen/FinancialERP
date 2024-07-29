using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Purchases.Models;

namespace Purchases.Controllers
{
    public class SearchOrdersController : Controller
    {
        Entities db = new Entities();  
        // GET: SearchOrders
        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult LoadData(int ?department ,  DateTime ? orderdate , string description)
        {
            //DateTime date = Convert.ToDateTime(orderdate);
            IQueryable<Order> orders = db.Orders;

            if(department != null)
            {
                orders = orders.Where(f => f.DepartmentId == department);
            }
            if (description != null)
            {
                orders = orders.Where(f => f.Description.Contains(description));
            }
            if (orderdate != null)
            {
                orders = orders.Where(f => f.OrderDate.Value.Year == orderdate.Value.Year & f.OrderDate.Value.Month == orderdate.Value.Month & f.OrderDate.Value.Day == orderdate.Value.Day);
            }

            var data = orders.Select(p => new
            {
                Id = p.Id,
                Department = p.DepartmentName,
                DepartmentId = p.DepartmentId,
                OrderDate = p.OrderDate.Value.Day + "/" + p.OrderDate.Value.Month + "/" + p.OrderDate.Value.Year,
                Description = p.Description,
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
    }
}