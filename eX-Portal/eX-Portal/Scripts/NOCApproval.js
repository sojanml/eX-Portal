﻿var DroneApprovals = [];
var map = null;
var Form = document.forms['frmDroneSetup'];
var showMsgTimer = null;
var OuterBorder = null;
var OuterPoly = null;
var LoadPolygonsTimer = null;

function showMsg(Message) {
  if (showMsgTimer) {
    window.clearTimeout(showMsgTimer);
  }
  $('#error-message').html(Message).show();

  showMsgTimer = window.setTimeout(function () {
    $('#error-message').slideUp();
  }, 10000);

}

$(document).ready(function () {



  initialize();

  var KmlUrl = 'http://test.exponent-ts.com/Map/NoFlyzone?R=' + Math.random();
  var kmlOptions = {
    preserveViewport: true,
    map: map
  };
  NoFlyZone = new google.maps.KmlLayer(KmlUrl, kmlOptions);
  NoFlyZone.setValues({ map: map });

  $("#submitButton").on("click", function (e) {
    e.preventDefault();
    $("body").scrollTop(0);
    var data = $('#frmDroneSetup').serialize();

    showMsg("Updating....Please Wait.");

    $("#frmDroneSetup :input").prop("disabled", true);

    $.ajax({
      type: 'POST',
      url: '/Rpas/NOCApproval',
      data: data,
      success: function (data) {
        var Rows = data.split("|");
        $("#frmDroneSetup :input").prop("disabled", false);
        if (Rows[0] == 'OK') {
          showMsg("Data has been updated. ");
          top.location.href = Rows[1];
        } else {
          showMsg(data);
        }
      },
      error: function () {

      }
    });
  });


  $('#GcaApproval_Coordinates').on("change", function (e) {
    e.preventDefault();
    updateCordinates();

    LoadPolygons();
  })



});//$(document).ready()

function setDefaultApprovals() {
  var eDate = new Date();
  eDate.setDate(eDate.getDate() + 90);
  var FORM = document.forms['frmDroneSetup'];
  FORM['GcaApproval.Coordinates'].value = getDefaultCoordinates();
  FORM['GcaApproval.EndDate'].value = fmtDt(eDate);
  FORM['GcaApproval.EndTime'].value = '18:00';
  FORM['GcaApproval.MaxAltitude'].value = 40;
  FORM['GcaApproval.MinAltitude'].value = 0;
  FORM['GcaApproval.StartDate'].value = fmtDt(new Date());
  FORM['GcaApproval.StartTime'].value = '08:00';
}

function toDate(value) {
  var pattern = /Date\(([^)]+)\)/;
  var results = pattern.exec(value);
  var dt = new Date(parseFloat(results[1]));
  return fmtDt(dt);
}

function fmtDt(dt) {
  var Months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  return dt.getDate() + "-" + Months[dt.getMonth()] + "-" + dt.getFullYear();
}

function DroneID_Success(Row) {
  $("#frmDroneSetup :input").prop("disabled", false);
  var FORM = document.forms['frmDroneSetup'];


  // clear before appending new list
  $("#GcaApproval_ApprovalID").html("");
  $("#GcaApproval_ApprovalID").append(
    $('<option></option>').val(0).html("--Select Regulator Approval--"));

  updateCordinates();
  setDefaultApprovals();

  if (Row.hasRows) {
    FORM['GcaApproval.PilotUserId'].value = Row.PilotUserId;
    FORM['GcaApproval.GroundStaffUserId'].value = Row.GroundStaffUserId;
    FORM['GcaApproval.NotificationEmails'].value = Row.NotificationEmails;
    FORM['GcaApproval.ApprovalName'].value = '';

    DroneApprovals = Row.Approvals;

    if (DroneApprovals.length > 0) {
      $.each(DroneApprovals, function (i, approvl) {
        $("#GcaApproval_ApprovalID").append(
          $('<option></option>').val(approvl.ApprovalID).html(approvl.ApprovalName)
        )
      }); //each
    }

  } else {//if(Row.hasRows)
    DroneApprovals = [];
    FORM['GcaApproval.PilotUserId'].value = '';
    FORM['GcaApproval.GroundStaffUserId'].value = '';
    FORM['GcaApproval.NotificationEmails'].value = '';
    FORM['GcaApproval.ApprovalName'].value = '';
  }

}//DroneID_Success

function cordinates_Success(olistCoordinates) {
  console.log(olistCoordinates);
  if (olistCoordinates.length > 0) {
    document.forms['frmDroneSetup']['GcaApproval.Coordinates'].value = olistCoordinates[0].Cordinates;
    //$("#gcacoordinates").css("visibility","visible");
    //$("#newgcaapproval").css("visibility","visible");
    //$("#polygonmap").css("visibility", "visible");
    updateCordinates();
  } else {//if(Row.hasRows)
    document.forms['frmDroneSetup']['GcaApproval.Coordinates'].value = '';
  }
}//cordinates_Success

function initialize() {
  var myLatLng = { lat: 25, lng: 55 };
  var mapDiv = document.getElementById('map_canvas');
  map = new google.maps.Map(mapDiv, {
    center: myLatLng,
    zoom: 10,
    styles: getADSBMapStyle()
  });

  var BoxCord = setBoundary(getCoordinates());

  // Construct the polygon.
  BoundaryBox = new google.maps.Polygon({
    paths: BoxCord,
    strokeColor: '#00FF00',
    strokeOpacity: 0.7,
    strokeWeight: 1,
    fillColor: '#00FF00',
    fillOpacity: 0.1,
    editable: true,
    draggable: true
  });

  BoundaryBox.setMap(map);
  OuterBorder = new google.maps.Polyline({
    path: [],
    strokeColor: 'red',
    strokeOpacity: 1,
    strokeWeight: 4,
    map: map
  });
  OuterPoly = new google.maps.Polygon({
    paths: [],
    strokeWeight: 0,
    fillColor: '#f2a003',
    fillOpacity: 0.4,
    map: map
  });
  updateCordinates();
  LoadPolygons();
  google.maps.event.addListener(BoundaryBox.getPath(), 'set_at', setCoordinates);
  google.maps.event.addListener(BoundaryBox.getPath(), 'insert_at', setCoordinates);
  map.styles = getADSBMapStyle();
};

function getBounds(Polygon) {
  var bounds = new google.maps.LatLngBounds();
  var paths = Polygon.getPaths();
  var path;
  for (var i = 0; i < paths.getLength(); i++) {
    path = paths.getAt(i);
    for (var ii = 0; ii < path.getLength(); ii++) {
      bounds.extend(path.getAt(ii));
    }
  }
  return bounds;
}


function updateCordinates() {
  var Cordinates = getCoordinates();
  var Bounds = setBoundary(Cordinates);
  BoundaryBox.setPath(Bounds);
  google.maps.event.addListener(BoundaryBox.getPath(), 'set_at', setCoordinates);
  google.maps.event.addListener(BoundaryBox.getPath(), 'insert_at', setCoordinates);

  var thisBounds = getBounds(BoundaryBox);
  map.fitBounds(thisBounds);

}

function getCoordinates() {
  var Cordinates = $('#GcaApproval_Coordinates').val();
  if (Cordinates == "") {
    Cordinates = getDefaultCoordinates();
    $('#GcaApproval_Coordinates').val(Cordinates);
  }
  return Cordinates;
}

function getDefaultCoordinates() {
  var thisCoordinates =
    '24.94990 55.337585,' +
    '25.21855 55.620971,' +
    '25.38770 55.414978,' +
    '25.08709 55.137084';

  return thisCoordinates;
}


function setCoordinates1(Cord) {
  $('#GcaApproval_Coordinates').val(Cord);
  $('#Show_Coordinates').html(Cord);
  if (LoadPolygonsTimer) window.clearTimeout(LoadPolygonsTimer);
  LoadPolygonsTimer = window.setTimeout(LoadPolygons, 100);
}

function setBoundary(Cordinates) {
  var Bounds = [];
  var LatLng = Cordinates.split(',');
  for (var i = 0; i < LatLng.length; i++) {
    var Bound = LatLng[i].split(" ");
    Bounds.push({ lat: parseFloat(Bound[0]), lng: parseFloat(Bound[1]) });
  }
  return Bounds;
}

function setCoordinates() {
  var Cord = getBoundary();
  setCoordinates1(Cord);
}

function getBoundary() {
  var Bounds = BoundaryBox.getPath().getArray();
  var LatLng = '';
  for (var i = 0; i < Bounds.length; i++) {
    if (LatLng != '') LatLng += ',';
    var Lat = Bounds[i].lat();
    var Lng = Bounds[i].lng();
    LatLng = LatLng + Lat.toFixed(5) + ' ' + Lng.toFixed(5);
  }
  return LatLng;
}

function LoadPolygons() {
  var Cordinates = getCoordinates();
  var OuterPolygon = '';
  $.ajax({
    type: 'GET',
    url: '/Rpas/GetOuterPolygon?InnerPolygon=' + Cordinates,
    dataType: "json",
    async: true,
    success: function (data) {
      LoadOutPoly(Cordinates, data)
    },
    error: function () {
      alert('error')
    }
  });
}

function ToPath(Coordinates) {
  var Path = [];
  if (Coordinates == '' || Coordinates == 'null') return Path;
  var aLatLng = Coordinates.split(',');
  for (var i = 0; i < aLatLng.length; i++) {
    var Points = aLatLng[i].split(' ');
    Path.push({ lat: parseFloat(Points[0]), lng: parseFloat(Points[1]) });
  }
  return Path;
}

function LoadOutPoly(inner, outer) {
  var InnerPolyPath = ToPath(inner);
  var OuterPath = ToPath(outer);
  if (InnerPolyPath.length < 1) return;
  if (OuterPath.length < 1) return;

  //Generate Holo Poly
  var HoloPath = InnerPolyPath;
  //Close the polygon
  HoloPath.push(InnerPolyPath[0]);
  HoloPath = HoloPath.concat(OuterPath.reverse());
  OuterPoly.setPath(HoloPath);
  OuterBorder.setPath(OuterPath);


}

