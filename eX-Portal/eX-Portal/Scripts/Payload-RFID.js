var IsLoading = false;
var FlightPath = null;
var RFIDPath = null;
var BoundBox = null;
var Markers = [];

$(document).ready(function () {
  initializeMap();
  PayloadRunsLoad();
  $(document).on("click", "li.RFIDRuns", PayloadRunsClick);
  $(document).on("click", "li.RFID", RFIDClick);
});

function initializeMap() {
  var MarkerPosition = { "lat": 25.098205, "lng": 55.1562066667 };
  
  var mapOptions = {
    zoom: 20,
    mapTypeControl: true,
    streetViewControl: false,
    center: MarkerPosition,
    styles: getADSBMapStyle()
  };

  map = new google.maps.Map(document.getElementById('adsb_map'), mapOptions);

  FlightPath = new google.maps.Polyline({
    strokeColor: '#FF0000',
    strokeOpacity: 1.0,
    strokeWeight: 1,

  });

  BoundBox = new google.maps.Polygon({
    strokeColor: '#00FF00',
    strokeOpacity: 0.5,
    strokeWeight: 1
  });  

  RFIDPath = new google.maps.Polyline({
    strokeColor: '#0000FF',
    strokeOpacity: 1.0,
    strokeWeight: 3
  });

  
}


function PayloadRunsLoad() {
  var URL = '/PayLoad/runs';
  var AJAX = $.ajax({
    url: URL,
    type: 'GET',
    success: PayloadRunsSuccess, //succes
    complete: function () {
    }
  });//$.ajax

}

function PayloadRunsSuccess(TheData) {
  for (var i = 0; i < TheData.length; i++) {
    var data = TheData[i];
    var LI = $('<li class="RFIDRuns" data-UKey="' + data.FlightUniqueID + '">' +
      '<div class="c1">' + data.FlightUniqueID + '</div>' +
      '<div class="c2">' + data.RFIDCount + '</div>' +
      '<div class="c3">' + dtConvFromJSON(data.ReadDate, true) + '</div>' +
      '</li>');
    $('#PayloadRunIDs').append(LI);
  }
}

function PayloadRunsClick(TheObj) {
  if (IsLoading) return;
  IsLoading = true;
  var uKey = $(this).attr('data-UKey');
  $('#PayloadRunIDs').find('li.active').removeClass('active');
  $(this).addClass('active');

  $('#RFIDs').html('<li>Loading the key ' + uKey + '...</li>');
  $('#RSSI').empty();

  var URL = '/PayLoad/Data/' + uKey;
  var AJAX = $.ajax({
    url: URL,
    type: 'GET',
    success: RFIDLoadSuccess, //succes
    complete: function () {
    }
  });//$.ajax

}

function RFIDLoadSuccess(TheData) {

  //clear the path
  RFIDPath.setMap(null);

  //Clear the markers
  for (var i = 0; i < Markers.length; i++) {
    Markers[i].setMap(null);
  }
  Markers = [];

  var TheBoundPath = TheData.BoundBox;
  if (TheBoundPath != null) {
    ResetPath(BoundBox, TheBoundPath, true);
  }

  ResetPath(FlightPath, TheData.FlightPath);
  $('#RFIDs').empty();
  var LI = $('<li >' +
    '<div class="c1">RFID</div>' +
    '<div class="c2">Read Count</div>' +
    '<div class="c3">Row</div>' +
    '<div class="c4">Col</div>' +
    '<div class="c5">Lat</div>' +
    '<div class="c6">Lng</div>' +
    '</li>');
  $('#RFIDs').append(LI);

  for (var i = 0; i < TheData.RFID.length; i++) {
    var data = TheData.RFID[i];
    var LI = $('<li  data-UKey="' + data.FlightUniqueID + '" class="RFID" data-RFID="' + data.RFID + '">' +
      '<div class="c1">' + data.RFID + '</div>' +
      '<div class="c2">' + data.ReadCount + '</div>' +
      '<div class="c3">' + data.Row + '</div>' +
      '<div class="c4">' + data.Col + '</div>' +
      '<div class="c3">' + data.Lat + '</div>' +
      '<div class="c4">' + data.Lng + '</div>' +      '</li>');
    $('#RFIDs').append(LI);

    if (data.Lat > 0 && data.Lng > 0){
      var Pos = new google.maps.LatLng(data.Lat, data.Lng);

      var marker = new google.maps.Marker({
        position: Pos,
        map: map,
        title: '# ' + i + ' - Row: ' + data.Row + ', Col: ' + data.Col
      });
      Markers.push(marker);
    }

  }

  IsLoading = false;
}

function RFIDClick(TheObj) {
  if (IsLoading) return;
  IsLoading = true;
  var uKey = $(this).attr('data-UKey');
  var RFID = $(this).attr('data-RFID');

  $('#RFIDs').find('li.active').removeClass('active');
  $(this).addClass('active');

  var URL = '/PayLoad/RFID/' + uKey + '?RFID=' + RFID;
  var AJAX = $.ajax({
    url: URL,
    type: 'GET',
    success: RFIDSuccess, //succes
    complete: function () {
      IsLoading = false;
    }
  });//$.ajax

}

function RFIDSuccess(TheData) {
  ResetPath(RFIDPath, TheData);

  $('#RSSI').empty();
  for (var i = 0; i < TheData.length; i++) {
    var LI = $('<li>' + TheData[i].rssi + '</li>');
    $('#RSSI').append(LI);
  }

}

function ResetPath(ThePolyline, ThePath, isFitBounds) {
  ThePolyline.setMap(null);
  var OldPath = ThePolyline.getPath();
  isFitBounds = isFitBounds || false;

  while(OldPath.length > 0) {
    OldPath.removeAt(0);
  }

  var latlngbounds = new google.maps.LatLngBounds();

  for (var i = 0; i < ThePath.length; i++) {
    data = ThePath[i];
    var lat = data.lat ? data.lat : data.Lat;
    var lng = data.lng ? data.lng : data.Lng;
    if (lat > 0 && lng > 0) {
      var Pos = new google.maps.LatLng(lat, lng);
      OldPath.push(Pos);
      if (isFitBounds) latlngbounds.extend(Pos);
    }
    
  }
  //ThePolyline.setPoints(ThePath);
  ThePolyline.setMap(map);
  if (isFitBounds) {
    map.fitBounds(latlngbounds);
    map.panToBounds(latlngbounds);
  }

}