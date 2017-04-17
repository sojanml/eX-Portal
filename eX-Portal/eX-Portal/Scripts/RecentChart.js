﻿var category = [];
var LastFlightData = [];
var CurrentmonthFlightData = [];
var TotalFlightData = [];
var xLabel = new Object();
var scrollData = new Object();
var max;
var ChartWidth = 545;
var ChartHeight = 290;

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
  var Hdata = "";
  var Hdata1 = "";
  if (aData.length > 15) {
    max = 15;
    scrollData = {
      enabled: true,
      height: 8

    }

  }
  else {
    max = aData.length - 1;
    scrollData = {
      enabled: false
    }
  }
  var count = 0;
  for (var i = 0; i < aData.length; i++) {
    var data = aData[i];

    var name = data.DroneName;
    var value = data.ShortName;
    var ChColor = ChColor = data.ChartColor;

    xLabel[value] = name;

    category.push(data.ShortName);
    
    if (ChColor == null || ChColor == '') {
      ChColor = "#FF0000";
    }
    //  alert(ColorLuminance(ChColor, -10));
    var testcolor = getNewBrightnessColor(ChColor, 80)
    var lstdata = {
      y: data.LastFlightTime,
      color: ChColor,
      url: "/DroneFlight/Index/" + data.DroneID + "?Flighttype=LastFlight"

    };
    var csdata = {
      y: data.CurrentFlightTime,
      color: testcolor,
      url: "/DroneFlight/Index/" + data.DroneID + "?Flighttype=CurrentMonthFlight"
    };
    var Totaldata = {
      y: data.TotalFightTime,
      color: ChColor,
      url: "/DroneFlight/Index/" + data.DroneID
    };

    LastFlightData.push(lstdata);
    CurrentmonthFlightData.push(csdata);
    TotalFlightData.push(Totaldata)

  }


  initChart();
  initChartTotalFlight();
  
};


function OnErrorCall_(repo) {
  //alert("Woops something went wrong, pls try later !");
}

//shade colouring function
function getNewBrightnessColor(rgbcode, brightness) {
  var r = parseInt(rgbcode.slice(1, 3), 16),
    g = parseInt(rgbcode.slice(3, 5), 16),
    b = parseInt(rgbcode.slice(5, 7), 16),
    HSL = rgbToHsl(r, g, b),
    RGB;

  $('#original_brightness').text(HSL[2] * 100);

  RGB = hslToRgb(HSL[0], HSL[1], brightness / 100);
  rgbcode = '#'
    + convertToTwoDigitHexCodeFromDecimal(RGB[0])
    + convertToTwoDigitHexCodeFromDecimal(RGB[1])
    + convertToTwoDigitHexCodeFromDecimal(RGB[2]);

  return rgbcode;
}

function convertToTwoDigitHexCodeFromDecimal(decimal) {
  var code = Math.round(decimal).toString(16);

  (code.length > 1) || (code = '0' + code);
  return code;
}

function rgbToHsl(r, g, b) {
  r /= 255, g /= 255, b /= 255;
  var max = Math.max(r, g, b), min = Math.min(r, g, b);
  var h, s, l = (max + min) / 2;

  if (max == min) {
    h = s = 0; // achromatic
  } else {
    var d = max - min;
    s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
    switch (max) {
      case r: h = (g - b) / d + (g < b ? 6 : 0); break;
      case g: h = (b - r) / d + 2; break;
      case b: h = (r - g) / d + 4; break;
    }
    h /= 6;
  }

  return [h, s, l];
}


function hslToRgb(h, s, l) {
  var r, g, b;

  if (s == 0) {
    r = g = b = l; // achromatic
  } else {
    function hue2rgb(p, q, t) {
      if (t < 0) t += 1;
      if (t > 1) t -= 1;
      if (t < 1 / 6) return p + (q - p) * 6 * t;
      if (t < 1 / 2) return q;
      if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
      return p;
    }

    var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
    var p = 2 * l - q;
    r = hue2rgb(p, q, h + 1 / 3);
    g = hue2rgb(p, q, h);
    b = hue2rgb(p, q, h - 1 / 3);
  }

  return [r * 255, g * 255, b * 255];
}
//end of conversion

function initChart() {
  var chart = new Highcharts.Chart({
    chart: {
      renderTo: 'RecentFlight',
      width: ChartWidth,
      height: ChartHeight,
      type: 'column',
      marginRight: 0,
      marginBottom: 68,
      spacingLeft: 0,
      spacingBottom: 0,
      events: {
        click: function (event) {
          var chart1 = $('#RecentFlight').highcharts();
          chart1.update({
            title: {
              Exporting: false
            }
          });
        }
      }
    },
    title: {
      text: "Recent Flight Time",
      align: 'left',
      style: {
        "fontFamily": 'OpenSans',
        "fontSize": "12pt",
        color: '#ff6666',
        "fontWeight": "bold"
      }
    },


    xAxis: {
      min: 0,
      max: max,
      //  max: category.length > 3 ? 3 : category.length-1,
      title: {
        text: 'RPAS Name'
      },
      labels: {
        rotation: -60,
        style: {
          font: '8px'
        }
      },
      categories: category,

      crosshair: true,
      scrollbar: scrollData,
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
      }
    },
    credits: {
      enabled: false
    },
    plotOptions: {
      column: {
        stacking: 'normal',
        pointPadding: 0,
        groupPadding: 0,
        pointWidth: 10,

        borderWidth: 0
      }
    },

    series: [{
      name: 'Last Flight',
      data: LastFlightData,
      showInLegend: false,
      stack: 'male',

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
    }]
  });

}


function initChartTotalFlight() {

  //$('#container').highcharts({
  var chart = new Highcharts.Chart({
    chart: {
      width: ChartWidth,
      height: ChartHeight,
      renderTo: 'TotalFlight',

      type: 'column',
      marginRight: 0,
      marginBottom: 68,
      spacingLeft: 0,
      spacingBottom: 0,

    },
    title: {
      text: "Total Flight Time",
      align: 'left',
      style: {
        "fontFamily": 'OpenSans',
        "fontSize": "12pt",
        color: '#ff6666',
        "fontWeight": "bold"
      }
    },

    xAxis: {
      min: 0,
      max: max,
      categories: category,
      title: {
        text: 'RPAS Name'
      },
      labels: {
        rotation: -60,
        style: {

          font: '8px'
        }
      },

      crosshair: true,
      scrollbar: scrollData,
    },


    exporting: {
      enabled: true,
      sourceWidth: 960,
      sourceHeight: 400,
      chartOptions: {
        xAxis: [{
          categories: category,
          min: 0,
          minRange: category.length - 1,
          max: category.length - 1
        }],
        scrollbar: {
          enabled: false
        }
      }
    },

    yAxis: {

      tickPixelInterval: 5,
      //  min:.00,
      // max:2000
      // type: 'Logarthmic',
      //allowDecimals: true,
      //startOnTick: false,
      minPadding: 0,
      //minorTickInterval: 0.1,
      //  tickInterval: 2.5,
      title: {
        text: 'Time  (Minutes)'
      }

    },
    plotOptions: {
      series: {
        minPointLength: 3
      },
      //column: {
      //  minPointLength: 5
      //}
    },
    legend: {
      display: null

    },
    tooltip: {

      formatter: function () {
        var html = xLabel[this.x];
        return '<b>' + html + '</b><br/>' +
          this.series.name + ': ' + this.y + '<br/>'
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
        pointPadding: 0.1,
        borderWidth: 0
      }
    },
    series: [{
      name: 'Total Flight Time',
      data: TotalFlightData,
      showInLegend: false,
      minPointLength: 3,
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