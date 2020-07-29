using Microsoft.AspNet.Identity;
using Semplicita.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace Semplicita.Helpers
{
    public class TicketsHelper
    {
        public ApplicationDbContext db { get; set; }

        public TicketsHelper( ApplicationDbContext context ) {
            db = context;
        }

        public TicketsHelper() {
            db = new ApplicationDbContext();
        }

        public ICollection<Ticket> TicketSearch( string content ) {
            var output = new List<Ticket>();

            return output;
        }

        public List<Ticket> GetTicketsAvailableToUser( IPrincipal User ) {
            List<Ticket> tickets = new List<Ticket>();
            string userId = User.Identity.GetUserId();

            if ( User.IsInRole( "ServerAdmin" ) ) {
                tickets = db.Tickets.ToList();
            } else if ( User.IsInRole( "ProjectAdmin" ) ) {
                tickets = db.Tickets.Where( t => t.ParentProject.ProjectManagerId == userId ).ToList();
            } else if ( User.IsInRole( "SuperSolver" ) ) {
                tickets = db.Tickets.Where( t => t.ParentProject.Members.Select( u => u.Id ).Contains( userId ) ).ToList();
            } else if ( User.IsInRole( "Solver" ) ) {
                tickets = db.Tickets.Where( t => t.AssignedSolverId == userId ).ToList();
            } else if ( User.IsInRole( "Reporter" ) ) {
                tickets = db.Tickets.ToList().Where( t => t.ReporterId == userId ).ToList();
            }

            return tickets;
        }
    }
}