using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Semplicita.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastInteractionAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public int ParentProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TicketStatusId { get; set; }
        public int TicketPriorityLevelId { get; set; }
        public string ReporterId { get; set; }
        public string AssignedSolverId { get; set; }

        public virtual Project ParentProject { get; set; }
        public virtual TicketType TicketType { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual TicketPriority TicketPriorityLevel { get; set; }
        public virtual ApplicationUser Reporter { get; set; }
        public virtual ApplicationUser AssignedSolver { get; set; }

        public virtual ICollection<TicketAttachment> Attachments { get; set; }
        public virtual ICollection<TicketComment> Comments { get; set; }
        public virtual ICollection<TicketHistoryEntry> HistoryEntries { get; set; }
        public virtual ICollection<TicketNotification> Notifications { get; set; }


        public Ticket() {
            Attachments = new HashSet<TicketAttachment>();
            Comments = new HashSet<TicketComment>();
            HistoryEntries = new HashSet<TicketHistoryEntry>();
            Notifications = new HashSet<TicketNotification>();
        }


        public string GetTicketIdentifier() {
            return ParentProject.TicketTag + this.Id;
        }

        public void SaveTicketCreatedHistoryEntry(IPrincipal User, ApplicationDbContext context) {
            var createdEntry = this.GetCreatedHistoryEntry(User, context);

            context.TicketHistoryEntries.Add(createdEntry);
            context.SaveChanges();
        }
        private TicketHistoryEntry GetCreatedHistoryEntry(IPrincipal User, ApplicationDbContext context) {
            var history = context.TicketHistoryEntries.FirstOrDefault(the => the.EntryType == TicketHistoryEntry.TicketHistoryEntryType.Created
                                                                          && the.ParentTicketId == this.Id);
            if( history == null ) {
                //does not exist in db for this ticket yet, create a new one to be added.
                return new TicketHistoryEntry() {
                    UserId = User.Identity.GetUserId(),
                    ParentTicketId = this.Id,
                    EntryType = TicketHistoryEntry.TicketHistoryEntryType.Created,
                    OldData = null,
                    NewData = this.Title
                };

            } else {
                //already exists
                return history;

            }
        }


        public ICollection<TicketHistoryEntry> UpdateTicket(Ticket newTicket, IPrincipal User, ApplicationDbContext context) {
            var output = new List<TicketHistoryEntry>();

            //Check field changes
            if( this.Title != newTicket.Title ) { output.Add(UpdateTitle(newTicket.Title, User, context)); }
            if( this.Description != newTicket.Description ) { output.Add(UpdateDescription(newTicket.Description, User, context)); }
            if( this.TicketPriorityLevelId != newTicket.TicketPriorityLevelId ) { output.Add(UpdatePriority(newTicket.TicketPriorityLevelId, User, context)); }
            if( this.TicketTypeId != newTicket.TicketTypeId ) { output.Add(UpdateTicketType(newTicket.TicketTypeId, User, context)); }

            //Check ticket assignments
            if( this.AssignedSolverId == null & newTicket.AssignedSolverId != null ) { output.Add(UpdateSolverAssignment(newTicket.AssignedSolverId, User, context)); }
            if( this.AssignedSolverId != null & this.AssignedSolverId != newTicket.AssignedSolverId ) { output.Add(UpdateSolverReassigned(newTicket.AssignedSolverId, User, context)); }
            if( this.AssignedSolverId != null & newTicket.AssignedSolverId == null ) { output.Add(UpdateUnassigned(User, context)); }

            //Check status
            if( this.TicketStatusId != newTicket.TicketStatusId ) { output.Add(UpdateStatus(newTicket.TicketStatusId, User, context)); }


            return output;
        }
        private TicketHistoryEntry UpdateTitle(string newValue, IPrincipal User, ApplicationDbContext context) {
            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.TitleModified,
                OldData = this.Title,
                NewData = newValue
            };

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.Title = newValue;
            context.SaveChanges();

            return history;
        }
        private TicketHistoryEntry UpdateDescription(string newValue, IPrincipal User, ApplicationDbContext context) {
            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.DescriptionModified,
                OldData = this.Description,
                NewData = newValue
            };

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.Description = newValue;
            context.SaveChanges();

            return history;
        }
        private TicketHistoryEntry UpdatePriority(int newId, IPrincipal User, ApplicationDbContext context) {
            var newPriority = context.TicketPriorityTypes.Find(newId);

            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.PriorityChanged,
                OldData = this.TicketPriorityLevel.Name + "(" + this.TicketPriorityLevel.Rank + ")",
                NewData = newPriority.Name + "(" + newPriority.Rank + ")"
            };

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.TicketPriorityLevelId = newPriority.Id;
            context.SaveChanges();

            return history;
        }
        private TicketHistoryEntry UpdateTicketType(int newId, IPrincipal User, ApplicationDbContext context) {
            var newType = context.TicketTypes.Find(newId);

            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.TicketTypeModified,
                OldData = this.TicketType.Name + "(" + this.TicketType.Description + ")",
                NewData = newType.Name + "(" + newType.Description + ")"
            };

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.TicketTypeId = newType.Id;
            context.SaveChanges();

            return history;
        }
        private TicketHistoryEntry UpdateSolverAssignment(string newId, IPrincipal User, ApplicationDbContext context) {
            var newSolver = context.Users.Find(newId);

            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.AssignedToSolver,
                OldData = null,
                NewData = newSolver.Id
            };

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.AssignedSolverId = newSolver.Id;
            context.SaveChanges();

            return history;
        }
        private TicketHistoryEntry UpdateSolverReassigned(string newId, IPrincipal User, ApplicationDbContext context) {
            var newSolver = context.Users.Find(newId);

            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.AssignedToNewSolver,
                OldData = this.AssignedSolverId,
                NewData = newSolver.Id
            };

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.AssignedSolverId = newSolver.Id;
            context.SaveChanges();

            return history;
        }
        private TicketHistoryEntry UpdateUnassigned(IPrincipal User, ApplicationDbContext context) {
            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.UnassignedFromSolver,
                OldData = this.AssignedSolverId,
                NewData = null
            };

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.AssignedSolverId = null;
            context.SaveChanges();

            return history;
        }
        private TicketHistoryEntry UpdateStatus(int newId, IPrincipal User, ApplicationDbContext context) {
            var newStatus = context.TicketStatuses.Find(newId);

            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.StatusChanged,
                OldData = this.TicketStatus.Display,
                NewData = newStatus.Display
            };

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.TicketStatusId = newStatus.Id;
            context.SaveChanges();

            return history;
        }


        public TicketAttachment AddAttachment(HttpPostedFileBase file, IPrincipal User, ApplicationDbContext context, HttpServerUtilityBase Server) {
            var output = TicketAttachment.ProcessUpload(file, Server, this, User, context);
            
            context.TicketAttachments.Add(output);
            context.SaveChanges();

            return output;
        }
        public ICollection<TicketAttachment> AddAttachments(ICollection<HttpPostedFileBase> files, IPrincipal User, ApplicationDbContext context, HttpServerUtilityBase Server) {
            var output = new List<TicketAttachment>();
            foreach( HttpPostedFileBase file in files ) {
                try {
                    output.Add(TicketAttachment.ProcessUpload(file, Server, this, User, context));
                } catch { }
            }

            context.TicketAttachments.AddRange(output);
            context.SaveChanges();

            return output;
        }



    }
}