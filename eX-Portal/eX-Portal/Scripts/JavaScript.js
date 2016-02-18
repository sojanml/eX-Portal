var map;
var _Location = [];
var livemarkers = [];

var initLat = 24.9899106;
var initLng = 55.0034188;
var defaultZoom = 10;

var bounds = new google.maps.LatLngBounds();
var infowindow = new google.maps.InfoWindow();

function _fnFooterCallback(nFoot, aData, iStart, iEnd, aiDisplay) {
  //alert('Start at: ' + iStart);
  deleteMarkers();
  setMarker(map, aData)
}

$(document).ready(function () {
  initialize();
});

function initialize() {
  geocoder = new google.maps.Geocoder();
  var mapOptions = {
    zoom: defaultZoom,
    center: new google.maps.LatLng(initLat, initLng),
    panControl: false,
    mapTypeControl: false,
    mapTypeControlOptions: {
      position: google.maps.ControlPosition.RIGHT_TOP,
    },
    zoomControl: true,
    zoomControlOptions: {
      style: google.maps.ZoomControlStyle.LARGE,
      position: google.maps.ControlPosition.LEFT_TOP,
    },
    scaleControl: false,
    streetViewControl: true,
    overviewMapControl: false,
    mapTypeId: google.maps.MapTypeId.ROADMAP
  };
  map = new google.maps.Map(document.getElementById('map_canvas'),
      mapOptions);

  //GetDrones();
};



function setMarker(map, _Location) {
  $.each(_Location, function (index, location) {
    var body = '' +
        '<b>' + location['RFID'] + '</b><br>\n' +
        'RSSI: ' + location['RSSI'] + '<br>\n' +
        location['Latitude'] + ", " + location['Longitude'];

    var myLatLng = new google.maps.LatLng(location['Latitude'], location['Longitude']);
    var marker = createMarker(map, myLatLng, location['DroneName'], body);

  });
  map.fitBounds(bounds);
}

function createMarker(map, latlng, heading, body) {
  var image = '/images/car-icon.png';
  var marker = new google.maps.Marker({
    position: latlng,
    map: map,
    icon: image,
    title: heading
  });

  var myOptions = {
    content: heading,
    boxStyle: {
      textAlign: "center",
      color: 'red',
      fontSize: "8pt",
      width: "auto"
    },
    disableAutoPan: true,
    pixelOffset: new google.maps.Size(-25, 0),
    position: latlng,
    closeBoxURL: "",
    isHidden: false,
    pane: "mapPane",
    enableEventPropagation: true
  };

  marker.addListener('click', function () {
    var infowindow = new google.maps.InfoWindow({
      content: body
    });
    infowindow.open(map, marker);
  });

  bounds.extend(marker.position);

  livemarkers.push(marker);
  return marker;
}

function ShowInfo(marker, i) {
  return function () {
    infowindow.open(map, marker);
  }
}

// Sets the map on all markers in the array.
function setMapOnAll(map) {
  for (var i = 0; i < livemarkers.length; i++) {
    livemarkers[i].setMap(map);
  }
}


// Removes the markers from the map, but keeps them in the array.
function clearMarkers() {
  setMapOnAll(null);
}

// Shows any markers currently in the array.
function showMarkers() {
  setMapOnAll(map);
}

// Deletes all markers in the array by removing references to them.
function deleteMarkers() {
  clearMarkers();
  livemarkers = [];
}
