
var value = 2;
var count = 1;
var timer;
function showAuto(type) {
    if ($("ul.J-slide li").length == 0)
    {
        return;
    }
    var number = Number(document.getElementById("numberClick").value);
    if (type == number && count != 1) {
        return;
    }
    count++;
    if (timer != null) {
        clearTimeout(timer);
    }
    fadeIn(document.getElementById("bannerBackground" + type), 40, 100);
    document.getElementById("number" + type).className = "";
    var i = 1;
    while (i <= $("#J-slide-number a").length) {
        if (i != type) {
            fadeOut(document.getElementById("bannerBackground" + i), 40, 0);
            document.getElementById("number" + i).className = "slide-number-active";
        }
        i++;
    }
    var j = type;
    while (j < $("#J-slide-number a").length) {
        document.getElementById("bannerBackground" + (j + 1)).style.display = "none";
        j++;
    }
    if (type < $("#J-slide-number a").length) {
        document.getElementById("numberClick").value = type;
        value++;
    }
    else {
        document.getElementById("numberClick").value = type;
        value = 1;
    }
    timer = setTimeout("showAuto(" + value + ")", 10000);
}
timer = setTimeout("showAuto(" + value + ")", 10000);

//底层共用
var iBase = {
    Id: function (name) {
        return document.getElementById(name);
    },
    //设置元素透明度,透明度值按IE规则计,即0~100
    SetOpacity: function (ev, v) {
        ev.filters ? ev.style.filter = 'alpha(opacity=' + v + ')' : ev.style.opacity = v / 100;
    }
};

var tadeInTime;
//淡入效果(含淡入到指定透明度)
function fadeIn(elem, speed, opacity) {
    if (tadeInTime != null) {
        clearTimeout(tadeInTime);
    }
    /*
    * 参数说明
    * elem==>需要淡入的元素
    * speed==>淡入速度,正整数(可选)
    * opacity==>淡入到指定的透明度,0~100(可选)
    */
    speed = speed || 20;
    opacity = opacity || 100;
    //显示元素,并将元素值为0透明度(不可见)
    if (elem != null)
    {
        elem.style.display = 'block';
        iBase.SetOpacity(elem, 0);
        //初始化透明度变化值为0
        var val = 0;
        //循环将透明值以5递增,即淡入效果
        (function () {
            iBase.SetOpacity(elem, val);
            val += 5;
            if (val <= opacity) {
                tadeTime = setTimeout(arguments.callee, speed);
            }
        })();
    }

   
}

var tadeOutTime;
//淡出效果(含淡出到指定透明度)
function fadeOut(elem, speed, opacity) {
    if (tadeOutTime != null) {
        clearTimeout(tadeOutTime);
    }

    /*
    * 参数说明
    * elem==>需要淡入的元素
    * speed==>淡入速度,正整数(可选)
    * opacity==>淡入到指定的透明度,0~100(可选)
    */
    speed = speed || 20;
    opacity = opacity || 0;
    //初始化透明度变化值为0
    var val = 100;
    //循环将透明值以5递减,即淡出效果
    (function () {
        iBase.SetOpacity(elem, val);
        val -= 5;
        if (val >= opacity) {
            tadeOutTime = setTimeout(arguments.callee, speed);
        } else if (val < 0) {
            //元素透明度为0后隐藏元素
            elem.style.display = 'none';
        }
    })();
}