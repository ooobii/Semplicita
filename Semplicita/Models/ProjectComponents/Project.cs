﻿using System;
using System.Collections.Generic;

namespace Semplicita.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }

        public bool IsActiveProject { get; set; }

        public virtual ICollection<ApplicationUser> Members { get; set; }
        public virtual ICollection<Ticket> ChildTickets { get; set; }

        public Project() {
            Members = new HashSet<ApplicationUser>();

            ChildTickets = new HashSet<Ticket>();
        }
    }
}