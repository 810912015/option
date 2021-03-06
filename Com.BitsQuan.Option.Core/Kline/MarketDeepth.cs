﻿using System.Collections.Generic;

namespace Com.BitsQuan.Option.Core
{ 
    public class MarketDeepth
    {
        string s1 = "[[[2322.15,1.614,1.614],[2321,16.169,14.555],[2320,21.949,5.78],[2319.71,22.163,0.214],[2319.32,22.859,0.696],[2319.31,23.095,0.236],[2319.3,23.965,0.87],[2319.26,25.001,1.036],[2319.08,25.234,0.233],[2318.51,25.996,0.762],[2318.49,26.076,0.08],[2318.45,27.79,1.714],[2318.19,28.144,0.354],[2318,30.353,2.209],[2317.84,30.363,0.01],[2317.43,32.624,2.261],[2317.35,33.471,0.847],[2317,35.275,1.804],[2316.74,36.225,0.95],[2316.5,37.225,1],[2316.17,37.449,0.224],[2316.05,38.449,1],[2316,39.838,1.389],[2315.98,40.295,0.457],[2315.65,41.969,1.674],[2315.58,42.494,0.525],[2315.46,47.821,5.327],[2315.02,50.113,2.292],[2315,53.861,3.748],[2314.62,58.801,4.94],[2314.59,60.487,1.686],[2314.21,65.207,4.72],[2314.2,81.795,16.588],[2314,82.594,0.799],[2313.77,86.067,3.473],[2313.48,87.75,1.683],[2313.3,88.122,0.372],[2313.18,89.631,1.509],[2313.14,90.705,1.074],[2313.11,97.585,6.88],[2313,99.585,2],[2312.83,101.09,1.505],[2312.32,108.621,7.531],[2312,110.621,2],[2311.87,110.722,0.101],[2311.52,120.241,9.519],[2311.47,121.261,1.02],[2311.3,128.786,7.525],[2311.11,129.93,1.144],[2311.01,129.98,0.05]],[[2326.67,0.2,0.2],[2329.84,0.25,0.05],[2330,6.222,5.972],[2330.02,28.22,21.998],[2330.11,28.28,0.06],[2331.5,28.578,0.298],[2331.58,30.888,2.31],[2331.82,31.348,0.46],[2332,33.806,2.458],[2332.34,38.019,4.213],[2333.41,38.097,0.078],[2333.56,38.169,0.072],[2334.05,63.972,25.803],[2334.64,91.955,27.983],[2335.44,95.711,3.756],[2335.75,96.136,0.425],[2336,114.664,18.528],[2336.06,116.989,2.325],[2336.65,117.386,0.397],[2336.79,118.084,0.698],[2338,123.52,5.436],[2338.04,127.133,3.613],[2338.15,130.054,2.921],[2338.91,130.169,0.115],[2338.97,130.348,0.179],[2339.43,132.455,2.107],[2339.45,132.653,0.198],[2339.49,137.194,4.541],[2339.99,137.444,0.25],[2340.1,137.578,0.134],[2340.26,137.727,0.149],[2340.41,138.095,0.368],[2340.63,178.095,40],[2341.08,180.741,2.646],[2341.43,180.83,0.089],[2341.48,184.47,3.64],[2342.84,185.329,0.859],[2343.64,185.425,0.096],[2344.09,188.935,3.51],[2344.15,189.249,0.314],[2344.31,194.76,5.511],[2344.37,195.181,0.421],[2344.52,195.276,0.095],[2344.56,195.385,0.109],[2344.88,201.626,6.241],[2345,203.626,2],[2345.23,215.038,11.412],[2345.55,264.182,49.144],[2345.9,264.278,0.096],[2346.03,277.087,12.809]]]";
        List<double> Convert(string str)
        {
            var s = s1.Replace("[", "").Replace("]", "").Split(',');
            List<double> l = new List<double>();
            for (int i = 0; i < s.Length; i++)
            {
                double d = 0;
                double.TryParse(s[i], out d);
                l.Add(d);
            }
            return l;
        }
        List<List<List<double>>> Make(List<double> l)
        {
            
            List<List<List<double>>> r = new List<List<List<double>>>();
            List<List<double>> t = new List<List<double>>();
            for (int i = 0; i < l.Count; i += 3)
            {
                t.Add(new List<double> { l[i], l[i + 1], l[i + 2] });
            }
            List<List<double>> t1 = new List<List<double>>();
            List<List<double>> t2= new List<List<double>>();
            for (int i = 0; i < t.Count / 2; i++)
            {
                t1.Add(t[i]);
            }
            for (int i = t.Count / 2; i < t.Count; i++)
            {
                t2.Add(t[i]);
            }
            r.Add(t1);
            r.Add(t2);

                return r;
        }
        List<List<List<double>>> fl = null;
        public List<List<List<double>>> Fake()
        {
            if (fl == null)
            {
                var l = Convert(s1);
                var r = Make(l);
                fl = r;
            }
            return fl; 
        }
    } 
}
