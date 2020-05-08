using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Semplicita.Models;
using Semplicita.Helpers;

namespace Semplicita.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper rolesHelper = new UserRolesHelper();
        private ProjectHelper projectHelper = new ProjectHelper();

        // GET: Admin
        [Authorize(Roles = "ServerAdmin")]
        public ActionResult Index()
        {
            return View();
        }


        #region Project Managment

        [Authorize(Roles ="ServerAdmin,ProjectAdmin")]
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
        #endregion


    }
}