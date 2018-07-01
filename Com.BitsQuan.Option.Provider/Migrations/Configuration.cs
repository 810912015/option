namespace Com.BitsQuan.Option.Provider.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Com.BitsQuan.Option.Provider.OptionDbCtx>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }

        protected override void Seed(Com.BitsQuan.Option.Provider.OptionDbCtx context)
        {
            AddtionalTableCreator atc = new AddtionalTableCreator();
            atc.CreateTempOrder(context);
            atc.CreateTempSpotOrder(context);
             
            var initor = new TraderInitor();
            initor.Init(context);

            //var ci = new ContractInitor();
            //ci.Init(context); 
        }
    }
}
