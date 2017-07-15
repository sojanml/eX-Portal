jwplayer.key = "vYTpeN5XOdY1qcyCv75ibloaO/VRGoOeHn6CsA==";
var theChartSeries = null;
var theChart = null;
var ChartIndex = 0;
var _ADSBLayer = null;

var ChartData = getChartDataInit();


var RunningTotal = new function () {
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
  InitVideo();
  InitChart();
  initializeMap();
});

function initializeMap() {
  var MarkerPosition = { lat: 25.0955354, lng: 55.1527025 };

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
  if (Position >= ChartData.length || ChartIndex >= ChartData.length) {
    if (thePlayTimer) window.clearInterval(thePlayTimer);
    return;
  }
  var iPosition = Math.round(Position);
  var PosMod = iPosition % 15;
  if (PosMod == 0 || iPosition == 1) getADSB();

  var isRemovePoints = ChartIndex >= 15 ? true : false;
  for (; ChartIndex <= iPosition; ChartIndex++) {
    var DataItem = ChartData[ChartIndex];
    var categories = theChartSeries[0].xAxis.categories;
    categories.shift;
    categories.push(DataItem[0]);

    var BarColor = getBarColor(DataItem[2]);
    var theData = {
      type: "column",
      color: BarColor,
      y: DataItem[2],
      categories

    }; //{ y: , x: ], color: 'yellow' };


    //theChartSeries[0].xAxis.categories = categories;
    theChartSeries[0].addPoint(theData, false, isRemovePoints, false);
    theChartSeries[1].addPoint([categories, DataItem[5]], true, isRemovePoints, true);

    //add data to the chart 
    AddChartData(DataItem);

  }
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
    '<td>' + DataItem[0] + '</td>' +
    '<td>DIC</td>' +
    '<td>' + DataItem[1] + '</td>' +
    '<td>' + DataItem[2] + '</td>' +
    '<td>' + DataItem[3] + '</td>' +
    '<td>' + DataItem[4] + '</td>' +
    '<td>' + DataItem[5] + '</td>' +
    '</tr>');
  TABLE.prepend(TR);


  var Avg = RunningTotal.Add(DataItem[1], DataItem[2], DataItem[3], DataItem[4], DataItem[5]);

  /*Time, MinSpeed,MedSpeed, AvgSpeed,MaxSpeed,VechileCount*/
  $('#tile-min-speed').html(Avg.MinSpeed.toFixed(2))
  $('#tile-max-speed').html(Avg.MaxSpeed.toFixed(2))
  $('#tile-avg-speed').html(Avg.AvgSpeed.toFixed(2))
  $('#tile-med-speed').html(Avg.MedSpeed.toFixed(2))
  $('#tile-vechile-count').html(Avg.VechileCount.toFixed(2))

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
  }, 500);
}

function InitVideo() {
  var PlayList = [
    {
      sources: [{
        file: "http://exponent-s3.s3.amazonaws.com/traffic/v2.mp4"
      }],
      title: 'al_sufouh_road_2016101452',
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
        zIndex: 1
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

function getADSB() {
  $.ajax({
    type: "GET",
    url: '/ADSB/',
    contentType: "application/json",
    success: function (result) {
      _ADSBLayer.setADSB(result);
    },
    error: function (data, text) {
      //alert('Failed to fetch flight: ' + data);
    }
  });
}


function ADSBOverlay(options, ADSBData) {
  this.setValues(options);
  this.markerLayer = $('<div />').addClass('overlay');
  this.ADSBData = ADSBData;
  this.ResetDraw = false;
  this.DrawCount = 0;
  this.MovePointsToSecond = 0;
  this.gADSBData = new Object();
  this.setADSB = function (ADSBData) {
    this.ADSBData = ADSBData;
    this.draw();
    this.DrawCount++;
  }


  this.getIconColor = function (Height) {

    var Colors = [
    '#FF0000', //Red
    '#FF9600', //Yellow
    '#00ff10' //Green
    ];


    if (Height < 1000) {
      return Colors[0];
    } else if (Height < 2000) {
      return Colors[1];
    } else {
      return Colors[2];
    }


  }
};

ADSBOverlay.prototype = new google.maps.OverlayView;

ADSBOverlay.prototype.onAdd = function () {
  var $pane = $(this.getPanes().overlayImage); // Pane 3  
  $pane.append(this.markerLayer);
};

ADSBOverlay.prototype.onRemove = function () {
  this.markerLayer.remove();
};

ADSBOverlay.prototype.draw = function () {
  var projection = this.getProjection();
  var zoom = this.getMap().getZoom();
  var fragment = document.createDocumentFragment();
  var NewPoints = 0;


  for (var i = 0; i < this.ADSBData.length; i++) {
    var lat = this.ADSBData[i].Lat;
    var lng = this.ADSBData[i].Lon;
    var alt = this.ADSBData[i].Altitude * 100;
    var title = this.ADSBData[i].FlightID.trim();
    var heading = this.ADSBData[i].Heading;
    heading = parseFloat(heading);
    if (isNaN(heading)) heading = 0;

    // Determine a random location from the bounds set previously  
    var IconGeoPos = new google.maps.LatLng(lat, lng)
    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);
    var DivID = 'adsb-' + title;
    var IconColor = this.getIconColor(alt);

    if (heading == 0) {
      //Landed flight - Ignore movement
    } else if (this.gADSBData[DivID]) {
      var $point = $('#' + DivID);
      $point.animate({ left: IconLocation.x, top: IconLocation.y });
      $point.find(".icon").css({ transform: 'rotate(' + (heading - 45) + 'deg)', color: IconColor });
      $point.attr({ 'data-lat': lat, 'data-lng': lng, 'data-alt': alt });
    } else {
      var $point = $(
          '<div  class="adsb-point" id="' + DivID + '" title="' + title + '" '
        + 'data-lat="' + lat + '" '
        + 'data-lng="' + lng + '" '
        + 'data-alt="' + alt + '" '
        + 'data-ident="' + title + '" '
        + 'style="left:' + IconLocation.x + 'px; top:' + IconLocation.y + 'px;">'
        + '<span class="icon FlightIcon" style=" transform: rotate(' + (heading - 45) + 'deg); color: ' + IconColor + '">&#xf072;</span>'
        + '<span class="flight-title" style="">' + title + '</span>' +
        + '</div>'
      );
      // Append the HTML to the fragment in memory  
      fragment.appendChild($point.get(0));
      NewPoints++;
    }

    this.gADSBData[DivID] = this.DrawCount;

    if (ADSBLine.FlightID == title) {
      ADSBLine.setABSPos(lat, lng, alt);
      ADSBLine.Show();
      $('#' + DivID).trigger("click");
    }

  }//for (var i = 0)

  //if the item does not exists in 20 request, then remove it from the screen
  var AllKeys = Object.keys(this.gADSBData);
  for (i = 0; i <= AllKeys.length; i++) {
    var TheKey = AllKeys[i];
    if (this.gADSBData[TheKey] != this.DrawCount) {
      $('#' + TheKey).fadeOut();
      delete this.gADSBData[TheKey];
    }//if (gADSBData[key] > 20) 
  }//for (var key in gADSBData)


  // Now append the entire fragment from memory onto the DOM  
  if (NewPoints > 0) this.markerLayer.append(fragment);
};



var ADSBLine = {
  FlightID: "",
  EndPos: null,
  StartPos: null,
  AltABS: 0,
  AltEnd: 0,

  PolyLine: new google.maps.Polyline({
    path: [],
    geodesic: true,
    strokeColor: '#0000FF',
    strokeOpacity: 1.0,
    strokeWeight: 2
  }),

  setABSPos: function (lat, lng, alt) {
    this.EndPos = new google.maps.LatLng(lat, lng);
    alt = parseFloat(alt + '');
    this.AltABS = alt * 0.3048;
  },
  setStart: function (lat, lng, alt) {
    this.StartPos = new google.maps.LatLng(lat, lng);
    alt = parseFloat(alt + '');
    this.AltEnd = alt;
  },
  Show: function () {
    if (this.EndPos == null) return;
    if (this.StartPos == null) return;
    this.PolyLine.setPath([this.EndPos, this.StartPos]);
    if (FlightID == "") return;
    this.PolyLine.setMap(map);
  },

  Hide: function () {
    this.PolyLine.setMap(null);
    this.FlightID = ""
  },

  getDistance: function () {
    var distance = getDistanceFromLatLonInKm(
      this.StartPos.lat(),
      this.StartPos.lng(),
      this.EndPos.lat(),
      this.EndPos.lng()
    );
    return distance.toFixed(2);
  },

  getAltDiff: function () {
    var alt = (this.AltABS - this.AltEnd) / 0.3048;
    return alt.toFixed(0);
  }

};




function getChartDataInit() {
  return [
/*Time, MinSpeed,MedSpeed, AvgSpeed,MaxSpeed,VechileCount*/
['0:00', 60, 66, 68.5, 77, 2],
['0:01', 48, 67, 54.5, 61, 4],
['0:02', 45, 74, 65.0, 85, 3],
['0:03', 41, 69, 58.5, 76, 3],
['0:04', 98, 36, 98.0, 98, 3],
['0:05', 88, 96, 96.0, 104, 1],
['0:06', 101, 102, 101.0, 101, 1],
['0:07', 55, 62, 55.0, 55, 1],
['0:08', 10, 35, 47.0, 84, 2],
['0:09', 58, 64, 58.0, 58, 1],
['0:10', 81, 68, 81.0, 81, 1],
['0:11', 55, 59, 62.5, 70, 4],
['0:12', 82, 76, 110.0, 138, 2],
['0:13', 73, 99, 97.0, 121, 3],
['0:14', 83, 81, 87.5, 92, 1],
['0:15', 70, 73, 74.0, 78, 3],
['0:16', 64, 70, 70.0, 76, 2],
['0:17', 58, 69, 67.5, 77, 2],
['0:18', 51, 38, 52.5, 54, 2],
['0:19', 42, 67, 62.0, 82, 4],
['0:20', 41, 68, 62.0, 83, 4],
['0:21', 76, 80, 91.0, 106, 3],
['0:22', 54, 61, 58.0, 62, 2],
['0:23', 50, 78, 76.0, 102, 4],
['0:24', 43, 69, 62.5, 82, 5],
['0:25', 35, 54, 52.0, 69, 3],
['0:26', 55, 62, 70.5, 86, 5],
['0:27', 46, 71, 82.5, 119, 5],
['0:28', 39, 70, 87.0, 135, 4],
['0:29', 51, 51, 82.5, 114, 3],
['0:30', 58, 56, 60.0, 62, 4],
['0:31', 55, 62, 70.0, 85, 4],
['0:32', 34, 69, 61.0, 88, 5],
['0:33', 48, 71, 66.0, 84, 4],
['0:34', 53, 65, 77.0, 101, 5],
['0:35', 45, 72, 77.0, 109, 6],
['0:36', 55, 59, 75.5, 96, 6],
['0:37', 61, 72, 69.0, 77, 4],
['0:38', 60, 71, 61.5, 63, 4],
['0:39', 43, 64, 55.0, 67, 5],
['0:40', 64, 71, 70.5, 77, 5],
['0:41', 66, 70, 73.0, 80, 5],
['0:42', 70, 69, 77.0, 84, 3],
['0:43', 58, 71, 62.5, 67, 2],
['0:44', 46, 52, 69.0, 92, 3],
['0:45', 42, 50, 57.0, 72, 3],
['0:46', 58, 57, 60.5, 63, 4],
['0:47', 51, 57, 68.0, 85, 5],
['0:48', 46, 58, 59.0, 72, 8],
['0:49', 39, 73, 61.0, 83, 9],
['0:50', 46, 60, 68.0, 90, 10],
['0:51', 48, 63, 60.0, 72, 7],
['0:52', 44, 50, 64.0, 84, 3],
['0:53', 37, 44, 55.5, 74, 5],
['0:54', 37, 42, 57.5, 78, 6],
['0:55', 38, 51, 47.5, 57, 4],
['0:56', 41, 55, 59.5, 78, 7],
['0:57', 42, 50, 64.0, 86, 8],
['0:58', 41, 57, 56.5, 72, 6],
['0:59', 57, 34, 76.0, 95, 4],
['1:00', 40, 45, 49.5, 59, 5],
['1:01', 41, 33, 41.0, 41, 2],
['1:02', 47, 22, 47.0, 47, 3],
['1:03', 53, 49, 57.0, 61, 4],
['1:04', 42, 56, 53.0, 64, 5],
['1:05', 47, 66, 70.0, 93, 3],
['1:06', 64, 66, 72.0, 80, 4],
['1:07', 74, 72, 74.0, 74, 1],
['1:08', 81, 79, 81.0, 81, 2],
['1:09', 77, 78, 77.0, 77, 4],
['1:10', 82, 82, 82.5, 83, 3],
['1:11', 85, 83, 85.0, 85, 2],
['1:12', 44, 84, 44.0, 44, 3],
['1:13', 38, 84, 67.0, 96, 3],
['1:14', 42, 81, 66.5, 91, 7],
['1:15', 47, 54, 53.5, 60, 2],
['1:16', 55, 53, 73.5, 92, 5],
['1:17', 72, 55, 118.0, 164, 4],
['1:18', 77, 82, 77.0, 77, 3],
['1:19', 81, 10, 81.0, 81, 16],
['1:20', 88, 39, 104.5, 121, 8],
['1:21', 74, 81, 78.5, 83, 7],
['1:22', 73, 86, 73.0, 73, 2],
['1:23', 85, 74, 92.0, 99, 1],
['1:24', 41, 0, 53.0, 65, 0],
['1:25', 42, 0, 63.5, 85, 0],
['1:26', 38, 55, 38.0, 38, 2],
['1:27', 49, 59, 62.0, 75, 2],
['1:28', 44, 69, 60.0, 76, 4],
['1:29', 50, 52, 72.5, 95, 5],
['1:30', 56, 57, 61.5, 67, 5],
['1:31', 62, 60, 64.5, 67, 3],
['1:32', 67, 99, 92.5, 118, 2],
['1:33', 97, 110, 110.0, 123, 2],
['1:34', 43, 83, 106.0, 169, 3],
['1:35', 49, 68, 59.5, 70, 4],
['1:36', 51, 55, 69.5, 88, 3],
['1:37', 52, 56, 66.0, 80, 3],
['1:38', 54, 57, 70.5, 87, 4],
['1:39', 65, 60, 76.0, 87, 4],
['1:40', 69, 75, 81.5, 94, 4],
['1:41', 73, 75, 78.5, 84, 4],
['1:42', 70, 76, 77.0, 84, 2],
['1:43', 74, 71, 88.0, 102, 2],
['1:44', 10, 83, 55.5, 101, 4],
['1:45', 76, 75, 93.0, 110, 8],
['1:46', 12, 66, 48.5, 85, 8],
['1:47', 38, 60, 61.5, 85, 12],
['1:48', 39, 62, 59.5, 80, 13],
['1:49', 34, 57, 74.0, 114, 15],
['1:50', 48, 59, 91.5, 135, 14],
['1:51', 50, 67, 81.5, 113, 12],
['1:52', 39, 58, 85.0, 131, 12],
['1:53', 46, 49, 63.0, 80, 9],
['1:54', 34, 56, 59.5, 85, 6],
['1:55', 37, 46, 55.0, 73, 9],
['1:56', 17, 49, 51.5, 86, 9],
['1:57', 57, 49, 78.5, 100, 11],
['1:58', 53, 65, 74.0, 95, 12],
['1:59', 44, 50, 69.0, 94, 17],
['2:00', 40, 58, 71.5, 103, 12],
['2:01', 41, 56, 65.5, 90, 12]

  ];
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
