using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Linq;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 操作合约和用户
    /// </summary>
    public class EntitySvcImp : SvcImpBase
    {
        IOptionModel Model;
        public EntitySvcImp(IOptionModel Model)
        {
            this.Model = Model;
        }

        public OperationResult AddContract(string coinName, DateTime exeTime, decimal exePrice, OptionType optionType, string target, decimal coinCount = 1)
        {
            return Operate(() =>
            {
                if (exeTime < DateTime.Now)
                {
                    return new OperationResult(1, "行权日不能小于当前日期");
                }
                var q = Model.Contracts.Where(a => a.ExcuteTime == exeTime && a.ExcutePrice == exePrice && a.OptionType == optionType && a.CoinCount == coinCount && a.Coin.Name == coinName.ToUpper() && a.IsDel == false);

                if (q != null && q.Count() > 0)
                {
                    return new OperationResult(1, "已包含此合约");
                }
                var r = Model.CreateContract(coinName, exeTime, exePrice, optionType, target, coinCount);
                if (r == null)
                {
                    return new OperationResult(1, "添加失败");
                }
                else
                {
                    Model.AddContract(r);
                    return OperationResult.SuccessResult;
                }
            });
        }
        public OperationResult DisableContract(int contractId)
        {
            return Operate(() =>
            {
                var q = Model.Contracts.Where(a => a.Id == contractId && a.IsDel == false).FirstOrDefault();
                if (q == null) return new OperationResult { ResultCode = 201, Desc = "没有此合约" };
                q.IsDel = true;
                return OperationResult.SuccessResult;
            });
        }
        public OperationResult CreateTrader(string name, bool IsAutoAddBailFromCache, bool IsAutoSellRight)
        {
            return Operate(() =>
            {
                var q = Model.Traders.Where(a => a.Name == name).FirstOrDefault();
                if (q != null) return new OperationResult { ResultCode = 101, Desc = "用户已存在" };
                var r = Model.CreateTrader(name);
                r.IsAutoAddBailFromCache = IsAutoAddBailFromCache;
                r.IsAutoSellRight = IsAutoSellRight;
                Model.AddTrader(r);
                return OperationResult.SuccessResult;
            });
        }
        public OperationResult UpdateTrader(int traderId, TraderUpdateType type, object value)
        {
            return Operate(() =>
            {
                OperationResult r = null;
                var trader = Model.Traders.Where(a => a.Id == traderId).FirstOrDefault();
                if (trader == null) return new OperationResult(101, "无此用户");
                switch (type)
                {
                    case TraderUpdateType.提现:
                    case TraderUpdateType.充值:
                        {
                            if (!(value is decimal)) return new OperationResult(301, "参数类型错误");
                            var tv = (decimal)value;
                            if (tv <= 0) return new OperationResult(302, "充值提现事金额不能小于等于0");
                            TraderService.OperateAccount(trader, tv, (AccountChangeType)(int)type, trader.Name, null);
                            r = OperationResult.SuccessResult;
                        }
                        break;
                    case TraderUpdateType.设置保证金自动转入:
                        {
                            if (!(value is bool)) return new OperationResult(301, "参数类型错误");
                            var sv = (bool)value;
                            trader.IsAutoAddBailFromCache = sv;
                            r = OperationResult.SuccessResult;
                        }
                        break;
                    case TraderUpdateType.设置冻结用户:
                        {
                            if (!(value is bool)) return new OperationResult(301, "参数类型错误");
                            var sv = (bool)value;
                            trader.IsFrozen = sv;
                            r = OperationResult.SuccessResult;
                        }
                        break;
                    case TraderUpdateType.设置自动买平:
                        {
                            if (!(value is bool)) return new OperationResult(301, "参数类型错误");
                            var sv = (bool)value;
                            trader.IsAutoSellRight = sv;
                            r = OperationResult.SuccessResult;
                        }
                        break;
                    default:
                        r = new OperationResult(303, "不允许此种类型的操作");
                        break;
                }
                return r;
            });

        }
    }
}
