jwplayer.key = "vYTpeN5XOdY1qcyCv75ibloaO/VRGoOeHn6CsA==";
var theChartSeries = null;
var theChart = null;
var ChartIndex = 0;
var _ADSBLayer = null;
var LastProcessedID = 0;
var ChartData = [];


var RunningTotal = new function() {
  this.count = 0;
  var ArrayMinSpeed = [];
  var ArrayMaxSpeed = [];
  var ArrayAvgSpeed = [];
  var ArrayMedSpeed = [];
  var ArrayVechileCount = [];

  var SumMinSpeed = 0;
  var SumMaxSpeed = 0;
  var SumAvgSpeed = 0;
  var SumMedSpeed = 0;
  var SumVechileCount = 0;

  this.Add = function (MinSpeed, MedSpeed, AvgSpeed, MaxSpeed, VechileCount) {
    var tempMinSpeed = 0;
    var tempMaxSpeed = 0;
    var tempAvgSpeed = 0;
    var tempMedSpeed = 0;
    var tempVechileCount = 0;

    if (this.count >= 10) {
      tempMinSpeed = ArrayMinSpeed.shift();
      tempMaxSpeed = ArrayMaxSpeed.shift();
      tempAvgSpeed = ArrayAvgSpeed.shift();
      tempMedSpeed = ArrayMedSpeed.shift();
      tempVechileCount = ArrayVechileCount.shift();
    } else {
      this.count++;
    }

    ArrayMinSpeed.push(MinSpeed);
    ArrayMaxSpeed.push(MaxSpeed);
    ArrayAvgSpeed.push(AvgSpeed);
    ArrayMedSpeed.push(MedSpeed);
    ArrayVechileCount.push(VechileCount);

    SumMinSpeed = SumMinSpeed + MinSpeed - tempMinSpeed;
    SumMaxSpeed = SumMaxSpeed + MaxSpeed - tempMaxSpeed;
    SumAvgSpeed = SumAvgSpeed + AvgSpeed - tempAvgSpeed;
    SumMedSpeed = SumMedSpeed + MedSpeed - tempMedSpeed;
    SumVechileCount = SumVechileCount + VechileCount - tempVechileCount;

    return this.GetAverage();
  }

  this.GetAverage = function () {
    return {
      MinSpeed: SumMinSpeed / this.count,
      MaxSpeed: SumMaxSpeed / this.count,
      AvgSpeed: SumAvgSpeed / this.count,
      MedSpeed: SumMedSpeed / this.count,
      VechileCount: SumVechileCount / this.count
    }
  }

  this.Reset = function () {
    this.count = 0;
    ArrayMinSpeed = [];
    ArrayMaxSpeed = [];
    ArrayAvgSpeed = [];
    ArrayMedSpeed = [];
    ArrayVechileCount = [];

    SumMinSpeed = 0;
    SumMaxSpeed = 0;
    SumAvgSpeed = 0;
    SumMedSpeed = 0;
    SumVechileCount = 0;
  }
};

function getChartData(Column) {
  var data = [];
  return data;


  for (var i = 0; i <= 10; i++) {
    data.push(ChartData[i][Column]);
  }
  return data;
}

$(document).ready(function () {
  getChartDataInit();
  InitVideo();
  InitChart();
  initializeMap();
  //startReplayTimer();
});

function initializeMap() {
  var MarkerPosition = FlightPosition;

  var mapOptions = {
    zoom: 14,
    mapTypeControl: false,
    streetViewControl: false,
    center: MarkerPosition,
    styles: getMapStyle()
  };

  map = new google.maps.Map(document.getElementById('traffic_map'), mapOptions);

  var MarkerImage = {
    url: '/images/marker.png',
    size: new google.maps.Size(37, 120),
    origin: new google.maps.Point(0, 0),
    anchor: new google.maps.Point(23, 106)
  };


  var markerOptions = {
    position: MarkerPosition,
    map: map,
    icon: MarkerImage
  };
  var marker = new google.maps.Marker(markerOptions);

  _ADSBLayer = new ADSBOverlay({ map: map }, []);

}

function ChartDataAdd(Position) {
  //console.log("Position : " + Position + ", ChartIndex:" + ChartIndex);
  if(ChartData.length < 1) return;


  var isRemovePoints = ChartIndex >= 15 ? true : false;
  var DataItem = ChartData.shift();
  var categories = theChartSeries[0].xAxis.categories;
  categories.shift;
  categories.push(DataItem.ProcessTime);

  var BarColor = getBarColor(DataItem.AvgSpeed);
  var theData = {
    type: "column",
    color: BarColor,
    y: DataItem.AvgSpeed,
    categories
  }; //{ y: , x: ], color: 'yellow' };


  //theChartSeries[0].xAxis.categories = categories;
  theChartSeries[0].addPoint(theData, false, isRemovePoints, false);
  theChartSeries[1].addPoint([categories, DataItem.VechileCount], true, isRemovePoints, true);

  //add data to the chart 
  AddChartData(DataItem);

  LastProcessedID = DataItem.PTid;
  ChartIndex++;
}

function getBarColor(theSpeed) {
  if (theSpeed < 40) {
    return '#d80c0f';
  } else if (theSpeed < 60) {
    return '#d56f0a';
  } else {
    return '#5de15a';
  }
}


function AddChartData(DataItem) {
  var TABLE = $('#traffic_data tbody');
  var Rows = TABLE.find('TR');
  if (Rows.length >= 6) TABLE.find('tr:last').remove();
  var TR = $('<tr>' + 
    '<td>' + DataItem.ProcessTime + '</td>' +
    '<td>DIC</td>'+
    '<td>' + DataItem.MinSpeed + '</td>' +
    '<td>' + DataItem.MedSpeed + '</td>'+
    '<td>' + DataItem.AvgSpeed + '</td>' +
    '<td>' + DataItem.MaxSpeed + '</td>' +
    '<td>' + DataItem.VechileCount + '</td>' +
    '</tr>');
  TABLE.prepend(TR);


  var Avg = RunningTotal.Add(DataItem);

    
  ///*Time, MinSpeed,MedSpeed, AvgSpeed,MaxSpeed,VechileCount*/
  //$('#tile-min-speed').html(Avg.MinSpeed.toFixed(2))
  //$('#tile-max-speed').html(Avg.MaxSpeed.toFixed(2))
  //$('#tile-avg-speed').html(Avg.AvgSpeed.toFixed(2))
  //$('#tile-med-speed').html(Avg.MedSpeed.toFixed(2))
  //$('#tile-vechile-count').html(Avg.VechileCount.toFixed(2))
   
  $('#tile-min-speed').html(DataItem.MinSpeed.toFixed(2))
  $('#tile-max-speed').html(DataItem.MaxSpeed.toFixed(2))
  $('#tile-avg-speed').html(DataItem.AvgSpeed.toFixed(2))
  $('#tile-med-speed').html(DataItem.MedSpeed.toFixed(2))
  $('#tile-vechile-count').html(DataItem.VechileCount.toFixed(2))
}


var thePlayTimer = null;
function fn_on_play(theData) {
  startReplayTimer();
}

function fn_on_pause(theData) {
  if (thePlayTimer) window.clearInterval(thePlayTimer);
}

function fn_on_firstFrame() {
  theChartSeries[0].setData([]);
  theChartSeries[1].setData([]);
  $('#traffic_data tbody').html('');
  ChartIndex = 0;
  RunningTotal.Reset();
}

function startReplayTimer() {
  if (thePlayTimer) {
    window.clearInterval(thePlayTimer);
    //window.clearTimeout(thePlayTimer);
  }

  thePlayTimer = window.setInterval(function () {
    var Position = playerInstance.getPosition();
    ChartDataAdd(Position);
  }, 1000);
}

function InitVideo() {
  //var Value = 'https:' + '//exponent-s3.s3.amazonaws.com/traffic/' + FlightVideo;
  var Value = 'https:' + '//exponent-s3.s3.amazonaws.com/VOD/' + FlightVideo;
  //'https:' + '//exponent-s3.s3.amazonaws.com/traffic/v2.flv';
  var PlayList = [
    {
      sources: [{
          file: Value, type: "flv"
      }],
      title: 'Traffic Video',
    }
  ];

  playerInstance = jwplayer("traffic_video");
  playerInstance.setup({
    width: '100%',
    height: 260,
    description: 'Click on play to start video.',
    mediaid: '5aac25b7e7f544ad9f89d433435a2506',
    playlist: PlayList
  });
  playerInstance.on("play", fn_on_play);
  playerInstance.on("pause", fn_on_pause);
  playerInstance.on('firstFrame', fn_on_firstFrame)
}


function InitChart() {
  var GeneralStyle = {
    color: '#FFFFFF',
    fontSize: '8px'
  };

  theChart = $('#traffic_chart').highcharts({
    chart: {
      renderTo: 'container',
      backgroundColor: '#39596e',
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
      labels: {
        distance: 0,
        rotation: 0,
        style: GeneralStyle
      },
    }],
    yAxis: [{ // Primary yAxis
      labels: {
        format: '{value}',
        style: GeneralStyle
      },
      title: {
        text: ''
      },
      gridLineColor: '#6c8393',
      plotLines: [{
        color: 'red', // Color value
        value: 40, // Value of where the line will appear
        width: 1, // Width of the line   
        zIndex:1
      }, {
        color: 'orange', // Color value
        value: 60, // Value of where the line will appear
        width: 1, // Width of the line    
        zIndex: 1
      }
      ],
      min: 0,
      max: 140,
      tickAmount: 8
    }, { // Secondary yAxis
      title: {
        text: '',
      },
      labels: {
        format: '{value}',
        style: GeneralStyle
      },
      opposite: true,
      min: 0,
      max: 20,
      gridLineColor: '#6c8393',
      gridLineWidth: 0
    }],
    tooltip: {
      shared: true
    },
    legend: {
      enabled: false
    },
    series: [{
      name: 'Speed',
      type: 'column',
      data: [],
      tooltip: {
        valueSuffix: ' km/h'
      }

    }, {
      name: 'Vechiles',
      yAxis: 1,
      type: 'spline',
      data: [],
      tooltip: {
        valueSuffix: ''
      }
    }]
  });
}


/************************************************************************
*************************************************************************
*************************************************************************
*************************************************************************/

function getChartDataInit() {
  //download data from the server
  var URL = '/traffic/livedata?FlightID=' + FlightID + '&LastProcessedID=' + LastProcessedID;
  var AJAX = $.ajax({
    url: URL,
    type: 'GET',
    success: function (data) {
      ChartData = ChartData.concat(data);
      //Save the last ID for next fetch
      if (ChartData.length > 0) {
        LastProcessedID = ChartData[ChartData.length - 1].PTid;
      }//if
    }//succes
  });//$.ajax

  AJAX.always(function () {
    window.setTimeout(getChartDataInit, 1000 * 3);
  });
}

function getMapStyle() {
  return [{
    "elementType": "all",
    "featureType": "all",
    "stylers": [{ "visibility": "on" }]
  }, {
    "elementType": "labels",
    "featureType": "all",
    "stylers": [{ "visibility": "off" },
        { "saturation": "-100" }
    ]
  }, {
    "elementType": "labels.text.fill",
    "featureType": "all",
    "stylers": [{ "saturation": 36 },
        { "color": "#000000" },
        { "lightness": 40 },
        { "visibility": "off" }
    ]
  }, {
    "elementType": "labels.text.stroke",
    "featureType": "all",
    "stylers": [{ "visibility": "off" },
        { "color": "#000000" },
        { "lightness": 16 }
    ]
  }, {
    "elementType": "labels.icon",
    "featureType": "all",
    "stylers": [{ "visibility": "off" }]
  }, {
    "elementType": "geometry.fill",
    "featureType": "administrative",
    "stylers": [{ "color": "#000000" },
        { "lightness": 20 }
    ]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "administrative",
    "stylers": [{ "color": "#000000" },
        { "lightness": 17 },
        { "weight": 1.2 }
    ]
  }, {
    "elementType": "geometry",
    "featureType": "landscape",
    "stylers": [{ "color": "#000000" },
        { "lightness": 20 }
    ]
  }, {
    "elementType": "geometry.fill",
    "featureType": "landscape",
    "stylers": [{ "color": "#4d6059" }]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "landscape",
    "stylers": [{ "color": "#4d6059" }]
  }, {
    "elementType": "geometry.fill",
    "featureType": "landscape.natural",
    "stylers": [{ "color": "#4d6059" }]
  }, {
    "elementType": "geometry",
    "featureType": "poi",
    "stylers": [{ "lightness": 21 }]
  }, {
    "elementType": "geometry.fill",
    "featureType": "poi",
    "stylers": [{ "color": "#4d6059" }]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "poi",
    "stylers": [{ "color": "#4d6059" }]
  }, {
    "elementType": "geometry",
    "featureType": "road",
    "stylers": [{ "visibility": "on" },
        { "color": "#7f8d89" }
    ]
  }, {
    "elementType": "geometry.fill",
    "featureType": "road",
    "stylers": [{ "color": "#7f8d89" }]
  }, {
    "elementType": "geometry.fill",
    "featureType": "road.highway",
    "stylers": [{ "color": "#7f8d89" },
        { "lightness": 17 }
    ]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "road.highway",
    "stylers": [{ "color": "#7f8d89" },
        { "lightness": 29 },
        { "weight": 0.20000000000000001 }
    ]
  }, {
    "elementType": "geometry",
    "featureType": "road.arterial",
    "stylers": [{ "color": "#000000" },
        { "lightness": 18 }
    ]
  }, {
    "elementType": "geometry.fill",
    "featureType": "road.arterial",
    "stylers": [{ "color": "#7f8d89" }]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "road.arterial",
    "stylers": [{ "color": "#7f8d89" }]
  }, {
    "elementType": "geometry",
    "featureType": "road.local",
    "stylers": [{ "color": "#000000" },
        { "lightness": 16 }
    ]
  }, {
    "elementType": "geometry.fill",
    "featureType": "road.local",
    "stylers": [{ "color": "#7f8d89" }]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "road.local",
    "stylers": [{ "color": "#7f8d89" }]
  }, {
    "elementType": "geometry",
    "featureType": "transit",
    "stylers": [{ "color": "#000000" },
        { "lightness": 19 }
    ]
  }, {
    "elementType": "all",
    "featureType": "water",
    "stylers": [{ "color": "#2b3638" },
        { "visibility": "on" }
    ]
  }, {
    "elementType": "geometry",
    "featureType": "water",
    "stylers": [{ "color": "#2b3638" },
        { "lightness": 17 }
    ]
  }, {
    "elementType": "geometry.fill",
    "featureType": "water",
    "stylers": [{ "color": "#24282b" }]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "water",
    "stylers": [{ "color": "#24282b" }]
  }, {
    "elementType": "labels",
    "featureType": "water",
    "stylers": [{ "visibility": "off" }]
  }, {
    "elementType": "labels.text",
    "featureType": "water",
    "stylers": [{ "visibility": "off" }]
  }, {
    "elementType": "labels.text.fill",
    "featureType": "water",
    "stylers": [{ "visibility": "off" }]
  }, {
    "elementType": "labels.text.stroke",
    "featureType": "water",
    "stylers": [{ "visibility": "off" }]
  }, {
    "elementType": "labels.icon",
    "featureType": "water",
    "stylers": [{ "visibility": "off" }]
  }
  ];
}
