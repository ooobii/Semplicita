using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Semplicita.Models
{
    public class TicketsIndexViewModel
    {
        public ICollection<Ticket> Tickets { get; set; }

        public ICollection<Project> Projects { get; set; }
    }

    public class CreateTicketViewModel
    {
        public Project ParentProject { get; set; }
        public ApplicationUser Reporter { get; set; }

        public Dictionary<string, int> PrioritySelections { get; set; }
        public Dictionary<string, int> AvailableStatuses { get; set; }
        public ICollection<TicketType> AvailableTicketTypes { get; set; }

    }
    public class NewTicketModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        
        public int ParentProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketPriorityId { get; set; }
        public int? TicketStatusId { get; set; }


        public ICollection<HttpPostedFileBase> Attachments { get; set; }
    }


    public class EditTicketViewModel : CreateTicketViewModel
    {
        public Ticket CurrentTicket { get; set; }
        public ICollection<ApplicationUser> AvailableSolvers { get; set; }
    }
    public class EditTicketModel
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public int ParentProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketPriorityId { get; set; }
        public int? TicketStatusId { get; set; }

        public ICollection<HttpPostedFileBase> Attachments { get; set; }

        public int CurrentTicketId { get; set; }
        public string SolverId { get; set; }
    }

    public class AddTicketCommentModel
    {
        public int TicketId { get; set; }
        public string Body { get; set; }
    }

}