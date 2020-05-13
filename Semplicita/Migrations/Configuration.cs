namespace Semplicita.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Semplicita.Models;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Drawing;
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
            if( !context.Roles.Any(r => r.Name == "SuperSolver") ) {
                roleManager.Create(new IdentityRole { Name = "SuperSolver" });
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
                context.TicketTypes.Add(new TicketType() { Name = "Bug", Description = "Normal functionality is breaking when the user is operating in particular conditions." });
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

            if( !context.TicketStatuses.Any(tst => tst.Name == "New-Unassigned") ) {
                var status = new TicketStatus() {
                    Name = "New-Unassigned",
                    Display = "New (Unassigned)",
                    Description = "The issue has just been created, and has not been assigned to a solver yet.",
                    IsStarted = false,
                    IsInProgress = false,
                    IsPausedPending = false,
                    IsResolved = false,
                    IsClosed = false,
                    IsCanceled = false
                };
                context.TicketStatuses.Add(status);
            }
            if( !context.TicketStatuses.Any(tst => tst.Name == "New-Assigned") ) {
                var status = new TicketStatus() {
                    Name = "New-Assigned",
                    Display = "New",
                    Description = "The issue has been created, and has been assigned to a solver pending first contact.",
                    IsStarted = false,
                    IsInProgress = false,
                    IsPausedPending = false,
                    IsResolved = false,
                    IsClosed = false,
                    IsCanceled = false
                };
                context.TicketStatuses.Add(status);
            }

            if( !context.TicketStatuses.Any(tst => tst.Name == "Investigation") ) {
                var status = new TicketStatus() {
                    Name = "Investigation",
                    Display = "Investigation",
                    Description = "The solver has started looking into potential solutions to the reporter's inquiry.",
                    IsStarted = true,
                    IsInProgress = false,
                    IsPausedPending = false,
                    IsResolved = false,
                    IsClosed = false,
                    IsCanceled = false
                };
                context.TicketStatuses.Add(status);
            }
            if( !context.TicketStatuses.Any(tst => tst.Name == "InProgress") ) {
                var status = new TicketStatus() {
                    Name = "InProgress",
                    Display = "Solution In Progress",
                    Description = "The solver is in the middle of implimenting a potential solution discovered.",
                    IsStarted = true,
                    IsInProgress = true,
                    IsPausedPending = false,
                    IsResolved = false,
                    IsClosed = false,
                    IsCanceled = false
                };
                context.TicketStatuses.Add(status);
            }
            if( !context.TicketStatuses.Any(tst => tst.Name == "NeedInfo") ) {
                var status = new TicketStatus() {
                    Name = "NeedInfo",
                    Display = "More Information Required",
                    Description = "This issue cannot be worked on with the information redialy available. More information is required from the reporter before work can continue.",
                    IsStarted = true,
                    IsInProgress = false,
                    IsPausedPending = true,
                    IsResolved = false,
                    IsClosed = false,
                    IsCanceled = false
                };
                context.TicketStatuses.Add(status);
            }
            if( !context.TicketStatuses.Any(tst => tst.Name == "Reviewing") ) {
                var status = new TicketStatus() {
                    Name = "Reviewing",
                    Display = "Under Review",
                    Description = "The solver needs time to review findings before working on implementing a solution.",
                    IsStarted = true,
                    IsInProgress = false,
                    IsPausedPending = true,
                    IsResolved = false,
                    IsClosed = false,
                    IsCanceled = false
                };
                context.TicketStatuses.Add(status);
            }
            if( !context.TicketStatuses.Any(tst => tst.Name == "Pending") ) {
                var status = new TicketStatus() {
                    Name = "Pending",
                    Display = "Pending/Blocked",
                    Description = "This issue cannot be worked on at the moment due to other pending issues, or because a potential solution has not yet been found.",
                    IsStarted = true,
                    IsInProgress = false,
                    IsPausedPending = true,
                    IsResolved = false,
                    IsClosed = false,
                    IsCanceled = false
                };
                context.TicketStatuses.Add(status);
            }

            if( !context.TicketStatuses.Any(tst => tst.Name == "Solved") ) {
                var status = new TicketStatus() {
                    Name = "Solved",
                    Display = "Solved!",
                    Description = "The reporter's issue has been resolved, and resulted in a resolution that alligns with the intended outcome of their inquiry.",
                    IsStarted = true,
                    IsInProgress = false,
                    IsPausedPending = false,
                    IsResolved = true,
                    IsClosed = false,
                    IsCanceled = false
                };
                context.TicketStatuses.Add(status);
            }

            if( !context.TicketStatuses.Any(tst => tst.Name == "CantSolve") ) {
                var status = new TicketStatus() {
                    Name = "CantSolve",
                    Display = "Can't Solve",
                    Description = "The reporter's issue was not able to be solved due to a block or obstruction that would result from available potential solutions.",
                    IsStarted = true,
                    IsInProgress = false,
                    IsPausedPending = false,
                    IsResolved = true,
                    IsClosed = false,
                    IsCanceled = true
                };
                context.TicketStatuses.Add(status);
            }
            if( !context.TicketStatuses.Any(tst => tst.Name == "WontSolve") ) {
                var status = new TicketStatus() {
                    Name = "WontSolve",
                    Display = "Refused/Won't Solve",
                    Description = "The reporter's issue will not be solved, because the reporter is inquiring about intended functionality or a procedure that is actually working as intended.",
                    IsStarted = true,
                    IsInProgress = false,
                    IsPausedPending = false,
                    IsResolved = true,
                    IsClosed = false,
                    IsCanceled = true
                };
                context.TicketStatuses.Add(status);
            }

            if( !context.TicketStatuses.Any(tst => tst.Name == "Closed") ) {
                var status = new TicketStatus() {
                    Name = "Closed",
                    Display = "Closed",
                    Description = "The reporter's issue has been resolved and has been confirmed as satisfactory with the reporter.",
                    IsStarted = true,
                    IsInProgress = false,
                    IsPausedPending = false,
                    IsResolved = true,
                    IsClosed = true,
                    IsCanceled = false
                };
                context.TicketStatuses.Add(status);
            }
            if( !context.TicketStatuses.Any(tst => tst.Name == "Disposed") ) {
                var status = new TicketStatus() {
                    Name = "Disposed",
                    Display = "Disposed",
                    Description = "The reporter's issue was cancled, and has been archived.",
                    IsStarted = true,
                    IsInProgress = false,
                    IsPausedPending = false,
                    IsResolved = false,
                    IsClosed = true,
                    IsCanceled = true
                };
                context.TicketStatuses.Add(status);
            }


            #endregion

        }
    }
}
