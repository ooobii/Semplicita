using Semplicita.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Semplicita.Models
{
    public class ProjectWorkflow
    {
        public enum AutoTicketAssignBehaviorType
        {
            LeaveUnassigned = 0,
            AutoAssignToUser = 1,
            RoundRobin = 2,
            EvenSteven = 3,
            WorkloadBasedAvailability = 4
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }


        //ticket auto-assign behavior
        public AutoTicketAssignBehaviorType AutoTicketAssignBehavior { get; set; }
        public string AutoTicketAssignUserId { get; set; }
        public int? MinutesUntilAutoAssigned { get; set; }


        //event-based status change IDs for each role interaction with the ticket
        public int? InitialTicketStatusId { get; set; }
        public int? TicketAssignedStatusId { get; set; }
        public int? TicketReassignedStatusId { get; set; }
        public int? TicketUnassignedStatusId { get; set; }
        public int? ReporterInteractionStatusId { get; set; }
        public int? SolverInteractionStatusId { get; set; }
        public int? SuperSolverInteractionStatusId { get; set; }
        public int? ProjMgrInteractionStatusId { get; set; }
        public int? ServerAdminInteractionStatusId { get; set; }


        //allow reporter or staff (solver and above) to manually set ticket statuses after interaction
        public bool CanStaffSetStatusOnInteract { get; set; }
        public bool CanTicketOwnerSetStatusOnInteract { get; set; }

        public List<TicketStatus> GetAvailableStatuses(int TicketId, IPrincipal User, ApplicationDbContext context) {
            var rolesHelper = new UserRolesHelper(context);

            var output = new List<TicketStatus>();
            output.AddRange(context.TicketStatuses.ToList());

            var ticket = context.Tickets.Find(TicketId);
            var currStatus = ticket.TicketStatus;

            foreach( var s in context.TicketStatuses.ToList() ) {

                //if the status is only available to staff, and the user is not staff, remove it.
                if (s.IsForStaff && !rolesHelper.IsUserStaff(User)) {
                    output.Remove(s);
                    continue;
                }

                //if the status is only available to reporters, and the user is not the ticket's reporter, remove it.
                if( s.IsForReporter && !rolesHelper.IsTicketOwner(User, TicketId) ) {
                    output.Remove(s);
                    continue;
                }


                //the ticket has been started, only show statuses where the ticket was already started.
                if( currStatus.IsStarted ) {
                    if( s.IsStarted == false ) {
                        output.Remove(s);
                        continue;
                    }
                }
                if( currStatus.IsInProgress ) {
                    if( s.IsInProgress == false ) {
                        output.Remove(s);
                        continue;
                    }
                }

                if( ticket.AssignedSolverId == null ) {
                    //no solver is assigned, remove statuses that require the ticket to be assigned.
                    if( s.MustBeAssigned == true) {
                        output.Remove(s);
                        continue;
                    }
                } else {
                    //solver is alredy assigned, remove statuses that dont require solver
                    if( s.MustBeAssigned == false ) {
                        output.Remove(s);
                        continue;
                    }
                }

                //if the ticket has been resolved, remove statuses that dont require it being solved.
                if (currStatus.IsResolved) {
                    if (s.IsResolved == false) {
                        output.Remove(s);
                        continue;
                    }
                }


                //if the ticket has been closed, remove statuses that dont require it to be closed.
                if( currStatus.IsClosed ) {
                    if( s.IsClosed == false ) {
                        output.Remove(s);
                        continue;
                    }
                }

                //if the ticket has been canceled, remove statuses that dont require it to be cancled.
                if( currStatus.IsCanceled ) {
                    if( s.IsCanceled == false ) {
                        output.Remove(s);
                        continue;
                    }
                }

                //if the ticket has been canceled, remove statuses that dont require it to be cancled.
                if( currStatus.IsStarted ) {
                    if( s.IsStarted == false ) {
                        output.Remove(s);
                        continue;
                    }
                }

            }

            if( !output.Contains(currStatus) ) { output.Add(currStatus); }
            

            return output;
        }
    }
}