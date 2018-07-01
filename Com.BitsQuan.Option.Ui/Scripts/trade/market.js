 
function curMarket() {
    this.BtcCur = 0;
    this.main = {};
    this.related = new Hashtable();
     
    this.update = function (d) {
        this.BtcCur = d.BtcCur;
        this.main = d.Main;
        
        if (d.Related != null && d.Related.length > 0) {
            for (var i = 0; i < d.Related.length; i++) {
                var tm = d.Related[i];
                this.related.add(tm.Name, tm);
            }
        }
    }
}
var myMarket = new curMarket();
var myBail = {};
var getBail = function () { };

var getMarket = function () { }
$(function () {
    
    getBail = function () {
        $.get("/home/RefreshMyBail",{"ts":Math.random()}, function (d) {
            myBail = d;
            var dt = new Date();
            var dts = dt.getHours() + ":" + dt.getMinutes() + ":" + dt.getSeconds();
            $("#refreshtime").text(dts);
            //更新保证率等
            if (myBail != null) {
                $("#mymainbail").text(myBail.Maintain);
                $("#myrealbail").text(Math.round((myBail.Total + myBail.Frozen)*100)/100);
                $("#myclosablebail").text(myBail.Usable);
                if (myBail.Ratio == -1) {
                    $("#mymainratio").text("∞");
                    $("#mrshortcut").text("∞");
                }
                else {
                    var tm = myBail.Ratio;
                    $("#mymainratio").text(tm);
                    $("#mrshortcut").text(tm);
                    if (tm == 0) tm = 100;
                    var mmrt = $("#mymainratio");
                    if (tm >= 1.2) {
                        $("#mrshortcut").css("color", "blue");
                        mmrt.css("color", "blue");
                    }
                    else if (tm >= 1.1) {
                        $("#mrshortcut").css("color", "black");
                        mmrt.css("color", "black");
                    }
                    else if (tm >= 1) {
                        $("#mrshortcut").css("color", "#4f0e38");
                        mmrt.css("color", "#4f0e38");
                    }
                    else {
                        $("#mrshortcut").css("color", "red");
                        mmrt.css("color", "red");
                    }
                }
            }
            ResetTip();
        })
        setTimeout("getBail();", 30000);
    }

    getMarket = function () {
        $.get("/home/QueryMarket", { "cname": curCName,"ts":Math.random() }, function (data) {
                    
            myMarket.update(data);

            $("#curbtc").text("币指数:¥" + myMarket.BtcCur);
            jsModel.UpdateSta();
            if (data.Main != null) {
                var d = data.Main;
                $("#anewest").text("最新:¥" + d.NewestDealPrice);
                $("#lraise").html(d.Raise);
                $("#lfall").html(d.Fall);
                $("#araise").text("上限:¥" + d.Raise);
                $("#afall").text("下限:¥" + d.Fall);
                if (d.FuseSeconds > 0) {
                    $("#afusetime").text("熔断:" + d.FuseSeconds + "秒");
                }
                else {
                    $("#afusetime").text("熔断未启动");
                }
                
                //$("#asell").text("卖1:¥" + d.Sell1Price);
                $("#lsell").text(d.Sell1Price);
                $("#lsellcount").text(d.Sell1Count);
                $("#lbuy").text(d.Buy1Price);
                $("#lbuycount").text(d.Buy1Count);
                //$("#abuy").text("买1:¥" + d.Buy1Price);
                $("#curtotal").text("成交:" + d.Times);
                $("#curpos").text("持仓:" + d.PositionTotal);
                $("#curexe").text("行权:" + d.CurExe);

                //总开仓,24小时:开仓,平仓,净开仓
                $("#postotal").text(d.OpenTotal);
                $("#open24").text(d.Open24);
                $("#close24").text(d.Close24);
                $("#pure24").text(d.Pure24);
            }
            
        })
        setTimeout("getMarket();", 2000);
    }
    getMarket();
    getBail();
});

 