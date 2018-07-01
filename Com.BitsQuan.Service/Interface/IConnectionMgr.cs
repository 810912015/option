using Com.BitsQuan.Option.Imp;

namespace Com.BitsQuan.Service
{
    public interface IConnectionMgr<T>
    {
        OperationResult Login(string name, string pwd, T callback);
        string CurUserName { get; }
    }
}
