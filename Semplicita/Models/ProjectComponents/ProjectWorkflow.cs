using System;
using System.Collections.Generic;
using System.Linq;
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
        public int? ReporterInteractionStatusId { get; set; }
        public int? SolverInteractionStatusId { get; set; }
        public int? SuperSolverInteractionStatusId { get; set; }
        public int? ProjMgrInteractionStatusId { get; set; }
        public int? ServerAdminInteractionStatusId { get; set; }


        //allow reporter or staff (solver and above) to manually set ticket statuses after interaction
        public bool CanStaffSetStatusOnInteract { get; set; }
        public bool CanTicketOwnerSetStatusOnInteract { get; set; }



    }
}