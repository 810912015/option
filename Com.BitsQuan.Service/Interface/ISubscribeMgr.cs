using Com.BitsQuan.Option.Core;
using System.Collections.Generic;

namespace Com.BitsQuan.Service
{
    public interface ISubscribeMgr
    {
        void Subscribe(List<int> contractIds, bool IsSubscribe);
        bool ShouldPush(Order o);
    }
}
