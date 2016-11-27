
var _isUTCFormat = false;
var _ElapsedTime = 0;
var _LastDroneDataID = 0;
var _LocationPoints = [];
var _LocationIndex = -1;
var _ZoomBounds = new google.maps.LatLngBounds();
var _AllMarkers = [];
var _Boundary = [];

var _FirstTotalFlightTime = -100;
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

var FillOptions = {
  Outer: { fillColor: 'orange', strokeWeight: 1, fillOpacity: 0.3 },
  Inner: { fillColor: '#55FF55', strokeWeight: 0, fillOpacity: 0.3 }
};



function initializeMap() {
  var mapOptions = {
    zoom: 10,
    center: { lat: -34.397, lng: 150.644 },
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



function getChartData() {
  var ChartDataSet = [];
  if (_lineChartLegend['Altitude']) ChartDataSet.push({
    label: "Altitude",
    strokeColor: "rgb(219, 211, 1)",
    pointColor: "rgb(219, 211, 1)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgb(219, 211, 1)",
    data: _lineChartData['Altitude']
  });

  if (_lineChartLegend['Satellites']) ChartDataSet.push({
    label: "Satellites",
    strokeColor: "rgba(101, 186, 25,1)",
    pointColor: "rgba(101, 186, 25,1)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgba(101, 186, 25,1)",
    data: _lineChartData['Satellites']
  });

  if (_lineChartLegend['Pitch']) ChartDataSet.push({
    label: "Pitch",
    strokeColor: "rgba(255, 89, 0,1)",
    pointColor: "rgba(255, 89, 0,1)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgba(255, 89, 0,1)",
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
    strokeColor: "rgb(11, 144, 118)",
    pointColor: "rgb(11, 144, 118)",
    pointStrokeColor: "#fff",
    pointHighlightStroke: "rgba(11, 144, 118,1)",
    data: _lineChartData['Speed']
  });

  data = {
    labels: _lineChartData['Labels'],
    datasets: ChartDataSet
  };
  return data;
}


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
              + '<th>Volt</th>'
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
  for (var i = 0; i < AllowedLocation.length; i++) {
    var OuterPath = [];
    //Add outer area
    for (var j = 0; j < AllowedLocation[i].Outer.length; j++) {
      OuterPath.push(AllowedLocation[i].Outer[j])
    }
    //close the polygon - add the last first point again
    OuterPath.push(OuterPath[0]);
    //Add the inner polygon on reverse
    for (var j = AllowedLocation[i].Inner.length - 1; j >= 0; j--) {
      OuterPath.push(AllowedLocation[i].Inner[j])
    }

    var OuterBoundary = new google.maps.Polygon({
      paths: OuterPath,
      strokeWeight: 0,
      fillColor: FillOptions['Outer']['fillColor'],
      fillOpacity: FillOptions['Outer']['fillOpacity'],
      editable: false,
      draggable: false
    });
    OuterBoundary.setMap(map);
    _Boundary.push(OuterBoundary);

    var InnerBoundary = new google.maps.Polygon({
      paths: AllowedLocation[i].Outer,
      fillColor: FillOptions['Inner']['fillColor'],
      fillOpacity: FillOptions['Inner']['fillOpacity'],
      strokeWeight: 0,
      editable: false,
      draggable: false
    });
    InnerBoundary.setMap(map);
    _Boundary.push(InnerBoundary);

    var OuterBorder = new google.maps.Polygon({
      paths: AllowedLocation[i].Inner,
      strokeOpacity: 1,
      strokeColor: 'red',
      strokeWeight: FillOptions['Outer']['strokeWeight'],
      fillOpacity: 0,
      editable: false,
      draggable: false
    });
    OuterBorder.setMap(map);
    _Boundary.push(OuterBorder);
  }
}



function setFormatData(_LastValue) {
  var date;

  $.each(_LastValue, function (key, value) {
    if (value == null) value = '';
    switch (key) {
      case "ReadTime":
        var iDt = parseInt(_LastValue['ReadTime'].substr(6));
        var theDate = new Date(iDt);
        theDate.setMinutes(theDate.getMinutes() + Now.getTimezoneOffset());
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
      case "voltage":
        value = parseFloat(value);
        if (isNaN(value)) value = 0;
        //if (value > 0) value = value / (60 * 60) * 1000;
        value = value.toFixed(2);
        break;
      case "TotalFlightTime":
        value = parseFloat(value);
        if (isNaN(value)) value = 0;
        /*
        //First time only
        if (_FirstTotalFlightTime < 0) {
          _FirstTotalFlightTime = value;
        }
        //Set the offset for flightime
        value = value - _FirstTotalFlightTime;
        */
        if (value > 0) value = value / 60;
        value = value.toFixed(3);
        break;
      case "Heading":
        value = parseFloat(value);
        if (isNaN(value)) value = 0;
        if (value < 0) value = value + 360;
        value = value.toFixed(2);
        break;
      case "OtherFlightIDs":
        if (value == "") {
          _LastValue['OtherFlight'] = [];
        } else {
          _LastValue['OtherFlight'] = parseFlight(value);
        }
    }
    _LastValue[key] = value;
  });

  return _LastValue;
  // var oCompaniesTable = $('#MapData Table')
}

function parseFlight(FlightInfo) {
  var ReturnObject = [];
  var aFlight = FlightInfo.split("|");
  for (var i = 0; i < aFlight.length; i++) {
    var Data = aFlight[i].split(",");
    var thisObj = {
      "FlightID":  parseInt(Data[0]),
      "Lat": parseFloat(Data[1]),
      "Lng": parseFloat(Data[2]),
      "Distance": parseFloat(Data[3])
    };
    if (thisObj['FlightID'] != FlightID) ReturnObject.push(thisObj);
  }
  return ReturnObject;
}


function fmtDt(date) {
  if (date instanceof Date) {

  } else {
    return 'Invalid';
  }
  var day = date.getDate();
  var hours = _isUTCFormat ? date.getUTCHours() : date.getHours();
  var minutes = _isUTCFormat ? date.getUTCMinutes() : date.getMinutes();
  var seconds = _isUTCFormat ? date.getUTCSeconds() : date.getSeconds();
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

  var hours = _isUTCFormat ? date.getUTCHours() : date.getHours();
  var minutes = _isUTCFormat ? date.getUTCMinutes() : date.getMinutes();
  var seconds = _isUTCFormat ? date.getUTCSeconds() : date.getSeconds();
  hours = hours < 10 ? '0' + hours : hours;
  minutes = minutes < 10 ? '0' + minutes : minutes;
  seconds = seconds < 10 ? '0' + seconds : seconds;
  return hours + ':' + minutes + ':' + seconds;
}
