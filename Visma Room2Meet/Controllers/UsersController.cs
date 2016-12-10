using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Visma_Room2Meet.Helpers;

namespace Visma_Room2Meet.Controllers
{
    [Authorize(Roles = "Administrator, TeamLeader, Executive")] //authorize only users with levels -1, 1 and 2 to access this page
    public class UsersController : Controller
    {
        private room2meet_dbEntities db = new room2meet_dbEntities(); //main database

        public ActionResult Details(string id)
        {
            try
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
                }
                AspNetUser user = db.AspNetUsers.Find(id);
                if (user == null)
                {
                    throw new Exception("Not found.");
                }
                return View(user);
            }
            catch (Exception ex)
            {
                ViewBag.Log = LogHandler.HandleLog(Helpers.LogType.Critical, "Users/Details", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""));
                return View("Error");
            }
        }

        // GET: Users
        //Show all users
        public ActionResult Index(int? BuildingID)
        {
            try
            {
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name); //get current user
                var userRoleLevel = Helpers.User.GetUserRoleOrder(User.Identity.Name); //get current user's role level
                var result = db.AspNetUsers.Where(u => u.Id != user.Id).ToList(); //gets all users in order to filter them
                if (userRoleLevel > 0) //if user is team leader or executive
                {
                    result = result.Where(u => u.BuildingID == user.BuildingID && u.AspNetRoles.FirstOrDefault().Order < user.AspNetRoles.FirstOrDefault().Order && u.AspNetRoles.FirstOrDefault().Order != -1).ToList(); //filter users so that they are only from the current user's building and with a level lower than his
                }
                if (BuildingID != null && userRoleLevel == -1)
                {
                    result = result.Where(u => u.BuildingID == BuildingID).ToList(); //filter users so that they are only from the selected building
                }
            
                return View(result); //show users
            }
            catch (Exception ex)
            {
                ViewBag.Log = LogHandler.HandleLog(Helpers.LogType.Critical, "Users/Index", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""));
                return View("Error");
            }
        }

        [Authorize(Roles = "Administrator, Executive")]
        //Delete user
        public ActionResult Delete(string UserID)
        {
            try
            {
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name); //get current user
                AspNetUser userToDelete = db.AspNetUsers.Find(UserID); //get the user to delete
                if (user == null || userToDelete == null)
                {
                    throw new Exception("Not found.");
                }
                var userRoleLevel = Helpers.User.GetUserRoleOrder(User.Identity.Name); //get current user's role level
                if (userRoleLevel == -1) //if is admin
                {
                    db.AspNetUsers.Remove(userToDelete);
                    db.SaveChanges();
                }
                else
                {
                    var role = Helpers.User.GetUserRoleOrder(userToDelete.UserName);
                    if (role < userRoleLevel && user.BuildingID == userToDelete.BuildingID) //if logged user has a higher role level
                    {
                        db.AspNetUsers.Remove(userToDelete);
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Not allowed!");
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Log = LogHandler.HandleLog(Helpers.LogType.Warning, "Users/Delete", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        [Authorize(Roles = "Administrator, TeamLeader, Executive")]
        //Updgrade or downgrade user
        public ActionResult UpdateRole(string id, string RoleID)
        {
            try
            {
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name); //get current user
                AspNetUser userToUpdate = db.AspNetUsers.Find(id); //get the user to update
                AspNetRole aspRole = db.AspNetRoles.Find(RoleID);
                if (user == null || userToUpdate == null || aspRole == null)
                {
                    throw new Exception("Not found.");
                }
                var userRoleLevel = Helpers.User.GetUserRoleOrder(User.Identity.Name); //get current user's role level
                if (userRoleLevel == -1) //if is admin
                {
                    foreach (AspNetRole role in userToUpdate.AspNetRoles.ToList())
                    {
                        userToUpdate.AspNetRoles.Remove(role);
                        db.Entry(userToUpdate).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    userToUpdate.AspNetRoles.Add(aspRole);
                    db.Entry(userToUpdate).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    if (aspRole.Order < userRoleLevel && user.BuildingID == userToUpdate.BuildingID) //if logged user has a higher role level
                    {
                        foreach (AspNetRole role in userToUpdate.AspNetRoles.ToList())
                        {
                            userToUpdate.AspNetRoles.Remove(role);
                            db.Entry(userToUpdate).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        userToUpdate.AspNetRoles.Add(aspRole);
                        db.Entry(userToUpdate).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Not allowed!");
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Log = LogHandler.HandleLog(Helpers.LogType.Warning, "Users/UpdateRole", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }

        [Authorize(Roles = "Administrator, TeamLeader, Executive")]
        //Approve just registered users
        public ActionResult Unlock(string id)
        {
            try
            {
                AspNetUser user = Helpers.User.GetUser(User.Identity.Name); //get current user
                AspNetUser userToUpdate = db.AspNetUsers.Find(id); //get the user to update
                if (user == null || userToUpdate == null)
                {
                    throw new Exception("Not found.");
                }
                var userRoleLevel = Helpers.User.GetUserRoleOrder(User.Identity.Name); //get current user's role level
                if (userRoleLevel == -1) //if is admin
                {
                    userToUpdate.LockoutEnabled = false;
                    userToUpdate.LockoutEndDateUtc = null;
                    db.Entry(userToUpdate).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    if (user.AspNetRoles.FirstOrDefault().Order < userRoleLevel && user.BuildingID == userToUpdate.BuildingID) //if logged user has a higher role level
                    {
                        userToUpdate.LockoutEnabled = false;
                        userToUpdate.LockoutEndDateUtc = null;
                        db.Entry(userToUpdate).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("Not allowed!");
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Log = LogHandler.HandleLog(Helpers.LogType.Warning, "Users/Unlock", ex.Message, (ex.InnerException != null ? ex.InnerException.Message : ""), Request.Params);
                return View("Error");
            }
        }
    }
}
