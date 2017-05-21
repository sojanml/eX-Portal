var _ADSBLayer = null;
var UpdateDelay = 5000;

var Timers = {
  getADSB: null
}

var _InfoWindow = null;
var _InfoWindowKey = '';
var _InfoWindowLine = null;

$(document).ready(function () {

  _InfoWindow = new google.maps.InfoWindow({
    content: '<div id="InfoWindowContent">...</div>'
  });

  _InfoWindowLine = new google.maps.Polyline({
    path: [],
    geodesic: true,
    strokeColor: '#ff5900',
    strokeOpacity: 1.0,
    strokeWeight: 3
  });

  google.maps.event.addListener(_InfoWindow, 'closeclick', function () {
    _InfoWindowKey = '';
    _InfoWindowLine.setMap(null);
  });

  $(document).on("click", "div.adsb-point", function () {
    var t = $(this);
    _InfoWindowKey = t.attr('id');
    ShowInfoWindow(t);
  });
  _ADSBLayer = new ADSBOverlay({ map: map }, []);

  Timers['getADSB'] = window.setTimeout(getADSB, 10, _ADSBLayer);
});


function setADSBLine(t) {

  var alt = t.attr('data-alt');
  var lat = t.attr('data-lat').toString();
  var lng = t.attr('data-lng').toString();
  //   var Position = { lat: lt, lng: lg };


  var Path = [
    { lat: parseFloat(lat), lng: parseFloat(lng) },
    { lat: _RPASIconData.Lat, lng: _RPASIconData.Lng }
  ];
  _InfoWindowLine.setPath(Path);
  _InfoWindowLine.setMap(map);

}

function getADSB(ADSBObj) {
  var ADSBURL = "";
  if (IsLive) {
    ADSBURL = "/FlightMap/ADSBData"
  } else {
    if (_RPASIconData == null) {
      Timers['getADSB'] = window.setTimeout(getADSB, UpdateDelay, ADSBObj);
      return;
    }
    ADSBURL = "/FlightMap/ADSBHistory?History=" + toDate(_RPASIconData.FlightTime);
  }
  
  // var QueryData = $('input.spinner, input.query').serialize();
  $.ajax({
    type: "GET",
    url: '/FlightMap/ADSBData',
    contentType: "application/json",
    success: function (data) {
      ADSBObj.setADSB(data);
    },
    error: function (data, text) {
      //alert('Failed to fetch flight: ' + data);
    },
    complete: function () {
      Timers['getADSB'] = window.setTimeout(getADSB, UpdateDelay, ADSBObj);
    }
  });
}


function toDate(JsonDate) {
  var Month = new Array("January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December");
  //var Dt = Date(JsonDate.replace(/[\/"]/g, ""));
  var Dt = new Date(JsonDate.match(/\d+/)[0] * 1);
  var dd = "0" + Dt.getDate();
  var MM = Dt.getMonth();
  var yyyy = Dt.getFullYear();
  var HH = "0" + Dt.getHours();
  var mm = "0" + Dt.getMinutes();
  var ss = "0" + Dt.getSeconds();
  
  var TheDate =
    dd.substring(dd.length - 2) + "-" +
    Month[MM].substr(0, 3) + "-" +
    yyyy + "&nbsp;" +
    HH.substring(HH.length - 2) + ":" +
    mm.substring(mm.length - 2) + ":" +
    ss.substring(ss.length - 2);

  return TheDate;
}

function ADSBOverlay(options, ADSBData) {
  this.setValues(options);
  this.markerLayer = $('<div />').addClass('overlay');
  this.ADSBData = ADSBData;
  this.ADSBDrawn = {};
  this.ADSBActive = [];

  this.getIconFor = function (FlightID, Angle) {
    var IsDrone = (FlightID.substr(0, 3) == 'A00');
    var ReturnHTML = '';
    var Icon = '/images/Airline.png';

    if (IsDrone) {
      ReturnHTML = '<span class="icon DroneIcon" style=" transform: rotate(' + Angle + 'deg);"><img src="' + Icon + '" width="25" height="25"/></span>'
    } else {
      ReturnHTML = '<span class="icon FlightIcon" style=" transform: rotate(' + Angle + 'deg);"><img src="' + Icon + '" width="25" height="25"/></span>'
    }
    return ReturnHTML;
  };
  
  this.setADSB = function (ADSBData) {
    this.ADSBData = ADSBData;
    this.draw();
  };
}

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

  if (!projection) return false;
  this.ADSBActive = [];

  for (var i = 0; i < this.ADSBData.length; i++) {
    var lat = this.ADSBData[i].Lat;
    var lng = this.ADSBData[i].Lon;
    var alt = this.ADSBData[i].Altitude;
    var title = this.ADSBData[i].FlightID.trim();
    var heading = this.ADSBData[i].Heading;
    heading = parseFloat(heading);
    if (isNaN(heading)) heading = 0;
    if (title.substr(0, 3) == 'A00') continue;
    // Determine a random location from the bounds set previously  
    var IconGeoPos = new google.maps.LatLng(lat, lng)
    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);
    var DivID = 'adsb-' + title;
    var IconClass = 'drone';
    var $point = null;
    var RotateAngle = 45;
    this.ADSBActive.push(DivID);
    var Icon = this.getIconFor(title, heading);

    if (this.ADSBDrawn[DivID]) {
      $point = $('#' + DivID);
      $point.animate({ left: IconLocation.x, top: IconLocation.y });
      $point.attr('class', 'adsb-point ' + IconClass);
      $point.find(".icon").css({ transform: 'rotate(' + (heading) + 'deg)' });
      $point.attr({ 'data-lat': lat, 'data-lng': lng, 'data-alt': alt });
      //$point.find('img').attr('src', Icon);

      if (DivID == _InfoWindowKey) {
        _InfoWindow.setPosition(IconGeoPos);
        setADSBLine($point);
      }
    } else {
      var $NewPoint = $(
        '<div  class="adsb-point ' + IconClass + '" id="' + DivID + '" title="' + title + '" '
        + 'data-lat="' + lat + '" '
        + 'data-lng="' + lng + '" '
        + 'data-alt="' + alt + '" '
        + 'data-ident="' + title + '" '
        + 'style="left:' + IconLocation.x + 'px; top:' + IconLocation.y + 'px;">'
        + '<canvas id="canvas_' + DivID + '"></canvas>' 
        + Icon 
        + '<span class="flight-title" style="">' + title.replace('A00', 'SC0') + '</span>' 
        + '</div>'
      );
      // Append the HTML to the fragment in memory  
      this.markerLayer.append($NewPoint.get(0));
      $point = $('#' + DivID);
    }

    /*
    var TailHTML = getTail(this.ADSBData[i]);
    $point.find('.tail').remove();
    $point.append(TailHTML);    
    */
    setTail(DivID, this.ADSBData[i]);
    this.ADSBDrawn[DivID] = true;

  }//for (var i = 0)

  //if the item does not exists, then remove it from the screen
  var OldKeys = Object.keys(this.ADSBDrawn);  
  for (i = 0; i < OldKeys.length; i++) {
    var OldKey = OldKeys[i];
    var OldKeyIndex = this.ADSBActive.indexOf(OldKey);
    if (OldKeyIndex >= 0) {
      //keep the key
    } else {
      $('#' + OldKey).remove();
      if (_InfoWindowKey == OldKey) {
        _InfoWindow.close();
        _InfoWindowLine.setMap(null);
        _InfoWindowLine = '';
      }
    }
  }//for (var key in gADSBData)

  this.ADSBDrawn = {};
  for (i = 0; i < this.ADSBActive.length; i++) {
    var NewKey = this.ADSBActive[i];
    this.ADSBDrawn[NewKey] = true;
  }//for (var 

};

function setTail(ID, ADSBData) {
  var HTML = '';
  var History = ADSBData.History;
  if (History.length < 1) History.push(ADSBData.Heading);

  //If the history length is less than 5, push the last pos untill it reaches 5
  var LastHeading = History[History.length - 1];
  for (var i = History.length - 1; i < 5; i++) {
    History.push(LastHeading);
  }

  //ok. now generate html for each point
  var Center = 50;
  var Distance = 15;
  var CanvasObject = document.getElementById("canvas_" + ID);
  if (!CanvasObject) return;
  CanvasObject.width = 100;
  CanvasObject.height = 100;
  var ctx = CanvasObject.getContext("2d");
  ctx.clearRect(0, 0, ctx.width, ctx.height);
  ctx.strokeStyle = 'rgba(255,255,0,120)';
  ctx.setLineDash([1, 1]);
  ctx.beginPath();
  ctx.moveTo(Center, Center);

  for (var i = History.length - 1; i >= 1; i--) {
    var xy = lineToAngle(Center, Center, Distance, History[i]);    
    ctx.lineTo(xy.x, xy.y);   
    ctx.moveTo(xy.x, xy.y);
    Distance = Distance + 4;
  }
  ctx.stroke();
  

  //ctx.fillStyle = 'red';
  //ctx.fillRect(10, 10, 90, 90);

}

function getTail(ADSBData) {
  var HTML = '';
  var History = ADSBData.History;
  if (History.length < 1) History.push(ADSBData.Heading);

  //If the history length is less than 5, push the last pos untill it reaches 5
  var LastHeading = History[History.length - 1];
  for (var i = History.length - 1; i < 5; i++) {
    History.push(LastHeading);
  }

  //ok. now generate html for each point
  var Center = 50;
  var Distance = 15;
  for (var i = History.length - 1; i >= 0 ; i--) {
    var xy = lineToAngle(Center, Center, Distance, History[i]);
    HTML += '<span class="tail" style="left:' + xy.x + 'px; top:' + xy.y + 'px"></span>';
    Distance += 4;
  }




  return HTML;
}



function lineToAngle(x1, y1, length, angle) {

  angle = angle + 90;
  if (angle > 360) angle = angle - 360;

  angle *= Math.PI / 180;

  var x2 = x1 + length * Math.cos(angle),
    y2 = y1 + length * Math.sin(angle);


  return { x: x2, y: y2 };
}


function ShowInfoWindow(t) {
  if (t == null) {
    if (_InfoWindowKey == '') return;
    t = $('#' + _InfoWindowKey);
  }

  var title = t.attr('data-ident');

  var FeetToMeter = 0.3048;

  var alt = parseFloat(t.attr('data-alt'));
  var lt = parseFloat(t.attr('data-lat').toString());
  var lg = parseFloat(t.attr('data-lng').toString());
  //   var Position = { lat: lt, lng: lg };

  var vDistance = alt - (_RPASIconData.Altitude / FeetToMeter);
  var hDistance = getDistance({ lat: lt, lng: lg }, { lat: _RPASIconData.Lat, lng: _RPASIconData.Lng });
  var Content =
    '<div class="InfoWindow">' +
    '<div><b>' + title + '</b></div>' +
    '<div><span class="caption">Location</span>:<span class="value">' + lt + '&deg;N ' + lg  + '&deg;E</span></div>\n' +
    '<div><span class="caption">Altitude</span>:<span class="feet">' + alt.toFormatted(0) + ' Feet</span><span class="meter">(' + (alt * FeetToMeter).toFormatted(0) + ' Meter)</div>\n' +
    '<div class="hr"></div>' +
    '<div><span class="caption">Horizontal Distance</span>:<span class="feet">' + (hDistance / FeetToMeter).toFormatted(0) + ' Feet</span><span class="meter">(' + hDistance.toFormatted(0) + ' Meter)</div>\n' +
    '<div><span class="caption">Vertical Distance</span>:<span class="feet">' + vDistance.toFormatted(0) + ' Feet</span><span class="meter">(' + (vDistance * FeetToMeter).toFormatted(0) + ' Meter)</div>\n' +
    '</div>';

  

  var myLatlng = new google.maps.LatLng(lt, lg);
  _InfoWindow.setPosition(myLatlng);
  _InfoWindow.setContent(Content);
  _InfoWindow.open(map);
  setADSBLine(t);
}

var rad = function (x) {
  return x * Math.PI / 180;
};

Number.prototype.toFormatted = function (c) {
  var n = this,
    c = isNaN(c = Math.abs(c)) ? 2 : c,
    d = ".",
    t = ",",
    s = n < 0 ? "-" : "",
    i = String(parseInt(n = Math.abs(Number(n) || 0).toFixed(c))),
    j = (j = i.length) > 3 ? j % 3 : 0;
  return s + (j ? i.substr(0, j) + t : "") + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + t) + (c ? d + Math.abs(n - i).toFixed(c).slice(2) : "");
};

var getDistance = function (p1, p2) {
  var R = 6378137; // Earth’s mean radius in meter
  var dLat = rad(p2.lat - p1.lat);
  var dLong = rad(p2.lng - p1.lng);
  var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
    Math.cos(rad(p1.lat)) * Math.cos(rad(p2.lat)) *
    Math.sin(dLong / 2) * Math.sin(dLong / 2);
  var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
  var d = R * c;
  return d; // returns the distance in meter
};