using Com.BitsQuan.Option.Core;
using Com.BitsQuan.Option.Core.Spot;
using Com.BitsQuan.Option.Provider;
using Com.BitsQuan.Option.Ui.Areas.Supervise.Controllers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;

namespace Com.BitsQuan.Option.Ui.Models.Query
{
    /// <summary>
    /// 代表一个能查询的列的元数据:如列名字串,类型等
    /// </summary>
    public class QueryArgsCol
    {
        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 列名称:必须与类的属性名称字串完全相符
        ///        如果嵌套,则用propertyName.propertyName的表示形式
        /// </summary>
        public string ColName { get; set; }
        [JsonIgnore]
        private Type t; public string TypeStr { get; set; }
        [JsonIgnore]
        /// <summary>
        /// 列类型
        /// </summary>
        public Type ValueType
        {
            get { return t; }
            set
            {
                t = value;
                if (value == typeof(int) || t == typeof(decimal) || t == typeof(double) || t == typeof(float) ||
                    value == typeof(int?) || t == typeof(decimal?) || t == typeof(double?) || t == typeof(float?))
                    TypeStr = "number";
                else if (value == typeof(DateTime) || value == typeof(DateTime?))
                    TypeStr = "datetime";
                else if (value.IsEnum) TypeStr = "enum";
                else TypeStr = "text";
            }
        }


        /// <summary>
        /// 为此列生成下拉框所用的数据
        /// </summary>
        public List<SelectListItem> ListItems { get; set; }
        /// <summary>
        /// 为一个类指定能查询的列的元数据列表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<QueryArgsCol> GetCols<T>()
        {
            #region application user
            if (typeof(T) == typeof(ApplicationUser))
            {
                var Cols = new List<QueryArgsCol>
                {
                      new QueryArgsCol
                      { 
                          DisplayName="用户名",
                          ValueType=typeof (string),
                          ColName="UserName"
                      } ,
                     new QueryArgsCol
                      { 
                          DisplayName="身份证",
                          ValueType=typeof (string),
                          ColName="IdNumber"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="注册时间",
                          ValueType=typeof (DateTime?),
                          ColName="RegisterTime"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="证件类型",
                          ValueType=typeof (string),
                          ColName="IdNumberType"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="认证时间",
                          ValueType=typeof (string),
                          ColName="IdentiTime"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="最后一次错误时间",
                          ValueType=typeof (DateTime?),
                          ColName="EnderrorTime"
                      }  
                };
                return Cols;
            }
            #endregion

            #region user op log
            if (typeof(T) == typeof(UserOpLog))
            {
                var Cols = new List<QueryArgsCol>
                {
                     new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="用户",
                          ValueType=typeof (string),
                          ColName="Name"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="Ip",
                          ValueType=typeof (string),
                          ColName="Ip"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="控制器",
                          ValueType=typeof (string),
                          ColName="Controller"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="操作",
                          ValueType=typeof (string),
                          ColName="Action"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="时间",
                          ValueType=typeof (DateTime),
                          ColName="When"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="参数",
                          ValueType=typeof (string),
                          ColName="Prms"
                      } ,
                };
                return Cols;
            }
            #endregion


            #region SpotOrder
            if (typeof(T) == typeof(SpotOrder))
            {
                var Cols = new List<QueryArgsCol>
                {
                     new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="用户",
                          ValueType=typeof (string),
                          ColName="Trader.Name"
                      } ,
                       
                       new QueryArgsCol
                      { 
                          DisplayName="买卖",
                          ValueType=typeof (TradeDirectType),
                          ColName="Direction",
                          ListItems=new List<SelectListItem> {
                              new SelectListItem{ Text="买",Value="1"},
                              new SelectListItem{ Text="卖",Value="2"},
                          }

                      } ,
                         new QueryArgsCol
                      { 
                          DisplayName="币种",
                          ValueType=typeof (string),
                          ColName="Coin.Name"
                      } ,
                       
                      new QueryArgsCol
                      { 
                          DisplayName="价格",
                          ValueType=typeof (decimal),
                          ColName="Price"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="数量",
                          ValueType=typeof (int),
                          ColName="Count"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="成交数量",
                          ValueType=typeof (int),
                          ColName="DoneCount"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="委托时间",
                          ValueType=typeof (DateTime),
                          ColName="OrderTime"
                      } ,

                       new QueryArgsCol
                      { 
                          DisplayName="挂单状态",
                          ValueType=typeof (OrderRequestStatus),
                          ColName="RequestStatus",
                          ListItems=new List<SelectListItem> {
                              new SelectListItem{ Text="挂单成功",Value="1"},
                              new SelectListItem{ Text="挂单失败",Value="2"},
                          }

                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="当前状态",
                          ValueType=typeof (OrderState),
                          ColName="State",
                          ListItems=new List<SelectListItem> {
                              new SelectListItem{ Text="等待中",Value="1"},
                              new SelectListItem{ Text="已撤销",Value="2"},
                              new SelectListItem{ Text="已成交",Value="3"},
                              new SelectListItem{ Text="部分成交",Value="4"},
                              new SelectListItem{ Text="已行权",Value="5"}
                          }

                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="系统单",
                          ValueType=typeof (bool),
                          ColName="IsBySystem"
                      } ,
                };
                return Cols;
            }
            #endregion


            #region SpotDeal
            if (typeof(T) == typeof(SpotDeal))
            {
                var Cols = new List<QueryArgsCol>
                {
                     new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="币种",
                          ValueType=typeof (string),
                          ColName="Coin.Name"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="主委托人",
                          ValueType=typeof (string),
                          ColName="MainTraderName"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="主委托编号",
                          ValueType=typeof (int),
                          ColName="MainId"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="数量",
                          ValueType=typeof (decimal),
                          ColName="Count"
                      } ,
                       
                       new QueryArgsCol
                      { 
                          DisplayName="价格",
                          ValueType=typeof (decimal),
                          ColName="Price"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="从委托人",
                          ValueType=typeof (string),
                          ColName="SlaveTraderId"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="从委托编号",
                          ValueType=typeof (int),
                          ColName="SlaveId"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="成交时间",
                          ValueType=typeof (DateTime),
                          ColName="When"
                      } ,

                };
                return Cols;
            }
            #endregion


            #region ContractExecuteRecord
            if (typeof(T) == typeof(ContractExecuteRecord))
            {
                var Cols = new List<QueryArgsCol>
                {
                     new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="用户",
                          ValueType=typeof (string),
                          ColName="Trader.Name"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="合约",
                          ValueType=typeof (string),
                          ColName="Contract.Name"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="持仓数量",
                          ValueType=typeof (int),
                          ColName="Count"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="执行时间",
                          ValueType=typeof (DateTime),
                          ColName="When"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="持仓类型",
                          ValueType=typeof (PositionType),
                          ColName="PosType",
                           ListItems=new List<SelectListItem> {
                              new SelectListItem{ Text="权利仓",Value="1"},
                              new SelectListItem{ Text="义务仓",Value="2"},
                          }
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="收钱付钱",
                          ValueType=typeof (bool),
                          ColName="IsAddTo"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="实际金额",
                          ValueType=typeof (decimal),
                          ColName="Total"
                      } ,

                };
                return Cols;
            }
            #endregion

            #region userposition
            if (typeof(T) == typeof(UserPosition))
            {
                var Cols = new List<QueryArgsCol>
                {
                     new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="用户",
                          ValueType=typeof (string),
                          ColName="Trader.Name"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="合约",
                          ValueType=typeof (string),
                          ColName="Order.Contract.Code"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="数量",
                          ValueType=typeof (int),
                          ColName="Count"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="时间",
                          ValueType=typeof (DateTime),
                          ColName="DealTime"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="委托编号",
                          ValueType=typeof (int),
                          ColName="OrderId"
                      } ,
                };
                return Cols;
            }
            #endregion

            #region PositionSummaryDatas
            if (typeof(T) == typeof(PositionSummaryData))
            {
                var Cols = new List<QueryArgsCol>
                {
                     new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      }  ,
                        new QueryArgsCol
                      { 
                          DisplayName="合约名称",
                          ValueType=typeof (string),
                          ColName="Contract.Name"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="合约代码",
                          ValueType=typeof (string),
                          ColName="Contract.Code"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="持仓类型",
                          ValueType=typeof (string),
                          ColName="PositionType"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="数量",
                          ValueType=typeof (decimal),
                          ColName="Count"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="平仓数量",
                          ValueType=typeof (DateTime),
                          ColName="ClosableCount"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="买的数量",
                          ValueType=typeof (decimal),
                          ColName="BuyTotal"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="浮动盈亏",
                          ValueType=typeof (decimal),
                          ColName="FloatProfit"
                      }  ,
                        new QueryArgsCol
                      { 
                          DisplayName="平仓盈亏",
                          ValueType=typeof (decimal),
                          ColName="CloseProfit"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="保证率",
                          ValueType=typeof (decimal),
                          ColName="Maintain"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="下单数量",
                          ValueType=typeof (decimal),
                          ColName="TotalValue"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="下单类型",
                          ValueType=typeof (int),
                          ColName="OrderType"
                      } ,

                       new QueryArgsCol
                      { 
                          DisplayName="交易用户名",
                          ValueType=typeof (string),
                          ColName="Trader.Name"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="时间",
                          ValueType=typeof (DateTime),
                          ColName="When"
                      } ,
                };
                return Cols;
            }
            #endregion

            #region SysAccountRecord
            if (typeof(T) == typeof(SysAccountRecord))
            {
                var Cols = new List<QueryArgsCol>
                {
                     new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="用户",
                          ValueType=typeof (string),
                          ColName="Who.Name"
                      } ,
                     
                      new QueryArgsCol
                      { 
                          DisplayName="金额",
                          ValueType=typeof (decimal),
                          ColName="Delta"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="私有总额",
                          ValueType=typeof (decimal),
                          ColName="PrivateSum"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="公共总额",
                          ValueType=typeof (decimal),
                          ColName="PublicSum"
                      } ,
                      
                       new QueryArgsCol
                      { 
                          DisplayName="发生时间",
                          ValueType=typeof (DateTime),
                          ColName="When"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="类型",
                          ValueType=typeof (SysAccountChangeType),
                          ColName="ChangedType",
                          ListItems = Enum.GetValues(typeof(SysAccountChangeType))
                            .Cast<SysAccountChangeType>()
                            .Select(_=>new SelectListItem{Text=_.ToString(),Value=((int)_).ToString()})
                            .ToList(),

                      } ,
                };
                return Cols;
            }
            #endregion


            #region bankrecord
            if (typeof(T) == typeof(BankRecord))
            {
                var cols = new List<QueryArgsCol>
                {
                       new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="银行",
                          ValueType=typeof (string),
                          ColName="v.Account.BankName"
                      } ,
                        
                        new QueryArgsCol
                      { 
                          DisplayName="账号",
                          ValueType=typeof (string),
                          ColName="v.Account.Number"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="开户省",
                          ValueType=typeof (string),
                          ColName="v.Account.Province"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="开户市",
                          ValueType=typeof (string),
                          ColName="v.Account.City"
                      } ,

                        new QueryArgsCol
                      { 
                          DisplayName="开户名",
                          ValueType=typeof (string),
                          ColName="Account.Name"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="用户",
                          ValueType=typeof (string),
                          ColName="v.Account.User.UserName"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="用户UID",
                          ValueType=typeof (string),
                          ColName="Uid"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="开户省",
                          ValueType=typeof (string),
                          ColName="v.Account.Province"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="操作类型",
                          ValueType=typeof (BankRecordType),
                          ColName="v.BankRecordType",
                           ListItems=new List<SelectListItem> {
                               new SelectListItem{ Text="充值",Value="1"},
                               new SelectListItem{ Text="提现",Value="2"}
                           }
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="金额",
                          ValueType=typeof (decimal),
                          ColName="Delta"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="请求时间",
                          ValueType=typeof (DateTime),
                          ColName="When"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="实际金额",
                          ValueType=typeof (decimal),
                          ColName="ActualDelta"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="办理时间",
                          ValueType=typeof (DateTime),
                          ColName="AuditTime"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="是否已办理",
                          ValueType=typeof (bool),
                          ColName="IsApproved"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="办理结果",
                          ValueType=typeof (bool?),
                          ColName="ApprovedResult"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="结果描述",
                          ValueType=typeof (string ),
                          ColName="ApproveDesc"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="经手人",
                          ValueType=typeof (string),
                          ColName="ByWho"
                      } ,
                      
                };
                return cols;
            }
            #endregion

            #region blastoperation
            if (typeof(T) == typeof(TraderMsg))
            {
                var cols = new List<QueryArgsCol>
                {
                       new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="用户",
                          ValueType=typeof (string),
                          ColName="Name"
                      } ,
                        
                        new QueryArgsCol
                      { 
                          DisplayName="类型",
                          ValueType=typeof (string),
                          ColName="MsgType"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="时间",
                          ValueType=typeof (DateTime),
                          ColName="When"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="内容",
                          ValueType=typeof (string),
                          ColName="Msg"
                      } 
                      
                };
                return cols;
            }
            #endregion
            #region blastoperation
            if (typeof(T) == typeof(BlasterOperaton))
            {
                var cols = new List<QueryArgsCol>
                {
                       new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="爆仓编号",
                          ValueType=typeof (int),
                          ColName="BlasterRecordId"
                      } ,
                        
                        new QueryArgsCol
                      { 
                          DisplayName="持仓编号",
                          ValueType=typeof (int),
                          ColName="PositionId"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="委托时间",
                          ValueType=typeof (DateTime),
                          ColName="Order.OrderTime"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="合约名称",
                          ValueType=typeof (string),
                          ColName="Order.Contract.Name"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="委托编号",
                          ValueType=typeof (int),
                          ColName="OpOrderId"
                      }  
                      
                };
                return cols;
            }
            #endregion

            #region fuserecord
            if (typeof(T) == typeof(FuseRecord))
            {
                var cols = new List<QueryArgsCol>
                {
                       new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="合约代码",
                          ValueType=typeof (string),
                          ColName="Contract.Code"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="合约名称",
                          ValueType=typeof (string),
                          ColName="Contract.Name"
                      } ,
                        
                        new QueryArgsCol
                      { 
                          DisplayName="熔断价格",
                          ValueType=typeof (decimal),
                          ColName="Price"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="开始时间",
                          ValueType=typeof (DateTime),
                          ColName="StartTime"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="熔断类型",
                          ValueType=typeof (FuseType),
                          ColName="FuseType",
                          ListItems=new List<SelectListItem> {
                              new SelectListItem{ Text="上涨熔断",Value="1"},
                              new SelectListItem{Text="下跌熔断",Value="2"}
}
                      } ,
                };
                return cols;
            }
            #endregion


            #region blast
            if (typeof(T) == typeof(BlastRecord))
            {
                var cols = new List<QueryArgsCol>
                {
                       new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="ContractId"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="用户名称",
                          ValueType=typeof (string),
                          ColName="Trader.Name"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="当前保证金金额",
                          ValueType=typeof (decimal),
                          ColName="BailTotal"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="需要的保证金金额",
                          ValueType=typeof (decimal),
                          ColName="NeededBail"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="开始时间",
                          ValueType=typeof (DateTime),
                          ColName="StartTime"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="爆仓类型",
                          ValueType=typeof (BlastType),
                          ColName="BlastType",
                          ListItems=new List<SelectListItem> {
                              new SelectListItem{ Text="强平权利仓",Value="1"},
                              new SelectListItem{Text="强平义务仓",Value="2"}
}
                      } ,
                };
                return cols;
            }
            #endregion

            #region deal
            if (typeof(T) == typeof(Deal))
            {
                var cols = new List<QueryArgsCol>
                {
                       new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="主动委托人",
                          ValueType=typeof (string),
                          ColName="MainName"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="被动委托人",
                          ValueType=typeof (string),
                          ColName="SlaveName"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="合约代码",
                          ValueType=typeof(string),
                          ColName="ContractCode"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="主动委托编号",
                          ValueType=typeof (int),
                          ColName="MainOrderId"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="被动委托编号",
                          ValueType=typeof (int),
                          ColName="SlaveOrderId"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="成交时间",
                          ValueType=typeof (DateTime),
                          ColName="When"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="是否是部分成交",
                          ValueType=typeof (bool),
                          ColName="IsPartialDeal"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="数量",
                          ValueType=typeof (int),
                          ColName="Count"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="成交类型",
                          ValueType=typeof (DealType),
                          ColName="DealType",
                          ListItems=new List<SelectListItem> {
                              new SelectListItem{Text="双开",Value="1"},
                              new SelectListItem{Text="双平",Value="2"},
                              new SelectListItem{Text="空换",Value="3"}
                          }
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="价格",
                          ValueType=typeof (decimal),
                          ColName="Price"
                      } ,
                };
                return cols;
            }
            #endregion

            #region order
            if (typeof(T) == typeof(Order))
            {
                var Cols = new List<QueryArgsCol>
                {
                     new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="用户姓名",
                          ValueType=typeof (string),
                          ColName="Trader.Name"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="合约代码",
                          ValueType=typeof (string),
                          ColName="Contract.Code"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="合约名称",
                          ValueType=typeof (string),
                          ColName="Contract.Name"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="合约类型",
                          ValueType=typeof (ContractType),
                          
                          ColName="Contract.ContractType",
                           ListItems=new List<SelectListItem> {
                               new SelectListItem{ Text="货币",Value="1"},
                               new SelectListItem{ Text="期权",Value="2"},
                               new SelectListItem{ Text="期货",Value="3"},
                           }
                      } ,

                       new QueryArgsCol
                      { 
                          DisplayName="买卖",
                          ValueType=typeof (TradeDirectType),
                          ColName="Direction",
                          ListItems=new List<SelectListItem> {
                              new SelectListItem{ Text="买",Value="1"},
                              new SelectListItem{ Text="卖",Value="2"},
                          }

                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="开平",
                          ValueType=typeof (OrderType),
                          ColName="OrderType",
                          ListItems=new List<SelectListItem> {
                              new SelectListItem{ Text="平仓",Value="1"},
                              new SelectListItem{ Text="开仓",Value="2"},
                          }

                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="策略",
                          ValueType=typeof (OrderPolicy),
                          ColName="OrderPolicy",
                          ListItems=new List<SelectListItem> {
                              new SelectListItem{ Text="限价申报",Value="1"},
                              new SelectListItem{ Text="市价IOC",Value="2"},
                              new SelectListItem{ Text="市价剩余转限价",Value="3"},
                              new SelectListItem{ Text="限价FOK",Value="4"},
                              new SelectListItem{ Text="市价FOK",Value="5"}
                          }

                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="持仓类型",
                          ValueType=typeof (PositionType),
                          ColName="positionType",
                          ListItems=new List<SelectListItem> {
                              new SelectListItem{ Text="权利仓",Value="1"},
                              new SelectListItem{ Text="义务仓",Value="2"},
                          }

                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="价格",
                          ValueType=typeof (decimal),
                          ColName="Price"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="数量",
                          ValueType=typeof (int),
                          ColName="Count"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="成交数量",
                          ValueType=typeof (int),
                          ColName="DoneCount"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="委托时间",
                          ValueType=typeof (DateTime),
                          ColName="OrderTime"
                      } ,

                       new QueryArgsCol
                      { 
                          DisplayName="挂单状态",
                          ValueType=typeof (OrderRequestStatus),
                          ColName="RequestStatus",
                          ListItems=new List<SelectListItem> {
                              new SelectListItem{ Text="挂单成功",Value="1"},
                              new SelectListItem{ Text="挂单失败",Value="2"},
                          }

                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="当前状态",
                          ValueType=typeof (OrderState),
                          ColName="State",
                          ListItems=new List<SelectListItem> {
                              new SelectListItem{ Text="等待中",Value="1"},
                              new SelectListItem{ Text="已撤销",Value="2"},
                              new SelectListItem{ Text="已成交",Value="3"},
                              new SelectListItem{ Text="部分成交",Value="4"},
                              new SelectListItem{ Text="已行权",Value="5"}
                          }

                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="系统单",
                          ValueType=typeof (bool),
                          ColName="IsBySystem"
                      } ,
                };
                return Cols;
            }
            #endregion

            #region AccountTradeRecord
            if (typeof(T) == typeof(AccountTradeRecord))
            {
                var Cols = new List<QueryArgsCol>
                {
                     new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="用户姓名",
                          ValueType=typeof (string),
                          ColName="Who.Name"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="金额",
                          ValueType=typeof (decimal),
                          ColName="Delta"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="操作类型",
                          ValueType=typeof (AccountChangeType),
                          ColName="OperateType",
                          ListItems = Enum.GetValues(typeof(AccountChangeType))
                            .Cast<AccountChangeType>()
                            .Select(_=>new SelectListItem{Text=_.ToString(),Value=((int)_).ToString()})
                            .ToList(),
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="操作时间",
                          ValueType=typeof (DateTime),
                          ColName="When"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="当前余额",
                          ValueType=typeof (decimal),
                          ColName="Current"
                      } ,

                       new QueryArgsCol
                      { 
                          DisplayName="是否是保证金",
                          ValueType=typeof (bool),
                          ColName="IsBail"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="货币名称",
                          ValueType=typeof (string),
                          ColName="Coin.Name"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="描述",
                          ValueType=typeof (string),
                          ColName="OrderDesc"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="操作人",
                          ValueType=typeof (string),
                          ColName="ByWho"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="操作结果",
                          ValueType=typeof (bool),
                          ColName="IsAddTo"
                      } ,
                };
                return Cols;
            }

            #endregion

            #region Coin
            if (typeof(T) == typeof(Coin))
            {
                var Cols = new List<QueryArgsCol>
                {
                     new QueryArgsCol
                      { 
                          DisplayName="编号",
                          ValueType=typeof (int),
                          ColName="Id"
                      } ,
                        new QueryArgsCol
                      { 
                          DisplayName="货币名称",
                          ValueType=typeof (string),
                          ColName="Name"
                      } ,
                      new QueryArgsCol
                      { 
                          DisplayName="维持保证金",
                          ValueType=typeof (decimal),
                          ColName="MainBailRatio"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="期权价倍数",
                          ValueType=typeof (string),
                          ColName="MainBailTimes"
                      } ,
                       new QueryArgsCol
                      { 
                          DisplayName="合约代码",
                          ValueType=typeof (ContractType),
                          ColName="CotractCode"
                      } ,
                };
                return Cols;
            }
            #endregion
            return null;
        }
    }


    /// <summary>
    /// 代表一个查询条件
    /// </summary>
    public class QueryArgs
    {
        /// <summary>
        /// 查询列的表示名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 阀值
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 与阀值比较关系
        /// </summary>
        public string Relation { get; set; }
        /// <summary>
        /// 与下一个条件的关系
        /// </summary>
        public string Next { get; set; }

        public string Relation2 { get; set; }
        /// <summary>
        /// 与下一个条件的关系
        /// </summary>
        public string Value2 { get; set; }

    }
    /// <summary>
    /// 表达式合并器
    /// </summary>
    public static class PredicateBuilder
    {
        public static Expression<Func<T, bool>> AndAlso<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            // need to detect whether they use the same
            // parameter instance; if not, they need fixing
            ParameterExpression param = expr1.Parameters[0];
            if (ReferenceEquals(param, expr2.Parameters[0]))
            {
                // simple version
                return Expression.Lambda<Func<T, bool>>(
                    Expression.AndAlso(expr1.Body, expr2.Body), param);
            }
            // otherwise, keep expr1 "as is" and invoke expr2
            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(
                    expr1.Body,
                    Expression.Invoke(expr2, param)), param);
        }
        public static Expression<Func<T, bool>> OrElse<T>(
                          this Expression<Func<T, bool>> expr1,
                          Expression<Func<T, bool>> expr2)
        {
            ParameterExpression param = expr1.Parameters[0];
            if (ReferenceEquals(param, expr2.Parameters[0]))
            {
                return Expression.Lambda<Func<T, bool>>(
                    Expression.OrElse(expr1.Body, expr2.Body), param);
            }
            return Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(
                    expr1.Body,
                    Expression.Invoke(expr2, param)), param);
        }
    }

    /// <summary>
    /// 查询引擎
    /// </summary>
    public class QueryEngine
    {
        /// <summary>
        /// 查询条件
        /// </summary>
        public List<QueryArgs> Args { get; set; }
        /// <summary>
        /// 能查询的列元数据
        /// </summary>
        public List<QueryArgsCol> Cols { get; set; }
        /// <summary>
        /// 列元数据列表的json表示
        /// </summary>
        public string JsonString { get { return JsonConvert.SerializeObject(Cols); } }
        public QueryEngine()
        {
            Args = new List<QueryArgs>();

            PageIndex = 1;
        }
        public string BackUrl { get; set; }

        public QueryArgsCol GetCol(QueryArgs qa)
        {
            if (qa == null || qa.Name == null) return null;
            var q = (from a in Cols where a.DisplayName == qa.Name select a).FirstOrDefault<QueryArgsCol>();
            return q;
        }
        Expression<Func<T, bool>> GetExpression<T>(string propertyName, string propertyValue)
        {
            var parameterExp = Expression.Parameter(typeof(T), "type");
            var propertyExp = Expression.Property(parameterExp, propertyName);
            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var someValue = Expression.Constant(propertyValue, typeof(string));
            var containsMethodExp = Expression.Call(propertyExp, method, someValue);

            return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
        }

        object GetValueObj(Type t, string v)
        {
            try
            {
                var c = TypeDescriptor.GetConverter(t);
                var av = c.ConvertFrom(v);
                return av;
            }
            catch (Exception e)
            {
                if (t.IsValueType)
                    return Activator.CreateInstance(t);
                return null;
            }
        }
        MemberExpression GetMemberExp(ParameterExpression p, string colName)
        {
            if (!colName.Contains('.')) return Expression.PropertyOrField(p, colName);
            var ca = colName.Split('.');
            MemberExpression r = null;
            for (int i = 0; i < ca.Length; i++)
            {
                if (i == 0)
                    r = Expression.PropertyOrField(p, ca[i]);
                else
                    r = Expression.PropertyOrField(r, ca[i]);
            }
            return r;
        }
        static TextLog Log = new TextLog("QueryError.txt");
        static readonly int CountPerPage = 20;
        public Expression<Func<T, bool>> GetPredict<T>()
        {
            if (Args == null || Args.Count == 0) return a => true;
            try
            {
                ParameterExpression c = Expression.Parameter(typeof(T), "p");
                Expression<Func<T, bool>> result = null;

                for (int i = 0; i < Args.Count; i++)
                {
                    var arg = Args[i];
                    var col = GetCol(arg);
                    if (col == null) continue;
                    var av = GetValueObj(col.ValueType, arg.Value);
                    ConstantExpression constant1 = Expression.Constant(av, col.ValueType);

                    var av2 = GetValueObj(col.ValueType, arg.Value2);
                    ConstantExpression constant2 = Expression.Constant(av2, col.ValueType);

                    MemberExpression member = GetMemberExp(c, col.ColName);
                    Expression e = null;
                    switch (arg.Relation)
                    {
                        case "等于":
                            try
                            {
                                e = Expression.Equal(member, constant1);
                            }
                            catch (Exception ex) { Log.Error(ex, "express", "queryEngine"); e = null; }

                            break;
                        case "不等于":
                            try
                            {
                                e = Expression.NotEqual(member, constant1);
                            }
                            catch (Exception ex) { Log.Error(ex, "express", "queryEngine"); e = null; }
                            break;
                        case "大于":
                            try
                            {
                                e = Expression.GreaterThan(member, constant1);
                            }
                            catch (Exception ex) { Log.Error(ex, "express", "queryEngine"); e = null; }
                            break;
                        case "小于":
                            try
                            {
                                e = Expression.LessThan(member, constant1);
                            }
                            catch (Exception ex) { Log.Error(ex, "express", "queryEngine"); e = null; }
                            break;
                        case "包含":
                            {
                                try
                                {
                                    var propertyExp = GetMemberExp(c, col.ColName);
                                    MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                    var someValue = Expression.Constant(arg.Value, typeof(string));
                                    var containsMethodExp = Expression.Call(propertyExp, method, someValue);
                                    e = Expression.Lambda<Func<T, bool>>(containsMethodExp, c);
                                }
                                catch (Exception ex) { Log.Error(ex, "express", "queryEngine"); e = null; }
                                break;
                            }
                        case "不包含":
                            {
                                try
                                {
                                    var propertyExp = GetMemberExp(c, col.ColName);
                                    MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                                    var someValue = Expression.Constant(arg.Value, typeof(string));
                                    var containsMethodExp = Expression.Call(propertyExp, method, someValue);
                                    var not = Expression.Not(containsMethodExp);
                                    e = Expression.Lambda<Func<T, bool>>(not, c);
                                }
                                catch (Exception ex) { Log.Error(ex, "express", "queryEngine"); e = null; }
                                break;
                            }
                    }

                    if (e == null) continue;
                    if (result == null || i == 0)
                    {
                        result = (e is BinaryExpression) ? Expression.Lambda<Func<T, bool>>(e, c) : ((Expression<Func<T, bool>>)e);
                        continue;
                    }
                    else
                    {
                        switch (Args[i - 1].Next)
                        {
                            case "而且":
                                if (e is BinaryExpression)
                                    result = Expression.Lambda<Func<T, bool>>(Expression.AndAlso(result.Body, e), c);
                                else
                                {
                                    result = result.AndAlso<T>((Expression<Func<T, bool>>)e);
                                }
                                break;
                            case "或者":
                                if (e is BinaryExpression)
                                    result = Expression.Lambda<Func<T, bool>>(Expression.OrElse(result.Body, e), c);
                                else
                                {
                                    result = result.OrElse<T>((Expression<Func<T, bool>>)e);
                                }
                                break;
                        }
                    }
                }
                if (result == null) return a => true;
                return result;
            }
            catch (Exception exx)
            {
                Log.Error(exx);
                return a => true;
            }
        }

        public int PageCount { get; set; }
        public int PageIndex { get; set; }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="S"></typeparam>
        /// <param name="aqa"></param>
        /// <param name="ar"></param>
        /// <param name="backUrl"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static List<T> Query<T, S>(ref QueryEngine aqa, BaseRepository<T> ar, string backUrl, Expression<Func<T, S>> order) where T : class
        {
            if (aqa == null) aqa = new QueryEngine();
            aqa.Cols = QueryArgsCol.GetCols<T>();// Cols;
            aqa.BackUrl = backUrl;

            Expression<Func<T, bool>> qf = aqa.GetPredict<T>();// a => true;
            int pc;
            var q = ar.LoadPage(aqa.PageIndex, CountPerPage, out pc,
               qf,
                false, order);
            var ql = q.ToList<T>();

            aqa.PageCount = pc / CountPerPage + 1;
            return ql;
        }

        public static List<T> QueryList<T, S>(ref QueryEngine aqa, List<T> ar, string backUrl, Func<T, S> order) where T : class
        {
            if (aqa == null) aqa = new QueryEngine();
            aqa.Cols = QueryArgsCol.GetCols<T>();
            aqa.BackUrl = backUrl;

            Expression<Func<T, bool>> qf = aqa.GetPredict<T>();// a => true;
            var q = ar;
            var ql = q.ToList<T>();

            aqa.PageCount = 10 / CountPerPage + 1;
            return ql;
        }
    }
}