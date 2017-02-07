var map = null;
var LandLocation = { lat: 25.0955354, lng: 55.1527025 };
var LandLocationMarker = null;
var gFileCounter = 0;
var LocationMarkers = [];
var LocationBoundaryBox = null;
var LocationBoundBox = [];
var LocationPolygon = new google.maps.Polygon({
  strokeColor: '#00FF00',
  strokeOpacity: 0.4,
  strokeWeight: 2,
  fillColor: '#00FF00',
  fillOpacity: 0.2
});;

$(document).ready(function () {
  initializeMap();
  $('#frmImages').on("submit", function (e) {
    e.preventDefault();
    StartUpload();
  });
  $(document).on("click", 'span.delete', function () {
    DeleteImage($(this));
  })
  LoadMapLocation();
});

function initializeMap() {
  $('#Lat').val(LandLocation['lat'].toFixed(5));
  $('#Lng').val(LandLocation['lng'].toFixed(5));

  var mapOptions = {
    zoom: 14,
    mapTypeControl: false,
    streetViewControl: false,
    center: LandLocation,
    styles: getMapStyle()
  };

  map = new google.maps.Map(document.getElementById('AgricultureMap'), mapOptions);

}

function LoadMapLocation() {
  $.ajax({
    url: '/Agriculture/MapLocation/' + AgriTraxID + '?AgriTraxGroupID=' + AgriTraxGroupID,  //server script to process data
    type: 'GET',
    success: function (data) {
      completeHandler(data, 0)
    },
    cache: false
  }, 'json');
}

function DeleteImage(Span) {
  var URL = '/Agriculture/MapImage/' + AgriTraxID +
    '?ImageID=' + Span.attr('data-id') +
    '&Process=delete' +
    '&AgriTraxGroupID=' + AgriTraxGroupID;
  $.ajax({
    url: URL,  //server script to process data
    type: 'GET',
    success: function (data) {
      completeHandler(data, 0)
    }
  });
}


function StartUpload() {
  var ImageFiles = document.forms['frmImages']['AgriImage'];
  for (var i = 0; i < ImageFiles.files.length; i++) {
    var file = ImageFiles.files[i];
    UploadSingleFile(file);
  }
  document.forms['frmImages'].reset();
}

function UploadSingleFile(file) {
  name = file.name;
  size = file.size;
  type = file.type;

  if (file.name.length < 1) {
    //nothing to do
  } else if (file.size > 50 * 1000 * 1000) {
    alert("The file is too big");
  } else if (file.type !== 'image/png' && file.type !== 'image/jpg' && file.type !== 'image/gif' && file.type !== 'image/jpeg') {
    alert("The file does not match png, jpg or gif");
  } else {
    gFileCounter++;
    var ID = gFileCounter;

    var Elem = $('<li id="file_row_' + ID + '">Uploading ' + name + " ..." + 
      '<div id="file_upload_' + ID + '" style="float:right"><span id="file_uploaded_' + ID + '">0</span>% Uploaded' +
      '</LI>');
    $('UL#FileQueue').prepend(Elem);
    var formData = new FormData();
    formData.append('CreatedDate', file.lastModified);
    formData.append("file", file);

    $.ajax({
      url: '/Agriculture/UploadImage/' + AgriTraxID + '?AgriTraxGroupID=' + AgriTraxGroupID,  //server script to process data
      type: 'POST',
      xhr: function () { return getNewXhr(ID)},
      // Ajax events
      success: function (data) {
        $('#file_row_' + ID).remove();
        completeHandler(data)
      },
      error: errorHandler,
      // Form data
      data: formData,
      // Options to tell jQuery not to process data or worry about the content-type
      cache: false,
      contentType: false,
      processData: false
    }, 'json');

  }
}

function getNewXhr(ID) {  // custom xhr
  myXhr = $.ajaxSettings.xhr();
  if (myXhr.upload) { // if upload property exists
    myXhr.upload.addEventListener('progress', function (evt) {
      updateProgress(evt, ID);
    }, false); // progressbar
  }
  return myXhr;
}


function updateProgress(evt, ID) {
  if (evt.lengthComputable) {
    // evt.loaded and evt.total are ProgressEvent properties
    var loaded = (evt.loaded / evt.total) * 100;
    //console.log("ID: " + ID + " at " + loaded.toFixed(0) + "%");
    $('#file_uploaded_' + ID).html(loaded.toFixed(1));
  }
}

function completeHandler(data) {
  //console.log(data);
  var PolygonPath = [];

  for (var i = 0; i < LocationMarkers.length; i++) {
    LocationMarkers[i].setMap(null);
  }
  LandLocationMarker = null;
  $('#DataImageThumbnailGroup').empty();
  LocationMarkers = [];
  LocationBoundBox = new google.maps.LatLngBounds();
  LocationPolygon.setMap(null);

  for (var j = 0; j < data.Images.length; j++) {
    var Image = data.Images[j];
    var ImageURL = 'url("/Upload/Agriculture/' + Image.AgriTraxID + '/' + Image.Thumbnail + '")'
    var LI = $('<LI></LI>').css({ 'background-image': ImageURL });
    LI.append('<span data-id="' + Image.AgriTraxImageID + '" class="delete">x</span>')
    LI.append('<span class="label">' + (j + 1) + '</span>')
    LI.append('<span class="title">' + Image.Thumbnail.replace('.t.png','') + '</span>')
    $('#DataImageThumbnailGroup').append(LI);
    AddMarker(Image.Lat, Image.Lng, Image.AgriTraxImageID, true, j+1);
    LocationBoundBox.extend({ lat: Image.Lat, lng: Image.Lng });
    PolygonPath.push({ lat: Image.Lat, lng: Image.Lng });
  }

  LandLocationMarker = AddMarker(data.Location.Lat, data.Location.Lng, "0", false);
  LocationBoundBox.extend({ lat: data.Location.Lat, lng: data.Location.Lng });
  map.fitBounds(LocationBoundBox);
  LocationPolygon.setPath(PolygonPath);
  LocationPolygon.setMap(map);
}


function AddMarker(Lat, Lng, ID, isSpecial, Seq) {

  var position = { lat: Lat, lng: Lng };


  //Add a marker in the form
  var TheMarker = new google.maps.Marker({
    position: position,
    map: map,
    id: ID
  });

  if(isSpecial) {
    TheMarker.setIcon('http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=' + Seq + '|ffb400|000000');
    TheMarker.setDraggable(true);
  }

  google.maps.event.addListener(TheMarker, "dragend", function (event) {
    LocationMarkerMoved(ID, event);
  });

  LocationMarkers.push(TheMarker);

  return TheMarker;
}


function LocationMarkerMoved(ID, event) {

  var URL = '/Agriculture/MapImage/' + AgriTraxID +
    '?ImageID=' + ID +
    '&Process=location' +
    '&Lat=' + event.latLng.lat() +
    '&Lng=' + event.latLng.lng() +
    '&AgriTraxGroupID=' + AgriTraxGroupID;

  $.ajax({
    url: URL,  //server script to process data
    type: 'GET',
    success: function (data) {
      PolygonPath = [];
      for (var i = 0; i < LocationMarkers.length; i++) {
        var Marker = LocationMarkers[i];
        if (parseInt(Marker.id) !== 0) PolygonPath.push(Marker.getPosition());
      }
      LocationPolygon.setPath(PolygonPath);
      LandLocationMarker.setPosition({ lat: data.Location.Lat, lng: data.Location.Lng });
    }
  });
}

function errorHandler(data) {
  alert("Something went wrong!");
}