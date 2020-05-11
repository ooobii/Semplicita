namespace Semplicita.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OrdinalWorkflowPosition { get; set; }

        //the issue has been started to be looked into by the assigned solver.
        public bool IsStarted { get; set; }

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
    }
}