﻿var MaxRecords = 2000;
var map;
var _Location = [];
var _SavedLocation = [];
var isAutoQuery = true;

var PlotTimer = null;
var PlotTimerDelay = 500;
var isReplayMode = false;
var mytimer = null;

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
var HiddenPoly;
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
var FirstTotalFlightTime = -100;
var isGraphReplayMode = false;
var LastDrawPoint = null;
var lineChartStore = [];
var lineChartLegend = new Object();

$(document).ready(function () {
  initialize();
  setPolygon();

  $('#chkShowFullPath').on("change", function (e) {
    if (this.checked) {
      HiddenPoly.setMap(map);
    } else {
      HiddenPoly.setMap(null);
    }
  })
});

var BoundaryBox = null;
var FillOptions = [
  { fillColor: '#55FF55', strokeWeight: 0, fillOpacity: 0.2 },
  { fillColor: 'yellow', strokeWeight: 1, fillOpacity: 0 }
];

function setPolygon() {
  for (var i = 0; i < AllowedLocation.length; i++) {
    var paths = AllowedLocation[i];
    BoundaryBox = new google.maps.Polygon({
      paths: paths,
      strokeOpacity: 1,
      strokeColor: 'red',
      strokeWeight: FillOptions[i]['strokeWeight'],
      fillColor: FillOptions[i]['fillColor'],
      fillOpacity: FillOptions[i]['fillOpacity'],
      editable: false,
      draggable: false
    });
    BoundaryBox.setMap(map);
  }
}


function getBoundary() {
  var Bounds = BoundaryBox.getPath().getArray();
  var LatLng = '';
  for (var i = 0; i < Bounds.length; i++) {
    if (LatLng != '') LatLng += ',';
    LatLng = LatLng + Bounds[i].lat() + ' ' + Bounds[i].lng()
  }
  return LatLng;
}
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
    strokeOpacity:1,
    strokeWeight: 2
  });
  poly.setMap(map);

  HiddenPoly = new google.maps.Polyline({
    strokeColor: '#000000',
    strokeOpacity: 0.5,
    strokeWeight: 2
  });
  HiddenPoly.setMap(map);

  var loctr = '<thead>' + 
              '<tr>' 
              + '<th>ReadTime (UTC)</th>'
              + '<th>Latitude</th>'
              + '<th>Longitude</th>'
              + '<th>Altitude (m)</th'
              + '><th>Speed (m/s)</th>'
              + '<th>FixQuality</th>'
              + '<th>Satellite</th>'
              + '<th>Pitch</th>'
              + '<th>RollData</th>'
              + '<th>Heading</th>'
              + '<th>Volt</th>'
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
    var Speed = TheObj['Speed'];
    Speed = parseInt(Speed * 100, 0);
    var delay = Speed * PlotTimerDelay;
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
    url: MapDataURL +
      "&LastFlightDataID=" + LastDroneDataID +
      '&MaxRecords=' + MaxRecords +
      '&Replay=' + (isReplayMode ? 1 : 0),
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
    var thisPointRaw = _Location.shift();
    if (LastDrawPoint != null &&
      LastDrawPoint["Latitude"] == thisPointRaw["Latitude"] &&
      LastDrawPoint["Longitude"] == thisPointRaw["Longitude"]) {
      //nothing
    } else {
      setMarker(map, thisPointRaw);
    }
    thisPoint = SetCurrentValues(thisPointRaw);
    SetMapTable(thisPoint);
    LastDrawPoint = thisPointRaw;
  }
  if (!isGraphReplayMode) {
    GetChartData();
    lineChart.initialize(data);
  }

}

function plotPoints() {
  if (PlotTimer) window.clearTimeout(PlotTimer);
  if (_Location.length < 1) return;

  var thisPointRaw = _Location.shift();
  if (LastDrawPoint != null &&
    LastDrawPoint["Latitude"] == thisPointRaw["Latitude"] &&
    LastDrawPoint["Longitude"] == thisPointRaw["Longitude"]) {
    //nothing
  } else {
    setMarker(map, thisPointRaw);
  }

  thisPoint = SetCurrentValues(thisPointRaw);
  SetMapTable(thisPoint);
  LastDrawPoint = thisPointRaw;
  var delay = getDelay(thisPointRaw);
  PlotTimer = window.setTimeout(plotPoints, delay);

}


function setMarker(map, loc) {

  var iDt = parseInt(loc['ReadTime'].substr(6));
  var theDate = new Date(iDt);
  var FormatDate = fmtDt(theDate);
  

  var body = 'Lat: <b>' + loc['Latitude'] + '</b>, Lng:  <b>' + loc['Longitude'] + "</b><br>" +
  "Time (UTC): <b>" + FormatDate + '</b>';
  var myLatLng = new google.maps.LatLng(loc['Latitude'], loc['Longitude']);
  var marker = createMarker(map, myLatLng, loc['DroneRFID'], body, i, loc['IsOutSide']);
}


function createMarker(map, latlng, heading, body, zindex, IsOutSide) {
  if (poly.map == null) { addLines(); }
  var path = poly.getPath();
  if (path && path.length >= 20) {
    var LastItem = path.removeAt(0);
    var HiddenPath = HiddenPoly.getPath();
    if (!HiddenPath.length) HiddenPath.push(LastItem);

    //get the next item and add to hidden path
    var FirstItem = path.getAt(0);
    HiddenPath.push(FirstItem);

  }
  path.push(latlng);
  var image = '/bullet_red.png';
  var marker = new google.maps.Marker({
    position: latlng,
    map: map,
    icon: image,
    title: heading,
    zIndex: 9999
  });
  if (MyLastMarker != null) {
    if (IsOutSide) {
      //keep red
    } else {
      MyLastMarker.setIcon('/bullet_blue.png');
    }
  }
  MyLastMarker = marker;
  map.setCenter(latlng);
  closeMargin = '120px';
  livemarkers.push(marker);
  if (livemarkers[0] != null)
    livemarkers[0].setIcon('/bullet_green.png');
  google.maps.event.addListener(marker, 'click', function () {
    var infowindow = new google.maps.InfoWindow({
      content: body
    });
    infowindow.open(map, marker);
  });


  //set opacity of marker
  var initOpacity = 1;
  for (var i = livemarkers.length - 1; i > 0; i--) {
    if (initOpacity < 0) initOpacity = 0;
    livemarkers[i].setOpacity(initOpacity);
    initOpacity = initOpacity - 0.05;
    if (initOpacity < 0) return;
  }
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

  path = new google.maps.MVCArray();
  HiddenPoly.setPath(path);
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
      case "voltage":
        value = parseFloat(value);
        if (isNaN(value)) value = 0;
        //if (value > 0) value = value / (60 * 60) * 1000;
        value = value.toFixed(2);
        break;
      case "TotalFlightTime":
        value = parseFloat(value);
        if (isNaN(value)) value = 0;
        //First time only
        if (FirstTotalFlightTime < 0) {
          FirstTotalFlightTime = value;
        }
        //Set the offset for flightime
        value = value - FirstTotalFlightTime;
        if (value > 0) value = value / 60;
        value = value.toFixed(3);

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
    var vdate = new Date(_LastValue['ReadTime']);
    var theDate = _LastValue['ReadTime'];
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
    var loctr = '<tr>' +
    tDrTime +
    tLatData +
    tLonData +
    tAltData +
    tSpeedData +
    tFxQltyData +
    tSatelliteData +
    tPitchData +
    tRollData +
    tHeadData +
    tTotFlightTimeData +
    '<td>' + _LastValue['voltage'] + '</td>' +
    '</tr>';
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
  var day = date.getDate();
  var hours = date.getUTCHours();
  var minutes = date.getUTCMinutes();
  var seconds = date.getSeconds();
  var Months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  //var ampm = hours >= 12 ? 'pm' : 'am';
  //hours = hours % 12;
  //hours = hours ? hours : 12; // the hour '0' should be '12'
  seconds = seconds < 10 ? '0' + seconds : seconds;
  hours = hours < 10 ? '0' + hours : hours;
  minutes = minutes < 10 ? '0' + minutes : minutes;
  day = day < 10 ? '0' + day : day;
  var strTime = hours + ':' + minutes + ':' + seconds;
  var strDate = day + "-" + Months[date.getMonth()] + "-" + date.getFullYear();
  return strDate + " " + strTime;
}

function SetChart() {
  //init legend first
  lineChartLegend = {
    'Altitude': true,
    'Satellites': true,
    'Pitch': true,
    'Roll': true,
    'Speed': true
  };

  GetChartData();
  var ctx = $("#myChart").get(0).getContext('2d');
  ctx.canvas.height = 300;  // setting height of canvas
  ctx.canvas.width = 1000; // setting width of canvas
  lineChart = new Chart(ctx).Line(data, {
    bezierCurve: true,
    datasetFill: false,
    animateScale: false,
    // String - Template string for single tooltips
    tooltipTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].datasetLabel){%><%=datasets[i].value%><%}%></li><%}%></ul>",
    //String - A legend template
    multiTooltipTemplate: "<%= datasetLabel %> : <%= value %>",
    legendTemplate:
  '<ul id=\"line-legend">\n' +
  '  <% for (var i=0; i<datasets.length; i++){%>\n' +
  '  <li class="active" onclick="updateDataset($(this))" data-label="<%=datasets[i].label%>">\n' +
  '    <span class="legend" style=\"background-color:<%=datasets[i].strokeColor%>\">' +
  '     <span class="icon">&#xf00c;</span>\n' +
  '    </span>\n' +
  '    <span><%=datasets[i].label%></span>\n' +
  '  </li>\n' +
  '  <%}%>\n' +
  '</ul>'
  });
  var legend = lineChart.generateLegend();
  $('#map-legent').append(legend);


}



function GetChartData() {
  var ChartDataSet = [];
  if (lineChartLegend['Altitude']) ChartDataSet.push({
    label: "Altitude",
    strokeColor: "rgb(236, 215, 101)",
    pointColor: "rgb(236, 215, 101)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgb(236, 215, 101)",
    data: aDatasets1
  });

  if (lineChartLegend['Satellites']) ChartDataSet.push({
    label: "Satellites",
    strokeColor: "rgba(151,187,205,1)",
    pointColor: "rgba(151,187,205,1)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgba(151,187,205,1)",
    data: aDatasets2
  });

  if (lineChartLegend['Pitch']) ChartDataSet.push({
    label: "Pitch",
    strokeColor: "rgba(255,119,119,1)",
    pointColor: "rgba(255,119,119,1)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgba(255,119,119,1)",
    data: aDatasets3
  });

  if (lineChartLegend['Roll']) ChartDataSet.push({
    label: "Roll",
    strokeColor: "rgb(153, 131, 199)",
    pointColor: "rgb(153, 131, 199)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgb(153, 131, 199)",
    data: aDatasets4
  });

  if (lineChartLegend['Speed']) ChartDataSet.push({
    label: "Speed",
    strokeColor: "rgb(117, 237, 251)",
    pointColor: "rgb(117, 237, 251)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgba(187,17,17,1)",
    data: aDatasets5
  });

  data = {
    labels: aCopy(aLabels),
    datasets: ChartDataSet
  };
  return data;
}

function aCopy(theArray) {
  var nArray = [];
  for (var i = 0; i < theArray.length; i++)
    nArray.push(theArray[i]);
  return nArray;
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
  str = aData['ReadTime'].substr(aData['ReadTime'].indexOf(' ') + 1);
  //str = aData["TotalFlightTime"];

  if (isGraphReplayMode) {
    if (lineChart.datasets[0].points.length > 20) lineChart.removeData();
    var LineData = [];
    if (lineChartLegend['Altitude']) LineData.push(aData['Altitude']);
    if (lineChartLegend['Satellites']) LineData.push(aData['Satellites']);
    if (lineChartLegend['Pitch']) LineData.push(aData['Pitch']);
    if (lineChartLegend['Roll']) LineData.push(aData['Roll']);
    if (lineChartLegend['Speed']) LineData.push(aData['Speed']);

    lineChart.addData(LineData, str);
  } //else {
    aLabels.push(str);
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
  //}
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



function updateDataset(legendLi) {
  var label = legendLi.attr("data-label");
  var exists = false;
  if (lineChartLegend[label]) {
    lineChartLegend[label] = false;
    legendLi.fadeTo("slow", 0.33);
  } else {
    lineChartLegend[label] = true;
    legendLi.fadeTo("slow", 1);
  }

  GetChartData();
  lineChart.initialize(data);
}