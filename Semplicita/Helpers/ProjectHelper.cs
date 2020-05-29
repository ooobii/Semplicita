using Semplicita.Models;
using System.Collections.Generic;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Security.Principal;

namespace Semplicita.Helpers
{
    public class ProjectHelper
    {
        public ApplicationDbContext db { get; set; }
        public ProjectHelper(ApplicationDbContext context) {
            db = context;
        }
        public ProjectHelper() {
            db = new ApplicationDbContext();
        }

        public bool IsUserOnProject(string userId, int projectId) {
            return db.Projects.Find(projectId).Members.Any(u => u.Id == userId);
        }
        public ICollection<Project> ProjectsAssignedToUser(string userId) {
            return db.Users.Find(userId).Projects.ToList();
        }
        public void AddUserToProject(string userId, int projectId, out bool result) {
            if( IsUserOnProject(userId, projectId) ) { result = false; return; }

            try {
                db.Projects.Find(projectId).Members.Add(db.Users.Find(userId));
                db.SaveChanges();
                result = true;
            } catch {
                result = false;
            }

        }
        public bool AddUserToProject(string userId, int projectId) {
            if( IsUserOnProject(userId, projectId) ) { return false; }

            try {
                db.Projects.Find(projectId).Members.Add(db.Users.Find(userId));
                db.SaveChanges();
                return true;
            } catch {
                return false;
            }

        }
        public void RemoveUserFromProject(string userId, int projectId, out bool result) {
            if( !IsUserOnProject(userId, projectId) ) { result = false; return; }

            try {
                db.Projects.Find(projectId).Members.Add(db.Users.Find(userId));
                db.SaveChanges();
                result = true;
            } catch {
                result = false;
            }
        }
        public bool RemoveUserFromProject(string userId, int projectId) {
            if( !IsUserOnProject(userId, projectId) ) { return false; }

            try {
                db.Projects.Find(projectId).Members.Remove(db.Users.Find(userId));
                db.SaveChanges();
                return true;
            } catch {
                return false;
            }
        }

        public ICollection<ApplicationUser> UsersOnProject(int projectId) {
            return db.Projects.Find(projectId).Members;
        }
        public ICollection<ApplicationUser> UsersNotOnProject(int projectId) {
            return db.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToList();
        }

        public List<Project> GetProjectsAvailableToUser(IPrincipal User) {
            var projects = new List<Project>();
            var user = db.Users.Find(User.Identity.GetUserId());

            if( User.IsInRole("ServerAdmin") ) {
                projects = db.Projects.ToList();

            } else if( User.IsInRole("ProjectAdmin") ||
                       User.IsInRole("SuperSolver") ||
                       User.IsInRole("Solver") ||
                       User.IsInRole("Reporter") ) {
                projects = db.Projects.ToList().Where(p => IsUserOnProject(user.Id, p.Id)).ToList();
            }

            return projects;
        }

        public bool SetProjectWorkflow(int projectId, int workflowId, ApplicationDbContext context) {
            if( context.Projects.Find(projectId).ActiveWorkflowId == workflowId ) { return false; }

            try {
                var foundWorkflow = db.ProjectWorkflows.Find(workflowId);
                context.Projects.Find(projectId).ActiveWorkflowId = foundWorkflow.Id;
                //context.Projects.Find(projectId).ActiveWorkflow = foundWorkflow;
                context.SaveChanges();
                return true;
            } catch {
                return false;
            }

        }
    }
}