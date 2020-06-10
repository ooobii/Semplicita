using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Semplicita.Helpers;
using Semplicita.Models;

namespace Semplicita.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();



        public ActionResult Index() {
            return View(new RolesHelper.PermissionsContainer(new RolesHelper(db), User));
        }


        [Route("tickets/{ticketIdentifier}")]
        public ActionResult Show(string ticketIdentifier) {
            if( string.IsNullOrWhiteSpace(ticketIdentifier) ) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.ToList().FirstOrDefault(t => t.GetTicketIdentifier() == ticketIdentifier);
            if( ticket == null ) {
                return HttpNotFound();
            }

            ProjectWorkflow parentProjWorkflow = ticket.ParentProject.ActiveWorkflow;
            List<ApplicationUser> solvers = null;
            if( User.Identity.GetUserId() == ticket.ParentProject.ProjectManagerId || User.IsInRole("ServerAdmin") ) {
                solvers = ticket.ParentProject.GetSolverMembers();
            }

            var viewModel = new EditTicketViewModel() {
                ParentProject = ticket.ParentProject,
                Reporter = db.Users.Find(User.Identity.GetUserId()),
                PrioritySelections = db.TicketPriorityTypes.ToDictionary(tp => tp.Name, tp => tp.Id),
                AvailableTicketTypes = db.TicketTypes.ToList(),
                CurrentTicket = ticket,
                AvailableSolvers = solvers
            };


            return View(viewModel);
        }

        [Route("tickets/{ticketIdentifier}/attachments/{attachmentId}")]
        public ActionResult GetTicketAttachment(string ticketIdentifier, int attachmentId) {
            var ticket = db.Tickets.ToList().FirstOrDefault(t => t.GetTicketIdentifier() == ticketIdentifier);
            if( ticket == null ) {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            var attachment = ticket.Attachments.FirstOrDefault(a => a.Id == attachmentId);
            if( attachment == null ) {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }

            return File(attachment.MediaUrl, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(attachment.MediaUrl));
        }


        [Route("tickets/create-new/{ticketTag}")]
        public ActionResult Create(string ticketTag) {
            if( string.IsNullOrWhiteSpace(ticketTag) ) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            } else {
                Dictionary<string, int> availStatuses = null;
                Project parentProj = db.Projects.First(p => p.TicketTag == ticketTag);
                ProjectWorkflow parentProjWorkflow = parentProj.ActiveWorkflow;

                if( parentProjWorkflow.InitialTicketStatusId == null ) {
                    availStatuses = new Dictionary<string, int>();
                    foreach( TicketStatus ts in db.TicketStatuses.Where(ts => ts.MustBeAssigned == false && ts.IsStarted == false).ToList() ) {
                        availStatuses.Add(ts.Display, ts.Id);
                    }
                }

                var viewModel = new CreateTicketViewModel() {
                    ParentProject = parentProj,
                    Reporter = db.Users.Find(User.Identity.GetUserId()),
                    PrioritySelections = db.TicketPriorityTypes.ToDictionary(tp => tp.Name, tp => tp.Id),
                    AvailableStatuses = availStatuses,
                    AvailableTicketTypes = db.TicketTypes.ToList()
                };
                return View(viewModel);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("tickets/create")]
        public ActionResult CreateTicket(NewTicketModel model) {
            if( ModelState.IsValid ) {
                var now = DateTime.Now;
                var userId = User.Identity.GetUserId();

                Project parentProject = db.Projects.FirstOrDefault(p => p.Id == model.ParentProjectId);
                if( parentProject == null ) {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                TicketStatus initStatus;
                if( model.TicketStatusId == null && parentProject.ActiveWorkflow.InitialTicketStatusId != null ) {
                    initStatus = db.TicketStatuses.ToList().FirstOrDefault(ts => ts.Id == parentProject.ActiveWorkflow.InitialTicketStatusId);
                    if( initStatus == null ) { return new HttpStatusCodeResult(HttpStatusCode.InternalServerError); }

                } else {
                    initStatus = db.TicketStatuses.Find(model.TicketStatusId);

                }

                TicketPriority priority = db.TicketPriorityTypes.FirstOrDefault(tp => tp.Id == model.TicketPriorityId);
                if( priority == null ) {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                string initSolverId = parentProject.GetNextSolverIdFromWorkflow();

                var newTicket = new Ticket() {
                    Title = model.Title
                    , Description = model.Description
                    , CreatedAt = now
                    , ParentProjectId = parentProject.Id
                    , TicketTypeId = model.TicketTypeId
                    , TicketStatusId = initStatus.Id
                    , TicketPriorityLevelId = priority.Id
                    , ReporterId = User.Identity.GetUserId()
                    , AssignedSolverId = initSolverId
                };
                db.Tickets.Add(newTicket);
                db.SaveChanges();

                newTicket.SaveTicketCreatedHistoryEntry(User, db);
                if( model.Attachments.First() != null ) {
                    newTicket.AddAttachments(model.Attachments, User, db, Server);
                }

                return RedirectToAction("Show", "Tickets", new { TicketIdentifier = newTicket.GetTicketIdentifier() });
            }
            return View();
        }


        [Route("tickets/{ticketIdentifier}/edit")]
        public ActionResult Edit(string ticketIdentifier) {
            if( string.IsNullOrWhiteSpace(ticketIdentifier) ) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.ToList().FirstOrDefault(t => t.GetTicketIdentifier() == ticketIdentifier);
            if( ticket == null ) {

                return HttpNotFound();
            }

            var uPerm = new RolesHelper.PermissionsContainer(new RolesHelper(db), User, false);
            var tPerm = new RolesHelper.TicketPermissionsContainer(new RolesHelper(db), User, ticket.Id);

            ProjectWorkflow parentProjWorkflow = ticket.ParentProject.ActiveWorkflow;
            List<ApplicationUser> solvers = null;
            if( User.Identity.GetUserId() == ticket.ParentProject.ProjectManagerId || User.IsInRole("ServerAdmin") ) {
                solvers = ticket.ParentProject.GetSolverMembers();
            }

            List<TicketStatus> availStatuses = null;
            if ((uPerm.IsUserStaff && parentProjWorkflow.CanStaffSetStatusOnInteract) ||
                (tPerm.IsTicketOwner && parentProjWorkflow.CanTicketOwnerSetStatusOnInteract ) ) {
                availStatuses = parentProjWorkflow.GetAvailableStatuses(ticket.Id, User, db);
            }

            var viewModel = new EditTicketViewModel() {
                ParentProject = ticket.ParentProject,
                Reporter = db.Users.Find(User.Identity.GetUserId()),
                PrioritySelections = db.TicketPriorityTypes.ToDictionary(tp => tp.Name, tp => tp.Id),
                AvailableTicketTypes = db.TicketTypes.ToList(),
                AvailableStatuses = availStatuses.ToDictionary(s => s.Name, s => s.Id),
                CurrentTicket = ticket,
                AvailableSolvers = solvers
            };


            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Tickets/EditTicket")]
        public ActionResult EditTicket(EditTicketModel model) {
            if( ModelState.IsValid ) {
                var now = DateTime.Now;
                var userId = User.Identity.GetUserId();

                var parentProject = db.Projects.FirstOrDefault(p => p.Id == model.ParentProjectId);
                if( parentProject == null ) {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                var nextStatus = db.TicketStatuses.Find(model.TicketStatusId);

                var priority = db.TicketPriorityTypes.FirstOrDefault(tp => tp.Id == model.TicketPriorityId);
                if( priority == null ) {
                    return new HttpStatusCodeResult(HttpStatusCode.NotFound);
                }

                var oldTicket = db.Tickets.Find(model.CurrentTicketId);
                var newTicket = new Ticket() {
                    Title = model.Title
                    , Description = model.Description
                    , CreatedAt = oldTicket.CreatedAt
                    , LastInteractionAt = now
                    , ParentProjectId = parentProject.Id
                    , TicketTypeId = model.TicketTypeId
                    , TicketType = db.TicketTypes.Find(model.TicketTypeId)
                    , TicketStatusId = nextStatus.Id
                    , TicketPriorityLevelId = priority.Id
                    , ReporterId = User.Identity.GetUserId()
                    , AssignedSolverId = model.SolverId
                };

                var histories = oldTicket.UpdateTicket(newTicket, User, db);
                db.TicketHistoryEntries.AddRange(histories);
                db.SaveChanges();

                if( model.Attachments.First() != null ) {
                    oldTicket.AddAttachments(model.Attachments, User, db, Server);
                }

                return RedirectToAction("Show", "Tickets", new { TicketIdentifier = oldTicket.GetTicketIdentifier() });
            }

            var ticket = db.Tickets.Find(model.CurrentTicketId);
            return RedirectToAction("Edit", "Tickets", new { TicketIdentifier = ticket.GetTicketIdentifier() });
        }


        [Route("tickets/{ticketIdentifier}/archive")]
        public ActionResult Archive(string ticketIdentifier) {
            if( string.IsNullOrWhiteSpace(ticketIdentifier) ) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.ToList().FirstOrDefault(t => t.GetTicketIdentifier() == ticketIdentifier);
            if( ticket == null ) {
                return HttpNotFound();
            }
            return View(ticket);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Tickets/ArchiveTicket")]
        public ActionResult ArchiveTicket(int id) {
            Ticket ticket = db.Tickets.First(t => t.Id == id);
            
            var historyEntry = ticket.ArchiveTicket(User, db);
            db.TicketHistoryEntries.Add(historyEntry);
            db.SaveChanges();

            return RedirectToAction("Index");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("tickets/{TicketIdentifier}/add-comment")]
        public ActionResult AddComment(AddTicketCommentModel model) {
            if( ModelState.IsValid ) {
                var oldTicket = db.Tickets.Find(model.TicketId);

                var newComment = new TicketComment() {
                    AuthorId = User.Identity.GetUserId(),
                    CreatedAt = DateTime.Now,
                    ParentTicketId = model.TicketId,
                    Body = model.Body
                };

                oldTicket.AddComment(newComment, User, db, true, true);

                return RedirectToAction("Show", "Tickets", new { TicketIdentifier = oldTicket.GetTicketIdentifier() });
            }

            var ticket = db.Tickets.Find(model.TicketId);
            return RedirectToAction("Show", "Tickets", new { TicketIdentifier = ticket.GetTicketIdentifier() });
        }



        protected override void Dispose(bool disposing) {
            if( disposing ) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
