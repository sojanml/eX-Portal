var map;
var _Location = [];
var livemarkers = [];

var initLat = 24.9899106;
var initLng = 55.0034188;
var defaultZoom = 10;


var bounds = new google.maps.LatLngBounds();
var infowindow = new google.maps.InfoWindow();



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

  GetDrones();
};


function GetDrones() {
  var _locVal = [];
  $.ajax({
    type: "GET",
    url: HomeMapURL,
    contentType: "application/json;charset=utf-8",
    dataType: "json",
    async: true,
    success: function (_Location) {
      setMarker(map, _Location)
    },
    failure: function (msg) {
      alert('Live Drone ' + msg);
    }
  });
}

function setMarker(map, _Location) {
  $.each(_Location, function (index, location) {
    var body = '' +
        '<b>' + location['DroneName'] + '</b><br>\n' +
        'Owner: ' + location['OwnerName'] + '<br>\n' +
        'Manufacture: ' + location['Manufacture'] + '<br>\n' +
        'UAV Type: ' + location['UAVType'];

    var myLatLng = new google.maps.LatLng(location['LastLatitude'], location['LastLongitude']);
    var marker = createMarker(map, myLatLng, location['DroneName'], body);

  });
  map.fitBounds(bounds);
}

function createMarker(map, latlng, heading, body) {
  var image = '/Blue.png';
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

  var ibLabel = new InfoBox(myOptions);
  ibLabel.open(map);

  marker.addListener('click', function () {
    var infowindow = new google.maps.InfoWindow({
      content: body
    });
    infowindow.open(map, marker);
  });

  bounds.extend(marker.position);
}

function ShowInfo(marker, i) {
  return function () {
    infowindow.open(map, marker);
  }
}