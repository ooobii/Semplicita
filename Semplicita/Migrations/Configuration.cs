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

            #endregion User Creation

        }
    }
}
