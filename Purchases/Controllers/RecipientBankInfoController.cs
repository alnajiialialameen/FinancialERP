using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Purchases.Models;
using System.Data.Entity;
using Purchases.Models.ViewModal;
using Purchases.MyLogic;

namespace Purchases.Controllers
{
    public class RecipientBankInfoController : Controller
    {
        Entities db = new Entities();
        TreeClass treecls = new TreeClass();

        // GET: RecipientBankInfo
        public ActionResult Index()
        {
            var data = db.Recipients.ToList();
            return View();
        }

        public ActionResult LoadData()
        {
            var data = db.Recipients.ToList();

            return Json(data, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetBanks(string q)
        {
            var data = db.Recipients
                        .Select(p => new {text = p.BankName }).Distinct().Where(f => f.text.Contains(q)).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetBranches(string q)
        {
            var data = db.Recipients
                        .Select(p => new { text = p.BranchName }).Distinct().Where(f => f.text.Contains(q)).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);

        }
    }
}