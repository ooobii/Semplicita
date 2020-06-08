namespace Semplicita.Migrations
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Semplicita.Models;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.Drawing;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Semplicita.Models.ApplicationDbContext>
    {
        public Configuration() {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Semplicita.Models.ApplicationDbContext context) {
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            try {

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

                #endregion Roles Generation

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
                        LastName = "Wendel",
                        AvatarImagePath = "/img/avatars/matthew.png"
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
                        LastName = "Twichell",
                        AvatarImagePath = "/img/avatars/jason.png"
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
                        LastName = "Russell",
                        AvatarImagePath = "/img/avatars/andrew.png"
                    };
                    userManager.Create(user, "Abc&123!");
                    userManager.AddToRole(user.Id, "SuperSolver");
                }


                //Shelby (Reporter)
                if( !context.Users.Any(u => u.Email == "shelby_99@rocketmail.com") ) {
                    var user = new ApplicationUser {
                        UserName = "shelby_99@rocketmail.com",
                        Email = "shelby_99@rocketmail.com",
                        FirstName = "Shelby",
                        LastName = "Holtzclaw",
                        AvatarImagePath = "/img/avatars/shelby.png"
                    };
                    userManager.Create(user, "ShelbyH!");
                    userManager.AddToRole(user.Id, "Reporter");
                }


                //Demo Users
                #region demousers
                //if( !context.Users.Any(u => u.Email == "demo_admin@matthewwendel.info") ) {
                //    var user = new ApplicationUser {
                //        UserName = "demo_admin@matthewwendel.info",
                //        Email = "demo_admin@matthewwendel.info",
                //        FirstName = "Demo:",
                //        LastName = "Server Admin",
                //        EmailConfirmed = true,
                //        IsDemoUser = true
                //    };
                //    userManager.Create(user, Util.GetSetting("demo:Password"));
                //    userManager.AddToRole(user.Id, "ServerAdmin");
                //}
                //if( !context.Users.Any(u => u.Email == "demo_projadmin@matthewwendel.info") ) {
                //    var user = new ApplicationUser {
                //        UserName = "demo_projadmin@matthewwendel.info",
                //        Email = "demo_projadmin@matthewwendel.info",
                //        FirstName = "Demo:",
                //        LastName = "Project Admin",
                //        EmailConfirmed = true,
                //        IsDemoUser = true
                //    };
                //    userManager.Create(user, Util.GetSetting("demo:Password"));
                //    userManager.AddToRole(user.Id, "ProjectAdmin");
                //}
                //if( !context.Users.Any(u => u.Email == "demo_ssolver@matthewwendel.info") ) {
                //    var user = new ApplicationUser {
                //        UserName = "demo_ssolver@matthewwendel.info",
                //        Email = "demo_ssolver@matthewwendel.info",
                //        FirstName = "Demo:",
                //        LastName = "Super Solver",
                //        EmailConfirmed = true,
                //        IsDemoUser = true
                //    };
                //    userManager.Create(user, Util.GetSetting("demo:Password"));
                //    userManager.AddToRole(user.Id, "SuperSolver");
                //}
                //if( !context.Users.Any(u => u.Email == "demo_solver@matthewwendel.info") ) {
                //    var user = new ApplicationUser {
                //        UserName = "demo_solver@matthewwendel.info",
                //        Email = "demo_solver@matthewwendel.info",
                //        FirstName = "Demo:",
                //        LastName = "Solver",
                //        EmailConfirmed = true,
                //        IsDemoUser = true
                //    };
                //    userManager.Create(user, Util.GetSetting("demo:Password"));
                //    userManager.AddToRole(user.Id, "Solver");
                //}
                //if( !context.Users.Any(u => u.Email == "demo@matthewwendel.info") ) {
                //    var user = new ApplicationUser {
                //        UserName = "demo@matthewwendel.info",
                //        Email = "demo@matthewwendel.info",
                //        FirstName = "Demo:",
                //        LastName = "Reporter",
                //        EmailConfirmed = true,
                //        IsDemoUser = true
                //    };
                //    userManager.Create(user, Util.GetSetting("demo:Password"));
                //    userManager.AddToRole(user.Id, "Reporter");
                //}
                #endregion

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

                context.SaveChanges();

                #endregion TicketType Creation

                #region TicketPriority Creation

                if( !context.TicketPriorityTypes.Any(tpt => tpt.Name == "Low") ) {
                    var priority = new TicketPriority() {
                        Name = "Low",
                        Rank = 100
                    };

                    context.TicketPriorityTypes.Add(priority);
                }
                if( !context.TicketPriorityTypes.Any(tpt => tpt.Name == "Medium") ) {
                    var priority = new TicketPriority() {
                        Name = "Medium",
                        Rank = 50
                    };

                    context.TicketPriorityTypes.Add(priority);
                }
                if( !context.TicketPriorityTypes.Any(tpt => tpt.Name == "High") ) {
                    var priority = new TicketPriority() {
                        Name = "High",
                        Rank = 1
                    };

                    context.TicketPriorityTypes.Add(priority);
                }

                context.SaveChanges();

                #endregion TicketPriority Creation

                #region TicketStatus Creation

                if( !context.TicketStatuses.Any(tst => tst.Name == "New-Unassigned") ) {
                    var status = new TicketStatus() {
                        Name = "New-Unassigned",
                        Display = "New (Unassigned)",
                        DisplayForeColor = "#000000",
                        DisplayBackColor = "#EC860F",
                        Description = "The issue has just been created, and has not been assigned to a solver yet.",
                        IsStarted = false,
                        MustBeAssigned = false,
                        IsInProgress = false,
                        IsPausedPending = false,
                        IsResolved = false,
                        IsClosed = false,
                        IsCanceled = false,
                        IsForReporter = true,
                        IsForStaff = true
                    };
                    context.TicketStatuses.Add(status);
                }
                if( !context.TicketStatuses.Any(tst => tst.Name == "New-Assigned") ) {
                    var status = new TicketStatus() {
                        Name = "New-Assigned",
                        Display = "New",
                        DisplayForeColor = "#000000",
                        DisplayBackColor = "#DDE31E",
                        Description = "The issue has been created, and has been assigned to a solver pending first contact.",
                        IsStarted = false,
                        MustBeAssigned = true,
                        IsInProgress = false,
                        IsPausedPending = false,
                        IsResolved = false,
                        IsClosed = false,
                        IsCanceled = false,
                        IsForReporter = false,
                        IsForStaff = false
                    };
                    context.TicketStatuses.Add(status);
                }

                if( !context.TicketStatuses.Any(tst => tst.Name == "Investigation") ) {
                    var status = new TicketStatus() {
                        Name = "Investigation",
                        Display = "Investigation",
                        DisplayForeColor = "#FFF",
                        DisplayBackColor = "#509D8D",
                        Description = "The solver has started looking into potential solutions for the reporter's inquiry.",
                        IsStarted = true,
                        MustBeAssigned = true,
                        IsInProgress = true,
                        IsPausedPending = false,
                        IsResolved = false,
                        IsClosed = false,
                        IsCanceled = false,
                        IsForReporter = false,
                        IsForStaff = true
                    };
                    context.TicketStatuses.Add(status);
                }
                if( !context.TicketStatuses.Any(tst => tst.Name == "Reviewing") ) {
                    var status = new TicketStatus() {
                        Name = "Reviewing",
                        Display = "Under Review",
                        DisplayForeColor = "#FFF",
                        DisplayBackColor = "#509D8D",
                        Description = "The solver needs time to review findings before working on implementing a solution.",
                        IsStarted = true,
                        MustBeAssigned = true,
                        IsInProgress = false,
                        IsPausedPending = true,
                        IsResolved = false,
                        IsClosed = false,
                        IsCanceled = false,
                        IsForReporter = false,
                        IsForStaff = true
                    };
                    context.TicketStatuses.Add(status);
                }
                if( !context.TicketStatuses.Any(tst => tst.Name == "InProgress") ) {
                    var status = new TicketStatus() {
                        Name = "InProgress",
                        Display = "Solution In Progress",
                        DisplayForeColor = "#000",
                        DisplayBackColor = "#32E89F",
                        Description = "The solver is in the middle of implimenting a potential solution discovered.",
                        IsStarted = true,
                        MustBeAssigned = true,
                        IsInProgress = true,
                        IsPausedPending = false,
                        IsResolved = false,
                        IsClosed = false,
                        IsCanceled = false,
                        IsForReporter = false,
                        IsForStaff = true
                    };
                    context.TicketStatuses.Add(status);
                }
                if( !context.TicketStatuses.Any(tst => tst.Name == "ReporterWaiting") ) {
                    var status = new TicketStatus() {
                        Name = "ReporterWaiting",
                        Display = "Reporter Waiting",
                        DisplayForeColor = "#FFF",
                        DisplayBackColor = "#96389C",
                        Description = "The reporter has added additional information, and is waiting for confirmation from the solver.",
                        IsStarted = true,
                        MustBeAssigned = true,
                        IsInProgress = false,
                        IsPausedPending = false,
                        IsResolved = false,
                        IsClosed = false,
                        IsCanceled = false,
                        IsForReporter = true,
                        IsForStaff = false
                    };
                    context.TicketStatuses.Add(status);
                }

                if( !context.TicketStatuses.Any(tst => tst.Name == "Solved") ) {
                    var status = new TicketStatus() {
                        Name = "Solved",
                        Display = "Solved!",
                        DisplayForeColor = "#FFF",
                        DisplayBackColor = "#428A3B",
                        Description = "The reporter's issue has been resolved, and resulted in a resolution that alligns with the intended outcome of their inquiry.",
                        IsStarted = true,
                        MustBeAssigned = true,
                        IsInProgress = false,
                        IsPausedPending = false,
                        IsResolved = true,
                        IsClosed = false,
                        IsCanceled = false,
                        IsForReporter = false,
                        IsForStaff = true
                    };
                    context.TicketStatuses.Add(status);
                }


                if( !context.TicketStatuses.Any(tst => tst.Name == "NeedInfo") ) {
                    var status = new TicketStatus() {
                        Name = "NeedInfo",
                        Display = "More Information Required",
                        DisplayForeColor = "#000",
                        DisplayBackColor = "#C8687B",
                        Description = "This issue cannot be worked on with the information redialy available. More information is required from the reporter before work can continue.",
                        IsStarted = true,
                        MustBeAssigned = true,
                        IsInProgress = false,
                        IsPausedPending = true,
                        IsResolved = false,
                        IsClosed = false,
                        IsCanceled = false,
                        IsForReporter = false,
                        IsForStaff = true
                    };
                    context.TicketStatuses.Add(status);
                }
                if( !context.TicketStatuses.Any(tst => tst.Name == "Pending") ) {
                    var status = new TicketStatus() {
                        Name = "Pending",
                        Display = "Pending/Blocked",
                        DisplayForeColor = "#000",
                        DisplayBackColor = "#4C60BB",
                        Description = "This issue cannot be worked on at the moment due to other pending issues, or because a potential solution has not yet been found.",
                        IsStarted = true,
                        MustBeAssigned = true,
                        IsInProgress = false,
                        IsPausedPending = true,
                        IsResolved = false,
                        IsClosed = false,
                        IsCanceled = false,
                        IsForReporter = false,
                        IsForStaff = true
                    };
                    context.TicketStatuses.Add(status);
                }
                if( !context.TicketStatuses.Any(tst => tst.Name == "CantSolve") ) {
                    var status = new TicketStatus() {
                        Name = "CantSolve",
                        Display = "Can't Solve",
                        DisplayForeColor = "#FFF",
                        DisplayBackColor = "#D74242",

                        Description = "The reporter's issue was not able to be solved due to a block or obstruction that would result from available potential solutions.",
                        IsStarted = true,
                        MustBeAssigned = true,
                        IsInProgress = false,
                        IsPausedPending = false,
                        IsResolved = false,
                        IsClosed = false,
                        IsCanceled = true,
                        IsForReporter = false,
                        IsForStaff = true
                    };
                    context.TicketStatuses.Add(status);
                }
                if( !context.TicketStatuses.Any(tst => tst.Name == "WontSolve") ) {
                    var status = new TicketStatus() {
                        Name = "WontSolve",
                        Display = "Refused/Won't Solve",
                        DisplayForeColor = "#FFF",
                        DisplayBackColor = "#933030",
                        Description = "The reporter's issue will not be solved, because the reporter is inquiring about intended functionality or a procedure that is actually working as intended.",
                        IsStarted = true,
                        MustBeAssigned = false,
                        IsInProgress = false,
                        IsPausedPending = false,
                        IsResolved = false,
                        IsClosed = false,
                        IsCanceled = true,
                        IsForReporter = false,
                        IsForStaff = true
                    };
                    context.TicketStatuses.Add(status);
                }

                if( !context.TicketStatuses.Any(tst => tst.Name == "Closed") ) {
                    var status = new TicketStatus() {
                        Name = "Closed",
                        Display = "Closed",
                        DisplayForeColor = "#FFF",
                        DisplayBackColor = "#7D7D7D",
                        Description = "The reporter's issue has been resolved and has been confirmed as satisfactory with the reporter.",
                        IsStarted = true,
                        MustBeAssigned = false,
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
                        DisplayForeColor = "#FFF",
                        DisplayBackColor = "#4F4F4F",
                        Description = "The reporter's issue was cancled, and has been archived.",
                        IsStarted = true,
                        MustBeAssigned = false,
                        IsInProgress = false,
                        IsPausedPending = false,
                        IsResolved = false,
                        IsClosed = true,
                        IsCanceled = true
                    };
                    context.TicketStatuses.Add(status);
                }

                context.SaveChanges();

                #endregion TicketStatus Creation

                #region Project Workflow Creation

                if( !context.ProjectWorkflows.Any(pwf => pwf.Name == "Blank Workflow") ) {
                    var workflow = new ProjectWorkflow() {
                        Name = "Blank Workflow",
                        Description = "As named, this allows tickets to flow through projects, with all statuses being set manually.",
                        CreatedAt = DateTime.Now,

                        AutoTicketAssignBehavior = ProjectWorkflow.AutoTicketAssignBehaviorType.LeaveUnassigned,

                        CanStaffSetStatusOnInteract = true,
                        CanTicketOwnerSetStatusOnInteract = true
                    };

                    context.ProjectWorkflows.Add(workflow);
                }
                if( !context.ProjectWorkflows.Any(pwf => pwf.Name == "Default Workflow") ) {
                    var workflow = new ProjectWorkflow() {
                        Name = "Default Workflow",
                        Description = "An example workflow that takes tickets into standard statuses upon normal interactions.",
                        CreatedAt = DateTime.Now,

                        AutoTicketAssignBehavior = ProjectWorkflow.AutoTicketAssignBehaviorType.WorkloadBasedAvailability,

                        InitialTicketStatusId = context.TicketStatuses.ToList().First(ts => ts.Name == "New-Unassigned").Id,
                        TicketUnassignedStatusId = context.TicketStatuses.ToList().First(ts => ts.Name == "New-Unassigned").Id,
                        TicketAssignedStatusId = context.TicketStatuses.ToList().First(ts => ts.Name == "New-Assigned").Id,
                        ReporterInteractionStatusId = context.TicketStatuses.ToList().First(ts => ts.Name == "ReporterWaiting").Id,
                        SolverInteractionStatusId = context.TicketStatuses.ToList().First(ts => ts.Name == "Investigation").Id,
                        SuperSolverInteractionStatusId = context.TicketStatuses.ToList().First(ts => ts.Name == "Investigation").Id,
                        ProjMgrInteractionStatusId = context.TicketStatuses.ToList().First(ts => ts.Name == "Investigation").Id,
                        ServerAdminInteractionStatusId = context.TicketStatuses.ToList().First(ts => ts.Name == "Investigation").Id,

                        CanStaffSetStatusOnInteract = true,
                        CanTicketOwnerSetStatusOnInteract = false
                    };

                    context.ProjectWorkflows.Add(workflow);
                }

                context.SaveChanges();

                #endregion Project Workflow Creation

                #region Project Creation

                if( !context.Projects.Any(p => p.Name == "Default Project") ) {
                    var project = new Project() {
                        Name = "Default Project",
                        Description = "The default project that issues get placed into.",
                        TicketTag = "DEF",
                        CreatedAt = DateTime.Now,
                        ProjectManagerId = context.Users.First(u => u.Email == "matt_wendel@hotmail.com").Id,
                        IsActiveProject = true,

                        Members = new List<ApplicationUser>() { context.Users.ToList().First(u => u.Email == "matt_wendel@hotmail.com"),
                                                            context.Users.ToList().First(u => u.Email == "JasonTwichell@coderfoundry.com"),
                                                            context.Users.ToList().First(u => u.Email == "arussell@coderfoundry.com")
                                                          },

                        ActiveWorkflowId = context.ProjectWorkflows.ToList().FirstOrDefault(pwf => pwf.Name == "Default Workflow").Id
                    };
                    context.Projects.Add(project);
                }

                context.SaveChanges();
                #endregion Project Creation

            } catch( DbEntityValidationException e ) {
                foreach( var eve in e.EntityValidationErrors ) {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach( var ve in eve.ValidationErrors ) {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw e;
            }
        }
    }
}