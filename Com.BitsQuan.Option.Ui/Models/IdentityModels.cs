using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Provider.Migrations;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;

namespace Com.BitsQuan.Option.Ui.Models
{   

    /// <summary>
    /// 菜单表
    /// </summary>
    [Table("Menu")]
    public class Menu
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Display(Name = "菜单名称")]
        public string Name { get; set; }
        [Display(Name = "上级ID")]
        public int? SubId { get; set; }
        [ForeignKey("SubId")]
        public virtual Menu BelongTo { get; set; }
        public virtual List<Menu> SubMenus { get; set; }
        [Display(Name = "链接地址")]
        public string Url { get; set; }
        [Display(Name = "排序编号")]
        public int orderId { get; set; }
        [Display(Name = "创建时间")]
        public DateTime? CreateDate { get; set; }
        [Display(Name = "备注")]
        public string Remark { get; set; }

        public virtual ICollection<ApplicationRole> Roles { get; set; }
    }

    public class MenuInit : IDbInit
    {
        public DbInitType Type
        {
            get { return DbInitType.Init; }
        }

        public void Init(System.Data.Entity.DbContext db)
        {
            List<Tuple<string, string>> l = new List<Tuple<string, string>>();
            l.Add(Tuple.Create("管理主页", "supervise/main/index"));
            l.Add(Tuple.Create("审核", "supervise/audit/index"));
            l.Add(Tuple.Create("用户", "supervise/appusers/index"));
            l.Add(Tuple.Create("货币", "supervise/coin/index"));
            l.Add(Tuple.Create("合约", "supervise/contracts/index"));
            l.Add(Tuple.Create("论坛", "supervise/forum/index"));
            l.Add(Tuple.Create("帮助", "supervise/helper/index"));
            l.Add(Tuple.Create("新闻", "supervise/new/index"));
            l.Add(Tuple.Create("数据", "supervise/tradedata/index"));
            l.Add(Tuple.Create("安全", "supervise/security/index"));
            l.Add(Tuple.Create("监控", "supervise/snap/index"));
            l.Add(Tuple.Create("参数", "supervise/parameter/index"));
            l.Add(Tuple.Create("网站参数", "supervise/siteParameter/index"));
            l.Add(Tuple.Create("交易用户", "supervise/appusers/index"));
            List<Menu> lm = new List<Menu>();
            for (int i = 0; i < l.Count; i++)
            {
                var m = new Menu { Id = i + 1, Name = l[i].Item1, Url = l[i].Item2 };
                lm.Add(m);
            }
            db.Set<Menu>().AddOrUpdate(lm.ToArray());
            db.SaveChanges();
        }
    }
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
   
        public string IdNumber { get; set; }
        public bool IsAllowToTrade { get; set; }
        public DateTime? RegisterTime { get; set; }
        public string TradePwd { get; set; }

        public string IdNumberType { get; set; }//证件类型
        public string RealityName { get; set; }//真实姓名

        public string IdentiTime { get; set; }//认证时间

        public DateTime? EnderrorTime { get; set; }//最后一次错误时间
        public int error { get; set; }//错误次数（到了24点自动将所有用户的改为0）

        public string tradePwdCount { get; set; }//交易密码输入次数控制

        public string Uiden { get; set; }//用户标识
        /// <summary>
        /// 推荐人id
        /// </summary>
        public string InvitorId { get; set; }
        /// <summary>
        /// 手续费返回费率
        /// </summary>
        public decimal InvitorFeeRatio { get; set; }
        /// <summary>
        /// 累计推荐人数
        /// </summary>
        public int InviteCount { get; set; }
        /// <summary>
        /// 累计获得的手续费数
        /// </summary>
        public decimal InviteFeeSum { get; set; }
        /// <summary>
        /// 累计获得的奖金数
        /// </summary>
        public decimal InviteBonusSum { get; set; }
        /// <summary>
        /// 是不是推荐人
        /// </summary>
        public bool IsInvitor { get; set; }
        /// <summary>
        /// 推荐人最后奖金最后划转时间
        /// </summary>
        public DateTime? LastTransferFeeTime { get; set; }
    }

    public class ApplicationRole : IdentityRole
    {
        public string Description { get; set; }
        public virtual List<Menu> Menus { get; set; }

    }


    public class AppDbInit:CreateDatabaseIfNotExists<ApplicationDbContext>
    {
       static readonly DateTime d70 = new DateTime(2000, 1, 1);
       protected override void Seed(ApplicationDbContext context)
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
           for (int i = -2; i < 0; i++)
           {
               string name = "robot" + i * (-1);
               string password = "123456";

               var user = new ApplicationUser();
               user.UserName = name;
               user.Email = "a@a.com";
               user.PhoneNumber = "15921462689";
               user.IdNumber = "123456789012345678";
               user.RegisterTime = DateTime.Now;
               user.TradePwd = "999999";
               user.Uiden = iden;
               user.tradePwdCount = "1";
               user.EmailConfirmed = true;
               //   user.Uiden = "555";
               var adminresult = UserManager.Create(user, password);


               if (adminresult.Succeeded)
               {
                   var result = UserManager.AddToRole(user.Id, "交易员");
               }
           }
           var rs = Com.BitsQuan.Miscellaneous.AppSettings.Read<bool>("hasHello", false);
             if (rs)
             {
                 for (int i = 1; i < 101; i++)
                 {
                     string name = "hello" + i;
                     string password = "123456";

                     var user = new ApplicationUser();
                     user.UserName = name;
                     user.Email = "a@a.com";
                     user.PhoneNumber = "15921462689";
                     user.IdNumber = "123456789012345678";
                     user.RegisterTime = DateTime.Now;
                     user.TradePwd = "00000";
                     user.Uiden = iden;
                     user.tradePwdCount = "1";
                     user.EmailConfirmed = true;
                     //  user.Uiden = "555";
                     var adminresult = UserManager.Create(user, password);


                     if (adminresult.Succeeded)
                     {
                         var result = UserManager.AddToRole(user.Id, "交易员");
                     }
                 }
             }
           // context.SaveChanges();
           List<string> rn = new List<string> { "资金管理员", "权限管理员", "网站管理员", "交易管理员" };
           List<string> un = new List<string> { "bqcachemgr", "bqauthmgr", "bqsitemgr", "bqtrademgr" };

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
                       tradePwdCount = "1",
                       EmailConfirmed = true
                       // Uiden = "555",
                   };
                   var ar = UserManager.Create(u, u.UserName);
                   if (ar.Succeeded)
                   {
                       var r = UserManager.AddToRole(u.Id, rn[i]);
                   }
               }
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
           base.Seed(context);
       }
    }
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(new AppDbInit());
        }
        public ApplicationDbContext()
            : base("AppDb")
        {
        }
        public DbSet<Help> Helps { get; set; }
        public DbSet<ForumHost> ForumHosts { get; set; }
        public DbSet<ForumReply> ForumReplys { get; set; }

        public DbSet<BankAccount> BankAccounts { get; set; }

        public DbSet<CurrenAddress> CurrenAddress { get; set; }
        public DbSet<BankRecord> BankRecords { get; set; }
        public DbSet<BigType> BigTypes { get; set; }
        public DbSet<SmallType> SmallTypes { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<SiteParameter> SiteParameters { get; set; }
        public DbSet<UserOpLog> UserLogs { get; set; }

        public DbSet<SysBankRecord> SysBankRecords { get; set; }

        public DbSet<FriendlyLink> FriendlyLinks { get; set; }
        public DbSet<AdvImg> AdvImgs { get; set; }
        public DbSet<WebHelper> WebHelpers { get; set; }
    }
    public class BankAccount
    {
        [Key]
        [Display(Name = "银行卡号")]
        public string Number { get; set; }
        // public virtual ApplicationUser User { get; set; }
        public string Uname { get; set; }
        [Display(Name="银行卡名称")]
        public string Name { get; set; }
        [Display(Name = "银行")]
        public string BankName { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>
        [Display(Name = "开户省")]
        public string Province { get; set; }
        [Display(Name = "开户市")]
        public string City { get; set; }
        /// <summary>
        /// 营业部名称
        /// </summary>
        [Display(Name = "营业部")]
        public string SalesOfficeName { get; set; }
        public bool IsDel { get; set; }
        public bool IsSystem { get; set; }
    }
    public class SysBankRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public virtual BankAccount Account { get; set; }
        public decimal Delta { get; set; }
        public DateTime When { get; set; } 
        public string Desc { get; set; }
        public int  ApproveId { get; set; }
        public string By { get; set; }
    }
    /// <summary>
    /// BTC提现地址
    /// </summary>
    public class CurrenAddress
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// 地址名称
        /// </summary>
        public string Name { get; set; }
        //用户
        //    public virtual ApplicationUser Userap { get; set; }
          public  string Uname { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 货币类型
        /// </summary>
        public string Coin { get; set; }
        public bool IsDel { get; set; }

        
    }
    public enum BankRecordType
    {
        充值 = 1, 提现 = 2,
    }
    public class BankRecord:IEntityWithId
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("Account")]
        public string AccountNum { get; set; }
        public virtual BankAccount Account { get; set; }
        public BankRecordType BankRecordType { get; set; }
        public decimal Delta { get; set; } //金额
        public decimal ActualDelta { get; set; }
        public DateTime When { get; set; }
        public string AppUserName { get; set; }
        public string ByWho { get; set; }
        public DateTime? AuditTime { get; set; }
        public bool IsApproved { get; set; }
        public bool? ApprovedResult { get; set; }
        public string ApproveDesc { get; set; }

        public string AddressNum { get; set; }
        public string Address { get; set; }
        public string Uid { get; set; }//用户标志
     
        /// <summary>
        /// 充值或提现数量
        /// </summary>
        public decimal Num { get; set; }
        //货币类型
        public string coin { get; set; }

        public virtual SysBankRecord SysRecord { get; set; }

    }


    public class FriendlyLink
    {
        [Key]
        [Display(Name = "序号")]
        public int Id { get; set; }
        [Display(Name = "链接名称")]
        public string LinkName { get; set; }
        [Display(Name = "链接地址")]
        public string LinkUrl { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>
        [Display(Name = "链接类型")]
        public string LinkType { get; set; }
        [Display(Name = "排序")]
        public string SortId { get; set; }

    }

    public class AdvImg
    {
        [Key]
        [Display(Name = "序号")]
        public int Id { get; set; }
        [Display(Name = "链接名称")]
        public string LinkName { get; set; }
        [Display(Name = "链接地址")]
        public string LinkUrl { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>
        [Display(Name = "链接类型")]
        public string ImageAddress { get; set; }
        [Display(Name = "排序")]
        public string SortId { get; set; }

    }
    public class WebHelper
    {
        [Key]
        [Display(Name = "序号")]
        public int Id { get; set; }
        [Display(Name = "网页标题")]
        public string WebTitle { get; set; }
        [Display(Name = "网页参数")]
        public string WebEnd { get; set; }
        /// <summary>
        /// 开户行
        /// </summary>
        [Display(Name = "网页内容")]
        public string WebContent { get; set; }
        [Display(Name = "排序")]
        public string SortId { get; set; }

    }
}