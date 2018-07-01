using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.BitsQuan.Option.Core
{
    /// <summary>
    /// 蜡烛图数据
    /// </summary>
    public class Ohlc : IEntityWithId
    {
        [Key]
        public int Id { get; set; }
        public int WhatId { get; set; }
        public OhlcType OhlcType { get; set; }
        /// <summary>
        /// 时间:自1970.1.1以来的毫秒数
        /// </summary>
        public double When { get; set; }

        public DateTime WhenInDt { get; set; }
        /// <summary>
        /// 开盘
        /// </summary>
        public double Open { get; set; }
        /// <summary>
        /// 最高
        /// </summary>
        public double High { get; set; }
        /// <summary>
        /// 最低
        /// </summary>
        public double Low { get; set; }
        /// <summary>
        /// 收盘
        /// </summary>
        public double Close { get; set; }
        /// <summary>
        /// 成交量
        /// </summary>
        public double Volume { get; set; }
        public List<double> List
        {
            get
            {
                return new List<double> { When,Math.Round(Open,2), Math.Round(High,2), Math.Round(Low,2), Math.Round(Close,2), Volume };
            }
        }
        static readonly DateTime d70 = new DateTime(1970, 1, 1);
        public Ohlc() { }
        public Ohlc(DateTime when, double open, double high, double low, double close, double volume)
        {
            SetTime(when);
            this.Open = open; this.High = high; this.Low = low;
            this.Close = close; this.Volume = volume;
        }
        public void SetTime(DateTime when)
        {
            this.WhenInDt = when;
            this.When = when.Subtract(d70).TotalMilliseconds;
        }
        /// <summary>
        /// 区间合并
        /// </summary>
        /// <param name="o"></param>
        public void Calc(Ohlc o)
        {
            if (o.WhenInDt > WhenInDt)
            {
                Close = o.Close;
            }
            else if (o.WhenInDt < WhenInDt)
            {
                Open = o.Open;
            }
            if (o.High > High) High = o.High;
            if (o.Low > 0 && o.Low < Low) Low = o.Low;
            Volume += o.Volume;
        }

        public static Ohlc Fake(DateTime when)
        {
            Ohlc o = new Ohlc(when, 83 + when.Minute * 10, 99 + when.Minute * 10, 79 + when.Minute * 10, 93 + when.Minute * 10, 100);
            return o;
        }
    }
}
