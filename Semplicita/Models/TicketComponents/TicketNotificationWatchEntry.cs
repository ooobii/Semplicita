using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Semplicita.Models
{
    public class TicketNotificationWatchEntry
    {
        public int Id { get; set; }


        public string WatcherId { get; set; }
        public int TicketId { get; set; }

        public virtual ApplicationUser Watcher { get; set; }
        public virtual Ticket Ticket { get; set; }

    }
}