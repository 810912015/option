using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Provider;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;

namespace Com.BitsQuan.Option.Imp
{
    public class ContractRepo : BaseRepository<Contract>
    {
        public ContractRepo(OptionDbCtx db) { this.dbContext = db; }

        public bool Add(string coinName, DateTime exeTime, decimal exePrice, OptionType optionType, string target)
        {
            var c = new Contract
            {
                Id=IdService<Contract>.Instance.NewId(),
                ExcutePrice = exePrice,
                ExcuteTime = exeTime,
                CoinId =CoinRepo.Instance.GetByName(coinName).Id,
                 Coin= CoinRepo.Instance.GetByName(coinName),
                ContractType = ContractType.期权,OptionType =optionType ,
                Target = target
            };
            return Add(c);
        }

        public override bool Add(Contract entity)
        {
            entity.SetCodeAndName();
            var adapter = dbContext as IObjectContextAdapter;
            adapter.ObjectContext.AttachTo("Coins", entity.Coin);
            return base.Add(entity);
        }
        public override bool AddRanged(IEnumerable<Contract> entities)
        {
            foreach (var v in entities)
                v.SetCodeAndName();
            return base.AddRanged(entities);
        }

    }
}
