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



$(function () {
    var refresh = function () {
        $.get("/home/refresh",
            {"code":curCCode,"ts":Math.random()},
            function (fd) {
                if (fd == null) return;
                //更新盘面和成交
                $("#curDequeuePanel").html(fd.OrderStr);
                $("#curDealPanel").html(fd.DealStr);
                //更新保证金提示
                myBail = fd.Bail;
                var dt = new Date();
                var dts = dt.getHours() + ":" + dt.getMinutes() + ":" + dt.getSeconds();
                $("#refreshtime").text(dts);
                //更新保证率等
                if (myBail != null) {
                    $("#mymainbail").text(myBail.Maintain);
                    $("#myrealbail").text(Math.round(( myBail.Total + myBail.Frozen)*100)/100);
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
                //更新市场信息
                myMarket.update(fd.Market);

                $("#curbtc").text("¥" + myMarket.BtcCur);
                jsModel.UpdateSta();
                if (fd.Market.Main != null) {
                    var d = fd.Market.Main;
                    $("#anewest").text("¥" + d.NewestDealPrice);
                    $("#lraise").html(d.Raise);
                    $("#lfall").html(d.Fall);
                    $("#araise").text("¥" + d.Raise);
                    $("#afall").text("¥" + d.Fall);
                    if (d.FuseSeconds > 0) {
                        $("#afusetime").text("距离熔断结束:" + d.FuseSeconds + "秒");
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
                    $("#curtotal").text("" + d.Times);
                    $("#curpos").text("" + d.PositionTotal);
                    $("#curexe").text("" + d.CurExe);

                    //总开仓,24小时:开仓,平仓,净开仓
                    $("#postotal").text(d.OpenTotal);
                    $("#open24").text(d.Open24);
                    $("#close24").text(d.Close24);
                    $("#pure24").text(d.Pure24);

                    //$("#price").val(d.Sell1Price);
                }
            }
        );
       
    };
    var refreshFunc = function () {
        refresh();
        setTimeout(function () { refreshFunc(); }, 2000);
    }
    refreshFunc();
});

 
$(function () {
    depthCal("/home/getdeepth?contractName=" + encodeURIComponent(curCName), curCName + "深度图");
    $('#redraw > a').click(function () {

        var th = $(this);
        var ct = th.data("ct");
        drawChart(ct, "/home/getnow?code=" + encodeURIComponent(curCCode), "期权" + curCName);
        $("#redraw > a").each(function (i, e) {
            $(e).removeClass("btn-danger");
        });
        th.addClass("btn-danger");
    })
});
 