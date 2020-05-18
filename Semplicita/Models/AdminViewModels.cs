using Semplicita.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Semplicita.Models
{
    public class ServerConfigViewModel
    {
        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Project> Projects { get; set; }
        public ICollection<ProjectWorkflow> Workflows { get; set; }
    }

    public class AddRemoveUsersProjectsModel
    {
        public ICollection<string> UserIds { get; set; }
        public ICollection<int> ProjectIds { get; set; }
        public string ReturningViewName { get; set; }
    }
    public class AddRemoveUsersRolesModel
    {
        public ICollection<string> UserIds { get; set; }
        public ICollection<string> Roles { get; set; }
    } 


    public class SetProjectWorkflowModel
    {
        public ICollection<int> ProjectIds { get; set; }
        public int? WorkflowId { get; set; }
    }
}
