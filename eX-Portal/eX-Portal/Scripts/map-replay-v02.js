var MaxRecords = 2000;
var map;
var Now = new Date();
var LatestLine = null;
var OldLine = null;
var playerInstance = null;
var thePlayTimer = null;
var _IsGeoTagShown = false;
var _GeoTagLayer = null;
var _IsADSBShown = null;
var _ADSBLayer = null;
var HomePoint = null;
var _BlackBoxStartAt = null;
var _ReplayTimeAt = null;
var _VideoStartTime = new Date(1970, 0, 0, 24, 0, 0);
var _IsDrawInitilized = false;
var _OtherFlights = {};
var _FlightOra = null
var ReplayInterval = 1000;
var DottedLine = [];
var NoFlyZone = null;
var gADSBData = {};
var ADSBTimer = null;
var _ADSBAnimationSec = 5;
var _ADSBAnimationTimer = null;
var _ADSBTimerCount = 0;

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


var DistanceOptions = {
  Radius: 50,
  Critical: 100,
  Warning: 500
};

var GeoInfoWindow = null;
var PayloadLayer = null;
var _IsPlayloadShown = false;
var DronePositionIcon = false;


$(document).ready(function () {

  initializeMap();

  var KmlUrl = 'http://test.exponent-ts.com/Map/NoFlyzone?R=' + Math.random();
  var kmlOptions = {
    preserveViewport: true,
    map: map
  };
  NoFlyZone = new google.maps.KmlLayer(KmlUrl, kmlOptions);
  NoFlyZone.setValues({ map: map });
  setAllowedRegion();
  //drawLocationPoints();
  initilizeTable();
  initilizeChart();


  getLocationPoints();
  GeoInfoWindow = new google.maps.InfoWindow();

  $('#chkShowFullPath').on("change", function (e) {
    if (this.checked) {
      OldLine.setMap(map);
    } else {
      OldLine.setMap(null);
    }
  });

  google.maps.event.addListener(GeoInfoWindow, 'closeclick', function () {
    ADSBLine.Hide();
  });

  $('#ReplaySpeed').on("change", function (e) {
    ReplayInterval = parseInt($(this).val(), 0);
    startReplayTimer();
  });

  $('#btnGeoTag').on("click", function (e) {
    ShowHideGeoTag($(this));
  });
  $('#btnADSB').on("click", function (e) {
    ShowHideADSB($(this));
  });

  $(document).on("click", ".map-point", function () {
    fn_MapPoint($(this));
  });

  $(document).on("click", ".adsb-point", function () {
    fn_AdsbPoint($(this));
  });

  $('#btnPayload').on("click", function () {
    ShowHidePayload($(this));
  });

});


function fn_MapPoint(thisObj) {
  var Lat = thisObj.attr("data-lat");
  var Lng = thisObj.attr("data-lng");
  var Doc = thisObj.attr("data-doc");
  var Title = thisObj.attr("data-ident");
  var Alt = thisObj.attr("data-alt");

  var Center = new google.maps.LatLng(Lat, Lng);
  GeoInfoWindow.setPosition(Center);
  var TD_Tumb = '';
  if (Doc == '') {
    var Thump = '/Upload/Drone/' + DroneName + '/' + FlightID + '/' + Doc.replace(".jpg", ".t.png");
    var DocURL = '/Upload/Drone/' + DroneName + '/' + FlightID + '/' + Doc;
    TD_Tumb = '<td><a target="_blank" href="' + DocURL + '"><img style="margin-right:10px; width:80px; height: auto;" src="' + Thump + '"></a></td>';

  }
  if (Title != '') {
    Title = '<b style="color:red">' + Title + '</b><br>';
  }
  var Content =
  '<table cellpadding=0 cellspacig=0>' +
  '<tr>' +
  TD_Tumb +
  '<td style="white-space:nowrap;">' +
  Title +
  'Lat: <b>' + Lat + "</b><br>" +
  "Lng: <b>" + Lng + "</b><br>" +
  "Alt: <b>" + Alt + "</b>" +
  "</td>" +
  '</tr></table>'

  GeoInfoWindow.setContent(Content);
  GeoInfoWindow.open(map);
}


function fn_AdsbPoint(thisObj) {
  var Lat = parseFloat(thisObj.attr("data-lat"));
  var Lng = parseFloat(thisObj.attr("data-lng"));
  var Doc = thisObj.attr("data-doc");
  var FlightID = Title = thisObj.attr("data-ident");
  var Alt = thisObj.attr("data-alt");
  var Center = new google.maps.LatLng(Lat, Lng);
  var TD_Tumb = '';
  Alt = parseInt(Alt);

  ADSBLine.FlightID = FlightID;
  ADSBLine.setABSPos(Lat, Lng, Alt);

  Alt = (isNaN(Alt) ? 0 : Alt * 1) + ' feet';

  if (Doc == '') {
    var Thump = '/Upload/Drone/' + DroneName + '/' + FlightID + '/' + Doc.replace(".jpg", ".t.png");
    var DocURL = '/Upload/Drone/' + DroneName + '/' + FlightID + '/' + Doc;
    TD_Tumb = '<td><a target="_blank" href="' + DocURL + '"><img style="margin-right:10px; width:80px; height: auto;" src="' + Thump + '"></a></td>';
  }
  if (Title != '') {
    Title = '<b style="color:red">' + Title + '</b><br>';
  }
  var Content =
  '<table cellpadding=0 cellspacig=0>' +
  '<tr>' +
  TD_Tumb +
  '<td style="white-space:nowrap;" valign="top">' +
  Title +
  'Lat: <b>' + Lat.toFixed(4) + "</b><br>" +
  "Lng: <b>" + Lng.toFixed(4) + "</b><br>" +
  "Alt: <b>" + Alt + "</b><br>" +
  '</td><td valign="top" style="padding-left:20px;">' +
  '<b style="color:green">Separation</b><br>\n' +
  "Distance : <b>" + ADSBLine.getDistance() + ' KM</b><br>\n' +
  "Height: <b>" + ADSBLine.getAltDiff() + " feet</b>" +
  "</td>" +
  '</tr></table>'

  GeoInfoWindow.setPosition(Center);
  GeoInfoWindow.setContent(Content);

  ADSBLine.Show();

  GeoInfoWindow.open(map);
}

function ShowHidePayload(btn) {
  if (btn.length) btn.val("Loading...");


  if (_IsPlayloadShown) {
    PayloadLayer.setValues({ map: null });
    _IsPlayloadShown = false;
    if (btn.length) btn.val("Show Payload");
    return;
  }

  if (PayloadLayer) {
    PayloadLayer.setValues({ map: map });
    _IsPlayloadShown = true;
    if (btn.length) btn.val("Hide Payload");
    return;
  }

  var kmlOptions = {
    preserveViewport: false,
    map: map
  };

  var KmlUrl = '/Map/PayloadData/' + FlightID;
  //var KmlUrl = 'http://test.exponent-ts.com/Upload/Temp/aaa-01.xml';
  PayloadLayer = new google.maps.KmlLayer(KmlUrl, kmlOptions);
  if (btn.length) btn.val("Hide Payload");
  _IsPlayloadShown = true;
}

function ShowHideGeoTag(btn) {
  if (btn.length) btn.val("Loading...");

  if (_IsGeoTagShown) {
    _GeoTagLayer.setValues({ map: null });
    _IsGeoTagShown = false;
    if (btn.length) btn.val("Show Geo Tag");
    return;
  }

  if (_GeoTagLayer) {
    _GeoTagLayer.setValues({ map: map });
    _IsGeoTagShown = true;
    if (btn.length) btn.val("Hide Geo Tag");
    return;
  }

  $.ajax({
    type: "GET",
    url: '/Map/GeoTag/' + FlightID,
    contentType: "application/json;charset=utf-8",
    dataType: "json",
    async: true,
    success: function (GeoTagData) {
      _IsGeoTagShown = true;
      _GeoTagLayer = new GeoTagOverlay({ map: map }, GeoTagData);
      if (btn.length) btn.val("Hide Geo Tag");
    },
    failure: function (msg) {
      alert('Geo tag load error' + msg);
    }
  });


}

function ShowHideADSB(btn) {
  if (btn.length) btn.val("Loading...");
  _ADSBTimerCount = 0;

  if (_IsADSBShown) {
    _ADSBLayer.setValues({ map: null });
    _IsADSBShown = false;
    if (btn.length) btn.val("Show ADS/B Data");
    GeoInfoWindow.close();
    ADSBLine.Hide();
    if (_ADSBAnimationTimer) window.clearInterval(_ADSBAnimationTimer);
    _ADSBAnimationTimer = null;
    ShowHideRPAS(true);
    return;
  }

  if (_ADSBLayer) {
    _ADSBLayer.setValues({ map: map });
    _IsADSBShown = true;
    if (btn.length) btn.val("Hide ADS/B Data");
    GetADSBData(0, 0);  /*getLocalADSB(); */
    _ADSBAnimationTimer = window.setInterval(ADSBAnimation, _ADSBAnimationSec * 1000);
    ShowHideRPAS(false);
    return;
  }

  _ADSBLayer = new ADSBOverlay({ map: map }, []);
  GetADSBData(0, 0);  /*getLocalADSB(); */
  if (btn.length) btn.val("Hide ADSB Data");
  _ADSBAnimationTimer = window.setInterval(ADSBAnimation, _ADSBAnimationSec * 1000);
  ShowHideRPAS(false);

}

function ShowHideRPAS(isShowRPAS) {

  var MapObj = isShowRPAS ? map : null;
  LatestLine.setMap(MapObj);
  OldLine.setMap(MapObj);
  DronePositionIcon.setMap(MapObj);
  for (var i = 0; i < _AllMarkers.length; i++) {
    _AllMarkers[i].setMap(MapObj);
  }
  for (var i = 0; i < DottedLine.length; i++) {
    DottedLine[i].setMap(MapObj);
  }
  for (var i = 0; i < _Boundary.length; i++) {
    _Boundary[i].setMap(MapObj);
  }
  

}

function ADSBAnimation() {
  _ADSBLayer.draw();
}

function LiveADSData() {
  if (!_IsADSBShown) return;
  _ADSBTimerCount++;
  if (_ADSBTimerCount > (6 * 5)) {
    _IsADSBShown = true;
    ShowHideADSB($('#btnADSB'));
    return;
  }
  //var RangeLat = (HomePoint["Latitude"] - 1).toString() + ' ' + HomePoint["Latitude"].toString();
  //var RangeLon = (HomePoint["Longitude"]).toString() + ' ' + (HomePoint["Longitude"] + 1).toString();
  GetADSBData(0, 0);  /*getLocalADSB(); */
}



function getLocalADSB() {
  var loc = _LocationPoints[_LocationIndex];
  var RequestURL =
    '/map/getadsbdata?' +
    'ID=0' + /*loc['FlightMapDataID'] +*/
    '&Lat=' + loc['Latitude'] +
    '&Lng=' + loc['Longitude'];

  $.ajax({
    type: "GET",
    url: RequestURL,
    contentType: "application/json",
    success: function (result) {
      if (result.error) {
        //alert('Failed to fetch flight: ' + result.error);
        return;
      }
      _IsADSBShown = true;
      _ADSBLayer.setADSB(result);
      if (ADSBTimer) window.clearTimeout(ADSBTimer);
      ADSBTimer = window.setTimeout(LiveADSData, 60 * 1000);
    },
    error: function (data, text) {
      //alert('Failed to fetch flight: ' + data);
    }
  });
}


function xx_GetADSBData(RangeLat, RangeLon) {
  console.log("ADSB Timer Count: " + _ADSBTimerCount);
  RangeLat = '23.85438 26.39335';
  RangeLon = '52.99612 59.31375';

  var query = '{> alt 5} ' +
            '{> speed 200} ' +
            '{true inAir} ' +
            '{range lat ' + RangeLat + '} ' +
            '{range lon +' + RangeLon + '}';
  var Offset = 100;

  var fxml_url = 'http://catheythattil:9f8b8719108bdaa973fe4a96fef5646cd3fd32ea@flightxml.flightaware.com/json/FlightXML2/';
  $.ajax({
    type: "GET",
    url: fxml_url + 'SearchBirdseyeInFlight',
    contentType: "application/json;charset=utf-8",
    data: {
      'query': query,
      'howMany': 30,
      'offset': 0
    },
    success: function (result) {
      if (result.error) {
        alert('Failed to fetch flight: ' + result.error);
        return;
      }

      _IsADSBShown = true;
      //_ADSBLayer = new ADSBOverlay({ map: map }, result.SearchBirdseyeInFlightResult.aircraft);
      _ADSBLayer.setADSB(result.SearchBirdseyeInFlightResult.aircraft);
      //_ADSBLayer.setMap(map);
      if (ADSBTimer) window.clearTimeout(ADSBTimer);
      ADSBTimer = window.setTimeout(LiveADSData, 10 * 1000);
    },
    error: function (data, text) { alert('Failed to fetch flight: ' + data); },
    dataType: 'jsonp',
    jsonp: 'jsonp_callback',
    xhrFields: { withCredentials: true }
  });

}

function GetADSBData(RangeLat, RangeLon) {
  $.ajax({
    url: '/adsb/',
    success: function (result) {
      _IsADSBShown = true;
      //_ADSBLayer = new ADSBOverlay({ map: map }, result.SearchBirdseyeInFlightResult.aircraft);
      _ADSBLayer.setADSB(result);
      //_ADSBLayer.setMap(map);
      if (ADSBTimer) window.clearTimeout(ADSBTimer);
      ADSBTimer = window.setTimeout(LiveADSData, 5 * 1000);
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
  this.setADSB = function (ADSBData) {
    this.ADSBData = ADSBData;
    this.ResetDraw = true;
    this.MovePointsToSecond = 0;
    this.draw();

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
  if (!projection) return;

  var zoom = this.getMap().getZoom();
  var fragment = document.createDocumentFragment();
  var IsAdded = false;
  var ThisKeys = {};


  var ThisCounter = this.DrawCount++;

  for (var i = 0; i < this.ADSBData.length; i++) {
    var lat = this.ADSBData[i].Lat;
    var lng = this.ADSBData[i].Lon;
    var alt = this.ADSBData[i].Altitude;
    var title = this.ADSBData[i].FlightID.trim();
    var heading = this.ADSBData[i].Heading;
    heading = parseFloat(heading);
    if (isNaN(heading)) heading = 0;

    // Determine a random location from the bounds set previously  
    var IconGeoPos = new google.maps.LatLng(lat, lng)
    lat = IconGeoPos.lat();
    lng = IconGeoPos.lng();

    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);
    var DivID = 'adsb-' + title;
    var IconColor = getIconColor(alt);

    if (heading == 0) {
      //Landed flight - Ignore movement
    } else if (gADSBData[DivID]) {
      var $point = $('#' + DivID);
      $point.animate({ left: IconLocation.x, top: IconLocation.y });
      $point.find(".icon").css({ transform: 'rotate(' + (heading - 45) + 'deg)', color: IconColor });
      $point.attr({ 'data-lat': lat, 'data-lng': lng, 'data-alt': alt });
    } else {
      var data = {
        id: DivID,
        title: title,
        lat: lat,
        lng: lng,
        alt: alt,
        x: IconLocation.x,
        y: IconLocation.y,
        heading: (heading - 45),
        IconColor: IconColor
      };
      var $point = getADSBIcon(data);
      // Append the HTML to the fragment in memory  
      fragment.appendChild($point.get(0));
      IsAdded = true;
    }

    gADSBData[DivID] = ThisCounter;

    if (ADSBLine.FlightID == title) {
      ADSBLine.setABSPos(lat, lng, alt);
      ADSBLine.Show();
      $('#' + DivID).trigger("click");
    }

  }//for (var i = 0)

  //if the item does not exists in 20 request, then remove it from the screen
  var AllKeys = Object.keys(gADSBData);
  for (i = 0; i < AllKeys.length; i++) {
    var TheKey = AllKeys[i];
    if (gADSBData[TheKey] != ThisCounter) {
      $('#' + TheKey).fadeOut();
      delete gADSBData[TheKey];
    }
  }//for (var key in gADSBData)


  // Now append the entire fragment from memory onto the DOM  
  if(IsAdded) this.markerLayer.append(fragment);
};


function getADSBIcon(data) {
  var ThePoint = '';
  if (data.title.substr(0, 3) == 'DRN') {
    ThePoint = $(
        '<div  class="adsb-point" id="' + data.id + '" title="' + data.title + '" '
      + 'data-lat="' + data.lat + '" '
      + 'data-lng="' + data.lng + '" '
      + 'data-alt="' + data.alt + '" '
      + 'data-ident="' + data.title + '" '
      + 'style="left:' + data.x + 'px; top:' + data.y + 'px;">'
      + '<span class="icon FlightIcon" style=" transform: rotate(' + (data.heading) + 'deg); color: ' + data.IconColor + '">' 
      + '<img class="green" src="/images/map/drone-vector.svg" width="50" height="50">'
      + '</span>'
      + '<span class="flight-title" style="">' + data.title + '</span>' +
      + '</div>'
    );
  } else {
  ThePoint = $(
      '<div  class="adsb-point" id="' + data.id + '" title="' + data.title + '" '
    + 'data-lat="' + data.lat + '" '
    + 'data-lng="' + data.lng + '" '
    + 'data-alt="' + data.alt + '" '
    + 'data-ident="' + data.title + '" '
    + 'style="left:' + data.x + 'px; top:' + data.y + 'px;">'
    + '<span class="icon FlightIcon" style=" transform: rotate(' + (data.heading) + 'deg); color: ' + data.IconColor + '">&#xf072;</span>'
    + '<span class="flight-title" style="">' + data.title + '</span>' +
    + '</div>'
  );
  }
  return ThePoint;
}

function GeoTagOverlay(options, GeoTagData) {
  this.setValues(options);
  this.markerLayer = $('<div />').addClass('overlay');
  this.GeoTagData = GeoTagData;
};
GeoTagOverlay.prototype = new google.maps.OverlayView;

GeoTagOverlay.prototype.onAdd = function () {
  var $pane = $(this.getPanes().overlayImage); // Pane 3  
  $pane.append(this.markerLayer);
};

GeoTagOverlay.prototype.onRemove = function () {
  this.markerLayer.remove();
};

GeoTagOverlay.prototype.draw = function () {
  var projection = this.getProjection();
  var zoom = this.getMap().getZoom();
  var fragment = document.createDocumentFragment();


  this.markerLayer.empty(); // Empty any previous rendered markers  

  for (var i = 0; i < this.GeoTagData.length; i++) {
    // Determine a random location from the bounds set previously  
    var IconGeoPos = new google.maps.LatLng(
            this.GeoTagData[i].Latitude,
            this.GeoTagData[i].Longitude
    );

    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);
    var $point = $(
        '<div class="map-point" id="p' + i + '" title="' + i + '" '
      + 'data-lat="' + this.GeoTagData[i].Latitude + '" '
      + 'data-lng="' + this.GeoTagData[i].Longitude + '" '
      + 'data-alt="' + this.GeoTagData[i].Altitude + '" '
      + 'data-doc="' + this.GeoTagData[i].DocumentName + '" '
      + 'style="left:' + IconLocation.x + 'px; top:' + IconLocation.y + 'px;">'
      + '<span class="icon GeoTagIcon">&#xf03e;</span>'
      + '<span class="icon GeoTagPoint">&#xf0d7;</span>'
      + '</div>'
    );

    // Append the HTML to the fragment in memory  
    fragment.appendChild($point.get(0));
  }

  // Now append the entire fragment from memory onto the DOM  
  this.markerLayer.append(fragment);
};


//run the replay of map;
function Replay() {
  //clear all maps
  for (var i = 0; i < _AllMarkers.length; i++) {
    _AllMarkers[i].setMap(null);
  }
  _AllMarkers = [];
  _ZoomBounds = new google.maps.LatLngBounds();
  LatestLine.getPath().clear();
  OldLine.getPath().clear();
  ClearDottedLine();
  //Clear the Map Data Table
  $('#MapData').html('<table class="report" id="TableMapData"></table>');
  initilizeTable();

  //Clear the Graph
  drawLineChart(1);

  _ReplayTimeAt = new Date(_LocationPoints[0].ReadTime);
  setMapInfo();

  //Stop the Video
  if (playerInstance) playerInstance.stop();


  //Reset the date to start time
  var iDt = _LocationPoints[0]['ReadTime'];
  _ReplayTimeAt = new Date(iDt);
  _ElapsedTime = 0;
  //_BlackBoxStartAt = _ReplayTimeAt;
  //_BlackBoxStartAt.setMinutes(_BlackBoxStartAt.getMinutes() + Now.getTimezoneOffset());

  //Start the timer
  _LocationIndex = -1;
  _IsLiveMode = false;
  startReplayTimer();

}


function getLocationPoints() {
  $.ajax({
    type: "GET",
    url: MapDataURL +
      "&LastFlightDataID=" + _LastDroneDataID +
      '&MaxRecords=' + MaxRecords +
      '&Replay=0',
    contentType: "application/json;charset=utf-8",
    dataType: "json",
    async: true,
    success: function (msg) {
      msg = msg.hasOwnProperty('d') ? msg.d : msg;
      $.each(msg, function (index, obj) {
        _LocationPoints.push(obj);
        _LastDroneDataID = obj['FlightMapDataID'];
      });
      //if (_IsLiveMode) MaxRecords = 1;
      //if any pending records try to get that as well
      if (_IsLiveMode && msg.length <= 20) {
        if (!_IsDrawInitilized) {
          setDrawIntilize();
        } else {
          window.setTimeout(getLocationPoints, 1000);
        }
      } else {//if (_IsLiveMode)
        if (msg.length > 0) {
          getLocationPoints();
        } else {
          setDrawIntilize();
        }
      }//if (_IsLiveMode)

    },
    failure: function (msg) {
      alert('Live Drone Position Error' + msg);
    }
  });
}

function setDrawIntilize() {



  if (_LocationPoints.length > 1) {
    var loc = _LocationPoints[0];
    var myLatLng = new google.maps.LatLng(loc['Latitude'], loc['Longitude']);
    var iDt = parseInt(loc['ReadTime'].substr(6));
    _BlackBoxStartAt = new Date(iDt);
    _BlackBoxStartAt.setMinutes(_BlackBoxStartAt.getMinutes() + Now.getTimezoneOffset());
    map.setCenter(myLatLng);
    map.setZoom(20);
    drawLocationPoints();
    _IsDrawInitilized = true;
    ADSBLine.setStart(loc['Latitude'], loc['Longitude'], loc['Altitude']);

    var image = {
      path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
      strokeColor: '#FF2A6A',
      strokeWidth: '10px',
      scale: 6,
      rotation: parseInt(loc["Heading"])
    };


    var MarkerImage = {
      path: 'M74.068 1.542 C 50.101 6.596,28.753 21.235,15.530 41.681 C 13.004 45.587,10.938 49.031,10.938 49.334 C 10.937 49.638,9.753 52.460,8.306 55.607 C -4.657 83.785,-0.974 117.319,18.200 145.703 C 21.361 150.383,30.820 160.209,35.531 163.707 C 37.242 164.977,39.262 166.543,40.021 167.188 C 41.757 168.661,47.798 172.161,52.148 174.214 C 61.700 178.722,62.752 179.078,75.781 182.210 C 80.332 183.304,105.703 183.345,109.766 182.265 C 126.537 177.805,132.045 175.642,140.289 170.275 C 145.372 166.965,146.094 166.692,146.094 168.078 C 146.094 168.567,147.152 170.688,148.445 172.790 C 157.832 188.045,157.457 210.365,147.519 227.930 C 144.824 232.692,145.139 232.566,141.940 230.162 C 127.517 219.321,102.489 213.535,80.919 216.055 C 16.746 223.552,-19.197 291.458,10.130 349.790 C 11.909 353.327,13.785 356.667,14.300 357.212 C 14.816 357.757,16.184 359.660,17.341 361.441 C 24.833 372.973,51.129 392.969,58.801 392.969 C 59.496 392.969,60.247 393.264,60.470 393.626 C 60.851 394.241,64.810 395.381,75.000 397.806 C 77.647 398.435,84.097 398.816,91.797 398.797 C 155.172 398.641,198.203 338.943,178.779 278.125 C 176.481 270.929,170.671 258.850,168.158 256.042 C 166.547 254.242,167.479 253.541,176.647 249.659 C 192.353 243.009,209.975 242.856,221.334 249.272 C 222.111 249.711,224.841 251.134,227.400 252.435 C 230.404 253.961,231.858 255.126,231.501 255.720 C 231.197 256.226,229.762 258.507,228.312 260.788 C 223.879 267.759,219.694 278.313,217.100 289.063 C 214.722 298.915,214.793 317.377,217.249 327.805 C 219.725 338.319,227.127 355.760,230.738 359.587 C 231.236 360.115,232.582 361.953,233.729 363.672 C 237.498 369.317,252.285 383.594,254.364 383.594 C 254.838 383.594,255.832 384.263,256.572 385.080 C 263.129 392.326,289.075 400.000,307.015 400.000 C 376.506 400.000,420.193 328.386,388.815 265.910 C 386.879 262.054,384.736 258.436,384.053 257.870 C 383.371 257.303,382.813 256.480,382.813 256.039 C 382.813 254.055,367.901 238.186,363.306 235.281 C 362.218 234.593,360.098 233.080,358.594 231.918 C 355.203 229.299,342.314 222.656,340.622 222.656 C 339.936 222.656,339.041 222.325,338.633 221.921 C 337.137 220.439,321.245 217.255,312.891 216.764 C 293.598 215.629,270.707 221.373,257.589 230.640 C 254.546 232.790,253.751 232.202,250.535 225.431 C 242.028 207.518,242.224 194.132,251.270 175.124 L 254.743 167.826 256.864 169.227 C 262.619 173.030,272.704 178.125,274.475 178.125 C 275.022 178.125,275.809 178.465,276.225 178.881 C 277.661 180.317,292.593 183.690,300.646 184.397 C 344.392 188.241,384.586 160.208,397.014 117.188 C 401.681 101.033,400.594 75.220,394.629 60.548 C 392.295 54.806,385.890 42.747,383.949 40.441 C 382.894 39.187,382.031 37.820,382.031 37.403 C 382.031 35.846,365.759 20.483,360.043 16.643 C 311.905 -15.696,245.345 3.887,223.812 56.723 C 217.135 73.106,214.595 91.908,217.214 105.557 C 217.658 107.872,218.220 110.996,218.463 112.500 C 219.695 120.130,227.835 139.678,230.807 142.144 C 237.927 148.053,209.397 157.527,192.578 154.839 C 185.573 153.720,172.828 148.519,168.082 144.843 C 167.930 144.724,169.426 142.212,171.407 139.259 C 174.845 134.134,180.413 122.008,180.465 119.531 C 180.478 118.887,181.171 116.183,182.005 113.523 C 185.259 103.145,185.264 75.572,182.013 69.498 C 181.593 68.713,181.250 67.493,181.250 66.786 C 181.250 62.516,171.655 42.537,167.543 38.245 C 166.488 37.144,165.623 35.910,165.620 35.504 C 165.609 33.763,149.019 17.996,144.705 15.625 C 143.532 14.980,141.519 13.706,140.231 12.793 C 125.377 2.255,95.101 -2.893,74.068 1.542 M107.422 9.381 C 116.883 11.006,134.506 18.005,138.065 21.551 C 138.591 22.075,140.349 23.306,141.971 24.288 C 144.952 26.090,150.457 30.875,153.451 34.265 C 158.955 40.498,160.810 42.836,163.080 46.401 C 172.348 60.961,177.260 79.934,176.147 96.875 C 175.293 109.867,170.431 125.549,164.482 134.494 C 160.266 140.835,160.819 140.726,155.019 136.356 C 139.444 124.620,129.048 115.702,114.705 101.775 L 110.269 97.468 110.017 91.246 L 109.766 85.025 116.797 79.305 C 134.444 64.948,145.218 50.265,145.572 40.090 L 145.703 36.328 142.571 36.097 C 135.690 35.590,119.606 50.528,106.283 69.801 C 102.227 75.668,102.026 75.833,99.683 75.231 C 84.484 71.324,71.794 82.190,75.690 95.774 C 76.346 98.063,76.151 98.363,71.621 102.019 C 49.928 119.526,35.516 139.978,40.518 146.156 C 45.388 152.169,62.787 137.660,78.876 114.168 L 83.684 107.149 86.476 108.315 C 88.128 109.006,90.949 109.380,93.388 109.233 C 98.073 108.950,98.960 109.617,109.766 121.537 C 112.773 124.855,116.289 128.719,117.578 130.122 C 118.867 131.525,121.680 134.858,123.828 137.527 C 125.977 140.196,129.053 143.870,130.664 145.691 C 132.275 147.513,133.594 149.251,133.594 149.554 C 133.594 150.177,137.436 155.450,138.758 156.641 C 144.657 161.953,120.348 173.145,99.219 174.844 C 87.527 175.784,65.915 172.689,62.107 169.530 C 61.677 169.172,59.567 168.185,57.420 167.336 C 30.220 156.585,9.357 123.656,9.381 91.515 C 9.412 49.654,37.775 17.333,82.031 8.727 C 86.624 7.834,100.493 8.191,107.422 9.381 M324.609 10.885 C 333.369 12.742,341.628 15.656,346.846 18.733 C 349.048 20.031,351.125 21.094,351.460 21.094 C 353.542 21.094,368.422 34.315,373.699 40.853 C 376.717 44.593,383.594 55.526,383.594 56.586 C 383.594 56.835,384.477 58.871,385.558 61.110 C 386.638 63.349,388.230 68.164,389.096 71.810 C 403.223 131.292,356.167 183.220,295.286 175.333 C 287.064 174.267,272.118 168.880,266.081 164.806 C 264.201 163.538,262.346 162.500,261.957 162.500 C 256.925 162.500,269.039 146.174,293.983 119.340 L 302.513 110.163 306.139 110.294 C 308.134 110.365,310.850 110.173,312.175 109.866 C 314.450 109.339,314.842 109.625,319.206 114.993 C 334.613 133.943,352.383 146.990,361.389 145.963 C 371.314 144.832,350.417 117.595,332.086 107.768 C 325.552 104.266,324.038 102.192,325.058 98.142 C 328.401 84.866,319.506 74.154,306.056 75.257 C 301.071 75.667,300.730 75.588,299.797 73.807 C 298.561 71.448,292.681 64.750,285.493 57.513 C 273.941 45.883,264.813 40.268,257.422 40.247 L 252.734 40.234 252.888 43.615 C 253.039 46.916,254.669 50.510,257.553 53.896 C 263.503 60.883,277.216 73.394,281.828 76.044 C 282.697 76.544,285.542 78.448,288.148 80.275 L 292.888 83.597 291.675 86.500 C 290.873 88.418,290.573 90.938,290.789 93.928 L 291.117 98.454 278.566 109.625 C 255.863 129.832,241.078 141.193,239.350 139.758 C 230.808 132.669,222.521 98.688,225.367 82.422 C 228.502 64.507,232.127 55.827,242.430 41.563 C 254.208 25.257,274.346 13.713,297.656 9.907 C 303.435 8.963,318.040 9.493,324.609 10.885 M110.679 225.857 C 117.382 227.449,124.772 230.197,129.688 232.925 C 131.191 233.760,133.903 235.195,135.713 236.113 C 137.523 237.032,139.176 238.233,139.387 238.782 C 140.200 240.899,124.419 260.800,107.006 279.619 L 97.589 289.796 93.521 289.625 C 91.284 289.530,88.334 289.740,86.966 290.090 C 84.791 290.646,84.349 290.484,83.450 288.797 C 76.239 275.262,50.741 254.900,40.462 254.470 L 36.328 254.297 36.446 257.908 C 36.576 261.892,37.511 264.045,41.016 268.438 C 43.632 271.717,56.391 284.368,57.086 284.372 C 57.331 284.374,58.737 285.460,60.211 286.786 C 61.685 288.113,63.920 289.736,65.179 290.394 C 66.437 291.053,69.153 292.923,71.213 294.551 C 73.272 296.180,75.365 297.648,75.863 297.814 C 76.501 298.026,76.375 298.877,75.438 300.689 C 69.179 312.793,84.761 330.284,96.301 324.108 C 97.724 323.346,98.146 323.486,99.179 325.065 C 100.558 327.170,105.069 332.499,111.565 339.698 C 116.481 345.146,124.874 352.411,128.673 354.507 C 130.020 355.251,132.248 356.505,133.624 357.295 C 143.373 362.892,149.803 360.768,146.549 353.025 C 144.388 347.882,128.249 329.688,125.848 329.688 C 125.537 329.688,124.076 328.602,122.602 327.276 C 121.128 325.950,118.867 324.316,117.578 323.645 C 115.651 322.642,112.663 320.469,107.599 316.388 C 107.267 316.121,107.561 314.549,108.252 312.895 C 109.017 311.063,109.405 308.369,109.245 305.998 L 108.983 302.107 117.968 293.706 C 132.707 279.927,157.920 259.358,160.045 259.380 C 161.925 259.400,171.059 279.774,172.986 288.250 C 186.727 348.661,133.762 401.825,73.047 388.565 C 62.579 386.279,47.469 379.255,41.797 374.038 C 40.508 372.852,39.206 371.881,38.905 371.879 C 36.945 371.866,23.187 355.909,20.041 350.000 C 19.240 348.496,17.973 346.211,17.226 344.922 C 4.527 323.023,5.332 291.335,19.235 265.806 C 36.361 234.361,75.145 217.417,110.679 225.857 M313.305 225.040 C 321.294 225.860,334.069 228.511,335.547 229.656 C 335.977 229.989,338.388 231.020,340.905 231.948 C 343.422 232.876,347.001 234.681,348.858 235.958 C 350.715 237.236,352.459 238.281,352.733 238.281 C 354.817 238.281,368.621 251.167,373.237 257.422 C 374.823 259.570,376.328 261.504,376.583 261.719 C 377.667 262.635,383.275 273.368,384.542 276.953 C 385.302 279.102,386.255 281.563,386.661 282.422 C 390.502 290.558,391.640 309.658,389.134 323.929 C 381.138 369.466,334.830 400.089,289.453 389.845 C 281.272 387.998,272.975 384.983,267.997 382.048 C 265.795 380.750,263.850 379.688,263.673 379.688 C 262.881 379.687,252.354 371.390,248.828 367.987 C 220.983 341.113,215.700 296.264,236.506 263.386 L 239.159 259.193 240.869 260.747 C 241.809 261.601,244.143 263.312,246.056 264.548 C 247.968 265.785,250.781 267.938,252.306 269.332 C 253.830 270.727,255.345 271.869,255.671 271.871 C 255.997 271.873,258.458 273.848,261.140 276.261 C 263.821 278.673,267.530 281.924,269.381 283.487 C 271.231 285.050,274.571 288.086,276.802 290.234 C 279.034 292.383,283.029 296.160,285.680 298.629 C 290.365 302.991,290.480 303.193,289.763 305.790 C 289.311 307.427,289.301 309.690,289.737 311.629 L 290.448 314.796 283.274 320.625 C 269.228 332.039,260.365 342.620,255.954 353.243 C 247.310 374.058,270.095 362.904,288.114 337.500 C 298.219 323.254,297.795 323.673,300.951 324.814 C 313.937 329.509,328.681 316.372,324.355 303.961 C 323.530 301.595,323.591 301.516,329.925 296.745 C 347.883 283.215,363.027 261.848,360.105 254.161 C 357.154 246.401,330.959 267.613,321.865 285.126 C 321.076 286.647,319.436 289.054,318.223 290.476 L 316.016 293.062 313.409 291.752 C 311.562 290.825,309.551 290.546,306.516 290.798 L 302.231 291.154 297.405 286.017 C 294.750 283.193,290.996 279.228,289.063 277.208 C 285.527 273.513,281.988 269.471,277.197 263.656 C 275.774 261.928,273.906 259.747,273.047 258.810 C 272.188 257.872,269.988 255.066,268.160 252.576 C 266.331 250.085,263.450 246.235,261.756 244.021 L 258.678 239.996 261.670 238.016 C 270.120 232.424,285.810 226.810,296.875 225.419 C 301.387 224.852,305.129 224.350,305.191 224.303 C 305.253 224.257,308.904 224.588,313.305 225.040',
      size: new google.maps.Size(512, 512),
      origin: new google.maps.Point(0, 0),
      anchor: new google.maps.Point(256, 256),
      fillColor: '#df00c8',
      fillOpacity: 1,
      scale: 0.09,
      strokeColor: 'black',
      strokeWeight: 0.2

      };

    DronePositionIcon = new google.maps.Marker({
      position: myLatLng,
      map: map,
      icon: MarkerImage,
    });
  }
}

function drawLocationPoints() {
  var locationLength = _LocationPoints.length;
  //When the item reaches the end
  if (locationLength > 0 && _LocationIndex < locationLength - 1) {
    _LocationIndex++;
    _LocationPoints[_LocationIndex] = setFormatData(_LocationPoints[_LocationIndex]);
    drawLocationAtIndex();
  }

  if (_LocationIndex == locationLength - 1) {
    drawLineChart(0);
    setInformationAtIntex();
    map.fitBounds(_ZoomBounds);
    $('#clickReplay').css({ display: 'block' });

    //Show Other Flight info at end of drawing
    addOtherFlightInfoAtIndex();

    //Here, the first set from the server is completed
    //Run the timer
    if (_IsLiveMode) {
      if (thePlayTimer) window.clearInterval(thePlayTimer);
      thePlayTimer = window.setInterval(drawNextSetFromServer, 100);
      //get the next set of points from server
      getLocationPoints();

    }
    return;
  }

  window.setTimeout(drawLocationPoints, 10);
}


function drawNextSetFromServer() {
  var locationLength = _LocationPoints.length;
  if (locationLength < 1) return;
  //When the item reaches the end


  if (_LocationIndex < locationLength - 1) {
    //Check if the last point time is ready to ploat, then
    //ploat it and move to next index
    _LocationPoints[_LocationIndex] = setFormatData(_LocationPoints[_LocationIndex]);
    if (_LocationIndex < 0) _LocationIndex = 0;
    drawLocationAtIndexTimer();
    _LocationIndex++;

    //here
    setMapInfo();
  }
}

function drawLocationPointsTimer() {
  var locationLength = _LocationPoints.length;
  if (locationLength < 1) return;
  //When the item reaches the end


  if (_LocationIndex < locationLength - 1) {
    //Check if the last point time is ready to ploat, then
    //ploat it and move to next index
    if (_LocationIndex < 0) _LocationIndex = 0;
    var loc = _LocationPoints[_LocationIndex];
    if (loc['ReadTimeObject'] <= _ReplayTimeAt) {
      drawLocationAtIndexTimer();
      _LocationIndex++;
    }

  } else {
    if (thePlayTimer) window.clearTimeout(thePlayTimer);
    thePlayTimer = null;
  }

}

function drawLocationAtIndex() {
  var MarkerIndexStartAt = _LocationPoints.length - 20;
  if (_LocationIndex >= MarkerIndexStartAt) {
    var Opacity = 1 - (_LocationPoints.length - _LocationIndex) / 20;
    if (Opacity < 0) Opacity = 0;
    drawMarkerAtIndex(Opacity);
    drawPolyLineAtIndex(LatestLine);
    addDataToTableAtIntex();
    addDataToChartAtIntex();
    _ReplayTimeAt = new Date(_LocationPoints[_LocationIndex].ReadTime);
    setMapInfo();

    if (_LocationIndex == MarkerIndexStartAt) drawPolyLineAtIndex(OldLine);
  } else {
    drawPolyLineAtIndex(OldLine);
  }

  //Add Other Flight Info as Intex
  //addOtherFlightInfoAtIndex();
}

function addOtherFlightInfoAtIndex() {
  var loc = _LocationPoints[_LocationIndex];
  var OtherFlight = loc['OtherFlight'];
  var ProcessedIDs = {};
  var thisFlightDistance = 2000;
  for (var i = 0; i < OtherFlight.length; i++) {
    var FlightID = OtherFlight[i]['FlightID'];
    var Center = new google.maps.LatLng(OtherFlight[i]['Lat'], OtherFlight[i]['Lng']);
    if (_OtherFlights[FlightID] + '' == 'undefined') {
      _OtherFlights[FlightID] = new google.maps.Circle({
        strokeColor: '#FF0000',
        strokeOpacity: 1,
        strokeWeight: 2,
        fillOpacity: 0,
        center: Center,
        radius: DistanceOptions['Radius']
      });
      var InfoWindow = new google.maps.InfoWindow({
        content: '<a href="/Map/FlightData/' + FlightID + '">View Flight ' + FlightID + '</a>',
        position: Center
      });

      _OtherFlights[FlightID].addListener('click', function () {
        InfoWindow.open(map);
      });
      _OtherFlights[FlightID + 'Info'] = InfoWindow;
    }
    _OtherFlights[FlightID + 'Info'].setPosition(Center);

    if (OtherFlight[i]['Distance'] < thisFlightDistance) {
      thisFlightDistance = OtherFlight[i]['Distance'];
    }
    var Options = {};

    if (OtherFlight[i]['Distance'] <= DistanceOptions['Critical']) {
      //Options.fillColor = 'red';
      Options.strokeColor = 'red';
    } else if (OtherFlight[i]['Distance'] <= DistanceOptions['Warning']) {
      //Options.fillColor = 'orange';
      Options.strokeColor = 'orange';
    } else {
      //Options.fillColor = 'green';
      Options.strokeColor = 'green';
    }
    Options.center = Center;
    Options.map = map;

    _OtherFlights[FlightID].setOptions(Options);
    //_OtherFlights[FlightID].setMap(map);
    ProcessedIDs[FlightID] = true;
    ProcessedIDs[FlightID + 'Info'] = true;
    console.log("Flight ID: " + FlightID + ", Distance: " + OtherFlight[i]['Distance']);
  }

  //Hide if the flight is not available anymore
  for (var FlightID in _OtherFlights) {
    if (ProcessedIDs[FlightID]) {
      //nothing

    } else {
      _OtherFlights[FlightID].setMap(null);
    }
  }
  addThisFlightInfoAtIndex(thisFlightDistance);
}

function addThisFlightInfoAtIndex(MinDistance) {
  var loc = _LocationPoints[_LocationIndex];
  var thisFlight = 'thisFlight';
  var Center = new google.maps.LatLng(loc['Latitude'], loc['Longitude']);
  if (_OtherFlights[thisFlight] + '' == 'undefined') {
    _OtherFlights[thisFlight] = new google.maps.Circle({
      strokeColor: '#4E648E',
      strokeOpacity: 1,
      strokeWeight: 2,
      fillOpacity: 0,
      center: Center,
      radius: DistanceOptions['Radius']
    });
  }

  var Options = {
    map: map,
    center: Center
  };
  if (MinDistance <= DistanceOptions['Critical']) {
    Options.fillColor = null;
    Options.strokeColor = '#4E648E';
  } else if (MinDistance <= DistanceOptions['Warning']) {
    Options.fillColor = null;
    Options.strokeColor = '#4E648E';
  } else {
    Options.map = null;
  }
  _OtherFlights[thisFlight].setOptions(Options);

}

function drawLocationAtIndexTimer() {

  drawPolyLineAtIndex(LatestLine);
  drawMarkerAtIndex(1);
  addDataToTableAtIntex();
  addDataToChartTimer();
  setInformationAtIntex();
  addOtherFlightInfoAtIndex();

  //set opacity of marker
  var initOpacity = 1;
  for (var i = _AllMarkers.length - 1; i > 0; i--) {
    if (initOpacity < 0) initOpacity = 0;
    _AllMarkers[i].setOpacity(initOpacity);
    initOpacity = initOpacity - 0.05;
    if (initOpacity < 0) break;
  }
  if (_AllMarkers.length > 20) {
    var Marker = _AllMarkers.shift();
    Marker.setMap(null);
  }

  //Add the line to old one after 20 points
  if (_LocationIndex >= 20) {
    var LastPoint = LatestLine.getPath().removeAt(0);
    if (OldLine.getPath().length == 0) OldLine.getPath().push(LastPoint);

    var LastPoint = LatestLine.getPath().getAt(0);
    OldLine.getPath().push(LastPoint);
  }
}


function drawMarkerAtIndex(Opacity) {
  var loc = _LocationPoints[_LocationIndex];
  var body =
    'Lat: <b>' + loc['Latitude'] + '</b>,' +
    'Lng:  <b>' + loc['Longitude'] + "</b><br>" +
    "Time (UTC): <b>" + loc['ReadTime'] + '</b>';
  var myLatLng = new google.maps.LatLng(loc['Latitude'], loc['Longitude']);
  var IsOutSide = loc['IsOutSide'];
  var image = IsOutSide ? '/bullet_red.png' : '/bullet_blue.png';
  var marker = new google.maps.Marker({
    position: myLatLng,
    map: map,
    icon: image,
    title: loc['DroneRFID'],
    zIndex: 99
  });
  marker.setOpacity(Opacity);
  google.maps.event.addListener(marker, 'click', function () {
    var infowindow = new google.maps.InfoWindow({
      content: body
    });
    infowindow.open(map, marker);
  });
  map.setCenter(myLatLng);
  _ZoomBounds.extend(myLatLng);


  //Add to global marker array
  _AllMarkers.push(marker);

}


function drawPolyLineAtIndex(Polygon) {
  var loc = _LocationPoints[_LocationIndex];
  var myLatLng = new google.maps.LatLng(loc['Latitude'], loc['Longitude']);
  var Path = Polygon.getPath();

  if (DronePositionIcon) {
    var icons = DronePositionIcon.getIcon();
    icons.rotation = parseInt(loc['Heading']);
    DronePositionIcon.setIcon(icons);
    DronePositionIcon.setPosition(myLatLng);
  }
  ADSBLine.setStart(loc['Latitude'], loc['Longitude'], loc['Altitude']);

  if (_LocationIndex > 0) {
    var FirstLat = _LocationPoints[_LocationIndex - 1];
    var SecondLat = _LocationPoints[_LocationIndex];
    var seconds = (SecondLat.ReadTimeObject.getTime() - FirstLat.ReadTimeObject.getTime()) / 1000;
    if (seconds > 10) {
      var firstlatlng = new google.maps.LatLng(FirstLat['Latitude'], FirstLat['Longitude']);
      var secondlatlng = new google.maps.LatLng(SecondLat['Latitude'], SecondLat['Longitude']);
      var lineSymbol = {
        path: 'M 0,-1 0,1',
        strokeOpacity: 1,
        scale: 4
      };
      var errpolyOptions = {
        strokeColor: "red",
        strokeOpacity: 0.4,
        strokeWeight: 1,
        icons: [{
          icon: lineSymbol,
          offset: '0',
          repeat: '20px'
        }]
      }
      var errorpoly = new google.maps.Polyline(errpolyOptions);
      errorpoly.setMap(map);
      var errorpath = errorpoly.getPath();
      errorpath.push(firstlatlng);
      errorpath.push(secondlatlng);
      DottedLine.push(errorpoly);
    }

    Path.push(myLatLng);
  } else {
    Path.push(myLatLng);
  }

}

function ClearDottedLine() {
  for (var i = 0; i < DottedLine.length; i++) {
    DottedLine[i].setMap(null);
  }
}

function drawLineChart(isReset) {
  //reset data to Empty array to iniltilize
  if (isReset == 1) {
    _lineChartData = {
      'Labels': [],
      'Altitude': [],
      'Satellites': [],
      'Pitch': [],
      'Roll': [],
      'Speed': []
    };
  }

  var data = getChartData();
  _lineChart.initialize(data);
}


function addDataToTableAtIntex() {
  var _LastValue = _LocationPoints[_LocationIndex];

  var tLatData = '<td>' + _LastValue['Latitude'] + '</td>';
  var tLonData = '<td>' + _LastValue['Longitude'] + '</td>';
  var tAltData = '<td>' + _LastValue['Altitude'] + '</td>';
  var tSpeedData = '<td>' + _LastValue['Speed'] + '</td>';
  var tFxQltyData = '<td>' + _LastValue['FixQuality'] + '</td>';
  var tSatelliteData = '<td>' + _LastValue['Satellites'] + '</td>';
  var tDrTime = '<td>' + _LastValue['ReadTime'] + '</td>';
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
  '<td>' + _LastValue['voltage'] + '</td>\n' +
  tTotFlightTimeData +
  '</tr>';
  if ($('#TableMapData tbody tr').length > 20) {
    $('#TableMapData tbody tr:last-child').remove();
  }
  if ($('#TableMapData tbody tr').length > 0) {
    $('#TableMapData tbody tr:first').before(loctr);
  } else {
    $('#TableMapData').append(loctr);
  }
}

function addDataToChartAtIntex() {
  var aData = _LocationPoints[_LocationIndex];
  _lineChartData['Labels'].push(aData['fReadTime']);
  _lineChartData['Altitude'].push(aData['Altitude']);
  _lineChartData['Satellites'].push(aData['Satellites']);
  _lineChartData['Pitch'].push(aData['Pitch']);
  _lineChartData['Roll'].push(aData['Roll']);
  _lineChartData['Speed'].push(aData['Speed']);
}

function addDataToChartTimer() {
  var aData = _LocationPoints[_LocationIndex];
  var LineData = [];
  var Label = aData['fReadTime'];

  if (_lineChartLegend['Altitude']) LineData.push(aData['Altitude']);
  if (_lineChartLegend['Satellites']) LineData.push(aData['Satellites']);
  if (_lineChartLegend['Pitch']) LineData.push(aData['Pitch']);
  if (_lineChartLegend['Roll']) LineData.push(aData['Roll']);
  if (_lineChartLegend['Speed']) LineData.push(aData['Speed']);

  if (_lineChart.datasets[0].points.length > 20) _lineChart.removeData();
  _lineChart.addData(LineData, Label);
}

function setInformationAtIntex() {
  var _LastValue = _LocationPoints[_LocationIndex];
  $.each(_LastValue, function (key, value) {
    $('#data_' + key).html(value);
  });

}

function setMapInfo() {
  //Show the information of drawing
  if (_LocationIndex < 0) return;
  var loc = _LocationPoints[_LocationIndex];
  var sDate = fmtDt(_ReplayTimeAt);
  $('#map-info').html(
    '<span class="date">' + sDate + '</span> ' +
    '<span class="gps">' + Math.abs(loc['Latitude']) + '<span>&deg;' + (loc['Latitude'] > 0 ? 'N' : 'S') + '</span></span> ' +
    '<span class="gps">' + Math.abs(loc['Longitude']) + '<span>&deg;' + (loc['Longitude'] > 0 ? 'E' : 'W') + '</span></span> ' + ''
  );
}



function fn_on_play(theData) {
  if (thePlayTimer) window.clearInterval(thePlayTimer);
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
    var payState = 'playing';
    if (playerInstance) {
      payState = playerInstance.getState();
      if (_ReplayTimeAt >= _VideoStartTime) {
        if (payState == 'playing') {
          _ElapsedTime++;
        } else if (payState != 'buffering') {
          playerInstance.play();
        }
      } else {
        _ElapsedTime++;
      }
    } else {
      _ElapsedTime++;
    }
    _ReplayTimeAt = new Date(_BlackBoxStartAt);
    _ReplayTimeAt.setSeconds(_BlackBoxStartAt.getSeconds() + _ElapsedTime);
    setMapInfo();
    drawLocationPointsTimer();
  }, ReplayInterval);
}

function getDistanceFromLatLonInKm(lat1, lon1, lat2, lon2) {
  var R = 6371; // Radius of the earth in km
  var dLat = deg2rad(lat2 - lat1);  // deg2rad below
  var dLon = deg2rad(lon2 - lon1);
  var a =
    Math.sin(dLat / 2) * Math.sin(dLat / 2) +
    Math.cos(deg2rad(lat1)) * Math.cos(deg2rad(lat2)) *
    Math.sin(dLon / 2) * Math.sin(dLon / 2)
  ;
  var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
  var d = R * c; // Distance in km
  return d;
}

function deg2rad(deg) {
  return deg * (Math.PI / 180)
}


function toRad(Num) {
  return Num * Math.PI / 180;
}

function toDeg(Num) {
  return Num * 180 / Math.PI;
}

function destinationPoint(Point, brng, dist) {
  /*
  reference http://stackoverflow.com/questions/2637023/how-to-calculate-the-latlng-of-a-point-a-certain-distance-away-from-another
  */
  dist = dist / 6371;
  brng = toRad(brng);

  var lat1 = toRad(Point.lat()), lon1 = toRad(Point.lng());

  var lat2 = Math.asin(Math.sin(lat1) * Math.cos(dist) +
                       Math.cos(lat1) * Math.sin(dist) * Math.cos(brng));

  var lon2 = lon1 + Math.atan2(Math.sin(brng) * Math.sin(dist) *
                               Math.cos(lat1),
                               Math.cos(dist) - Math.sin(lat1) *
                               Math.sin(lat2));

  if (isNaN(lat2) || isNaN(lon2)) return null;

  return new google.maps.LatLng(toDeg(lat2), toDeg(lon2));
}

function getIconColor(Height) {

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