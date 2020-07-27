using Microsoft.AspNet.Identity;
using Semplicita.Helpers;
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

        public Ticket()
        {
            Attachments = new HashSet<TicketAttachment>();
            Comments = new HashSet<TicketComment>();
            HistoryEntries = new HashSet<TicketHistoryEntry>();
            Notifications = new HashSet<TicketNotification>();
        }

        #region HtmlHelpers

        public HtmlString GetAssignmentBadgeHtml(bool fullName = false)
        {
            var style = $"style=\"font-size: 12px; font-weight:normal;\"";
            if (this.AssignedSolver != null)
            {
                string name;
                if (fullName) { name = this.AssignedSolver.FullNameStandard; } else { name = $"Solver: {this.AssignedSolver.ShortName}"; }

                return new HtmlString($"<span class=\"badge badge-secondary\" {style}>{name}</span>");
            }
            else
            {
                return new HtmlString("");
            }
        }

        public HtmlString GetLastInteractBadgeHtml()
        {
            var style = $"style=\"font-size: 12px; font-weight:normal;\"";

            if (this.LastInteractionAt != null)
            {
                var dif = DateTime.Now - DateTime.Parse(LastInteractionAt.ToString());
                var totalYears = dif.TotalDays != 0 ? int.Parse((dif.TotalDays / 365).ToString("0")) : 0;
                string timestring = "";

                if (totalYears != 0) { timestring = $"{totalYears}'yrs"; } else if (dif.Days != 0) { timestring = dif.ToString(@"%d'd '%h'hr '%m'min '%s'sec'"); } else if (dif.Hours != 0) { timestring = dif.ToString(@"%h'hr '%m'min '%s'sec'"); } else if (dif.Minutes != 0) { timestring = dif.ToString(@"%m'min '%s'sec'"); } else if (dif.Seconds != 0) { timestring = dif.ToString(@"%s'sec'"); }

                return new HtmlString($"<span class=\"badge badge-info\" {style}><i class=\"far fa-clock\"></i> {timestring}</span>");
            }
            else
            {
                return new HtmlString("");
            }

            //<small class="badge badge-danger"><i class="far fa-clock"></i> 2 mins</small>
        }

        #endregion HtmlHelpers

        public PermissionsHelper.TicketPermissionsContainer Permissions(IPrincipal User)
        {
            return new PermissionsHelper.TicketPermissionsContainer(new PermissionsHelper(), User, this.Id);
        }

        public string GetTicketIdentifier()
        {
            return ParentProject.TicketTag + this.Id;
        }

        public void SaveTicketCreatedHistoryEntry(IPrincipal User, ApplicationDbContext context)
        {
            var createdEntry = this.MakeCreatedHistoryEntry(User, context);

            if (createdEntry != null)
            {
                context.TicketHistoryEntries.Add(createdEntry);
                context.SaveChanges();
            }
        }

        private TicketHistoryEntry MakeCreatedHistoryEntry(IPrincipal User, ApplicationDbContext context)
        {
            var history = context.TicketHistoryEntries.FirstOrDefault(the => the.EntryType == TicketHistoryEntry.TicketHistoryEntryType.Created
                                                                           & the.ParentTicketId == this.Id);
            if (history == null)
            {
                //does not exist in db for this ticket yet, create a new one to be added.
                return new TicketHistoryEntry()
                {
                    UserId = User.Identity.GetUserId(),
                    ParentTicketId = this.Id,
                    EntryType = TicketHistoryEntry.TicketHistoryEntryType.Created,
                    OldData = null,
                    NewData = this.Title,
                    OccuredAt = this.CreatedAt
                };
            }
            else
            {
                //already exists
                return null;
            }
        }

        public TicketHistoryEntry ArchiveTicket(IPrincipal User, ApplicationDbContext context)
        {
            TicketHistoryEntry output = null;

            if (this.TicketStatus.IsResolved && this.ParentProject.ActiveWorkflow.ArchivedResolvedStatusId != null)
            {
                output = this.UpdateStatus(this.ParentProject.ActiveWorkflow.ArchivedResolvedStatusId.Value,
                                           User, context, true, true);
            }
            else
            if (!this.TicketStatus.IsResolved && this.ParentProject.ActiveWorkflow.ArchivedNotResolvedStatusId != null)
            {
                output = this.UpdateStatus(this.ParentProject.ActiveWorkflow.ArchivedNotResolvedStatusId.Value,
                                           User, context, true, true);
            }

            return output;
        }

        public ICollection<TicketHistoryEntry> UpdateTicket(Ticket newTicket, IPrincipal User, ApplicationDbContext context)
        {
            var output = new List<TicketHistoryEntry>();
            bool useWorkflowStatus = true;

            //Check status change (before workflow potentially modifies it)
            if (this.TicketStatusId != newTicket.TicketStatusId) { output.Add(UpdateStatus(newTicket.TicketStatusId, User, context)); useWorkflowStatus = false; }

            //Check field changes
            if (this.Title != newTicket.Title) { output.Add(UpdateTitle(newTicket.Title, User, context)); }
            if (this.Description != newTicket.Description) { output.Add(UpdateDescription(newTicket.Description, User, context)); }
            if (this.TicketPriorityLevelId != newTicket.TicketPriorityLevelId) { output.Add(UpdatePriority(newTicket.TicketPriorityLevelId, User, context)); }
            if (this.TicketTypeId != newTicket.TicketTypeId) { output.Add(UpdateTicketType(newTicket.TicketTypeId, User, context)); }

            //Check ticket assignments, but only check once.
            if (string.IsNullOrWhiteSpace(this.AssignedSolverId) & newTicket.AssignedSolverId != null)
            {
                output.AddRange(UpdateSolverAssignment(newTicket.AssignedSolverId, User, context, useWorkflowStatus));
            }
            else if (!string.IsNullOrWhiteSpace(this.AssignedSolverId) & newTicket.AssignedSolverId == null)
            {
                output.AddRange(UpdateUnassigned(User, context, useWorkflowStatus));
            }
            else if (!string.IsNullOrWhiteSpace(this.AssignedSolverId) & this.AssignedSolverId != newTicket.AssignedSolverId)
            {
                output.AddRange(UpdateSolverReassigned(newTicket.AssignedSolverId, User, context, useWorkflowStatus));
            }

            return output;
        }

        private TicketHistoryEntry UpdateTitle(string newValue, IPrincipal User, ApplicationDbContext context)
        {
            var history = new TicketHistoryEntry()
            {
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

        private TicketHistoryEntry UpdateDescription(string newValue, IPrincipal User, ApplicationDbContext context)
        {
            var history = new TicketHistoryEntry()
            {
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

        private TicketHistoryEntry UpdatePriority(int newId, IPrincipal User, ApplicationDbContext context)
        {
            var newPriority = context.TicketPriorityTypes.Find(newId);

            var history = new TicketHistoryEntry()
            {
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

        private TicketHistoryEntry UpdateTicketType(int newId, IPrincipal User, ApplicationDbContext context)
        {
            var newType = context.TicketTypes.Find(newId);

            var history = new TicketHistoryEntry()
            {
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

        private ICollection<TicketHistoryEntry> UpdateSolverAssignment(string newId, IPrincipal User, ApplicationDbContext context, bool statusByWorkflow = true)
        {
            var output = new List<TicketHistoryEntry>();
            var newSolver = context.Users.Find(newId);

            var history = new TicketHistoryEntry()
            {
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
            if (statusByWorkflow && ticket.ParentProject.ActiveWorkflow.TicketAssignedStatusId != null)
            {
                output.Add(UpdateStatus(int.Parse(ticket.ParentProject.ActiveWorkflow.TicketAssignedStatusId.ToString()), User, context, false, true));
            }
            context.SaveChanges();

            return output;
        }

        private ICollection<TicketHistoryEntry> UpdateSolverReassigned(string newId, IPrincipal User, ApplicationDbContext context, bool statusByWorkflow = true)
        {
            var output = new List<TicketHistoryEntry>();
            var newSolver = context.Users.Find(newId);

            var history = new TicketHistoryEntry()
            {
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
            if (statusByWorkflow && ticket.ParentProject.ActiveWorkflow.TicketReassignedStatusId != null)
            {
                output.Add(UpdateStatus(int.Parse(ticket.ParentProject.ActiveWorkflow.TicketReassignedStatusId.ToString()), User, context, false, true));
            }
            context.SaveChanges();

            return output;
        }

        private ICollection<TicketHistoryEntry> UpdateUnassigned(IPrincipal User, ApplicationDbContext context, bool statusByWorkflow = true)
        {
            var output = new List<TicketHistoryEntry>();
            var history = new TicketHistoryEntry()
            {
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
            if (statusByWorkflow && ticket.ParentProject.ActiveWorkflow.TicketReassignedStatusId != null)
            {
                output.Add(UpdateStatus(int.Parse(ticket.ParentProject.ActiveWorkflow.TicketReassignedStatusId.ToString()), User, context, false, true));
            }
            context.SaveChanges();

            return output;
        }

        private TicketHistoryEntry UpdateStatus(int newId, IPrincipal User, ApplicationDbContext context, bool save = true, bool workflow = false)
        {
            var newStatus = context.TicketStatuses.Find(newId);
            if (newId == this.TicketStatusId) { return null; }

            var history = new TicketHistoryEntry()
            {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = workflow ? (newStatus.IsArchived ? TicketHistoryEntry.TicketHistoryEntryType.Archived : TicketHistoryEntry.TicketHistoryEntryType.StatusChangedByWorkflow) : TicketHistoryEntry.TicketHistoryEntryType.StatusChanged,
                OldData = this.TicketStatus.Id.ToString(),
                NewData = newStatus.Id.ToString(),
                OccuredAt = DateTime.Now
            };

            var ticket = context.Tickets.Find(this.Id);
            ticket.LastInteractionAt = DateTime.Now;
            ticket.TicketStatusId = newStatus.Id;

            if (save) { context.SaveChanges(); }

            return history;
        }

        public TicketAttachment AddAttachment(HttpPostedFileBase file, IPrincipal User, ApplicationDbContext context, HttpServerUtilityBase Server)
        {
            var output = TicketAttachment.ProcessNewUpload(file, Server, this, User, context);

            context.TicketAttachments.Add(output);
            context.SaveChanges();

            context.TicketHistoryEntries.Add(GetAddedAttachmentHistory(output, User, context));
            context.SaveChanges();

            return output;
        }

        public ICollection<TicketAttachment> AddAttachments(ICollection<HttpPostedFileBase> files, IPrincipal User, ApplicationDbContext context, HttpServerUtilityBase Server)
        {
            //Upload and add attachment models
            var output = new List<TicketAttachment>();
            foreach (HttpPostedFileBase file in files)
            {
                try
                {
                    output.Add(TicketAttachment.ProcessNewUpload(file, Server, this, User, context));
                }
                catch { }
            }
            context.TicketAttachments.AddRange(output);
            context.SaveChanges();

            //Generate and add history entries for uploaded histories
            var histories = new List<TicketHistoryEntry>();
            foreach (TicketAttachment attach in output)
            {
                try
                {
                    histories.Add(GetAddedAttachmentHistory(attach, User, context));
                }
                catch { }
            }
            context.TicketHistoryEntries.AddRange(histories);
            context.SaveChanges();

            return output;
        }

        private TicketHistoryEntry GetAddedAttachmentHistory(TicketAttachment attach, IPrincipal User, ApplicationDbContext context)
        {
            var history = new TicketHistoryEntry()
            {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.AttachmentAdded,
                OldData = null,
                NewData = attach.Id.ToString(),
                OccuredAt = attach.UploadedAt
            };

            return history;
        }

        public ICollection<TicketComment> AddComments(ICollection<TicketComment> comments, IPrincipal User, ApplicationDbContext context, bool statusByWorkflow = true)
        {
            context.TicketComments.AddRange(comments);
            context.SaveChanges();

            var histories = new List<TicketHistoryEntry>();
            foreach (TicketComment c in comments)
            {
                try
                {
                    histories.Add(GetAddedCommentHistory(c, User, context));
                    var interactionStatusUpdate = ProcessInteraction(User, context, statusByWorkflow);
                    if (interactionStatusUpdate != null) { histories.Add(interactionStatusUpdate); }
                }
                catch { }
            }
            context.TicketHistoryEntries.AddRange(histories);
            context.SaveChanges();

            return comments;
        }

        public TicketComment AddComment(TicketComment comment, IPrincipal User, ApplicationDbContext context, bool save = true, bool statusByWorkflow = true)
        {
            context.TicketComments.Add(comment);
            context.TicketHistoryEntries.Add(GetAddedCommentHistory(comment, User, context));

            var interactionStatusUpdate = ProcessInteraction(User, context, statusByWorkflow);
            if (interactionStatusUpdate != null) { context.TicketHistoryEntries.Add(interactionStatusUpdate); }

            context.SaveChanges();
            return comment;
        }

        private TicketHistoryEntry GetAddedCommentHistory(TicketComment comment, IPrincipal User, ApplicationDbContext context)
        {
            var history = new TicketHistoryEntry()
            {
                UserId = User.Identity.GetUserId(),
                ParentTicketId = this.Id,
                EntryType = TicketHistoryEntry.TicketHistoryEntryType.CommentAdded,
                OldData = null,
                NewData = comment.Id.ToString(),
                OccuredAt = comment.CreatedAt
            };

            return history;
        }

        private TicketHistoryEntry ProcessInteraction(IPrincipal User, ApplicationDbContext context, bool statusByWorkflow = true)
        {
            if (statusByWorkflow)
            {
                var isStaff = new PermissionsHelper.PermissionsContainer(new PermissionsHelper(context), User, false).IsUserStaff;
                var tPerm = new PermissionsHelper.TicketPermissionsContainer(new PermissionsHelper(context), User, this.Id);
                var pPerm = new PermissionsHelper.ProjectPermissionsContainer(new PermissionsHelper(context), User, this.ParentProjectId);
                var workflow = this.ParentProject.ActiveWorkflow;

                if (isStaff && this.TicketStatus.ShouldWorkflowContinueStaff || !isStaff && this.TicketStatus.ShouldWorkflowContinueReporter)
                {
                    switch (new PermissionsHelper(context).GetHighestRole(User))
                    {
                        case "ServerAdmin":
                            if (workflow.ServerAdminInteractionStatusId != null)
                            {
                                return UpdateStatus(workflow.ServerAdminInteractionStatusId.Value, User, context, false, true);
                            }
                            break;

                        case "ProjectAdmin":
                            if (workflow.ProjMgrInteractionStatusId != null && pPerm.IsProjectManager)
                            {
                                return UpdateStatus(workflow.ProjMgrInteractionStatusId.Value, User, context, false, true);
                            }
                            break;

                        case "SuperSolver":
                            if (workflow.SuperSolverInteractionStatusId != null && tPerm.IsEligibleSolver)
                            {
                                return UpdateStatus(workflow.SuperSolverInteractionStatusId.Value, User, context, false, true);
                            }
                            break;

                        case "Solver":
                            if (workflow.SolverInteractionStatusId != null && tPerm.IsEligibleSolver)
                            {
                                return UpdateStatus(workflow.SolverInteractionStatusId.Value, User, context, false, true);
                            }
                            break;

                        case "Reporter":
                            if (workflow.ReporterInteractionStatusId != null && tPerm.IsTicketOwner)
                            {
                                return UpdateStatus(workflow.ReporterInteractionStatusId.Value, User, context, false, true);
                            }
                            break;

                        default:
                            throw new Exception();
                    }
                }
            }
            return null;
        }
    }
}