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
    public class BookingsController : Controller
    {
        private room2meet_dbEntities db = new room2meet_dbEntities();

        // GET: Bookings
        public ActionResult Index(int? RoomID)
        {
            try
            {
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name); //get current user
                var userRoleLevel = Helpers.User.GetUserRoleOrder(User.Identity.Name); //get current user's role level
                var bookings = db.Bookings.Include(b => b.AspNetUser).Include(b => b.BookingStatus).Include(b => b.Room).OrderByDescending(b => b.CreatedDate).ToList();
                if (RoomID != null)
                {
                    bookings = bookings.Where(b => b.RoomID == RoomID).ToList();
                }
                if (userRoleLevel == -1)
                {
                    return View(bookings.ToList());
                }
                else
                {
                    var my = bookings.Where(b => b.AspNetUserID == user.Id);
                    var model = bookings.Where(b => b.AspNetUserID != user.Id && b.AspNetUser.AspNetRoles.FirstOrDefault().Order < userRoleLevel && b.AspNetUser.AspNetRoles.FirstOrDefault().Order != -1);
                    ViewBag.My = my.ToList();
                    return View(model.ToList());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Critical, "Bookings/Index", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""));
                return View("Error");
            }
        }

        // GET: Bookings/Details/5
        public ActionResult Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Booking booking = db.Bookings.Find(id);
                if (booking == null)
                {
                    return HttpNotFound();
                }
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
                int? userRoleOrder = Helpers.User.GetUserRoleOrder(User.Identity.Name);
                if (userRoleOrder.HasValue && (userRoleOrder == -1 || (userRoleOrder > booking.AspNetUser.AspNetRoles.FirstOrDefault().Order && booking.AspNetUser.AspNetRoles.FirstOrDefault().Order != -1) || user.Id == booking.AspNetUserID))
                {
                    return View(booking);
                }
                else
                {
                    throw new Exception("Not allowed.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Critical, "Bookings/Details", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""));
                return View("Error");
            }
        }

        public ActionResult History(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Booking booking = db.Bookings.Find(id);
                if (booking == null)
                {
                    return HttpNotFound();
                }
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
                int? userRoleOrder = Helpers.User.GetUserRoleOrder(User.Identity.Name);
                if (userRoleOrder.HasValue && (userRoleOrder == -1 || (userRoleOrder > booking.AspNetUser.AspNetRoles.FirstOrDefault().Order && booking.AspNetUser.AspNetRoles.FirstOrDefault().Order != -1) || user.Id == booking.AspNetUserID))
                {
                    return View(booking.BookingHistories.OrderByDescending(bh => bh.ChangeDate).ToList());
                }
                else
                {
                    throw new Exception("Not allowed.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Critical, "Bookings/Details", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""));
                return View("Error");
            }
        }

        public ActionResult Approve(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Booking booking = db.Bookings.Find(id);
                if (booking == null)
                {
                    return HttpNotFound();
                }
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
                int? userRoleOrder = Helpers.User.GetUserRoleOrder(User.Identity.Name);
                if (userRoleOrder.HasValue && (userRoleOrder == -1 || (userRoleOrder > booking.AspNetUser.AspNetRoles.FirstOrDefault().Order && booking.AspNetUser.AspNetRoles.FirstOrDefault().Order != -1)))
                {
                    booking.BookingStatusID = (int)Helpers.BookingStatus.Booked;
                    db.Entry(booking).State = EntityState.Modified;
                    db.SaveChanges();
                    Helpers.BookingChanges.HandleChange(booking.BookingID, user.Id, "Changed status to: Booked");
                    return RedirectToAction("Index");
                }
                else
                {
                    throw new Exception("Not allowed.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Critical, "Bookings/Approve", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""));
                return View("Error");
            }
        }

        public ActionResult Cancel(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Booking booking = db.Bookings.Find(id);
                if (booking == null)
                {
                    return HttpNotFound();
                }
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
                int? userRoleOrder = Helpers.User.GetUserRoleOrder(User.Identity.Name);
                if (userRoleOrder.HasValue && (userRoleOrder == -1 || (userRoleOrder > booking.AspNetUser.AspNetRoles.FirstOrDefault().Order && booking.AspNetUser.AspNetRoles.FirstOrDefault().Order != -1) || user.Id == booking.AspNetUserID))
                {
                    booking.BookingStatusID = (int)Helpers.BookingStatus.Canceled;
                    db.Entry(booking).State = EntityState.Modified;
                    db.SaveChanges();
                    Helpers.BookingChanges.HandleChange(booking.BookingID, user.Id, "Changed status to: Canceled");
                    return RedirectToAction("Index");
                }
                else
                {
                    throw new Exception("Not allowed.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Critical, "Bookings/Cancel", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""));
                return View("Error");
            }
        }

        public ActionResult StartMeeting(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Booking booking = db.Bookings.Find(id);
                if (booking == null)
                {
                    return HttpNotFound();
                }
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
                int? userRoleOrder = Helpers.User.GetUserRoleOrder(User.Identity.Name);
                if (user.Id == booking.AspNetUserID)
                {
                    booking.BookingStatusID = (int)Helpers.BookingStatus.Meeting;
                    db.Entry(booking).State = EntityState.Modified;
                    db.SaveChanges();
                    Helpers.BookingChanges.HandleChange(booking.BookingID, user.Id, "Changed status to: Meeting");
                    return RedirectToAction("Index");
                }
                else
                {
                    throw new Exception("Not allowed.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Critical, "Bookings/StartMeeting", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""));
                return View("Error");
            }
        }

        public ActionResult FinishMeeting(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Booking booking = db.Bookings.Find(id);
                if (booking == null)
                {
                    return HttpNotFound();
                }
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
                int? userRoleOrder = Helpers.User.GetUserRoleOrder(User.Identity.Name);
                if (user.Id == booking.AspNetUserID)
                {
                    booking.BookingStatusID = (int)Helpers.BookingStatus.Completed;
                    db.Entry(booking).State = EntityState.Modified;
                    db.SaveChanges();
                    Helpers.BookingChanges.HandleChange(booking.BookingID, user.Id, "Changed status to: Completed");
                    return RedirectToAction("Index");
                }
                else
                {
                    throw new Exception("Not allowed.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Critical, "Bookings/FinishMeeting", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""));
                return View("Error");
            }
        }

        // GET: Bookings/Edit/5
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Booking booking = db.Bookings.Find(id);
                if (booking == null)
                {
                    return HttpNotFound();
                }
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
                int? userRoleOrder = Helpers.User.GetUserRoleOrder(User.Identity.Name);
                if (userRoleOrder.HasValue && (userRoleOrder == -1 || (userRoleOrder > booking.AspNetUser.AspNetRoles.FirstOrDefault().Order && booking.AspNetUser.AspNetRoles.FirstOrDefault().Order != -1) || user.Id == booking.AspNetUserID))
                {
                    ViewBag.RoomID = new SelectList(db.Rooms.Where(r => r.BuildingID == user.BuildingID).ToList(), "RoomID", "Name", booking.RoomID);
                    return View(booking);
                }
                else
                {
                    throw new Exception("Not allowed.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Critical, "Bookings/Edit", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""));
                return View("Error");
            }
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookingID,RoomID")] Booking booking, DateTime Date, TimeSpan Start, TimeSpan End)
        {
            try
            {
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
                int? userRoleOrder = Helpers.User.GetUserRoleOrder(User.Identity.Name);
                if (ModelState.IsValid && userRoleOrder.HasValue && (userRoleOrder == -1 || (userRoleOrder > booking.AspNetUser.AspNetRoles.FirstOrDefault().Order && booking.AspNetUser.AspNetRoles.FirstOrDefault().Order != -1) || user.Id == booking.AspNetUserID))
                {
                    Room room = db.Rooms.Find(booking.RoomID);
                    if (room == null)
                    {
                        throw new Exception("Not found.");
                    }
                    string role = Helpers.User.GetUserRoleId(User.Identity.Name);
                    if (room.RoomRoles.Where(rr => rr.AspNetRoleID == role).Count() <= 0)
                    {
                        throw new Exception("Not available for the current user type.");
                    }
                    if (!User.IsInRole("Administrator"))
                    {
                        if (room.BuildingID != user.BuildingID)
                        {
                            throw new Exception("This room is in a different building than yours.");
                        }
                    }
                    DateTime start = new DateTime(Date.Year, Date.Month, Date.Day, Start.Hours, Start.Minutes, Start.Seconds);
                    DateTime end = new DateTime(Date.Year, Date.Month, Date.Day, End.Hours, End.Minutes, End.Seconds);
                    if (Start > End)
                    {
                        throw new Exception("The start time is after the end time!");
                    }
                    if (start < DateTime.Now)
                    {
                        throw new Exception("The start time is before the current time!");
                    }
                    if (Start == End)
                    {
                        throw new Exception("The start time is equal to the end time!");
                    }
                    if (room.Bookings.Where(b => b.EndDate >= start && b.StartDate <= end).Count() > 0)
                    {
                        throw new Exception("Room is not available for the selected period of time.");
                    }
                    if (room.Building.OpenHour != null)
                    {
                        if (room.Building.OpenHour > Start)
                        {
                            throw new Exception("The building opens after the selected start time.");
                        }
                    }
                    if (room.Building.CloseHour != null)
                    {
                        if (room.Building.CloseHour < End)
                        {
                            throw new Exception("The building closes after the selected start time.");
                        }
                    }
                    if (room.MaximumBookingHours < End - Start)
                    {
                        throw new Exception("The room accepts only bookings that are quicker than " + room.MaximumBookingHours + " hours.");
                    }
                    booking.StartDate = start;
                    booking.EndDate = end;
                    db.Entry(booking).State = EntityState.Modified;
                    db.SaveChanges();
                    Helpers.BookingChanges.HandleChange(booking.BookingID, user.Id, "Changed room, date, start and/or end times. Room: " + booking.Room.RoomCode + "; Start date: " + start + "; End date: " + end);
                    return RedirectToAction("Index");
                }
                ViewBag.RoomID = new SelectList(db.Rooms.Where(r => r.BuildingID == user.BuildingID).ToList(), "RoomID", "Name", booking.RoomID);
                return View(booking);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Critical, "Bookings/EditPOST", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""));
                return View("Error");
            }
        }

        // GET: Bookings/Delete/5
        public ActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Booking booking = db.Bookings.Find(id);
                if (booking == null)
                {
                    return HttpNotFound();
                }
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
                int? userRoleOrder = Helpers.User.GetUserRoleOrder(User.Identity.Name);
                if (userRoleOrder.HasValue && (userRoleOrder == -1 || (userRoleOrder > booking.AspNetUser.AspNetRoles.FirstOrDefault().Order && booking.AspNetUser.AspNetRoles.FirstOrDefault().Order != -1) || user.Id == booking.AspNetUserID))
                {
                    db.Bookings.Remove(booking);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    throw new Exception("Not allowed.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Critical, "Bookings/Delete", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""));
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
