var IsLoading = false;
var FlightPath = null;
var RFIDPath = null;
var BoundBox = null;
var Markers = {};
var GridLines = [];
var infowindow = null;
var _RFIDOverlay = {};
var _GPSPath = {};
var _UniqueKey = "";
var _TimerPayloadRunsLoad = null;

var RfidData = {
  "55C14070C30D32E70C400000": { VIN: "1FD10625", Brand: "Jaguar", Model: "XE S", Make: "2017", Color: "Ultimate Black" },
  "55C14070C30D32E70DC00000": { VIN: "3E1532EA", Brand: "Jaguar", Model: "XF Saloon", Make: "2017", Color: "Glacier White" },
  "55C14070C30D32E70CC00000": { VIN: "B0116C0F", Brand: "Jaguar", Model: "XJ R-Sport", Make: "2017", Color: "Italian Racing Red" },
  "55C14070C30D32E70D400000": { VIN: "7DA447D0", Brand: "Jaguar", Model: "F-Type", Make: "2017", Color: "Polaris White" },
  "55C14070C30D32E39C400000": { VIN: "AAF23396", Brand: "Land Rover", Model: "Discovery", Make: "2017", Color: "Narvik Black" },
  "55C14070C30D32E70D800000": { VIN: "32FC77A5", Brand: "Land Rover", Model: "Range Rover Evoque", Make: "2017", Color: "Firenze Red" },
  "55C14070C30D32E39E400000": { VIN: "6693039E", Brand: "Land Rover", Model: "Discovery SVX", Make: "2017", Color: "Scotia Grey" },
  "55C14070C30D32E39D400000": { VIN: "80B8BEB1", Brand: "Land Rover", Model: "Range Rover", Make: "2017", Color: "Fuji White" },
  "55C14070C30D32E39CC00000": { VIN: "8808C8B1", Brand: "Land Rover", Model: "Range Rover Velar", Make: "2017", Color: "Loire Blue" },
  "55C14070C30D32E70E400000": { VIN: "E2A1C687", Brand: "Land Rover", Model: "Range Rover Sport", Make: "2017", Color: "Montalcino Red" },
};                                                                  

$(document).ready(function () {
  initializeMap();
  PayloadRunsLoad();
  $(document).on("click", "li.RFIDRuns", PayloadRunsClick);
  $(document).on("click", "li.RFID", RFIDClick);
  $('#chkRawRFIDGPS').on("click", fnRawRFIDGPS_Click);
  $('#chkRawGPS').on("click", fnRawGPS_Click);
  $('#chkAutoRefresh').on("click", fnchkAutoRefresh_Click);
});

function fnchkAutoRefresh_Click(e) {
  if ($('#chkAutoRefresh').prop('checked')) {
    PayloadRunsStart();
  } else {
    PayloadRunsStop();
  }

}

function PayloadRunsStart() {
  if (_TimerPayloadRunsLoad) window.clearTimeout(_TimerPayloadRunsLoad);
  _TimerPayloadRunsLoad = window.setTimeout(PayloadRunsLoad, 5000);
}
function PayloadRunsStop() {
  if (_TimerPayloadRunsLoad) window.clearTimeout(_TimerPayloadRunsLoad);
  _TimerPayloadRunsLoad = null;
}

function fnRawRFIDGPS_Click() {
  RFIDPath.setMap($(this).is(':checked') ? map : null);
}
function fnRawGPS_Click() {
  FlightPath.setMap($(this).is(':checked') ? map : null);
}

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

  _GPSPath = new google.maps.Polyline({
    strokeColor: '#00FF00',
    strokeOpacity: 1.0,
    strokeWeight: 3,
    map: map
  });


  FlightPath = new google.maps.Polyline({
    strokeColor: '#FF0000',
    strokeOpacity: 1.0,
    strokeWeight: 1,
    map:map
  });

  BoundBox = new google.maps.Polygon({
    strokeColor: '#00FF00',
    strokeOpacity: 0.5,
    strokeWeight: 1,
    fillColor: '#000000',
    fillOpacity: 0.2,
    map: map
  });  

  Markers['TopLeft'] = new google.maps.Marker({
    map: map,
    title: 'TopLeft',
    draggable: true
  });

  Markers['TopRight'] = new google.maps.Marker({
    map: map,
    title: 'TopRight',
    draggable: true
  });

  Markers['BottomRight'] = new google.maps.Marker({
    map: map,
    title: 'BottomRight',
    draggable: true
  });

  Markers['BottomLeft'] = new google.maps.Marker({
    map: map,
    title: 'BottomLeft',
    draggable: true
  });

  RFIDPath = new google.maps.Polyline({
    strokeColor: '#0000FF',
    strokeOpacity: 1.0,
    strokeWeight: 3,
    map: map
  });


  infowindow = new google.maps.InfoWindow({
    content: 'loading'
  });

  _RFIDOverlay = new RFIDOverlay({ map: map }, []);

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
  fnchkAutoRefresh_Click();
}

function PayloadRunsSuccess(TheData) {
  $('#PayloadRunIDs').empty();  

  var LI = $('<li>' +
    '<div class="c1">Ref ID</div>' +
    '<div class="c2">Count</div>' +
    '<div class="c3">Grid</div>' +
    '<div class="c4">Date</div>' +
    '</li>');
  $('#PayloadRunIDs').append(LI);

  for (var i = 0; i < TheData.length; i++) {
    var data = TheData[i];
    var ClassName = data.FlightUniqueID == _UniqueKey ?
      "active" :
      (_UniqueKey == "" && i == 0 ? "active" : "");    

    var LI = $('<li class="RFIDRuns ' + ClassName + '" data-UKey="' + data.FlightUniqueID + '">' +
      '<div class="c1">' + data.FlightUniqueID + '</div>' +
      '<div class="c2">' + data.RFIDCount + '</div>' +
      '<div class="c3">' + data.Grid + '</div>' +
      '<div class="c4">' + dtConvFromJSON(data.ReadDate, true) + '</div>' +
      '</li>');
    $('#PayloadRunIDs').append(LI);
  }

  $('li.RFIDRuns.active').trigger("click");
}

function PayloadRunsClick(TheObj) {
  if (IsLoading) return;
  IsLoading = true;
  var uKey = $(this).attr('data-UKey');
  _UniqueKey = $(this).index() > 1 ? uKey : "";

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


  for (var i = 0; i < GridLines.length; i++) {
    GridLines[i].setMap(null);
  }
  GridLines = [];

  var TheBoundPath = TheData.BoundBox;
  if (TheBoundPath != null) {
    ResetPath(BoundBox, TheBoundPath, true, true);
    Markers['TopLeft'].setPosition(TheBoundPath[0]);
    Markers['TopRight'].setPosition(TheBoundPath[1]);
    Markers['BottomRight'].setPosition(TheBoundPath[2]);
    Markers['BottomLeft'].setPosition(TheBoundPath[3]);
  }

  ResetPath(FlightPath, TheData.FlightPath, false, $('#chkRawGPS').is(':checked'));
  if(TheData.GridLines.length > 0) {
    _GPSPath.setMap(null);
  } else {
    ResetPath(_GPSPath, TheData.GPS, true, true);
  }

  $('#RFIDs').empty();
  var LI = $('<li >' +
    '<div class="c2">RFID</div>' +
    '<div class="c2">VIN</div>' +
    '<div class="c2">Make</div>' +
    '<div class="c3">Type</div>' +
    '<div class="c4">Year</div>' +
    '<div class="c5">Colour</div>' +
    '<div class="c6">Pos</div>' +
    '</li>');
  $('#RFIDs').append(LI);

  for (var i = 0; i < TheData.RFID.length; i++) {
    var data = TheData.RFID[i];
    var LI = $('<li class="RFID" id="' + data.RFID + '"' +
      ' data-Row="' + data.Row + '" data-Col="' + data.Col + '" ' + 
      ' data-UKey="' + data.FlightUniqueID + '" ' +
      ' data-RFID="' + data.RFID + '"' +
      ' data-index="' + i + '"' +
      '>' +
      '<div class="c2">' + data.RFID.substr(14,6) + '</div>' +
      '<div class="c2">' + (RfidData[data.RFID] ? RfidData[data.RFID].VIN : 'N/A') + '</div>' +
      '<div class="c2">' + (RfidData[data.RFID] ? RfidData[data.RFID].Brand : 'N/A') + '</div>' +
      '<div class="c3">' + (RfidData[data.RFID] ? RfidData[data.RFID].Model : 'N/A') + '</div>' +
      '<div class="c4">' + (RfidData[data.RFID] ? RfidData[data.RFID].Make : 'N/A') + '</div>' +
      '<div class="c3">' + (RfidData[data.RFID] ? RfidData[data.RFID].Color : 'N/A') + '</div>' +
      '<div class="c4">R' + data.Row + 'C' + data.Col + '</div>' +      '</li>');
    $('#RFIDs').append(LI);
  }

  for (var i = 0; i < TheData.GridLines.length; i++) {
    var data = TheData.GridLines[i];
    var GridLine = new google.maps.Polyline({
      strokeColor: data.RowCol == 'R' ? '#0000FF' : '#FFFF00',
      path: [data.StartPoint, data.EndPoint],
      strokeOpacity: 1,
      strokeWeight: 4,
      map: map
    });
    GridLines.push(GridLine);
  }

  _RFIDOverlay.setLocationData(TheData);

  IsLoading = false;
}



function fmMarkerClick() {
  var o = this;
  $('#' + o.rfid).trigger("click");
}

function RFIDClick(TheObj) {
  if (IsLoading) return;
  IsLoading = true;
  var uKey = $(this).attr('data-UKey');
  var RFID = $(this).attr('data-RFID');
  var Row = $(this).attr('data-Row');
  var Index = parseInt($(this).attr('data-index'));

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

  //infowindow.setContent(Markers[Index].title);
  //infowindow.open(map, Markers[Index]);
  
}

function RFIDSuccess(TheData) {
  ResetPath(RFIDPath, TheData, false, $('#chkRawRFIDGPS').is(':checked'));
  
  $('#RSSI').empty();
  for (var i = 0; i < TheData.length; i++) {
    var LI = $('<li>' + TheData[i].rssi + '</li>');
    $('#RSSI').append(LI);
  }
  

}

function ResetPath(ThePolyline, ThePath, isFitBounds, isShowOnMap) {
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
  if (isShowOnMap) {
    ThePolyline.setMap(map);
  }
  if (isFitBounds) {
    map.fitBounds(latlngbounds);
    map.panToBounds(latlngbounds);
  }

}

/*
************************************************************************************
*/


function RFIDOverlay(options, LocationData) {
  this.setValues(options);
  this.markerLayer = $('<div />').addClass('overlay');
  this.LocationData = {};
  
  this.setLocationData = function (LocationData) {
    this.LocationData = LocationData;
    this.draw();
  };

  this.Last5 = function (RFID) {
    var nRFID = RFID;
    while (1) {
      if (nRFID.substr(nRFID.length - 1, 1) === "0")
        nRFID = nRFID.substr(0, nRFID.length - 1)
      else
        break;
    }
    return nRFID.substr(nRFID.length - 5, 5);
  }
}

RFIDOverlay.prototype = new google.maps.OverlayView;

RFIDOverlay.prototype.onAdd = function () {
  var $pane = $(this.getPanes().overlayImage); // Pane 3  
  $pane.append(this.markerLayer);
};

RFIDOverlay.prototype.onRemove = function () {
  this.markerLayer.remove();
};

RFIDOverlay.prototype.draw = function () {
  var projection = this.getProjection();
  if (!projection) return false;
  if (!this.LocationData.RFID) return;
  this.markerLayer.empty();
  var IsGridMode = (this.LocationData.GridLines.length > 0);


  for (var i = 0; i < this.LocationData.RFID.length; i++) {
    var RFID = this.LocationData.RFID[i];
    var lat = RFID.Lat;
    var lng = RFID.Lng;
    var IconGeoPos = new google.maps.LatLng(lat, lng)
    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);

    var ID = 'rfid-' + RFID.PayLoadDataRFIDID;
    var MoreClass = (i == 0 ? 'First' : (i == this.LocationData.RFID.length - 1 ? "Last" : ""));
    var Layer = IsGridMode ?
      '<div id="' + ID + '" class="rfid-item ' + MoreClass + '" ' +
      'style="left:' + (IconLocation.x - 8) + 'px; top:' + (IconLocation.y - 8) + 'px;">' +
      '<span class="rfid-item-circle"></span>' + 
      '<ul>' +
      '<li class="c4">Pos: R' + RFID.Row + 'C' + RFID.Col +
      ", VIN: <b>" + (RfidData[RFID.RFID] ? RfidData[RFID.RFID].VIN : 'N/A') + '</b>' +
      '</li>' +
      '<li class="c2">' + (RfidData[RFID.RFID] ? RfidData[RFID.RFID].Brand : 'N/A') + 
      ' ' + (RfidData[RFID.RFID] ? RfidData[RFID.RFID].Model : 'N/A') + 
      ' [' + (RfidData[RFID.RFID] ? RfidData[RFID.RFID].Make : 'N/A') + "]" +
      '</li>' +
      '<li class="c4">' +
      'Make: ' + (RfidData[RFID.RFID] ? RfidData[RFID.RFID].Make : 'N/A') +
      ', Model: ' + (RfidData[RFID.RFID] ? RfidData[RFID.RFID].Color : 'N/A') +
      '</li>' +
      (MoreClass !== '' ? '<li>' + MoreClass + '<li>' : '') +
      '</ul>' +
      '</div>'
      :
      '<div id="' + ID + '" class="rfid-item-box ' + MoreClass + '" ' +
      'style="left:' + (IconLocation.x - 5) + 'px; top:' + (IconLocation.y - 5) + 'px;">' +
      '<div class="dot"></div>\n'+
      '<ul>' +
      '<li>' + this.Last5(RFID.RFID) + '</li>' +
      '<li>' + RFID.ReadCount + ' Reads</li>' +
      '</ul>' +
      '</div>'

    this.markerLayer.append(Layer);

  }

};

function getPos() {
  var PosArray =
    '(' +
    'TopLeftLat, TopLeftLon,' +
    'TopRightLat, TopRightLon,' +
    'BottomLeftLat, BottomLeftLon, ' +
    'BottomRightLat, BottomRightLon) VALUES (' +
    Markers['TopLeft'].getPosition().lat() + ',' + Markers['TopLeft'].getPosition().lng() + ',' +
    Markers['TopRight'].getPosition().lat() + ',' + Markers['TopRight'].getPosition().lng() + ',' +
    Markers['BottomLeft'].getPosition().lat() + ',' + Markers['BottomLeft'].getPosition().lng() + ',' +
    Markers['BottomRight'].getPosition().lat() + ',' + Markers['BottomRight'].getPosition().lng() + ')' +
    "\n\n\n" +

    'TopLeftLat=' + Markers['TopLeft'].getPosition().lat() + ',\n' +
    'TopLeftLon= ' + Markers['TopLeft'].getPosition().lng() + ',\n' +
    'TopRightLat= ' + Markers['TopRight'].getPosition().lat() + ',\n' +
    'TopRightLon= ' + Markers['TopRight'].getPosition().lng() + ',\n' +
    'BottomLeftLat= ' + Markers['BottomLeft'].getPosition().lat() + ',\n' +
    'BottomLeftLon= ' + Markers['BottomLeft'].getPosition().lng() + ',\n' +
    'BottomRightLat=' + Markers['BottomRight'].getPosition().lat() + ',\n' +
    'BottomRightLon= ' + Markers['BottomRight'].getPosition().lng();

    ;

  return PosArray;

}