using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Models
{
    public class Pager
    {
        /// <summary>
        /// 总页数
        /// </summary>
        public int PageCount { get; set; }
        /// <summary>
        /// 当前页
        /// </summary>
        public int PageIndex { get; set; }
        /// <summary>
        /// 返回url:当前是actionName
        /// </summary>
        public string PostAction { get; set; }
        /// <summary>
        /// 返回要替换的id
        /// </summary>
        public string TargetId { get; set; }

        private Object _pagerParam;

        public Pager()
        {
            SetPagerParam(new DefaultPagerParam());
        }
        public T GetPagerParam<T>(T param) where T : PagerUrlParam
        {
            return (T)_pagerParam;
        }
        public Pager SetPagerParam<T>(T param) where T : PagerUrlParam
        {
            _pagerParam = param;
            return this;
        }

        public PagerUrlParam ClonePagerParam(int pageIndex)
        {
            return ((PagerUrlParam)_pagerParam).Clone().SetPageIndex(pageIndex);
        }
    }

    /// <summary>
    /// 分页url参数基类。自定义参数需继承这个抽象类
    /// </summary>
    public abstract class PagerUrlParam
    {
        protected String EncodeUri(String input)
        {
            return Microsoft.JScript.GlobalObject.encodeURIComponent(input);
        }
        private int _pageIndex;
        public int GetPageIndex()
        {
            return _pageIndex;
        }
        public PagerUrlParam SetPageIndex(int pageIndex)
        {
            _pageIndex = pageIndex;
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>返回一个匿名类对象</returns>
        public abstract Object GetParams();

        public abstract PagerUrlParam Clone();
    }
    public class DefaultPagerParam : PagerUrlParam
    {
        public override PagerUrlParam Clone()
        {
            return new DefaultPagerParam().SetPageIndex(GetPageIndex());
        }
        public override Object GetParams()
        {
            return new { PageIndex = GetPageIndex() };
        }
    }
}