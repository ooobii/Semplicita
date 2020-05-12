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
using System.Reflection;
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
        private RoleDisplayDictionary roleDisplays = new RoleDisplayDictionary();

        // GET: Admin
        [Authorize(Roles = "ServerAdmin")]
        public ActionResult Index() {
            return View();
        }

        #region User Managment
        [HttpPost]
        [Authorize(Roles = "ServerAdmin")]
        [MultipleButton(Name = "userRoles", Argument = "Add")]
        public ActionResult AddUsersToRoles(AddRemoveUsersRolesViewModel model) {
            var errors = new List<string>();
            var successes = new List<string>();

            if( model.UserIds == null ) {

                errors.Add("You must select at least 1 user to manipulate.");

            } else if( model.Roles == null ) {

                errors.Add("You must select at least 1 role.");

            } else {

                foreach( string userId in model.UserIds ) {
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);

                    if( user != null ) {

                        foreach( string role in model.Roles ) {
                            var addRoleSuccess = rolesHelper.AddUserToRole(userId, role);

                            if( !addRoleSuccess ) {
                                errors.Add($"Unable to add '{user.FullNameStandard}' to the role '{roleDisplays[ role ]}'.");
                            } else {
                                successes.Add($"'{user.FullNameStandard}' has been added to the '{roleDisplays[ role ]}' role.");
                            }
                        }


                    } else { //user ID was not found.

                        if( !errors.Contains("Unable to locate one of the selected users.") ) {
                            errors.Add("Unable to locate one of the selected users.");
                        }

                    }

                }
            }

            if( errors.Count > 0 ) {
                ViewBag.UserRoleAssignErrors = errors;
            }
            if( successes.Count > 0 ) {
                ViewBag.UserRoleAssignMessages = successes;
            }

            return View(new UsersAllocViewModel() { Users = db.Users.ToList(), Projects = db.Projects.ToList() });
        }
        [HttpPost]
        [Authorize(Roles = "ServerAdmin")]
        [MultipleButton(Name = "userRoles", Argument = "Remove")]
        public ActionResult RemoveUsersFromRoles(AddRemoveUsersRolesViewModel model) {
            var errors = new List<string>();
            var successes = new List<string>();

            if( model.UserIds == null ) {

                errors.Add("You must select at least 1 user to manipulate.");

            } else if( model.Roles == null ) {

                errors.Add("You must select at least 1 role.");

            } else {

                foreach( string userId in model.UserIds ) {
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);

                    if( user != null ) {

                        foreach( string role in model.Roles ) {
                            var removeRoleSuccess = rolesHelper.RemoveUserFromRole(userId, role);

                            if( !removeRoleSuccess ) {
                                errors.Add($"Unable to remove '{user.FullNameStandard}' from the role '{roleDisplays[ role ]}'. Is the user in this role?");
                            } else {
                                successes.Add($"'{user.FullNameStandard}' has been removed from the '{roleDisplays[ role ]}' role.");
                            }
                        }


                    } else { //user ID was not found.

                        if( !errors.Contains("Unable to locate one of the selected users.") ) {
                            errors.Add("Unable to locate one of the selected users.");
                        }

                    }

                }
            }

            if( errors.Count > 0 ) {
                ViewBag.UserRoleAssignErrors = errors;
            }
            if( successes.Count > 0 ) {
                ViewBag.UserRoleAssignMessages = successes;
            }

            return View(new UsersAllocViewModel() { Users = db.Users.ToList(), Projects = db.Projects.ToList() });
        }

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
        public ActionResult UserAllocation() {
            var model = new UsersAllocViewModel() { Users = db.Users.ToList(), Projects = db.Projects.ToList() };
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [MultipleButton(Name = "projectUsers", Argument = "Add")]
        public ActionResult AddUsersToProject([Bind(Include = "UserIds,ProjectIds")]AddRemoveUsersProjectsViewModel model) {
            var errors = new List<string>();
            var successes = new List<string>();

            if( model.UserIds == null ) {

                errors.Add("You must select at least 1 user to manipulate.");

            } else if( model.ProjectIds == null ) {

                errors.Add("You must select at least 1 project.");

            } else {

                foreach( string userId in model.UserIds ) {
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);

                    if( user != null ) {

                        foreach( int projectId in model.ProjectIds ) {
                            var addProjectSuccess = projectHelper.AddUserToProject(userId, projectId);

                            if( !addProjectSuccess ) {
                                errors.Add($"Unable to add '{user.FullNameStandard}' to project '{db.Projects.Find(projectId).Name}'.");
                            } else {
                                successes.Add($"'{user.FullNameStandard}' has been added to the '{db.Projects.Find(projectId).Name}' project.");
                            }
                        }


                    } else { //user ID was not found.

                        if( !errors.Contains("Unable to locate one or more selected users.") ) {
                            errors.Add("Unable to locate one or more selected users.");
                        }

                    }

                }
            }

            if( errors.Count > 0 ) {
                ViewBag.UserProjectAllocErrors = errors;
            }
            if( successes.Count > 0 ) {
                ViewBag.UserProjectAllocMessages = successes;
            }

            return View(new UsersAllocViewModel() { Users = db.Users.ToList(), Projects = db.Projects.ToList() });
        }

        [HttpPost]
        [Authorize(Roles = "ServerAdmin,ProjectAdmin")]
        [MultipleButton(Name = "projectUsers", Argument = "Remove")]
        public ActionResult RemoveUserFromProject([Bind(Include = "UserIds,ProjectIds")]AddRemoveUsersProjectsViewModel model) {
            var errors = new List<string>();
            var successes = new List<string>();

            if( model.UserIds == null ) {

                errors.Add("You must select at least 1 user to manipulate.");

            } else if( model.ProjectIds == null ) {

                errors.Add("You must select at least 1 project.");

            } else {

                foreach( string userId in model.UserIds ) {
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);

                    if( user != null ) {

                        foreach( int projectId in model.ProjectIds ) {
                            var removeProjectSuccess = projectHelper.RemoveUserFromProject(userId, projectId);

                            if( !removeProjectSuccess ) {
                                errors.Add($"Unable to remove '{user.FullNameStandard}' from project '{db.Projects.Find(projectId).Name}'.");
                            } else {
                                successes.Add($"'{user.FullNameStandard}' has been removed from the '{db.Projects.Find(projectId).Name}' project.");
                            }
                        }


                    } else { //user ID was not found.

                        if( !errors.Contains("Unable to locate one or more selected users.") ) {
                            errors.Add("Unable to locate one or more selected users.");
                        }

                    }

                }

            }

            if( errors.Count > 0 ) {
                ViewBag.UserProjectAllocErrors = errors;
            }
            if( successes.Count > 0 ) {
                ViewBag.UserProjectAllocMessages = successes;
            }


            return View(new UsersAllocViewModel() { Users = db.Users.ToList(), Projects = db.Projects.ToList() });
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

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MultipleButtonAttribute : ActionNameSelectorAttribute
    {
        public string Name { get; set; }
        public string Argument { get; set; }

        public override bool IsValidName(ControllerContext controllerContext, string actionName, MethodInfo methodInfo) {
            var isValidName = false;
            var keyValue = string.Format("{0}:{1}", Name, Argument);
            var value = controllerContext.Controller.ValueProvider.GetValue(keyValue);

            if( value != null ) {
                controllerContext.Controller.ControllerContext.RouteData.Values[ Name ] = Argument;
                isValidName = true;
            }

            return isValidName;
        }
    }
}