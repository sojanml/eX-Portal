jwplayer.key = "vYTpeN5XOdY1qcyCv75ibloaO/VRGoOeHn6CsA==";
var theChartSeries = null;
var theChart = null;
var ChartIndex = 0;

var ChartData = [
['0:00',	66	, 2 ],
['0:01',	67	, 4 ],
['0:02',	74	, 3 ],
['0:03',	69	, 3 ],
['0:04',	36	, 3 ],
['0:05',	96	, 1 ],
['0:06',	102	, 1 ],
['0:07',	62	, 1 ],
['0:08',	35	, 2 ],
['0:09',	64	, 1 ],
['0:10',	68	, 1 ],
['0:11',	59	, 4 ],
['0:12',	76	, 2 ],
['0:13',	99	, 3 ],
['0:14',	81	, 1 ],
['0:15',	73	, 3 ],
['0:16',	70	, 2 ],
['0:17',	69	, 2 ],
['0:18',	38	, 2 ],
['0:19',	67	, 4 ],
['0:20',	68	, 4 ],
['0:21',	80	, 3 ],
['0:22',	61	, 2 ],
['0:23',	78	, 4 ],
['0:24',	69	, 5 ],
['0:25',	54	, 3 ],
['0:26',	62	, 5 ],
['0:27',	71	, 5 ],
['0:28',	70	, 4 ],
['0:29',	51	, 3 ],
['0:30',	56	, 4 ],
['0:31',	62	, 4 ],
['0:32',	69	, 5 ],
['0:33',	71	, 4 ],
['0:34',	65	, 5 ],
['0:35',	72	, 6 ],
['0:36',	59	, 6 ],
['0:37',	72	, 4 ],
['0:38',	71	, 4 ],
['0:39',	64	, 5 ],
['0:40',	71	, 5 ],
['0:41',	70	, 5 ],
['0:42',	69	, 3 ],
['0:43',	71	, 2 ],
['0:44',	52	, 3 ],
['0:45',	50	, 3 ],
['0:46',	57	, 4 ],
['0:47',	57	, 5 ],
['0:48',	58	, 8 ],
['0:49',	73	, 9 ],
['0:50',	60	, 10],
['0:51',	63	, 7 ],
['0:52',	50	, 3 ],
['0:53',	44	, 5 ],
['0:54',	42	, 6 ],
['0:55',	51	, 4 ],
['0:56',	55	, 7 ],
['0:57',	50	, 8 ],
['0:58',	57	, 6 ],
['0:59',	34	, 4 ],
['1:00',	45	, 5 ],
['1:01',	33	, 2 ],
['1:02',	22	, 3 ],
['1:03',	49	, 4 ],
['1:04',	56	, 5 ],
['1:05',	66	, 3 ],
['1:06',	66	, 4 ],
['1:07',	72	, 1 ],
['1:08',	79	, 2 ],
['1:09',	78	, 4 ],
['1:10',	82	, 3 ],
['1:11',	83	, 2 ],
['1:12',	84	, 3 ],
['1:13',	84	, 3 ],
['1:14',	81	, 7 ],
['1:15',	54	, 2 ],
['1:16',	53	, 5 ],
['1:17',	55	, 4 ],
['1:18',	82	, 3 ],
['1:19',	10	, 16],
['1:20',	39	, 8 ],
['1:21',	81	, 7 ],
['1:22',	86	, 2 ],
['1:23',	74	, 1 ],
['1:24',	0	 , 0  ],
['1:25',	0	 ,  0 ],
['1:26',	55	, 2 ],
['1:27',	59	, 2 ],
['1:28',	69	, 4 ],
['1:29',	52	, 5 ],
['1:30',	57	, 5 ],
['1:31',	60	, 3 ],
['1:32',	99	, 2 ],
['1:33',	110	, 2 ],
['1:34',	83	, 3 ],
['1:35',	68	, 4 ],
['1:36',	55	, 3 ],
['1:37',	56	, 3 ],
['1:38',	57	, 4 ],
['1:39',	60	, 4 ],
['1:40',	75	, 4 ],
['1:41',	75	, 4 ],
['1:42',	76	, 2 ],
['1:43',	71	, 2 ],
['1:44',	83	, 4 ],
['1:45',	75	, 8 ],
['1:46',	66	, 8 ],
['1:47',	60	, 12],
['1:48',	62	, 13],
['1:49',	57	, 15],
['1:50',	59	, 14],
['1:51',	67	, 12],
['1:52',	58	, 12],
['1:53',	49	, 9 ],
['1:54',	56	, 6 ],
['1:55',	46	, 9 ],
['1:56',	49	, 9 ],
['1:57',	49	, 11],
['1:58',	65	, 12],
['1:59',	50	, 17],
['2:00',	58	, 12],
['2:01',	56	, 12]
];

function getChartData(Column) {
  var data = [];
  return data;


  for (var i = 0; i <= 10; i++) {
    data.push(ChartData[i][Column]);
  }
  return data;
}

$(document).ready(function () {
  InitVideo();
  InitChart();  
});

function ChartDataAdd(Position) {
  //console.log("Position : " + Position + ", ChartIndex:" + ChartIndex);
  if (Position > ChartData.length) {
    if (thePlayTimer) window.clearInterval(thePlayTimer);
    return;
  }
  var iPosition = Math.round(Position);
  var isRemovePoints = ChartIndex >= 15 ? true : false;
  for (; ChartIndex <= iPosition; ChartIndex++) {
    var DataItem = ChartData[ChartIndex];
    var categories = theChartSeries[0].xAxis.categories;
    categories.shift;
    categories.push(DataItem[0]);
    theChartSeries[1].addPoint([categories, DataItem[1]], false, isRemovePoints);
    theChartSeries[0].addPoint([categories, DataItem[2]], true, isRemovePoints);
  }
}

var thePlayTimer = null;
function fn_on_play(theData) {
  startReplayTimer();
}

function fn_on_pause(theData) {
  if (thePlayTimer) window.clearInterval(thePlayTimer);
}

function startReplayTimer() {
  if (thePlayTimer) {
    window.clearInterval(thePlayTimer);
    //window.clearTimeout(thePlayTimer);
  }

  thePlayTimer = window.setInterval(function () {
    var Position = playerInstance.getPosition();
    ChartDataAdd(Position);
  }, 500);
}

function InitVideo() {
  var PlayList = [
    {
      sources: [{
        file: "https://exponent-s3.s3.amazonaws.com/traffic/v2.flv", type: "flv"
      }],
      title: 'al_sufouh_road_2016101452',
    }
  ];

  playerInstance = jwplayer("traffic_video");
  playerInstance.setup({
    width: '100%',
    height: 400,
    description: 'Click on play to start video.',
    mediaid: '5aac25b7e7f544ad9f89d433435a2506',
    playlist: PlayList
  });
  playerInstance.on("play", fn_on_play);
  playerInstance.on("pause", fn_on_pause);
}


function InitChart() {
  theChart = $('#myChart').highcharts({
    chart: {
      renderTo: 'container',
      events: {
        load: function () {
          // set up the updating of the chart each second
          theChartSeries = this.series;
        }
      }
    },
    credits: {
      enabled: false
    },
    title: {
      text: ''
    },
    subtitle: {
      text: ''
    },
    exporting: {
      enabled: false
    },
    xAxis: [{
      categories: getChartData(0),
      crosshair: true
    }],
    yAxis: [{ // Primary yAxis
      labels: {
        format: '{value} km/h',
        style: {
          color: Highcharts.getOptions().colors[1]
        }
      },
      title: {
        text: 'Speed',
        style: {
          color: Highcharts.getOptions().colors[1]
        }
      }
    }, { // Secondary yAxis
      title: {
        text: 'Number of Vechiles',
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
      opposite: true
    }],
    tooltip: {
      shared: true
    },
    legend: {
      enabled: false
    },
    series: [{
      name: 'Vechiles',
      type: 'column',
      yAxis: 1,
      data: getChartData(2),
      tooltip: {
        valueSuffix: ''
      }

    }, {
      name: 'Speed',
      type: 'spline',
      data: getChartData(1),
      tooltip: {
        valueSuffix: ' km/h'
      }
    }]
  });
}