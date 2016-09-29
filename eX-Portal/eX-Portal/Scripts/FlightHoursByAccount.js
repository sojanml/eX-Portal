
var Months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
var CurrentDate = new Date();
var CurrentMonth = CurrentDate.getMonth();


var MonthData = [];
var SeriesData=[];
$(document).ready(function () {
    $.ajax({
        type: "GET",
        url: MonthlyDataURL,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        async: true,
        success: OnSuccessMonthlyData_,
        error: OnErrorCall_
    });
});





function OnSuccessMonthlyData_(reponse) {
    var start = 0;
    for (var k = 0; k < 12; k++) {
        if ((CurrentMonth + k)>= 12) {
         
            MonthData[k] = Months[start];
            start=start+1;
        }
        else {
            MonthData[k] = Months[CurrentMonth + k];
        }
    }

    var aDataMonthly = reponse;
    


    for (var i = 0; i < aDataMonthly.length; i++) {
        var data = aDataMonthly[i];
        var DataTotal=(data.M1 + data.M2 + data.M3+ data.M4 + data.M5 + data.M6 + data.M7+data.M8+data.M9+data.M10+data.M11+data.M12);
        if (DataTotal > 0) {
           
            var MonthHrs = [];
            MonthHrs.push(data.M1)
            MonthHrs.push(data.M2)
            MonthHrs.push(data.M3)
            MonthHrs.push(data.M4)
            MonthHrs.push(data.M5)
            MonthHrs.push(data.M6)
            MonthHrs.push(data.M7)
            MonthHrs.push(data.M8)
            MonthHrs.push(data.M9)
            MonthHrs.push(data.M10)
            MonthHrs.push(data.M11)
            MonthHrs.push(data.M12)


            var _MonthData = {
                name: data.AccountName,
                data: MonthHrs,
                color: data.ChartColor
            }
            SeriesData.push(_MonthData);
        }
      //  categoryPilot.push(data.PilotName);  


    }
   
    initChartsMonthly();
};

function OnErrorCall_(repo) {
    //alert("Woops something went wrong, pls try later !");
}
function initChartsMonthly() {
       

    var chart5 = new Highcharts.Chart({
        chart: {
            renderTo: 'MonthlyFlight',

            type: 'spline'
            

        },
        legend: {
            itemStyle: {
                
                fontSize:'10px'
            }
        },
        credits: {
            enabled: false
        },

      
            title: {
                text: null,
               // x: -20 //center
            },
            subtitle: {
                 text: null,
                 //x:-20

            },
            xAxis: {
              categories: MonthData,
              title: {
                text: 'Month',
                style: {
                  color: 'black'
                }
              }
            },
            yAxis: [{ // Primary yAxis
                tickPixelInterval: 50,
                labels: {

                    style: {
                        color: Highcharts.getOptions().colors[2]
                    }
                },
                title: {
                    text: 'Total  Flight Time (Minutes)',
                    style: {
                        color: 'black'
                    }
               
                },
                labels: {
                    format: '{value}',
                    style: {
                        color: 'black'
                    }
                }
             ,
                opposite: true
           

            } //{ // Secondary yAxis
            //    gridLineWidth: 0,
            //    tickPixelInterval:20,
            //    title: {
            //        text: 'Total Flight Time(minutes)',
            //        style: {
            //            color: Highcharts.getOptions().colors[2]
            //        }
            //    },
            //    labels: {
            //        format: '{value}',
            //        style: {
            //            color: Highcharts.getOptions().colors[2]
            //        }
            //    },
            

            //}, { // Tertiary yAxis
            //    gridLineWidth: 0,
            //    title: {
            //        text:null,
            //        style: {
            //            color: Highcharts.getOptions().colors[1]
            //        }
            //    },
            //    labels: {
            //        format: '{value} M',
            //        style: {
            //            color: Highcharts.getOptions().colors[1]
            //        }
            //    },
            //    opposite: true
            //}
            ],
            tooltip: {
                valueSuffix: 'Minutes'
            },
            legend: {
                layout: 'vertical',
                align:  'right',
                verticalAlign: 'top',
                x: 0,
                y: 100,
                borderWidth: 0
            },
        series:SeriesData
           
        });
}