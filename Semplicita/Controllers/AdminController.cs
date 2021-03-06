﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Semplicita.Helpers;
using Semplicita.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Semplicita.Controllers
{
    [Authorize]
    [SuppressMessage("ReSharper", "Mvc.ViewNotResolved")]
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

        // GET: Admin
        [Authorize( Roles = "ServerAdmin" )]
        public ActionResult Index() {
            return View();
        }

        #region User Allocation

        [Authorize( Roles = "ServerAdmin,ProjectAdmin" )]
        public ActionResult UserAllocation() {
            var model = new ServerConfigViewModel() { Users = db.Users.ToList(), Projects = db.Projects.ToList() };
            return View( model );
        }

        [HttpPost]
        [Authorize( Roles = "ServerAdmin,ProjectAdmin" )]
        [MultipleButton( Name = "projectUsers", Argument = "Add" )]
        [ValidateAntiForgeryToken]
        public ActionResult AddUsersToProject( AddRemoveUsersProjectsModel model ) {
            var projectHelper = new ProjectHelper(db);

            var errors = new List<string>();
            var successes = new List<string>();

            if ( model.UserIds == null ) {
                errors.Add( "You must select at least 1 user to manipulate." );
            } else if ( model.ProjectIds == null ) {
                errors.Add( "You must select at least 1 project." );
            } else {
                foreach ( string userId in model.UserIds ) {
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);

                    if ( user != null ) {
                        foreach ( int projectId in model.ProjectIds ) {
                            var addProjectSuccess = projectHelper.AddUserToProject(userId, projectId);

                            if ( !addProjectSuccess ) {
                                errors.Add( $"Unable to add '{user.FullNameStandard}' to project '{db.Projects.Find( projectId ).Name}'." );
                            } else {
                                successes.Add( $"'{user.FullNameStandard}' has been added to the '{db.Projects.Find( projectId ).Name}' project." );
                            }
                        }
                    } else { //user ID was not found.
                        if ( !errors.Contains( "Unable to locate one or more selected users." ) ) {
                            errors.Add( "Unable to locate one or more selected users." );
                        }
                    }
                }
            }

            if ( errors.Count > 0 ) {
                foreach ( var err in errors ) {
                    TempData.AddDangerToast( err );
                }
            }
            if ( successes.Count > 0 ) {
                foreach ( var msg in successes ) {
                    TempData.AddSuccessToast( msg );
                }
            }

            ServerConfigViewModel viewModel = new ServerConfigViewModel()
            {
                Users = db.Users.ToList(),
                Projects = db.Projects.ToList(),
                Workflows = db.ProjectWorkflows.ToList()
            };
            return View( viewName: model.ReturningViewName ?? null, model: viewModel );
        }

        [HttpPost]
        [Authorize( Roles = "ServerAdmin,ProjectAdmin" )]
        [MultipleButton( Name = "projectUsers", Argument = "Remove" )]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveUserFromProject( AddRemoveUsersProjectsModel model ) {
            var projectHelper = new ProjectHelper(db);
            var errors = new List<string>();
            var successes = new List<string>();

            if ( model.UserIds == null ) {
                errors.Add( "You must select at least 1 user to manipulate." );
            } else if ( model.ProjectIds == null ) {
                errors.Add( "You must select at least 1 project." );
            } else {
                foreach ( string userId in model.UserIds ) {
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);

                    if ( user != null ) {
                        foreach ( int projectId in model.ProjectIds ) {
                            var removeProjectSuccess = projectHelper.RemoveUserFromProject(userId, projectId);

                            if ( !removeProjectSuccess ) {
                                errors.Add( $"Unable to remove '{user.FullNameStandard}' from project '{db.Projects.Find( projectId ).Name}'." );
                            } else {
                                successes.Add( $"'{user.FullNameStandard}' has been removed from the '{db.Projects.Find( projectId ).Name}' project." );
                            }
                        }
                    } else { //user ID was not found.
                        if ( !errors.Contains( "Unable to locate one or more selected users." ) ) {
                            errors.Add( "Unable to locate one or more selected users." );
                        }
                    }
                }
            }

            if ( errors.Count > 0 ) {
                foreach ( var err in errors ) {
                    TempData.AddDangerToast( err );
                }
            }
            if ( successes.Count > 0 ) {
                foreach ( var msg in successes ) {
                    TempData.AddSuccessToast( msg );
                }
            }

            ServerConfigViewModel viewModel = new ServerConfigViewModel()
            {
                Users = db.Users.ToList(),
                Projects = db.Projects.ToList(),
                Workflows = db.ProjectWorkflows.ToList()
            };
            return View( viewName: model.ReturningViewName ?? null, model: viewModel );
        }

        [HttpPost]
        [Authorize( Roles = "ServerAdmin" )]
        [MultipleButton( Name = "userRoles", Argument = "Add" )]
        [ValidateAntiForgeryToken]
        public ActionResult AddUsersToRoles( AddRemoveUsersRolesModel model ) {
            var roleHelper = new PermissionsHelper(db);
            var roleDisplays = new RoleDisplayDictionary();
            var errors = new List<string>();
            var successes = new List<string>();

            if ( model.UserIds == null ) {
                errors.Add( "You must select at least 1 user to manipulate." );
            } else if ( model.Roles == null ) {
                errors.Add( "You must select at least 1 role." );
            } else {
                foreach ( var userId in model.UserIds ) {
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);

                    if ( user != null ) {
                        foreach ( var role in model.Roles ) {
                            var addRoleSuccess = roleHelper.AddUserToRole(userId, role);

                            if ( !addRoleSuccess ) {
                                errors.Add( $"Unable to add '{user.FullNameStandard}' to the role '{roleDisplays[role]}'." );
                            } else {
                                successes.Add( $"'{user.FullNameStandard}' has been added to the '{roleDisplays[role]}' role." );
                            }
                        }
                    } else { //user ID was not found.
                        if ( !errors.Contains( "Unable to locate one of the selected users." ) ) {
                            errors.Add( "Unable to locate one of the selected users." );
                        }
                    }
                }
            }

            if ( errors.Count > 0 ) {
                foreach ( var err in errors ) {
                    TempData.AddDangerToast( err );
                }
            }
            if ( successes.Count > 0 ) {
                foreach ( var msg in successes ) {
                    TempData.AddSuccessToast( msg );
                }
            }

            ServerConfigViewModel viewModel = new ServerConfigViewModel()
            {
                Users = db.Users.ToList(),
                Projects = db.Projects.ToList(),
                Workflows = db.ProjectWorkflows.ToList()
            };
            return View( viewModel );
        }

        [HttpPost]
        [Authorize( Roles = "ServerAdmin" )]
        [MultipleButton( Name = "userRoles", Argument = "Remove" )]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveUsersFromRoles( AddRemoveUsersRolesModel model ) {
            var roleHelper = new PermissionsHelper(db);
            var roleDisplays = new RoleDisplayDictionary();
            var errors = new List<string>();
            var successes = new List<string>();

            if ( model.UserIds == null ) {
                errors.Add( "You must select at least 1 user to manipulate." );
            } else if ( model.Roles == null ) {
                errors.Add( "You must select at least 1 role." );
            } else {
                foreach ( string userId in model.UserIds ) {
                    var user = db.Users.FirstOrDefault(u => u.Id == userId);

                    if ( user != null ) {
                        foreach ( string role in model.Roles ) {
                            var removeRoleSuccess = roleHelper.RemoveUserFromRole(user.Id, role);

                            if ( !removeRoleSuccess ) {
                                errors.Add( $"Unable to remove '{user.FullNameStandard}' from the role '{roleDisplays[role]}'. Is the user in this role?" );
                            } else {
                                successes.Add( $"'{user.FullNameStandard}' has been removed from the '{roleDisplays[role]}' role." );
                            }
                        }
                    } else { //user ID was not found.
                        if ( !errors.Contains( "Unable to locate one of the selected users." ) ) {
                            errors.Add( "Unable to locate one of the selected users." );
                        }
                    }
                }
            }

            if ( errors.Count > 0 ) {
                foreach ( var err in errors ) {
                    TempData.AddDangerToast( err );
                }
            }
            if ( successes.Count > 0 ) {
                foreach ( var msg in successes ) {
                    TempData.AddSuccessToast( msg );
                }
            }

            ServerConfigViewModel viewModel = new ServerConfigViewModel()
            {
                Users = db.Users.ToList(),
                Projects = db.Projects.ToList(),
                Workflows = db.ProjectWorkflows.ToList()
            };
            return View( viewModel );
        }

        #endregion User Allocation

        #region Project Configuration

        [Authorize( Roles = "ServerAdmin" )]
        public ActionResult ProjectConfiguration() {
            return View( new ServerConfigViewModel() {
                Users = db.Users.ToList(),
                Projects = db.Projects.ToList(),
                Workflows = db.ProjectWorkflows.ToList()
            } );
        }

        [HttpPost]
        [Authorize]
        [MultipleButton( Name = "projWorkflow", Argument = "Set" )]
        public ActionResult SetProjectWorkflow( SetProjectWorkflowModel model ) {
            var projectHelper = new ProjectHelper(db);
            var errors = new List<string>();
            var successes = new List<string>();

            if ( model.ProjectIds == null ) {
                errors.Add( "You must select at least 1 project to set the workflow of." );
            } else if ( model.WorkflowId == null ) {
                errors.Add( "You must select at least 1 workflow to assign." );
            } else {
                var workflow = db.ProjectWorkflows.FirstOrDefault(pwf => pwf.Id == model.WorkflowId);

                if ( workflow == null ) {
                    errors.Add( "Unable to locate the selected workflow." );
                } else {
                    foreach ( int projectId in model.ProjectIds ) {
                        var project = db.Projects.FirstOrDefault(p => p.Id == projectId);

                        if ( project != null ) {
                            var result = projectHelper.SetProjectWorkflow(projectId, workflow.Id, db);
                            if ( !result ) {
                                errors.Add( $"Unable to change the workflow of project '{project.Name}' to '{workflow.Name}'." );
                            } else {
                                successes.Add( $"{project.Name}'s workflow has been changed to {workflow.Name}" );
                            }
                        } else { //project ID was not found.
                            errors.Add( "Unable to locate one of the selected projects." );
                        }
                    }
                }
            }
            db.SaveChanges();

            if ( errors.Count > 0 ) {
                foreach ( var err in errors ) {
                    TempData.AddDangerToast( err );
                }
            }
            if ( successes.Count > 0 ) {
                foreach ( var msg in successes ) {
                    TempData.AddSuccessToast( msg );
                }
            }

            var viewModel = new ServerConfigViewModel()
            {
                Users = db.Users.ToList(),
                Projects = db.Projects.ToList(),
                Workflows = db.ProjectWorkflows.ToList()
            };

            return View( viewModel );
        }

        #endregion Project Configuration

        #region Helpers

        private void AddErrors( IdentityResult result ) {
            foreach ( var error in result.Errors ) {
                TempData.AddDangerToast( error );
            }
        }

        #endregion Helpers
    }
}