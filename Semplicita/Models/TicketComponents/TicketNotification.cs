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

        private static ICollection<string> GetNotificationRecipients( ApplicationDbContext context, string senderId, int ticketId, bool forceToReporter = false, bool forceToAllStaff = false ) {
            var permissionsHelper = new PermissionsHelper();
            var output = new List<string>();

            var ticket = context.Tickets.FirstOrDefault(t => t.Id == ticketId);
            if ( ticket == null ) { return null; }


            if ( senderId == ticket.ReporterId ) {
                //sender is reporter, send notification to staff
                if ( ticket.AssignedSolverId == null ) {
                    //if not assigned to solver, add project manager and super solvers in the project as recipients
                    output.Add( ticket.ParentProject.ProjectManagerId );
                    foreach ( ApplicationUser u in ticket.ParentProject.Members.ToList() ) {
                        if ( permissionsHelper.IsInRole( u.Id, "SuperSolver" ) ) {
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

            if(ticket.Watchers != null) {
                //add users on the ticket's watchlist who want to recieve notifications about the ticket
                output.AddRange( ticket.Watchers.Select( w => w.WatcherId ).ToList() );
            }


            if ( forceToReporter ) { output.Add( ticket.ReporterId ); }
            if ( forceToAllStaff ) {
                foreach ( ApplicationUser u in ticket.ParentProject.Members.ToList() ) {
                    if ( u.Id == ticket.ParentProject.ProjectManagerId || permissionsHelper.IsInRole( u.Id, "SuperSolver" ) ) {
                        output.Add( u.Id );
                    }
                }
                if (ticket.AssignedSolverId != null ) { output.Add( ticket.AssignedSolverId ); }
            }

            return output.Distinct().ToList();
        }
        public static ICollection<TicketNotification> GenerateNotifications( ApplicationDbContext context, string senderId, int ticketId, string body, bool forceToReporter = false, bool forceToAllStaff = false ) {
            var output = new List<TicketNotification>();
            var recipients = GetNotificationRecipients(context, senderId, ticketId);
            var now = DateTime.Now;

            foreach ( string recip in recipients ) {
                var notif = new TicketNotification() {
                    ParentTicketId = ticketId
                    ,Created = now
                    ,IsRead = false
                    ,NotificationBody = body
                    ,RecipientId = recip
                    ,SenderId = senderId
                };
                output.Add( notif );
            }

            return output;
        }
    }
}