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
  var mapOptions = {
    zoom: 14,
    mapTypeControl: false,
    streetViewControl: false
  };
  map = new google.maps.Map(document.getElementById('GoogleMap'), mapOptions);
  
  var KmlUrl = 'http://dcaa.exponent-ts.com/Map/NoFlyzone?001';
  var kmlOptions = {
    preserveViewport: true,
    map: map
  };
  NoFlyZone = new google.maps.KmlLayer(KmlUrl, kmlOptions);

  // Construct the polygon.
  var ApprovalInnerCoordinates = ToPath(ApprovalInnerPath);
  var TempOuterPath = ToPath(ApprovalOuterPath);
  var ApprovalOuterCoordinates = ApprovalInnerCoordinates.concat(ApprovalInnerCoordinates[0]);
  ApprovalOuterCoordinates = ApprovalOuterCoordinates.concat(TempOuterPath);
  
  _ApprovalInnerPath = new google.maps.Polygon({
    paths: ApprovalInnerCoordinates,
    strokeColor: '#FF0000',
    strokeOpacity: 0.8,
    strokeWeight: 0,
    fillColor: '#00FF00',
    fillOpacity: 0.35,
    map: map,
    zIndex: 200
  });

  _ApprovalOuterPath = new google.maps.Polygon({
    paths: ApprovalOuterCoordinates,
    strokeColor: '#FF0000',
    strokeOpacity: 0.8,
    strokeWeight: 0,
    fillColor: '#FF0000',
    fillOpacity: 0.35,
    map: map,
    zIndex: 210
  });

  _FlightReplayPath = new google.maps.Polyline({
    path: PolyPath,
    geodesic: true,
    strokeColor: 'red',
    strokeOpacity: 1.0,
    strokeWeight: 2,
    map: map,
    zIndex: 220
  });




  var bounds = new google.maps.LatLngBounds();
  for (var i = 0; i < PolyPath.length; i++) {
    bounds.extend(PolyPath[i]);
  }
  for (var i = 0; i < ApprovalOuterCoordinates.length; i++) {
    bounds.extend(ApprovalOuterCoordinates[i]);
  }
  map.fitBounds(bounds);
  
}
