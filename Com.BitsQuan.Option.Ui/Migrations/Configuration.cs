namespace Com.BitsQuan.Option.Ui.Migrations
{
    using Com.BitsQuan.Option.Ui.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Com.BitsQuan.Option.Ui.Models.ApplicationDbContext>
    {
        static readonly DateTime d70 = new DateTime(2000, 1, 1);
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Com.BitsQuan.Option.Ui.Models.ApplicationDbContext context)
        {

            var mi = new MenuInit();
            mi.Init(context);
            var UserManager = new UserManager<ApplicationUser>(new
                                               UserStore<ApplicationUser>(context));

            var RoleManager = new RoleManager<ApplicationRole>(new
                                           RoleStore<ApplicationRole>(context));

            if (!RoleManager.RoleExists("交易员"))
            {

                var roleresult = RoleManager.Create(new ApplicationRole
                {
                    Name = "交易员"
                });
            }
            string iden = DateTime.Now.Subtract(d70).Milliseconds.ToString();
            for (int i = -10; i > -1; i++)
            {
                string name = "robot" + i * (-1);
                string password = "123456";

                var user = new ApplicationUser();
                user.UserName = name;
                user.IsAllowToTrade = true;
                user.Email = "a@a.com";
                user.PhoneNumber = "13512341234";
                user.IdNumber = "123456789012345678";
                user.RegisterTime = DateTime.Now;
                user.TradePwd = "123456";
                user.Uiden = iden;
                user.EmailConfirmed = true;
                user.tradePwdCount = "n";
                var adminresult = UserManager.Create(user, password);


                if (adminresult.Succeeded)
                {
                    var result = UserManager.AddToRole(user.Id, "交易员");
                }
            }

            for (int i = 1; i < 101; i++)
            {
                string name = "hello" + i;
                string password = "123456";

                var user = new ApplicationUser();
                user.UserName = name;
                user.IsAllowToTrade = true;
                user.Email = "a@a.com";
                user.PhoneNumber = "13512341234";
                user.IdNumber = "123456789012345678";
                user.RegisterTime = DateTime.Now;
                user.TradePwd = "123456";
                user.Uiden = iden;
                user.EmailConfirmed = true;
                user.tradePwdCount = "n";
                var adminresult = UserManager.Create(user, password);


                if (adminresult.Succeeded)
                {
                    var result = UserManager.AddToRole(user.Id, "交易员");
                }
            }
            // context.SaveChanges();
            List<string> rn = new List<string> { "资金管理员", "权限管理员", "网站管理员", "交易管理员" };
            List<string> un = new List<string> { "cachemgr", "authmgr", "sitemgr", "trademgr" };


            for (int i = 0; i < rn.Count; i++)
            {
                if (!RoleManager.RoleExists(rn[i]))
                {
                    var roleresult = RoleManager.Create(new ApplicationRole { Name = rn[i] });
                }
                for (int j = 0; j < 2; j++)
                {
                    var u = new ApplicationUser
                    {
                        UserName = un[i] + j,
                        Email = string.Format("mgr{0}@a.com", (i + 1) * j),
                        RegisterTime = DateTime.Now,
                        PhoneNumber = (12312312312 + (i + 1) * j).ToString(),
                        Uiden = iden,
                        EmailConfirmed = true,
                        tradePwdCount = "n"
                    };
                    var ar = UserManager.Create(u, u.UserName);
                    if (ar.Succeeded)
                    {
                        var r = UserManager.AddToRole(u.Id, rn[i]);
                    }
                }
                //context.SaveChanges();
            };

            context.SaveChanges();

            var role = context.Set<ApplicationRole>().Where(a => a.Name == "网站管理员").First();
            var menus = context.Set<Menu>().ToList();
            foreach (var v in menus)
            {
                if (v.Roles == null) v.Roles = new List<ApplicationRole>();
                v.Roles.Add(role);
            }
            role.Menus = menus;
            context.SaveChanges();

            

            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }
    }
}
