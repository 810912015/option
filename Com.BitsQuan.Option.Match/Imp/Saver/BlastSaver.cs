using Com.BitsQuan.Option.Core;
using System;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{
    public class BlastSaver : BulkSaver
    {
        public BlastSaver() : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "BlastRecords",1) { }

        protected override void CreateTable()
        {
            table = new DataTable();

            //[Id] [int] NOT NULL,
            table.Columns.Add("Id", typeof(int));
            //[TraderId] [int] NOT NULL,
            table.Columns.Add("TraderId", typeof(int));
            //[BailTotal] [decimal](18, 2) NOT NULL,
            table.Columns.Add("BailTotal", typeof(decimal));
            //[NeededBail] [decimal](18, 2) NOT NULL,
            table.Columns.Add("NeededBail", typeof(decimal));
            //[BlastType] [int] NOT NULL,
            table.Columns.Add("BlastType", typeof(int));
            //[StartTime] [datetime] NOT NULL,
            table.Columns.Add("StartTime", typeof(DateTime));
        }

        protected override void AddToTable(object o)
        {
            var r = o as BlastRecord;
            DataRow dr = table.NewRow();
            dr[0] = r.Id; dr[1] = r.TraderId; dr[2] = r.BailTotal; dr[3] = r.NeededBail;
            dr[4] = r.BlastType; dr[5] = r.StartTime;
            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is BlastRecord;
        }
    }

    #region gigant test savers
    public class AccountSaver : BulkSaver
    {
        public AccountSaver():
            base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(),
            "Accounts", 10000) { }
        protected override void CreateTable()
        {
            table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Sum", typeof(decimal));
            table.Columns.Add("Frozen", typeof(decimal));
            table.Columns.Add("Discriminator", typeof(string));
            table.Columns.Add("CacheType_Id", typeof(int));
        }

        protected override void AddToTable(object o)
        {
            var a = o as Account;
            var d = table.NewRow();
            d[0] = a.Id;
            d[1] = a.Sum;
            d[2] = a.Frozen;
            d[3] = (o is BailAccount) ? "BailAccount" : "Account";
            d[4] = a.CacheType.Id;
            table.Rows.Add(d);
        }

        protected override bool ShouldSave(object o)
        {
            return o is Account;
        }
    }
    public class CacheAccountSaver : BulkSaver
    {
        public CacheAccountSaver():
            base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(),
            "CacheAccounts", 10000) { }

        protected override void CreateTable()
        {
            table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("BtcAccount_Id", typeof(int));
            table.Columns.Add("CnyAccount_Id", typeof(int));
        }

        protected override void AddToTable(object o)
        {
            var c = o as CacheAccount;
            var d = table.NewRow();
            d[0] = c.Id;
            d[1] = c.BtcAccount.Id;
            d[2] = c.CnyAccount.Id;
            table.Rows.Add(d);
        }

        protected override bool ShouldSave(object o)
        {
            return o is CacheAccount;
        }
    }
    public class TraderAccountSaver : BulkSaver
    {
        public TraderAccountSaver():
            base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(),
            "TraderAccounts", 10000) { }
        protected override void CreateTable()
        {
            table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("BailAccount_Id", typeof(int));
            table.Columns.Add("CacheAccount_Id", typeof(int));
        }

        protected override void AddToTable(object o)
        {
            var t = o as TraderAccount;
            var d = table.NewRow();
            d[0] = t.Id;
            d[1] = t.BailAccount.Id;
            d[2] = t.CacheAccount.Id;
            table.Rows.Add(d);
        }

        protected override bool ShouldSave(object o)
        {
            return o is TraderAccount;
        }
    }
    public class TraderSaver : BulkSaver
    {
        public TraderSaver() :
            base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), 
            "Traders", 10000) { }
        protected override void CreateTable()
        {
            table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("IsAutoAddBailFromCache", typeof(bool));
            table.Columns.Add("IsAutoSellRight", typeof(bool));
            table.Columns.Add("IsFrozen", typeof(bool));
            table.Columns.Add("Account_Id", typeof(int));
        }

        protected override void AddToTable(object o)
        {
            var t = o as Trader;
            var d = table.NewRow();
            d[0] = t.Id; d[1] = t.Name; d[2] = t.IsAutoAddBailFromCache;
            d[3] = t.IsAutoSellRight;
            d[4] = t.IsFrozen;
            d[5] = t.Account.Id;
            table.Rows.Add(d);
        }

        protected override bool ShouldSave(object o)
        {
            return o is Trader;
        }
    }

    public class UserSaver : BulkSaver
    {
        public UserSaver() :
            base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), 
            "Traders", 1) { }
        protected override void CreateTable()
        {
            table = new DataTable();
            table.Columns.Add("Id", typeof(string));
            table.Columns.Add("IdNumber", typeof(string));
            table.Columns.Add("IsAllowToTrade", typeof(bool));
            table.Columns.Add("RegisterTime", typeof(DateTime));
            table.Columns.Add("TradePwd", typeof(string));
            table.Columns.Add("IdNumberType", typeof(string));
            table.Columns.Add("RealityName", typeof(string));
            table.Columns.Add("IdentiTime", typeof(string));
            table.Columns.Add("EnderrorTime", typeof(DateTime));
            table.Columns.Add("error", typeof(int));
            table.Columns.Add("tradePwdCount", typeof(string));
            table.Columns.Add("Uiden", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("EmailConfirmed", typeof(bool));
            table.Columns.Add("PasswordHash", typeof(string));
            table.Columns.Add("SecurityStamp", typeof(string));
            table.Columns.Add("PhoneNumber", typeof(string));
            table.Columns.Add("PhoneNumberConfirmed", typeof(string));
            table.Columns.Add("TwoFactorEnabled", typeof(string));
            table.Columns.Add("LockoutEndDateUtc", typeof(string));
            table.Columns.Add("LockoutEnabled", typeof(string));
            table.Columns.Add("AccessFailedCount", typeof(string));
            table.Columns.Add("UserName", typeof(string)); 
        }

        protected override void AddToTable(object o)
        {
            throw new NotImplementedException();
        }

        protected override bool ShouldSave(object o)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
