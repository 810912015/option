using System;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 初始化:在对象建立时执行初始化任务
    /// </summary>
    public interface IInitialbe
    {
        void Init();
    }
    /// <summary>
    /// 数据保存:需要时将数据保存(至数据库或文件)
    /// </summary>
    public interface IFlush
    {
        void Flush();
    }
    public class SingletonWithInit<T> where T : IInitialbe,new()
    {
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                        _instance.Init();
                        return _instance;
                    }
                        
                }
                return _instance;
            }
        }

        private static T _instance;
        private static readonly object Lock = new Object();
    }
    public class Singleton<T> where T : new()
    {
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (Lock)
                {
                    if (_instance == null)
                        return _instance = new T();
                }
                return _instance;
            }
        }

        private static T _instance;
        private static readonly object Lock = new Object();
    }
}
