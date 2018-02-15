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

  var LoadADSB = function (ADSBObj) {
    // var QueryData = $('input.spinner, input.query').serialize();
    $.ajax({
      type: "GET",
      url: '/FlightMap/ADSBData',
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
  this.ADSBData = {};
  this.ActiveAirlines = {};
  this.IsDrawing = 

  this.setADSB = function (Data) {
    for (var i = 0; i < Data.length; i++) {
      var ADSB = Data[i];
      this.ADSBData[ADSB.HexCode] = ADSB;
      this.ADSBData[ADSB.HexCode].UpdatedOn = new Date();
    }
    this.draw();
  };

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
    if (HexCode in this.ActiveAirlines) {
      var Elapsed = (now.getTime() - this.ActiveAirlines[HexCode].UpdatedOn.getTime()) / 1000;
      if (Elapsed > 10) {
        console.log(
          "Total Tracked Flights: " + Object.keys(this.ADSBData).length + 
          ", Checking Flight: " + HexCode +
          ", Updated On: " + this.ActiveAirlines[HexCode].UpdatedOn +
          ", Checking On: " + now +
          ", Elapsed: " + Elapsed);

        console.log("Removing Flight ID: " + HexCode);
        delete this.ADSBData[HexCode];
        $('#ADSB-' + HexCode).remove();
      }
    }
  };


  this.getIconImage = function (FlightID) {
    var Icon = '/images/Airline.png';
    if ((FlightID.substr(0, 3) === 'A00')) {
      Icon = '/images/Drone.png';
    }
    return Icon;
  }

}




ADSBOverlay.prototype.draw = function () {
  var projection = this.getProjection();
  if (!projection) return false;

  this.ActiveAirlines = {};

  //Adding Flight Icon to screen
  for (HexCode in this.ADSBData) {
    var ADSB = this.ADSBData[HexCode];
    this.ActiveAirlines[HexCode] = {
      UpdatedOn: ADSB.UpdatedOn
    };
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
};

ADSBOverlay.prototype.onRemove = function () {
  this.markerLayer.remove();
};

