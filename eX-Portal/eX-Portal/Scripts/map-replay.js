var MaxRecords = 2000;
var map;
var Now = new Date();
var LatestLine = null;
var OldLine = null;
var playerInstance = null;

var _BlackBoxStartAt = null;
var _ReplayTimeAt = null;
var _VideoStartTime = new Date(1970, 0, 0, 24, 0, 0);
var _IsDrawInitilized = false;

$(document).ready(function () {
  initializeMap();
  setAllowedRegion();
  //drawLocationPoints();
  initilizeTable();
  initilizeChart();

  getLocationPoints();

  $('#chkShowFullPath').on("change", function (e) {
    if (this.checked) {
      OldLine.setMap(map);
    } else {
      OldLine.setMap(null);
    }
  })
});

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
    map.setCenter(myLatLng)
    drawLocationPoints();    
    _IsDrawInitilized = true;

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
}

function drawLocationAtIndexTimer() {

  drawPolyLineAtIndex(LatestLine);
  drawMarkerAtIndex(1);
  addDataToTableAtIntex();
  addDataToChartTimer();
  setInformationAtIntex();

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
  Path.push(myLatLng);
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
  }, 1000);
}

