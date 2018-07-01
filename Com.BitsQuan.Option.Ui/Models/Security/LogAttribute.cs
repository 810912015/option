using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Ui.Controllers;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers
{
    public class UserOpLog
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime When { get; set; }
        public string Ip { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Prms { get; set; }
        public UserOpLog() { }
        public UserOpLog(string name, DateTime when, string ip, string action, string controller, string prms)
        {
            this.Name = name; this.When = when; this.Ip = ip; this.Action = action; this.Controller = controller; this.Prms = prms;
        }
    }
    public class UserOpLogSaver : BulkSaver
    {
        public UserOpLogSaver() : base(System.Configuration.ConfigurationManager.ConnectionStrings["AppDb"].ToString(), "UserOpLogs") { }
        protected override void CreateTable()
        {
            table = new DataTable();
            //        [Id] [int] IDENTITY(1,1) NOT NULL,
            table.Columns.Add("Id", typeof(int));
            //[Name] [nvarchar](max) NULL,
            table.Columns.Add("Name", typeof(string));
            //[When] [datetime] NOT NULL,
            table.Columns.Add("When", typeof(DateTime));
            //[Ip] [nvarchar](max) NULL,
            table.Columns.Add("Ip", typeof(string));
            //[Action] [nvarchar](max) NULL,
            table.Columns.Add("Action", typeof(string));
            //[Controller] [nvarchar](max) NULL,
            table.Columns.Add("Controller", typeof(string));
            //[Prms] [nvarchar](max) NULL,
            table.Columns.Add("Prms", typeof(string));
        }
        protected override void AddToTable(object o)
        {
            var r = o as UserOpLog;
            if (r == null) return;
            var dr = table.NewRow();
            dr[1] = r.Name; dr[2] = r.When; dr[3] = r.Ip; dr[4] = r.Action; dr[5] = r.Controller; dr[6] = r.Prms;
            table.Rows.Add(dr);
        }
        protected override bool ShouldSave(object o)
        {
            return o is UserOpLog;
        }
    }
    public static class ObjectExtension
    {
        public static string Detail(this object member)
        {
            if (member == null) return "";
            try
            {
                return JsonConvert.SerializeObject(member);
            }
            catch
            {
                return "";
            }
        }

    }
    public class LogAttribute : ActionFilterAttribute
    {
        static UserOpLogSaver uols = new UserOpLogSaver();
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var n = filterContext.HttpContext.User.Identity.Name;
            var a = filterContext.ActionDescriptor.ActionName;
            var c = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

            StringBuilder sb = new StringBuilder();
            foreach (var v in filterContext.ActionParameters)
            {
                sb.AppendFormat("{0}:{1};", v.Key, v.Detail());
            }
            var when = filterContext.HttpContext.Timestamp;
            var ip = filterContext.HttpContext.Request.UserHostAddress;
            uols.Save(new UserOpLog(n, when, ip, a, c, sb.ToString()));
            base.OnActionExecuting(filterContext);
        }

    }

}