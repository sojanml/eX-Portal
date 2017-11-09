var DynamicZone = function () {
  var _map = {};
  var _ZoneOnScreen = {};
  var _ZoneLoadTimer = null;

  var _initilize = function (map) {
    _map = map;
    _ZoneLoadTimer = window.setTimeout(_LoadDynamicZone, 500);
  };
  
  var _LoadDynamicZone = function () {
    $.ajax({
      type: "GET",
      url: '/Map/DynamicZone/',
      contentType: "application/json;charset=utf-8",
      dataType: "json",
      success: _ShowDynamicZone,
      failure: function (msg) {
        alert('Live Drone Position Error' + msg);
      },
      complete: function (msg) {
      }
    });
  };

  var _ShowDynamicZone = function (ZoneData) {
    var LoadedZone = Object.keys(_ZoneOnScreen);
    var MarkDrawingZone = {};

    //Draw Zone for the data received
    for (var i = 0; i < ZoneData.length; i++) {
      var ID = ZoneData[i].ID + '';
      var Path = ZoneData[i].Path;
      if (LoadedZone.indexOf(ID) >= 0) {
        _ZoneOnScreen[ID].setPath(Path);
      } else {
        _ZoneOnScreen[ID] = _createPolygon(Path);
      }
      MarkDrawingZone[ID] = true;
    }

    //Delete zone that already on screen which is expired
    for (var i = 0; i < LoadedZone.length; i++) {
      var ID = LoadedZone[i];
      if (!MarkDrawingZone[ID]) {
        _ZoneOnScreen[ID].setMap(null);
        delete _ZoneOnScreen[ID]
      }
    }

    //Set timer for next loading
    window.clearTimeout(_ZoneLoadTimer);
    _ZoneLoadTimer = window.setTimeout(_LoadDynamicZone, 5000);

  };

  var _createPolygon = function (Path) {
    // Construct the polygon.
    var Polygon = new google.maps.Polygon({
      paths: Path,
      strokeWeight: 0,
      fillColor: 'Orange',
      fillOpacity: 0.5,
      strokeColor: 'Orange',
      strokeOpacity: 0.8,
      strokeWeight: 2,
      map: map,

    });
    return Polygon;
  };


  return {
    Initilize: _initilize
  };

}();


var DynamicZoneNotify = function () {
  var _container = {};
  var _messageTimer = null;
  var _flightID = 0;
  var _messagesList = $('<ul class="_ZoneNotifications"></ul>');
  var _activeNotifications = {};
  var _loadCounter = 0;

  var _initilize = function (MessageContainer, FlightID) {
    if (!(MessageContainer instanceof jQuery)) return false;
    _flightID = FlightID;
    _container = MessageContainer;
    _container.append(_messagesList);
    _messageTimer = window.setTimeout(_loadZoneNotifications, 5000);
  };

  var _loadZoneNotifications = function () {
    $.ajax({
      type: "GET",
      url: '/Map/DynamicZoneNotification/' + _flightID,
      contentType: "application/json;charset=utf-8",
      dataType: "json",
      success: _processZoneNotifications,
      failure: function (msg) {
        alert('Live Drone Position Error' + msg);
      },
      complete: function (msg) {
      }
    });
  };

  var _processZoneNotifications = function (data) {
    var OldClassName = 'display-of-' + _loadCounter;
    _loadCounter++;
    var NewClassName = 'display-of-' + _loadCounter;
    for (var i = 0; i < data.length; i++) {
      var ID = '_ZoneNotifications_' + data[i].ID;
      var LI = $('#' + ID);
      if (LI.length <= 0) {
        LI = $('<li id="' + ID + '">' + data[i].Description +  '</li>');
        _messagesList.append(LI);
      }
      LI.attr({ 'class': NewClassName });
    }
    $('LI.' + OldClassName).remove();
    if (_messagesList.children().length <= 0) {
      _container.hide();
    } else {
      _container.show();
    }

    //Load the next messages
    _messageTimer = window.setTimeout(_loadZoneNotifications, 5000);
  }


  return {
    Initilize: _initilize
  };

}();