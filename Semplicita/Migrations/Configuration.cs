namespace Semplicita.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Semplicita.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Semplicita.Models.ApplicationDbContext>
    {
        public Configuration() {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Semplicita.Models.ApplicationDbContext context) {
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            #region Roles Generation
            if( !context.Roles.Any(r => r.Name == "ServerAdmin") ) {
                roleManager.Create(new IdentityRole { Name = "ServerAdmin" });
            }
            if( !context.Roles.Any(r => r.Name == "ProjectAdmin") ) {
                roleManager.Create(new IdentityRole { Name = "ProjectAdmin" });
            }
            if( !context.Roles.Any(r => r.Name == "Solver") ) {
                roleManager.Create(new IdentityRole { Name = "Solver" });
            }
            if( !context.Roles.Any(r => r.Name == "Reporter") ) {
                roleManager.Create(new IdentityRole { Name = "Reporter" });
            }
            #endregion

            #region User Creation

            //stores
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new UserManager<ApplicationUser>(userStore);

            //Matthew
            if( !context.Users.Any(u => u.Email == "matt_wendel@hotmail.com") ) {
                var user = new ApplicationUser {
                    UserName = "matt_wendel@hotmail.com",
                    Email = "matt_wendel@hotmail.com",
                    FirstName = "Matthew",
                    LastName = "Wendel"
                };
                userManager.Create(user, "Abcd3FG#");
                userManager.AddToRole(user.Id, "ServerAdmin");
            }

            //Jason
            if( !context.Users.Any(u => u.Email == "JasonTwichell@coderfoundry.com") ) {
                var user = new ApplicationUser {
                    UserName = "JasonTwichell@coderfoundry.com",
                    Email = "JasonTwichell@coderfoundry.com",
                    FirstName = "Jason",
                    LastName = "Twichell"
                };
                userManager.Create(user, "Abc&123!");
                userManager.AddToRole(user.Id, "ProjectAdmin");
            }

            //Drew
            if( !context.Users.Any(u => u.Email == "arussell@coderfoundry.com") ) {
                var user = new ApplicationUser {
                    UserName = "arussell@coderfoundry.com",
                    Email = "arussell@coderfoundry.com",
                    FirstName = "Andrew",
                    LastName = "Russell"
                };
                userManager.Create(user, "Abc&123!");
                userManager.AddToRole(user.Id, "SuperSolver");
            }

            #endregion User Creation

            #region TicketType Creation

            if( !context.TicketTypes.Any(tt => tt.Name == "Missing/Broken Dependancy") ) {
                context.TicketTypes.Add(new TicketType() { Name = "Missing/Broken Dependancy", Description = "The issue is suspected of originating from a missing, corrupt, or outdated package/module." });
            }
            if( !context.TicketTypes.Any(tt => tt.Name == "Bug") ) {
                context.TicketTypes.Add(new TicketType() { Name = "Bug)", Description = "Normal functionality is breaking when the user is operating in particular conditions." });
            }
            if( !context.TicketTypes.Any(tt => tt.Name == "Not as Intended") ) {
                context.TicketTypes.Add(new TicketType() { Name = "Not as Intended", Description = "A feature is not working within proper specification, or is not generating proper results." });
            }
            if( !context.TicketTypes.Any(tt => tt.Name == "Aesthetic Correction") ) {
                context.TicketTypes.Add(new TicketType() { Name = "Aesthetic Correction", Description = "A request for spelling correction, grammar validation, paragraph structure, or layout changes. This issue should not be impactful to functionality." });
            }
            if( !context.TicketTypes.Any(tt => tt.Name == "Feature Request") ) {
                context.TicketTypes.Add(new TicketType() { Name = "Feature Request", Description = "Features that the reporter desires are missing, and this request is for consideration to add the requested feature." });
            }

            #endregion

            #region TicketStatus Creation

            #endregion
        }
    }
}
