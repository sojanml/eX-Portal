var MaxRecords = 2000;
var map;
var LatestLine = null;
var OldLine = null;
var playerInstance = null;
var FillOptions = [
  { fillColor: '#55FF55', strokeWeight: 0, fillOpacity: 0.2 },
  { fillColor: 'yellow', strokeWeight: 1, fillOpacity: 0 }
];
var _ElapsedTime = 0;
var _LastDroneDataID = 0;
var _LocationPoints = [];
var _LocationIndex = -1;
var _ZoomBounds = new google.maps.LatLngBounds();
var _AllMarkers = [];
var _FirstTotalFlightTime = -100;
var _BlackBoxStartAt = null;
var _ReplayTimeAt = null;
var _VideoStartTime = new Date(1970, 0, 0, 24, 0, 0);
var _lineChart = null;
var _lineChartLegend = {
  'Altitude': true,
  'Satellites': true,
  'Pitch': true,
  'Roll': true,
  'Speed': true
};
var _lineChartData = {
  'Labels': [],
  'Altitude': [],
  'Satellites': [],
  'Pitch': [],
  'Roll': [],
  'Speed': []
};

$(document).ready(function () {
  initializeMap();
  setAllowedRegion();
  getLocationPoints();
  //drawLocationPoints();
  initilizeTable();
  initilizeChart();

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
  startReplayTimer();


}


function initializeMap() {
  var mapOptions = {
    zoom: 10,
    center: {lat: -34.397, lng: 150.644},
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

  LatestLine = new google.maps.Polyline({
    strokeColor: '#000000',
    strokeOpacity: 1,
    strokeWeight: 2
  });
  LatestLine.setMap(map);

  OldLine = new google.maps.Polyline({
    strokeColor: '#000000',
    strokeOpacity: 0.5,
    strokeWeight: 2
  });
  OldLine.setMap(map);

  //  myInterval = setInterval(function () { }, 500);

};

function initilizeTable() {
  var TableHeader = '<thead>' +
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
              + '</tr></thead>';
  $('#TableMapData').append(TableHeader);
}


function initilizeChart() {
  //init legend first
  var ctx = $("#myChart").get(0).getContext('2d');
  ctx.canvas.height = 200;  // setting height of canvas
  ctx.canvas.width = 1000; // setting width of canvas

  var ChartData = getChartData();
  _lineChart = new Chart(ctx).Line(ChartData, {
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
  var legend = _lineChart.generateLegend();
  $('#map-legent').append(legend);
}

function setAllowedRegion() {
  var OptionCounter = 0;
  for (var i = 0; i < AllowedLocation.length; i += 2) {
    if (OptionCounter++ >= 2) OptionCounter = 0;
    var paths = AllowedLocation[i];
    var OuterBoundary = new google.maps.Polygon({
      paths: paths,
      strokeOpacity: 1,
      strokeColor: 'red',
      strokeWeight: FillOptions[OptionCounter]['strokeWeight'],
      fillColor: FillOptions[OptionCounter]['fillColor'],
      fillOpacity: FillOptions[OptionCounter]['fillOpacity'],
      editable: false,
      draggable: false
    });
    OuterBoundary.setMap(map);
  }
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
      if (msg.length > 0) {
        window.setTimeout(getLocationPoints, 1000);
      } else {
        setDrawIntilize();
      }
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
    map.setCenter(myLatLng)
    drawLocationPoints();
    $('#clickReplay').css({ display: 'block' });
  }
}

function drawLocationPoints() {
  var locationLength = _LocationPoints.length;
  //When the item reaches the end
  if (_LocationIndex == locationLength - 1) {
    drawLineChart(0);
    setInformationAtIntex();
    return;
  }
  if (locationLength > 0 && _LocationIndex < locationLength - 1) {
    _LocationIndex++;
    _LocationPoints[_LocationIndex] = setFormatData(_LocationPoints[_LocationIndex]);
    /*
    var thisDrawPoint = _LocationPoints[_LocationIndex];
    var lastDrawPoint = _LocationIndex > 0 ? _LocationPoints[_LocationIndex - 1] : null;
    if (lastDrawPoint != null &&
        lastDrawPoint["Latitude"] == thisDrawPoint["Latitude"] &&
        lastDrawPoint["Longitude"] == thisDrawPoint["Longitude"]) {
      //nothing
    } else {
      drawLocationAtIndex();
    }
    */
    drawLocationAtIndex();
  }
  window.setTimeout(drawLocationPoints, 10);
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
  map.fitBounds(_ZoomBounds);

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

function getChartData() {
  var ChartDataSet = [];
  if (_lineChartLegend['Altitude']) ChartDataSet.push({
    label: "Altitude",
    strokeColor: "rgb(236, 215, 101)",
    pointColor: "rgb(236, 215, 101)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgb(236, 215, 101)",
    data: _lineChartData['Altitude']
  });

  if (_lineChartLegend['Satellites']) ChartDataSet.push({
    label: "Satellites",
    strokeColor: "rgba(151,187,205,1)",
    pointColor: "rgba(151,187,205,1)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgba(151,187,205,1)",
    data: _lineChartData['Satellites']
  });

  if (_lineChartLegend['Pitch']) ChartDataSet.push({
    label: "Pitch",
    strokeColor: "rgba(255,119,119,1)",
    pointColor: "rgba(255,119,119,1)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgba(255,119,119,1)",
    data: _lineChartData['Pitch']
  });

  if (_lineChartLegend['Roll']) ChartDataSet.push({
    label: "Roll",
    strokeColor: "rgb(153, 131, 199)",
    pointColor: "rgb(153, 131, 199)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgb(153, 131, 199)",
    data: _lineChartData['Roll']
  });

  if (_lineChartLegend['Speed']) ChartDataSet.push({
    label: "Speed",
    strokeColor: "rgb(117, 237, 251)",
    pointColor: "rgb(117, 237, 251)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgba(187,17,17,1)",
    data: _lineChartData['Speed']
  });

  data = {
    labels: _lineChartData['Labels'],
    datasets: ChartDataSet
  };
  return data;
}

function setInformationAtIntex() {
  var _LastValue = _LocationPoints[_LocationIndex];
  $.each(_LastValue, function (key, value) {
    $('#data_' + key).html(value);
  });
  

}


function setFormatData(_LastValue) {
  var date;

  $.each(_LastValue, function (key, value) {
    if (value == null) value = '';
    switch (key) {
      case "ReadTime":
        var iDt = parseInt(_LastValue['ReadTime'].substr(6));
        var theDate = new Date(iDt);
        value = fmtDt(theDate);
        _LastValue['fReadTime'] = fmtTime(theDate);
        _LastValue['ReadTimeObject'] = theDate;
        break;
      case "Distance":
        value = parseInt(value);
        if (isNaN(value)) value = 0;
        break;
      case "Latitude":
      case "Longitude":
        value = parseFloat(value);
        if (isNaN(value)) value = 0;
        value = value.toFixed(5);
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
        //First time only
        if (_FirstTotalFlightTime < 0) {
          _FirstTotalFlightTime = value;
        }
        //Set the offset for flightime
        value = value - _FirstTotalFlightTime;
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
  });

  return _LastValue;
  // var oCompaniesTable = $('#MapData Table')
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

function fmtDt(date) {
  if (date instanceof Date) {

  } else {
    return 'Invalid';
  }
  var day = date.getDate();
  var hours = date.getHours();
  var minutes = date.getMinutes();
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

function fmtTime(date) {
  if (date instanceof Date) {
  } else {
    return 'Invalid';
  }

  var hours = date.getHours();
  var minutes = date.getMinutes();
  var seconds = date.getSeconds();
  hours = hours < 10 ? '0' + hours : hours;
  minutes = minutes < 10 ? '0' + minutes : minutes;
  seconds = seconds < 10 ? '0' + seconds : seconds;
  return hours + ':' + minutes + ':' + seconds;
}


function fn_on_play(theData) {
  if (thePlayTimer) window.clearInterval(thePlayTimer);
  startReplayTimer();
}

function fn_on_pause(theData) {
  if (thePlayTimer) window.clearInterval(thePlayTimer);
}

function startReplayTimer() {
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
    _ReplayTimeAt = new Date(_BlackBoxStartAt.toString());
    _ReplayTimeAt.setSeconds(_ReplayTimeAt.getSeconds() + _ElapsedTime);
    setMapInfo();
    drawLocationPointsTimer();
  }, 1000);
}

