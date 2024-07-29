using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Purchases.Models;

namespace Purchases.Controllers
{
    public class CurrencyDetailsController : Controller
    {
        private Entities db = new Entities();

        // GET: CurrencyDetails
        public ActionResult Index()
        {
            var currencyDetails = db.CurrencyDetails.Include(c => c.AspNetUser).Include(c => c.AspNetUser1).Include(c => c.AspNetUser2).Include(c => c.CurrencyType);
            return View(currencyDetails.ToList());
        }

        // GET: CurrencyDetails/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CurrencyDetail currencyDetail = db.CurrencyDetails.Find(id);
            if (currencyDetail == null)
            {
                return HttpNotFound();
            }
            return View(currencyDetail);
        }

        // GET: CurrencyDetails/Create
        public ActionResult Create()
        {
            ViewBag.UpdatedBy = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.UpdatedBy = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.CreatedBy = new SelectList(db.AspNetUsers, "Id", "Email");
            ViewBag.CurrencyTypeId = new SelectList(db.CurrencyTypes, "Id", "Name");
            return View();
        }

        // POST: CurrencyDetails/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,CurrencyTypeId,Month,Year,ExchangeRate,IsActive,CreatedBy,CreationDate,UpdatedBy,UpdatingDate")] CurrencyDetail currencyDetail)
        {
            if (ModelState.IsValid)
            {
                db.CurrencyDetails.Add(currencyDetail);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UpdatedBy = new SelectList(db.AspNetUsers, "Id", "Email", currencyDetail.UpdatedBy);
            ViewBag.UpdatedBy = new SelectList(db.AspNetUsers, "Id", "Email", currencyDetail.UpdatedBy);
            ViewBag.CreatedBy = new SelectList(db.AspNetUsers, "Id", "Email", currencyDetail.CreatedBy);
            ViewBag.CurrencyTypeId = new SelectList(db.CurrencyTypes, "Id", "Name", currencyDetail.CurrencyTypeId);
            return View(currencyDetail);
        }

        // GET: CurrencyDetails/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CurrencyDetail currencyDetail = db.CurrencyDetails.Find(id);
            if (currencyDetail == null)
            {
                return HttpNotFound();
            }
            ViewBag.UpdatedBy = new SelectList(db.AspNetUsers, "Id", "Email", currencyDetail.UpdatedBy);
            ViewBag.UpdatedBy = new SelectList(db.AspNetUsers, "Id", "Email", currencyDetail.UpdatedBy);
            ViewBag.CreatedBy = new SelectList(db.AspNetUsers, "Id", "Email", currencyDetail.CreatedBy);
            ViewBag.CurrencyTypeId = new SelectList(db.CurrencyTypes, "Id", "Name", currencyDetail.CurrencyTypeId);
            return View(currencyDetail);
        }

        // POST: CurrencyDetails/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,CurrencyTypeId,Month,Year,ExchangeRate,IsActive,CreatedBy,CreationDate,UpdatedBy,UpdatingDate")] CurrencyDetail currencyDetail)
        {
            if (ModelState.IsValid)
            {
                db.Entry(currencyDetail).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UpdatedBy = new SelectList(db.AspNetUsers, "Id", "Email", currencyDetail.UpdatedBy);
            ViewBag.UpdatedBy = new SelectList(db.AspNetUsers, "Id", "Email", currencyDetail.UpdatedBy);
            ViewBag.CreatedBy = new SelectList(db.AspNetUsers, "Id", "Email", currencyDetail.CreatedBy);
            ViewBag.CurrencyTypeId = new SelectList(db.CurrencyTypes, "Id", "Name", currencyDetail.CurrencyTypeId);
            return View(currencyDetail);
        }

        // GET: CurrencyDetails/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CurrencyDetail currencyDetail = db.CurrencyDetails.Find(id);
            if (currencyDetail == null)
            {
                return HttpNotFound();
            }
            return View(currencyDetail);
        }

        // POST: CurrencyDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            CurrencyDetail currencyDetail = db.CurrencyDetails.Find(id);
            db.CurrencyDetails.Remove(currencyDetail);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
