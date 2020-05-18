using Microsoft.Ajax.Utilities;
using Semplicita.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Semplicita.Helpers
{
    public class ProjectHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

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

        public bool SetProjectWorkflow(int projectId, int workflowId) {
            if (db.Projects.Find(projectId).ActiveWorkflowId == workflowId) { return false; }

            try {
                var foundWorkflow = db.ProjectWorkflows.Find(workflowId);
                db.Projects.Find(projectId).ActiveWorkflowId = foundWorkflow.Id;
                //db.Projects.Find(projectId).ActiveWorkflow = foundWorkflow;
                db.SaveChanges();
                return true;
            } catch {
                return false;
            }

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