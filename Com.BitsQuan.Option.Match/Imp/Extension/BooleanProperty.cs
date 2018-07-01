using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match.Imp
{
    /// <summary>
    /// 为对象设置一个bool类型属性
    ///     如:为trader设置一个正在监视属性值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BooleanProperty<T>
    {
        List<T> l = new List<T>();
        object loc = new object();
        public bool Get(T key)
        {
            return l.Contains(key);
        }
        public IEnumerable<T> Items
        {
            get { return l; }
        }
        public void Set(T key, bool value)
        {
            if (value == true)
            {
                lock (loc)
                {
                    if (!l.Contains(key))
                    {
                        l.Add(key);
                    }
                }
            }
            else
            {
                lock (loc)
                {
                    if (l.Contains(key))
                    {
                        l.Remove(key);
                    }
                }
            }
        }
    }
}
