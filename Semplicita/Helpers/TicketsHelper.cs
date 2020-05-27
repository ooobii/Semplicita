using Semplicita.Models;
using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Security.Cryptography.X509Certificates;

namespace Semplicita.Helpers
{
    public class TicketsHelper
    {
        public ApplicationDbContext db { get; set; }
        public TicketsHelper(ApplicationDbContext context) {
            db = context;
        }
        public TicketsHelper() {
            db = new ApplicationDbContext();
        }

        public ICollection<Ticket> TicketSearch(string content) {
            var output = new List<Ticket>();



            return output;
        }

        public List<Ticket> GetTicketsAvailableToUser(IPrincipal User) {
            List<Ticket> tickets = new List<Ticket>();

            if( User.IsInRole("ServerAdmin") ) {
                tickets = db.Tickets.ToList();

            } else if( User.IsInRole("ProjectAdmin") ) {
                tickets = db.Tickets.Where(t => t.ParentProject.ProjectManagerId == User.Identity.GetUserId()).ToList();

            } else if( User.IsInRole("SuperSolver") ) {
                tickets = db.Tickets.Where(t => t.ParentProject.Members.Contains(db.Users.Find(User.Identity.GetUserId()))).ToList();

            } else if( User.IsInRole("Solver") ) {
                tickets = db.Tickets.Where(t => t.AssignedSolver == db.Users.Find(User.Identity.GetUserId())).ToList();

            } else if( User.IsInRole("Reporter") ) {
                tickets = db.Tickets.Where(t => t.Reporter == db.Users.Find(User.Identity.GetUserId())).ToList();
            }

            return tickets;
        }

    }
}