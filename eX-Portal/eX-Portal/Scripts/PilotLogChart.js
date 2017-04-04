var categoryPilot = [];
var TotalMultiDashHrs = [], TotalFixedWingHrs = [], LastMultiDashHrs = [], LastFixedwingHrs = [], LastMonthMultiDashHrs = [], LastMonthFixedwingHrs = [];
var Datalength;
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

  Datalength = aDatas.length;
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
      color: "Green"
    };
    var LastMonthFixedwing = {
      y: data.LastMonthFixedwingHrs,
      url: "/Pilot/PilotDetail/" + data.UserID,
      color: "Green"
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
      //events: {
      //  load: moveLegend,
      //  redraw: moveLegend
      //},
      type: 'column',
      marginRight: 95,
      marginBottom: 110,
      spacingTop: 5,

      spacingLeft: 0,


    },
    legend: {
      padding: 0,
      margin: 5,
      //  align: 'top',
      // verticalAlign: 'top',
      //  y: 180,

      itemStyle: {

        fontSize: '8px'
      }
    },

    title: {
      text: "Total Pilot Time",
      align: 'left',

      style: {
        "fontFamily": 'OpenSans',
        "fontSize": "12pt",
        color: '#ff6666',
        "fontWeight": "bold"
      }
    },

    xAxis: [{

      categories: categoryPilot,
      title: {

        text: 'Pilot Name'
      },

      labels: {

        formatter: function () {
          var text = this.value,
            formatted = text.length > 5 ? text.substring(0, 5) + '..' : text;

          return '<div class="js-ellipse" style="width:150px; overflow:hidden" title="' + text + '">' + formatted + '</div>';
        },

        rotation: -60,
        style: {

          font: '8px'
        }
      }


    }],


    yAxis: [{ // Primary yAxis
      tickPixelInterval: 10,
      labels: {

        style: {
          color: Highcharts.getOptions().colors[2]
        },

      },
      title: {
        text: '                                   Last Flight Time (Minutes)',
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
      tickPixelInterval: 20,
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
        text: null,
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
    }, {
      name: 'LastFlight(Multi Dash Rotor)',
      data: LastMultiDashHrs,
      color: "#397D02",
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