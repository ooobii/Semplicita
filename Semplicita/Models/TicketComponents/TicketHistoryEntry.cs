namespace Semplicita.Models
{
    public class TicketHistoryEntry
    {
        public enum TicketHistoryEntryType
        {
            Created = 0,
            AssignedToSolver = 1,
            AssignedToNewSolver = 2,
            UnassignedFromSolver = 3,
            StatusChanged = 4,
            AttachmentAdded = 5,
            CommentAdded = 6,
            TicketTypeModified = 7,
            TitleModified = 8,
            DescriptionModified = 9,
            PriorityChanged = 10
        }

        public int Id { get; set; }

        public int ParentTicketId { get; set; }
        public string UserId { get; set; }

        public virtual Ticket ParentTicket { get; set; }
        public virtual ApplicationUser User { get; set; }


        public TicketHistoryEntryType EntryType { get; set; }
        public string OldData { get; set; }
        public string NewData { get; set; }
    }
}