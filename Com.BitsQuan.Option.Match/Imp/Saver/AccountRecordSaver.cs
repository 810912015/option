using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Provider;
using System;
using System.Collections.Generic;
using System.Data;

namespace Com.BitsQuan.Option.Match.Imp
{
    public  class AccountRecordSaver:BulkSaver
    {
        public AccountRecordSaver() : base(System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(), "AccountTradeRecords",13) { }
        protected override void CreateTable()
        {
            table = new DataTable();
           
    //        [Id] [int] IDENTITY(1,1) NOT NULL,
    //[IsAddTo] [bit] NOT NULL,
    //[Delta] [decimal](18, 2) NOT NULL,
    //[When] [datetime] NOT NULL,
    //[OperateType] [int] NOT NULL,
    //[ByWho] [nvarchar](max) NULL,
    //[OrderDesc] [nvarchar](max) NULL,
    //[Current] [decimal](18, 2) NOT NULL,
    //[Who_Id] [int] NULL,


            //[Id] [int] IDENTITY(1,1) NOT NULL,
            table.Columns.Add("Id", typeof(int));
    //[IsAddTo] [bit] NOT NULL,
            table.Columns.Add("IsAddTo", typeof(bool));
    //[Delta] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Delta", typeof(decimal));
    //[When] [datetime] NOT NULL,
            table.Columns.Add("When", typeof(DateTime));
    //[OperateType] [int] NOT NULL,
            table.Columns.Add("OperateType", typeof(int));
    //[ByWho] [nvarchar](max) NULL,
            table.Columns.Add("ByWho", typeof(string));
    
            //OrderDesc
            table.Columns.Add("OrderDesc", typeof(string));
            //[Current] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Current", typeof(decimal));
           
            //[Frozen] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Frozen", typeof(decimal));
            table.Columns.Add("IsBail", typeof(bool));
            //[Who_Id] [int] NULL
            
            table.Columns.Add("CoinId", typeof(int));
            table.Columns.Add("WhoId", typeof(int));


            table.Columns.Add("BailSum", typeof(decimal));
            table.Columns.Add("BailFrozen", typeof(decimal));
            table.Columns.Add("CnySum", typeof(decimal));
            table.Columns.Add("CnyFrozen", typeof(decimal));
            table.Columns.Add("BtcSum", typeof(decimal));
            table.Columns.Add("BtcFrozen", typeof(decimal));
        }

        protected override void AddToTable(object o)
        {
            var r = o as AccountTradeRecord;
            DataRow dr = table.NewRow();
            dr[0] = r.Id;
            dr[1] = r.IsAddTo;
            dr[2] = r.Delta;
            dr[3] = r.When;
            dr[4] = r.OperateType;
            dr[5] = r.ByWho;
            dr[6] = r.OrderDesc;
            dr[7] = r.Current;
           
            dr[8] = r.Frozen;
            dr[9] = r.IsBail;
            dr[10] = r.CoinId;
            dr[11] = r.Who.Id;

            dr[12] = r.Who.Account.BailAccount.Sum;
            dr[13] = r.Who.Account.BailAccount.Frozen;
            dr[14] = r.Who.Account.CacheAccount.CnyAccount.Sum;
            dr[15] = r.Who.Account.CacheAccount.CnyAccount.Frozen;
            dr[16] = r.Who.Account.CacheAccount.BtcAccount.Sum;
            dr[17] = r.Who.Account.CacheAccount.BtcAccount.Frozen;
            
            table.Rows.Add(dr);
        }

        protected override bool ShouldSave(object o)
        {
            return o is AccountTradeRecord;
        }
    }

    public class MarketRecordSaver : BulkSaver
    {
        public MarketRecordSaver() : base(
            System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(),
            "MarketRecords",11) { }
     
        protected override void CreateTable()
        {
            table = new DataTable();
            //[Id] [int] IDENTITY(1,1) NOT NULL,
            table.Columns.Add("Id", typeof(int));
            //[ContractId] [int] NOT NULL,
            table.Columns.Add("ContractId", typeof(int));
            //[NewestPrice] [decimal](18, 2) NOT NULL,
            table.Columns.Add("NewestPrice", typeof(decimal));
            //[SellCount] [int] NOT NULL,
            table.Columns.Add("SellCount", typeof(int));
            //[BuyCount] [int] NOT NULL,
            table.Columns.Add("BuyCount", typeof(int));
            //[FuseMax] [decimal](18, 2) NOT NULL,
            table.Columns.Add("FuseMax", typeof(decimal));
            //[FuseMin] [decimal](18, 2) NOT NULL,
            table.Columns.Add("FuseMin", typeof(decimal));
            //[Times] [int] NOT NULL,
            table.Columns.Add("Times", typeof(int));
            //[Copies] [int] NOT NULL,
            table.Columns.Add("Copies", typeof(int));
            //[NewestDealPrice] [decimal](18, 2) NOT NULL,
            table.Columns.Add("NewestDealPrice", typeof(decimal));
            //[Total] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Total", typeof(decimal));
            //[OpenTotal] [decimal](18, 2) NOT NULL,
            table.Columns.Add("OpenTotal", typeof(int));
            //[Open24] [int] NOT NULL,
            table.Columns.Add("Open24", typeof(int));
            //[Close24] [int] NOT NULL,
            table.Columns.Add("Close24", typeof(int));
            //[PureOpen24] [int] NOT NULL,
            table.Columns.Add("PureOpen24", typeof(int));
            //[High24] [decimal](18, 2) NOT NULL,
            table.Columns.Add("High24", typeof(decimal));
            //[Low24] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Low24", typeof(decimal));
        }

        protected override void AddToTable(object o)
        {
            var mr = o as MarketRecord;
            if (mr == null) return;

            var r = table.NewRow();
            r[1] = mr.ContractId; r[2] = mr.NewestPrice; r[3] = mr.SellCount; r[4] = mr.BuyCount; r[5] = mr.FuseMax; r[6] = mr.FuseMin;
            r[7] = mr.Times; r[8] = mr.Copies; r[9] = mr.NewestDealPrice; r[10] = mr.Total; r[11] = mr.OpenTotal; r[12] = mr.Open24;
            r[13] = mr.Close24; r[14] = mr.PureOpen24; r[15] = mr.High24; r[16] = mr.Low24;
            table.Rows.Add(r);
        }

        protected override bool ShouldSave(object o)
        {
            return o is MarketRecord;
        }
    }


    public class MarketRecordReader
    {
        public List<MarketRecord> Read()
        {
            string sql = "select * from marketrecords where id in (select MAX(id) from marketrecords group by contractId)";
            List<MarketRecord> l = new List<MarketRecord>();
            using (OptionDbCtx db = new OptionDbCtx())
            {
               var q= db.Database.SqlQuery<MarketRecord>(sql);
               foreach (var v in q)
                   l.Add(v);
            }
            return l;
        }
    }


    public class PosDataSaver : BulkSaver
    {
        public PosDataSaver()
            : base(
            System.Configuration.ConfigurationManager.ConnectionStrings["OptionDb"].ToString(),
            "PositionSummaryDatas",5) { }
        protected override void CreateTable()
        {
            table = new DataTable();
            //        [Id] [int] IDENTITY(1,1) NOT NULL,
            table.Columns.Add("Id", typeof(int));
            //[ContractId] [int] NOT NULL,
            table.Columns.Add("ContractId", typeof(int));
            //[PositionType] [nvarchar](max) NULL,
            table.Columns.Add("PositionType", typeof(string));
            //[Count] [int] NOT NULL,
            table.Columns.Add("Count", typeof(int));
            //[ClosableCount] [int] NOT NULL,
            table.Columns.Add("ClosableCount", typeof(int));
            //[BuyPrice] [decimal](18, 2) NOT NULL,
            table.Columns.Add("BuyPrice", typeof(decimal));
            //[BuyTotal] [decimal](18, 2) NOT NULL,
            table.Columns.Add("BuyTotal", typeof(decimal));
            //[FloatProfit] [decimal](18, 2) NOT NULL,
            table.Columns.Add("FloatProfit", typeof(decimal));
            //[CloseProfit] [decimal](18, 2) NOT NULL,
            table.Columns.Add("CloseProfit", typeof(decimal));
            //[Maintain] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Maintain", typeof(decimal));
            //[TotalValue] [decimal](18, 2) NOT NULL,
            table.Columns.Add("TotalValue", typeof(decimal));
            //[OrderType] [int] NOT NULL,
            table.Columns.Add("OrderType", typeof(int));
            //[Commission] [decimal](18, 2) NOT NULL,
            table.Columns.Add("Commission", typeof(decimal));
            table.Columns.Add("TraderId", typeof(int));
            table.Columns.Add("When", typeof(DateTime));
           
        }

        protected override void AddToTable(object o)
        {
            var t = o as PositionSummaryData;
            if (t == null) return;
            var r = table.NewRow();
            r[1] = t.ContractId; r[2] = t.PositionType; r[3] = t.Count; r[4] = t.ClosableCount; r[5] = t.BuyPrice;
            r[6] = t.BuyTotal; r[7] = t.FloatProfit; r[8] = t.ClosableCount; r[9] = t.Maintain;
            r[10] = t.TotalValue; r[11] = t.OrderType; r[12] = t.Commission;
            r[13] = t.TraderId;
            r[14] = t.When;
            table.Rows.Add(r);
        }

        protected override bool ShouldSave(object o)
        {
            return o is PositionSummaryData;
        }
    }
}
