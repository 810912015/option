selfunc = function (c, n) {
    $("#selectOption").trigger("click");
    setSel(c, n); refreshSel(c, n);
}

refreshSel = function (c, n) {
    curCCode = c; curCName = n;
    $("#scurCCode").text(c);
    $("#scurCName").text(n);
    $("#code").text(c);
    $("#cname").text(n);


    //更新ohlc数据
    $.get("/home/fakekline?code=" + curCCode, function (d) {
        if (d == null) return;
        klinedata = d;
        drawChart("M5", "/home/getnow?code=" + curCCode, "期权" + curCName);
    })
    //更新深度图之前设置深度图宽度
    var cw = $("#container").css("width");
    var mw = $("#mdcontainer").css("width");
    if (cw != mw) {
        $("#mdcontainer").css("width", cw);
    }

    //更新深度图数据
    depthCal("/home/getdeepth?contractName=" + encodeURIComponent(curCName), curCName + "深度图");

}
setSel = function (c, n) {
    setCookie("op-sel-code", c);
    setCookie("op-sel-name", n);
    
}
isCodeValid = function (c) {
    var l = $("#scc_" + c).length;
    return l>0;
}
makeValid = function (c, n) {
    if (isCodeValid(c)) return [c, n];
    var fc = $('div[data-cat="contract"]').first();
    var cc = fc.data("code");
    var cn = fc.data("name");
    return [cc, cn];
}

getSel = function () {
    var c = getCookie("op-sel-code");
    var n = getCookie("op-sel-name");

    var r = makeValid(c, n);
    c = r[0];
    n = r[1];

    if (c != "" && n != "") {
        var s = false;
        var code = "";
        var name = "";
        $("#contracts div div div strong").each(function (i) {
            if (i == 0)
            {
                code = $(this).html();
                name = $(this).siblings().html();
            }
            if (c == $(this).html()) {
                s = true;
            }
        });
        if (s) {
            refreshSel(c, n);
        }
        else {
            setSel(code, name); refreshSel(code, name);
        }
    }
    else {
        var c = $("#scurCCode").text();
        var n = $("#scurCName").text();
        setSel(c, n); refreshSel(c, n);
    }
    
}
moredetail = function () {
    window.location = "/home/getallos?contractCode=" + curCCode;
}


$(function () {
    getSel();
    $("#selectOption").popover({
        html: true, content: $("#contracts").html(),
        viewport: { selector: 'body', padding: 0 },
        container: 'body'
    });
})
 
