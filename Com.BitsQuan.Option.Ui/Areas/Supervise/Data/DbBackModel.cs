using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Match.Imp;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Data
{
    public class DbBackModel : IDbBackModel, IDisposable
    {
        ApplicationDbContext db = new ApplicationDbContext();
        OptionDbCtx dbc = new OptionDbCtx();

        BaseRepository<Help> br = new BaseRepository<Help>();
        BaseRepository<ForumHost> fh = new BaseRepository<ForumHost>();
        BaseRepository<ForumReply> fr = new BaseRepository<ForumReply>();
        BaseRepository<ApplicationUser> au = new BaseRepository<ApplicationUser>();
        BaseRepository<BigType> bt = new BaseRepository<BigType>();
        BaseRepository<SmallType> st = new BaseRepository<SmallType>();
        BaseRepository<SiteParameter> sp = new BaseRepository<SiteParameter>();
        BaseRepository<BankRecord> sr = new BaseRepository<BankRecord>();
        BaseRepository<CurrenAddress> cad = new BaseRepository<CurrenAddress>();
        BaseRepository<BankAccount> bk = new BaseRepository<BankAccount>();

        public void Dispose()
        {
            if (db != null) db.Dispose();
            if (dbc != null) dbc.Dispose();
        }
        public int AddHelper(Help h)
        {
            db.Helps.Add(h);
            int i = db.SaveChanges();
            return i;
        }
        public bool AddFriendlyLinks(FriendlyLink f)
        {
            bool r = true;
            db.FriendlyLinks.Add(f);
            int i = db.SaveChanges();
            if (i <= 0)
            {
                r = false;
            }
            return r;
        }
        public bool UpdateHelper(Help h, List<string> list)
        {
            br.SetCtx(db);
            return br.UpdateWithChanged(h, list);
        }
        //查询
        public IEnumerable<Help> Helps
        {
            get
            {
                Dictionary<int, Help> cond = new Dictionary<int, Help>();
                using (var db = new ApplicationDbContext())
                {

                    foreach (var v in db.Set<Help>())
                        cond.Add(v.Id, new Help
                        {
                            Id = v.Id,
                            Htitle = v.Htitle,
                            Hcontent = v.Hcontent,
                            Hdate = v.Hdate,
                            Hperson = v.Hperson,
                            HforeShow = v.HforeShow,
                            HlastDate = v.HlastDate,
                            imgSrc = v.imgSrc,
                            type = v.type
                        });
                }
                return cond.Values.ToList();
            }
        }
        //删除指定帮助
        public bool DeleteHelper(Help h)
        {
            br.SetCtx(db);
            return br.Delete(h);
        }
        void ExecuteSql(string sql)
        {
            var ds = new BackDBServer();
            ds.ExecNonQuery(sql);
            ds.Dispose();
            ds = null;

        }
        public void Flush()
        {
            throw new NotImplementedException();
        }
        OperationResult HandleError(Func<OperationResult> f)
        {
            try
            {
                return f();
            }
            catch (Exception e)
            {
                Singleton<TextLog>.Instance.Error(e);
                return new OperationResult(900, "服务器内部错误:" + e.Message);
            }
        }
        public IEnumerable<Coin> Coins
        {
            get
            {
                Dictionary<int, Coin> cond = new Dictionary<int, Coin>();
                using (var db = new OptionDbCtx())
                {

                    foreach (var v in db.Set<Coin>())
                        cond.Add(v.Id, new Coin
                        {
                            Id = v.Id,
                            Name = v.Name,
                            CotractCode = v.CotractCode,
                            MainBailRatio = v.MainBailRatio,
                            MainBailTimes = v.MainBailTimes
                        });
                }
                return cond.Values.ToList();
            }
        }
        void ExecuteSql2(string sql)
        {
            var ds = new DBServer();
            ds.ExecNonQuery(sql);
            ds.Dispose();
            ds = null;
        }
        //添加货币
        public OperationResult AddCoin(Coin c)
        {
            return HandleError(() =>
            {
                Task.Factory.StartNew(() =>
                {
                    string f = @"insert Coins values({0},'{1}',{2},{3},{4})";
                    var sql = string.Format(f, c.Id, c.Name, c.MainBailRatio, c.MainBailTimes, c.CotractCode);
                    ExecuteSql2(sql);
                });
                return OperationResult.SuccessResult;
            });
        }
        //修改货币信息
        public OperationResult UpdateCoin(Coin c)
        {
            return HandleError(() =>
            {
                Task.Factory.StartNew(() =>
                {
                    string f = @"update Coins set Name='{0}',MainBailRatio={1},MainBailTimes={2},CotractCode={3} where Id={4}";
                    var sql = string.Format(f, c.Name, c.MainBailRatio, c.MainBailTimes, c.CotractCode, c.Id);
                    ExecuteSql2(sql);
                });
                return OperationResult.SuccessResult;
            });
        }
        //删除指定货币
        public OperationResult DeleteCoin(int id)
        {
            return HandleError(() =>
            {
                Task.Factory.StartNew(() =>
                {
                    string f = @"delete Coins where Id={0}";
                    var sql = string.Format(f, id);
                    ExecuteSql2(sql);
                });
                return OperationResult.SuccessResult;
            });

        }

        //查询主贴
        public IEnumerable<ForumHost> ForumHosts
        {
            get
            {
                Dictionary<int, ForumHost> cond = new Dictionary<int, ForumHost>();
                using (var db = new ApplicationDbContext())
                {

                    foreach (var v in db.Set<ForumHost>())
                        cond.Add(v.Id, new ForumHost
                        {
                            Id = v.Id,
                            Fcontent = v.Fcontent,
                            Fname = v.Fname,
                            FDate = v.FDate,
                            FuserName = v.FuserName,
                            Smalltype = v.Smalltype,
                            Bigtype = v.Bigtype,
                            replyCount = v.replyCount

                        });
                }
                return cond.Values.ToList();
            }

        }
        //查询回帖
        public IEnumerable<ForumReply> ForumReplys
        {
            get
            {
                Dictionary<int, ForumReply> cond = new Dictionary<int, ForumReply>();
                using (var db = new ApplicationDbContext())
                {

                    foreach (var v in db.Set<ForumReply>())
                        cond.Add(v.Id, new ForumReply
                        {
                            Id = v.Id,
                            Fid = v.Fid,
                            Rcontent = v.Rcontent,
                            RDate = v.RDate,
                            Uid = v.Uid,
                            Smalltype = v.Smalltype,
                            Bigtype = v.Bigtype
                        });
                }
                return cond.Values.ToList();
            }
        }
        //删除回帖
        public bool DeleteFReplys(ForumReply f)
        {
            fr.SetCtx(db);
            return fr.Delete(f);
        }
        //删除主贴
        public bool DeleteFHosts(ForumHost f)
        {
            fh.SetCtx(db);
            return fh.Delete(f);
        }
        //添加回帖
        public bool AddReply(ForumReply f)
        {
            fr.SetCtx(db);
            return fr.Add(f);
        }
        //添加主贴
        public bool AddHost(ForumHost f)
        {
            fh.SetCtx(db);
            return fh.Add(f);
        }

        public bool UpdateUser(ApplicationUser u)
        {
            au.SetCtx(db);
            return au.Update(u);
        }

        public IEnumerable<BigType> BigTypes
        {
            get
            {
                Dictionary<int, BigType> conn = new Dictionary<int, BigType>();
                using (var db = new ApplicationDbContext())
                {
                    foreach (var item in db.Set<BigType>())
                    {
                        conn.Add(item.Id, new BigType
                        {
                            Id = item.Id,
                            Name = item.Name
                        });
                    }
                }
                return conn.Values.ToList();
            }
        }

        public IEnumerable<SmallType> SmallTypes
        {
            get
            {
                Dictionary<int, SmallType> sconn = new Dictionary<int, SmallType>();
                using (var dba = new ApplicationDbContext())
                {

                    foreach (var vs in dba.Set<SmallType>())
                        sconn.Add(vs.Id, new SmallType
                        {
                            Id = vs.Id,
                            Name = vs.Name,
                            BigType = vs.BigType
                        });
                }
                return sconn.Values.ToList();
            }
        }


        public bool AddBigType(BigType h)
        {
            bt.SetCtx(db);
            return bt.Add(h);
        }

        public bool AddSmaillType(SmallType h)
        {
            st.SetCtx(db);
            return st.Add(h);
        }

        public bool DeleteBigType(BigType h)
        {
            bt.SetCtx(db);
            return bt.Delete(h);
        }

        public bool DeleteSmaillType(SmallType h)
        {
            st.SetCtx(db);
            return st.Delete(h);
        }


        public bool updateHost(ForumHost f)
        {
            fh.SetCtx(db);
            return fh.Update(f);
        }

        public bool UpdateSite(SiteParameter p)
        {
            sp.SetCtx(db);
            return sp.Update(p);
        }


        public IEnumerable<SiteParameter> SiteParameters
        {
            get
            {
                Dictionary<int, SiteParameter> conn = new Dictionary<int, SiteParameter>();
                using (var db = new ApplicationDbContext())
                {
                    foreach (var item in db.Set<SiteParameter>())
                    {
                        conn.Add(item.Id, new SiteParameter
                        {
                            Id = item.Id,
                            Copyright = item.Copyright,
                            Describe = item.Describe,
                            Keyword = item.Keyword,
                            SiteName = item.SiteName,
                            SiteUrl = item.SiteUrl,
                            OpenSite = item.OpenSite,
                            siteState = item.siteState,
                            userRegiste = item.userRegiste,
                            trader = item.trader,
                            tradePwd = item.tradePwd,
                            outTime = item.outTime,
                            emailUserPwd = item.emailUserPwd,
                            emailUserName = item.emailUserName,
                            emaiSendName = item.emaiSendName,
                            SendEmail = item.SendEmail,
                            sendFreezeEmail = item.sendFreezeEmail,
                            sendRecoverEmail = item.sendRecoverEmail,
                            sendZhuanzEmail = item.sendZhuanzEmail,
                            sendWithdrawEmail = item.sendWithdrawEmail,
                            sendreChargeEmail = item.sendreChargeEmail,
                            sendFreezeMsg = item.sendFreezeMsg,
                            sendThawMsg = item.sendThawMsg,
                            sendZhuanzMsg = item.sendZhuanzMsg,
                            sendWithdrawMsg = item.sendWithdrawMsg,
                            sendreChargeMsg = item.sendreChargeMsg,
                            BankZhuanz = item.BankZhuanz,
                            OnlinePayment = item.OnlinePayment
                        });
                    }
                }
                return conn.Values.ToList();
            }
        }

        //删除充值提现记录
        public bool DeleteBankRecords(BankRecord b)
        {
            sr.SetCtx(db);
            return sr.Delete(b);


        }


        //修改提现地址（之前总是报错，是因为引用了用户表的外键对象）
        public bool UpdCurrenAddress(CurrenAddress ca)
        {
            cad.SetCtx(db);
            return cad.Update(ca);

        }




        public bool UpdateBigType(BigType h)
        {
            bt.SetCtx(db);
            return bt.Update(h);
        }


        public bool UpdateSmallType(SmallType h)
        {

            st.SetCtx(db);
            return st.Update(h);
        }


        public bool UpdBank(BankAccount ca)
        {
            bk.SetCtx(db);
            return bk.Update(ca);
        }
    }
}