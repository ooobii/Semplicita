using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Semplicita.Helpers
{

    public class RoleDisplayDictionary : Dictionary<string, string>
    {
        public RoleDisplayDictionary() {
            Add("ServerAdmin", "Server Administrator");
            Add("ProjectAdmin", "Project Administrator");
            Add("SuperSolver", "Super Solver");
            Add("Solver", "Solver");
            Add("Reporter", "Reporter");
        }
    }
}