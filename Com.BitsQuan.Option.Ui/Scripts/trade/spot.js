//=================================
//可撤委托更新
 
    var spotModel = {
        soTemp: '<tr id="curspot_{{Id}}"> \
            <td>{{Id}}</td>\
            <td>{{OrderTime}}</td>\
            <td>{{Coin}}</td> \
            <td id="rf_{{Id}}">{{Direction}}</td>  \
            <td>{{Price}}</td> \
            <td id="rc_{{Id}}">{{ReportCount}}</td> \
            <td id="ud_{{Id}}">{{Undeal}}</td> \
            <td id="td_{{Id}}">{{TotalDoneCount}}</td>  \
            <td><a class=" btn btn-danger btn-xs" onclick="spotModel.redo({{Id}})">撤销</a></td> ',
        ids:[],
        updateOrder: function (o) {
            var inredo = $.inArray(o.Id, spotModel.ids) > -1;
            if (o.State == "等待中" || o.State == "部分成交") {
                if (inredo) {
                    //update it
                    $("#rc_" + o.Id).text(o.ReportCount);
                    $("#ud_" + o.Id).text(o.Undeal);
                    $("#td_" + o.Id).text(o.TotalDoneCount);
                    if (o.Direction == "买") {
                        $("#rf_" + o.Id).text("买入");
                    }
                    else {
                        $("#rf_" + o.Id).text("卖出");
                    }
                }
                else {
                    //add to it
                    spotModel.ids.push(o.Id);
                    var tar = Mustache.render(spotModel.soTemp, o);
                   
                    $(tar).insertAfter($("#curheader"));
                    if ($("#rf_" + o.Id).html() == "买") {
                        $("#rf_" + o.Id).html("买入");
                    }
                    else {
                        $("#rf_" + o.Id).html("卖出");
                    }
                }
            }
            else {
                if (inredo) {
                    spotModel.ids.splice($.inArray(o.Id, spotModel.ids), 1);
                    $("#curspot_" + o.Id).empty();
                }
            }
        },
        redo: function (tid) { 
            $.get("/spot/redo?soid=" + tid, function (d) {
                if (d.ResultCode == 0) {
                    $("#curspot_" + tid).empty(); 
                }
            })
        }
    }

$(function () {
    var trade = $.connection.spotHub;
    trade.client.SpotFinish = function (o1) {
        var o = JSON.parse(o1);  
        spotModel.updateOrder(o);
    }
    trade.client.SpotPartialFinish = function (o1) {
        var o = JSON.parse(o1); 
        spotModel.updateOrder(o);
    }
    $.connection.hub.start();

    $.get("getorders", function (d) {
        if (d.Result == 0) {
            for (var i = 0; i < d.Spot.length;i++) {
                spotModel.updateOrder(d.Spot[i]);
            }
        }
    })
})

 
//================================================
//深度图


 
$(function () {
    //更新深度图之前设置深度图宽度
    var cw = $("#container").css("width");
    var mw = $("#mdcontainer").css("width");
    if (cw != mw) {
        $("#mdcontainer").css("width", cw);
    }

        depthCal("/spot/getdeepth?coinName=BTC","BTC深度图");
    })
 

//========================================
//K线图
 
    $(function () {
        $.get("/spot/GetBtcKline", function (d) {
            klinedata = d;
            drawChart("M5", "/spot/GetBtcKlinenow", "BTC行情图");
        })

        $('#redraw > a').click(function () {
            var th = $(this);
            var ct = th.data("ct");
            drawChart(ct, "/spot/GetBtcKlinenow", "BTC行情图");
            $("#redraw > a").each(function (i, e) {
                $(e).removeClass("btn-danger");
            });
            th.addClass("btn-danger");
        })
    });
 

//==============================================================
//市场

        var getMarket = function () {
            $.get("/spot/getmarket", function (d) {
                $("#newestprice").text(d.NewBtc); 
                $("#s1price").text(d.S1Price);
                $("#buytrue").text(d.S1Price);
                $("#b1price").text(d.B1Price);
                $("#selltrue").text(d.B1Price);
                $("#max24").text(d.Max24);
                $("#min24").text(d.Min24);
                $("#totalcount").text(d.Total24);
                selltrueDefault = d.B1Price;
                buytrueDefault = d.S1Price;
         
            });
      
            setTimeout("getMarket();", 3000);
       
        };
        var getMarket2 = function () {
            $.get("/spot/getmarket", function (d) {
                $("#mprice").attr("placeholder", d.S1Price);
                $("#mprice2").attr("placeholder", d.B1Price);
            });
        };
    $(function () {
        getMarket();
        getMarket2();
    })
 

//===================================================================
//下单

 
    var limitTo2 = function (id) {
        var th = $("#" + id);
        var tv = th.val();
        var tp = parseFloat(tv);
        if (isNaN(tp)) {
            th.val(0.01);
            return;
        }
        var tar = Math.round(tp * 100) / 100;
        if (tar <= 0) tar = 0.01;
        th.val(tar);
    };
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
            var getCurrent = function () {
                $.get("/spot/getcurrent", function (d) {
                    $("#spotcurrent").html(d);
                })
            }
            var getHotDeals = function () {
                $.get("/spot/gethotdeals", function (d) {
                    $("#spotdeals").html(d);
                })
            }
            $(function () {
  



                $("#mcount").change(function () { limitTo2("mcount") });
                $("#mprice").change(function () { limitTo2("mprice") });
                $("#mcount2").change(function () { limitTo2("mcount2") });
                $("#mprice2").change(function () { limitTo2("mprice2") });
                $('a[data-dir="orderbuy"]').click(function () {
                    var th = $(this);
                    var c = parseFloat($("#mcount").val(), 0);
                    var p = parseFloat($("#mprice").val(), 0);

                    var op = th.data("op");
                    var poli = "policy";
                    //判断用户是否登录
                    $.post("/spot/UserIsLogin", {}, function (d) {
                        if (d == "False" || d == false) {
                            $("#mresult").text("请先登录");
                            return;
                        }
                    })


                    var v = $("#" + poli).val();
                    var isM = v == 2 || v == 3 || v == 5;
                 
                   
               

                    if (!$.isNumeric(c) || c == 0) {
                        $("#mcount").focus();
                        $("#mresult").text("数量不能空");
                        return;
                    }
                    if (!$.isNumeric(p) || p == 0) {
                        if (!isM) {
                            $("#mprice").focus();
                            $("#mresult").text("价格不能空");
                            return;
                        }
                
                    }
                    $.post("/spot/GetInputTradeCount", {}, function (d) {
                        if (d == "n") {
                            confirmPwd.next = function (pwd) {
                                soptTrade(op, pwd, v, c, p, d,"buy");
                            }
                            confirmPwd.confirm("buyorder");

                        } else if (d == "null" || d == null) {
                            soptTrade(op, "", v, c, p, d, "buy");
                        } else if (d == "1") {
                            confirmPwd.next = function (pwd) {
                                soptTrade(op, pwd, v, c, p, d, "buy");
                            }
                            confirmPwd.confirm("buyorder");

                        } else if (d == "11") {
                            soptTrade(op, "", v, c, p, d, "buy");
                        }
                    });

                    //confirmPwd.next = function (pwd) {
                    //    $.post("/spot/orderit", {
                    //        "coinId": 2,
                    //        "dir": op,
                    //        "pwd": pwd,
                    //        "policy":v,
                    //        "count": c,
                    //        "price": p
                    //    }, function (d) {
                    //        if (d.ResultCode == 0) {
                    //            spotModel.updateOrder(d.Spot);
                    //        }
                    //        $("#mresult").text(d.Desc);
                    //        getCurrent();
                    //    });
                    //};
                    //var tid = $(this).attr("id");
                    //confirmPwd.confirm(tid);

                });
                $('select[data-cat="policy"]').change(function () {
                    var $th = $(this);
                    var v = $th.val();
                    var tar = $("#" + $th.data("tar"));
                    var isM = v == 2 || v == 3 || v == 5;
                    if (isM) {
                        tar.val(0);
                        tar.attr("disabled", true);
                    }
                    else {
                        tar.val('');
                        tar.attr("disabled", false);
                    }
                })


                //$('a[data-dir="ordersell"]').popover({
                //    html: true, content: $("#loginpp").html(),
                //    viewport: { selector: 'body', padding: 0 },
                //    container: 'body'
                //});

              
                $('a[data-dir="ordersell"]').click(function () {
                    var th = $(this);
                    var c = parseFloat($("#mcount2").val(), 0);
                    var p = parseFloat($("#mprice2").val(), 0);

                    var op = th.data("op");
                    var poli = "policy2";
                    //判断用户是否登录
                    $.ajax({
                        type: "post",
                        dataType: "html",
                        url: "/spot/UserIsLogin",
                        data: {},
                        success: function (d) {
                            if (d == "False" || d == false) {
                                $("#mresult2").text("请先登录");
                                return;
                            }
                        }, async: false
                    });

                    //判断用户是否绑定手机号
                    //$.ajax({
                    //    type: "post",
                    //    dataType: "html",
                    //    url: "/spot/UserIsBindPhone",
                    //    data: {},
                    //    success: function (d) {
                    //        if (d == "False" || d == false) {
                    //            $('#myModal').modal({
                    //                keyboard: true
                    //            })
                    //            return;
                    //        }
                    //    }, async: false
                    //});

                    var v = $("#" + poli).val();
                    var isM = v == 2 || v == 3 || v == 5;


                    if (!$.isNumeric(c) || c == 0) { 
                        $("#mcount2").focus();
                        $("#mresult2").text("数量不能空");
                        return; 
                
                    }
                    if (!$.isNumeric(p) || p == 0) {
                        if (!isM) {
                            $("#mprice2").focus();
                            $("#mresult2").text("价格不能空");
                            return;
                        }
                
                    }


                   

                    $.ajax({
                        type: "post",
                        dataType: "html",
                        url: "/spot/GetInputTradeCount",
                        data: {},
                        success: function (d) {
                            if (d == "n") {
                                confirmPwd.next = function (pwd) {
                                    soptTrade(op, pwd, v, c, p, d, "sell");
                                }
                                confirmPwd.confirm("sellorder");

                            } else if (d == "null" || d == null) {
                                soptTrade(op, "", v, c, p, d, "sell");
                            } else if (d == "1") {
                                confirmPwd.next = function (pwd) {
                                    soptTrade(op, pwd, v, c, p, d, "sell");
                                }
                                confirmPwd.confirm("sellorder");

                            } else if (d == "11") {
                                soptTrade(op, "", v, c, p, d, "sell");
                            }
                        }, async: false
                    });
                });

                $("#bindphone").click(function () {
                    window.location.href = "/secure/Index?ph=1";
                })
               
                setInterval("getCurrent();", 2000);
                setInterval("getHotDeals();", 3000);
                //计算总金额
                sumMoney();
                sumMoney2();
      
            })
            //计算总金额
            function sumMoney() {
                //买
                var mprice = $("#mprice").val();
                var mcount = $("#mcount").val();
                $("#buysum").val(mprice * mcount);
       
            }

            function sumMoney2() {
                //卖id="sellsum"buysum
                var mprice2 = $("#mprice2").val();
                var mcount2 = $("#mcount2").val();
                $("#sellsum").val(mprice2 * mcount2);
            }
           

            function soptTrade(op, pwd, v, c, p, tcount, direction) {
                $.post("/spot/orderit", {
                    "coinId": 2,
                    "dir": op,
                    "pwd": pwd,
                    "policy": v,
                    "count": c,
                    "price": p
                }, function (d) {
                    if (d.ResultCode == 0) {
                        spotModel.updateOrder(d.Spot);
                    }
                    if (d.Desc == "下单成功" && tcount == "1") {
                        soptTradeOne();//输入交易密码后，修改状态，不要再输入
                    }
                    if (d.Desc == "请进入安全中心绑定手机号") {
                        $('#mybox').modal({
                            keyboard: true
                        })
                    }else if (direction == "buy") {

                        $("#mresult").text(d.Desc);
                    } else {
                        $("#mresult2").text(d.Desc);
                    }
                   
                    getCurrent();
                });
            }

//只输入一次
            function soptTradeOne() {
                $.post("/spot/UpdateTradeCount", {}, function () {});
            }

