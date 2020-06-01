using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Semplicita.Models;

namespace Semplicita.Controllers
{
    public class TicketsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();




        public ActionResult Index() {
            var tickets = db.Tickets.Include(t => t.AssignedSolver)
                                    .Include(t => t.ParentProject)
                                    .Include(t => t.Reporter)
                                    .Include(t => t.TicketPriorityLevel)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType);


            return View(tickets.ToList());
        }


        [Route("tickets/{TicketIdentifier}")]
        public ActionResult Details(string TicketIdentifier) {
            if( string.IsNullOrWhiteSpace(TicketIdentifier) ) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.ToList().FirstOrDefault(t => t.GetTicketIdentifier() == TicketIdentifier);
            if( ticket == null ) {
                return HttpNotFound();
            }
            return View(ticket);
        }




        [Route("tickets/create-new")]
        public ActionResult Create() {
            Dictionary<string, int> availStatuses = null;
            Project parentProj = db.Projects.First(p => p.TicketTag == "DEF");
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

        [Route("tickets/create-new/{TicketTag}")]
        public ActionResult Create(string TicketTag) {
            if( string.IsNullOrWhiteSpace(TicketTag) ) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            } else {
                Dictionary<string, int> availStatuses = null;
                Project parentProj = db.Projects.First(p => p.TicketTag == TicketTag);
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

                string initSolverId = parentProject.GetNextSolverIdFromWorkflow(db);

                var newTicket = new Ticket() {
                    Title = model.Title
                    ,Description = model.Description
                    ,CreatedAt = now
                    ,ParentProjectId = parentProject.Id
                    ,TicketTypeId = model.TicketTypeId
                    ,TicketStatusId = initStatus.Id
                    ,TicketPriorityLevelId = priority.Id
                    ,ReporterId = User.Identity.GetUserId()
                    ,AssignedSolverId = initSolverId
                };
                db.Tickets.Add(newTicket);
                db.SaveChanges();

                newTicket.SaveTicketCreatedHistoryEntry(User, db);
                if( model.Attachments != null ) {
                    newTicket.AddAttachments(model.Attachments, User, db, Server);
                }

                return RedirectToAction("Details", "Tickets", new { TicketIdentifier = newTicket.GetTicketIdentifier() } );
            }
            return View();
        }




        [Route("tickets/{TicketIdentifier}/edit")]
        public ActionResult Edit(string TicketIdentifier) {
            if( string.IsNullOrWhiteSpace(TicketIdentifier) ) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.ToList().FirstOrDefault(t => t.GetTicketIdentifier() == TicketIdentifier);
            if( ticket == null ) {
                return HttpNotFound();
            }

            Project parentProj = db.Projects.First(p => p.TicketTag == "DEF");
            ProjectWorkflow parentProjWorkflow = parentProj.ActiveWorkflow;
            List<ApplicationUser> solvers = null;
            if (User.Identity.GetUserId() == parentProj.ProjectManagerId || User.IsInRole("ServerAdmin")) {
                solvers = parentProj.GetSolverMembers(db);
            }

            var viewModel = new EditTicketViewModel() {
                ParentProject = parentProj,
                Reporter = db.Users.Find(User.Identity.GetUserId()),
                PrioritySelections = db.TicketPriorityTypes.ToDictionary(tp => tp.Name, tp => tp.Id),
                AvailableTicketTypes = db.TicketTypes.ToList(),
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

                return RedirectToAction("Details", "Tickets", new { TicketIdentifier = oldTicket.GetTicketIdentifier() });
            }

            var ticket = db.Tickets.Find(model.CurrentTicketId);
            return RedirectToAction("Edit", "Tickets", new { TicketIdentifier = ticket.GetTicketIdentifier() });
        }


        [Route("tickets/{TicketIdentifier}/delete")]
        public ActionResult Delete(string TicketIdentifier) {
            if( string.IsNullOrWhiteSpace(TicketIdentifier) ) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.ToList().FirstOrDefault(t => t.GetTicketIdentifier() == TicketIdentifier);
            if( ticket == null ) {
                return HttpNotFound();
            }
            return View(ticket);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteTicket(int id) {
            Ticket ticket = db.Tickets.Find(id);
            db.Tickets.Remove(ticket);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) {
            if( disposing ) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
