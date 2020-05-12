using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Semplicita.Helpers;
using Semplicita.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace Semplicita.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager {
            get {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set {
                _userManager = value;
            }
        }

        private UserRolesHelper rolesHelper = new UserRolesHelper();
        private ProjectHelper projectHelper = new ProjectHelper();

        // GET: Admin
        [Authorize(Roles = "ServerAdmin")]
        public ActionResult Index() {
            return View();
        }

        #region User Managment
        #endregion


        #region Project Managment

        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        public ActionResult Projects() {
            return View(db.Projects.ToList());
        }

        public ActionResult ProjectDetails(int? id) {
            if( id == null ) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if( project == null ) {
                return HttpNotFound();
            }
            return View(project);
        }

        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        public ActionResult NewProject() {
            return View();
        }

        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateProject([Bind(Include = "Id,Name,Description,CreatedAt,ModifiedAt,IsActiveProject")] Project project) {
            if( ModelState.IsValid ) {
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        public ActionResult EditProject(int? id) {
            if( id == null ) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if( project == null ) {
                return HttpNotFound();
            }
            return View(project);
        }

        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProject([Bind(Include = "Id,Name,Description,CreatedAt,ModifiedAt,IsActiveProject")] Project project) {
            if( ModelState.IsValid ) {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        public ActionResult DeleteProject(int? id) {
            if( id == null ) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if( project == null ) {
                return HttpNotFound();
            }
            return View(project);
        }

        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [HttpPost, ActionName("DeleteProject")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteProjectConfirmed(int id) {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        public ActionResult ProjectUsers() {
            var model = new ProjectUsersViewModel() { Users = db.Users.ToList(), Projects = db.Projects.ToList() };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        public ActionResult AddUsersToProject([Bind(Include = "UserIds,ProjectIds")]AddUserToProjectViewModel model) {
            var errors = new List<string>();

            foreach( string userId in model.UserIds ) {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);

                if( user != null ) {

                    foreach( int projectId in model.ProjectIds ) {
                        var addRoleSuccess = projectHelper.AddUserToProject(userId, projectId);

                        if( !addRoleSuccess ) {
                            errors.Add($"Unable to add '{user.FullNameStandard}' to project '{db.Projects.Find(projectId).Name}'.");
                        }
                    }


                } else { //user ID was not found.

                    if( !errors.Contains("Unable to locate one or more selected users.") ) {
                        errors.Add("Unable to locate one or more selected users.");
                    }

                }

            }

            if( errors.Count > 0 ) {
                ViewBag.AddUsersToProjectErrors = errors;
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        public ActionResult RemoveUserFromProject([Bind(Include = "UserIds,ProjectIds")]RemoveUserFromProjectViewModel model) {
            var errors = new List<string>();

            foreach( string userId in model.UserIds ) {
                var user = db.Users.FirstOrDefault(u => u.Id == userId);

                if( user != null ) {

                    foreach( int projectId in model.ProjectIds ) {
                        var addRoleSuccess = projectHelper.AddUserToProject(userId, projectId);

                        if( !addRoleSuccess ) {
                            errors.Add($"Unable to add '{user.FullNameStandard}' to project '{db.Projects.Find(projectId).Name}'.");
                        }
                    }


                } else { //user ID was not found.

                    if( !errors.Contains("Unable to locate one or more selected users.") ) {
                        errors.Add("Unable to locate one or more selected users.");
                    }

                }

            }

            if( errors.Count > 0 ) {
                ViewBag.AddUsersToProjectErrors = errors;
            }

            return View();
        }

        #endregion Project Managment




        #region Helpers
        private void AddErrors(IdentityResult result) {
            foreach( var error in result.Errors ) {
                ModelState.AddModelError("", error);
            }
        }
        #endregion
    }
}