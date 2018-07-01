
using Com.BitsQuan.Option.Match;
using System;
namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 撮合接口
    /// </summary>
    public interface IMatch:IDisposable
    {
        void Handle(Order o);
        bool Redo(Order o);
        event Action<Order> OnBeforeMatch;
        event Action<Order> OnFinish;
        event Action<Deal> OnDeal;
        IMatcherDataContainer Container { get; }
    }
}
