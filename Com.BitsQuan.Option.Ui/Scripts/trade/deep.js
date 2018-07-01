 
        Highcharts.theme = {
            colors: ["#FC898D", "#9BCE85", "#ED561B", "#DDDF00", "#24CBE5", "#64E572", "#FF9655", "#FFF263", "#6AF9C4"],
            chart: { borderColor: "#DDD", plotShadow: !0, plotBorderWidth: 1 },
            title: { style: { color: "#000", font: 'bold 16px "Trebuchet MS", Verdana, sans-serif' } },
            subtitle: { style: { color: "#666666", font: 'bold 12px "Trebuchet MS", Verdana, sans-serif' } },
            xAxis: {
                gridLineWidth: 1, lineColor: "#000", tickColor: "#000",
                labels: { style: { color: "#000", font: "11px Trebuchet MS, Verdana, sans-serif" } },
                title: { style: { color: "#333", fontWeight: "bold", fontSize: "12px", fontFamily: "Trebuchet MS, Verdana, sans-serif" } }
            },
            yAxis: {
                minorTickInterval: "auto", lineColor: "#000", lineWidth: 1, tickWidth: 1, tickColor: "#000",
                labels: { style: { color: "#000", font: "11px Trebuchet MS, Verdana, sans-serif" } },
                title: { style: { color: "#333", fontWeight: "bold", fontSize: "12px", fontFamily: "Trebuchet MS, Verdana, sans-serif" } }
            },
            legend: { itemStyle: { font: "9pt Trebuchet MS, Verdana, sans-serif", color: "black" }, itemHoverStyle: { color: "#039" }, itemHiddenStyle: { color: "gray" } },
            labels: { style: { color: "#99b" } },
            navigation: { buttonOptions: { theme: { stroke: "#CCCCCC" } } }
        };
var highchartsOptions = Highcharts.setOptions(Highcharts.theme);
var chart = {};
var deepInterval = undefined;


function depthCal(url, dt) {
    if (deepInterval != undefined) {
        clearInterval(deepInterval);
    }

   chart= new Highcharts.Chart({
       chart: {
           type: "area", renderTo: "mdcontainer",
           events: {
               load: function () {
                 
                   var rfunc = function () {
                       $.get(url, function (s) {
                           var ser = chart.series[0];
                           var ser1 = chart.series[1];
                           ser.setData(s[0].sort(function (t, a)
                           { return t[0] > a[0] ? 1 : t[0] < a[0] ? -1 : 0 }));
                           ser1.setData(s[1].sort(function (t, a) {
                               return t[0]>a[0]?1:t[0]<a[0]?-1:0
                           })
                               );
                       },'json')
                   };
                   rfunc();
                   deepInterval=setInterval(function () { rfunc(); }, 10000);
               }
           }
       },
        title: { text: dt },
        xAxis: {
            title: { text: '价格' },           
            labels: { formatter: function () { return "" + this.value } }
        },
        yAxis: {
            title: { text: '数量' },
            
        },
        tooltip: { pointFormat: "价格:" + "{point.x} <br />累计数量:{point.y}" },
        plotOptions: { area: { pointStart: 0, marker: { enabled: !1, symbol: "circle", radius: 2, states: { hover: { enabled: !0 } } } } },
        series: [{ name: '累积买单', data: [] }, { name: '累积卖单', data: [] }]
    });
}
 
 