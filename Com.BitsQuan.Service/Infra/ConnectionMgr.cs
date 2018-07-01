using Com.BitsQuan.Option.Imp;

namespace Com.BitsQuan.Service
{
    public class ConnectionMgr<T> : IConnectionMgr<T>
    {

        public OperationResult Login(string name, string pwd, T callback)
        {
            return OperationResult.SuccessResult;
        }

        public string CurUserName
        {
            get { return "hell1"; }
        }
    }
}
