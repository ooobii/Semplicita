namespace Semplicita.Models
{
    public class TicketHistoryEntry
    {
        public int Id { get; set; }

        public int ParentTicketId { get; set; }
        public string UserId { get; set; }

        public virtual Ticket ParentTicket { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}