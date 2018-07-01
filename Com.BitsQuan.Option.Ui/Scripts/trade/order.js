function gotoLogin() {
    location = "/account/login";
}

var selfunc = function (c, n) { }; 
var curCCode = "";
var curCName = "";
var confirmPwd = {
    pwd: "",
    showed: false,
    confirm: function (id) {
        if (confirmPwd.showed == false) {



            var c = '<div ><div id="confirmPwd" style="border: 2px solid #ddd;padding:3px;width:300px;margin:0px auto">\
            <span>请输入密码:</span>\
            <span>\
                <input type="password" id="thispwd" value="" style="width:100px;"/>\
            </span>\
            <span>\
                <a class="btn btn-primary btn-xs" onclick="confirmPwd.close();">确认</a> \
         <a class="btn btn-info btn-xs" onclick="confirmPwd.cancel();">取消</a> \
            </span>\
        </div></div>';
            $(c).insertBefore($("#" + id).parent());
            confirmPwd.showed = true;
        }

    },
    close: function () {

        confirmPwd.pwd = $("#thispwd").val();
        $("#confirmPwd").parent().empty();
        this.next(this.pwd);
        confirmPwd.showed = false;
    },
    cancel: function () {
        $("#confirmPwd").parent().empty();
        confirmPwd.showed = false;
    },
    next: function () { }
};
var ResetTip = function () { }
var redo = function (e) { }

$(function () {
    curCCode = $("#scurCCode").text();
    curCName = $("#scurCName").text();
   
    //买卖开仓平仓的界面效果
    $("a[data-cat='order']").click(function () {

        var th = $(this);
        var tag = th.data("tag");
        var tar = th.data("target");
        var op = $("#" + tar);
        if (op.hasClass("btn-info"))
            op.removeClass("btn-info");
        if (!th.hasClass("btn-info"))
            th.addClass("btn-info");
        var ot = $("#order");
        if (tar == "sell") {
            $("[for='mprice']").html("买入价");
            $("[for='mcount']").html("买入量");
            ot.removeClass("btn-success");
            ot.addClass("btn-danger");
        }
        else if (tar == "buy") {
            $("[for='mprice']").html("卖出价");
            $("[for='mcount']").html("卖出量");
            ot.removeClass("btn-danger");
            ot.addClass("btn-success");
        }
        var sb = "";
        var oc = "";
        $("a[data-cat='order']").each(function (i, e) {
            var el = $(e);
            if (el.hasClass("btn-info")) {
                var et = el.data("tag");
                if (et.length > 1) {
                    oc = et;
                }
                else sb = et;
            }
        });
        //卖开买开按钮组合的提示信息

        var v = $("#policy").val();
        var isM = v == 2 || v == 3 || v == 5;

        ot.data("dir", sb);
        ot.data("openclose", oc);

        var oc = ot.data("openclose");
        var pin = $("#price");
        //var tip = $("#ordertip");

        if (sb == "卖") {
            if (!isM) {
                var p = 0;
                try {
                    p = myMarket.main.Buy1Price;
                }
                catch (e) {

                }
                if (p == 0) try { p = myMarket.main.Newest; } catch (e) { }
                pin.val(p);
            }
 

        } else if (sb == "买") {
            if (!isM) {
                var p = 0;
                try {
                    p = myMarket.main.Sell1Price;
                }
                catch (e) { }
                if (p == 0)
                    try { p = myMarket.main.Newest; }
                    catch (e) { }
                pin.val(p);
            }
 
        }

        ot.text("下单(" + sb + " " + oc + ")");
    })
    ResetTip = function () {
       
    }
    $("#policy").change(function () {
        var v = $(this).val();
        var p = $("#price");
        if (v == 2 || v == 3 || v == 5) {
            p.val(0);
            p.attr("disabled", true);
        }
        else {
            p.attr("disabled", false);
        }
    })
    //下单
    $("#order").click(function () {
        var th = $(this);

        $.post("/home/GetInputTradeCount", {}, function (d) {
            if (d == "n") {//每次都输入
                confirmPwd.next = function (cr) {
                    soptTrade(cr, th, d);
                }
                confirmPwd.confirm("order");

            } else if (d == "null" || d == null) {//从不输入
                soptTrade("", th, d);
            } else if (d == "1") {//输入一次后变成11在退出前都不会再输入
                confirmPwd.next = function (cr) {
                    var r = soptTrade(cr, th, d);
                }
                confirmPwd.confirm("order");


            } else if (d == "11") {
                soptTrade("", th, d);
            }
        });

        //判断用户是否登录
        $.post("/home/UserIsLogin", {}, function (d) {
            if (d == "False" || d == false) {
                gotoLogin();
            }
        })

        $("#bindphone").click(function () {
            window.location.href = "/secure/Index?ph=1";
        })

    });


    $("#price").change(function () {
        var th = $(this);
        var tv = th.val();
        var tp = parseFloat(tv);
        if (isNaN(tp)) {
            th.val(0.01);
            return;
        }
        var tar = Math.round(tp * 100) / 100;
        if (tar <= 0) tar = 0.01;
        if (tar > 100000) tar = 100000;
        th.val(tar);
        ResetTip();
    });

    $("#count").change(function () {
        var th = $(this);
        var tv = th.val();
        var tp = parseFloat(tv);
        if (isNaN(tp)) {
            th.val(1);
            return;
        }
        var tar = Math.round(tp);
        if (tar <= 0) tar = 1;
        if (tar > 100000) tar = 100000;
        th.val(tar);
        ResetTip();
    });


    redo = function (e) {
        //判断用户是否登录
        $.post("/home/UserIsLogin", {}, function (d) {
            if (d == "False" || d == false) {
                alert("..");
                gotoLogin();
                return;
            }
        })
        $.post("/home/redo", { "orderId": e, "pwd": "" }, function (d) {
            if (d.ResultCode != 0) {
                alert(d.Desc);
            }
            else {
                jsModel.removeFromRedo(e);
                //FreshDequeues();
            }
        });
    }
});

function soptTrade(cr, th, tcount) {
    var r = "dd";//返回结果
    th.addClass("disabled")
    var dir = th.data("dir");
    var openclose = th.data("openclose");
    var price = $("#price").val();
    var code = $("#code").text();
    var policy = $("#policy").val();
    var count = $("#count").val();
    $.post("/home/OrderIt", {
        "code": code,
        "policy": policy,
        "price": price,
        "count": count,
        "direct": dir == "买" ? 1 : 2,
        "openclose": openclose == "开仓" ? 2 : 1,
        "userOpId": new Date().getTime().toString(),
        "pwd": cr
    }, function (d) {
        r = d.Desc;
        if (r == "成功" && tcount == "1") {
            soptTradeOne();
        }


        th.removeClass("disabled"); //FreshDequeues(); FreshDeals();
        var dn = new Date();

        if (d.Desc == "请进入安全中心绑定手机号") {
            $('#mybox').modal({
                keyboard: true
            })
        } else {
            $("#orderResult").text(dn.getHours() + ":" + dn.getMinutes() + ":" + dn.getSeconds() + "  " + d.Desc);
        }
        if (d.ResultCode == 0) {

            jsModel.updateOrder(d.Order);
            //getBail();
            if ((d.Order.Dir == "卖平" || d.Order.Dir == "买平") && d.Order.State == "等待中") {
                var uid = "#up_closecount_" + d.Order.Contract.Code;
                var u = $(uid);
                var oc = parseInt(u.text());
                var nc = oc - d.Order.Count;
                u.text(nc);

                var pid = d.Order.Contract.Name;
                var up = jsModel.PosStroe.items(pid);
                up.ClosableCount -= d.Order.Count;

            }
        }

    })

    return r;
}

//只输入一次
function soptTradeOne() {
    $.post("/spot/UpdateTradeCount", {}, function () { });
}