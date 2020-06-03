using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Semplicita.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? LastInteractionAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        [Required]
        public int ParentProjectId { get; set; }

        [Required]
        public int TicketTypeId { get; set; }

        [Required]
        public int TicketStatusId { get; set; }

        [Required]
        public int TicketPriorityLevelId { get; set; }

        [Required]
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
            var createdEntry = this.MakeCreatedHistoryEntry(User, context);

            if( createdEntry != null ) {
                context.TicketHistoryEntries.Add(createdEntry);
                context.SaveChanges();
            }
        }
        private TicketHistoryEntry MakeCreatedHistoryEntry(IPrincipal User, ApplicationDbContext context) {
            var history = context.TicketHistoryEntries.FirstOrDefault(the => the.EntryType == TicketHistoryEntry.TicketHistoryEntryType.Created
                                                                           & the.ParentTicketId == this.Id);
            if( history == null ) {
                //does not exist in db for this ticket yet, create a new one to be added.
                return new TicketHistoryEntry() {
                    UserId = User.Identity.GetUserId(),
                    ParentTicketId = this.Id,
                    EntryType = TicketHistoryEntry.TicketHistoryEntryType.Created,
                    OldData = null,
                    NewData = this.Title,
                    OccuredAt = this.CreatedAt
                };

            } else {
                //already exists
                return null;

            }
        }

        public ICollection<TicketHistoryEntry> UpdateTicket(Ticket newTicket, IPrincipal User, ApplicationDbContext context) {
            var output = new List<TicketHistoryEntry>();

            //Check status change (before workflow potentially modifies it)
            if( this.TicketStatusId != newTicket.TicketStatusId ) { output.Add(UpdateStatus(newTicket.TicketStatusId, User, context)); }

            //Check field changes
            if( this.Title != newTicket.Title ) { output.Add(UpdateTitle(newTicket.Title, User, context)); }
            if( this.Description != newTicket.Description ) { output.Add(UpdateDescription(newTicket.Description, User, context)); }
            if( this.TicketPriorityLevelId != newTicket.TicketPriorityLevelId ) { output.Add(UpdatePriority(newTicket.TicketPriorityLevelId, User, context)); }
            if( this.TicketTypeId != newTicket.TicketTypeId ) { output.Add(UpdateTicketType(newTicket.TicketTypeId, User, context)); }

            //Check ticket assignments, but only check once.
            if( string.IsNullOrWhiteSpace(this.AssignedSolverId) & newTicket.AssignedSolverId != null ) {
                output.AddRange(UpdateSolverAssignment(newTicket.AssignedSolverId, User, context));
            } else if( !string.IsNullOrWhiteSpace(this.AssignedSolverId) & newTicket.AssignedSolverId == null ) {
                output.AddRange(UpdateUnassigned(User, context));
            } else if( !string.IsNullOrWhiteSpace(this.AssignedSolverId) & this.AssignedSolverId != newTicket.AssignedSolverId ) {
                output.AddRange(UpdateSolverReassigned(newTicket.AssignedSolverId, User, context));
            }



            return output;
        }
        private TicketHistoryEntry UpdateTitle(string newValue, IPrincipal User, ApplicationDbContext context) {
            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.TitleModified,
                OldData = this.Title,
                NewData = newValue,
                OccuredAt = DateTime.Now
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
                NewData = newValue,
                OccuredAt = DateTime.Now
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
                OldData = this.TicketPriorityLevel.Id.ToString(),
                NewData = newPriority.Id.ToString(),
                OccuredAt = DateTime.Now
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
                OldData = this.TicketType.Id.ToString(),
                NewData = newType.Id.ToString(),
                OccuredAt = DateTime.Now
            };

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.TicketTypeId = newType.Id;
            ticket.TicketType = newType;
            context.SaveChanges();

            return history;
        }
        private ICollection<TicketHistoryEntry> UpdateSolverAssignment(string newId, IPrincipal User, ApplicationDbContext context) {
            var output = new List<TicketHistoryEntry>();
            var newSolver = context.Users.Find(newId);

            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.AssignedToSolver,
                OldData = null,
                NewData = newSolver.Id,
                OccuredAt = DateTime.Now
            };
            output.Add(history);

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.AssignedSolverId = newSolver.Id;
            if( ticket.ParentProject.ActiveWorkflow.TicketAssignedStatusId != null ) {
                output.Add(UpdateStatus(int.Parse(ticket.ParentProject.ActiveWorkflow.TicketAssignedStatusId.ToString()), User, context, false));
                //ticket.TicketStatusId = int.Parse(ticket.ParentProject.ActiveWorkflow.TicketAssignedStatusId.ToString());
                //this.TicketStatusId = int.Parse(ticket.ParentProject.ActiveWorkflow.TicketAssignedStatusId.ToString());
            }
            context.SaveChanges();

            return output;
        }
        private ICollection<TicketHistoryEntry> UpdateSolverReassigned(string newId, IPrincipal User, ApplicationDbContext context) {
            var output = new List<TicketHistoryEntry>();
            var newSolver = context.Users.Find(newId);

            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.AssignedToNewSolver,
                OldData = this.AssignedSolverId,
                NewData = newSolver.Id,
                OccuredAt = DateTime.Now
            };
            output.Add(history);

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.AssignedSolverId = newSolver.Id;
            if( ticket.ParentProject.ActiveWorkflow.TicketReassignedStatusId != null ) {
                output.Add(UpdateStatus(int.Parse(ticket.ParentProject.ActiveWorkflow.TicketReassignedStatusId.ToString()), User, context, false));

                //ticket.TicketStatusId = int.Parse(ticket.ParentProject.ActiveWorkflow.TicketReassignedStatusId.ToString());
                //this.TicketStatusId = int.Parse(ticket.ParentProject.ActiveWorkflow.TicketReassignedStatusId.ToString());
            }
            context.SaveChanges();

            return output;
        }
        private ICollection<TicketHistoryEntry> UpdateUnassigned(IPrincipal User, ApplicationDbContext context) {
            var output = new List<TicketHistoryEntry>();
            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.UnassignedFromSolver,
                OldData = this.AssignedSolverId,
                NewData = null,
                OccuredAt = DateTime.Now
            };
            output.Add(history);

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.AssignedSolverId = null;
            if( ticket.ParentProject.ActiveWorkflow.TicketReassignedStatusId != null ) {
                output.Add(UpdateStatus(int.Parse(ticket.ParentProject.ActiveWorkflow.TicketReassignedStatusId.ToString()), User, context, false));
            }
            context.SaveChanges();

            return output;
        }
        private TicketHistoryEntry UpdateStatus(int newId, IPrincipal User, ApplicationDbContext context, bool save = true) {
            var newStatus = context.TicketStatuses.Find(newId);

            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.StatusChangedByWorkflow,
                OldData = this.TicketStatus.Id.ToString(),
                NewData = newStatus.Id.ToString(),
                OccuredAt = DateTime.Now
            };

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.TicketStatusId = newStatus.Id;

            if( save ) { context.SaveChanges(); }

            return history;
        }


        public TicketAttachment AddAttachment(HttpPostedFileBase file, IPrincipal User, ApplicationDbContext context, HttpServerUtilityBase Server) {
            var output = TicketAttachment.ProcessUpload(file, Server, this, User, context);

            context.TicketAttachments.Add(output);
            context.SaveChanges();

            context.TicketHistoryEntries.Add(GetAddedAttachmentHistory(output, User, context));
            context.SaveChanges();

            return output;
        }
        public ICollection<TicketAttachment> AddAttachments(ICollection<HttpPostedFileBase> files, IPrincipal User, ApplicationDbContext context, HttpServerUtilityBase Server) {
            //Upload and add attachment models
            var output = new List<TicketAttachment>();
            foreach( HttpPostedFileBase file in files ) {
                try {
                    output.Add(TicketAttachment.ProcessUpload(file, Server, this, User, context));
                } catch { }
            }
            context.TicketAttachments.AddRange(output);
            context.SaveChanges();

            //Generate and add history entries for uploaded histories
            var histories = new List<TicketHistoryEntry>();
            foreach( TicketAttachment attach in output ) {
                try {
                    histories.Add(GetAddedAttachmentHistory(attach, User, context));
                } catch { }
            }
            context.TicketHistoryEntries.AddRange(histories);
            context.SaveChanges();


            return output;
        }
        private TicketHistoryEntry GetAddedAttachmentHistory(TicketAttachment attach, IPrincipal User, ApplicationDbContext context) {
            var history = new TicketHistoryEntry() {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.AttachmentAdded,
                OldData = null,
                NewData = attach.Id.ToString(),
                OccuredAt = attach.UploadedAt
            };

            return history;
        }

    }
}