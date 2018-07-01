using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Com.BitsQuan.Option.Match.Dto
{  
     [DataContract]
     [KnownType(typeof(OrderDto))]
     public class D2Model
     {
         [DataMember(Name = "Sell")]
         /// <summary>
         /// 卖
         /// </summary>
         public List<OrderDto> D1 { get; set; }
         [DataMember(Name = "Buy")]
         /// <summary>
         /// 买
         /// </summary>
         public List<OrderDto> D2 { get; set; }
         [DataMember(Name = "SellCount")]
         public int D1Count { get; set; }
         [DataMember(Name = "BuyCount")]
         public int D2Count { get; set; }
         public int Count { get; private set; }
         public D2Model(List<Order> d1, List<Order> d2, int count = 8)
         {
             this.Count = count;
             D1 = new List<OrderDto>(); D2 = new List<OrderDto>();
             Merge(D1, d1); Merge(D2, d2);
             if (D1.Count > Count) D1 = D1.Skip(D1.Count - Count).Take(Count).ToList();
             if (D2.Count > Count) D2 = D2.OrderByDescending(a => a.Price).Take(Count).ToList();
             D1Count = D1.Select(a => a.Count).Sum();
             D2Count = D2.Select(a => a.Count).Sum();
         }
         void Merge(List<OrderDto> odl, List<Order> ol)
         {
             if (ol == null) return;
             for (int i = 0; i < ol.Count; i++)
             {
                 if (i == 0)
                 {
                     var dt = new OrderDto(ol[i]);
                     odl.Add(dt);
                     dt.Count = ol[i].ReportCount - ol[i].TotalDoneCount;
                 }
                 else
                 {
                     var c = odl[odl.Count - 1];
                     if (ol[i].Price == c.Price)
                         c.Count += ol[i].ReportCount - ol[i].TotalDoneCount;
                     else
                     {
                         var dt = new OrderDto(ol[i]);
                         odl.Add(dt);
                         dt.Count = ol[i].ReportCount - ol[i].TotalDoneCount;
                     }
                 }
             }
         }
     }

     public class SpotD2Model
     {
         public List<SpotOrderDto> D1 { get; set; }
         public List<SpotOrderDto> D2 { get; set; }
         public decimal D1Count { get; set; }
         public decimal D2Count { get; set; }
         public int Count { get; private set; }
         public SpotD2Model(List<SpotOrder> d1, List<SpotOrder> d2, int count = 8)
         {
             this.Count = count;
             D1 = new List<SpotOrderDto>(); D2 = new List<SpotOrderDto>();
             Merge(D1, d1); Merge(D2, d2);
             if (D1.Count > Count) D1 = D1.Take(Count).ToList();
             if (D2.Count > Count) D2 = D2.OrderByDescending(a => a.Price).Take(Count).ToList();
             D1Count = D1.Select(a => a.Count).Sum();
             D2Count = D2.Select(a => a.Count).Sum();
         }
         void Merge(List<SpotOrderDto> odl, List<SpotOrder> ol)
         {
             if (ol == null) return;
             for (int i = 0; i < ol.Count; i++)
             {
                 if (i == 0)
                 {
                     var dt = new SpotOrderDto(ol[i]);
                     odl.Add(dt);
                     dt.Count = ol[i].ReportCount - ol[i].TotalDoneCount;
                 }
                 else
                 {
                     var c = odl[odl.Count - 1];
                     if (ol[i].Price == c.Price)
                         c.Count += ol[i].ReportCount - ol[i].TotalDoneCount;
                     else
                     {
                         var dt = new SpotOrderDto(ol[i]);
                         odl.Add(dt);
                         dt.Count = ol[i].ReportCount - ol[i].TotalDoneCount;
                     }
                 }
             }
         }
     }
}