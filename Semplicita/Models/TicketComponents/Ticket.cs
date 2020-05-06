using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Semplicita.Models
{
    public class Ticket
    {

        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastInteractionAt { get; set; }
        public DateTime? ResolvedAt { get; set; }


        public int ParentProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketStatusId { get; set; }
        public int TicketPriorityLevelId { get; set; }
        public string ReporterId { get; set; }
        public string AssignedSolverId { get; set; }


        public virtual Project ParentProject { get; set; }
        public virtual TicketType TicketType { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual TicketPriority TicketPriorityLevel { get; set; }
        public virtual ApplicationUser Reporter { get; set; }
        public virtual ApplicationUser AssignedSolver { get; set; }

        public virtual ICollection<TicketAttachment> Attachments { get; set; }
        public virtual ICollection<TicketComment> Comments { get; set; }
        public virtual ICollection<TicketHistoryEntry> HistoryEntries { get; set; }
        public virtual ICollection<TicketNotification> Notifications { get; set; }


        public Ticket() {
            Attachments = new HashSet<TicketAttachment>();
            Comments = new HashSet<TicketComment>();
            HistoryEntries = new HashSet<TicketHistoryEntry>();
            Notifications = new HashSet<TicketNotification>();
        }

    }
}