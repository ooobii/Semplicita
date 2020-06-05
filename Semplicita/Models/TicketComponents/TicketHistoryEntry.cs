using System;
using System.Text;
using System.Web;

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
            StatusChangedByWorkflow = 11,
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


        public DateTime OccuredAt { get; set; }


        public virtual Ticket ParentTicket { get; set; }
        public virtual ApplicationUser User { get; set; }

        public TicketHistoryEntryType EntryType { get; set; }
        public string OldData { get; set; }
        public string NewData { get; set; }


        public HtmlString GetHistoryTitleHtml() {            
            switch( this.EntryType ) {
                case TicketHistoryEntryType.Created:
                    return new HtmlString($"<u>{User.ShortName}</u> created the ticket '{ParentTicket.GetTicketIdentifier()}'.");

                case TicketHistoryEntryType.AssignedToSolver:
                    return new HtmlString($"<u>{User.ShortName}</u> <b class=\"text-success\">assigned</b> ticket'{ParentTicket.GetTicketIdentifier()}'");

                case TicketHistoryEntryType.AssignedToNewSolver:
                    return new HtmlString($"<u>{User.ShortName}</u> <b class=\"text-warning\"><i>re</i>-assigned</b> ticket'{ParentTicket.GetTicketIdentifier()}'");

                case TicketHistoryEntryType.UnassignedFromSolver:
                    return new HtmlString($"<u>{User.ShortName}</u> <b class=\"text-danger\">unassigned</b> ticket'{ParentTicket.GetTicketIdentifier()}'");

                case TicketHistoryEntryType.StatusChanged:
                    return new HtmlString($"Ticket status was updated by <u>{User.ShortName}</u>.");

                case TicketHistoryEntryType.StatusChangedByWorkflow:
                    return new HtmlString($"Ticket status was changed by workflow rules.");

                case TicketHistoryEntryType.AttachmentAdded:
                    return new HtmlString($"<u>{User.ShortName}</u> has uploaded an attachment.");

                case TicketHistoryEntryType.CommentAdded:
                    return new HtmlString($"<u>{User.ShortName}</u> added a comment.");

                case TicketHistoryEntryType.TicketTypeModified:
                    return new HtmlString($"<u>{User.ShortName}</u> updated the ticket type.");

                case TicketHistoryEntryType.TitleModified:
                    return new HtmlString($"<u>{User.ShortName}</u> changed the title.");

                case TicketHistoryEntryType.DescriptionModified:
                    return new HtmlString($"<u>{User.ShortName}</u> modified the description.");

                case TicketHistoryEntryType.PriorityChanged:
                    return new HtmlString($"The ticket's priority was updated by <u>{User.ShortName}</u>.");

                default:
                    return new HtmlString("");
            }
        }
        public HtmlString GetHistoryBodyHtml() {
            var db = new ApplicationDbContext();
            //https://localhost:44349/tickets/DEF2
            switch( this.EntryType ) {
                case TicketHistoryEntryType.Created:
                    return new HtmlString($"<h6><b>{this.NewData}</b></h6>");

                case TicketHistoryEntryType.AssignedToSolver:
                    return new HtmlString($"Assigned: <i class=\"fas fa-times-circle text-danger\"></i> <i class=\"fas fa-arrow-right\"></i> {db.Users.Find(this.NewData).FullNameStandard}");

                case TicketHistoryEntryType.AssignedToNewSolver:
                    return new HtmlString($"Assigned: '{db.Users.Find(this.NewData).FullNameStandard}' <i class=\"fas fa-arrow-left\"></i> <del class=\"text-secondary\">{db.Users.Find(OldData).FullNameStandard}</del>");

                case TicketHistoryEntryType.UnassignedFromSolver:
                    return new HtmlString($"Assigned: <del class=\"text-secondary\">{db.Users.Find(this.OldData).FullNameStandard}</del> <i class=\"fas fa-arrow-right\"></i> <i class=\"fas fa-times-circle text-danger\"></i>");

                case TicketHistoryEntryType.StatusChanged:
                case TicketHistoryEntryType.StatusChangedByWorkflow:
                    return new HtmlString($"Status: <del class=\"text-secondary\">{db.TicketStatuses.Find(int.Parse(this.OldData)).Display}</del> <i class=\"fas fa-arrow-right\"></i> {db.TicketStatuses.Find(int.Parse(this.NewData)).Display}");

                case TicketHistoryEntryType.AttachmentAdded:
                    return db.TicketAttachments.Find(int.Parse(NewData)).GetDisplayHtml();

                case TicketHistoryEntryType.CommentAdded:
                    return new HtmlString($"<p>{this.NewData}</p>");

                case TicketHistoryEntryType.TicketTypeModified:
                    return new HtmlString($"Issue type: <del class=\"text-secondary\">{db.TicketTypes.Find(int.Parse(this.OldData)).Name}</del> <i class=\"fas fa-arrow-right\"></i> {db.TicketTypes.Find(this.NewData).Name}");

                case TicketHistoryEntryType.TitleModified:
                    return new HtmlString($"Title: <del class=\"text-secondary\">{this.OldData}</del> <i class=\"fas fa-arrow-right\"></i> {this.NewData}");

                case TicketHistoryEntryType.DescriptionModified:
                    return new HtmlString(
                         "<div class=\"card p-3 full-width\">" +
                        $"  <del class=\"text-secondary\">{this.OldData}</del>" +
                         "</div></p>" +
                         "<div class=\"card p-3 full-width\">" +
                        $"  {this.NewData}" +
                         "</div>");

                case TicketHistoryEntryType.PriorityChanged:
                    return new HtmlString($"Priority: <del class=\"text-secondary\">{db.TicketPriorityTypes.Find(int.Parse(this.OldData)).Name}</del> <i class=\"fas fa-arrow-right\"></i> {db.TicketPriorityTypes.Find(int.Parse(this.NewData)).Name}");

                default:
                    return new HtmlString("");
            }
        }

    }
}