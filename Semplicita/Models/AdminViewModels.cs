using Semplicita.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Semplicita.Models
{
    public class UsersAllocViewModel
    {
        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Project> Projects { get; set; }
    }

    public class AddRemoveUsersProjectsViewModel
    {
        public ICollection<string> UserIds { get; set; }
        public ICollection<int> ProjectIds { get; set; }
    }
    public class AddRemoveUsersRolesViewModel
    {
        public ICollection<string> UserIds { get; set; }
        public ICollection<string> Roles { get; set; }
    } 


    public class ProjectConfigViewModel
    {
        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Project> Projects { get; set; }

        public ICollection<ProjectWorkflow> Workflows { get; set; }
    }
}
