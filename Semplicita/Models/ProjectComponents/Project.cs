using Semplicita.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.WebSockets;

namespace Semplicita.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 1)]
        public string TicketTag { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ProjectManagerId { get; set; }



        public bool IsActiveProject { get; set; }

        public virtual ICollection<ApplicationUser> Members { get; set; }
        public virtual ICollection<Ticket> ChildTickets { get; set; }

        public int ActiveWorkflowId { get; set; }
        public virtual ProjectWorkflow ActiveWorkflow { get; set; }


        public Project() {
            Members = new HashSet<ApplicationUser>();
            ChildTickets = new HashSet<Ticket>();
        }


        public ApplicationUser GetNextSolverFromWorkflow(ApplicationDbContext context) {
            switch( this.ActiveWorkflow.AutoTicketAssignBehavior ) {
                case ProjectWorkflow.AutoTicketAssignBehaviorType.LeaveUnassigned:
                    return null;

                case ProjectWorkflow.AutoTicketAssignBehaviorType.AutoAssignToUser:
                    if( this.ActiveWorkflow.AutoTicketAssignUserId != null && this.Members.Any(u => u.Id == this.ActiveWorkflow.AutoTicketAssignUserId) ) {
                        return context.Users.ToList().FirstOrDefault(u => u.Id == this.ActiveWorkflow.AutoTicketAssignUserId);
                    } else {
                        return null;
                    }

                case ProjectWorkflow.AutoTicketAssignBehaviorType.RoundRobin:
                case ProjectWorkflow.AutoTicketAssignBehaviorType.EvenSteven:
                case ProjectWorkflow.AutoTicketAssignBehaviorType.WorkloadBasedAvailability:
                default:
                    return null;
            }
        }
        public string GetNextSolverIdFromWorkflow(ApplicationDbContext context) {
            var output = GetNextSolverFromWorkflow(context);
            if ( output != null ) { return output.Id; } else { return null; }
        }


        public List<ApplicationUser> GetSolverMembers(ApplicationDbContext context) {
            var roleHelper = new RolesHelper(context);

            var output = new List<ApplicationUser>();

            foreach(ApplicationUser u in this.Members.ToList().Where(m => roleHelper.ListUserRoles(m.Id).Contains("SuperSolver") ||
                                                                          roleHelper.ListUserRoles(m.Id).Contains("Solver"))) {
                if( u.IsDemoUser == false ) { output.Add(u); }
            }

            return output;
        }
        public List<ApplicationUser> GetReporterMembers(ApplicationDbContext context) {
            var roleHelper = new RolesHelper(context);

            var output = new List<ApplicationUser>();

            foreach( ApplicationUser u in this.Members.ToList().Where(m => roleHelper.ListUserRoles(m.Id).Contains("Reporter"))) {
                if( u.IsDemoUser == false ) { output.Add(u); }
            }

            return output;
        }



    }
}