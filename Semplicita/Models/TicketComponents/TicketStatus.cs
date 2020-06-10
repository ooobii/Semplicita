using Antlr.Runtime.Tree;
using System.Drawing;
using System.EnterpriseServices.Internal;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;

namespace Semplicita.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Display { get; set; }
        public string DisplayForeColor { get; set; }
        public string DisplayBackColor { get; set; }
        public string Description { get; set; }

        //the issue has been started to be looked into by the assigned solver.
        public bool IsStarted { get; set; }

        //the issue has been assigned to a solver (on ticket status change, checks if solver is present before allowing status update)
        public bool MustBeAssigned { get; set; }

        //the issue is currently being worked on by the assigned solver.
        public bool IsInProgress { get; set; }

        //the issue is currently pending resolution/attention by the solver.
        public bool IsPausedPending { get; set; }

        //the issue has been solved by the solver, and currently requires no additional action
        public bool IsResolved { get; set; }

        //the issue has been closed and is marked for archive; statuses cannot change and comments cannot be added until not closed
        public bool IsClosed { get; set; }

        //the issue was rejected by the assigned solver, and will not be looked into.
        public bool IsCanceled { get; set; }

        //the issue is archived and should not be displayed in general views
        public bool IsArchived { get; set; }



        //this status can only be set by the reporter of the ticket.
        public bool IsForReporter { get; set; }

        //this status can only be set by a staff member
        public bool IsForStaff { get; set; }


        //this status should not update the issue's status if a staff member interacts with the ticket
        public bool ShouldWorkflowContinueStaff { get; set; }

        //this status should not update the issue's status if the reporter interacts with the ticket
        public bool ShouldWorkflowContinueReporter { get; set; }




        public HtmlString GetStatusBadgeHtml() {
            var style = $"style=\"color: {this.DisplayForeColor}; background-color: {this.DisplayBackColor}; font-size: 12px; font-weight:500;\"";

            return new HtmlString($"<span class=\"badge\" {style}>{this.Display}</span>");
        }
        public HtmlString GetStatusBadgeHtml(int fontSize_px) {
            var style = $"style=\"color: {this.DisplayForeColor}; background-color: {this.DisplayBackColor}; font-size: {fontSize_px}px; font-weight:bold;\"";

            return new HtmlString($"<span class=\"badge\" {style}>{this.Display}</span>");
        }


    }
}