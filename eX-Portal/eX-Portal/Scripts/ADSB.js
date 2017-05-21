
var AlertRef = {}
var BreachRef = {}
var _InfoWindow = new google.maps.InfoWindow({
  content: '<div id="InfoWindowContent">...</div>'
});

$(document).ready(function () {

  $(document).on("click", "div.adsb-point img", function () {
    var t = $(this).closest('div');
    ShowInfoWindow(t);
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
  //$("#slider").slider(({
  //    range: true,
  //    min: 0,
  //    max: 50000
  //}));

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


function getStatus(ADSBObj) {
  var QueryData =
    $('input.spinner, input.query').serialize() +
    '&IsQueryChanged=' + IsQueryChanged;
  IsQueryChanged = 0;

  //$("input.spinner").spinner("disable");

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
        //RPASList.push(elem.replace('A00', 'SC0'));
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
 
    //var AllFlights = ADSBObj.GetAllFlights();
  if ($('input.query#BreachLine').is(':checked'))
  {
   //   ADSBObj.DrawLinesOf(null);
      ADSBObj.DrawLinesOf(BreachRef);
      

  } 
  if ($('input.query#AlertLine').is(':checked'))
  {
   //   ADSBObj.DrawLinesOf(null);
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
    this.gADSBData = {};
    this.DrawCount = 0;
    this.ADSBFlightRef = {};
    this.WatchingFlightIDs = [];
    this.setADSB = function (ADSBData) {
        this.ADSBData = ADSBData;
        this.DrawCount++;
        this.GetWatchingFlight();
        this.draw();
    },
        this.ADSBLines = {};
    this.ADSBAlertLines = {};
    this.SavedLineReferece = null;
    this.SavedAlertLineReference = null;

    this.getIconColor = function (Height) {
        var Colors = [
          '#FF0000', //Red
          '#FF9600', //Yellow
          '#00ff10' //Green
        ];
        if (Height < 1000) {
            return Colors[0];
        } else if (Height < 2000) {
            return Colors[1];
        } else {
            return Colors[2];
        }
    }

    this.getIconClass = function (FlightID) {
        var IsDrone = (FlightID.substr(0, 3) == 'SC0');
        var ClassName = 'normal';
        //return IsDrone ? 'drone' : 'normal';

        if ((FlightID.substr(0, 3) === 'SC0')) {
            ClassName = 'drone';
            if (StatusInfo['Breach']['RPAS'] && FlightID in StatusInfo['Breach']['RPAS']) {
                ClassName = 'drone breach';
            } else if (StatusInfo['Alert']['RPAS'] && FlightID in StatusInfo['Alert']['RPAS']) {
                ClassName = 'drone alert';
            }
        }
        return ClassName;
    }

    this.GetAllFlights = function () {
        var FlightRef = {};
        for (var i = 0; i < this.ADSBData.length; i++) {
            var FlightID = this.ADSBData[i].FlightID.trim();
            FlightRef[FlightID] = this.ADSBData[i];
        }
        return FlightRef;

    };

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

    this.getIconFor = function (FlightID, Angle) {
        var IsDrone = (FlightID.substr(0, 3) == 'SC0');

        var ReturnHTML = '';
        var Icon = this.getIconImage(FlightID);
        if (IsDrone) {
            ReturnHTML = '<span class="icon DroneIcon" style=" transform: rotate(' + Angle + 'deg);"><img src="' + Icon + '" width="25" height="25"/></span>'
        } else {
            ReturnHTML = '<span class="icon FlightIcon" style=" transform: rotate(' + Angle + 'deg);"><img src="' + Icon + '" width="25" height="25"/></span>'
        }
        return ReturnHTML;
    }

    this.getIconImage = function (FlightID) {
        var Icon = '/images/Airline.png';
        if ((FlightID.substr(0, 3) === 'SC0')) {
            Icon = '/images/Drone.png';
            if (StatusInfo['Breach']['RPAS'] && FlightID in StatusInfo['Breach']['RPAS']) {
                Icon = '/images/Drone-breach.png';
            } else if (StatusInfo['Alert']['RPAS'] && FlightID in StatusInfo['Alert']['RPAS']) {
                Icon = '/images/Drone-alert.png';
            }
        }
        return Icon;
    }

    this.DrawLinesOf = function (LineReferece) {
        if (this.map == null || this.map == undefined) return;
        if (LineReferece == null) LineReferece = this.SavedLineReferece;
        if (LineReferece == null) return;

        var RefKeys = Object.keys(LineReferece);
        var AllFlights = this.GetAllFlights();
        var DrawingLineKeys = [];
        var ADSBLines = this.ADSBLines;
        RefKeys.forEach(function (RpasID, index, array) {
            var FromFlight = AllFlights[RpasID];
            if (FromFlight == undefined) {
            } else {
                var CenterPos = { lat: FromFlight.Lat, lng: FromFlight.Lon };
                LineReferece[RpasID].forEach(function (AircraftID, index, array) {
                    var toFlight = AllFlights[AircraftID];
                    if (toFlight == undefined) {
                        //nothing
                    } else {
                        var newPos = { lat: toFlight.Lat, lng: toFlight.Lon };
                        var LineKey = RpasID + "_" + AircraftID;
                        var LinePath = [CenterPos, newPos];
                        DrawingLineKeys.push(LineKey);
                        if (ADSBLines[LineKey] == undefined) {
                            var flightPath = new google.maps.Polyline({
                                path: LinePath,
                                geodesic: true,
                                strokeColor: '#fe0000',
                                strokeOpacity: 1,
                                strokeWeight: 1
                            });
                            flightPath.setMap(this.map);
                            ADSBLines[LineKey] = flightPath;
                        } else {
                            ADSBLines[LineKey].setPath(LinePath);
                        }//if (ADSBLines[RpasID + "_" + AircraftID] == undefined) {
                    }
                });//foreach
            }//if (FromFlight == undefined) 
        });//foreach

        //option to delete the path if not drawing
        var AllLines = Object.keys(ADSBLines);
        AllLines.forEach(function (LineKey, index, array) {
            if (DrawingLineKeys.indexOf(LineKey) < 0) {
                ADSBLines[LineKey].setMap(null);
                delete ADSBLines[LineKey];
            }
        })
    }; //this.DrawLinesOf
    this.DrawLinesOfAlert = function (LineReferece) {
        if (this.map == null || this.map == undefined) return;
        if (LineReferece == null) LineReferece = this.SavedAlertLineReference;
        if (LineReferece == null) return;

        var RefKeys = Object.keys(LineReferece);
        var AllFlights = this.GetAllFlights();
        var DrawingLineKeys = [];
        var ADSBAlertLines = this.ADSBAlertLines;
        RefKeys.forEach(function (RpasID, index, array) {
            var FromFlight = AllFlights[RpasID];
            if (FromFlight == undefined) {
            } else {
                var CenterPos = { lat: FromFlight.Lat, lng: FromFlight.Lon };
                LineReferece[RpasID].forEach(function (AircraftID, index, array) {
                    var toFlight = AllFlights[AircraftID];
                    if (toFlight == undefined) {
                        //nothing
                    } else {
                        var newPos = { lat: toFlight.Lat, lng: toFlight.Lon };
                        var LineKey = RpasID + "_" + AircraftID;
                        var LinePath = [CenterPos, newPos];
                        DrawingLineKeys.push(LineKey);
                        if (ADSBAlertLines[LineKey] == undefined) {
                            var flightPath = new google.maps.Polyline({
                                path: LinePath,
                                geodesic: true,
                                strokeColor: '#f2a003',
                                strokeOpacity: 1,
                                strokeWeight: 1
                            });
                            flightPath.setMap(this.map);
                            ADSBAlertLines[LineKey] = flightPath;
                        } else {
                            ADSBAlertLines[LineKey].setPath(LinePath);
                        }//if (ADSBLines[RpasID + "_" + AircraftID] == undefined) {
                    }
                });//foreach
            }//if (FromFlight == undefined) 
        });//foreach

        //option to delete the path if not drawing
        var AllLines = Object.keys(ADSBAlertLines);
        AllLines.forEach(function (LineKey, index, array) {
            if (DrawingLineKeys.indexOf(LineKey) < 0) {
                ADSBAlertLines[LineKey].setMap(null);
                delete ADSBAlertLines[LineKey];
            }
        })
    }; //this.DrawLinesOfAlert


    this.DeleteLineOf = function (FlightID) {
        //option to delete the path if not drawing
        var ADSBLines = this.ADSBLines;
        var AllLines = Object.keys(ADSBLines);
        AllLines.forEach(function (LineKey, index, array) {
            if (LineKey.indexOf(FlightID) < 0) {
                ADSBLines[LineKey].setMap(null);
                delete ADSBLines[LineKey];
            }
        });
    };
    this.DeleteLinesOf = function (Lines) {
        var ADSBLines = Lines;
        var AllLines = Object.keys(ADSBLines);
        AllLines.forEach(function (LineKey, index, array) {
           
                ADSBLines[LineKey].setMap(null);
                delete ADSBLines[LineKey];
          
        });
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
  this.DrawCount++;

  for (var i = 0; i < this.ADSBData.length; i++) {
    var lat = this.ADSBData[i].Lat;
    var lng = this.ADSBData[i].Lon;
    var alt = this.ADSBData[i].Altitude;
    var title = this.ADSBData[i].FlightID.trim();
    var heading = this.ADSBData[i].Heading;
    heading = parseFloat(heading);
    if (isNaN(heading)) heading = 0;

    // Determine a random location from the bounds set previously  
    var IconGeoPos = new google.maps.LatLng(lat, lng)
    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);
    var DivID = 'adsb-' + title;
    var IconClass = this.getIconClass(title);
    var $point = $('#' + DivID);
    var RotateAngle = 45;

    if (heading == 0) {
      //Landed flight - Ignore movement
    } else if (this.gADSBData[DivID]) {
      $point.animate({ left: IconLocation.x, top: IconLocation.y });
      $point.attr('class', 'adsb-point ' + IconClass);
      $point.find(".icon").css({ transform: 'rotate(' + (heading) + 'deg)' });
      $point.attr({ 'data-lat': lat, 'data-lng': lng, 'data-alt': alt });
      $point.find('img').attr('src', this.getIconImage(title));

    } else {
      var Icon = this.getIconFor(title, heading);
      var $NewPoint = $(
        '<div  class="adsb-point ' + IconClass + '" id="' + DivID + '" title="' + title + '" '
        + 'data-lat="' + lat + '" '
        + 'data-lng="' + lng + '" '
        + 'data-alt="' + alt + '" '
        + 'data-ident="' + title + '" '
        + 'style="left:' + IconLocation.x + 'px; top:' + IconLocation.y + 'px;">'
        + Icon
        + '<span class="flight-title" style="">' + title + '</span>' +
        + '</div>'
      );
      // Append the HTML to the fragment in memory  
      this.markerLayer.append($NewPoint.get(0));
      $point = $('#' + DivID);
     
    }

    var TailHTML = getTail(this.ADSBData[i]);
    $point.find('.tail').remove();
    $point.append(TailHTML);

    this.gADSBData[DivID] = this.DrawCount;




  }//for (var i = 0)

  //if the item does not exists, then remove it from the screen
  var AllKeys = Object.keys(this.gADSBData);
  for (i = 0; i < AllKeys.length; i++) {
    var TheKey = AllKeys[i];
    if (this.gADSBData[TheKey] != this.DrawCount) {
      var FlightID = TheKey.replace("adsb-", "");
      $('#' + TheKey).remove();
      $('div.' + DivID + '-history').remove();
      delete this.gADSBData[TheKey];
      delete this.ADSBFlightRef[FlightID];
      this.DeleteLineOf(FlightID);
    }//if (gADSBData[key] > 20) 
  }//for (var key in gADSBData)


  this.DrawLinesOf(null);
};



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


function ShowInfoWindow(t) {

    var title = t.attr('data-ident');
    var DroneFlightID = parseInt(title.substr(title.length-4)); //
    var IsDrone = (title.substr(0, 3) == 'SC0');
    var FlightLink = '\n';
    if (IsDrone)
    {
       
        FlightLink = '<a href="/FlightMap/Map/' + DroneFlightID + '">View Flight</a>\n';
    } else
    {
        FlightLink = '';
    }
  var alt = t.attr('data-alt');
  var lt = t.attr('data-lat').toString();
  var lg = t.attr('data-lng').toString();
    //   var Position = { lat: lt, lng: lg };

  var Content =
    '<div class="InfoWindow">' +
    '<div><b>' + t.attr('data-ident') + '</b></div>' +
    '<div >Latitude:' + lt + ' N</div>\n' +
    '<div >Longitude: ' + lg + ' E</div>\n' +
    '<div>Altitude: ' + alt + ' Feet</div>\n' +
   FlightLink+
    '</div>';
  var myLatlng = new google.maps.LatLng(lt, lg);
  _InfoWindow.setPosition(myLatlng);
  _InfoWindow.setContent(Content);
  _InfoWindow.open(map);

}