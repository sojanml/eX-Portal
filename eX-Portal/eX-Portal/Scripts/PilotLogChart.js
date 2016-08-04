var categoryPilot = [];
var TotalMultiDashHrs = [], TotalFixedWingHrs = [], LastMultiDashHrs = [], LastFixedwingHrs = [], LastMonthMultiDashHrs = [], LastMonthFixedwingHrs = [];

$(document).ready(function () {
    $.ajax({
        type: "GET",
        url: PilotDataURL,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        async: true,
        success: OnSuccessPilot_,
        error: OnErrorCall_
    });
});


function OnSuccessPilot_(reponse) {

    var aDatas = reponse;
    var TotalAlerts = {};
    var LastFlightAlert = {};
    var ThisMonth = {};
    var Sum = {
        TotalAlert: 0,
        LastFlightAlert: 0,
        CurrentMonthAlert: 0
    };
    categoryPilot = [];
   

    for (var i = 0; i < aDatas.length; i++) {
        var data = aDatas[i];
        categoryPilot.push(data.PilotName);

        var TotalMultiDash = {
            y: data.TotalMultiDashHrs,
            url: "/Pilot/PilotDetail/" + data.UserID,
            color: "#009ACD"
        };
        var TotalFixedWing = {
            y: data.TotalFixedWingHrs,
            url: "/Pilot/PilotDetail/" + data.UserID,
            color: "#7AC5CD"
        };
        var LastMultiDash = {
            y: data.LastMultiDashHrs,
            url: "/Pilot/PilotDetail/" + data.UserID,
            color: "#397D02"
        };

        var LastFixedwing = {
            y: data.LastFixedwingHrs,
            url: "/Pilot/PilotDetail/" + data.UserID,
            color: "#8CDD81"
        };

        var LastMonthMultiDash = {
            y: data.LastMonthMultiDashHrs,
            url: "/Pilot/PilotDetail/" + data.UserID,
            color:"Green"
        };
        var LastMonthFixedwing = {
            y: data.LastMonthFixedwingHrs,
            url: "/Pilot/PilotDetail/" + data.UserID,
            color:"Green"
        };

        TotalMultiDashHrs.push(TotalMultiDash);
        // TotalMultiDashHrs[i] = data.TotalMultiDashHrs;
        TotalFixedWingHrs.push(TotalFixedWing);
        // TotalFixedWingHrs[i] = data.TotalFixedWingHrs;
        LastMultiDashHrs.push(LastMultiDash);
        // LastMultiDashHrs[i] = data.LastMultiDashHrs;
        LastFixedwingHrs.push(LastFixedwing);
        //LastFixedwingHrs[i] = data.LastFixedwingHrs;
        LastMonthMultiDashHrs.push(LastMonthMultiDash);
        //LastMonthMultiDashHrs[i] = data.LastMonthMultiDashHrs;
        LastMonthFixedwingHrs.push(LastMonthFixedwing);
       // LastMonthFixedwingHrs[i] = data.LastMonthFixedwingHrs;

    }
    //category = category + "]"
    //alert(category);
    initCharts();
};

function OnErrorCall_(repo) {
    //alert("Woops something went wrong, pls try later !");
}
function initCharts() {




    //$('#Pilotcontainer').highcharts({

    var chart2 = new Highcharts.Chart({
        chart: {
            renderTo: 'Pilotcontainer',

            type: 'column'
      
        },
        legend: {
            itemStyle: {
                
                fontSize:'8px'
            }
        },

        title: {
            text: null
        },

        xAxis: [{
            //categories: ['Apples', 'Oranges', 'Pears', 'Grapes', 'Bananas',]
            categories: categoryPilot,
            //categories: ['Apples', 'Oranges', 'Pears', 'Grapes']
            //category
            
            
        }],


        yAxis: [{ // Primary yAxis
            tickPixelInterval: 10,
            labels: {

                style: {
                    color: Highcharts.getOptions().colors[2]
                }
            },
            title: {
                text: '**                                   Last Flight Time (Minutes)',
                style: {
                    color: Highcharts.getOptions().colors[2]
                }
               
            },
            labels: {
                format: '{value}',
                style: {
                    color: Highcharts.getOptions().colors[2]
                }
            }
            ,
            opposite: true
           

        }, { // Secondary yAxis
            gridLineWidth: 0,
            tickPixelInterval:20,
            title: {
                text: 'Total Flight Time(minutes)',
                style: {
                    color: Highcharts.getOptions().colors[0]
                }
            },
            labels: {
                format: '{value}',
                style: {
                    color: Highcharts.getOptions().colors[0]
                }
            },
            

        }, { // Tertiary yAxis
            gridLineWidth: 0,
            title: {
                text:null,
                style: {
                    color: Highcharts.getOptions().colors[1]
                }
            },
            labels: {
                format: '{value} M',
                style: {
                    color: Highcharts.getOptions().colors[1]
                }
            },
            opposite: true
        }],
        tooltip: {
            
            formatter: function () {
                return '<b>' + this.x + '</b><br/>' +
                    this.series.name + ': ' + this.y + '<br/>' +
                    'Total: ' + this.point.stackTotal



                //$.each(this.points, function (i, point) {
                //    s += '<br/><span style="color:' + point.series.color + '">\u25CF</span>: ' + point.series.name + ': ' + point.y;
                //});

                //return s;
            }
        },
        credits: {
            enabled: false
        },
        plotOptions: {
            column: {
                stacking: 'normal'
            }
        },

        series: [{
            name: 'Total Flight( Multi Dash Rotor)',
            //data: [5, 3, 4, 7, 2],
            data: TotalMultiDashHrs,
            color: "#009ACD",
            yAxis: 1,
            stack: 'female',
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

        }, {
            name: 'Total Flight(Fixed Wing)',
            data: TotalFixedWingHrs,
            color: "#BFEFFF",
            yAxis: 1,
            stack: 'female',
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

//        }, {
//            name: 'Last Month( Multi Dash Rotor)',
//            data: LastMonthMultiDashHrs,
//            yAxis: 0,
            //stack: 'male',
//            //----
//            cursor: 'pointer',
//            point: {
//                events: {
//                    click: function () {
//                        location.href = this.options.url;
//                    }
//                }
//            }
//            //-----

//        }, {
//            name: 'Last Month(Fixed Wing)',
//            // data: [3, 4, 4, 2, 5],
//            data: LastMonthFixedwingHrs,
//            yAxis: 0,
//            stack: 'male',
//            //----
//            cursor: 'pointer',
//            point: {
//        events: {
//        click: function () {
//            location.href = this.options.url;
//        }
//}
//}
//-----
        }, {
            name: 'LastFlight(Multi Dash Rotor)',
            data: LastMultiDashHrs,
            color:"#397D02",
            yAxis: 0,
            stack: 'fem',
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
        }, {
            name: 'Last Flight(Fixed Wing)',
            data: LastFixedwingHrs,
            color: "#8CDD81",
            yAxis: 0,
            stack: 'fem',
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
    });
}