ADSBOverlay.prototype = new google.maps.OverlayView;

/*{
  "FlightID": "01003D",
  "Heading": 312,
  "TailNumber": "01003D",
  "FlightSource": "Exponent",
  "CallSign": "01003D",
  "Lon": 53.371323,
  "Lat": 25.408584,
  "Speed": 399,
  "Altitude": 30000,
  "ADSBDate": "\/Date(1518435836000)\/",
  "HexCode": "01003D",
  "BreachToFlights": [],
  "AlertToFlights": [],
  "History": [313, 313, 313, 313, 312, 312]
}*/

var ADSBLoader = function () {
  var _ADSBObj = null;
  var ADSBTimer = null;
  var LastLoadDateTime = null;

  var LoadADSB = function (ADSBObj) {
    var RequestURL = '/FlightMap/ADSBData';
    if (!FlightInfo.IsLive && _ADSBObj.DronePosition != null) {
      var nDate = Util.toDateTime(_ADSBObj.DronePosition.FlightTime);
      if (LastLoadDateTime != null && nDate.getTime() == LastLoadDateTime.getTime()) {
        //Same date and time. not required to refresh.
        StartTimer();
        return;
      }
      LastLoadDateTime = nDate;
      RequestURL = '/FlightMap/ADSBHistory?History=' + Util.toString(nDate);
    }

    // var QueryData = $('input.spinner, input.query').serialize();
    $.ajax({
      type: "GET",
      url: RequestURL,
      contentType: "application/json",
      success: function (data) {
        _ADSBObj.setADSB(data);
      },
      error: function (data, text) {
        //alert('Failed to fetch flight: ' + data);
      },
      complete: StartTimer
    });

  };

  var StartTimer = function () {
    if (ADSBTimer) window.clearTimeout(ADSBTimer);
    ADSBTimer = window.setTimeout(LoadADSB, 1000);
  }

  var _init = function (ADSBObj) {
    _ADSBObj = ADSBObj;
    LoadADSB();
  };

  return {
    Init: _init
  };
}();

function ADSBOverlay(options) {
  this.setValues(options);
  this.markerLayer = $('<div />').addClass('ADSBOverlay');
  this.infoLayer = $('<div id="ADSBInfo"/>').addClass('ADSBInfo');
  this.ADSBData = {};
  this.ActiveAirlines = {};
  this.activeClicked = null;
  this.infoLine =  new google.maps.Polyline({
    path: [],
    geodesic: true,
    strokeColor: '#18bdec',
    strokeOpacity: 1.0,
    strokeWeight: 2
  });
  this.DronePosition = {};
  this.Clear = function () {
    this.hideInfoLine();
    this.ADSBData = {};
    this.ActiveAirlines = {};
    this.markerLayer.empty();
  };

  this.setADSB = function (Data) {
    for (var i = 0; i < Data.length; i++) {
      var ADSB = Data[i];
      if (ADSB.HexCode == null || ADSB.HexCode == "")
        ADSB.HexCode = ADSB.FlightID;
      this.ADSBData[ADSB.HexCode] = ADSB;
      this.ADSBData[ADSB.HexCode].UpdatedOn = new Date();
    }
    this.draw();
    this.showInfoLine();
  };

  this.setDroneAt = function (ActiveItem) {
    this.DronePosition = ActiveItem;
  };

  this.ADSBOnClick = function (elem) {
    if (elem == null) {
      this.hideInfoLine();
    } else {
      var HexCode = elem.attr('data-hexcode');
      this.activeClicked = HexCode;
      this.showInfoLine();
    }
  };

  this.showInfoLine = function () {
    if (this.activeClicked == null) return;
    if (this.infoLine == null) return;
    var projection = this.getProjection();
    if (projection == null) return;

    var HexCode = this.activeClicked;
    var ADSB = this.ADSBData[HexCode];
    var p1 = { lat: this.DronePosition.Lat, lng: this.DronePosition.Lng }
    var p2 = { lat: ADSB.Lat, lng: ADSB.Lon };
    this.infoLine.setPath([p1, p2]);
    this.infoLine.setMap(this.map);

    var IconGeoPos = new google.maps.LatLng(ADSB.Lat, ADSB.Lon);
    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);    
    var FeetToMeter = 0.3048;    
    var vDistance = ADSB.Altitude - (this.DronePosition.Altitude / FeetToMeter);
    var hDistance = Util.getDistance(p1, p2);    
    var AircraftCode = ADSB.FlightID.substr(0, 3).toUpperCase();
    var InfoTitle = ADSB.FlightID;

    if (AircraftDB[AircraftCode]) {
      InfoTitle = AircraftDB[AircraftCode]["IATA"] + ' ' + ADSB.FlightID.substr(3).toUpperCase() +
        ' (' + AircraftDB[AircraftCode]["Name"] + ')';
    } else {
      InfoTitle = '<div class="BigTitle">' + ADSB.FlightID.toUpperCase() + '</div>';
    }


    var Html =
      '<div class="InfoWindowUpdate">' +
      '<div class="Header">' + InfoTitle + '</div>' +
      '<div class="Location"><span>' + ADSB.Lat.toFixed(6) + '&deg;N, ' + ADSB.Lon.toFixed(6) + '&deg;E</span></div>\n' +
      '<div class="Altitude"><span class="feet">' + ADSB.Altitude.toFixed(0) + ' Feet</span><span class="meter">(' + (ADSB.Altitude * FeetToMeter).toFixed(0) + ' Meter)</span></div>\n' +
      '<div><span class="caption">Horizontal Distance</span>:<span class="feet">' + (hDistance / FeetToMeter).toFixed(0) + ' Feet</span><span class="meter">(' + hDistance.toFixed(0) + ' Meter)</div>\n' +
      '<div><span class="caption">Vertical Distance</span>:<span class="feet">' + vDistance.toFixed(0) + ' Feet</span><span class="meter">(' + (vDistance * FeetToMeter).toFixed(0) + ' Meter)</div>\n' +
      '</div>';


    this.infoLayer
      .html(Html)
      .css({ left: IconLocation.x + 8, top: IconLocation.y + 8 })
      .show();


  }

  this.hideInfoLine = function () {
    this.infoLine.setMap(null);
    this.activeClicked = null;
    this.infoLayer.hide();
  }

  this.SetOrAdd = function(projection, ADSB) {
    
    var lat = ADSB.Lat;
    var lng = ADSB.Lon;
    var HexCode = ADSB.HexCode;
    // Determine a random location from the bounds set previously  
    var IconGeoPos = new google.maps.LatLng(lat, lng);
    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);
    var DivLayer = $('#ADSB-' + HexCode);
    var CSS = {
      left: IconLocation.x,
      top: IconLocation.y,
      'background-image': 'url("' + this.getIconImage(ADSB.FlightID) + '")',
      'transform': 'rotate(' + ADSB.Heading + 'deg)'
    };
    if (DivLayer.length < 1) {
      DivLayer = $('<div class="ADSB-Icon" data-hexcode="' + HexCode + '" id="ADSB-' + HexCode + '"></div>').css(CSS);
      this.markerLayer.append(DivLayer);
    } else {
      DivLayer.clearQueue();
      DivLayer.css(CSS);
    }    
  };

  this.ResetIcon = function (now, elem) {
    var HexCode = elem.attr("data-hexcode");
    if (!(HexCode in this.ActiveAirlines)) {
      console.log("Removing Flight ID: " + HexCode);
      delete this.ADSBData[HexCode];
      $('#ADSB-' + HexCode).remove();
      if (this.activeClicked == HexCode) this.hideInfoLine();
    }
  };

  this.getIconImage = function (FlightID) {
    var Icon = '/images/Airline.png';
    if ((FlightID.substr(0, 3) === 'A00')) {
      Icon = '/images/Drone.png';
    }
    return Icon;
  }


  this.getData = function (HexCode) {
    return this.ADSBData[HexCode];
  }

}




ADSBOverlay.prototype.draw = function () {
  var projection = this.getProjection();
  if (!projection) return false;

  this.ActiveAirlines = {};

  //Adding Flight Icon to screen
  for (HexCode in this.ADSBData) {
    var ADSB = this.ADSBData[HexCode];
    this.ActiveAirlines[HexCode] = true;
    this.SetOrAdd(projection, ADSB);
  }
  /*
  console.log("ActiveAirlines: " + Object.keys(ActiveAirlines).length
    +
    ", this.ADSBData " + Object.keys(this.ADSBData).length );
  */

  //Removing any icons that are not in this list
  var icons = $('div.ADSB-Icon');
  var now = new Date();
  for (var i = 0; i < icons.length; i++) {
    var elem = $(icons[i]);
    this.ResetIcon(now, elem);
  }

};


ADSBOverlay.prototype.onAdd = function () {
  var $pane = $(this.getPanes().overlayImage); // Pane 3  
  $pane.append(this.markerLayer);
  $pane.append(this.infoLayer);
};

ADSBOverlay.prototype.onRemove = function () {
  this.markerLayer.remove();
  this.infoLayer.remove();
};

