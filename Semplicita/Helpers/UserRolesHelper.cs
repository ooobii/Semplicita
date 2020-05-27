using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Semplicita.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
            foreach (var role in db.Roles.Select(r => r.Name).OrderBy(x =>
                x == "ServerAdmin" ? 1 :
                x == "ProjectAdmin" ? 2 :
                x == "SuperSolver" ? 3 :
                x == "Solver" ? 4 :
                x == "Reporter" ? 5 :
                6
            ).ToList()) {
                output.Add(roleDictionary[ role ]);
            }
            return output;
        }
        public ICollection<string> ListUserRoles(string userId) {
            return userManager.GetRoles(userId);
        }
        public ICollection<string> ListUserRoleDisplayNames(string userId) {
            var output = new List<string>();
            foreach( var role in userManager.GetRoles(userId).OrderBy(x =>
                x == "ServerAdmin" ? 1:
                x == "ProjectAdmin" ? 2:
                x == "SuperSolver" ? 3:
                x == "Solver" ? 4:
                x == "Reporter" ? 5:
                6
            )) {
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


    }
}