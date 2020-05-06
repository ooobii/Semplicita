using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Newtonsoft.Json.Linq;

namespace Semplicita.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AvatarImagePath { get; set; }

        [NotMapped]
        public string FullName { get { return $"{LastName}, {FirstName}"; } }

        [NotMapped]
        public string FullNameStandard { get { return $"{FirstName} {LastName}"; } }

        
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

        //public bool IsSolverUser() {
        //    return Roles.Select(r => r.ToString()).ToList().Contains("Solver");
        //}



        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager) {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
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
    }
}