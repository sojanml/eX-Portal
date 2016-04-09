var map;
var _Location = [];
var livemarkers = [];

var ColorNormal = '#AA3939';
var ColorHilite = '#CC0000';
var ColorActive = '#00FF00';
var ColorActiveHilite = '#0000FF';

//var initLat = GridBoundBox["TopLeftLat"];
//var initLng = GridBoundBox["TopLeftLon"];
var defaultZoom = 10;
var bounds = new google.maps.LatLngBounds();
var infowindow = new google.maps.InfoWindow();
var ProductGrid = new Object();
var IsGridInitilized = false;
var RefreshTimer = null;

var BoundaryBox = null;
var BoundaryMarker = [null, null, null, null];

function _fnFooterCallback(nFoot, aData, iStart, iEnd, aiDisplay) {
  //alert('Start at: ' + iStart);
  deleteMarkers();
  setMarker(map, aData)
  //setGridHilite(aData);


}

function chkAutoRefresh_click(evt) {
  if (RefreshTimer) window.clearTimeout(RefreshTimer);
  RefreshTimer = null;
  refreshData(false);
}

function refreshData(IsForceRefresh) {
  if (IsForceRefresh || $('#chkAutoRefresh').is(':checked')) {
    RefreshTimer = window.setTimeout(function () {
      qViewDataTable.ajax.reload(null, false);
    }, 10 * 1000);
  }
}


function _fnDrawCallback() {
  $('#qViewTable_paginate').append('<span class="refresh">&#xf021;</span>');
  if ($('#qViewTable_filter').find('.refresh').length <= 0)
    $('#qViewTable_filter').append('<span class="refresh">&#xf021;</span>\n' +
    '<span><input id="chkAutoRefresh" checked type="checkbox">Auto Refresh</span>');

  //add a refresh counter
  refreshData(false);
  $('#chkAutoRefresh').on("click", chkAutoRefresh_click)
}

$(document).ready(function () {
  initialize();
  setGridBox();
  drawGridLines(GridLinesRows);
  drawGridLines(GridLinesCols);
  //drawLabels();
  //drawProducts();
  setCordinatesEditable();


  $('#chkNewSetting').on("click", function (e) {
    if ($(this).is(":checked")) {
      $('#YardName-list').slideUp();
      $('#YardName-text').slideDown();
    } else {
      $('#YardName-list').slideDown();
      $('#YardName-text').slideUp();
    }
  });

  $('#loadYardID').on("change", function (e) {
    var value = $(this).val();
    var URL = '/Map/getYard/' + value;
    $.ajax({
      url: URL,
      method: 'GET'
    }).done(function (data) {
      setYard(data);
    });
    //alert('Value=' + value);
  });

  $('#rfid-settings').on("click", rfid_settings);

  $('#rfid-virtual-grid').on("click", function (e) {
      e.preventDefault();
    $('#map-holder').hide();
    $('#grid-holder').show();
    if (!IsGridInitilized) {
      $('#grid-holder-content').html(grid_getByRows());
      IsGridInitilized = true;
    }
  });

  $('#rfid-google-map').on("click", function (e) {
    e.preventDefault();
    $('#map-holder').show();
    $('#grid-holder').hide();
  });

  $('#setYard').on("submit", setYard_post);
  $('#btnAutoSet').on("click", btnAutoSet_click);

  $(document).on("click", ".more-detail-info", function () {
    more_detail_info($(this));
  })

});

function more_detail_info(Obj) {
  var RFID = Obj.attr('data-rfid');
  Obj.html("Loading...");
  $.ajax({
    method: 'get',
    url: '/Map/RFID/' + RFID
  }).done(function (data) {
    Obj.html(data);
  });

}

function setYard(theYard) {
  var Form = document.forms['setYard'];
  Form['TopLeftLat'].value  = theYard.Bounds.TopLeft.Latitude;
  Form['TopLeftLon'].value  = theYard.Bounds.TopLeft.Longitude;
  Form['TopRightLat'].value = theYard.Bounds.TopRight.Latitude;
  Form['TopRightLon'].value = theYard.Bounds.TopRight.Longitude;

  Form['BottomLeftLat'].value  = theYard.Bounds.BottomLeft.Latitude;
  Form['BottomLeftLon'].value  = theYard.Bounds.BottomLeft.Longitude;
  Form['BottomRightLat'].value = theYard.Bounds.BottomRight.Latitude;
  Form['BottomRightLon'].value = theYard.Bounds.BottomRight.Longitude;

  Form['VechileOrientation_H'].checked = (theYard.VechileOrientation == 'H');
  Form['VechileOrientation_V'].checked = (theYard.VechileOrientation == 'V');

  Form['VechileWidth'].value = theYard.VechileWidth;
  Form['VechileLength'].value = theYard.VechileLength;

  Form['YardID'].value = theYard.YardID;


  var Cord = ['TopLeft', 'TopRight', 'BottomRight', 'BottomLeft'];
  for (var i = 0; i < Cord.length; i++) {
    var Lat = parseFloat(Form[Cord[i] + 'Lat'].value);
    var Lon = parseFloat(Form[Cord[i] + 'Lon'].value);
    if (isNaN(Lat) || isNaN(Lon)) {
      alert('invalid point at:' + i);
      return;
    }
    var Point = new google.maps.LatLng(Lat, Lon);
    BoundaryBox.getPath().setAt(i, Point);
    BoundaryMarker[i].setPosition(Point);
  }

}

function btnAutoSet_click(e) {
  //auto border set

}



function setGridHilite(aData) {
  for (var r = 0; r < Grid.length; r++) {
    for (var c = 0; c < Grid[r].length; c++) {
      var refID = r + "_" + c;
      if (ProductGrid[refID]) ProductGrid[refID].setOptions({ fillColor: ColorNormal })
    }
  }
  for (var d = 0; d < aData.length; d++) {
    var R = aData[d]['Row'] - 1; //convert to 0 index
    var C = aData[d]['Col'] - 1; //convert to 0 index
    var refID = R + "_" + C;
    if (ProductGrid[refID]) ProductGrid[refID].setOptions({ fillColor: ColorActive })
  }
  map.fitBounds(bounds);

}


function rfid_settings(e) {
  e.stopPropagation();
  e.preventDefault();
  if($('#rfid-settings-layer').css('display') == 'none') {
    $('#rfid-settings-layer').slideDown();
    BoundaryBox.setDraggable(true);
    for (var i = 0; i < BoundaryMarker.length; i++) {
      BoundaryMarker[i].setMap(map);
      BoundaryMarker[i].setDraggable(true);
    }
  } else {
    $('#rfid-settings-layer').slideUp();
    BoundaryBox.setDraggable(false);
    for (var i = 0; i < BoundaryMarker.length; i++) {
      BoundaryMarker[i].setMap(null);
      BoundaryMarker[i].setDraggable(false);
    }
  }
}

function setGridBox() {
  if (!GridBoundBox["TopLeftLat"]) return;
  var BoxCoords = [
    { lat: GridBoundBox["TopLeftLat"], lng: GridBoundBox["TopLeftLon"] },
    { lat: GridBoundBox["TopRightLat"], lng: GridBoundBox["TopRightLon"] },
    { lat: GridBoundBox["BottomRightLat"], lng: GridBoundBox["BottomRightLon"] },
    { lat: GridBoundBox["BottomLeftLat"], lng: GridBoundBox["BottomLeftLon"] }
  ];
  // Construct the polygon.
  BoundaryBox = new google.maps.Polygon({
    paths: BoxCoords,
    strokeColor: '#FF0000',
    strokeOpacity: 0.7,
    strokeWeight: 1,
    fillColor: '#FF0000',
    fillOpacity: 0.03
  });
  BoundaryBox.setMap(map);

  google.maps.event.addListener(BoundaryBox, "dragend", moveCordinatesEditable);
}

function getBoundary() {
  if (BoundaryBox == null) return '';
  var Bounds = BoundaryBox.getPath().getArray();
  var LatLng = '';
  for (var i = 0; i < Bounds.length; i++) {
    if (LatLng != '') LatLng += ',';
    LatLng = LatLng + Bounds[i].lat() + ' ' + Bounds[i].lng()
  }
  return LatLng;
}


function drawGridLines(GridLines) {

  for (var i = 0; i < GridLines.length; i++) {
    var Color = '#000000';
    var lineCoordinates = [
      new google.maps.LatLng(GridLines[i]["sLat"], GridLines[i]["sLon"]),
      new google.maps.LatLng(GridLines[i]["eLat"], GridLines[i]["eLon"])
    ];

    //Draw Rows and Columns
    var line = new google.maps.Polyline({
      path: lineCoordinates,
      geodesic: true,
      strokeColor: Color,
      strokeOpacity: 0.4,
      strokeWeight: 1
    });
    line.setMap(map);
  }
}


function drawLabels() {
  //Set Row and Column Label
  var bounds = new google.maps.LatLngBounds();

  var TL = new google.maps.LatLng(GridBoundBox["TopLeftLat"], GridBoundBox["TopLeftLon"]);
  var TR = new google.maps.LatLng(GridBoundBox["TopRightLat"], GridBoundBox["TopRightLon"]);
  var BR = new google.maps.LatLng(GridBoundBox["BottomRightLat"], GridBoundBox["BottomRightLon"]);
  var BL = new google.maps.LatLng(GridBoundBox["BottomLeftLat"], GridBoundBox["BottomLeftLon"]);

  bounds.extend(TL);
  bounds.extend(TR);
  bounds.extend(BR);
  bounds.extend(BL);

  var BoxCenter = bounds.getCenter();

  var RowCenter = MidPoint(TL, TR);
  var Angle = google.maps.geometry.spherical.computeHeading(BoxCenter, RowCenter);
  var NextP = getNextPoint(RowCenter, Angle, 0.002);
  setLabelGrid("Row", Angle, 0, 0, NextP);


  var RowCenter = MidPoint(BL, BR);
  var Angle = google.maps.geometry.spherical.computeHeading(BoxCenter, RowCenter);
  var NextP = getNextPoint(RowCenter, Angle, 0.002);
  setLabelGrid("Row", Angle, 0, 0, NextP);


  var RowCenter = MidPoint(TR, BR);
  var Angle = google.maps.geometry.spherical.computeHeading(BoxCenter, RowCenter);
  var NextP = getNextPoint(RowCenter, Angle, 0.002);
  setLabelGrid("Column", Angle, 0, 0, NextP);

  var RowCenter = MidPoint(TL, BL);
  var Angle = google.maps.geometry.spherical.computeHeading(BoxCenter, RowCenter);
  var NextP = getNextPoint(RowCenter, Angle, 0.002);
  setLabelGrid("Column", Angle, 0, 0, NextP);


  for (var r = 0; r < Grid.length; r++) {
    var LastCol = Grid[r].length - 1;
    var GPSFirst = Grid[r][0]['grid'];
    var GPSLast = Grid[r][LastCol]['grid'];

    var FirstPoint = MidPoint(
      new google.maps.LatLng(GPSFirst[0][0], GPSFirst[0][1]),
      new google.maps.LatLng(GPSFirst[3][0], GPSFirst[3][1])
    );
    var LastPoint = MidPoint(
      new google.maps.LatLng(GPSLast[0][0], GPSLast[0][1]),
      new google.maps.LatLng(GPSLast[3][0], GPSLast[3][1])
    );

    var Angle = google.maps.geometry.spherical.computeHeading(LastPoint, FirstPoint);
    var NextP = getNextPoint(FirstPoint, Angle, 0.001);
    setLabelGrid(r + 1, 0, 0, 0, NextP);
  }


  var LastRow = Grid.length - 1;
  for (var c = 0; c < Grid[0].length; c++) {
    var GPSFirst = Grid[0][c]['grid'];
    var GPSLast = Grid[LastRow][c]['grid'];

    var FirstPoint = MidPoint(
      new google.maps.LatLng(GPSFirst[1][0], GPSFirst[1][1]),
      new google.maps.LatLng(GPSFirst[2][0], GPSFirst[2][1])
    );
    var LastPoint = MidPoint(
      new google.maps.LatLng(GPSLast[1][0], GPSLast[1][1]),
      new google.maps.LatLng(GPSLast[2][0], GPSLast[2][1])
    );

    var Angle = google.maps.geometry.spherical.computeHeading(LastPoint, FirstPoint);
    var NextP = getNextPoint(FirstPoint, Angle, 0.002);
    setLabelGrid(c + 1, 0, 0, 0, NextP);
  }


}

function MidPoint(P1, P2) {
  return new google.maps.LatLng(
    (P1.lat() + P2.lat()) / 2,
    (P1.lng() + P2.lng()) / 2
  );
}


function toRad(num) {
  return num * Math.PI / 180;
}

function toDeg(num) {
  return num * 180 / Math.PI;
}

function getNextPoint(Point, Angle, Distance) {

  var dist = Distance / 6371;
  var brng = toRad(Angle);

  var lat1 = toRad(Point.lat()), lon1 = toRad(Point.lng());

  var lat2 = Math.asin(Math.sin(lat1) * Math.cos(dist) +
                        Math.cos(lat1) * Math.sin(dist) * Math.cos(brng));

  var lon2 = lon1 + Math.atan2(Math.sin(brng) * Math.sin(dist) *
                                Math.cos(lat1),
                                Math.cos(dist) - Math.sin(lat1) *
                                Math.sin(lat2));

  if (isNaN(lat2) || isNaN(lon2)) return null;

  return new google.maps.LatLng(toDeg(lat2), toDeg(lon2));

}

function setLabelGrid(Caption, Angle, OffsetX, OffsetY, Position) {

  var myOptions = getLabelOptions(Caption, Angle, OffsetX, OffsetY, Position);
  var ibLabel = new InfoBox(myOptions);
  ibLabel.open(map);
}

function getLabelOptions(Caption, Angle, OffsetX, OffsetY, Position) {
  var myOptions = {
    content: '<span>' + Caption + '</span>',
    boxStyle: {
      textAlign: "center",
      color: 'black',
      fontSize: "8pt",
      width: "auto",
      transform: "rotate(" + Angle + "deg)"
    },
    disableAutoPan: true,
    pixelOffset: new google.maps.Size(OffsetX, OffsetY),
    position: Position,
    closeBoxURL: "",
    isHidden: false,
    pane: "mapPane",
    enableEventPropagation: true
  };

  return myOptions;
}


function setLabelLine(Caption, GeoTag) {

  var Angle = google.maps.geometry.spherical.computeHeading(GeoTag[0], GeoTag[1]);
  var myOptions = getLabelOptions(Caption, Angle, 5, 0, GeoTag[0]);
  var ibLabel = new InfoBox(myOptions);
  ibLabel.open(map);

}

function drawProducts() {
  for (var r = 0; r < Grid.length; r++) {
    for (var c = 0; c < Grid[r].length; c++) {
      var Col = Grid[r][c];
      var GPS = Col['grid'];
      if (Col['items'] > 0) {

        var theBox = [
          { lat: GPS[0][0], lng: GPS[0][1] },
          { lat: GPS[1][0], lng: GPS[1][1] },
          { lat: GPS[3][0], lng: GPS[3][1] },
          { lat: GPS[2][0], lng: GPS[2][1] }
        ];
        // Construct the polygon.

        var refID = r + "_" + c;
        ProductGrid[refID] = new google.maps.Polygon({
          paths: theBox,
          strokeColor: ColorNormal,
          strokeOpacity: 0.8,
          strokeWeight: 0,
          fillColor: ColorNormal,
          fillOpacity: 0.5
        });
        ProductGrid[refID].setMap(map);
        ProductGrid[refID]['Row'] = r;
        ProductGrid[refID]['Col'] = c;
        google.maps.event.addListener(ProductGrid[refID], "mouseover", Tile_mouseover);

        google.maps.event.addListener(ProductGrid[refID], "mouseout", Tile_mouseout);
        google.maps.event.addListener(ProductGrid[refID], "click", function () {
          var Obj = this;
          Tile_Click(Obj);
        });

      }//if (Col['products'] > 0)
    }//for(var c = 0)
  }//for (var r = 0)

}


function Tile_mouseover() {
  if (this.fillColor != ColorActive) {
    this.setOptions({ fillColor: ColorHilite });
  } else {
    this.setOptions({ fillColor: ColorActiveHilite });
  }
}

function Tile_mouseout() {
  if (this.fillColor != ColorActiveHilite) {
    this.setOptions({ fillColor: ColorNormal });
  } else {
    this.setOptions({ fillColor: ColorActive });
  }
}

function Tile_Click(Obj) {
  var row = Obj['Row'] + 1;
  var col = Obj['Col'] + 1;
  $('#map-info').html('Row: ' + row + ', Column: ' + col + 
  ' <span id="map-info-rfid"><span class="icon progress">&#xf110;</span></span>');
  var nURL = RFID_Url + '&row=' + row + '&Column=' + col;
  $.ajax({
    url: nURL,
    success: function (data) {
      $('#map-info-rfid').html(" - " + data);
    }
  });
}


function initialize() {
  var Center = new google.maps.LatLng(0, 0);
  geocoder = new google.maps.Geocoder();
  var mapOptions = {
    zoom: defaultZoom,
    center: Center,
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
  map = new google.maps.Map(document.getElementById('map_canvas'), mapOptions);

  //bounds.extend(Center);

  //GetDrones();
};


function setMarker(map, _Location) {
  $.each(_Location, function (index, location) {
    var body = '' +
        '<b>' + location['RFID'] + '</b><br>\n' +
        'RSSI: ' + location['RSSI'] + '<br>\n' +
        location['Latitude'] + ", " + location['Longitude'] +
      '<div id="RFID-' + location['RFID'] + '" class="more-detail-info" data-rfid="' + location['RFID'] + '"><span class="link">Detail</div></div>';

    var myLatLng = new google.maps.LatLng(location['Latitude'], location['Longitude']);
    var marker = createMarker(map, myLatLng, location['DroneName'], body, location['RFID']);

  });
  map.fitBounds(bounds);
}

function createMarker(map, latlng, heading, body, RFID) {
  var image = '/images/car-icon.png';
  var marker = new google.maps.Marker({
    position: latlng,
    map: map,
    //icon: image,
    title: heading,
    zIndex: 999
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

function getRandomColor() {
  var letters = '0123456789ABCDEF'.split('');
  var color = '#';
  for (var i = 0; i < 6; i++) {
    color += letters[Math.floor(Math.random() * 16)];
  }
  return color;
}




function grid_getByRows() {
  var Row = TableDef['Rows'];
  var Col = TableDef['Cols'];
  var Table = '<table cellpadding="5" cellspacing="0">';
  for (var R = 0; R <= Row; R++) {
    Table += '<TR>\n';
    for (var C = 0; C <= Col; C++) {
      if (R == 0 && C == 0) {
        Table += "<th>&nbsp;</th>";
      } else if (R == 0) {
        Table += "<th>" + C + "</th>";
      } else if (C == 0) {
        Table += "<th>" + R + "</th>";
      } else {
        var Items = TableDef[R + "." + C];
        if (Items == "") Items = '&nbsp;';
        Table += "<td>" + Items + "</td>";
      }
    }//for(C)
    Table += '</TR>\n';
  }//For(R)
  Table += "</table>";

  return Table;
}

function grid_getByRows_Rev() {
  var Row = TableDef['Rows'];
  var Col = TableDef['Cols'];
  var Table = '<table border="1" cellpadding="5" cellspacing="0">';
  for (var R = Row + 1; R >= 1; R--) {
    Table += '<TR>\n';
    for (var C = Col + 1; C >= 1 ; C--) {
      if (R == (Row + 1) && C == (Col + 1)) {
        Table += "<th>&nbsp;</th>";
      } else if (R == (Row + 1)) {
        Table += "<th>" + (Col - C + 1) + "</th>";
      } else if (C == (Col + 1)) {
        Table += "<th>" + (Row - R + 1) + "</th>";
      } else {
        var Items = TableDef[R + "." + C];
        if (Items <= 0) Items = '&nbsp;';
        Table += "<td>" + Items + "</td>";
      }
    }//for(C)
    Table += '</TR>\n';
  }//For(R)
  Table += "</table>";

  return Table;
}

function grid_getByCols() {
  var Row = TableDef['Rows'];
  var Col = TableDef['Cols'];
  var Table = '<table border="1" cellpadding="5" cellspacing="0">';
  for (var C = 0; C <= Col; C++) {
    Table += '<TR>\n';
    for (var R = 0; R <= Row; R++) {
      if (R == 0 && C == 0) {
        Table += "<th>&nbsp;</th>";
      } else if (R == 0) {
        Table += "<th>" + C + "</th>";
      } else if (C == 0) {
        Table += "<th>" + R + "</th>";
      } else {
        var Items = TableDef[R + "." + C];
        if (Items <= 0) Items = '&nbsp;';
        Table += "<td>" + Items + "</td>";
      }
    }//for(C)
    Table += '</TR>\n';
  }//For(R)
  Table += "</table>";

  return Table;
}



function setCordinatesEditable() {
  if (BoundaryBox == null) return;
  var Icons = ['top-left.png', 'top-right.png', 'bottom-right.png', 'bottom-left.png']
  var Bounds = BoundaryBox.getPath().getArray();
  for (var i = 0; i < Bounds.length; i++) {
    BoundaryMarker[i] = new google.maps.Marker({
      position: Bounds[i],
      icon: '/images/map/' + Icons[i]
    });
    resetCordinateEdit(i, Bounds[i]);
    google.maps.event.addListener(BoundaryMarker[i], "drag", dragCordinatesEditable(i));
  }

}

function moveCordinatesEditable() {
  if (BoundaryBox == null) return;
  var Bounds = BoundaryBox.getPath().getArray();
  for (var i = 0; i < Bounds.length; i++) {
    BoundaryMarker[i].setPosition(Bounds[i]);
    resetCordinateEdit(i, Bounds[i]);
  }

}

function resetCordinateEdit(pos, Point) {
  var FORM = document.forms['setYard'];
  var Cord = ['TopLeft', 'TopRight', 'BottomRight', 'BottomLeft'];
  FORM[Cord[pos] + 'Lat'].value = Point.lat();
  FORM[Cord[pos] + 'Lon'].value = Point.lng();
}

function dragCordinatesEditable(i) {
  return function (event) {
    var Point = event.latLng;
    BoundaryBox.getPath().setAt(i, Point);
    resetCordinateEdit(i, Point);
  };
}

function setYard_post(e) {
  e.preventDefault();
  $('#btnSubmit').val("Wait...");
  var FORM = $(this);
  var Data = FORM.serialize();
  $.ajax({
    url: '/map/setYard',
    method: "POST",
    data: Data,
    success: setYard_post_done
  })
}

function setYard_post_done(data, textStatus, jqXHR) {
  $('#rfid-settings-layer').slideUp();
  $('#btnSubmit').val("Save");
  document.forms['setYard']['YardID'].value = data.YardID;
  document.forms['setYard']['chkNewSetting'].checked = false;
}

function btnAutoSet_click(e) {

}