using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Semplicita.Helpers;
using Semplicita.Models;

namespace Semplicita.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper rolesHelper = new UserRolesHelper();


        // GET: Projects
        public ActionResult Index()
        {
            return View(db.Projects.ToList());
        }

        [Route("Projects/{tickettag}")]
        public ActionResult Index(string tickettag) {
            if( tickettag == null ) {
                return View(db.Projects.ToList());
            }
            Project project = db.Projects.FirstOrDefault(p => p.TicketTag == tickettag);
            if( project == null ) {
                return HttpNotFound();
            }
            return View("Show", project);
        }

        // GET: Projects/Details/5
        public ActionResult Show(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        public ActionResult New() {
            var projAdmins = new List<ApplicationUser>();
            var availMembers = new List<ApplicationUser>();

            foreach( ApplicationUser u in db.Users ) {
                var roles = rolesHelper.ListUserRoles(u.Id);
                if( roles.Contains("SuperSolver") || roles.Contains("Solver") || roles.Contains("Reporter") ) {
                    availMembers.Add(u);
                }
                if( roles.Contains("ProjectAdmin") ) {
                    projAdmins.Add(u);
                }
            }

            CreateProjectViewModel viewModel = new CreateProjectViewModel() {
                ProjectAdministrators = projAdmins.OrderBy(u => u.FullNameStandard).ToList(),
                AvailableMembers = availMembers.OrderBy(u => u.FullNameStandard).ToList(),
                Workflows = db.ProjectWorkflows.ToList()
            };

            return View(viewModel);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateNew(NewProjectModel project)
        {
            if (ModelState.IsValid)
            {
                var projectMembers = new List<ApplicationUser>();
                projectMembers.AddRange(db.Users.Where(u => project.MemberIds.Contains(u.Id)));

                var newProject = new Project() {
                    Name = project.Name,
                    Description = project.Description,
                    CreatedAt = DateTime.Now,
                    TicketTag = project.TicketTag,
                    IsActiveProject = project.IsActiveProject,
                    ProjectManagerId = project.ProjectManagerId,
                    ActiveWorkflowId = project.ActiveWorkflowId,
                    Members = projectMembers
                };

                db.Projects.Add(newProject);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,CreatedAt,ModifiedAt,IsActiveProject")] Project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
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
