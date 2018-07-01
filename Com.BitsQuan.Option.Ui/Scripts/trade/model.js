
var InitFunc = function () { };
var pageIndex = 1;
var pageCount = 1;
function Hashtable()//自定义hashtable
{
    this._hash = new Object();
    this.add = function (key, value) {
        if (typeof (key) != "undefined") {

            this._hash[key] = typeof (value) == "undefined" ? null : value;
            return true;


        } else {
            return false;
        }
    }
    this.remove = function (key) { delete this._hash[key]; }
    this.count = function () { var i = 0; for (var k in this._hash) { i++; } return i; }
    this.items = function (key) { return this._hash[key]; }
    this.contains = function (key) { return typeof (this._hash[key]) != "undefined"; }
    this.clear = function () { for (var k in this._hash) { delete this._hash[k]; } }
    this.all = function () {
        var a = [];
        for (var k in this._hash) {
            var v = this._hash[k];
            a.push(v);
        }
        return a;
    }
}

function PosStroe() {
    this.getDutyCC = function () {
        var key = curCName;
        var v = this._hash[key];
        if (v == null)
            return 0;
        return v.ClosableCount;
    }
    this.getRightCC = function () {
        var key = curCName;
        var v = this._hash[key];
        if (v == null)
            return 0;
        return v.ClosableCount;
    }
}
PosStroe.prototype = new Hashtable();

var jsModel = {
    waitingTemp: "<tr  id='waiting_{{Id}}'>\
<td>{{Id}}</td>\
<td>{{Time}}</td>\
                <td id='c_c_{{Id}}'>{{Contract.Code}}</td>\
                <td>{{Contract.Name}}</td>\
<td>{{Dir}}{{OrderType}}</td>\
<td>{{Policy}}</td>\
    <td>{{Price}}</td>\
    <td>{{Count}}</td>\
    <td id='w_dc_{{Id}}'>{{DoneCount}}</td>\
<td id='w_uc_{{Id}}'>{{UndealCount}}</td>\
<td><a class='btn btn-danger btn-xs' id='waiting_redo_{{Id}}' onclick='redo({{Id}});'>撤单</a></td>\
            </tr>",
    RedoHeader: $("#tredo-header"),
    WaitingIds: [],
    removeFromRedo: function (oid) {
        var inredo = $.inArray(oid, this.WaitingIds);
        if (inredo > -1) {
            this.WaitingIds.splice(inredo, 1);
        }
        $("#waiting_" + oid).empty();
    },
    updateOrder: function (o) {
        //如果已成交
        //    如果在可撤中则去除
        //否则
        //    如果在可撤中更新数量状态等
        //    否则加入
        var inredo = $.inArray(o.Id, this.WaitingIds) > -1;
        if (o.State == "等待中" || o.State == "部分成交") {
            if (inredo) { 
               $("#w_dc_" + o.Id).text(o.DoneCount); 
               $("#w_uc_" + o.Id).text(o.UndealCount);
               $("#w_dt_" + o.Id).text(o.DealTotal); 
            }
            else {
                this.WaitingIds.push(o.Id);
                var wt = Mustache.render(this.waitingTemp, o);
                $(wt).insertAfter(jsModel.RedoHeader);
            }
        }
        else {
            if (inredo) {
                this.WaitingIds.splice($.inArray(o.Id, this.WaitingIds), 1);
                $("#waiting_" + o.Id).empty();
            }
        }
    },

    upTemp: "<tr id='up_{{Id}}'><td>{{Contract.Code}}</td>\
            <td id='up_cname_{{Id}}'>{{Contract.Name}}</td>\
            <td id='up_positiontype_{{Id}}'>{{PositionType}}</td>\
    <td id='up_count_{{Id}}'>{{Count}}</td>\
    <td id='up_closecount_{{Contract.Code}}'>{{ClosableCount}}</td>\
    <td id='up_buyprice_{{Id}}'>{{BuyPrice}}</td>\
    <td id='up_buytotal_{{Id}}'>{{BuyTotal}}</td>\
    <td id='up_floatprofit_{{Id}}'>{{FloatProfit}}</td>\
    <td id='up_closeprofit_{{Id}}'>{{CloseProfit}}</td>\
    <td id='up_maintain_{{Id}}'>{{Maintain}}</td>\
    <td id='up_totalvalue_{{Id}}'>{{TotalValue}}</td>\
    </tr>",
    PosHeader: $("#position-header"),
    PosStroe: new PosStroe(),
    UpdateSta: function () {
        var ps = jsModel.PosStroe.all();
        var totalfp = 0;
        var totalcp = 0;
        for (var i = 0; i < ps.length; i++) {
            var p = ps[i];
            var m = myMarket.related.items(p.Contract.Name);
            if (m == undefined) continue;
            var cp = m.NewestDealPrice;
            var tv = cp * p.Count;
            $("#up_totalvalue" + p.Id).text(tv);


            totalfp += p.FloatProfit;
            totalcp += p.CloseProfit;
            //更新维持保证金
            if (p.PositionType == "权利仓") continue;
            if (p.Contract.OptionType == "认购期权") {
                var v1 = (cp + myMarket.BtcCur * 0.1*p.Contract.CoinCount) * p.Count;
                var v2 = p.Count * cp * 3;
                var v = v1 >= v2 ? v1 : v2;
                var va = Math.round(v * 100) / 100;
                $("#up_maintain_" + p.Id).text(va);
            }
            else {
                var m1 = cp * 3;
                var m2 = cp + p.Contract.ExcutePrice * 0.1*p.Contract.CoinCount;
                var m3 = m1 > m2 ? m1 : m2;
                var m4 = p.Contract.ExcutePrice*p.Contract.CoinCount;
                var m5 = m3 > m4 ? m4 : m3;
                var m6 = m5 * p.Count;
                var m7 = Math.round(m6 * 100) / 100;
                $("#up_maintain_" + p.Id).text(m7);
            }
        }

        //平仓盈亏
        $("#mycloseprofit").text(Math.round(totalcp * 100) / 100);
        //浮动盈亏
        $("#myfloatprofit").text(Math.round(totalfp * 100) / 100);


    },
    PosIds: [],
    UpdatePosition: function (up) {
        jsModel.PosStroe.add(up.Contract.Name, up);
        var hasnot = $.inArray(up.Id, jsModel.PosIds) == -1;
        if (hasnot) {
            if (up.Count > 0) {
                var r = Mustache.render(jsModel.upTemp, up);
                $(r).insertAfter(jsModel.PosHeader);
                jsModel.PosIds.push(up.Id);
                if (up.PositionType == "权利仓") {
                    $("#up_positiontype_" + up.Id).css("text-align", "left");
                }
                else {
                    $("#up_positiontype_" + up.Id).css("text-align", "right");
                    $("#up_positiontype_" + up.Id).css("color", "red");
                }
            }
        }
        else {
            if (up.Count == 0 || up.Contract.IsNotInUse) {
                $("#up_" + up.Id).empty();
                var index = $.inArray(up.Id, jsModel.PosIds);
                var r = jsModel.PosIds.splice(index, 1);
            }
            else {
                $("#up_count_" + up.Id).text(up.Count);
                $("#up_buyprice_" + up.Id).text(up.BuyPrice);
                $("#up_buytotal_" + up.Id).text(up.BuyTotal);
                $("#up_floatprofit_" + up.Id).text(up.FloatProfit);
                $("#up_closeprofit_" + up.Id).text(up.CloseProfit);
                $("#up_maintain_" + up.Id).text(up.Maintain);
                $("#up_totalvalue_" + up.Id).text(up.TotalValue);
                $("#up_positiontype_" + up.Contract.Code).text(up.PositionType);
                $("#up_closecount_" + up.Contract.Code).text(up.ClosableCount);
                if (up.PositionType == "权利仓") {
                    $("#up_positiontype_" + up.Id).css("text-align", "left");
                    $("#up_positiontype_" + up.Id).css("color", "green");
                }
                else {
                    $("#up_positiontype_" + up.Id).css("text-align", "right");
                    $("#up_positiontype_" + up.Id).css("color", "red");
                }
            }
        }
    }
};


$(function () {
    InitFunc = function () {
        $.get("/home/getmyinfo", function (d) {
            if (d.Orders.length > 0) {
                for (var i = 0; i < d.Orders.length; i++) {
                    jsModel.updateOrder(d.Orders[i]);
                }
            }
            if (d.Positions.length > 0) {
                for (var i = 0; i < d.Positions.length; i++) {
                    jsModel.UpdatePosition(d.Positions[i]);
                }
            }
        })
    };
    InitFunc();
});
