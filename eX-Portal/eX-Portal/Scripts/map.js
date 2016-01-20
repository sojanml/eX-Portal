var MaxRecords = 2000;
var map;
var _Location = [];
var PlotTimer = null;
var PlotTimerDelay = 100;
var isReplayMode = false;
var mytimer = null;

var _truckName = [];
var _viGroupName = [];
var _truck;
var _vigroup;
var _truckIcon;
var _viGroupTruckArry = [];
var _viGroupTruckIconArry = [];
var _vigroupValue;
var initLat = 24.9899106;
var initLng = 55.0034188;
var defaultZoom = 18;
var gpsGrpID;
var timezone;
var clientID;
var programName;

var livemarkers = [];
var datemarkers = [];
var secureDateMarker = [];
var historymarkers = [];
var allHistorymarkers = [];
var InfoMarker = null;
var trafficLayer = new google.maps.TrafficLayer();
var contentString;
var browser = false;
var enableRefreshIntervalID;
var pickupStatus = false;
var truckStatus = false;
var geocoder;
var LastDroneDataID = 0;
var INTERVAL = 100;
var poly;
var TimerValue = 0;
var LastDatas = [];
var i = 0;
var myInterval;
var myLatLong = [];
var MyLastLatLong = null;
var MyLastMarker = null;
var service = new google.maps.DirectionsService();
var path = new google.maps.MVCArray();
var aData = [];
var aLabels = [];
var aDatasets1 = [];
var aDatasets2 = [];
var aDatasets3 = [];
var aDatasets4 = [];
var aDatasets5 = [];
var data = [];
var lineChart = null;
var ldata = [];

var isGraphReplayMode = false;


$(document).ready(function () {

  initialize();
});

/*

[{
00 : "FlightMapDataID":18860,
01 : "DroneId":1,
02 : "DroneRFID":null,
03 : "Latitude":44.35260,
04 : "Longitude":-98.21462,
05 : "ProductRFID":null,
06 : "ProductQrCode":null,
07 : "ProductRSSI":null,
08 : "ReadTime":"\/Date(1450152653000)\/",
09 : "CreatedTime":null,
10 : "RecordType":null,
11 : "IsActive":null,
12 : "ProductId":null,
13 : "Altitude":2.00000,
14 : "Speed":3.00000,
15 : "FixQuality":4.00000,
16 : "Satellites":10,
17 : "Pitch":0.00000,
18 : "Roll":0.00000,
19 : "Heading":0.00000,
20 : "TotalFlightTime":100,
21 : "FlightID":22,
22 : "BBFlightID":"10",
23 : "avg_Altitude":2.00000,
24 : "Min_Altitude":2.00000,
25 : "Max_Altitude":2.00000,
26 : "Avg_Speed":3.00000,
27 : "Min_Speed":3.00000,
28 : "Max_Speed":3.00000,
29 : "Avg_Satellites":10.00000,
30 : "Min_Satellites":10.00000,
31 : "Max_Satellites":10.00000,
32 : "PointDistance":null,
33 : "Distance":0.00000
}]
*/

function initialize() {
  geocoder = new google.maps.Geocoder();

  var mapOptions = {
    zoom: defaultZoom,
    center: new google.maps.LatLng(initLat, initLng),
    panControl: false,
    mapTypeControl: true,
    mapTypeControlOptions: {
      position: google.maps.ControlPosition.RIGHT_TOP,
    },
    zoomControl: true,
    zoomControlOptions: {
      style: google.maps.ZoomControlStyle.LARGE,
      position: google.maps.ControlPosition.LEFT_TOP,
    },
    scaleControl: false,
    streetViewControl: true,
    overviewMapControl: false,

    mapTypeId: google.maps.MapTypeId.HYBRID
  };
  map = new google.maps.Map(document.getElementById('map_canvas'),
      mapOptions);

  poly = new google.maps.Polyline({
    strokeColor: '#000000',
    strokeOpacity: 1.0,
    strokeWeight: 3
  });
  poly.setMap(map);
  var loctr = '<thead><tr><th>Latitude</th><th>Longitude</th>'
              + '<th>Altitude (m)</th><th>Speed (m/s)</th>'
              + '<th>FixQuality</th><th>Satellite</th>'
              + '<th>ReadTime (UTC)</th><th>Pitch</th>'
              + '<th>RollData</th><th>Heading</th>'
              + '</tr></thead>';
  var firsttr = '<tr style="display:none"><td></td><td></td>'
             + '<td></td><td></td>'
             + '<td></td><td></td>'
             + '<td></td><td></td>'
             + '<td></td><td></td></tr>';
  $('#MapData table').append(loctr);
  $('#MapData table').append(firsttr);
  $('#MapData table').addClass('report');
  // setLocation();
  SetChart();
  GetDrones();

  //  myInterval = setInterval(function () { }, 500);

};

function setLocation() {
  if (navigator.geolocation) {
    browserSupportFlag = true;
    navigator.geolocation.getCurrentPosition(function (position) {
      initialLocation = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
      map.setCenter(initialLocation);
    }, function () {
      //initialLocation = new google.maps.LatLng(initLat, initLng),
      //map.setCenter(initialLocation);
    });
  }

}

function getDelay(TheObj) {
  if (TheObj == null) return PlotTimerDelay;
  if (isReplayMode) {
    var delay = TheObj['Speed'] * 5000;
    if (delay < PlotTimerDelay) delay = PlotTimerDelay;
    return delay;
  } else {
    return PlotTimerDelay
  }
}


function GetDrones() {
  var _locVal = [];
  $.ajax({
    type: "GET",
    url: MapDataURL + "&LastFlightDataID=" + LastDroneDataID + '&MaxRecords=' + MaxRecords,
    contentType: "application/json;charset=utf-8",
    dataType: "json",
    async: true,
    success: function (msg) {
      //   try {

      msg = msg.hasOwnProperty('d') ? msg.d : msg;
      $.each(msg, function (index, obj) {
        _Location.push(obj);
        LastDroneDataID = obj['FlightMapDataID'];
      });


      if (isReplayMode) {
        plotPoints();
      } else {
        directPlotPoints();
      }
      //LastDatas = _Location;

      // }
      //catch (err) {
      //    alert('Live Drone Position Error' + err);
      //}

    },
    failure: function (msg) {
      alert('Live Drone Position Error' + msg);
    },
    complete: function (msg) {
      if (mytimer) window.clearTimeout(mytimer);
      mytimer = window.setTimeout(GetDrones, 1000);
    }
  });

}


function directPlotPoints() {
  var locationLength = _Location.length;
  if (locationLength < 1)
    isGraphReplayMode = true;
  while (1) {
    if (_Location.length < 1) break;
    var thisPoint = _Location.shift();

    setMarker(map, thisPoint);
    thisPoint = SetCurrentValues(thisPoint);
    SetMapTable(thisPoint);

  }
  if (!isGraphReplayMode) {
    GetChartData();
    lineChart.initialize(data);
  }



}

function plotPoints() {
  if (PlotTimer) window.clearTimeout(PlotTimer);
  if (_Location.length < 1) return;

  var thisPoint = _Location.shift();


  setMarker(map, thisPoint);
  thisPoint = SetCurrentValues(thisPoint);
  SetMapTable(thisPoint);

  var delay = getDelay(thisPoint);
  PlotTimer = window.setTimeout(plotPoints, delay);

}


function setMarker(map, loc) {
  var body = '' +
      '<br/>Drone&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp' + loc['FlightMapDataID'] +
      '<br/>DroneID&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp' + loc['DroneRFID'] +
      '<br/>Address&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp';
  var myLatLng = new google.maps.LatLng(loc['Latitude'], loc['Longitude']);
  var marker = createMarker(map, myLatLng, loc['DroneRFID'], body, i);
}


function createMarker(map, latlng, heading, body, zindex) {
  if (poly.map == null)
  { addLines(); }
  var path = poly.getPath();

  path.push(latlng);
  var image = '/red.png';
  var marker = new google.maps.Marker({
    position: latlng,
    map: map,
    icon: image,
    title: heading,
    zIndex: 9999
  });
  if (MyLastMarker != null) {
    MyLastMarker.setIcon('/bullet_blue.png');
  }
  MyLastMarker = marker;
  map.setCenter(latlng);
  closeMargin = '120px';
  livemarkers.push(marker);
  if (livemarkers[0] != null)
    livemarkers[0].setIcon('/bullet_green.png');
  google.maps.event.addListener(marker, 'click', function () { });
}

function deleteMarkers() {
  clearMarkers(null);
  livemarkers = [];
}



function Replay() {
  //mytimer = null;
  _Location = [];
  if (PlotTimer) window.clearTimeout(PlotTimer);
  PlotTimer = null;
  isReplayMode = true;
  MaxRecords = 20;

  clearTimeout(mytimer);
  MyLastLatLong = null;
  MyLastMarker = null;
  myLatLong = null;
  LastDroneDataID = 0;
  deleteMarkers();
  removeLines();
  GetDrones();
  ClearChartValues();
  GetChartData();
  lineChart.initialize(data);


  // addLines();
}
function addLines() {
  path = new google.maps.MVCArray();
  poly.setPath(path);
  poly.setMap(map);
}

function clearMarkers(map) {
  for (var i = 0; i < livemarkers.length; i++) {
    livemarkers[i].setMap(map);
  }

}
function removeLines() {
  poly.setMap(null);
  poly.latLngs.clear();
  // path = [];
  //    setTimeout()
  // flightPath.setMap(null);
}

function SetCurrentValues(_LastValue) {
  var date;

  $.each(_LastValue, function (key, value) {
    if (value == null) value = '';
    switch (key) {
      case "ReadTime":
        var iDt = parseInt(_LastValue['ReadTime'].substr(6));
        var theDate = new Date(iDt);
        value = fmtDt(theDate);
        break;
      case "Distance":
        value = parseInt(value);
        if (isNaN(value)) value = 0;
        break;
      case "avg_Altitude":
      case "Min_Altitude":
      case "Max_Altitude":
      case "Altitude":
        value = parseFloat(value);
        if (isNaN(value)) value = 0;
        value = value.toFixed(2);
        break;
      case "Speed":
      case "Avg_Speed":
      case "Min_Speed":
      case "Max_Speed":
        value = parseFloat(value);
        if (isNaN(value)) value = 0;
        //if (value > 0) value = value / (60 * 60) * 1000;
        value = value.toFixed(2);
        break;
      case "TotalFlightTime":
        value = parseFloat(value);
        if (isNaN(value)) value = 0;
        if (value > 0) value = value / (60 * 60);
        value = value.toFixed(2);
        break;
      case "Heading":
        value = parseFloat(value);
        if (isNaN(value)) value = 0;
        if (value < 0) value = value + 360;
        value = value.toFixed(2);
        break;


    }
    _LastValue[key] = value;
    $('#data_' + key).html(value);
  });

  MyLastLatLong = new google.maps.LatLng(_LastValue['Latitude'], _LastValue['Longitude']);
  return _LastValue;
  // var oCompaniesTable = $('#MapData Table')
}

function SetMapTable(_LastValue) {

  if (_LastValue != null) {
    var date = new Date(parseInt(_LastValue['ReadTime'].substr(6)));
    var theDate = fmtDt(date)
    var tLatData = '<td>' + _LastValue['Latitude'] + '</td>';
    var tLonData = '<td>' + _LastValue['Longitude'] + '</td>';
    var tAltData = '<td>' + _LastValue['Altitude'] + '</td>';
    var tSpeedData = '<td>' + _LastValue['Speed'] + '</td>';
    var tFxQltyData = '<td>' + _LastValue['FixQuality'] + '</td>';
    var tSatelliteData = '<td>' + _LastValue['Satellites'] + '</td>';
    var tDrTime = '<td>' + theDate + '</td>';
    var tPitchData = '<td>' + _LastValue['Pitch'] + '</td>';
    var tRollData = '<td>' + _LastValue['Roll'] + '</td>';
    var tHeadData = '<td>' + _LastValue['Heading'] + '</td>';
    var tTotFlightTimeData = '';// '<td>' + _LastValue['TotalFlightTime'] + '</td>';
    var loctr = '<tr>' + tLatData + tLonData + tAltData + tSpeedData + tFxQltyData + tSatelliteData + tDrTime + tPitchData + tRollData + tHeadData + tTotFlightTimeData + '</tr>';
    $('#MapData table > tbody > tr:first').after(loctr);

    //  SetChart();
    // if ($("#MapData > table > tbody > tr").length <= 2)
    //    {
    // SetChartData(_LastValue);
    // SetChart();
    //  }
    //  else
    // {
    SetChartUpdateData(_LastValue);
    //  updateChart();
    //}
    if ($("#MapData > table > tbody > tr").length > 20)
      $('#MapData > table > tbody > tr:last').remove();
  }

  //get the last item
  $('#map-info').html(
    theDate + ', ' +
    'Lat: ' + _LastValue['Latitude'] + ', Lon: ' + _LastValue['Longitude']);
}

function fmtDt(date) {
  if (date instanceof Date) {

  } else {
    return 'Invalid';
  }
  var hours = date.getHours();
  var minutes = date.getMinutes();
  var Months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  //var ampm = hours >= 12 ? 'pm' : 'am';
  //hours = hours % 12;
  //hours = hours ? hours : 12; // the hour '0' should be '12'
  minutes = minutes < 10 ? '0' + minutes : minutes;
  var strTime = hours + ':' + minutes + ':' + date.getSeconds();
  return date.getDate() + "-" + Months[date.getMonth()] + "-" + date.getFullYear() + "  " + strTime;
}

function SetChart() {
  GetChartData();
  var ctx = $("#myChart").get(0).getContext('2d');
  ctx.canvas.height = 300;  // setting height of canvas
  ctx.canvas.width = 1000; // setting width of canvas
  lineChart = new Chart(ctx).Line(data, {
    bezierCurve: false,
    datasetFill: false,
    animateScale: false,
    // String - Template string for single tooltips
    tooltipTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].datasetLabel){%><%=datasets[i].value%><%}%></li><%}%></ul>",
    //String - A legend template
    multiTooltipTemplate: "<%= datasetLabel %> : <%= value %>",
    legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].datasetLabel){%><%=datasets[i].value%><%}%></li><%}%></ul>"
  });
}

function updateChart() {
  if (lineChart != null) //lineChart.addData(ldata, aLabels);
    lineChart.addData(ldata["datasets"], [ldata["labels"], 10, 20, 30]);
}


function GetChartData() {

  data = {
    labels: aLabels,
    datasets: [{
      label: "Altitudes",
      strokeColor: "rgb(236, 215, 101)",
      pointColor: "rgb(236, 215, 101)",
      pointStrokeColor: "#fff",
      pointHighlightStroke: "rgb(236, 215, 101)",
      data: aDatasets1
    },
    {
      label: "Satellites",
      strokeColor: "rgba(151,187,205,1)",
      pointColor: "rgba(151,187,205,1)",
      pointStrokeColor: "#fff",
      pointHighlightStroke: "rgba(151,187,205,1)",
      data: aDatasets2
    },
    {
      label: "Pitch",
      strokeColor: "rgba(255,119,119,1)",
      pointColor: "rgba(255,119,119,1)",
      pointStrokeColor: "#fff",
      pointHighlightStroke: "rgba(255,119,119,1)",
      data: aDatasets3
    },
    {
      label: "Roll",
      strokeColor: "rgb(153, 131, 199)",
      pointColor: "rgb(153, 131, 199)",
      pointStrokeColor: "#fff",
      pointHighlightStroke: "rgb(153, 131, 199)",
      data: aDatasets4
    },
    {
      label: "Speed",
      strokeColor: "rgb(117, 237, 251)",
      pointColor: "rgb(117, 237, 251)",
      pointStrokeColor: "#fff",
      pointHighlightStroke: "rgba(187,17,17,1)",
      data: aDatasets5
    }]
  };




}

function SetChartUpdateData(response) {
  //ClearChartValues();
  aData = response;

  // aLabels = aData['ReadTime'];
  var date = new Date(parseInt(aData['ReadTime'].substr(6)));
  var hours = date.getHours();
  var minutes = date.getMinutes();
  var seconds = date.getSeconds();
  var str = "";
  str += hours + ":" + minutes + ":" + seconds + " ";

  if (isGraphReplayMode) {
    if (lineChart.datasets[0].points.length > 20)
      lineChart.removeData();
    lineChart.addData([aData['Altitude'], aData['Satellites'], aData['Pitch'], aData['Roll'], aData['Speed']], str);
  } else {

    aLabels.push((str).toString());
    aDatasets1.push(aData['Altitude']);
    aDatasets2.push(aData['Satellites']);
    aDatasets3.push(aData['Pitch']);
    aDatasets4.push(aData['Roll']);
    aDatasets5.push(aData['Speed']);
    if (aLabels.length > 20) {
      aLabels.shift();
      aDatasets1.shift();
      aDatasets2.shift();
      aDatasets3.shift();
      aDatasets4.shift();
      aDatasets5.shift();
    }

  }
  /*
  ldata = {
      labels: aLabels,
      datasets: [{
          label: "Altitudes",
          strokeColor: "rgba(220,220,220,1)",
          pointColor: "rgba(220,220,220,1)",
          pointStrokeColor: "#fff",
          pointHighlightStroke: "rgba(220,220,220,1)",
          data: aDatasets1
      },
      {
          label: "Satellites",
          strokeColor: "rgba(151,187,205,1)",
          pointColor: "rgba(151,187,205,1)",
          pointStrokeColor: "#fff",
          pointHighlightStroke: "rgba(151,187,205,1)",
          data: aDatasets2
      },
      {
          label: "Pitch",
          strokeColor: "rgba(255,119,119,1)",
          pointColor: "rgba(255,119,119,1)",
          pointStrokeColor: "#fff",
          pointHighlightStroke: "rgba(255,119,119,1)",
          data: aDatasets3
      },
      {
          label: "Roll",
          strokeColor: "rgba(100,50,205,1)",
          pointColor: "rgba(100,50,205,1)",
          pointStrokeColor: "#fff",
          pointHighlightStroke: "rgba(100,50,205,1)",
          data: aDatasets4
      },
      {
          label: "Speed",
          strokeColor: "rgba(187,17,17,1)",
          pointColor: "rgba(187,17,17,1)",
          pointStrokeColor: "#fff",
          pointHighlightStroke: "rgba(187,17,17,1)",
          data: aDatasets5
      }]
  };
  */
}

function ClearChartValues() {
  aDatasets1 = [];
  aData = [];
  aDatasets2 = [];
  aDatasets3 = [];
  aDatasets4 = [];
  aDatasets5 = [];
  ldata = [];
  aLabels = [];
}

function OnErrorCall_(repo) {
  alert("Woops something went wrong, pls try later !");
}

