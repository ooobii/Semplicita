using Semplicita.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Semplicita.Controllers
{
    public class AdminController : Controller
    {
        private UserRolesHelper rolesHelper = new UserRolesHelper();
        private ProjectHelper projectHelper = new ProjectHelper();
        
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }


    }
}