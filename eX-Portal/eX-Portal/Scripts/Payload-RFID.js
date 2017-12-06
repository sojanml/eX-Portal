var IsLoading = false;
var FlightPath = null;
var RFIDPath = null;
var BoundBox = null;
var Markers = [];
var GridLines = [];
var infowindow = null;
var _RFIDOverlay = {};
var _GPSPath = {};

$(document).ready(function () {
  initializeMap();
  PayloadRunsLoad();
  $(document).on("click", "li.RFIDRuns", PayloadRunsClick);
  $(document).on("click", "li.RFID", RFIDClick);
  $('#chkRawRFIDGPS').on("click", fnRawRFIDGPS_Click);
  $('#chkRawGPS').on("click", fnRawGPS_Click);
});

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

}

function PayloadRunsSuccess(TheData) {
  for (var i = 0; i < TheData.length; i++) {
    var data = TheData[i];
    var LI = $('<li class="RFIDRuns" data-UKey="' + data.FlightUniqueID + '">' +
      '<div class="c1">' + data.FlightUniqueID + '</div>' +
      '<div class="c2">' + data.RFIDCount + '</div>' +
      '<div class="c3">' + data.Grid + '</div>' +
      '<div class="c4">' + dtConvFromJSON(data.ReadDate, true) + '</div>' +
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


  for (var i = 0; i < GridLines.length; i++) {
    GridLines[i].setMap(null);
  }
  GridLines = [];

  var TheBoundPath = TheData.BoundBox;
  if (TheBoundPath != null) {
    ResetPath(BoundBox, TheBoundPath, true, true);
  }

  ResetPath(FlightPath, TheData.FlightPath, false, $('#chkRawGPS').is(':checked'));
  if(TheData.GridLines.length > 0) {
    _GPSPath.setMap(null);
  } else {
    ResetPath(_GPSPath, TheData.GPS, true, true);
  }

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
    var LI = $('<li class="RFID" id="' + data.RFID + '"' +
      ' data-Row="' + data.Row + '" data-Col="' + data.Col + '" ' + 
      ' data-UKey="' + data.FlightUniqueID + '" ' +
      ' data-RFID="' + data.RFID + '"' +
      ' data-index="' + i + '"' +
      '>' +
      '<div class="c1">' + data.RFID + '</div>' +
      '<div class="c2">' + data.ReadCount + '</div>' +
      '<div class="c3">' + data.Row + '</div>' +
      '<div class="c4">' + data.Col + '</div>' +
      '<div class="c3">' + data.Lat + '</div>' +
      '<div class="c4">' + data.Lng + '</div>' +      '</li>');
    $('#RFIDs').append(LI);
  }

  for (var i = 0; i < TheData.GridLines.length; i++) {
    var data = TheData.GridLines[i];
    var GridLine = new google.maps.Polyline({
      strokeColor: data.RowCol == 'R' ? '#00FF00' : '#00FF00',
      path: [data.StartPoint, data.EndPoint],
      strokeOpacity: 0.5,
      strokeWeight: 1,
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
      'style="left:' + (IconLocation.x - 50) + 'px; top:' + IconLocation.y + 'px;">' +
      this.Last5(RFID.RFID) +
      '<ul>' +
      '<li>Row x Col: ' + RFID.Row + ' x ' + RFID.Col + '</li>' +
      '<li>' + RFID.ReadCount + ' Reads</li>' +
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

