using System.Collections.Generic;

namespace Com.BitsQuan.Option.Match
{
    /// <summary>
    /// 为某对象添加扩展属性使用到的数据结构
    /// </summary>
    /// <typeparam name="TTarget">目标类型</typeparam>
    /// <typeparam name="TProperty">扩展属性类型</typeparam>
    public class MyProperty<TTarget, TProperty>
    {
        Dictionary<TTarget, TProperty> dic = new Dictionary<TTarget, TProperty>();
        object loc = new object();

        public TProperty Get(TTarget tar)
        {
            if (dic.ContainsKey(tar))
            {
                return dic[tar];
            }
            else
            {
                return default(TProperty);
            }
        }
        public void Set(TTarget tar, TProperty pro)
        {
            if (dic.ContainsKey(tar))
                dic[tar] = pro;
            else
            {
                lock (loc)
                {
                    if (!dic.ContainsKey(tar))
                    {
                        dic.Add(tar, pro);
                    }
                }
            }

        }
        public void Clear(TTarget tar)
        {
            lock(loc)
            dic.Remove(tar);
        }
    }
}
