using System;

namespace Semplicita.Models
{
    public class TicketNotification
    {
        public int Id { get; set; }

        public int TicketId { get; set; }

        public string RecipientId { get; set; }

        public string SenderId { get; set; }

        public bool IsRead { get; set; }

        public string NotificationBody { get; set; }

        public DateTime Created { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser Sender { get; set; }
        public virtual ApplicationUser Recipient { get; set; }
    }
}