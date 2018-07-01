using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Com.BitsQuan.Option.Core
{ 

    public class ContractExecuteRecord : IEntityWithId
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Contract")]
        public int ContractId { get; set; }
        public virtual Contract Contract { get; set; }
        [ForeignKey("Trader")]
        public int TraderId { get; set; }
        public virtual Trader Trader { get; set; }
        public PositionType PosType { get; set; }
        public int Count { get; set; }
        public decimal BasePrice { get; set; }
        public bool IsAddTo { get; set; }
        public decimal Total { get; set; }
        public DateTime When { get; set; }
    }
}
