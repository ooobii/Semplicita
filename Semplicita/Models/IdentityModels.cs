using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Semplicita.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsDemoUser { get; set; }
        public string AvatarImagePath { get; set; }

        [NotMapped]
        public string FullName => $"{LastName}, {FirstName}";

        [NotMapped]
        public string FullNameStandard => $"{FirstName} {LastName}";

        [NotMapped]
        public string ShortName => $"{FirstName} {LastName.Substring(0, 1)}.";

        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<TicketComment> CommentsWritten { get; set; }
        public virtual ICollection<TicketAttachment> UploadedAttachments { get; set; }
        public virtual ICollection<TicketHistoryEntry> HistoryEntries { get; set; }

        public ApplicationUser() {
            Projects = new HashSet<Project>();

            CommentsWritten = new HashSet<TicketComment>();
            UploadedAttachments = new HashSet<TicketAttachment>();
            HistoryEntries = new HashSet<TicketHistoryEntry>();
        }


        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager) {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }



        public HtmlString GetRoleBadges() {
            var output = new StringBuilder();
            var mgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            if( mgr.IsInRole(this.Id, "ServerAdmin") ) { output.Append("<small class=\"badge badge-dark align-top\">Server Admin</small>"); }
            if( mgr.IsInRole(this.Id, "ProjectAdmin") ) { output.Append("<small class=\"badge badge-blue align-top\">Project Mgr</small>"); }
            if( mgr.IsInRole(this.Id, "SuperSolver") ) { output.Append("<small class=\"badge badge-secondary align-top\">Super Solver</small>"); }
            if( mgr.IsInRole(this.Id, "Solver") ) { output.Append("<small class=\"badge badge-success align-top\">Solver</small>"); }
            if( mgr.IsInRole(this.Id, "Reporter") ) { output.Append("<small class=\"badge badge-info align-top\">Reporter</small>"); }

            return new HtmlString(output.ToString());
        }
        public HtmlString GetStaffBadge(int fontSize = 12) {
            var mgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            if( mgr.IsInRole(this.Id, "ServerAdmin") ) { return new HtmlString($"<small class=\"badge badge-dark align-top\" style=\"font-size:{fontSize}px !important\">Server Admin</small>"); }
            if( mgr.IsInRole(this.Id, "ProjectAdmin") ) { return new HtmlString($"<small class=\"badge badge-blue align-top\" style=\"font-size:{fontSize}px !important\">Project Mgr</small>"); }
            if( mgr.IsInRole(this.Id, "SuperSolver") ) { return new HtmlString($"<small class=\"badge badge-secondary align-top\" style=\"font-size:{fontSize}px !important\">Super Solver</small>"); }
            if( mgr.IsInRole(this.Id, "Solver") ) { return new HtmlString($"<small class=\"badge badge-success align-top\" style=\"font-size:{fontSize}px !important\">Solver</small>"); }

            return new HtmlString("");
        }

        internal bool IsInRole(string role) {
            var mgr = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            return mgr.IsInRole(this.Id, role);
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false) {
        }

        public static ApplicationDbContext Create() {
            return new ApplicationDbContext();
        }

        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectWorkflow> ProjectWorkflows { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketHistoryEntry> TicketHistoryEntries { get; set; }
        public DbSet<TicketNotification> TicketNotifications { get; set; }
        public DbSet<TicketPriority> TicketPriorityTypes { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
    }
}