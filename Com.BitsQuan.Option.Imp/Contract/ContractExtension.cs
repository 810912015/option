using Com.BitsQuan.Option.Core;

namespace Com.BitsQuan.Option.Imp
{
    public static class ContractExtension
    {
        public static void SetCodeAndName(this Contract entity)
        {
            if (entity.Code == null)
                entity.Code = SingletonWithInit<ContractService>.Instance.GenerateOptionContractCode(entity.Coin,
                               entity.ContractType, entity.ExcuteTime, entity.OptionType,entity.TimeSpanType);
            if (entity.Name == null)
                entity.Name = SingletonWithInit<ContractService>.Instance.GenerateOptionContractName(entity.Coin, entity.ExcuteTime, entity.OptionType, entity.ExcutePrice);
        }
    }
}
