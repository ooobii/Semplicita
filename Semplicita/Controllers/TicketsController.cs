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
                if( model.InitialAttachments != null ) {
                    newTicket.AddAttachments(model.InitialAttachments, User, db, Server);
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
            ViewBag.AssignedSolverId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedSolverId);
            ViewBag.ParentProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ParentProjectId);
            ViewBag.ReporterId = new SelectList(db.Users, "Id", "FirstName", ticket.ReporterId);
            ViewBag.TicketPriorityLevelId = new SelectList(db.TicketPriorityTypes, "Id", "Name", ticket.TicketPriorityLevelId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditTicket([Bind(Include = "Id,Title,Description,CreatedAt,LastInteractionAt,ResolvedAt,ParentProjectId,TicketTypeId,TicketStatusId,TicketPriorityLevelId,ReporterId,AssignedSolverId")] Ticket ticket) {
            if( ModelState.IsValid ) {
                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AssignedSolverId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignedSolverId);
            ViewBag.ParentProjectId = new SelectList(db.Projects, "Id", "Name", ticket.ParentProjectId);
            ViewBag.ReporterId = new SelectList(db.Users, "Id", "FirstName", ticket.ReporterId);
            ViewBag.TicketPriorityLevelId = new SelectList(db.TicketPriorityTypes, "Id", "Name", ticket.TicketPriorityLevelId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
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


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) {
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
