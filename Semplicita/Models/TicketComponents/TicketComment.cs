using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Semplicita.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdateReason { get; set; }


        public string AuthorId { get; set; }
        public int ParentTicketId { get; set; }

        public virtual ApplicationUser Author { get; set; }
        public virtual Ticket ParentTicket { get; set; }


    }
}