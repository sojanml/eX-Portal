var FlightCharts = function () {
  var ChartAltitude = new FlightMapChart('ChartAltitude', 'Altitude','Meter');
  var ChartSpeed = new FlightMapChart('ChartSpeed','Speed','M/S');
  var ChartSatalite = new FlightMapChart('ChartSatalite', 'Satalite', 'Satalite');

  var _ChartData = {
    xAxis: [],
    Altitude: [],
    Speed: [],
    Satalite: []
  };

  var _AddChartData = function (Data) {
    for (var i = 0; i < Data.length; i++) {
      var DataItem = Data[i];
      _ChartData.xAxis.push(_FmtTime(DataItem.FlightTime));
      _ChartData.Speed.push(Math.round(DataItem.Speed * 10) / 10);
      _ChartData.Satalite.push(DataItem.Satellites);
      _ChartData.Altitude.push(Math.round(DataItem.Altitude, 0));
    }
  };

  var _SetIndex = function (EndIndex) {
    var StartIndex = 0;
    var MaxLen = 80;
    if (EndIndex >= MaxLen) {
      StartIndex = EndIndex - MaxLen;
    } else {
      StartIndex = 0;
    }

    //Slicer start index is not 0, it is 1
    //StartIndex = StartIndex + 1;
    EndIndex = EndIndex + 1;

    var xAxis = _ChartData.xAxis.slice(StartIndex, EndIndex);
    var Speed = new ChartSeries();
    var Satalite = new ChartSeries();
    var Altitude = new ChartSeries();
    Speed.AddArray(_ChartData.Speed.slice(StartIndex, EndIndex));
    Satalite.AddArray(_ChartData.Satalite.slice(StartIndex, EndIndex));
    Altitude.AddArray(_ChartData.Altitude.slice(StartIndex, EndIndex));

    ChartAltitude.SetData(xAxis, Altitude);
    ChartSpeed.SetData(xAxis, Speed);
    ChartSatalite.SetData(xAxis, Satalite);
    
  };

  var _SummaryChart = function (Data) {
    if (Data.length < 1) return;

    var aAltitude = new ChartSeries();
    var aSpeed = new ChartSeries();
    var aSatalite = new ChartSeries();
    var xAxis = [];

    var MinAltitude = Data[0].Altitude;
    var MinSpeed = Data[0].Speed;
    var MinSatalite = Data[0].Satellites;
    for (var i = 0; i < Data.length; i++) {
      var DataItem = Data[i];
      xAxis.push(_FmtTime(DataItem.FlightTime));
      aSpeed.Add(Math.round(DataItem.Speed * 10)/10);
      aSatalite.Add(DataItem.Satellites);
      aAltitude.Add(Math.round(DataItem.Altitude,0));
    }
    ChartAltitude.Init(xAxis, aAltitude);
    ChartSpeed.Init(xAxis, aSpeed);
    ChartSatalite.Init(xAxis, aSatalite);
  };

  var _FmtTime = function (sNetDate) {
    var nDate = new Date();
    if (sNetDate !== null) {
      var r = /\/Date\(([0-9]+)\)\//i
      var matches = sNetDate.match(r);
      if (matches.length === 2) {
        nDate = new Date(parseInt(matches[1]));
      }
    }
    //Convert date to UTC
    //nDate.setMinutes(nDate.getMinutes() + nDate.getTimezoneOffset());
    return _pad(nDate.getHours()) + ':' + _pad(nDate.getMinutes()) + ':' + _pad(nDate.getSeconds());
  };

  var _pad = function (Num) {
    if (Num >= 10) return Num;
    return '0' + Num;
  };

  return {
    Init: _SummaryChart,
    AddChartData: _AddChartData,
    MoveToIndex: _SetIndex
  }
}();


function ChartSeries() {
  var _TheArray = [];
  var _MinVal = 0;
  var _MaxVal = 0;

  var _AddArray = function (TheArray) {
    for (var i = 0; i < TheArray.length; i++) {
      _AddItem(TheArray[i]);
    }
  };

  var _AddItem = function (TheNum) {
    if (_TheArray.length === 0) {
      _MinVal = TheNum;
      _MaxVal = TheNum;
    } else {
      if (TheNum < _MinVal) _MinVal = TheNum;
      if (TheNum > _MaxVal) _MaxVal = TheNum;
    }
    _TheArray.push(TheNum);
  };

  var _Min = function () {
    return _MinVal;
  };

  var _Max = function () {
    return _MaxVal;
  };

  var _Get = function () {
    return _TheArray;
  };

  return {
    Add: _AddItem,
    AddArray: _AddArray,
    Min: _Min,
    Max: _Max,
    Get: _Get
  }
}


function FlightMapChart(DivID, SeriesName, SeriesUnit) {
  var _DivID = DivID;
  var _Chart = null;
  var _SeriesName = SeriesName;
  var _SeriesUnit = SeriesUnit;

  var _InitChart = function (xAxis, yAxisSeries) {
    _Chart = new Highcharts.Chart(_ChartOptions(yAxisSeries));
    _Chart.series[0].xAxis.setCategories(xAxis, false);
    _Chart.series[0].setData(yAxisSeries.Get(), false);
    _Chart.redraw();
  };

  var _SetData = function (xAxis, yAxisSeries) {
    _Chart.series[0].xAxis.setCategories(xAxis, false);
    _Chart.yAxis[0].setExtremes(yAxisSeries.Min() - 1, yAxisSeries.Max());
    _Chart.series[0].setData(yAxisSeries.Get(), false);
    _Chart.redraw();
  };

  var _ChartOptions = function (yAxisSeries) {
    var MinVal = yAxisSeries.Min();
    var MaxVal = yAxisSeries.Max();
    console.log('MinVal:' + MinVal + ', MaxVal: ' + MaxVal);

    return {
      chart: {
        spacing: [5, 0, 0, 0],
        renderTo: _DivID,
        backgroundColor: 'white',
        events: {
          load: function () {
            // set up the updating of the chart each second
            //CharSeries[renderTo] = this.series;
          }
        },
        zoomType: 'xy'
      },
      credits: { enabled: false },
      title: { text: '' },
      subtitle: { text: '' },
      xAxis: {
        visible: false,
        lineWidth: 0,
        minorGridLineWidth: 0,
        categories: [],
        crosshair: false,
        labels: {
          enabled: false,
          rotation: 320
        },
        gridLineWidth: '0px'
      },
      yAxis: [{ // Primary yAxis
        min: MinVal - 1,
        max: MaxVal,
        startOnTick: false,
        endOnTick: false,
        visible: false,
        lineWidth: 0,
        minorGridLineWidth: 0,
        gridLineWidth: '0px',
        labels: {
          format: '{value}',
          style: { color: 'black' },
          enabled: false
        },
        title: { text: '' }
      }, { // Secondary yAxis
        labels: {
          format: '{value}',
          style: { color: 'black' },
          enabled: false
        },
        title: { text: '', style: {} },
        opposite: true
      }],
      legend: { enabled: false },

      plotOptions: {
        area: {
          fillColor: {
            linearGradient: {
              x1: 0, y1: 0,
              x2: 0, y2: 1
            },
            stops: [
              [0, 'rgba(238,22,21,1)'],
              [1, 'rgba(64,255,239,1)']
            ]
          },
          marker: {
            enabled: false,
            radius: 2
          },
          lineWidth: 0,
          states: {
            hover: {
              lineWidth: 0
            }
          },
          threshold: null
        }
      },
      tooltip: {
        formatter: function () {
          return '<b>' + this.x + '</b><br>' + this.y + ' ' + _SeriesUnit;
        }
      },
      series: [{
        name: ' ',
        type: 'area',
        data: [],
        yAxis: 0,
        tooltip: {
          valueSuffix: ' ' + _SeriesUnit,
        }
      }]
    };
  };

  return {
    Init: _InitChart,
    SetData: _SetData
  };
};


