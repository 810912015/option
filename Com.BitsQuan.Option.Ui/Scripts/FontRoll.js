
var ScrollTime;
function ScrollAutoPlay(contID, scrolldir, showwidth, textwidth, steper) {
    var PosInit, currPos;
    with ($('#' + contID)) {
        currPos = parseInt(css('margin-left'));
        if (scrolldir == 'left') {
            if (currPos < 0 && Math.abs(currPos) > textwidth) {
                css('margin-left', showwidth);
            }
            else {
                css('margin-left', currPos - steper);
            }
        }
        else {
            if (currPos > showwidth) {
                css('margin-left', (0 - textwidth));
            }
            else {
                css('margin-left', currPos - steper);
            }
        }
    }
}
//--------------------------------------------左右滚动效果----------------------------------------------
/*
AppendToObj：        显示位置（目标对象）
ShowHeight：        显示高度
ShowWidth：        显示宽度
ShowText：        显示信息
ScrollDirection：    滚动方向（值：left、right）
Steper：        每次移动的间距（单位：px；数值越小，滚动越流畅，建议设置为1px）
Interval:        每次执行运动的时间间隔（单位：毫秒；数值越小，运动越快）
*/
function ScrollText(AppendToObj, ShowHeight, ShowWidth, ShowText, ScrollDirection, Steper, Interval) {
    var TextWidth, PosInit, PosSteper;
    with (AppendToObj) {
        html('');
        css('overflow', 'hidden');
        css('height', ShowHeight + 'px');
        css('line-height', ShowHeight + 'px');
        css('width', ShowWidth);
    }
    if (ScrollDirection == 'left') {
        PosInit = ShowWidth;
        PosSteper = Steper;
    }
    else {
        PosSteper = 0 - Steper;
    }
    if (Steper < 1 || Steper > ShowWidth) { Steper = 1 }//每次移动间距超出限制(单位:px)
    if (Interval < 1) { Interval = 20 }//每次移动的时间间隔（单位：毫秒）
    var Container = $('<div></div>');
    var ContainerID = 'ContainerTemp';
    var i = 0;
    while ($('#' + ContainerID).length > 0) {
        ContainerID = ContainerID + '_' + i;
        i++;
    }
    with (Container) {
        attr('id', ContainerID);
        css('float', 'left');
        css('cursor', 'default');
        appendTo(AppendToObj);
        html(ShowText);
        TextWidth = width();
        if (isNaN(PosInit)) { PosInit = 0 - TextWidth; }
        css('margin-left', PosInit);
        mouseover(function () {
            clearInterval(ScrollTime);
        });
        mouseout(function () {
            ScrollTime = setInterval("ScrollAutoPlay('" + ContainerID + "','" + ScrollDirection + "'," + ShowWidth + ',' + TextWidth + "," + PosSteper + ")", Interval);
        });
    }
    ScrollTime = setInterval("ScrollAutoPlay('" + ContainerID + "','" + ScrollDirection + "'," + ShowWidth + ',' + TextWidth + "," + PosSteper + ")", Interval);
}



if (document.getElementById('GoogleAD') != null) {
    document.getElementById('GoogleAD').innerHTML = '<div class="SearchEngine_AD1">' + document.getElementById('GoogleADCode').innerHTML + '</div>';
}

