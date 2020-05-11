namespace Semplicita.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        public int ParentTicketId { get; set; }
        public string AuthorId { get; set; }

        public string Name { get; set; }

        public virtual Ticket ParentTicket { get; set; }
        public virtual ApplicationUser Author { get; set; }
    }
}