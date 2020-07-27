using Semplicita.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;

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

        public Project()
        {
            Members = new HashSet<ApplicationUser>();
            ChildTickets = new HashSet<Ticket>();
        }

        public PermissionsHelper.ProjectPermissionsContainer Permissions(IPrincipal User)
        {
            return new PermissionsHelper.ProjectPermissionsContainer(new PermissionsHelper(), User, this.Id);
        }

        public ApplicationUser GetNextSolverFromWorkflow()
        {
            switch (this.ActiveWorkflow.AutoTicketAssignBehavior)
            {
                case ProjectWorkflow.AutoTicketAssignBehaviorType.AutoAssignToUser:
                    if (this.ActiveWorkflow.AutoTicketAssignUserId != null && this.Members.Any(u => u.Id == this.ActiveWorkflow.AutoTicketAssignUserId))
                    {
                        return this.Members.FirstOrDefault(u => u.Id == this.ActiveWorkflow.AutoTicketAssignUserId);
                    }
                    else
                    {
                        return null;
                    }

                case ProjectWorkflow.AutoTicketAssignBehaviorType.LeaveUnassigned:
                case ProjectWorkflow.AutoTicketAssignBehaviorType.RoundRobin:
                case ProjectWorkflow.AutoTicketAssignBehaviorType.EvenSteven:
                case ProjectWorkflow.AutoTicketAssignBehaviorType.WorkloadBasedAvailability:
                default:
                    return null;
            }
        }

        public string GetNextSolverIdFromWorkflow()
        {
            var output = GetNextSolverFromWorkflow();
            return output?.Id;
        }

        public List<ApplicationUser> GetSolverMembers()
        {
            return this.Members.ToList().Where(m => m.IsInRole("SuperSolver") ||
                                                                 m.IsInRole("Solver"))
                                        .Where(u => u.IsDemoUser == false).ToList();
        }

        public List<ApplicationUser> GetReporterMembers()
        {
            return this.Members.ToList().Where(m => m.IsInRole("Reporter") && m.IsDemoUser == false).ToList();
        }

        private List<Ticket> GetUnassignedTickets()
        {
            return this.ChildTickets.Where(t => t.AssignedSolverId == null).ToList();
        }

        private List<Ticket> GetAssignedTickets()
        {
            return this.ChildTickets.Where(t => t.AssignedSolverId != null).ToList();
        }

        private List<Ticket> GetOpenTickets()
        {
            return this.ChildTickets.Where(t => !t.TicketStatus.IsClosed && !t.TicketStatus.IsArchived).ToList();
        }

        private List<Ticket> GetClosedTickets()
        {
            return this.ChildTickets.Where(t => t.TicketStatus.IsClosed).ToList();
        }

        private List<Ticket> GetResolvedTickets()
        {
            return this.ChildTickets.Where(t => t.TicketStatus.IsResolved).ToList();
        }

        private List<Ticket> GetArchivedTickets()
        {
            return this.ChildTickets.Where(t => t.TicketStatus.IsArchived).ToList();
        }

        public class TicketsContainer
        {
            public List<Ticket> All { get; set; }
            public List<Ticket> UnassignedTickets { get; set; }
            public List<Ticket> AssignedTickets { get; set; }
            public List<Ticket> OpenTickets { get; set; }
            public List<Ticket> ClosedTickets { get; set; }
            public List<Ticket> ResolvedTickets { get; set; }
            public List<Ticket> ArchivedTickets { get; set; }

            public TicketsContainer(Project project)
            {
                All = project.ChildTickets.ToList();
                UnassignedTickets = project.GetUnassignedTickets();
                AssignedTickets = project.GetAssignedTickets();
                OpenTickets = project.GetOpenTickets();
                ClosedTickets = project.GetClosedTickets();
                ResolvedTickets = project.GetResolvedTickets();
                ArchivedTickets = project.GetArchivedTickets();
            }
        }
    }
}