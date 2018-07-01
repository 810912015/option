using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Imp;
using Com.BitsQuan.Option.Ui.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.BitsQuan.Option.Ui.Areas.Supervise.Data
{
    public class SuperviseModel:ISuperviseModel
    {
        List<Help> helps;
        public IEnumerable<Help> Helps
        {
            get { return helps; }
        }
        public IDbBackModel DbModel { get; private set; }
        public SuperviseModel() {
            DbModel = new DbBackModel();
        }


        /// <summary>
        /// 从数据库读出数据至内存中
        /// </summary>
        void ReadDb()
        {

            Dictionary<int, Help> cond = new Dictionary<int, Help>();
            using (var db = new ApplicationDbContext())
            {

                foreach (var v in db.Set<Help>())
                    cond.Add(v.Id, new Help
                    {
                         Id = v.Id,
                         Htitle=v.Htitle,
                         Hcontent=v.Hcontent,
                         Hdate=v.Hdate,
                         Hperson=v.Hperson,
                         HforeShow=v.HforeShow,
                         HlastDate=v.HlastDate
                    });
            }

                helps = cond.Values.ToList();
        }
        public void Init()
        {
            ReadDb();
        }



        public void Flush()
        {
            throw new NotImplementedException();
        }

        public void AddHelper(Help h)
        {
              helps.Add(h);
              DbModel.AddHelper(h);
             
            
        }
        public void UpdateHelper(Help h)
        {
            //DbModel.UpdateHelper(h);
            ////修改后替换内存中的
            //for (int i = 0; i < helps.Count; i++)
            //{
            //    if (helps[i].Id == h.Id) {
            //        helps[i].Htitle = h.Htitle;
            //        helps[i].Hcontent = h.Hcontent;
            //        helps[i].HlastDate = h.HlastDate;
            //        helps[i].HforeShow = h.HforeShow;
            //    }
            //}
           
        }
    }
}