using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Miscellaneous
{
    public class AppSettings
    {
        /// <summary>
        /// 根据键读取指定类型的值。如果不指定默认值在读取失败的情况下会抛出异常。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue">读取失败时使用的默认值</param>
        /// <returns></returns>
        public static T Read<T>(string key, Object defaultVal = null)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new Exception("key不能为空");

            var value = ConfigurationManager.AppSettings[key];
            var raiseException = defaultVal == null;
            T defaultValue = defaultVal == null || !(defaultVal is T) ? default(T) : (T)defaultVal;
            if (value == null)
            {
                if (raiseException)
                    throw new Exception("未找到key");
                else
                    return defaultValue;
            }
            var type = typeof(T);
            if (type.IsEnum)
            {
                if (!type.IsEnumDefined(value))
                {
                    if (raiseException)
                        throw new Exception("配置值无效");
                    else
                        return defaultValue;
                }
                else
                    return (T)Enum.Parse(type, value, true);
            }
            else
            {
                try
                {
                    return (T)TypeDescriptor.GetConverter(type).ConvertFromInvariantString(value);
                }
                catch (Exception ex)
                {
                    if (raiseException) throw new Exception("读取配置失败，键：" + key, ex);
                    else return defaultValue;
                }
            }
        }
    }
}
