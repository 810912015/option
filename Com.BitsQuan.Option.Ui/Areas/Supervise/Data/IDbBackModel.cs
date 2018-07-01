using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Ui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Data
{
    public interface IDbBackModel : IFlush
    {
        //查询大类型
        IEnumerable<BigType> BigTypes { get; }
        //查询小类型
        IEnumerable<SmallType> SmallTypes { get; }
        //添加大类型
        bool AddBigType(BigType h);
        //添加小类型
        bool AddSmaillType(SmallType h);
        //删除大类型
        bool DeleteBigType(BigType h);
        //删除小类型
        bool DeleteSmaillType(SmallType h);
        //修改大类型
        bool UpdateBigType(BigType h);
        //修改小类型
        bool UpdateSmallType(SmallType h);
        //修改指定用户对象
        bool UpdateUser(ApplicationUser u);
       //添加帮助对象
        int AddHelper(Help h);

        //修改帮助对象
        bool UpdateHelper(Help h, List<string> list);
        //删除对象
        bool DeleteHelper(Help h);
        //查询全部帮助
       IEnumerable<Help> Helps { get; }

        //添加货币
       OperationResult AddCoin(Coin c);
        //修改货币
       OperationResult UpdateCoin(Coin c);
        //删除货币
       OperationResult DeleteCoin(int id);
        //查询全部货币
       IEnumerable<Coin> Coins { get; }
       //查询主贴
       IEnumerable<ForumHost> ForumHosts { get; }

       //查看所有回贴
       IEnumerable<ForumReply> ForumReplys { get; }

        //删除回帖
       bool DeleteFReplys(ForumReply f);

        //删除主贴
       bool DeleteFHosts(ForumHost f);
        //添加回帖
       bool AddReply(ForumReply f);
        //添加主贴
       bool AddHost(ForumHost f);

       //修改主贴（置顶）
     //  bool DeleteFHosts(ForumHost f);
     
    //修改主贴
       bool updateHost(ForumHost f);
        //修改网站参数
        bool UpdateSite(SiteParameter p);
        //查询网站参数
        IEnumerable<SiteParameter> SiteParameters { get; }
        //删除（撤销）提现记录
        bool DeleteBankRecords(BankRecord b);
        //修改提现地址
        bool UpdCurrenAddress(CurrenAddress ca);
        //修改银行地址
        bool UpdBank(BankAccount ca);
    }
}
