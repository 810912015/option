using System.Collections.Generic;

namespace Com.BitsQuan.Service
{
    public class SubscribeMgr : ISubscribeMgr
    {

        public void Subscribe(List<int> contractIds, bool IsSubscribe)
        {

        }

        public bool ShouldPush(Option.Core.Order o)
        {
            return true;
        }
    }
}
