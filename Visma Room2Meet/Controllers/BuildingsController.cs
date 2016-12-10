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
    public class BuildingsController : Controller
    {
        private room2meet_dbEntities db = new room2meet_dbEntities();

        // GET: Buildings
        public ActionResult Index()
        {
            try
            {
                return View(db.Buildings.ToList());
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Buildings/Index", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // GET: Buildings/Details/5
        [Authorize(Roles = "Administrator, Executive, TeamLeader, Employee")]
        public ActionResult Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Building building = db.Buildings.Find(id);
                if (building == null)
                {
                    throw new Exception("Not found.");
                }
                return View(building);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Buildings/Details", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // GET: Buildings/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Buildings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BuildingCode,Name,Country,Address,Description,ImageUrl,OpenHour,CloseHour")] Building building)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Buildings.Add(building);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(building);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Buildings/Create", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // GET: Buildings/Edit/5
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Building building = db.Buildings.Find(id);
                if (building == null)
                {
                    throw new Exception("Not found.");
                }
                return View(building);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Buildings/Edit", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // POST: Buildings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BuildingID,BuildingCode,Name,Country,Address,Description,ImageUrl,OpenHour,CloseHour")] Building building)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(building).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                return View(building);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Buildings/EditPOST", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // GET: Buildings/Delete/5
        public ActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Building building = db.Buildings.Find(id);
                if (building == null)
                {
                    throw new Exception("Not found.");
                }
                db.Buildings.Remove(building);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Buildings/Delete", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
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
