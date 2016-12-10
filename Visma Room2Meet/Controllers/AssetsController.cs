using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Visma_Room2Meet;

namespace Visma_Room2Meet.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AssetsController : Controller
    {
        private room2meet_dbEntities db = new room2meet_dbEntities();

        // GET: Assets
        public ActionResult Index()
        {
            try
            {
                return View(db.Assets.ToList());
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Assets/Index", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }
        
        // GET: Assets/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Assets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Name,Description,ShowAs")] Asset asset)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Assets.Add(asset);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(asset);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Assets/Create", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // GET: Assets/Edit/5
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Asset asset = db.Assets.Find(id);
                if (asset == null)
                {
                    throw new Exception("Not found.");
                }
                return View(asset);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Assets/Edit", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // POST: Assets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AssetID,Name,Description,ShowAs")] Asset asset)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(asset).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(asset);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Assets/EditPOST", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // GET: Assets/Delete/5
        public ActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Asset asset = db.Assets.Find(id);
                if (asset == null)
                {
                    return HttpNotFound();
                }
                db.Assets.Remove(asset);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Assets/Delete", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
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
