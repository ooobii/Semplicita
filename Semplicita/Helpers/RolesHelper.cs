using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Semplicita.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Web;
using System.Web.Security;

namespace Semplicita.Helpers
{
    public class RolesHelper
    {
        private RoleDisplayDictionary roleDictionary = new RoleDisplayDictionary();

        private ApplicationDbContext db { get; set; }
        public RolesHelper(ApplicationDbContext context) {
            db = context;
        }
        public RolesHelper() {
            db = new ApplicationDbContext();
        }

        private UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

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
        public string GetHighestRole(IPrincipal User) {
            var roles = ListUserRoles(User.Identity.GetUserId());
            return roles.Count > 0 ? roles.ToList()[ 0 ] : "";
        }
        public bool IsInRole(string userId, string role) {
            return userManager.IsInRole(userId, role);
        }
        public bool AddUserToRole(string userId, string role) {
            return userManager.AddToRole(userId, role).Succeeded;
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


        private bool IsUserStaff(IPrincipal User) {
            if( User.IsInRole("ServerAdmin") ||
                 User.IsInRole("ProjectAdmin") ||
                 User.IsInRole("SuperSolver") ||
                 User.IsInRole("Solver") ) {
                return true;
            } else {
                return false;
            }
        }

        private bool CanCreateProject(IPrincipal User) {
            //Server Admins can OR
            //Project Admins can

            if( User.IsInRole("ServerAdmin") || User.IsInRole("ProjectAdmin") ) {
                return true;
            } else {
                return false;
            }
        }
        private bool CanViewProject(IPrincipal User, int projectId) {
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
        private bool CanViewAllProjects(IPrincipal User) {
            //ONLY Server Admins can
            return User.IsInRole("ServerAdmin");
        }
        private bool CanEditProject(IPrincipal User, int projectId) {
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
        private bool CanArchiveProject(IPrincipal User, int projectId) {
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
        private bool IsProjectManager(IPrincipal User, int projectId) {
            var mgrId = db.Projects.Find(projectId).ProjectManagerId;
            var isProjManager = User.IsInRole("ProjectAdmin");

            return ( User.Identity.GetUserId() == mgrId ) && isProjManager;
        }
        public ICollection<Project> GetViewableProjects(IPrincipal User) {
            var output = new List<Project>();

            foreach( Project p in db.Projects.ToList().Where(p => CanViewProject(User, p.Id)) ) {
                output.Add(p);
            }

            return output;
        }
        private ICollection<Project> GetEditableProjects(IPrincipal User) {
            var output = new List<Project>();

            foreach( Project p in db.Projects.ToList().Where(p => CanEditProject(User, p.Id)) ) {
                output.Add(p);
            }

            return output;
        }
        private ApplicationUser GetProjectManager(IPrincipal User, int projectId) {
            if( !CanViewProject(User, projectId) ) { return null; }

            var projadminId = db.Projects.Find(projectId).ProjectManagerId;
            return db.Users.FirstOrDefault(u => u.Id == projadminId);
        }


        private bool CanCreateTicket(IPrincipal User, int projectId) {
            //Reporters Only

            var userId = User.Identity.GetUserId();

            return User.IsInRole("Reporter") && db.Projects.Find(projectId).Members.Select(u => u.Id).Contains(userId);
        }
        private bool CanCreateTicket(IPrincipal User) {
            //Reporters Only

            var userId = User.Identity.GetUserId();

            if( User.IsInRole("Reporter") ) {
                return true;
            } else {
                return false;
            }
        }
        private bool CanViewTicket(IPrincipal User, int TicketId) {
            //ServerAdmin can OR
            //ProjectAdmin can IF assigned as parent project manager OR
            //SuperSolver or Solver can IF member of parent project
            //Reporter can IF owner of ticket
            var Ticket = db.Tickets.Find(TicketId);
            var userId = User.Identity.GetUserId();

            if( User.IsInRole("ServerAdmin") ||
                IsProjectManager(User, Ticket.ParentProjectId) ||
                ( ( User.IsInRole("SuperSolver") || User.IsInRole("Solver") ) && Ticket.ParentProject.Members.Select(u => u.Id).Contains(userId) ) ||
                ( User.IsInRole("Reporter") && Ticket.ReporterId == userId ) ) {
                return true;

            } else {
                return false;
            }
        }
        private bool CanViewAllTicketsInProject(IPrincipal User, int projectId) {
            //ServerAdmin can
            //ProjectAdmin can IF is project's manager
            //SuperSolver can IF they are member of the project
            //Solver can IF they are member of the project
            //Reporter CAN NOT

            var project = db.Projects.Find(projectId);
            var userId = User.Identity.GetUserId();

            if( User.IsInRole("ServerAdmin") ||
                IsProjectManager(User, projectId) ||
                ( ( User.IsInRole("SuperSolver") || User.IsInRole("Solver") ) && project.Members.Select(u => u.Id).Contains(userId) ) ) {
                return true;

            } else {
                return false;
            }
        }
        private bool CanViewAllTickets(IPrincipal User) {
            //ServerAdmin can
            //Anyone else CAN NOT

            var userId = User.Identity.GetUserId();

            if( User.IsInRole("ServerAdmin") ) {
                return true;

            } else {
                return false;
            }
        }
        private bool CanEditTicket(IPrincipal User, int TicketId) {
            //ServerAdmin can OR
            //ProjectAdmin can IF assigned as project manager
            //SuperSolver can IF member of parent project
            //Solver can IF assigned as solver
            //Reporter can IF ticket owner

            var Ticket = db.Tickets.Find(TicketId);
            var userId = User.Identity.GetUserId();

            if( User.IsInRole("ServerAdmin") ||
               IsProjectManager(User, Ticket.ParentProjectId) ||
               IsEligibleTicketSolver(User, TicketId) ||
               ( User.IsInRole("Reporter") & Ticket.ReporterId == userId ) ) {
                return true;
            } else {
                return false;
            }
        }
        private bool CanCommentOnTicket(IPrincipal User, int TicketId) {
            //ServerAdmin can OR
            //ProjectAdmin can IF assigned as project manager
            //SuperSolver can IF member of parent project
            //Solver can IF assigned as solver
            //Reporter can IF ticket owner

            var Ticket = db.Tickets.Find(TicketId);
            var userId = User.Identity.GetUserId();

            return User.IsInRole("ServerAdmin") ||
                   IsProjectManager(User, Ticket.ParentProjectId) ||
                   IsEligibleTicketSolver(User, TicketId) ||
                   ( User.IsInRole("Reporter") & Ticket.ReporterId == userId );
        }
        private bool CanUpdateTicketStatus(IPrincipal User, int TicketId) {
            var ticket = db.Tickets.Find(TicketId);
            var workflow = ticket.ParentProject.ActiveWorkflow;

            return workflow.CanStaffSetStatusOnInteract && ( IsEligibleTicketSolver(User, TicketId) || IsProjectManager(User, ticket.ParentProject.Id) ) ||
                   workflow.CanTicketOwnerSetStatusOnInteract && ( IsTicketOwner(User, TicketId) );
        }
        private bool CanArchiveTicket(IPrincipal User, int TicketId) {
            //Server Admins can OR
            //ProjectAdmins can IF assigned as project manager.

            var Ticket = db.Tickets.Find(TicketId);
            var userId = User.Identity.GetUserId();

            if( User.IsInRole("ServerAdmin") ||
               IsProjectManager(User, Ticket.ParentProjectId) ) {
                return true;
            } else {
                return false;
            }
        }
        private bool IsEligibleTicketSolver(IPrincipal User, int TicketId) {
            //SuperSolver can IF member of parent project
            //Solver can IF assigned as solver
            //Anyone else CAN NOT

            var Ticket = db.Tickets.Find(TicketId);
            var userId = User.Identity.GetUserId();

            if( ( User.IsInRole("SuperSolver") & Ticket.ParentProject.Members.Select(u => u.Id).Contains(userId) ) ||
               ( User.IsInRole("Solver") & Ticket.AssignedSolverId == userId ) ) {
                return true;
            } else {
                return false;
            }
        }
        private bool IsTicketOwner(IPrincipal User, int TicketId) {
            return User.Identity.GetUserId() == db.Tickets.Find(TicketId).ReporterId;
        }
        public ICollection<Ticket> GetViewableTickets(IPrincipal User) {
            var output = new List<Ticket>();

            foreach( Ticket t in db.Tickets.ToList().Where(t => CanViewTicket(User, t.Id)) ) {
                output.Add(t);
            }

            return output;
        }
        private ICollection<Ticket> GetEditableTickets(IPrincipal User) {
            var output = new List<Ticket>();

            foreach( Ticket t in db.Tickets.ToList() ) {
                if( CanEditTicket(User, t.Id) ) {
                    output.Add(t);
                }
            }

            return output;
        }


        public class PermissionsContainer
        {
            public bool IsUserStaff { get; }

            public bool CanCreateTickets { get; }
            public bool CanCreateProjects { get; }
            public bool CanViewAllTickets { get; }
            public bool CanViewAllProjects { get; }

            public List<Project> ViewableProjects { get; }
            public List<Project> EditableProjects { get; }
            public List<Ticket> ViewableTickets { get; }
            public List<Ticket> EditableTickets { get; }

            public PermissionsContainer(RolesHelper helper, IPrincipal User, bool fetchViewables = true) {
                IsUserStaff = helper.IsUserStaff(User);

                CanCreateTickets = helper.CanCreateTicket(User);
                CanCreateProjects = helper.CanCreateProject(User);
                CanViewAllTickets = helper.CanViewAllTickets(User);
                CanViewAllProjects = helper.CanViewAllProjects(User);

                if( !fetchViewables ) return;
                ViewableProjects = helper.GetViewableProjects(User).ToList();
                EditableProjects = helper.GetEditableProjects(User).ToList();
                ViewableTickets = helper.GetViewableTickets(User).ToList();
                EditableTickets = helper.GetEditableTickets(User).ToList();
            }
        }
        public class TicketPermissionsContainer
        {
            public bool CanViewTicket { get; }
            public bool CanEditTicket { get; }
            public bool CanCommentOnTicket { get; }
            public bool CanUpdateTicketStatus { get; }
            public bool CanArchiveTicket { get; }
            public bool IsEligibleSolver { get; }
            public bool IsTicketOwner { get; }

            public TicketPermissionsContainer(RolesHelper helper, IPrincipal User, int ticketId) {
                CanViewTicket = helper.CanViewTicket(User, ticketId);
                CanEditTicket = helper.CanEditTicket(User, ticketId);
                CanCommentOnTicket = helper.CanCommentOnTicket(User, ticketId);
                CanUpdateTicketStatus = helper.CanUpdateTicketStatus(User, ticketId);
                CanArchiveTicket = helper.CanArchiveTicket(User, ticketId);
                IsEligibleSolver = helper.IsEligibleTicketSolver(User, ticketId);
                IsTicketOwner = helper.IsTicketOwner(User, ticketId);
            }
        }
        public class ProjectPermissionsContainer
        {
            public bool CanViewProject { get; }
            public bool CanEditProject { get; }
            public bool CanArchiveProject { get; }
            public bool IsProjectManager { get; }
            public bool CanCreateTicket { get; }
            public bool CanViewAllTicketsInProject { get; }

            public ApplicationUser ProjectManager { get; set; }


            public ProjectPermissionsContainer(RolesHelper helper, IPrincipal User, int projectId) {
                CanViewProject = helper.CanViewProject(User, projectId);
                CanEditProject = helper.CanEditProject(User, projectId);
                CanArchiveProject = helper.CanArchiveProject(User, projectId);
                IsProjectManager = helper.IsProjectManager(User, projectId);
                CanCreateTicket = helper.CanCreateTicket(User, projectId);
                CanViewAllTicketsInProject = helper.CanViewAllTicketsInProject(User, projectId);

                ProjectManager = helper.GetProjectManager(User, projectId);
            }
        }
    }

}


