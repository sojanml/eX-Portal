var Timers = {
  LoadMapData: null
};
var _FlightData = [];
var _FlightPath = null;
var _FlightCoordinates = [];
var _FlightDataID = 0;
var _FlightBoundBox = null;


$(document).ready(function () {
  initializeMap();
  InitChart();
  LoadMapData();
});

function initializeMap() {
  var MarkerPosition = { lat: 25.0955354, lng: 55.1527025 };

  var mapOptions = {
    zoom: 14,
    mapTypeControl: false,
    streetViewControl: false,
    center: MarkerPosition,
    mapTypeId: 'satellite'
  };

  map = new google.maps.Map(document.getElementById('flightmap'), mapOptions);
  map.setTilt(45);

  _FlightPath = new google.maps.Polyline({
    path: [],
    geodesic: true,
    strokeColor: '#FF0000',
    strokeOpacity: 1.0,
    strokeWeight: 2,
    map: map
  });
  _FlightBoundBox = new google.maps.LatLngBounds();
 
}

function LoadMapData() {
  $.ajax({
    type: "GET",
    url: '/FlightMap/Data/' + FlightID + '?FlightMapDataID=' + _FlightDataID,
    contentType: "application/json;charset=utf-8",
    dataType: "json",
    success: function (msg) {
      AddDataToMap(msg.Data);
      AddChart(msg.Data);
    },
    failure: function (msg) {
      alert('Live Drone Position Error' + msg);
    },
    complete: function (msg) {      
      if (Timers[LoadMapData]) window.clearTimeout(Timers[LoadMapData]);
      Timers[LoadMapData] = window.setTimeout(LoadMapData, 1000);
    }
  });
}

function AddDataToMap(TheData) {
  if (TheData.length <= 0) return;

  for (var i = 0; i < TheData.length; i++) {
    var LatLng = { lat: TheData[i].Lat, lng: TheData[i].Lng};
    _FlightCoordinates.push(LatLng);
    _FlightBoundBox.extend(LatLng);
  }

  var LastDataItem = TheData[TheData.length - 1];
  _FlightDataID = LastDataItem.FlightMapDataID;
  _FlightPath.setPath(_FlightCoordinates);

  map.fitBounds(_FlightBoundBox);
  ShowFlightInformation(LastDataItem);
}


function ShowFlightInformation(TheDataItem) {
  $('#FlightInfo_Altitude').html(TheDataItem.Altitude.toFixed(1));
  $('#FlightInfo_FlightDuration').html(toHour(TheDataItem.FlightDuration));
  $('#FlightInfo_Speed').html(TheDataItem.Speed.toFixed(2));
  $('#FlightInfo_Distance').html(TheDataItem.Distance.toFixed(2));
  $('#FlightInfo_FlightDate').html(dtConvFromJSON(TheDataItem.FlightTime, true));
}




function toHour(Seconds) {
  var Hours = Math.floor(Seconds / 60);
  var RestSeconds = Seconds % 60;
  var sSeconds = '0' + RestSeconds;
  return Hours + ':' + sSeconds.substring(sSeconds.length - 2);
}