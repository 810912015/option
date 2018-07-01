using System;
namespace Com.BitsQuan.Option.Match.Imp.Collection
{
    public interface IOrderPack
    {
        bool Add(global::Com.BitsQuan.Option.Core.Order o);
        int Count { get; }
        int FirstCount { get; }
        global::Com.BitsQuan.Option.Core.Order FirstOrder { get; }
        decimal FirstPrice { get; }
        bool IsSell { get; }
        global::System.Collections.Generic.List<global::Com.BitsQuan.Option.Core.Order> Items { get; }
        bool Remove(global::Com.BitsQuan.Option.Core.Order o);
    }
}
