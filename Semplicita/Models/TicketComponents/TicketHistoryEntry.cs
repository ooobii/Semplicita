namespace Semplicita.Models
{
    public class TicketHistoryEntry
    {
        public enum TicketHistoryEntryType
        {
            Created = 0,
            AssignedToSolver = 1,
            ReAssignedToSolver = 2,
            StatusChange = 3,
            AttachmentAdded = 4,
            CommentAdded = 5,
            Archived = 6,
            TicketTypeModified = 9,
            TitleModified = 7,
            DescriptionModified = 8
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