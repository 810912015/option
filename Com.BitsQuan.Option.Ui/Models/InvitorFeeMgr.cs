using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Models
{
    public class InvitorFeeMgr
    {
        /// <summary>
        /// 获取推荐人关系
        /// </summary>
        /// <returns></returns>
        public Dictionary<ApplicationUser, List<ApplicationUser>> Init()
        {
            Dictionary<ApplicationUser, List<ApplicationUser>> relations = new Dictionary<ApplicationUser, List<ApplicationUser>>();
            List<ApplicationUser> users;
            using (ApplicationDbContext db = new ApplicationDbContext())
            {
                users = db.Users.ToList();
            }
            foreach (var v in users)
            {
                var q = users.Where(a => a.InvitorId != null && a.InvitorId == v.Id);
                if (q == null||q.Count()==0) continue;
                var ql = q.ToList();
                if (!relations.ContainsKey(v))
                {
                    relations.Add(v, ql);
                }
            }
            return relations;
        }
        /// <summary>
        /// 奖金划转
        /// </summary>
        /// <param name="t">推荐人</param>
        /// <param name="invitedNames">被推荐人名字</param>
        public decimal TransBonus(Trader t, List<string> invitedNames)
        {
            return InvitorFeeService.TransBonus(t, invitedNames);
            
        }
        /// <summary>
        /// 手续费划转:执行从最近一次转账时间以来的手续费划转
        /// </summary>
        public void TransferFee()
        {
            var relations = Init();
            Dictionary<string, decimal> dic = new Dictionary<string, decimal>();
            foreach (var v in relations)
            {
                if (v.Value.Count == 0) continue;

                var to = MvcApplication.OptionService.Model.Traders.Where(a => a.Name == v.Key.UserName).FirstOrDefault();
                if (to == null) continue;
                var slaves = v.Value.Select(u =>
                    new InvitorFeeTrans(MvcApplication.OptionService.Model.Traders.Where(a => a.Name == u.UserName).FirstOrDefault(),u.InvitorFeeRatio,
                        u.LastTransferFeeTime,(d)=>{
                            if (!dic.ContainsKey(u.UserName))
                            {
                                dic.Add(u.UserName, d);
                            }
                            else dic[u.UserName] += d;
                        }))
                    .ToList();
                InvitorFeeService.TransFee(slaves, to);
                SaveFeeDelta(dic);
            }
        }
        void SaveFeeDelta(Dictionary<string, decimal> dic)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var v in dic)
                sb.AppendFormat("update AspNetUsers set InviteFeeSum =InviteFeeSum+{0},LastTransferFeeTime =GETDATE() where UserName ='{1}' ", v.Value, v.Key);
            if (sb.Length == 0) return;
            using (ApplicationDbContext db=new ApplicationDbContext ())
            {
                db.Database.SqlQuery<int>(sb.ToString());
            }
        }

    }
}