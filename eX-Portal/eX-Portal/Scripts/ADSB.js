var _InfoWindowKey = '';
var AlertRef = {}
var BreachRef = {}
var _InfoWindow = null;

$(document).ready(function () {
  $(document).on("click", "div.adsb-point", function (e) {
    //e.stopPropagation();
    var t = $(this);
    _InfoWindowKey = t.attr('id');
    ShowInfoWindow();
  });
  $(document).on("click", "#infoLayerClose", function () {
    _InfoWindowKey = '';
    _ADSBLayer.hideInfoLayer();
  });

  


  $('input#BreachLine').on("change", ShowHideLines);
  $('input#AlertLine').on("change", ShowHideLines);

  $(document).on("mouseenter", 'span.SkyCommander', function () { _ADSBLayer.ShowCircles($(this)) });
  $(document).on("mouseleave", 'span.SkyCommander', function () { _ADSBLayer.HideCircles($(this)) });

  _InfoWindow = new google.maps.InfoWindow({
    content: '<div id="InfoWindowContent">...</div>'
  });
  google.maps.event.addListener(_InfoWindow, 'closeclick', function () {
    _InfoWindowKey = '';
  });




});

function ShowHideLines() {
  var Breach = $('input#BreachLine').length > 0 ? $('input#BreachLine').is(':checked') : InitQuery.BreachLine == 1;
  var Alert = $('input#AlertLine').is(':checked');
  _ADSBLayer.ShowHideLines(Breach, Alert);
  IsQueryChanged = 0;
}

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
  },

  AddStatusOf: function (FlightData) {
    for (var i = 0; i < FlightData.BreachToFlights.length; i++) {
      var thisFlightHex = FlightData.BreachToFlights[i];
      this.Breach.Aircraft[thisFlightHex] = true;
    }

    for (var i = 0; i < FlightData.AlertToFlights.length; i++) {
      var thisFlightHex = FlightData.AlertToFlights[i];
      this.Alert.Aircraft[thisFlightHex] = true;
    }
    var HexCode = FlightData.HexCode;

    if (FlightData.BreachToFlights.length > 0) {
      this.Breach.RPAS[HexCode] = true;
    }
    if (FlightData.AlertToFlights.length > 0) {
      this.Alert.RPAS[HexCode] = true;
    }
    if (FlightData.BreachToFlights.length == 0 && FlightData.AlertToFlights.length == 0) {
      this.Safe.RPAS[HexCode] = true;
    }

  },

  ShowStatus: function () {
    var BreachAircraft = Object.keys(this.Breach.Aircraft);
    var BreachRPAS = Object.keys(this.Breach.RPAS);
    $('#Breach-Aircraft-Count').html(BreachAircraft.length);
    $('#Breach-RPAS-Count').html(BreachRPAS.length);
    $('#Breach-Aircraft').html(this.NiceNames(BreachAircraft));

    var AlertAircraft = Object.keys(this.Alert.Aircraft);
    var AlertRPAS = Object.keys(this.Alert.RPAS);
    $('#Alert-Aircraft-Count').html(AlertAircraft.length);
    $('#Alert-RPAS-Count').html(AlertRPAS.length);
    $('#Alert-RPAS').html(AlertRPAS.join(", "));
    $('#Alert-Aircraft').html(this.NiceNames(AlertAircraft));

    var SafeRPAS = Object.keys(this.Safe.RPAS);
    $('#Safe-RPAS-Count').html(SafeRPAS.length);
    $('#Safe-RPAS').html(SafeRPAS.join(", "));

    setStatusWatching(BreachRPAS);
  },

  NiceNames: function (HexCodes) {
    var Names = [];
    for (var i = 0; i < HexCodes.length; i++) {
      var Data = _ADSBLayer.GetData(HexCodes[i]);
      if (Data != null) Names.push(Data.NiceName);
    }
    return Names.join(", ");
  }
}

function getADSB(ADSBObj) {
  var QueryData = $('input.spinner, input.query, input.QueryModel').serialize();
  if (QueryData == "") {
    QueryData = window.location.search.substr(1);
  } else {
    QueryData = QueryData + '&IsQueryChanged=' + IsQueryChanged;
  }

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
      if (Timers['getADSB']) window.clearTimeout(Timers['getADSB']);
      IsQueryChanged = 0;
      Timers['getADSB'] = window.setTimeout(getADSB, UpdateDelay, ADSBObj);
    }
  });
}

function setStatusWatching(FlightIDs) {
  var HTML = '';
  for (var i = 0; i < FlightIDs.length; i++) {
    var Flight = _ADSBLayer.GetData(FlightIDs[i]);
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
  this.infoLayer = $('<div style="display:none;" id="infoLayer">' +
    '<div id="infoLayerContent"></div>' +
    '<div id="infoLayerClose"><span class="icon red">&#xf057;</icon></div>' +
    '</div > ');
  this.ADSBData = ADSBData;
  this.FlightsOnMap = {};
  this.FlightsBeforeDrawing = {};
  this.WatchingFlightIDs = [];
  this.SkyCommander = [];
  this.Circles = {};
  this.BreachLineReference = [];
  this.AlertLineReference = [];

  this.hideInfoLayer = function() {
    $('#infoLayer').fadeOut();
  };

  this.showInfoLayer = function (Content) {
    var projection = this.getProjection();
    if (!projection) return false;

    var HexCode = _InfoWindowKey.replace('adsb-', '');
    var Data = this.GetData(HexCode);
    var FlightID = (Data.FlightID ? Data.FlightID : "");

    var lat = Data.Lat;
    var lng = Data.Lon;

    var IconGeoPos = new google.maps.LatLng(lat, lng)
    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);

    $('#infoLayerContent')
      .html(Content);

    $('#infoLayer')
      .animate({ left: IconLocation.x, top: IconLocation.y })
      .show();

  };

  this.setADSB = function (ADSBData) {
    this.ADSBData = ADSBData;
    this.DrawCount++;
    this.GetWatchingFlight();

    this.SkyCommander = [];
    StatusInfo.Reset();
    for (var i = 0; i < this.ADSBData.length; i++) {
      var FlightData = this.ADSBData[i];
      FlightData.NiceName = FlightData.FlightID.trim();

      if (FlightData.FlightSource == 'SkyCommander') {
        this.SkyCommander.push(FlightData.FlightID.trim());
        StatusInfo.AddStatusOf(FlightData);
      } else {
        var AircraftCode = FlightData.FlightID.substr(0, 3).toUpperCase();
        if (AircraftDB[AircraftCode]) FlightData.NiceName = AircraftDB[AircraftCode]["IATA"] + ' ' + FlightData.FlightID.substr(3).toUpperCase();
      
      }//if (this.ADSBData[i].FlightSource)
    }//for (var i = 0; i < this.ADSBData.length; i++)

    this.draw(true);
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
    var IconClass = FlightData.FlightSource == "SkyCommander" ? "SkyCommander" : "FlightIcon";
    ReturnHTML = '<span class="icon ' + IconClass + '" style="transform: rotate(' + Angle + 'deg);">' +
      '<img src="' + Icon + '" width="25" height="25" />' +
      '</span >'
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

};

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

ADSBOverlay.prototype.ShowHideLines = function (Breach, Alert) {
  for (var i = 0; i < this.BreachLineReference.length; i++) {
    this.BreachLineReference[i].setMap(Breach ? map : null);
  }

  for (var i = 0; i < this.AlertLineReference.length; i++) {
    this.AlertLineReference[i].setMap(Alert ? map : null);
  }
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


  var IsBreachLines = $('input#BreachLine').length > 0 ? $('input#BreachLine').is(':checked') : InitQuery.BreachLine == '1';
  var IsAlertLines = $('input#AlertLine').length > 0 ? $('input#AlertLine').is(':checked') : InitQuery.AlertLine == '1';
  

  this.ShowHideLines(IsBreachLines, IsAlertLines);

}; //this.DrawLinesOf


ADSBOverlay.prototype.onAdd = function () {
  var $pane = $(this.getPanes().overlayImage); // Pane 3  
  $pane.append(this.markerLayer);
  $pane.append(this.infoLayer);
};

ADSBOverlay.prototype.onRemove = function () {
  this.markerLayer.remove();
  this.infoLayer.remove();
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


ADSBOverlay.prototype.ShowCircles = function (Span) {
  var ID = Span.parent().attr("id").replace("adsb-", "");
  //console.log(Span);
}
ADSBOverlay.prototype.HideCircles = function (Span) {
  //console.log(Span);
}

ADSBOverlay.prototype.ClearCircles = function() {

}


ADSBOverlay.prototype.draw = function (IsSetADSB) {
  var projection = this.getProjection();
  if (!projection) return false;
  if (this.ADSBData.length < 1) return;
  this.FlightsOnMap = {};

  for (var i = 0; i < this.ADSBData.length; i++) {
    var lat = this.ADSBData[i].Lat;
    var lng = this.ADSBData[i].Lon;
    var alt = this.ADSBData[i].Altitude;
    var title = this.ADSBData[i].FlightID.trim();    
    var heading = this.ADSBData[i].Heading;
    var IconClass = 'drone';
    var HexCode = this.ADSBData[i].HexCode.trim();
    if (HexCode == '') {
      this.ADSBData[i].HexCode =  HexCode = this.ADSBData[i].FlightID.trim();     
    }
    var DivID = 'adsb-' + HexCode;
    heading = parseFloat(heading);
    if (isNaN(heading)) heading = 0;
    //save what is on map
    this.FlightsOnMap[HexCode] = i;

    // Determine a random location from the bounds set previously  
    var IconGeoPos = new google.maps.LatLng(lat, lng)
    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);
    var $point = $('#' + DivID);

    if ($point.length) {
      $point.animate({ left: IconLocation.x, top: IconLocation.y });
      $point.find(".icon").css({ transform: 'rotate(' + (heading) + 'deg)' });
      $point.attr({ 'data-lat': lat, 'data-lng': lng, 'data-alt': alt });
      $point.find('img').attr('src', this.getIconImage(this.ADSBData[i]));
    } else {
      var Icon = this.getIconFor(this.ADSBData[i]);

      var $NewPoint = $(
        '<div  class="adsb-point ' + IconClass + '" id="' + DivID + '" title="' + this.ADSBData[i].NiceName + '" '
        + 'data-lat="' + lat + '" '
        + 'data-lng="' + lng + '" '
        + 'data-alt="' + alt + '" '
        + 'data-ident="' + this.ADSBData[i].NiceName + '" '
        + 'style="left:' + IconLocation.x + 'px; top:' + IconLocation.y + 'px;">'
        + '<canvas style="z-Index:0" id="canvas_' + DivID + '"></canvas>'
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
      this.ClearLines();
      for (var i = 0; i < this.SkyCommander.length; i++) {
        this.DrawLinesTo(this.SkyCommander[i]);
        this.SetReverseReference(this.SkyCommander[i]);
      }
    } else {
      this.ClearLines();
    }
    StatusInfo.ShowStatus();

  }


  //if the item does not exists, then remove it from the screen
  var AllKeys = Object.keys(this.FlightsBeforeDrawing);
  for (i = 0; i < AllKeys.length; i++) {
    var TheKey = AllKeys[i];
    if (this.FlightsOnMap[TheKey] + 0 >= 0) {
    } else {
      $('#adsb-' + TheKey).remove();
      if (_InfoWindowKey == TheKey) {
        _InfoWindow.close()
        _InfoWindowKey = "";
      }
    }//if (gADSBData[key] > 20) 
  }//for (var key in gADSBData)

  if (_InfoWindowKey != "") ShowInfoWindow();


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

  var HexCode = _InfoWindowKey.replace('adsb-', '');
  var Data = _ADSBLayer.GetData(HexCode);
  if (Data == null) return;
  var FlightID = (Data.FlightID ? Data.FlightID : "");

  var InfoTitle = '';
  var BreachInfo = '';
  var AlertInfo = '';

  var AircraftCode = FlightID.substr(0, 3).toUpperCase();

  if (AircraftDB[AircraftCode]) {
    /*
  InfoTitle =
    '<div class="BigTitle">' + AircraftDB[AircraftCode]["IATA"] + ' ' + FlightID.substr(3).toUpperCase() +
    '<span>' + AircraftDB[AircraftCode]["Name"] + '</span>' +
    '</div>';
    //'<div class="SubTitle">' + AircraftDB[AircraftCode]["Country"] + '</div>';
    */
    InfoTitle = AircraftDB[AircraftCode]["IATA"] + ' ' + FlightID.substr(3).toUpperCase() +
      ' (' + AircraftDB[AircraftCode]["Name"] + ')';
  } else {
    InfoTitle = '<div class="BigTitle">' + FlightID.toUpperCase() + '</div>';
  }
  

  var alt = Data.Altitude;
  var lt = Data.Lat;
  var lg = Data.Lon;
  var FeetToMeter = 0.3048;
  var MeterToFeet = 3.28084;
  var FlightLink = '';
  if (Data.FlightSource == 'SkyCommander') {
    var DroneFlightID = FlightID.substr(3);
    FlightLink = '<div class="Link"><a href="/FlightMap/Map/' + DroneFlightID + '">View Flight</a></div>\n';
  }
  if (Data.FlightSource != 'SkyCommander' && Data.BreachToFlights.length > 0) {
    BreachInfo = '';
    for (var i = 0; i < Data.BreachToFlights.length; i++) {
      var FlightHexCode = Data.BreachToFlights[i];
      var FlightData = _ADSBLayer.GetData(FlightHexCode);
      if (FlightData != null) {
        var vDist = alt - (FlightData.Altitude * MeterToFeet);
        var hDist = getDistance({ lat: lt, lng: lg }, { lat: FlightData.Lat, lng: FlightData.Lon });
        BreachInfo = BreachInfo +
          '<div class="BreachInfo">\n' +
          '<div class="BreachFlight">' + FlightData.FlightID + '</div>\n' +
          '<div class="vDist">Vertical Seperation (Altitude):<span class="feet">' + vDist.toFormatted(0) + ' Feet</span><span class="meter">(' + (vDist * FeetToMeter).toFormatted(0) + ' Meter)</span></div>\n' +
          '<div class="hDist">Horizondal Seperation:<span class="meter">' + (hDist / 1000).toFormatted(2) + ' KM</span><span class="feet">(' + (hDist * MeterToFeet).toFormatted(0) + ' Feet)</span></div>\n' +
          '</div>\n';
      }
    }
  }

  if (Data.FlightSource != 'SkyCommander' && Data.AlertToFlights.length > 0) {
    AlertInfo = '';
    for (var i = 0; i < Data.AlertToFlights.length; i++) {
      var FlightHexCode = Data.AlertToFlights[i];
      var FlightData = _ADSBLayer.GetData(FlightHexCode);
      if (FlightData != null) {
        var vDist = alt - (FlightData.Altitude * MeterToFeet);
        var hDist = getDistance({ lat: lt, lng: lg }, { lat: FlightData.Lat, lng: FlightData.Lon });
        AlertInfo = AlertInfo +
          '<div class="AlertInfo">\n' +
          '<div class="AlertFlight">' + FlightData.FlightID + '</div>\n' +
          '<div class="vDist">Vertical Separation (Altitude):<span class="feet">' + vDist.toFormatted(0) + ' Feet</span><span class="meter">(' + (vDist * FeetToMeter).toFormatted(0) + ' Meter)</span></div>\n' +
          '<div class="hDist">Horizontal Separation:<span class="meter">' + (hDist / 1000).toFormatted(2) + ' KM</span><span class="feet">(' + (hDist * MeterToFeet).toFormatted(0) + ' Feet)</span></div>\n' +
          '</div>\n';
      }
    }
  }

  var Content =
    '<div class="InfoWindowUpdate">' +
    '<div class="Header">' + InfoTitle + '</div>' +
    '<div class="Location"><span>' + lt.toFormatted(4) + '&deg;N, ' + lg.toFormatted(4) + '&deg;E</span></div>\n' +
    '<div class="Altitude"><span class="feet">' + alt.toFormatted(0) + ' Feet</span><span class="meter">(' + (alt * FeetToMeter).toFormatted(0) + ' Meter)</span></div>\n' +
    BreachInfo +
    AlertInfo +
    FlightLink +
    '</div>';

  _ADSBLayer.showInfoLayer(Content);

}

function ShowInfoWindow_old() {
  if (_InfoWindowKey == '') return;
  var HexCode = _InfoWindowKey.replace('adsb-', '');
  var Data = _ADSBLayer.GetData(HexCode);
  var FlightID = (Data.FlightID ? Data.FlightID : "");
  if (Data == null) return;


  var InfoTitle = '';
  var BreachInfo = '';
  var AlertInfo = '';

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
  var MeterToFeet = 3.28084;
  var FlightLink = '';
  if (Data.FlightSource == 'SkyCommander') {
    var DroneFlightID = FlightID.substr(3);
    FlightLink = '<div class="Link"><a href="/FlightMap/Map/' + DroneFlightID + '">View Flight</a></div>\n';
  } 
  if (Data.FlightSource != 'SkyCommander' && Data.BreachToFlights.length > 0) {
    BreachInfo = '';
    for (var i = 0; i < Data.BreachToFlights.length; i++) {
      var FlightHexCode = Data.BreachToFlights[i];
      var FlightData = _ADSBLayer.GetData(FlightHexCode);
      if (FlightData != null) {
        var vDist = alt - (FlightData.Altitude * MeterToFeet);
        var hDist = getDistance({ lat: lt, lng: lg }, { lat: FlightData.Lat, lng: FlightData.Lon });
        BreachInfo = BreachInfo +
          '<div class="BreachInfo">\n' +
          '<div class="BreachFlight">' + FlightData.FlightID + '</div>\n' +
          '<div class="vDist">Vertical Seperation (Altitude):<span class="feet">' + vDist.toFormatted(0) + ' Feet</span><span class="meter">(' + (vDist * FeetToMeter).toFormatted(0) + ' Meter)</span></div>\n' +
          '<div class="hDist">Horizondal Seperation:<span class="meter">' + (hDist / 1000).toFormatted(2) + ' KM</span><span class="feet">(' + (hDist * MeterToFeet).toFormatted(0) + ' Feet)</span></div>\n' +
          '</div>\n';
      }
    }
  }

  if (Data.FlightSource != 'SkyCommander' && Data.AlertToFlights.length > 0) {
    AlertInfo = '';
    for (var i = 0; i < Data.AlertToFlights.length; i++) {
      var FlightHexCode = Data.AlertToFlights[i];
      var FlightData = _ADSBLayer.GetData(FlightHexCode);
      if (FlightData != null) {
        var vDist = alt - (FlightData.Altitude * MeterToFeet);
        var hDist = getDistance({ lat: lt, lng: lg }, { lat: FlightData.Lat, lng: FlightData.Lon });
        AlertInfo = AlertInfo +
          '<div class="AlertInfo">\n' +
          '<div class="AlertFlight">' + FlightData.FlightID + '</div>\n' +
          '<div class="vDist">Vertical Separation (Altitude):<span class="feet">' + vDist.toFormatted(0) + ' Feet</span><span class="meter">(' + (vDist * FeetToMeter).toFormatted(0) + ' Meter)</span></div>\n' +
          '<div class="hDist">Horizontal Separation:<span class="meter">' + (hDist / 1000).toFormatted(2) + ' KM</span><span class="feet">(' + (hDist * MeterToFeet).toFormatted(0) + ' Feet)</span></div>\n' +
          '</div>\n';
      }
    }  
  }

  var Content =
    '<div class="InfoWindow">' +
    '<div class="Header">' + InfoTitle + '</div>' + 
    '<div class="Location">Location: <span>' + lt + '&deg;N, ' + lg + '&deg;E</span></div>\n' +
    '<div class="Altitude">Altitude:<span class="feet">' + alt.toFormatted(0) + ' Feet</span><span class="meter">(' + (alt * FeetToMeter).toFormatted(0) + ' Meter)</span></div>\n' +
    BreachInfo +
    AlertInfo +
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