using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Imp;

namespace Com.BitsQuan.Option.Match.Spot
{
    public class SpotPost
    {
        /// <summary>
        /// 卖:冻结虚拟币
        /// 买:冻结现金
        /// 同时将委托添加到用户委托列表
        /// </summary>
        /// <param name="so"></param>
        public void Handle(SpotOrder so)
        {
            so.Freeze();
            so.State = OrderState.等待中;
            so.RequestStatus = OrderRequestStatus.挂单成功;
        }
         
    }
}
