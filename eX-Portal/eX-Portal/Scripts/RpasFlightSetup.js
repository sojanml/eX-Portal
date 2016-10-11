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

  var KmlUrl = 'http://test.exponent-ts.com/Map/NoFlyzone?R=' + Math.random();
  var kmlOptions = {
    preserveViewport: true,
    map: map
  };
  NoFlyZone = new google.maps.KmlLayer(KmlUrl, kmlOptions);
  NoFlyZone.setValues({ map: map });


  //****************************

    $("#submitButton").on("click", function (e) {
        e.preventDefault();
        $("body").scrollTop(0);
        var data = $('#frmDroneSetup').serialize();

        showMsg("Updating....Please Wait.");

        $("#frmDroneSetup :input").prop("disabled", true);

        $.ajax({
            type: 'POST',
            url: '/Rpas/FlightRegister',
            data: data,
            success: function (data) {
                $("#frmDroneSetup :input").prop("disabled", false);
                if (data == 'OK') {
                    showMsg("Data has been updated. ")
                } else {
                    showMsg(data);
                }
            },
            error: function () {

            }
        });
    });


    $('#ispilot').on("change", function (e) {
        if ($(this).prop('checked')) {
            document.forms['frmDroneSetup']['GcaApproval.PilotUserId'].value = LoginUserID;
        }
    })


    $('#isgroundstaff').on("change", function (e) {
        if ($(this).prop('checked')) {
            document.forms['frmDroneSetup']['GcaApproval.GroundStaffUserId'].value = LoginUserID;
        }
    })


    $('#GcaApproval_GroundStaffUserId').on("change", function (e) {
        $('#isgroundstaff').prop('checked', false);
        $('#error-message').html("").hide();
    })


    $('#GcaApproval_PilotUserId').on("change", function (e) {
        $('#ispilot').prop('checked', false);
        $('#error-message').html("").hide();
    })

    $('#GcaApproval_DroneID').on("change", function () {
        $("#frmDroneSetup :input").prop("disabled", true);
        $('#error-message').html("").hide();

        var droneeid = $("#GcaApproval_DroneID").val();
        $.ajax({
            url: '/DroneFlight/FillPilot',
            type: "GET",
            dataType: "JSON",
            data: { droneid: droneeid },
            success: DroneID_Success
        });
    });// $('#DroneSetup_DroneId').on("change")

    $('#GcaApproval_ApprovalID').on("change", function (e) {
        e.stopPropagation();
        e.preventDefault();
        $('#error-message').html("").hide();

        var FORM = document.forms['frmDroneSetup'];
        var index = $(this).prop("selectedIndex");
        if (index <= 0) {

            setDefaultApprovals();
        } else {
            var Item = index - 1;
            FORM['GcaApproval.Coordinates'].value = DroneApprovals[Item].Cordinates;
            FORM['GcaApproval.EndDate'].value = toDate(DroneApprovals[Item].EndDate);
            FORM['GcaApproval.EndTime'].value = DroneApprovals[Item].EndTime;
            FORM['GcaApproval.MaxAltitude'].value = DroneApprovals[Item].MaxAltitude;
            FORM['GcaApproval.MinAltitude'].value = DroneApprovals[Item].MinAltitude;
            FORM['GcaApproval.StartDate'].value = toDate(DroneApprovals[Item].StartDate);
            FORM['GcaApproval.StartTime'].value = DroneApprovals[Item].StartTime;

        }
        updateCordinates();
    });// $('#GcaApproval_ApprovalID').on("change")


    $('#GcaApproval_Coordinates').on("change", function (e) {
        e.preventDefault();
        updateCordinates();
    })

    $('#GcaApproval_IsUseCamara').on("change", function () {
        if ($(this).val() == '1') {
            $('#cameraopt').slideDown();           
        } else {
            $('#cameraopt').slideUp();            
        }//if
    })

    //to dynamically show the other camera textbox
    $('#GcaApproval_CameraId').on("change", function () {
        if ($(this).val() == '1') {
            $('#othercamera').slideDown();
        } else {
            $('#othercamera').slideUp();
        }//if
    })

    if (jQuery.ui) $('input.time-picker').timepicker({});

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
        zoom: 10
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
    for (var i = 0; i < paths.getLength() ; i++) {
        path = paths.getAt(i);
        for (var ii = 0; ii < path.getLength() ; ii++) {
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
