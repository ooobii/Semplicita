using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Semplicita.Models
{
    public class ProjectUsersViewModel
    {
        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Project> Projects { get; set; }
    }

    public class AddUserToProjectViewModel
    {
        public ICollection<string> UserIds { get; set; }
        public ICollection<int> ProjectIds { get; set; }
    }
    public class RemoveUserFromProjectViewModel
    {
        public ICollection<string> UserIds { get; set; }
        public ICollection<int> ProjectIds { get; set; }

    }

}