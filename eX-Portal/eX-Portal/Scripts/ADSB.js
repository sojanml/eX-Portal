var _InfoWindowKey = '';
var AlertRef = {}
var BreachRef = {}
var _InfoWindow = new google.maps.InfoWindow({
  content: '<div id="InfoWindowContent">...</div>'
});

$(document).ready(function () {
  $(document).on("click", "div.adsb-point", function () {
    var t = $(this);
    _InfoWindowKey = t.attr('id');
    ShowInfoWindow();
  });

  var CheckBox = $('input.query#BreachLine').on("change", function () {
    if ($('input.query#BreachLine').is(':checked')) {
      _ADSBLayer.DrawLinesOf(BreachRef);
    } else {
      _ADSBLayer.DeleteLinesOf(_ADSBLayer.ADSBLines);
    }

  });
  var CheckBox = $('input.query#AlertLine').on("change", function () {
    if ($('input.query#AlertLine').is(':checked')) {
      _ADSBLayer.DrawLinesOfAlert(AlertRef);
    } else {
      _ADSBLayer.DeleteLinesOf(_ADSBLayer.ADSBAlertLines);
    }

  });
});

var StatusInfo = {
  'Breach': {},
  'Alert': {},
  'Safe': {},
  WatchingFlights: {},
  Reset: function () {
    this.Breach.Aircraft = {};
    this.Breach.RPAS = {};
    this.Alert.Aircraft = {};
    this.Alert.RPAS = {};
    this.Safe.Aircraft = {};
    this.Safe.RPAS = {};
  },
  SetWatchingFlight: function (Data) {
    WatchingFlights = Data;
  }
}

function getADSB(ADSBObj) {
  var QueryData = $('input.spinner, input.query').serialize();
  $.ajax({
    type: "GET",
    url: '/ADSB?' + QueryData,
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

/*
function getStatus(ADSBObj) {
  var QueryData =
    $('input.spinner, input.query').serialize() +
    '&IsQueryChanged=' + IsQueryChanged;
  IsQueryChanged = 0;

  $.ajax({
    type: "GET",
    url: '/ADSB/Distance?' + QueryData,
    contentType: "application/json",
    success: function (data) {
      //console.log(data);
      setStatusSummary(data, ADSBObj);
    },
    error: function (data, text) {
      //alert('Failed to fetch flight: ' + data);
    },
    complete: function () {
      //$("input.spinner").spinner("enable");
      Timers['getStatus'] = window.setTimeout(getStatus, UpdateDelay, ADSBObj);
    }
  });
}

*/

function setStatusSummary(StatusData, ADSBObj) {
  StatusInfo.Reset();
  BreachRef = {};
  AlertRef = {};
  for (var i = 0; i < StatusData.length; i++) {
    var Stat = StatusData[i];
    var RpasID = Stat.FromFlightID;
    var AircraftID = Stat.ToFlightID;
    var StatKey = Stat.Status;
    if (!(RpasID in StatusInfo[StatKey]['RPAS'])) StatusInfo[StatKey]['RPAS'][RpasID] = 1;
    if (!(AircraftID in StatusInfo[StatKey]['Aircraft'])) StatusInfo[StatKey]['Aircraft'][AircraftID] = 1;

    if (StatKey == 'Breach') {
      if (BreachRef[RpasID] == undefined) BreachRef[RpasID] = [];
      BreachRef[RpasID].push(AircraftID);

    }
    if (StatKey == 'Alert') {
      if (AlertRef[RpasID] == undefined) AlertRef[RpasID] = [];
      AlertRef[RpasID].push(AircraftID);
    }
  }//for


  ['Safe', 'Breach', 'Alert'].forEach(function (Stat, index, array) {
    var RPASList = [];
    var RPASLabels = '';
    var aRPAS = Object.keys(StatusInfo[Stat]['RPAS']);
    aRPAS.forEach(function (elem) {
      RPASLabels = '<div class="label-auto">' + elem + '</div>' + RPASLabels;
    })
    var aAircraft = Object.keys(StatusInfo[Stat]['Aircraft']);
    $('#' + Stat + '-Aircraft-Count').html(aAircraft.length);
    $('#' + Stat + '-RPAS-Count').html(aRPAS.length);
    $('#' + Stat + '-Aircraft').html(aAircraft.join(", "));
    $('#' + Stat + '-RPAS').html(RPASLabels);
  });

  var Flights = StatusInfo['Breach']['RPAS'];
  ADSBObj.SetWatchingFlight(Flights);
  var TheDatas = ADSBObj.GetWatchingFlight()

  setStatusWatching(TheDatas);

  if ($('input.query#BreachLine').is(':checked')) {
    ADSBObj.DrawLinesOf(BreachRef);
  }
  if ($('input.query#AlertLine').is(':checked')) {
    ADSBObj.DrawLinesOfAlert(AlertRef);
  }
}

function setStatusWatching(WatchingFlights) {
  var FlightIDs = Object.keys(WatchingFlights);
  var HTML = '';
  for (var i = 0; i < FlightIDs.length; i++) {
    var Flight = WatchingFlights[FlightIDs[i]];
    HTML += '<div class="row">\n' +
      '<div class="col1">' + toDate(Flight.ADSBDate) + '</div>\n' +
      '<div class="col2">' + Flight.FlightID + '</div>\n' +
      '<div class="col3">' + Flight.Lat + '</div>\n' +
      '<div class="col4">' + Flight.Lon + '</div>\n' +
      '<div class="col5">' + Flight.Speed + '</div>\n' +
      '<div class="col6">' + Flight.Heading + '</div>\n' +
      '</div>\n';
  }
  $('#WatchingFlightsScroll')
    .html(HTML)
    .css({ top: 0 });
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
  this.FlightsOnMap = {};
  this.FlightsBeforeDrawing = {};
  this.WatchingFlightIDs = [];
  this.SkyCommander = [];

  this.BreachLineReference = [];
  this.AlertLineReference = [];

  this.setADSB = function (ADSBData) {
    this.ADSBData = ADSBData;
    this.DrawCount++;
    this.GetWatchingFlight();
    this.draw(true);
    this.SkyCommander = [];
    for (var i = 0; i < this.ADSBData.length; i++) {
      var FlightData = this.ADSBData[i];
      if (FlightData.FlightSource == 'SkyCommander') {
        this.SkyCommander.push(FlightData.FlightID.trim());
      }//if (this.ADSBData[i].FlightSource)
    }//for (var i = 0; i < this.ADSBData.length; i++)
  },


    this.GetWatchingFlight = function () {
      this.ADSBFlightRef = {};
      var FlightIDs = this.WatchingFlightIDs;

      for (var i = 0; i < this.ADSBData.length; i++) {
        var FlightID = this.ADSBData[i].FlightID.trim();
        if (FlightIDs.indexOf(FlightID) >= 0)
          this.ADSBFlightRef[FlightID] = this.ADSBData[i];
      }
      return this.ADSBFlightRef;
    }

  this.SetWatchingFlight = function (oFlightIDs) {
    this.WatchingFlightIDs = Object.keys(oFlightIDs);
  };

  this.getIconFor = function (FlightData) {
    var ReturnHTML = '';
    var Angle = parseInt(FlightData.Heading);
    if (isNaN(Angle)) Angle = 0;
    var Icon = this.getIconImage(FlightData);
    ReturnHTML = '<span class="icon FlightIcon" style="transform: rotate(' + Angle + 'deg);"><img src="' + Icon + '" width="25" height="25"/></span>'
    return ReturnHTML;
  }

  this.getIconImage = function (FlightData) {
    var Icon = '/images/Airline.png';
    if ((FlightData.FlightSource == 'SkyCommander')) {
      if (FlightData.BreachToFlights.length > 0)
        Icon = '/images/Drone-breach.png';
      else if (FlightData.AlertToFlights.length > 0)
        Icon = '/images/Drone-alert.png';
      else
        Icon = '/images/Drone.png';
    }
    return Icon;
  }

}

ADSBOverlay.prototype = new google.maps.OverlayView;

ADSBOverlay.prototype.ClearLines = function() {
  //Clear Lines
  for (var i = 0; i < this.BreachLineReference.length; i++) {
    this.BreachLineReference[i].setMap(null);
  }
  for (var i = 0; i < this.AlertLineReference.length; i++) {
    this.AlertLineReference[i].setMap(null);
  }
  this.BreachLineReference = [];
  this.AlertLineReference = [];
}

ADSBOverlay.prototype.DrawLinesTo = function (DroneID) {
  if (this.map == null || this.map == undefined) return;

  var DroneIndex = this.FlightsOnMap[DroneID];
  var Drone = this.ADSBData[DroneIndex];
  if (!Drone) return;
  var CenterPos = { lat: Drone.Lat, lng: Drone.Lon };

  var BreachToFlights = Drone.BreachToFlights;
  for (var i = 0; i < BreachToFlights.length; i++) {
    var FlightID = BreachToFlights[i];
    var FlightIndex = this.FlightsOnMap[FlightID] + 0;
    if (FlightIndex >= 0) {
      var Flight = this.ADSBData[FlightIndex];
      var newPos = { lat: Flight.Lat, lng: Flight.Lon };

      var flightPath = new google.maps.Polyline({
        path: [CenterPos, newPos],
        strokeColor: '#fe0000',
        strokeOpacity: 0.8,
        strokeWeight: 1
      });
      this.BreachLineReference.push(flightPath);
    }
  }


  var AlertToFlights = Drone.AlertToFlights;
  for (var i = 0; i < AlertToFlights.length; i++) {
    var FlightID = AlertToFlights[i];
    var FlightIndex = this.FlightsOnMap[FlightID] + 0;
    if (FlightIndex >= 0) {
      var Flight = this.ADSBData[FlightIndex];
      var newPos = { lat: Flight.Lat, lng: Flight.Lon };

      var flightPath = new google.maps.Polyline({
        path: [CenterPos, newPos],
        strokeColor: '#fefe00',
        strokeOpacity: 0.5,
        strokeWeight: 1
      });
      this.AlertLineReference.push(flightPath);
    }
  }

  for (var i = 0; i < this.BreachLineReference.length; i++) {
    this.BreachLineReference[i].setMap(map);
  }

  for (var i = 0; i < this.AlertLineReference.length; i++) {
    this.AlertLineReference[i].setMap(map);
  }
}; //this.DrawLinesOf


ADSBOverlay.prototype.onAdd = function () {
  var $pane = $(this.getPanes().overlayImage); // Pane 3  
  $pane.append(this.markerLayer);
};

ADSBOverlay.prototype.onRemove = function () {
  this.markerLayer.remove();
};

ADSBOverlay.prototype.SetReverseReference = function (DroneID) {
  if (this.map == null || this.map == undefined) return;

  var DroneIndex = this.FlightsOnMap[DroneID];
  var Drone = this.ADSBData[DroneIndex];
  if (!Drone) return;

  var BreachToFlights = Drone.BreachToFlights;
  for (var i = 0; i < BreachToFlights.length; i++) {
    var FlightID = BreachToFlights[i];
    var FlightIndex = this.FlightsOnMap[FlightID] + 0;
    if (FlightIndex >= 0) {
      var Flight = this.ADSBData[FlightIndex];
      Flight.BreachToFlights.push(DroneID);
    }
  }

  var AlertToFlights = Drone.AlertToFlights;
  for (var i = 0; i < AlertToFlights.length; i++) {
    var FlightID = AlertToFlights[i];
    var FlightIndex = this.FlightsOnMap[FlightID] + 0;
    if (FlightIndex >= 0) {
      var Flight = this.ADSBData[FlightIndex];
      Flight.AlertToFlights.push(DroneID);
    }
  }
};

ADSBOverlay.prototype.GetData = function (FlightID) {
  var FlightIndex = this.FlightsOnMap[FlightID] + 0;
  if (FlightIndex >= 0)
    return this.ADSBData[FlightIndex];
  return null;
}


ADSBOverlay.prototype.draw = function (IsSetADSB) {
  var projection = this.getProjection();
  if (!projection) return false;
  this.FlightsOnMap = {};

  for (var i = 0; i < this.ADSBData.length; i++) {
    var lat = this.ADSBData[i].Lat;
    var lng = this.ADSBData[i].Lon;
    var alt = this.ADSBData[i].Altitude;
    var title = this.ADSBData[i].FlightID.trim();    
    var heading = this.ADSBData[i].Heading;
    var IconClass = 'drone';
    var DivID = 'adsb-' + this.ADSBData[i].HexCode.trim();
    heading = parseFloat(heading);
    if (isNaN(heading)) heading = 0;
    //save what is on map
    this.FlightsOnMap[title] = i;

    // Determine a random location from the bounds set previously  
    var IconGeoPos = new google.maps.LatLng(lat, lng)
    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);
    var $point = $('#' + DivID);

    if (heading == 0) {
      //Landed flight - Ignore movement
    } else if ($point.length) {
      $point.animate({ left: IconLocation.x, top: IconLocation.y });
      $point.find(".icon").css({ transform: 'rotate(' + (heading) + 'deg)' });
      $point.attr({ 'data-lat': lat, 'data-lng': lng, 'data-alt': alt });
      $point.find('img').attr('src', this.getIconImage(this.ADSBData[i]));
    } else {
      var Icon = this.getIconFor(this.ADSBData[i]);
      var $NewPoint = $(
        '<div  class="adsb-point ' + IconClass + '" id="' + DivID + '" title="' + title + '" '
        + 'data-lat="' + lat + '" '
        + 'data-lng="' + lng + '" '
        + 'data-alt="' + alt + '" '
        + 'data-ident="' + title + '" '
        + 'style="left:' + IconLocation.x + 'px; top:' + IconLocation.y + 'px;">'
        + '<canvas id="canvas_' + DivID + '"></canvas>'
        + Icon
        + '<span class="flight-title" style="">' + title + '</span>'
        + '</div>'
      );
      // Append the HTML to the fragment in memory  
      this.markerLayer.append($NewPoint.get(0));
      $point = $('#' + DivID);
    }

    setTail(DivID, this.ADSBData[i]);

  }//for (var i = 0)

  if (IsSetADSB) {
    if (this.SkyCommander.length > 0) {
      for (var i = 0; i < this.SkyCommander.length; i++) {
        this.ClearLines();
        this.DrawLinesTo(this.SkyCommander[i]);
        this.SetReverseReference(this.SkyCommander[i]);
      }
    } else {
      this.ClearLines();
    }
  }


  //if the item does not exists, then remove it from the screen
  var AllKeys = Object.keys(this.FlightsBeforeDrawing);
  for (i = 0; i < AllKeys.length; i++) {
    var TheKey = AllKeys[i];
    if (this.FlightsOnMap[TheKey] + 0 >= 0) {
    } else {
      $('#adsb-' + TheKey).remove();
    }//if (gADSBData[key] > 20) 
  }//for (var key in gADSBData)


  //clone the object - not the refence to object
  this.FlightsBeforeDrawing = JSON.parse(JSON.stringify(this.FlightsOnMap));
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
  for (var i = 1; i < History.length; i++) {
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


function ShowInfoWindow() {
  if (_InfoWindowKey == '') return;
  var FlightID = _InfoWindowKey.replace('adsb-', '');
  var Data = _ADSBLayer.GetData(FlightID);
  if (Data == null) return;
  if (Data.FlightSource == 'SkyCommander') {
    var DroneFlightID = FlightID.substr(3);
    FlightLink = '<a href="/FlightMap/Map/' + DroneFlightID + '">View Flight</a>\n';
  } else {
    FlightLink = '';
  }

  var InfoTitle = '';
  var AircraftCode = FlightID.substr(0, 3).toUpperCase();
  if (AircraftDB[AircraftCode])
    InfoTitle =
      '<div class="BigTitle">' + AircraftDB[AircraftCode]["IATA"] + ' ' + FlightID.substr(3).toUpperCase() +
        '<span>' + AircraftDB[AircraftCode]["Name"] + '</span>' +
      '</div>' +
      '<div class="SubTitle">' + AircraftDB[AircraftCode]["Country"] + '</div>';
  else
    InfoTitle = '<div class="BigTitle">' + FlightID.toUpperCase() + '</div>';

  var alt = Data.Altitude;
  var lt = Data.Lat;
  var lg = Data.Lon;
  var FeetToMeter = 0.3048;

  var Content =
    '<div class="InfoWindow">' +
    '<div class="Header">' + InfoTitle + '</div>' + 
    '<div class="Location">Location: <span>' + lt + '&deg;N, ' + lg + '&deg;E</span></div>\n' +
    '<div class="Altitude">Altitude:<span class="feet">' + alt.toFormatted(0) + ' Feet</span><span class="meter">(' + (alt * FeetToMeter).toFormatted(0) + ' Meter)</span></div>\n' +
    '<div>Breach Alerts to: ' + Data.BreachToFlights.join(',') + '</div>\n' +
    '<div>Alerts to: ' + Data.AlertToFlights.join(',') + '</div>\n' +
    FlightLink +
    '</div>';
  var myLatlng = new google.maps.LatLng(lt, lg);
  _InfoWindow.setPosition(myLatlng);
  _InfoWindow.setContent(Content);
  _InfoWindow.open(map);
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