var TheFlightChart = null;
var TheFlightChartSeries = null;


function AddChart(ChartData) {
  //console.log("Position : " + Position + ", ChartIndex:" + ChartIndex);
  if (ChartData.length < 1) return;

  var categories = TheFlightChartSeries[0].xAxis.categories;

  for (var i = 0; i < ChartData.length; i++) {
    var DataItem = ChartData[i];
    var FlightTime = dtConvFromJSON(DataItem.FlightTime, true);
    var isRemovePoints = categories.length >= 20 ? true : false;
    if (isRemovePoints) categories.shift;
    categories.push(FlightTime.substring(15));


    TheFlightChartSeries[0].xAxis.setCategories(categories);
    TheFlightChartSeries[0].addPoint(DataItem.Speed, false);
    TheFlightChartSeries[1].addPoint(DataItem.Altitude, false);
    
    //TheFlightChartSeries[0].addPoint(DataItem.Speed);
    //TheFlightChartSeries[1].addPoint(DataItem.Altitude);
    
  }

  TheFlightChart.redraw();
}


function InitChart() {
  var ChartOptions = {
    chart: {
      spacing: [5, 0, 0, 0],
      renderTo: 'container',
      backgroundColor: 'white',
      events: {
        load: function () {
          // set up the updating of the chart each second
          TheFlightChartSeries = this.series;
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
        rotation: 320
      }
    }],
    yAxis: [{ // Primary yAxis
      labels: {
        format: '{value}',
        style: {
          color: Highcharts.getOptions().colors[1]
        },        
      },
      title: {
        text: 'Speed',
        style: {
          color: Highcharts.getOptions().colors[1]
        }
      }
    }, { // Secondary yAxis
      title: {
        text: 'Altitude',
        style: {
          color: Highcharts.getOptions().colors[0]
        }
      },
      labels: {
        format: '{value}',
        style: {color: Highcharts.getOptions().colors[0]}
      },
      opposite: true
    }],
    tooltip: {shared: true},
    legend: { enabled: false },
    series: [{
      name: 'Speed',
      type: 'column',
      yAxis: 1,
      data: [],
      tooltip: {valueSuffix: ' KM/Hour'}
    }, {
      name: 'Altitude',
      type: 'spline',
      data: [],
      tooltip: {valueSuffix: ' Feet'}
    }]
  };


  TheFlightChart = $('#FlightGraph').highcharts(ChartOptions);

}


