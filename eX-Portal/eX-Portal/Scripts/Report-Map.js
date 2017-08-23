var map;
window.onload = function () {
  initializeMap();
};


function ToPath(Coordinates) {
  var Path = [];
  if (Coordinates === '' || Coordinates === 'null') return Path;
  var aLatLng = Coordinates.split(',');
  for (var i = 0; i < aLatLng.length; i++) {
    var Points = aLatLng[i].split(' ');
    Path.push({ lat: parseFloat(Points[0]), lng: parseFloat(Points[1])});
  }
  return Path;
}

function initializeMap() {
  var MarkerPosition = new google.maps.LatLng(25.0955354, 55.1527025);

  var mapOptions = {
    zoom: 14,
    mapTypeControl: false,
    streetViewControl: false,
    center: MarkerPosition
  };

  map = new google.maps.Map(document.getElementById('GoogleMap'), mapOptions);
  map.setTilt(45);

  _FlightPath = new google.maps.Polyline({
    path: [],
    geodesic: true,
    strokeColor: '#12ffaf',
    strokeOpacity: 1.0,
    strokeWeight: 2,
    map: map
  });
  _FlightBoundBox = new google.maps.LatLngBounds();

  _FlightReplayPath = new google.maps.Polyline({
    path: [],
    geodesic: true,
    strokeColor: 'red',
    strokeOpacity: 1.0,
    strokeWeight: 2,
    map: map
  });

  var KmlUrl = 'http://test.exponent-ts.com/Map/NoFlyzone';
  var kmlOptions = {
    preserveViewport: true,
    map: map
  };
  NoFlyZone = new google.maps.KmlLayer(KmlUrl, kmlOptions);
}
