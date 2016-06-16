var DroneName;
var map;
var initLat = 24.9899106;
var initLng = 55.0034188;
var defaultZoom = 10;
// map center

var bounds = new google.maps.LatLngBounds();
var infowindow = new google.maps.InfoWindow();
var center = new google.maps.LatLng(initLat, initLng);

$(document).ready(function () {
    initialize();
});
// marker position
//var factory = new google.maps.LatLng(25.9899106, 55.0034188;);

function initialize() {
    var mapOptions = {
        center: center,
        zoom: defaultZoom,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };

     map = new google.maps.Map(document.getElementById("map-canvas"), mapOptions);
    GetGeoTagInfo();   
}


function GetGeoTagInfo() {
  setMarker(map, GpsTags);
}


function setMarker(map, _GeoInfo) {   
    $.each(_GeoInfo, function (index, GeoInfo) {       
        var body = '<b>' + "" + '</b><br>\n' +
            ' <img src="' + GeoInfo['Thumbnail'] + '"  height="100px" width="100px" />';
        var myLatLng = new google.maps.LatLng(GeoInfo['Latitude'], GeoInfo['Longitude']);
        var marker = createMarker(map, myLatLng, "", body, "");
    });
    map.fitBounds(bounds);
}


function createMarker(map, latlng, heading, body, live) {
  
    var marker = new google.maps.Marker({
        map: map, position: latlng
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