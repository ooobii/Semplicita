using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Semplicita.Helpers;
using Semplicita.Models;

namespace Semplicita.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        public ApplicationDbContext db = new ApplicationDbContext();
        private ProjectHelper projHelper;
        private TicketsHelper ticketsHelper;
        private UserRolesHelper rolesHelper;
        public ProjectsController() {
            projHelper = new ProjectHelper(db);
            ticketsHelper = new TicketsHelper(db);
            rolesHelper = new UserRolesHelper(db);
        }


        // GET: Projects
        [Route("projects")]
        public ActionResult Index()
        {
            var projects = projHelper.GetProjectsAvailableToUser(User);
            var availTickets = ticketsHelper.GetTicketsAvailableToUser(User);

            var viewModel = new ProjectIndexViewModel() {
                AvailableProjects = projects,
                Users = db.Users.ToList(),

                AvailableTickets = availTickets
            };
            return View(viewModel);
        }

        // View
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [Route("projects/create")]
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


        // Post
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




        [Route("project/{tickettag}")]
        public ActionResult Project(string TicketTag) {
            if( TicketTag == null ) {
                return View(db.Projects.ToList());
            }
            Project project = db.Projects.FirstOrDefault(p => p.TicketTag == TicketTag);
            if( project == null ) {
                return HttpNotFound();
            }
            return View("Show", project);
        }




        // GET: Projects/Edit/5
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [Route("project/edit/{tickettag}")]
        public ActionResult Edit(string TicketTag)
        {
            if ( TicketTag == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.FirstOrDefault(p => p.TicketTag == TicketTag);
            if (project == null)
            {
                return HttpNotFound();
            }

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

            var veiwModel = new EditProjectViewModel() {
                SelectedProject = project,
                ProjectAdministrators = projAdmins,
                AvailableMembers = availMembers,
                Workflows = db.ProjectWorkflows.ToList()
            };
            return View(veiwModel);
        }

        // POST: Projects/Edit/5
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProject(EditProjectModel model)
        {
            if (ModelState.IsValid)
            {
                Project project = db.Projects.Find(model.ProjectId);
                var projectMembers = new List<ApplicationUser>();
                projectMembers.AddRange(db.Users.Where(u => model.MemberIds.Contains(u.Id)));

                project.Name = model.Name;
                project.Description = model.Description;
                project.ModifiedAt = DateTime.Now;
                project.TicketTag = model.TicketTag;
                project.IsActiveProject = model.IsActiveProject;
                project.ProjectManagerId = model.ProjectManagerId;
                project.ActiveWorkflowId = model.ActiveWorkflowId;
                project.Members.Clear();
                foreach (string uId in model.MemberIds) {
                    project.Members.Add(db.Users.Find(uId));
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
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

            var veiwModel = new EditProjectViewModel() {
                SelectedProject = db.Projects.Find(model.ProjectId),
                ProjectAdministrators = projAdmins,
                AvailableMembers = availMembers,
                Workflows = db.ProjectWorkflows.ToList()
            };
            return View(veiwModel);
        }




        // GET: Projects/Delete/5
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [Route("project/archive/{tickettag}")]
        public ActionResult Archive(string TicketTag)
        {
            if( TicketTag == null ) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.FirstOrDefault(p => p.TicketTag == TicketTag);
            if( project == null ) {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ArchiveProject(string TicketTag)
        {
            Project project = db.Projects.First(p => p.TicketTag == TicketTag);
            project.IsActiveProject = false;
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
