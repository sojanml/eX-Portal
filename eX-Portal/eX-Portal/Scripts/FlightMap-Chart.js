var TheFlightChart = null;
var TheSecondChart = null;
var CharSeries = {};


function ClearChart() {

  CharSeries['FlightGraph'][0].xAxis.setCategories([], false);
  CharSeries['FlightGraph'][0].setData([], false);
  CharSeries['FlightGraph'][1].setData([], false);
  CharSeries['FlightGraph'][2].setData([], false);

  CharSeries['SecondGraph'][0].xAxis.setCategories([], false);
  CharSeries['SecondGraph'][0].setData([], false);
  CharSeries['SecondGraph'][1].setData([], false);

  TheFlightChart.redraw();
  TheSecondChart.redraw();
}

function AddChart(ChartData) {
  //console.log("Position : " + Position + ", ChartIndex:" + ChartIndex);
  //if (ChartData.length < 1) return;
  var MaxRows = 15;
  var Categories = [];
  var Series = [
    [], [], [], [], []
  ];

  var EndPoint = _FlightData.length;
  var StartPoint = _FlightData.length > MaxRows ? _FlightData.length - MaxRows : 0;
  if (_ReplayIndex > 0) {
    StartPoint = _ReplayIndex - MaxRows;
    if (StartPoint < 0) StartPoint = 0;
    if (EndPoint > _ReplayIndex) EndPoint = _ReplayIndex;
  }


  for (var i = StartPoint; i < EndPoint; i++) {
    var DataItem = _FlightData[i];
    var FlightTime = dtConvFromJSON(DataItem.FlightTime, true);
    Categories.push(FlightTime.substring(FlightTime.length - 5));
    Series[0].push(DataItem.Altitude);
    Series[1].push(DataItem.Pich);
    Series[2].push(DataItem.Roll);

    Series[3].push(DataItem.Speed);
    Series[4].push(DataItem.Satellites);

  }

  CharSeries['FlightGraph'][0].xAxis.setCategories(Categories, false);
  CharSeries['FlightGraph'][0].setData(Series[0], false);
  CharSeries['FlightGraph'][1].setData(Series[1], false);
  CharSeries['FlightGraph'][2].setData(Series[2], false);

  CharSeries['SecondGraph'][0].xAxis.setCategories(Categories, false);
  CharSeries['SecondGraph'][0].setData(Series[3], false);
  CharSeries['SecondGraph'][1].setData(Series[4], false);

  TheFlightChart.redraw();
  TheSecondChart.redraw();
}


function InitChart() {
  var ChartColors = {
    'Pitch': 'rgba(255, 89, 0,1)',
    'Altitude': 'rgb(219, 211, 1)',
    'Satallites': 'rgba(101, 186, 25,1)',
    'Speed': 'rgb(11, 144, 118)',
    'Roll': 'rgb(153, 131, 199)'
  };

  var ChartOptions1 = getChartOptions('FlightGraph');
  var ChartOptions2 = getChartOptions('SecondGraph');

  ChartOptions1.series = [ {
    name: 'Altitude',
    type: 'spline',
    data: [],
    yAxis: 1,
    tooltip: { valueSuffix: ' Meter' },
    color: ChartColors.Altitude
  }, {
    name: 'Pitch',
    type: 'spline',
    data: [],
    yAxis: 0,
    tooltip: { valueSuffix: '°' },
    color: ChartColors.Pitch
  }, {
    name: 'Roll',
    type: 'spline',
    data: [],
    yAxis: 0,
    tooltip: { valueSuffix: '°' },
    color: ChartColors.Roll
  }];

  ChartOptions2.series = [{
    name: 'Speed',
    type: 'spline',
    data: [],
    yAxis: 1,
    tooltip: { valueSuffix: ' Meter/Hour' },
    color: ChartColors.Speed
  }, {
    name: 'Satallite',
    type: 'spline',
    data: [],
    yAxis: 1,
    tooltip: { valueSuffix: '' },
    color: ChartColors.Satallites
  }
  ];

  TheFlightChart = new Highcharts.Chart(ChartOptions1);
  TheSecondChart = new Highcharts.Chart(ChartOptions2);
}

function getChartOptions(renderTo) {
  var IsSecondAxis = (renderTo !== 'FlightGraph');
  var ChartOptions = {
    chart: {
      spacing: [5, 0, 0, 0],
      renderTo: renderTo,
      backgroundColor: 'white',
      events: {
        load: function () {
          // set up the updating of the chart each second
          CharSeries[renderTo] = this.series;
        }
      },
      zoomType: 'xy'
    },
    credits: { enabled: false },
    title: { text: '' },
    subtitle: { text: '' },
    xAxis: [{
      categories: [],
      crosshair: true,
      labels: {
        enabled: false,
        rotation: 320
      }
    }],
    yAxis: [{ // Primary yAxis
      labels: {
        format: '{value}',
        style: { color: 'black' }
      },
      title: { text: '' }
    }, { // Secondary yAxis
      labels: {
        format: '{value}',
        style: { color: 'black' },
        enabled: IsSecondAxis
      },
      title: { text: '', style: {} },
      opposite: true
    }],
    tooltip: { shared: true },
    legend: { enabled: false },
    plotOptions: {
      spline: {
        marker: { enabled: false }
      }
    }
  };
  return ChartOptions;
}
