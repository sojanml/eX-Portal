var MaxRecords = 2000;
var map;
var Now = new Date();
var LatestLine = null;
var OldLine = null;
var playerInstance = null;
var thePlayTimer = null;
var _IsGeoTagShown = false;
var _GeoTagLayer = null;

var _BlackBoxStartAt = null;
var _ReplayTimeAt = null;
var _VideoStartTime = new Date(1970, 0, 0, 24, 0, 0);
var _IsDrawInitilized = false;
var _OtherFlights = {};
var _FlightOra = null
var ReplayInterval = 1000;
var DottedLine = [];

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
  })

  $('#ReplaySpeed').on("change", function(e) {
    ReplayInterval = parseInt($(this).val(), 0);
    startReplayTimer();
  });

  $('#btnGeoTag').on("click", function (e) {
    ShowHideGeoTag($(this));
  });

  $(document).on("click", ".map-point", function () {
    fn_MapPoint($(this));
  });

  $('#btnPayload').on("click", function () {
    ShowHidePayload($(this));
  });

});


function fn_MapPoint(thisObj) {
  var Lat = thisObj.attr("data-lat");
  var Lng = thisObj.attr("data-lng");
  var Doc = thisObj.attr("data-doc");
  var Thump = '/Upload/Drone/' + DroneName + '/' + FlightID + '/' + Doc.replace(".jpg", ".t.png");
  var DocURL = '/Upload/Drone/' + DroneName + '/' + FlightID + '/' + Doc;
  var Center = new google.maps.LatLng(Lat, Lng);
  GeoInfoWindow.setPosition(Center);
  GeoInfoWindow.setContent(
      '<table cellpadding=0 cellspacig=0>' +
      '<tr>'+
      '<td><a target="_blank" href="' + DocURL + '"><img style="margin-right:10px; width:80px; height: auto;" src="' + Thump + '"></a></td>' +
      '<td style="white-space:nowrap;">Lat: <b>' + Lat + "</b><br>" + 
      "Lng: <b>" + Lng + "</b><br>" + 
      "Alt: <b>" + thisObj.attr("data-alt") + "</b>" +
      "</td>" + 
      '</tr></table>');    
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

  //var KmlUrl = '/Map/PayloadData/' + FlightID;
  var KmlUrl = 'http://test.exponent-ts.com/Upload/Temp/aaa-01.xml';
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

    var image = {
      path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
      strokeColor: '#393',
      strokeWidth: '1px',
      scale: 6,
      rotation: parseInt(loc["Heading"])
    };

    DronePositionIcon = new google.maps.Marker({
      position: myLatLng,
      map: map,
      icon: image,
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
    if(_IsLiveMode) {
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
    if(thePlayTimer) window.clearTimeout(thePlayTimer);
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
  for(var i = 0; i < OtherFlight.length; i++) {    
    var FlightID = OtherFlight[i]['FlightID'];
    var Center = new google.maps.LatLng(OtherFlight[i]['Lat'], OtherFlight[i]['Lng']);
    if (_OtherFlights[FlightID] + '' == 'undefined') {
      _OtherFlights[FlightID] = new google.maps.Circle({
        strokeColor: '#FF0000',
        strokeOpacity: 0.8,
        strokeWeight: 0,
        fillColor: '#FF0000',
        fillOpacity: 0.3,
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
      Options.fillColor = 'red';
      Options.strokeColor = 'red';
    } else if (OtherFlight[i]['Distance'] <= DistanceOptions['Warning']) {
      Options.fillColor = 'orange';
      Options.strokeColor = 'orange';
    } else {
      Options.fillColor = 'green';
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
      strokeColor: null,
      strokeOpacity: 0.8,
      strokeWeight: 0,
      fillColor: '#FF0000',
      fillOpacity: 0.3,
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
    Options.strokeColor = 'blue';
  } else if (MinDistance <= DistanceOptions['Warning']) {
    Options.fillColor = null;
    Options.strokeColor = 'blue';
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
    if(OldLine.getPath().length == 0) OldLine.getPath().push(LastPoint);

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

    if (_LocationIndex > 0)
    {
        var FirstLat = _LocationPoints[_LocationIndex - 1];
        var SecondLat=_LocationPoints[_LocationIndex ];
        var seconds = (SecondLat.ReadTimeObject.getTime() - FirstLat.ReadTimeObject.getTime()) / 1000;
        if(seconds>5)
        {
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
        var  errorpoly = new google.maps.Polyline(errpolyOptions);
        errorpoly.setMap(map);
        var errorpath = errorpoly.getPath();
        errorpath.push(firstlatlng);
        errorpath.push(secondlatlng);
        DottedLine.push(errorpoly);
    }
    
        Path.push(myLatLng);
   }else
    {
        Path.push(myLatLng);
    }
 
 
}

function ClearDottedLine()
{
    for(var i=0;i<DottedLine.length;i++)
    {
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
  var tDrTime = '<td>' +  _LastValue['ReadTime'] + '</td>';
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
  if($('#TableMapData tbody tr').length > 0)  {
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
    window.clearTimeout(thePlayTimer);
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

