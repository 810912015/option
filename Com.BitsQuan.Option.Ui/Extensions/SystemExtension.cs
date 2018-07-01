using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Extensions
{
    public static class SystemExtension
    {
        /// <summary>
        /// 把某区间内的的所有字符替换为指定字符
        /// </summary>
        /// <param name="self"></param>
        /// <param name="startIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="substitude"></param>
        /// <returns></returns>
        public static string Replace(this string self, int startIndex, int endIndex, char substitude)
        {
            if (String.IsNullOrEmpty(self)) return "";

            endIndex = endIndex < 0 ? startIndex : endIndex;
            endIndex = endIndex >= self.Length ? self.Length - 1 : endIndex;
            if (startIndex > endIndex) startIndex = endIndex;

            char[] arr = self.ToCharArray();
            for (int i = startIndex; i <= endIndex; ++i)
            {
                arr[i] = substitude;
            }
            return new string(arr);
        }
    }
}