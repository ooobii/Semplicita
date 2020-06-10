using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Semplicita.Helpers;

namespace Semplicita.Models
{
    public class ProjectIndexViewModel
    {
        public ICollection<Project> AvailableProjects { get; set; }
        public ICollection<ApplicationUser> Users { get; set; }

        public ICollection<Ticket> AvailableTickets { get; set; }
    }

    public class ShowProjectViewModel {
        public Project SelectedProject { get; set; }
        public RolesHelper.PermissionsContainer uPerm { get; set; }
        public RolesHelper.ProjectPermissionsContainer pPerm { get; set; }

        public ICollection<ApplicationUser> Solvers { get; set; }
        public ICollection<ApplicationUser> Reporters { get; set; }

        public Project.TicketsContainer Tickets { get; set; }
    }

    public class CreateProjectViewModel
    {
        public ICollection<ApplicationUser> ProjectAdministrators { get; set; }
        public ICollection<ApplicationUser> AvailableMembers { get; set; }
        public ICollection<ProjectWorkflow> Workflows { get; set; }
    }
    public class NewProjectModel
    {
        [Required]
        public string Name { get; set; }
        

        [Required]
        public string Description { get; set; }


        [Required]
        [StringLength(3, MinimumLength = 1)]
        public string TicketTag { get; set; }


        [Required]
        public int ActiveWorkflowId { get; set; }


        [Required]
        public string ProjectManagerId { get; set; }


        [Required]
        public bool IsActiveProject { get; set; }
        

        [Required]
        public ICollection<string> MemberIds { get; set; }
       
    }

    public class EditProjectViewModel : CreateProjectViewModel
    {
        public Project SelectedProject { get; set; }
    }
    public class EditProjectModel : NewProjectModel
    {
        public int ProjectId { get; set; }
    }
}