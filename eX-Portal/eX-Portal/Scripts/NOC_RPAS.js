var DroneApprovals = [];
var map = null;
var Form = document.forms['frmDroneSetup'];
var showMsgTimer = null;

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

  //code for no fly zones
  var KmlUrl = 'http://dcaa.exponent-ts.com/Map/NoFlyzone?R=' + Math.random();
  var kmlOptions = {
    preserveViewport: true,
    map: map
  };
  NoFlyZone = new google.maps.KmlLayer(KmlUrl, kmlOptions);
  NoFlyZone.setValues({ map: map });

  $('#NOCApplicationUser').on("change", NOCApplicationUserChanged);
  $('#lat').on("change", SetLatLng);
  $('#lng').on("change", SetLatLng);

  //****************************
  $("#submitButton").on("click", function (e) {
    e.preventDefault();
    $("body").scrollTop(0);
    var data = $('#frmDroneSetup').serialize();

    showMsg("Updating....Please Wait.");

    $("#frmDroneSetup :input").prop("disabled", true);

    $.ajax({
      type: 'POST',
      url: '/Rpas/NOCRegister',
      data: data,
      success: function (data) {
        var Rows = data.split("|");
        $("#frmDroneSetup :input").prop("disabled", false);
        if (Rows[0] === 'OK') {
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
  })

  //to dynamically show the other camera textbox
  $('#GcaApproval_FlightTypeID').on("change", function () {
    var s = $('#GcaApproval_FlightTypeID option:selected').text().toUpperCase();
    if (s.slice(0, 5) == 'OTHER') {
      $('#Other_FlightType').slideDown();
    } else {
      $('#Other_FlightType').slideUp();
    }//if
  })

  if (jQuery.ui) {
    $('input.time-picker').timepicker({});
  }



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


function NOCApplicationUserChanged(e) {
  var UserID = $(this).val();
  var TheList = document.forms[0]['GcaApproval.DroneID'];
  TheList.options.length = 1;
  TheList.options[0].text = "Reading...";

  $.ajax({
    type: 'GET',
    url: '/Rpas/NOCRegisterRPAS/' + UserID ,
    success: function (data) {
      if (data.length > 0) {
        TheList.options.length = data.length + 1;
        for (var i = 0; i < data.length; i++) {
          TheList.options[i + 1].value = data[i].Value;
          TheList.options[i + 1].text = data[i].Text;
        }
      }
      TheList.options[0].text = "Please Select a UAS...";
    },
    error: function () {

    }
  });
}



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
    strokeColor: '#FF0000',
    strokeOpacity: 0.7,
    strokeWeight: 1,
    fillColor: '#FF0000',
    fillOpacity: 0.1,
    editable: true,
    draggable: true
  });
  BoundaryBox.setMap(map);
  updateCordinates();

  google.maps.event.addListener(BoundaryBox.getPath(), 'set_at', setCoordinates);
  google.maps.event.addListener(BoundaryBox.getPath(), 'insert_at', setCoordinates);
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

  var Center = thisBounds.getCenter();
  $('#lat').val(Center.lat().toFixed(6));
  $('#lng').val(Center.lng().toFixed(6));

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
  var thisCoordinates = '25.05569 55.44882,25.12533 55.52759,25.16790 55.50150,25.09580 55.41586';

  return thisCoordinates;
}

function setCoordinates1(Cord) {
  $('#GcaApproval_Coordinates').val(Cord);
  var thisBounds = getBounds(BoundaryBox);
  var Center = thisBounds.getCenter();
  $('#lat').val(Center.lat().toFixed(6));
  $('#lng').val(Center.lng().toFixed(6));
  //map.fitBounds(thisBounds);
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


function SetLatLng() {
  var nLat = parseFloat($('#lat').val());
  var nLng = parseFloat($('#lng').val());
  if (isNaN(nLat)) return;

  var bounds = new google.maps.LatLngBounds();
  var paths = BoundaryBox.getPaths();
  var path;
  for (var i = 0; i < paths.getLength(); i++) {
    path = paths.getAt(i);
    for (var ii = 0; ii < path.getLength(); ii++) {
      bounds.extend(path.getAt(ii));
    }
  }
  var Center = bounds.getCenter();
  var cLat = Center.lat();
  var cLng = Center.lng();
  var nBounds = new google.maps.LatLngBounds();

  var nPath = [];
  path = paths.getAt(0);
  for (var j = 0; j < path.length; j++) {
    var pPath = path.getAt(j);
    var dLat = cLat - pPath.lat();
    var dLng = cLng - pPath.lng();

    var tLat = nLat + dLat;
    var tLng = nLng + dLng;
    console.log("dLat: " + dLat + ", lat: " + pPath.lat() + ", lng: " + pPath.lng() + ", tLat: " + tLat + ", tLng: " + tLng);
    var LN = { lat: tLat, lng: tLng };
    nPath.push(LN);
    nBounds.extend(LN);
  }
  BoundaryBox.setPath(nPath);
  google.maps.event.addListener(BoundaryBox.getPath(), 'set_at', setCoordinates);
  google.maps.event.addListener(BoundaryBox.getPath(), 'insert_at', setCoordinates);
  map.fitBounds(nBounds);
}