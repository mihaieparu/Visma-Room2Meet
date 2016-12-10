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
    [Authorize]
    public class RoomsController : Controller
    {
        private room2meet_dbEntities db = new room2meet_dbEntities();

        // GET: Rooms
        [Authorize(Roles = "Administrator")]
        public ActionResult Index(int? BuildingID)
        {
            try
            {
                var rooms = db.Rooms.Include(r => r.BookingStatus).Include(r => r.Building);
                if (BuildingID != null)
                {
                    rooms = rooms.Where(r => r.BuildingID == BuildingID); //filter rooms so that they are only from the selected building
                }
                return View(rooms.ToList());
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Rooms/Index", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // GET: Rooms/Details/5
        public ActionResult Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Room room = db.Rooms.Find(id);
                if (room == null)
                {
                    throw new Exception("Not found.");
                }
                return View(room);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Rooms/Details", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // GET: Rooms/Create
        [Authorize(Roles = "Administrator")]
        public ActionResult Create()
        {
            ViewBag.DefaultBookingStatusID = new SelectList(db.BookingStatuses, "BookingStatusID", "Name");
            ViewBag.BuildingID = new SelectList(db.Buildings, "BuildingID", "Name");
            ViewBag.AspNetRoles = db.AspNetRoles.ToList();
            ViewBag.Assets = db.Assets.ToList();
            return View();
        }

        // GET: Rooms/Search
        public ActionResult Search()
        {
            try
            {
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
                var f = db.Rooms.Select(r => r.Floor).Distinct().ToList();
                List<SelectListItem> floors = new List<SelectListItem>();
                foreach (string floor in f)
                {
                    floors.Add(new SelectListItem { Text = floor, Value = floor });
                }
                var c = db.Buildings.Select(r => r.Country).Distinct().ToList();
                List<SelectListItem> countries = new List<SelectListItem>();
                foreach (string country in c)
                {
                    countries.Add(new SelectListItem { Text = country, Value = country });
                }
                ViewBag.BuildingID = new SelectList((User.IsInRole("Administrator") ? db.Buildings.ToList() : db.Buildings.Where(b => b.BuildingID == user.BuildingID).ToList()), "BuildingID", "Name");
                ViewBag.Floor = new SelectList(floors, "Value", "Text");
                ViewBag.Country = new SelectList(countries, "Value", "Text");
                ViewBag.Assets = db.Assets.ToList();
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Rooms/Search", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        [HttpPost]
        public ActionResult SearchPartial(string Country, int? BuildingID, string Floor, int? Capacity, DateTime? Date, TimeSpan? Start, TimeSpan? End, string Assets)
        {
            try
            {
                if (Request.IsAjaxRequest())
                {
                    string role = Helpers.User.GetUserRoleId(User.Identity.Name);
                    var result = db.Rooms.Where(r => r.RoomRoles.Where(rr => rr.AspNetRoleID == role).Count() > 0).ToList();
                    if (!User.IsInRole("Administrator"))
                    {
                        AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
                        result = result.Where(r => r.BuildingID == user.BuildingID).ToList();
                    }
                    if (Country != null)
                    {
                        result = result.Where(r => r.Building.Country == Server.UrlDecode(Country)).ToList();
                    }
                    if (BuildingID.HasValue)
                    {
                        result = result.Where(r => r.BuildingID == BuildingID.Value).ToList();
                    }
                    if (Floor != null)
                    {
                        result = result.Where(r => r.Floor == Server.UrlDecode(Floor)).ToList();
                    }
                    if (Capacity.HasValue)
                    {
                        result = result.Where(r => r.Capacity >= Capacity.Value).ToList();
                    }
                    if (Assets != null)
                    {
                        var assets = System.Web.Helpers.Json.Decode<List<int>>(Assets);
                        foreach (int asset in assets)
                        {
                            result = result.Where(r => r.RoomAssets.Where(ra => ra.AssetID == asset).Count() > 0).ToList();
                        }
                    }
                    if (Date != null && Start != null && End != null)
                    {
                        if (Start > End)
                        {
                            return Content("<center>The start time is after the end time!</center>");
                        }
                        if (Start == End)
                        {
                            return Content("<center>The start time is equal to the end time!</center>");
                        }
                        DateTime start = new DateTime(Date.Value.Year, Date.Value.Month, Date.Value.Day, Start.Value.Hours, Start.Value.Minutes, Start.Value.Seconds);
                        DateTime end = new DateTime(Date.Value.Year, Date.Value.Month, Date.Value.Day, End.Value.Hours, End.Value.Minutes, End.Value.Seconds);
                        if (start < DateTime.Now)
                        {
                            return Content("<center>The start time is before the current time!</center>");
                        }
                        result = result.Where(r => r.Bookings.Where(b => b.EndDate >= start && b.StartDate <= end).Count() == 0).ToList();
                        result = result.Where(r => r.Building.OpenHour == null || (r.Building.OpenHour != null && r.Building.OpenHour <= Start.Value)).Where(r => r.Building.CloseHour == null || (r.Building.CloseHour != null && r.Building.CloseHour >= End.Value)).ToList();
                        result = result.Where(r => r.MaximumBookingHours == null || (r.MaximumBookingHours != null && r.MaximumBookingHours >= End.Value - Start.Value)).ToList();
                    }

                    return PartialView("PartialResults", result);
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Rooms/SearchPartial", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        public ActionResult Book(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Room room = db.Rooms.Find(id);
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
                    AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
                    if (room.BuildingID != user.BuildingID)
                    {
                        throw new Exception("This room is in a different building than yours.");
                    }
                }
                return View(room);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Rooms/Book", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Book(int RoomID, DateTime Date, TimeSpan Start, TimeSpan End)
        {
            try
            {
                Room room = db.Rooms.Find(RoomID);
                if (room == null)
                {
                    throw new Exception("Not found.");
                }
                string role = Helpers.User.GetUserRoleId(User.Identity.Name);
                if (room.RoomRoles.Where(rr => rr.AspNetRoleID == role).Count() <= 0)
                {
                    throw new Exception("Not available for the current user type.");
                }
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name);
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
                string bookref = "";
                Random r = new Random();
                bookref = room.Building.BuildingCode + r.Next(10000, 99999).ToString() + room.Floor + Date.DayOfWeek.ToString().Substring(0, 1)  + room.RoomCode;
                Booking booking = new Booking()
                {
                    RoomID = room.RoomID,
                    BookingReference = bookref,
                    AspNetUserID = user.Id,
                    CreatedDate = DateTime.Now,
                    StartDate = start,
                    EndDate = end,
                    BookingStatusID = room.DefaultBookingStatusID
                };
                db.Bookings.Add(booking);
                db.SaveChanges();
                Helpers.BookingChanges.HandleChange(booking.BookingID, user.Id, "Created booking. Room: " + booking.Room.RoomCode + "; Start date: " + start.ToString() + "; End date: " + end.ToString() + "; Status: " + booking.BookingStatusID);
                return RedirectToAction("Index", "Bookings");
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Rooms/BookPOST", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // POST: Rooms/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Create([Bind(Include = "BuildingID,Floor,Capacity,RoomCode,DefaultBookingStatusID,MaximumBookingHours,Name,Description,ImgUrl")] Room room, string AllowedGroups, string Assets)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Rooms.Add(room);
                    db.SaveChanges();
                    var allGroups = System.Web.Helpers.Json.Decode<List<string>>(Server.UrlDecode(AllowedGroups));
                    foreach (string gr in allGroups)
                    {
                        RoomRole rr = new RoomRole()
                        {
                            AspNetRoleID = gr,
                            RoomID = room.RoomID
                        };
                        db.RoomRoles.Add(rr);
                        db.SaveChanges();
                    }
                    var asst = System.Web.Helpers.Json.Decode<List<Dictionary<string, string>>>(Server.UrlDecode(Assets));
                    foreach (Dictionary<string, string> asset in asst)
                    {
                        RoomAsset ra = new RoomAsset()
                        {
                            AssetID = Convert.ToInt32(asset["id"]),
                            Description = asset["description"],
                            RoomID = room.RoomID
                        };
                        db.RoomAssets.Add(ra);
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }

                ViewBag.DefaultBookingStatusID = new SelectList(db.BookingStatuses, "BookingStatusID", "Name", room.DefaultBookingStatusID);
                ViewBag.BuildingID = new SelectList(db.Buildings, "BuildingID", "Name", room.BuildingID);
                ViewBag.AspNetRoles = db.AspNetRoles.ToList();
                ViewBag.Assets = db.Assets.ToList();
                return View(room);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Rooms/Create", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // GET: Rooms/Edit/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Room room = db.Rooms.Find(id);
                if (room == null)
                {
                    throw new Exception("Not found.");
                }
                ViewBag.DefaultBookingStatusID = new SelectList(db.BookingStatuses, "BookingStatusID", "Name", room.DefaultBookingStatusID);
                ViewBag.BuildingID = new SelectList(db.Buildings, "BuildingID", "Name", room.BuildingID);
                ViewBag.AspNetRoles = db.AspNetRoles.ToList();
                ViewBag.Assets = db.Assets.ToList();
                return View(room);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Rooms/Edit", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // POST: Rooms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public ActionResult Edit([Bind(Include = "RoomID,BuildingID,Floor,Capacity,RoomCode,DefaultBookingStatusID,MaximumBookingHours,Name,Description,ImgUrl")] Room room, string AllowedGroups, string Assets)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(room).State = EntityState.Modified;
                    db.SaveChanges();
                    foreach (RoomRole rr in db.RoomRoles.Where(rro => rro.RoomID == room.RoomID).ToList())
                    {
                        db.RoomRoles.Remove(rr);
                        db.SaveChanges();
                    }
                    foreach (RoomAsset ra in db.RoomAssets.Where(rao => rao.RoomID == room.RoomID).ToList())
                    {
                        db.RoomAssets.Remove(ra);
                        db.SaveChanges();
                    }
                    var allGroups = System.Web.Helpers.Json.Decode<List<string>>(Server.UrlDecode(AllowedGroups));
                    foreach (string gr in allGroups)
                    {
                        RoomRole rr = new RoomRole()
                        {
                            AspNetRoleID = gr,
                            RoomID = room.RoomID
                        };
                        db.RoomRoles.Add(rr);
                        db.SaveChanges();
                    }
                    var asst = System.Web.Helpers.Json.Decode<List<Dictionary<string, string>>>(Server.UrlDecode(Assets));
                    foreach (Dictionary<string, string> asset in asst)
                    {
                        RoomAsset ra = new RoomAsset()
                        {
                            AssetID = Convert.ToInt32(asset["id"]),
                            Description = asset["description"],
                            RoomID = room.RoomID
                        };
                        db.RoomAssets.Add(ra);
                        db.SaveChanges();
                    }
                    return RedirectToAction("Index");
                }
                ViewBag.DefaultBookingStatusID = new SelectList(db.BookingStatuses, "BookingStatusID", "Name", room.DefaultBookingStatusID);
                ViewBag.BuildingID = new SelectList(db.Buildings, "BuildingID", "Name", room.BuildingID);
                ViewBag.AspNetRoles = db.AspNetRoles.ToList();
                ViewBag.Assets = db.Assets.ToList();
                return View(room);
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Rooms/EditPOST", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        // GET: Rooms/Delete/5
        [Authorize(Roles = "Administrator")]
        public ActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                Room room = db.Rooms.Find(id);
                if (room == null)
                {
                    throw new Exception("Not found.");
                }
                db.Rooms.Remove(room);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Log = Helpers.LogHandler.HandleLog(Helpers.LogType.Warning, "Rooms/Delete", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
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
