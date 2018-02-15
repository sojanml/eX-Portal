
var FlightMap = function () {
  var _map = null;
  var _DroneIcon = null;
  var _MapMarkers = null;
  var _ADSBOverlay = null;

  var _PolylineCompleted = null;
  var _PolylinePending = null;
  var _LatLngBounds = new google.maps.LatLngBounds();

  var _PolylinePendingPath = [];
  var _PolylineCompletedPath = [];
  var _FullPath = [];
  var _CurrentIndex = 0;
  var _FullData = [];

  var _IsMapBusy = false;

  var _Init = function (CenterPosition) {
    var mapOptions = {
      zoom: 10,
      mapTypeControl: true,
      streetViewControl: false,
      center: CenterPosition,
      styles: getADSBMapStyle()
    };
    _map = new google.maps.Map(document.getElementById('GoogleMap'), mapOptions);
    _DroneIcon = new DroneIcon({ map: _map }, CenterPosition);
    _MapMarkers = new MapMarkers({ map: _map }, CenterPosition);
    _ADSBOverlay = new ADSBOverlay({ map: _map });
    ADSBLoader.Init(_ADSBOverlay);

    _PolylineCompleted = new google.maps.Polyline({
      path: [],
      geodesic: true,
      strokeColor: '#18bdec',
      strokeOpacity: 1.0,
      strokeWeight: 2,
      map: _map
    });
    _PolylinePending = new google.maps.Polyline({
      path: [],
      geodesic: true,
      strokeColor: '#FF0000',
      strokeOpacity: 0.3,
      strokeWeight: 2,
      map: _map
    });


    _map.addListener('center_changed', _MapBusy );
    _map.addListener('idle', _MapIdle);

  };

  var _MapBusy = function() {
    _IsMapBusy = true;
  };
  var _MapIdle = function () {
    window.setTimeout(function () {
      _IsMapBusy = false;
    }, 200);
  };

  var _AddMapData = function (Data) {
    for (var i = 0; i < Data.length; i++) {
      var LatLng = { lat: Data[i].Lat, lng: Data[i].Lng };
      //_PolylinePendingPath.push(LatLng);
      _LatLngBounds.extend(LatLng);
      _FullPath.push(LatLng);
    }
    _FullData = _FullData.concat(Data);

    _PolylinePending.setPath(_FullPath);
    _MapMarkers.SetEndData(_FullData[_FullData.length - 1]);
    _MapMarkers.SetStartData(_FullData[0]);
  };

  var _MoveToIndex = function (Index) {
    if (Index >= _FullPath.length) return;
    _CurrentIndex = Index;
    var LatLng = _FullPath[Index];
    _PolylineCompleted.setPath(_FullPath.slice(0, Index + 1));
    _PolylinePending.setPath(_FullPath.slice(Index ));
    _DroneIcon.MoveTo(LatLng);

  };

  var _FitBounds = function () {
    _map.fitBounds(_LatLngBounds);
  };

  return {
    map: _map,
    Init: _Init,
    AddMapData: _AddMapData,
    AutoZoom: _FitBounds,
    MoveToIndex: _MoveToIndex
  };
}();

DroneIcon.prototype = new google.maps.OverlayView;
MapMarkers.prototype = new google.maps.OverlayView;

function MapMarkers(options, InitPosition) {
  this.setValues(options);
  this.StartData = null;
  this.EndData = null;

  this.StartGeoPos = new google.maps.LatLng(InitPosition.lat, InitPosition.lng);
  this.EndGeoPos = new google.maps.LatLng(InitPosition.lat, InitPosition.lng);

  this.StartLayer = $(
    '<div class="SpotMarker" id="StartMarker">' +
    '<span class="Marker Start"></span>' +
    '<div class="Marker-ToolTip">' +
    "</div>" +
    "</div>"
  );

  this.EndLayer = $(
    '<div class="SpotMarker" id="EndMarker">' +
    '<span class="Marker End"></span>' +
    '<div class="Marker-ToolTip">' +
    "</div>" +
    "</div>"
  );

  this.SetStartData = function (Data) {
    this.StartData = Data;
    this.StartGeoPos = new google.maps.LatLng(Data.Lat, Data.Lng);
    this.SetData(this.StartLayer.find('div.Marker-ToolTip'), Data);
    this.draw();
  };

  this.SetEndData = function (Data) {
    this.EndData = Data;
    this.EndGeoPos = new google.maps.LatLng(Data.Lat, Data.Lng);
    this.SetData(this.EndLayer.find('div.Marker-ToolTip'), Data);
    this.draw();
  };

  this.SetData = function (Layer, Data) {
    var Html =
      "<b>" + Util.FmtTime(Data.FlightDateTime) + "</b><br>" +
      'Lat: ' + Data.Lat.toFixed(6) + '<br>' +
      "Lng: " + Data.Lng.toFixed(6) + "<br>" +
      "Heading:  " + Data.Heading.toFixed(0) + "&deg;<br>" +
      "Altitude:  " + Data.Altitude.toFixed(0) + " Meter";
    Layer.html(Html);
  };



}


MapMarkers.prototype.onAdd = function () {
  var $pane = $(this.getPanes().overlayImage); // Pane 3  
  $pane.append(this.StartLayer);
  $pane.append(this.EndLayer);
};

MapMarkers.prototype.onRemove = function () {
  this.StartLayer.remove();
  this.EndLayer.remove();
};

MapMarkers.prototype.draw = function () {
  var projection = this.getProjection();
  if (!projection) return false;

  //this.StartLayer.clearQueue();
  //this.EndLayer.clearQueue();

  var IconLocation1 = projection.fromLatLngToDivPixel(this.StartGeoPos);
  this.StartLayer.css({ left: IconLocation1.x, top: IconLocation1.y });

  var IconLocation2 = projection.fromLatLngToDivPixel(this.EndGeoPos);
  this.EndLayer.css({ left: IconLocation2.x, top: IconLocation2.y });

};


function DroneIcon(options, DefaultPos) {
  this.lat = DefaultPos.lat;
  this.lng = DefaultPos.lng;
  this.markerLayer = $('<div id="FlightMapDroneIcon"></div>')
    .css({
      'background-image': 'url(\"' + FlightInfo.RPAS_Image + '\")',
      'z-Index': 99
    });
  this.setValues(options);


  this.MoveTo = function (LatLng) {
    this.lat = LatLng.lat;
    this.lng = LatLng.lng;
    this.draw();
  };

}

DroneIcon.prototype.onAdd = function () {
  var $pane = $(this.getPanes().overlayImage); // Pane 3  
  $pane.append(this.markerLayer);
};

DroneIcon.prototype.onRemove = function () {
  this.markerLayer.remove();
};

DroneIcon.prototype.draw = function () {
  var projection = this.getProjection();
  if (!projection) return false;

  var lat = this.lat;
  var lng = this.lng;
  // Determine a random location from the bounds set previously  
  var IconGeoPos = new google.maps.LatLng(lat, lng);
  //this.map.setCenter(IconGeoPos);
  var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);
  this.markerLayer.clearQueue();
  this.markerLayer.css(
    { left: IconLocation.x, top: IconLocation.y }
  );
};




