var map = null;
var AgriTraxID = 0;
var LandLocation = { lat: 25.0955354, lng: 55.1527025 };
var gFileCounter = 0;
var LocationPoly = [];
var LocationMarker = [];
var qViewDataTable = null;
var AgriTraxGroupID = 0;

var LocationColors = [
  null,
  'FFB300', 'CCFF00', '4DFF00', '00FF33',
  '00FFB3', '00CCFF', '004DFF', '3300FF', 'B300FF',
  'FF00CC', 'FF004D', 'FF3300', 'FFC53D', 'FFD77A',
  '3D77FF', '7AA2FF'
];
var _InfoWindow = null;
var isInitDataTable = false;

$(document).ready(function () {
  initializeMap();
  LoadMapLocation();

  $('#frmImages').on("submit", function (e) {
    e.preventDefault();
    StartUpload();
  });

  $(document).on("click", 'span.delete', function () {
    DeleteImage($(this));
  });

  $(document).on("click", 'span.btnAssign', function () {
    AssignPlot($(this));
  });

  $(document).on("click", 'span.btnAssignCustomer', fnAssignCustomer);
  $('#btnCancel').on("click", function () {
    $('#BulkUploadImages').show("slide", { direction: "left" }, 200);
    $('#BulkUploadSearch').hide("slide", { direction: "right" }, 200);
  });

  $(document).on("click", 'span.btnSelectRow', fnSelectCustomer);


  $('#btnSearch').on('click', function () {
    if (!isInitDataTable) InitDataTable();
    qViewDataTable.clear();
    qViewDataTable.draw();
    isInitDataTable = true;
  });

  _InfoWindow = new google.maps.InfoWindow({
    content: '<div id="InfoWindowContent">...</div>'
  });

});


function InitDataTable() {
  qViewDataTable = $('#qViewTable').DataTable({
    "processing": true,
    "serverSide": true,
    sAjaxDataProp: "",
    "bPaginate": false,
    "bFilter": false,
    "paging": false,
    "ordering": false,
    "info": false,
    "searchDelay": 1000,
    "iDisplayLength": 100,
    "order": [[0, "asc"]],
    "ajax": getDataTableAjax,
    "scrollY": "244px",
    "scrollCollapse": true,
    "deferLoading": 0, // here to load data later
    "aoColumns": [
      { "mData": "CustomerReference", mRender: function (data, type, row, meta) { return meta.row + 1; } },
      { "mData": "CustomerReference" },
      { "mData": "DisburementDate", "mRender": function (data, type, full) { return dtConvFromJSON(data); } },
      { "mData": "PrincipalAmount", "mRender": function (data, type, full) { return nFormat(data / 1000, 0) + 'K'; } },
      { "mData": "LandAddress" },
      { "mData": "LandSize", "mRender": function (data, type, full) { return nFormat(data, 0); } },
      { "mData": "AgriTraxID", "mRender": fnGetButtons }
    ]
  });
}

function fnGetButtons(data, type, full) {
  var Data =
  '<span class="btnAssignCustomer" data-id="' + data + '">Assign</span>' +
  '<span class="btnSelectRow" data-id="' + data + '">Select</span>';
  return Data;
}

function getDataTableAjax(data, callback, settings) {
  var Query = $('#SearchForm').serialize();
  var URL = '/Agriculture/Data?' + Query;
  var AJAX = $.ajax({
    url: URL,
    type: 'GET',
    success: function (data) {
      callback(data);
    }//succes
  });//$.ajax

}



function ShowInfoWindow(Polygon) {
  var vertices = Polygon.getPath();

  // Iterate over the vertices.
  var Center = { lat: 0, lng: 0 };
  var PointsCount = vertices.getLength();
  for (var i = 0; i < PointsCount; i++) {
    var xy = vertices.getAt(i);
    Center.lat += xy.lat();
    Center.lng += xy.lng();
  }
  Center.lat = Center.lat / PointsCount;
  Center.lng = Center.lng / PointsCount;

  var Content =
    '<div style="padding-bottom:30px; width:200px;">This plot has not been assigned to any customer. Please use the option continue...</div>' +
    '<div style="text-align:right" class="row"><span data-id="' + Polygon.id + '" class="button btnAssign">Assign</span></div>\n';

  _InfoWindow.setPosition(Center);
  _InfoWindow.setContent(Content);
  _InfoWindow.open(map);
}


function AssignPlot(Span) {
  var id = Span.attr('data-id');
  id = parseInt(id);
  if (isNaN(id)) return false;
  AgriTraxGroupID = id;

  $('#BulkUploadImages').hide("slide", { direction: "left" }, 200);
  $('#BulkUploadSearch').show("slide", { direction: "right" }, 200);

}

function fnAssignCustomer() {
  var Span = $(this);
  var AgriTraxID = Span.attr('data-id');
  OverlayStart();

  var URL = '/Agriculture/AssignCustomer?AgriTraxID=' + AgriTraxID + '&AgriTraxGroupID=' + AgriTraxGroupID;
  var AJAX = $.ajax({
    url: URL,
    type: 'GET',
    success: function (data) {
      LoadMapLocation();
    }//succes
  });//$.ajax
}

function fnSelectCustomer() {
  var Span = $(this);
  var TR = Span.closest("TR");
  $('table.report TR.selected').removeClass('selected');
  TR.addClass('selected');

}

function OverlayStart() {
  $('#Overlay')
    .css({ 'height': $(window).height() + 'px' })
    .fadeIn();
}

function initializeMap() {
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
    url: '/Agriculture/MapBulkUpload/',  //server script to process data
    type: 'GET',
    success: function (data) {
      completeHandler(data);
      $('#Overlay').fadeOut();
      $('#btnCancel').trigger("click");
      _InfoWindow.close();
    },
    cache: false
  }, 'json');
}

function DeleteImage(Span) {
  var URL = '/Agriculture/MapImage/' + AgriTraxID +
    '?ImageID=' + Span.attr('data-id') +
    '&Process=delete';
  $.ajax({
    url: URL,  //server script to process data
    type: 'GET',
    success: function (data) {
      completeHandler(data, 0);
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
    //NOTHING
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
      url: '/Agriculture/UploadImage/' + AgriTraxID,  //server script to process data
      type: 'POST',
      xhr: function () { return getNewXhr(ID); },
      // Ajax events
      success: function (data) {
        $('#file_row_' + ID).remove();
        completeHandler(data);
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
    var loaded = evt.loaded / evt.total * 100;
    //console.log("ID: " + ID + " at " + loaded.toFixed(0) + "%");
    $('#file_uploaded_' + ID).html(loaded.toFixed(1));
  }
}

function completeHandler(data) {
  $('#BulkImages').empty();
  var PolygonPaths = {};

  for (var i = 0; i < data.Images.length; i++) {
    //Generate thumbnail Image to load
    var Image = data.Images[i];
    var ImageURL = 'url("/Upload/Agriculture/' + Image.AgriTraxID + '/' + Image.Thumbnail + '")'
    var LI = $('<LI></LI>').css({ 'background-image': ImageURL });
    LI.append('<span class="label">' + (i + 1) + '</span>')
    LI.append('<span class="title">' + getFileTitle(Image.Thumbnail) + '</span>')
    LI.append('<span data-id="' + Image.AgriTraxImageID + '" class="delete">x</span>')

    var ImageGroupID = '#AgriTraxGroup' + Image.AgriTraxGroupID;
    var ImageGroup = $('#BulkImages').find(ImageGroupID);
    if (ImageGroup.length < 1) {
      ImageGroup = $('<ul id="AgriTraxGroup' + Image.AgriTraxGroupID + '"></ul>');
      var GroupSection = $('<LI></LI>');
      var GroupTitle = $('<div class="group-title">Images of ' + dtConvFromJSON(Image.ImageDateTime, true) + '</div>');
      GroupTitle.css('background-color', '#' + LocationColors[Image.AgriTraxGroupID]);
      GroupSection.append(GroupTitle);
      GroupSection.append(ImageGroup);
      $('#BulkImages').append(GroupSection);

      PolygonPaths[Image.AgriTraxGroupID] = [];
    }
    PolygonPaths[Image.AgriTraxGroupID].push({ lat: Image.Lat, lng: Image.Lng });
    ImageGroup.append(LI);
  }
  MakePolygon(PolygonPaths);
}



function MakePolygon(PolygonPaths) {
  var LocationBoundBox = new google.maps.LatLngBounds();
  var Seq = 0;

  //Clear all polygon
  for (var X = 0; X < LocationPoly.length; X++) {
    LocationPoly[X].setMap(null);
  }
  for (var i = 0; i < LocationMarker.length; i++) {
    LocationMarker[i].setMap(null);
  }
  LocationPoly = [];
  LocationMarker = [];

  for (AgriTraxGroupID in PolygonPaths) {
    var Path = PolygonPaths[AgriTraxGroupID];
    var Color = LocationColors[AgriTraxGroupID];
    var LocationPolygon = new google.maps.Polygon({
      strokeColor: '#' + Color,
      strokeOpacity: 0.4,
      strokeWeight: 2,
      fillColor: '#' + Color,
      fillOpacity: 0.2,
      path: Path,
      map: map,
      id: AgriTraxGroupID
    });
    for (var J = 0; J < Path.length; J++) {
      Seq++;
      var position = Path[J];
      //Add a marker in the form
      var TheMarker = new google.maps.Marker({
        position: position,
        map: map,
        icon: 'http://chart.apis.google.com/chart?chst=d_map_pin_letter&chld=' + Seq + '|' + Color + '|000000'
      });
      LocationMarker.push(TheMarker);
      LocationBoundBox.extend(position);
    }

    google.maps.event.addListener(LocationPolygon, 'click', function (event) {
      ShowInfoWindow(this);
    });
    LocationPoly.push(LocationPolygon);

  }

  map.fitBounds(LocationBoundBox);
}


function errorHandler(data) {
  alert("Something went wrong!");
}