var map;
var _Location = [];
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
var defaultZoom = 15;
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
var mytimer = null;
var service = new google.maps.DirectionsService();
var path = new google.maps.MVCArray();
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
              + '<th>Altitude</th><th>Speed</th>'
              + '<th>FixQuality</th><th>Satellite</th>'
              + '<th>ReadTime</th><th>Pitch</th>'
              + '<th>RollData</th><th>Heading</th>'
              + '<th>Total Flight Time</th></tr></thead>';
  var firsttr = '<tr style="display:none"><td></td><td></td>'
             + '<td></td><td></td>'
             + '<td></td><td></td>'
             + '<td></td><td></td>'
             + '<td></td>'
             + '<td></td><td></td></tr>';
  $('#MapData table').append(loctr);
  $('#MapData table').append(firsttr);
  $('#MapData table').addClass('report');
  GetDrones();
  //  myInterval = setInterval(function () { }, 500);

};

function GetDrones() {
  var _locVal = [];
  $.ajax({
    type: "GET",
    url: MapDataURL + "&LastFlightDataID=" + LastDroneDataID,
    contentType: "application/json;charset=utf-8",
    dataType: "json",
    async: true,
    success: function (msg) {
      //   try {
      _Location = [];
      msg = msg.hasOwnProperty('d') ? msg.d : msg;
      $.each(msg, function (index, obj) {
        //$.each(obj, function (key, value) {
        //  _locVal.push(value)
        //});
        _Location.push(obj);
        //_locVal = [];
      });
      LastDatas = _Location;
      setMarker(map, _Location);
      SetCurrentValues(_Location);
      SetMapTable(_Location);
      // }
      //catch (err) {
      //    alert('Live Drone Position Error' + err);
      //}
    },
    failure: function (msg) {
      alert('Live Drone Position Error' + msg);
    },
    complete: function (msg) { mytimer = setTimeout(function () { GetDrones(); }, 1000); }
  });

}

function setMarker(map, location) {
  for (var i = 0; i < location.length; i++) {
    var loc = location[i];
    var body = '' +
        '<br/>Drone&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp' + loc['FlightMapDataID'] +
        '<br/>DroneID&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp' + loc['DroneRFID'] +
        '<br/>Address&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp&nbsp';
    var myLatLng = new google.maps.LatLng(loc['Latitude'], loc['Longitude']);
    var marker = createMarker(map, myLatLng, loc['DroneRFID'], body, i);
  }
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
  clearTimeout(mytimer);
  MyLastLatLong = null;
  MyLastMarker = null;
  myLatLong = null;
  LastDroneDataID = 0;
  deleteMarkers();
  removeLines();
  GetDrones();
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

function SetCurrentValues(location) {
  var _LastValue = [];
  var date;
  if (location.length > 0) {
    _LastValue = location[location.length - 1];
    LastDroneDataID = _LastValue['FlightMapDataID'];
    /*
    $('#Lat').html(_LastValue['Latitude']);
    $('#Lon').html(_LastValue['Longitude']);
    $('#Alt').html(_LastValue['Altitude']);
    $('#Speed').html(_LastValue['Speed']);
    $('#FxQlty').html(_LastValue['FixQuality']);
    $('#Satellite').html(_LastValue['Satellites']);
    date = new Date(parseInt(_LastValue['ReadTime'].substr(6)));
    $('#DrTime').html(date.toDateString());
    $('#Pitch').html(_LastValue['Pitch']);
    $('#Roll').html(_LastValue['Roll']);
    $('#Head').html(_LastValue['Heading']);
    $('#TotFlightTime').html(_LastValue['TotalFlightTime']);
    */

    $.each(_LastValue, function (key, value) {
      if (key == "ReadTime") {
        var theDate = new Date(parseInt(value.substr(6)));
        value = theDate.toDateString();
      }
      $('#data_' + key).html(value);
    });

    MyLastLatLong = new google.maps.LatLng(_LastValue['Latitude'], _LastValue['Longitude']);
  }


  // var oCompaniesTable = $('#MapData Table')
}
function SetMapTable(location) {
  if (location.length > 0) {
    for (var i = 0; i < 15; i++) {
      var _LastValue = location[i];
      if (_LastValue != null) {
        var tLatData = '<td>' + _LastValue['Latitude'] + '</td>';
        var tLonData = '<td>' + _LastValue['Longitude'] + '</td>';
        var tAltData = '<td>' + _LastValue['Altitude'] + '</td>';
        var tSpeedData = '<td>' + _LastValue['Speed'] + '</td>';
        var tFxQltyData = '<td>' + _LastValue['FixQuality'] + '</td>';
        var tSatelliteData = '<td>' + _LastValue['Satellites'] + '</td>';
        var date = new Date(parseInt(_LastValue['ReadTime'].substr(6)));
        var tDrTime = '<td>' + date.toDateString() + '</td>';
        var tPitchData = '<td>' + _LastValue['Pitch'] + '</td>';
        var tRollData = '<td>' + _LastValue['Roll'] + '</td>';
        var tHeadData = '<td>' + _LastValue['Heading'] + '</td>';
        var tTotFlightTimeData = '<td>' + _LastValue['TotalFlightTime'] + '</td>';
        var loctr = '<tr>' + tLatData + tLonData + tAltData + tSpeedData + tFxQltyData + tSatelliteData + tDrTime + tPitchData + tRollData + tHeadData + tTotFlightTimeData + '</tr>';
        $('#MapData table > tbody > tr:first').after(loctr);
        if ($("#MapData > table > tbody > tr").length > 20)
          $('#MapData > table > tbody > tr:last').remove();
      }
    }
    //get the last item
    var _LastValue = location[location.length - 1];
    $('#map-info').html('Lat: ' + _LastValue['Latitude'] + ', Lon: ' + _LastValue['Longitude']);


  }//if (location.length > 0)

}