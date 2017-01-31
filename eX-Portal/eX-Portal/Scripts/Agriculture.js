var qViewDataTable;
var _MapPointsLayer = null;
var _InfoWindow = null;
var AgriTraxID = 0;

var LocationPolygon = new google.maps.Polygon({
  strokeColor: '#00FF00',
  strokeOpacity: 0.4,
  strokeWeight: 2,
  fillColor: '#00FF00',
  fillOpacity: 0.2
});

$(document).ready(function () {
  initializeMap();
  InitDataTable();

  $(document).on("click", "span.btnViewDetail", function (event) {
    var ID = parseInt($(this).attr('data-id'));
    if (isNaN(ID)) return;
    ShowInfoSection(ID);
    _InfoWindow.close();
  });
  $('#btnShowList').on("click", function () {
    ToggleInfo(false);
  });
  $('#DataImageThumbnail').on('click', 'li', function () {
    var LI = $(this);
    if (LI.hasClass('active')) return;
    $('#DataImageThumbnail LI').removeClass('active');
    var Image = LI.css('background-image');
    $('#DataImage').css({ 'background-image': Image });
    LI.addClass('active');
  });

  if (jQuery.ui) $('input.date-picker').datepicker({
    dateFormat: 'dd-M-yy',
    changeYear: true,
    changeMonth: true
  });

  $('#btnAddCustomer').on("click", function () {
    top.location.href = '/Agriculture/Create/'
  });

  $('#btnShowPoints').on("click", function () {
    LocationPolygon.setMap(null);
    _MapPointsLayer.show();
    $('#btnShowPoints').css({ 'visibility': 'hidden' });
    $('#btnSetImages').css({ 'visibility': 'hidden' });
  });

  $('#btnSetImages').on("click", function () {
    top.location.href = '/Agriculture/Images/' + AgriTraxID;
  });

});



function initializeMap() {
  var MarkerPosition = { lat: 25.0955354, lng: 55.1527025 };

  var mapOptions = {
    zoom: 14,
    mapTypeControl: false,
    streetViewControl: false,
    center: MarkerPosition,
    styles: getMapStyle()
  };

  map = new google.maps.Map(document.getElementById('adsb_map'), mapOptions);
  _MapPointsLayer = new MapPointsLayer({ map: map }, []);

  _InfoWindow = new google.maps.InfoWindow({
    content: '<div id="InfoWindowContent">...</div>'
  });
}


function InitDataTable() {
  qViewDataTable = $('#qViewTable').DataTable({
    "processing": true,
    "serverSide": true,
    sAjaxDataProp: "",
    "bPaginate": false,
    "bFilter": false,
    "paging":   false,
    "ordering": false,
    "info":     false,
    "searchDelay": 1000,
    "iDisplayLength": 100,
    "order": [[0, "asc"]],
    "ajax": getDataTableAjax,
    "scrollY": "244px",
    "scrollCollapse": true,
    "aoColumns": [
      { "mData": "CustomerReference", mRender: function (data, type, row, meta) {return meta.row + 1;}},
      { "mData": "CustomerReference" },
      { "mData": "DisburementDate", "mRender": function (data, type, full) { return dtConvFromJSON(data); } },
      { "mData": "PrincipalAmount", "mRender": function (data, type, full) { return nFormat(data/1000,0) + 'K'; } },
      { "mData": "LandAddress" },
      { "mData": "LandSize", "mRender": function (data, type, full) { return nFormat(data, 0); } }
    ]
  });


  $('#qViewTable tbody').on('click', 'tr', function () {
    ShowInfoWindow(qViewDataTable.row(this).index());
  });

  $('#btnSearch').on('click', function () {
    qViewDataTable.clear();
    qViewDataTable.draw();
    _InfoWindow.close();
    ToggleInfo(false);

  });
  

}


function ToggleInfo(IsDisplayDetails) {
  if (IsDisplayDetails) {
    if ($('div#qViewDataDetails').css('display') === 'block') return;
    $('div#qViewDataTable').slideUp();
    $('div#qViewDataDetails').slideDown();
    $('#btnShowList').show();
    $('#btnShowPoints').css({ 'visibility': 'visible' });
    $('#btnSetImages').css({ 'visibility': 'visible' });
  } else {
    if ($('div#qViewDataDetails').css('display') !== 'block') return;
    $('div#qViewDataTable').slideDown();
    $('div#qViewDataDetails').slideUp();
    $('#btnShowList').hide();
    LocationPolygon.setMap(null);
    _MapPointsLayer.show();
    $('#btnShowPoints').css({ 'visibility': 'hidden' });
    $('#btnSetImages').css({ 'visibility': 'hidden' });
  }
}

function ShowInfoSection(ID) {
  var Row = qViewDataTable.row(ID).data();
  var Keys = Object.keys(Row);
  for (var i = 0; i < Keys.length; i++) {
    var Key = Keys[i];
    var Val = Row[Key];
    switch (Key) {
      case 'Lat':
      case 'Lng':
        Val = parseFloat(Val).toFixed(4);
        break;
      case 'DisburementDate':
      case 'SiteVisitDate':
      case 'NextSiteVisitDate':
        Val = dtConvFromJSON(Val);
        break;
      case 'PrincipalAmount':
        Val = nFormat(Val,0);
        break;
    }
    $('#Field' + Key).html(Val);
  }

  AgriTraxID = Row['AgriTraxID'];
  $.ajax({
    url: '/Agriculture/MapLocation/' + Row['AgriTraxID'],  //server script to process data
    type: 'GET',
    success: ShowAgriTraxImages,
    cache: false
  }, 'json');

  ToggleInfo(true);
}

function ShowAgriTraxImages(data) {
  $('#DataImageThumbnail').empty();
  var PolygonPath = [];
  var LocationBoundBox = new google.maps.LatLngBounds();

  var Images = data.Images;
  for (var i = 0; i < Images.length; i++) {
    if (Images[i] !== '') {
      var IndexAt = Images[i].ImageFile.lastIndexOf('.');
      var ImageName = Images[i].ImageFile.substr(0, IndexAt) + ".m.png";
      var Image = 'url("/Upload/Agriculture/' + Images[i].AgriTraxID + '/' + ImageName + '")';
      var LI = $('<LI></LI>').css({ 'background-image': Image });

      if (i === 0) {
        LI.addClass('active');
        $('#DataImage').css({ 'background-image': Image });
      }
      $('#DataImageThumbnail').append(LI);

      var Path = { lat: Images[i].Lat, lng: Images[i].Lng };
      PolygonPath.push(Path);
      LocationBoundBox.extend(Path);
    }
  }

  //now show the box in map
  map.fitBounds(LocationBoundBox);
  LocationPolygon.setPath(PolygonPath);
  LocationPolygon.setMap(map);
  _MapPointsLayer.hide();
  $('#btnShowPoints').css({ 'visibility': 'visible' });
  $('#btnSetImages').css({ 'visibility': 'visible' });
}

function ShowInfoWindow(ID) {
  var Row = qViewDataTable.row(ID).data();
  var Position = { lat: Row.Lat, lng: Row.Lng };
  var Content =
    '<div class="row"><span class="value"><b>' + Row.CustomerReference + '</b></span></div>\n' +
    '<div class="row"><span class="value">' + Row.LandAddress + '</span></div>\n' +
    '<div class="row"><span class="label">Land Size: </span><span class="value">' + nFormat(Row.LandSize, 0) + ' M²</span></div>\n' +
    '<div class="row"><span class="label">Date: </span><span class="value">' + dtConvFromJSON(Row.DisburementDate) + '</span></div>\n' +
    '<div class="row"><span class="label">Branch: </span><span class="value">' + Row.BranchID + '</span></div>\n' +
    '<div class="row"><span class="label">Officer: </span><span class="value">' + Row.LoanOfficer + '</span></div>\n' +
    '<div style="text-align:right" class="row"><span data-id="' + ID + '" class="button btnViewDetail">View Details</span></div>\n';

  _InfoWindow.setPosition(Position);
  _InfoWindow.setContent(Content);
  _InfoWindow.open(map);
}

function getDataTableAjax(data, callback, settings) {
  var Query = $('#SearchForm').serialize();
  var URL = '/Agriculture/Data?' + Query;
  var AJAX = $.ajax({
    url: URL,
    type: 'GET',
    success: function (data) {
      callback(data);
      //Draw circles on map when drawing
      _MapPointsLayer.setData(data);
    }//succes
  });//$.ajax

}

function dtConvFromJSON(sNetDate) {
  var Months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
  if (sNetDate === null) return "N/A";
  var r = /\/Date\(([0-9]+)\)\//i
  var matches = sNetDate.match(r);
  if (matches.length === 2) {
    var tDate = new Date(parseInt(matches[1]));
    return (tDate.getDay() + 1) + '-' + Months[tDate.getMonth()] + '-' + tDate.getFullYear()
  } else {
    return "N/A";
  }
}

function nFormat(n, dp) {
  var w = n.toFixed(dp), k = w | 0, b = n < 0 ? 1 : 0,
      u = Math.abs(w - k), d = ('' + u.toFixed(dp)).substr(2, dp),
      s = '' + k, i = s.length, r = '';
  while ((i -= 3) > b) { r = ',' + s.substr(i, 3) + r; }
  return s.substr(0, i + 3) + r + (d ? '.' + d : '');
};





function MapPointsLayer(options, Data) {
  this.setValues(options);
  this.IsBoundsSet = false;
  this.markerLayer = $('<div />').addClass('overlay');
  this.MapData = Data;
  this.bounds = new google.maps.LatLngBounds();
  this.setData = function (Data) {
    this.MapData = Data;
    this.draw();
    if (Data.length > 0) {
      this.map.fitBounds(this.bounds);
    } else {
      this.map.setZoom(4);
    }
  };

  this.hide = function () {
    if (this.markerLayer) this.markerLayer.css({ 'visibility': 'hidden' });
  };

  this.show = function () {
    if (this.markerLayer) this.markerLayer.css({ 'visibility': 'visible' });
    this.map.fitBounds(this.bounds);
  };

};

MapPointsLayer.prototype = new google.maps.OverlayView;


MapPointsLayer.prototype.onAdd = function () {
  var $pane = $(this.getPanes().overlayImage); // Pane 3  
  $pane.append(this.markerLayer);
};

MapPointsLayer.prototype.onRemove = function () {
  this.markerLayer.remove();
};



MapPointsLayer.prototype.draw = function () {
  var projection = this.getProjection();
  if (!projection) return false;
  this.DrawCount++;
  this.markerLayer.empty();
  this.bounds = new google.maps.LatLngBounds();

  for (var i = 0; i < this.MapData.length; i++) {
    var lat = this.MapData[i].Lat;
    var lng = this.MapData[i].Lng;
    var title = this.MapData[i].LandAddress;
    var IconClass = 's';
    var DivID = 'row_' + i;

    // Determine a random location from the bounds set previously  
    var IconGeoPos = new google.maps.LatLng(lat, lng)
    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);
    var $NewPoint = $(
        '<div  class="map-point" id="' + DivID + '" title="' + title + '" '
      + 'data-id="' + i + '" '
      + 'style="left:' + IconLocation.x + 'px; top:' + IconLocation.y + 'px;">'
      + (i+1)
      + '</div>'
    );
    // Append the HTML to the fragment in memory  
    this.markerLayer.append($NewPoint.get(0));
    this.bounds.extend(new google.maps.LatLng(lat, lng));
  }//for (var i = 0)

  $('div.map-point').on('click', function (evt) {
    var ID = parseInt($(this).attr('data-id'));
    if (isNaN(ID)) return;
    ShowInfoWindow(ID);
  });


  if (this.IsBoundsSet) {
    //nothing
  } else {
    this.map.fitBounds(this.bounds);
    this.IsBoundsSet = true;
  }
};

