using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Semplicita.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace Semplicita.Helpers
{
    public class UserRolesHelper
    {
        private RoleDisplayDictionary roleDictionary = new RoleDisplayDictionary();

        private ApplicationDbContext db { get; set; }
        public UserRolesHelper(ApplicationDbContext context) {
            db = context;
        }
        public UserRolesHelper() {
            db = new ApplicationDbContext();
        }

        private UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

        public ICollection<string> ListAllRoles() {
            return db.Roles.Select(r => r.Name).OrderBy(x =>
                x == "ServerAdmin" ? 1 :
                x == "ProjectAdmin" ? 2 :
                x == "SuperSolver" ? 3 :
                x == "Solver" ? 4 :
                x == "Reporter" ? 5 :
                6
            ).ToList();
        }
        public ICollection<string> ListAllRoleDisplayNames() {
            var output = new List<string>();
            foreach( var role in db.Roles.Select(r => r.Name).OrderBy(x =>
                x == "ServerAdmin" ? 1 :
                x == "ProjectAdmin" ? 2 :
                x == "SuperSolver" ? 3 :
                x == "Solver" ? 4 :
                x == "Reporter" ? 5 :
                6
            ).ToList() ) {
                output.Add(roleDictionary[ role ]);
            }
            return output;
        }
        public ICollection<string> ListUserRoles(string userId) {
            var output = new List<string>();
            foreach( var role in userManager.GetRoles(userId).OrderBy(x =>
                x == "ServerAdmin" ? 1 :
                x == "ProjectAdmin" ? 2 :
                x == "SuperSolver" ? 3 :
                x == "Solver" ? 4 :
                x == "Reporter" ? 5 :
                6
            ).ToList() ) {
                output.Add(role);
            }
            return output;
        }
        public ICollection<string> ListUserRoleDisplayNames(string userId) {
            var output = new List<string>();
            foreach( var role in userManager.GetRoles(userId).OrderBy(x =>
                x == "ServerAdmin" ? 1 :
                x == "ProjectAdmin" ? 2 :
                x == "SuperSolver" ? 3 :
                x == "Solver" ? 4 :
                x == "Reporter" ? 5 :
                6
            ) ) {
                output.Add(roleDictionary[ role ]);
            }
            return output;
        }
        public bool IsInRole(string userId, string role) {
            return userManager.IsInRole(userId, role);
        }
        public void AddUserToRole(string userId, string role, out bool result) {
            result = userManager.AddToRole(userId, role).Succeeded;
        }
        public bool AddUserToRole(string userId, string role) {
            return userManager.AddToRole(userId, role).Succeeded;
        }
        public void RemoveUserFromRole(string userId, string role, out bool result) {
            result = userManager.RemoveFromRole(userId, role).Succeeded;
        }
        public bool RemoveUserFromRole(string userId, string role) {
            return userManager.RemoveFromRole(userId, role).Succeeded;
        }
        public ICollection<ApplicationUser> UsersInRole(string role) {
            var output = new List<ApplicationUser>();
            var list = userManager.Users.Where(u => IsInRole(u.Id, role)).ToList();
            output.AddRange(list);
            return output;
        }
        public ICollection<ApplicationUser> UsersNotInRole(string role) {
            var output = new List<ApplicationUser>();
            var list = userManager.Users.Where(u => !IsInRole(u.Id, role)).ToList();
            output.AddRange(list);
            return output;
        }


        public bool CanCreateProject(IPrincipal User) {
            //Server Admins can OR
            //Project Admins can

            if( User.IsInRole("ServerAdmin") || User.IsInRole("ProjectAdmin") ) {
                return true;
            } else {
                return false;
            }
        }
        public bool CanViewProject(IPrincipal User, int projectId) {
            //Server Admins can OR
            //Project Admins can IF assigned as project manager OR
            //Any other role (as long as user is member of project) can
            var project = db.Projects.Find(projectId);
            var userId = User.Identity.GetUserId();

            if( User.IsInRole("ServerAdmin") ||
                ( User.IsInRole("ProjectAdmin") && project.ProjectManagerId == userId ) ||
                project.Members.Select(u => u.Id).Contains(userId) ) {
                return true;
            } else {
                return false;
            }
        }
        public bool CanEditProject(IPrincipal User, int projectId) {
            //Server Admins can OR
            //Project Admins can IF assigned as project manager.

            var project = db.Projects.Find(projectId);
            var userId = User.Identity.GetUserId();

            if( User.IsInRole("ServerAdmin") ||
               ( User.IsInRole("ProjectAdmin") && project.ProjectManagerId == userId ) ) {
                return true;
            } else {
                return false;
            }
        }
        public bool CanArchiveProject(IPrincipal User, int projectId) {
            //Server Admins can OR
            //Project Admins can IF assigned as project manager.

            var project = db.Projects.Find(projectId);
            var userId = User.Identity.GetUserId();

            if( User.IsInRole("ServerAdmin") ||
               ( User.IsInRole("ProjectAdmin") && project.ProjectManagerId == userId ) ) {
                return true;
            } else {
                return false;
            }
        }


    }

}


