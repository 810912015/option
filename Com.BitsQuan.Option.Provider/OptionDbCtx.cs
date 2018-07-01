using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Infra;
using System.Data.Entity.ModelConfiguration.Conventions;
using Com.BitsQuan.Option.Provider.Migrations;
using Com.BitsQuan.Option.Core.Spot;

namespace Com.BitsQuan.Option.Provider
{
	public class AddtionalTableCreator
	{
		public void CreateTempOrder(OptionDbCtx context)
		{
			var conn = context.Database.Connection.Database;
			var sql1 = "use [" + conn + "] ";
			var sql2 = @" SET ANSI_NULLS ON
SET QUOTED_IDENTIFIER ON
if not exists (select * from dbo.sysobjects where id = object_id(N'TempOrders') and type='U')
begin
CREATE TABLE [dbo].[TempOrders](
[FakeId] [int] IDENTITY(1,1) NOT NULL,
	[Id] [int] NOT NULL,
	[TraderId] [int] NOT NULL,
	[ContractId] [int] NOT NULL,
	[Direction] [int] NOT NULL,
	[ReportCount] [int] NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Count] [int] NOT NULL,
	[DoneCount] [int] NOT NULL,
	[TotalDoneCount] [int] NOT NULL,
	[OrderTime] [datetime] NOT NULL,
	[State] [int] NOT NULL,
	[OrderType] [int] NOT NULL,
	[OrderPolicy] [int] NOT NULL,
	[RequestStatus] [int] NOT NULL,
	[DonePrice] [decimal](18, 2) NOT NULL,
	[IsBySystem] [bit] NOT NULL,
	[TotalDoneSum] [decimal](18, 2) NOT NULL,
	[Detail] [nvarchar](max) NULL
 CONSTRAINT [PK_dbo.TempOrders] PRIMARY KEY CLUSTERED 
(
	[FakeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
ALTER TABLE [dbo].[TempOrders]  WITH CHECK ADD  CONSTRAINT [FK_dbo.TempOrders_dbo.Contracts_ContractId] FOREIGN KEY([ContractId])
REFERENCES [dbo].[Contracts] ([Id])
ON DELETE CASCADE
ALTER TABLE [dbo].[TempOrders] CHECK CONSTRAINT [FK_dbo.TempOrders_dbo.Contracts_ContractId]
ALTER TABLE [dbo].[TempOrders]  WITH CHECK ADD  CONSTRAINT [FK_dbo.TempOrders_dbo.Traders_TraderId] FOREIGN KEY([TraderId])
REFERENCES [dbo].[Traders] ([Id])
ON DELETE CASCADE
ALTER TABLE [dbo].[TempOrders] CHECK CONSTRAINT [FK_dbo.TempOrders_dbo.Traders_TraderId]
 end";
			var sql = sql1 + sql2;
			context.Database.ExecuteSqlCommand(sql);
		}

		public void CreateTempSpotOrder(OptionDbCtx context)
		{
			var conn = context.Database.Connection.Database;
			var sql1 = "use [" + conn + "] ";
			var sql2 = @"   
SET ANSI_NULLS ON 

SET QUOTED_IDENTIFIER ON 
if not exists (select * from dbo.sysobjects where id = object_id(N'TempSpotOrders') and type='U') begin
CREATE TABLE [dbo].[TempSpotOrders](
	[Id] [int] NOT NULL,
	[TraderId] [int] NOT NULL,
	[CoinId] [int] NOT NULL,
	[Direction] [int] NOT NULL,
	[ReportCount] [decimal](18, 2) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[Count] [decimal](18, 2) NOT NULL,
	[DoneCount] [decimal](18, 2) NOT NULL,
	[TotalDoneCount] [decimal](18, 2) NOT NULL,
	[OrderTime] [datetime] NOT NULL,
	[State] [int] NOT NULL,
	[RequestStatus] [int] NOT NULL,
	[DonePrice] [decimal](18, 2) NOT NULL,
	[IsBySystem] [bit] NOT NULL,
	[TotalDoneSum] [decimal](18, 2) NOT NULL,
	[Detail] [nvarchar](max) NULL,
	[OrderPolicy] [int] NOT NULL,
 CONSTRAINT [PK_dbo.TempSpotOrders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
 

ALTER TABLE [dbo].[TempSpotOrders]  WITH CHECK ADD  CONSTRAINT [FK_dbo.TempSpotOrders.Coins_CoinId] FOREIGN KEY([CoinId])
REFERENCES [dbo].[Coins] ([Id])
ON DELETE CASCADE 

ALTER TABLE [dbo].[TempSpotOrders] CHECK CONSTRAINT [FK_dbo.TempSpotOrders.Coins_CoinId] 

ALTER TABLE [dbo].[TempSpotOrders]  WITH CHECK ADD  CONSTRAINT [FK_dbo.TempSpotOrders.Traders_TraderId] FOREIGN KEY([TraderId])
REFERENCES [dbo].[Traders] ([Id])
ON DELETE CASCADE 

ALTER TABLE [dbo].[TempSpotOrders] CHECK CONSTRAINT [FK_dbo.TempSpotOrders.Traders_TraderId] 

end
";
			var sql = sql1 + sql2;
			context.Database.ExecuteSqlCommand(sql);
		}

		public void CreateQueryUserOrderDealsStoredProcedure(OptionDbCtx db)
		{
            String sql = @"if exists (select * from sys.objects where object_id=OBJECT_ID(N'QueryUserOrderDeals') and type='P')
drop procedure QueryUserOrderDeals
execute('create procedure QueryUserOrderDeals
@name nvarchar(100),
@take int,
@skip int

as
begin
select top (@take) * 
from(
select row_number() over(order by d.DealTime desc) as Rid,* from(
select 
 a.MainOrderId as MainOrderId,
 b.ReportCount  as ReportCount,
b.Direction as Direction,
b.OrderPolicy  as OrderPolicy,
 b.OrderTime  as OrderTime,
 b.Count  as Count,
 b.Price as Price,
 b.[State]as [State], 
 b.OrderType as [OrderType], 
a.[When] as DealTime,
a.[Count] as DealCount,
a.Price as DealPrice,
con.Code as Code,
con.Name as Name
from Deals a left join Orders b on a.MainOrderId=b.Id
left join Contracts con on a.ContractId =con.Id
where a.MainName=@name

union

select 
a.SlaveOrderId  as MainOrderId,
c.ReportCount  as ReportCount,
 c.Direction  as Direction,
 c.OrderPolicy  as OrderPolicy,
c.OrderTime  as OrderTime,
 c.Count  as Count,
 c.Price  as Price,
c.[State]  as [State], 
 c.OrderType  as [OrderType], 
a.[When] as DealTime,
a.[Count] as DealCount,
a.Price as DealPrice,
con.Code as Code,
con.Name as Name
from Deals a 
left join Orders c on a.SlaveOrderId =c.Id
left join Contracts con on a.ContractId =con.Id
where a.SlaveName=@name
) d
) e
where e.Rid>@skip
end')";

			db.Database.ExecuteSqlCommand(sql);
		}

        public void CreateQueryUserSpotDealsStoredProcedure(OptionDbCtx db) {
            string sql=@"if exists (select * from sys.objects where object_id=OBJECT_ID(N'QueryUserSpotDeals') and type='P')
drop procedure QueryUserSpotDeals
execute('create procedure QueryUserSpotDeals
@name nvarchar(100),
@take int,
@skip int
as
begin

select top (@take) * 
from(
select 
ROW_NUMBER() over(order by (a.Id)  desc ) as Rid,
 a.MainId as MainOrderId,
b.ReportCount  as ReportCount,
 b.Direction as Direction,
 b.CoinId  as OrderPolicy,
 b.OrderTime  as OrderTime,
 b.Count  as Count,
 b.Price  as Price,
b.[State] as [State], 
a.[When] as DealTime,
a.[Count] as DealCount,
a.Price as DealPrice
from SpotDeals a left join SpotOrders b on a.MainId=b.Id
where a.MainTraderName=@name

union

select 
ROW_NUMBER() over(order by (a.Id)  desc ) as Rid,
a.SlaveId  as MainOrderId,
 c.ReportCount  as ReportCount,
c.Direction  as Direction,
 c.CoinId  as OrderPolicy,
c.OrderTime  as OrderTime,
 c.Count  as Count,
c.Price  as Price,
 c.[State]  as [State], 
a.[When] as DealTime,
a.[Count] as DealCount,
a.Price as DealPrice
from SpotDeals a  
left join SpotOrders c on a.SlaveId =c.Id
where a.SlaveTraderId=@name
) d
where d.Rid>@skip
end')";

            db.Database.ExecuteSqlCommand(sql);
        }
	}
	public class OptionDbInitializer : CreateDatabaseIfNotExists<OptionDbCtx>
	{

		public override void InitializeDatabase(OptionDbCtx context)
		{
			base.InitializeDatabase(context);

			AddtionalTableCreator atc = new AddtionalTableCreator();
			atc.CreateTempOrder(context);
			atc.CreateTempSpotOrder(context);
			atc.CreateQueryUserOrderDealsStoredProcedure(context);
            atc.CreateQueryUserSpotDealsStoredProcedure(context);
		}
		protected override void Seed(OptionDbCtx context)
		{
			var initor = new TraderInitor();
			initor.Init(context);

			//var ci = new ContractInitor();
			//ci.Init(context);
			base.Seed(context);
		}
	}
	public class OptionDbCtx : DbContext
	{
		static OptionDbCtx()
		{
			Database.SetInitializer<OptionDbCtx>(new OptionDbInitializer());
		}

		public OptionDbCtx() : this("OptionDb") { }
		public OptionDbCtx(string connStr) : base(connStr) { }

		//public void SaveAllChanges() { this.SaveAllChanges(); }

		public DbSet<GlobalPrm> GlobalPrms { get; set; }

		public DbSet<Trader> Traders { get; set; }
		public DbSet<Contract> Contracts { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<AccountTradeRecord> AccountRecords { get; set; }
		public DbSet<SysAccountRecord> SysAccountRecords { get; set; }
		public DbSet<Deal> Deals { get; set; }

		public DbSet<Ohlc> Ohlcs { get; set; }
		public DbSet<BlastRecord> BlastRecords { get; set; }
		public DbSet<BlasterOperaton> BlastOperations { get; set; }
		public DbSet<FuseRecord> FuseRecords { get; set; }

		public DbSet<PredefinedCondition> PredefinedConditions { get; set; }
		public DbSet<TraderMsg> TraderMsgs { get; set; }
		public DbSet<ContractExecuteRecord> ContractExecuteRecords { get; set; }

		public DbSet<SpotOrder> SpotOrders { get; set; }
		public DbSet<SpotDeal> SpotDeals { get; set; }

		public DbSet<MarketRecord> MarketRecords { get; set; }

		public DbSet<PositionSummaryData> PosData { get; set; }
	}
}
