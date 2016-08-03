var category = [];
var LastFlightData = [];
var CurrentmonthFlightData = [];
var TotalFlightData = [];
var xLabel = new Object();


//var LastFlightcolors = ['red', 'green', 'blue', 'orange', 'yellow'];
//var CurrentMonthFlightcolors = ['DarkRed', 'DarkGreen', 'DarkBlue', 'DarkOrange', 'DarkYellow'];


//var TotalMultiDashHrs = [], TotalFixedWingHrs = [], LastMultiDashHrs = [], LastFixedwingHrs = [], LastMonthMultiDashHrs = [], LastMonthFixedwingHrs = [];


$(document).ready(function () {
    $.ajax({
        type: "GET",
        url: FlightDataURL,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        async: true,
        success: OnSuccess_,
        error: OnErrorCall_
    });

  




});


function OnSuccess_(reponse) {

    var aData = reponse;
    var TotalAlerts = {};
    var LastFlightAlert = {};
    var ThisMonth = {};
    var Sum = {
        TotalAlert: 0,
        LastFlightAlert: 0,
        CurrentMonthAlert: 0
    };
    category = [];
    
   
    var LastAccountID = 0;
    var ColorSelector = 0;
    var Hdata="";
    for (var i = 0; i < aData.length; i++) {
        var data = aData[i];


      
        var name = data.DroneName;
        var value = data.ShortName;
        xLabel[value] = name;
        if (LastAccountID != data.AccountID)
        {
            Hdata = Hdata + '<td><span  style="color:' + data.ChartColor + '; font-size:80px">.</span>' + data.AccountName + '</td></tr>'
        }
        LastAccountID = data.AccountID;

        category.push(data.ShortName);
        var ChColor;
        if (data.ChartColor != null) {
            ChColor = data.ChartColor;
        }
        else {
            ChColor ="Black";
        }

            var lstdata = {
                y: data.LastFlightTime,
                color: ChColor,
                url:"/drone/lastflight/"+data.DroneID
               
            };
            var csdata = {
                y: data.CurrentFlightTime,
                color: ChColor,
                url:"/drone/lastmonthflight/"+data.DroneID
            };
            var Totaldata = {
                y: data.TotalFightTime,
                color: ChColor,
                url: "/DroneFlight/Index/"+data.DroneID
            };
      
            LastFlightData.push(lstdata);
            CurrentmonthFlightData.push(csdata);
            TotalFlightData.push(Totaldata)

    }

    
    HtmlData = '<table><tr>'+Hdata+'</tr></table>';
    
    //category = category + "]"
    //alert(category);
    initChart();
    initChartTotalFlight()
};

function OnErrorCall_(repo) {
    //alert("Woops something went wrong, pls try later !");
}
function initChart() {




    //$('#container').highcharts({
    var chart = new Highcharts.Chart({
        chart: {
            renderTo: 'RecentFlight',
        
            type: 'column'
        },
        title: {
            text: null
        },
       
        xAxis: {
            categories: category,
            
            crosshair: true
        },
        yAxis: {
            min: 0,
            tickInterval: 1,
            title: {
                text: 'Time  (Minutes)'
            }
        },
        legend: {
            display:null
            
        },
        tooltip: {

            formatter: function () {
                var html = xLabel[this.x];
                return '<b>' + html + '</b><br/>' +
                    this.series.name + ': ' + this.y + '<br/>'




                //$.each(this.points, function (i, point) {
                //    s += '<br/><span style="color:' + point.series.color + '">\u25CF</span>: ' + point.series.name + ': ' + point.y;
                //});

                //return s;
            }
        },
        credits: {
            enabled: false
        },
        //tooltip: {
        //    //var html=xLabel[this.x]
        //    headerFormat: '<span style="font-size:10px">{'+xLabel[this.xLabel]+'}</span><table>',
        //    pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
        //        '<td style="padding:0"><b>{point.y:.1f} mm</b></td></tr>',
        //    footerFormat: '</table>',
        //    shared: true,
        //    useHTML: true
        //},
        plotOptions: {
            column: {
                pointPadding: 0.2,
                borderWidth: 0
            }
        },
        series: [{
            name: 'Last Flight',
            data: LastFlightData,
            showInLegend: false,
            //----
            cursor: 'pointer',
            point: {
        events: {
        click: function () {
            location.href = this.options.url;
        }
    }
    }
    
        }, {
            name: 'Current Month',
            data: CurrentmonthFlightData,
            showInLegend: false,
            //----
            cursor: 'pointer',
            point: {
        events: {
       click: function () {
            location.href = this.options.url;
        }
}
}
//-----

        }]
        
        
       
    },
    function (chart) { // on complete


        chart.renderer.text(HtmlData, 50, 245)
        
        //chart.renderer.text('This text is <span style="color: red">styled</span> and <a href="http://example.com">linked</a>', 50,250)
        //    .css({
        //        color: '#4572A7',
        //        fontSize: '10px'
        //    })
            .add();
    }  );
}


function initChartTotalFlight() {




    //$('#container').highcharts({
    var chart = new Highcharts.Chart({
        chart: {
            renderTo: 'TotalFlight',

            type: 'column'
        },
        title: {
            text: null
        },

        xAxis: {
            categories: category,

            crosshair: true
        },
        yAxis: {
            min: 0,
            tickInterval: 1,
            title: {
                text: 'Time  (Minutes)'
            }
        },
        legend: {
            display: null

        },
        tooltip: {

            formatter: function () {
                var html = xLabel[this.x];
                return '<b>' + html + '</b><br/>' +
                    this.series.name + ': ' + this.y + '<br/>'




                //$.each(this.points, function (i, point) {
                //    s += '<br/><span style="color:' + point.series.color + '">\u25CF</span>: ' + point.series.name + ': ' + point.y;
                //});

                //return s;
            }
        },
        credits: {
            enabled: false
        },
        //tooltip: {
        //    //var html=xLabel[this.x]
        //    headerFormat: '<span style="font-size:10px">{'+xLabel[this.xLabel]+'}</span><table>',
        //    pointFormat: '<tr><td style="color:{series.color};padding:0">{series.name}: </td>' +
        //        '<td style="padding:0"><b>{point.y:.1f} mm</b></td></tr>',
        //    footerFormat: '</table>',
        //    shared: true,
        //    useHTML: true
        //},
        plotOptions: {
            column: {
                pointPadding: 0.2,
                borderWidth: 0
            }
        },
        series: [{
            name: 'Total Flight Time',
            data: TotalFlightData,
            showInLegend: false,

            //----
            cursor: 'pointer',
            point: {
        events: {
        click: function () {
            location.href = this.options.url;
        }
    }
    }
                //-----

        }   ]



    },
    function (chart) { // on complete


        chart.renderer.text(HtmlData, 50, 245)

        //chart.renderer.text('This text is <span style="color: red">styled</span> and <a href="http://example.com">linked</a>', 50,250)
        //    .css({
        //        color: '#4572A7',
        //        fontSize: '10px'
        //    })
            .add();
    });
}