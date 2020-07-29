using System;
using System.Collections.Generic;
using System.Linq;
using Semplicita.Helpers;

namespace Semplicita.Models
{
    public class TicketNotification
    {
        public int Id { get; set; }
        public int ParentTicketId { get; set; }
        public string RecipientId { get; set; }
        public string SenderId { get; set; }

        public bool IsRead { get; set; }
        public string NotificationBody { get; set; }
        public DateTime Created { get; set; }

        public virtual Ticket ParentTicket { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        public virtual ApplicationUser Recipient { get; set; }

        private ICollection<string> GetNotificationRecipients(ApplicationDbContext context, string senderId, int ticketId) {
            var permissionsHelper = new PermissionsHelper();
            var output = new List<string>();

            var ticket = context.Tickets.FirstOrDefault(t => t.Id == ticketId);
            if(ticket == null) { return null; }

            if(senderId == ticket.ReporterId) {
                //sender is reporter, send notification to staff
                if ( ticket.AssignedSolverId == null ) {
                    //if not assigned to solver, add project manager and super solvers in the project as recipients (if not sender)
                    foreach ( ApplicationUser u in ticket.ParentProject.Members.ToList() ) {
                        if ( u.Id == ticket.ParentProject.ProjectManagerId || permissionsHelper.IsInRole( u.Id, "SuerSolver" ) ) {
                            output.Add( u.Id );
                        }
                    }

                } else {
                    //if ticket is assigned, send to solver assigned to ticket.
                    output.Add( ticket.AssignedSolverId );
                }

            } else {
                //sender is staff, send notification to reporter.
                output.Add( ticket.ReporterId );

            }

            //add users on the ticket's watchlist who want to recieve notifications about the ticket
            output.AddRange( ticket.Watchers.Select(w => w.WatcherId).ToList() );


            return output;
        }
    }
}