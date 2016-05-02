var FillOptions = {
  Outer: { fillColor: 'orange' , strokeWeight: 1, fillOpacity: 0.3 },
  Inner: { fillColor: '#55FF55', strokeWeight: 0, fillOpacity: 0.3 }
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
    for (var j = 0;  j < AllowedLocation[i].Outer.length; j++) {
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

    var InnerBoundary = new google.maps.Polygon({
      paths: AllowedLocation[i].Inner,
      fillColor: FillOptions['Inner']['fillColor'],
      fillOpacity: FillOptions['Inner']['fillOpacity'],
      strokeWeight: 0,
      editable: false,
      draggable: false
    });
    InnerBoundary.setMap(map);


    var OuterBorder = new google.maps.Polygon({
      paths: AllowedLocation[i].Outer,
      strokeOpacity: 1,
      strokeColor: 'red',
      strokeWeight: FillOptions['Outer']['strokeWeight'],
      fillOpacity:0,
      editable: false,
      draggable: false
    });
    OuterBorder.setMap(map);

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
