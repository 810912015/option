var klinedata = {};
var drawChart = function (t) { };
var ohlcInterval = undefined;

Highcharts.setOptions({
    global: {
        useUTC: true
    },
    credits: {
        enabled: false//去掉右下角的标志
        //text: "比权网络科技(上海)有限公司",
        //position: {
        //    align: 'right',
        //    x: -10,
        //    verticalAlign: 'top',
        //    y: 45
        //}
    },
    exporting :{enabled:false },
    turboThreshold: 100,
    //colors: ['#FF0000', '#DD1111', '#DDDF0D', '#7798BF', '#55BF3B', '#DF5353', '#aaeeee', '#ff0066', '#eeaaee', '#55BF3B', '#DF5353', '#7798BF', '#aaeeee'],
    lang: {
        shortMonths: ['一月', '二月', '三月', '四月', '五月', '六月', '七月', '八月', '九月', '十月', '十一月', '十二月'],
        weekdays: ['周日', '周一', '周二', '周三', '周四', '周五', '周六'],
        rangeSelectorFrom: '',
        rangeSelectorTo: '',
        rangeSelectorZoom: '时间范围',
        printChart: '打印图表',
        downloadJPEG: '下载JPEG',
        downloadPDF: '下载PDF',
       downloadPNG: '下载PNG',
        downloadSVG: '下载SVG'
    },
});


drawChart = function (t, refreshUrl, chartTitle) {
    if (klinedata == null) return;
    if (ohlcInterval != undefined) {
        clearInterval(ohlcInterval);
    }
    var data = null;
    switch (t) {
        case "M5":
            data = klinedata.M5;
            break;
        case "M15":
            data = klinedata.M15;
            break;
        case "M30":
            data = klinedata.M30;
            break;
        case "M60":
            data = klinedata.M60;
            break;
        case "M480":
            data = klinedata.M480;
            break;
        case "M1440":
            data = klinedata.M1440;
            break;
    }
    if (data == undefined) return;
    var ohlc = [],
        volume = [],
        dataLength = data.length;


    for (i = 0; i < dataLength; i++) {
        ohlc.push([
           data[i][0], // the date
           data[i][1], // open
           data[i][2], // high
           data[i][3], // low
           data[i][4] // close
        ]);

        volume.push([
            data[i][0], // the date
            data[i][5] // the volume
        ])
    }

    // set the allowed units for data grouping
    var groupingUnits = [
        [
        'week',                         // unit name
        [1]                             // allowed multiples
        ], [
        'month',
        [1, 2, 3, 4, 6]
        ]];

    // create the chart
    $('#container').highcharts('StockChart', {
        chart: {
            
            events: {
                load: function () {
                    
                    // set up the updating of the chart each second
                    var series = this.series[0];
                    var s2 = this.series[1];
                    ohlcInterval= setInterval(function () {
                        $.get(refreshUrl,//"/home/getnow",
                            {}, function (d) {
                                //console.log(d);
                                try{
                                    if (typeof (d) != "undefined" && $.isArray(d) && typeof (d.length) != "undefined" && d.length > 0
                                        &&typeof(series.data.length)!="undefined"
                                        ) {

                                        var lastIndex = series.data.length-1;
                                        var last = series.data[lastIndex];
                                        if (typeof(last)!="undefined"&& last.category == d[0]) {
                                      
                                            series.removePoint(lastIndex, true, false);
                                        }

                                        series.addPoint(d, true, false);


                                        var vli = s2.data.length - 1;
                                        var vl = s2.data[vli];
                                    
                                        if (vl.category == d[0]) {
                                            s2.removePoint(vli, true, false);
                                        }
                                        s2.addPoint([d[0], d[5]], true, false);
                                    }
                                }
                                catch(e){}

                        });
                    }, 30000);
                }
            }
        },
        rangeSelector: {
            buttons: [{
                type: 'minute',
                count: 30,
                text: '30分钟 '
            }, {
                type: 'minute',
                count: 60,
                text: '1小时'
            }, {
                type: 'hour',
                count: 24,
                text: ' 1天 '
            }, {
                type: 'month',
                count: 1,
                text: '一月'
            }, {
                type: 'all',
                count: 1,
                text: '全部'
            }],
            buttonSpacing: 10,
            buttonTheme: { // styles for the buttons
                //fill: '#0f0',
                stroke: '#0f0',
                width: 40,
                height: 10,
                style: {
                    // color: '#039',
                    fontWeight: 'bold',
                },
                states: {
                    hover: {
                        //fill: '#f00',
                        style: {
                            color: '#e55600'
                        }
                    },
                    select: {
                        //fill: '#039',
                        style: {
                            color: '#e55600'
                        }
                    }
                }
            },
            inputBoxBorderColor: '#fff',
            inputBoxWidth: 0,
            inputBoxHeight: 0,
            inputStyle: {
                color: '#fff',
                fontWeight: 'bold'
            },
            labelStyle: {
                color: 'silver',
                fontWeight: 'bold'
            },
            selected: 0,

        },


        title: {
            text: chartTitle
        },
        navigator:{
            height: 20,
            offset: 0,
            top:315
        },
        
        xAxis: { type: 'datetime' },
        plotOptions: {
            candlestick: { color: '#00ff00', upColor: '#ff0000' },
            series: {threshold: 0}
        },
        yAxis: [{
            labels: {
                style: { color: '#e55600' }
            },
            //tickInterval: 0.1,
            //max: 30,
            //min:5,
            title: {
                text: '价格[RMB]', style: { color: '#e55600' }
            },
            height: 160, lineWidth: 2, gridLineDashStyle: 'Dash', showLastLabel: true
        }, {
            labels: {
                style: { color: '#4572A7' }
            },
            title: {
                text: '成交量', style: { color: '#4572A7' }
            },
            offset: 0,
            top: 280,
            height: 25,
            lineWidth: 2,
            gridLineDashStyle: 'Dash', showLastLabel: true
        }],

        series: [{
            type: 'candlestick',
            name: chartTitle,
            tooltip: {
                pointFormat: '<span style="color:{series.color}">\u25CF</span> <b> {series.name}</b><br/>' +
                    '开盘: {point.open}<br/>' +
                    '最高: {point.high}<br/>' +
                    '最低: {point.low}<br/>' +
                    '收盘: {point.close}<br/>'
            },
            
            data: ohlc,
            dataGrouping: {
                units: groupingUnits
            }
        }, {
            type: 'column',
            name: '成交量',
            data: volume,
            yAxis: 1,
            dataGrouping: {
                units: groupingUnits
            }
        }]
    });
}

 