using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
    }
}