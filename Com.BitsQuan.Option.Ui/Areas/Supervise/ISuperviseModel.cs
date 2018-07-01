using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Data;
using Com.BitsQuan.Option.Ui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise
{

    public interface ISuperviseModel : IInitialbe, IFlush
    {
        IEnumerable<Help> Helps { get; }
        IDbBackModel DbModel { get; }
        //添加帮助
        void AddHelper(Help h);
        //修改帮助
        void UpdateHelper(Help h);
    }
}