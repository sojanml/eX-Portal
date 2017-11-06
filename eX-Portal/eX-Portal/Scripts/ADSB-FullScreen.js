var _ADSBLayer = null;
var RefreshTimer = null;
var LastProcessedID = null;
var ChartIndex = 0;
var UpdateDelay = 1 * 1000;
var IsQueryChanged = 0;
var timeZoneOffset = 0; //(new Date()).getTimezoneOffset();
var TheChartObject = null;
var zonespoly = [];
var EditZone = { ID: 0, Name: "", Coordinates: "", FillColour: "", StartDate: "", EndDate: "", StartTime: "", EndTime: "", ZoneDescription: "", DisplayType: "", IsDeleted: 0};
var editzonepoly = new google.maps.Polygon({});
var DefaultZonesPoly = [];
var newzonepoly = new google.maps.Polygon({});
var map = null;
var AllZones = [];
var Timers = {
  getADSB: null,
  getChartData: null
}

$(document).ready(function () {
  initializeMap();
  $('#Coordinates').on("change", function (e) {
      e.preventDefault();
      updateCordinates();
  });
  $('body').on('focus', ".date-picker", function () {
      $(this).datepicker({
          dateFormat: 'dd-M-yy',
          changeYear: true,
          changeMonth: true
      });
  });
  $('body').on('focus', ".time-picker", function () {
      $(this).timepicker();
  });

  $("#DetailDiv").hide();
  $("#SaveZone").click(function () {
      SetZoneValues();
      SaveZone(EditZone);
  });
  $("#CancelZone").click(function () {
      //newzonepoly.setMap(null);
      var cord = ToPath(EditZone.Coordinates);
      editzonepoly.setPath(cord);
      EditZone = { Name: "", Coordinates: "", FillColour: "", StartDate: "", EndDate: "", StartTime: "", EndTime: "", ZoneDescription: "", DisplayType: "",IsDeleted:0 };
      SetEditable(-1, false);
      AddPolyClicks();
      $("#AddDiv").show();
      $("#DetailDiv").hide();
  });

  $("#RemoveZone").click(function () {
      SetZoneValues();
      RemoveZone(EditZone);
     
  });

 // Timers['getADSB'] = window.setTimeout(getADSB, 100, _ADSBLayer);
  
});


function ReGetAdsb() {
  IsQueryChanged = 1;
  if (Timers['getADSB']) window.clearTimeout(Timers['getADSB']);
  Timers['getADSB'] = window.setTimeout(getADSB, 400, _ADSBLayer);
}

function RequestFilterData() {
  //console.log("Running Timer ID : " + RefreshTimer);
  if (RefreshTimer) window.clearTimeout(RefreshTimer);
  RefreshTimer = null;
  
  //getStatus(_ADSBLayer);
}

function initializeMap() {
  var MarkerPosition = { lat: 25.2532, lng: 55.3657 };
  var AddbtnDiv = document.createElement('div');
  var btnctrlText = document.createElement('div');
  var mapOptions = {
    zoom: 10,
    mapTypeControl: true,
    streetViewControl: false,
    center: MarkerPosition,
    styles: getADSBMapStyle()
  };

  map = new google.maps.Map(document.getElementById('adsb_map'), mapOptions);
  //code for no fly zones
  var KmlUrl = 'http://dcaa.exponent-ts.com/Map/NoFlyzone?R=' + Math.random();
  var kmlOptions = {
      preserveViewport: true,
      map: map
  };
  NoFlyZone = new google.maps.KmlLayer(KmlUrl, kmlOptions);
               // NoFlyZone.setValues({ map: map });
  //_ADSBLayer = new ADSBOverlay({ map: map }, []);
 // CenterControl(AddbtnDiv, map, btnctrlText, 'add new zone');
  $('#addzone').on('click', function () {
      addnewPolygon(map, event.latLng);
      $("#DetailDiv").show();
      $("#AddDiv").hide();
      $("#RemoveZone").hide();
      RemovePolyClicks();

  });
  LoadZones(map);




 

}

function DrawAllZones(zones,map)
{
    for (var i = 0; i < zones.length; i++) {
        DrawPolygons(zones[i], map,i+1);
    }
}

function CenterControl(controlDiv, map, controlText ,buttontxt ) {

    // Set CSS for the control border.
    var controlUI = document.createElement('div');
    controlUI.style.backgroundColor = '#fff';
    controlUI.style.border = '2px solid #fff';
    controlUI.style.borderRadius = '3px';
    controlUI.style.boxShadow = '0 2px 6px rgba(0,0,0,.3)';
    controlUI.style.cursor = 'pointer';
    controlUI.style.marginBottom = '22px';
    controlUI.style.textAlign = 'center';
    controlUI.style.width = '100px';
    controlUI.title = '';
    controlDiv.appendChild(controlUI);
    map.controls[google.maps.ControlPosition.TOP_CENTER].push(controlDiv);
    // Set CSS for the control interior.
    controlText.style.color = 'rgb(25,25,25)';
    controlText.style.fontFamily = 'Arial,sans-serif';
    controlText.style.fontSize = '10px';
    controlText.style.lineHeight = '38px';
    controlText.style.paddingLeft = '5px';
    controlText.style.paddingRight = '5px';
    controlText.innerHTML = buttontxt;
    controlUI.appendChild(controlText);
}
function DetailControls(zone)
{
    // zonename.innerHTML = zone.name;
    $("#zonename").val(zone.Name);
    $("#ZoneCoordinates").val(zone.Coordinates);
    $("#colorpicker").val(zone.FillColour);
    $("#startdate").val(_ToDate(zone.StartDate));
    $("#enddate").val(_ToDate(zone.EndDate));
    $("#starttime").val(_ToTime(zone.StartTime));
    $("#endtime").val(_ToTime(zone.EndTime));
    $("#desc").val(zone.ZoneDescription);

}
function ClearControls()
{
    $("#zonename").val('');
    $("#ZoneCoordinates").val('');
    $("#colorpicker").val('');
    $("#startdate").val(_today);
    $("#enddate").val(_today);
    $("#starttime").val('');
    $("#endtime").val('');
    $("#desc").val('');
}
var _today = function () {
    var today = new Date();
    return _ToString(today);
}
function _ToDate (strDate) {
    if (strDate === null) return new _today();

    var r = /\/Date\(([0-9]+)\)\//i
    var matches = strDate.match(r);
    if (matches !== null && matches.length === 2) {
        var fDate = new Date(parseInt(matches[1]));
        return _ToString(fDate);
    }

    var nDate = new Date(Date.parse(strDate));
    if (nDate === null || isNaN(nDate)) return new _today();

    return _ToString(nDate);
}
function _ToTime(timeObj) {
    if (typeof timeObj === "string") return timeObj;
    //"Hours": 6, "Minutes": 0, "Seconds": 0, 
    var H = (timeObj.Hours <= 9 ? '0' : '') + timeObj.Hours;
    var M = (timeObj.Minutes <= 9 ? '0' : '') + timeObj.Minutes;
    return H + ':' + M;
}

function _ToString(objDate) {
    var Months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
    var sDate =
        objDate.getDate() + '-' +
        Months[objDate.getMonth()] + '-' +
        objDate.getFullYear();

    return sDate;
}

function addnewPolygon(map) {
    ClearControls();
    
    var _DefaultCoordinates = '25.05569 55.44882,25.12533 55.52759,25.16790 55.50150,25.09580 55.41586';
    var BoundaryBoxCoordinates = getLatLngArray(_DefaultCoordinates);
    BoundaryBox = new google.maps.Polygon({
        paths: BoundaryBoxCoordinates,
        strokeColor: '#FF0000',
        strokeOpacity: 0.7,
        strokeWeight: 1,
        fillColor: '#FF0000',
        fillOpacity: 0.1,
        editable: true,
        draggable: true
    });
    BoundaryBox.setMap(map);

    editzonepoly = BoundaryBox;
    EditZone = { ID: 0, Name: "", Coordinates: "", FillColour: "", StartDate: "", EndDate: "", StartTime: "", EndTime: "", ZoneDescription: "", DisplayType: "" };
    google.maps.event.addListener(BoundaryBox.getPath(), 'set_at', setCoordinates);
    google.maps.event.addListener(BoundaryBox.getPath(), 'insert_at', setCoordinates);
    setCoordinates();
}

function getLatLngArray(Cordinates) {
    var Bounds = [];
    var LatLng = Cordinates.split(',');
    for (var i = 0; i < LatLng.length; i++) {
        var Bound = LatLng[i].split(" ");
        Bounds.push({ lat: parseFloat(Bound[0]), lng: parseFloat(Bound[1]) });
    }
    return Bounds;
}

function LoadZones(map) {
   // var Cordinates = getCoordinates();
    var OuterPolygon = '';
    $.ajax({
        type: 'GET',
        url: '/ADSB/NoFlyZone',
        dataType: "json",
        async: true,
        success: function (data) {
            AllZones = data;
            DrawAllZones(data, map);
        },
        error: function () {
            alert('error')
        }
    });
}
function SetZoneValues()
{
    EditZone.Name = $("#zonename").val();
    EditZone.Coordinates = $("#ZoneCoordinates").val();
    EditZone.FillColour = $("#colorpicker").val();
    EditZone.StartDate = $("#startdate").val();
    EditZone.EndDate = $("#enddate").val();
    EditZone.StartTime = $("#starttime").val();
    EditZone.EndTime = $("#endtime").val();
    EditZone.ZoneDescription = $("#desc").val();
  //  EditZone.DisplayType=
   // EditZone.
}
function SaveZone(zone) {
    // var Cordinates = getCoordinates();
    var OuterPolygon = '';
    $.ajax({
        type: 'POST',
        url: '/ADSB/SaveZone',
        dataType: "json",
        async: true,
        data :zone,
        success: function (data) {
            //AllZones = data;
            var newpath = ToPath(zone.Coordinates);
            if (zone.ID === 0)
                {
            zone.ID = data;
            
            
            editzonepoly.setOptions({
                paths: newpath ,
                strokeWeight: 0,
                fillColor: 'rgb(255,165,0)',
                fillOpacity: 0.9,
                zIndex: 1,
                content:  zone.Name,
                mapid: zone.ID,
                edit: false,
                draggable: false
            });
            zonespoly.push(editzonepoly);
            AllZones.push(zone);
            }
            else {
                editzonepoly.setPath(newpath);
                editzonepoly.setDraggable(false);
                editzonepoly.setEditable(false);
            }
        
            editzonepoly = new google.maps.Polygon({});
         // SetEditable(-1, false);
            AddPolyClicks();
            //DrawAllZones(data, map);
           
            $("#AddDiv").show();
            $("#DetailDiv").hide();
        },
        error: function () {
            alert('error');
        }
    });
}
function RemoveZone(zone) {
    $.ajax({
        type: 'POST',
        url: '/ADSB/RemoveZone',
        dataType: "json",
        async: true,
        data: zone,
        success: function (data) {
            RemovezonePolygon();
            editzonepoly.setMap(null);
            EditZone = { Name: "", Coordinates: "", FillColour: "", StartDate: "", EndDate: "", StartTime: "", EndTime: "", ZoneDescription: "", DisplayType: "", IsDeleted: 0 };
            SetEditable(-1, false);
            AddPolyClicks();
            $("#AddDiv").show();
            $("#DetailDiv").hide();
            
        }
    });
}
function DrawPolygons(zone,map,index)
{
   
        var InnerPolyPath = [];
        InnerPolyPath = ToPath(zone.Coordinates);
        if (InnerPolyPath.length !== 0)
            {
        var fcolor = '';
        if (zone.FillColour === 'Red')
            fcolor = 'rgb(255, 0, 0)';
        else if (zone.FillColour === 'Green')
            fcolor = 'rgb(0, 160, 0)';
        else
            fcolor = 'rgb(255,165,0)';
        var InnerPoly = new google.maps.Polygon({
            paths: InnerPolyPath,
            strokeWeight: 0,
            fillColor: fcolor,
            fillOpacity: 0.9,
            zIndex: 1,
            content: index + '' + zone.Name,
            mapid: zone.ID
        });
        InnerPoly.setMap(map);
        InnerPoly.addListener('click', function (event) {
            //DrawAllZones(zonespoly, map);
            InnerPoly.setEditable(true);
            InnerPoly.setDraggable(true);
            DetailControls(zone);
            SetEditable(this.mapid, false);
            EditZone = zone;
            $("#DetailDiv").show();
            $("#AddDiv").hide();
            $("#RemoveZone").show();
        });
        zonespoly.push(InnerPoly);
        google.maps.event.addListener(InnerPoly.getPath(), 'set_at', setCoordinates);
        google.maps.event.addListener(InnerPoly.getPath(), 'insert_at', setCoordinates);
        }
}
function updateCordinates() {
    var Cordinates = getCoordinates();
   // var Bounds = setBoundary(Cordinates);
    BoundaryBox.setPath(Bounds);
    google.maps.event.addListener(BoundaryBox.getPath(), 'set_at', setCoordinates);
    google.maps.event.addListener(BoundaryBox.getPath(), 'insert_at', setCoordinates);

    var thisBounds = getBounds(BoundaryBox);
    map.fitBounds(thisBounds);

}

function getBoundary() {
    var Bounds = editzonepoly.getPath().getArray();
    var LatLng = '';
    for (var i = 0; i < Bounds.length; i++) {
        if (LatLng !== '') LatLng += ',';
        var Lat = Bounds[i].lat();
        var Lng = Bounds[i].lng();
        LatLng = LatLng + Lat.toFixed(5) + ' ' + Lng.toFixed(5);
    }
    return LatLng;
}

function getCoordinates() {
    var Cordinates = $('#ZoneCoordinates').val();
    if (Cordinates === "") {
        Cordinates = getDefaultCoordinates();
        $('#ZoneCoordinates').val(Cordinates);
    }
    return Cordinates;
}
function setCoordinates() {
    var cord = getBoundary();
    $('#ZoneCoordinates').val(cord);
}

function SetEditable(mapid,status)
{
    for (var i = 0; i < zonespoly.length; i++)
    {
        var p = zonespoly[i].mapid;
        if (mapid === -1 || p !== mapid)
        {
           
            zonespoly[i].setEditable(status);
            zonespoly[i].setDraggable(status);
            if(i!==-1)
             google.maps.event.clearListeners(zonespoly[i], 'click'); 
        }
        else
            editzonepoly = zonespoly[i];
        
    }
}

function RemovePolyClicks()
{
    for (var i = 0; i < zonespoly.length; i++) {
        google.maps.event.clearListeners(zonespoly[i], 'click'); 
    }
}
function AddPolyClicks()
{
    for (var i = 0; i < zonespoly.length; i++) {
        var zone = AllZones[i];
        var mapid = zone.ID;
        var poly = zonespoly[i];
        SetPolyClicks(zone, mapid, poly);
    }
}

function SetPolyClicks(zone,mapid,poly)
{
    poly.setEditable(false);
    google.maps.event.addListener(poly, 'click', function (event) {
        //DrawAllZones(zonespoly, map);
        poly.setEditable(true);
        poly.setDraggable(true);
        DetailControls(zone);
        SetEditable(mapid, false);
        EditZone = zone;
        $("#DetailDiv").show();
        $("#AddDiv").hide();
        $("#RemoveZone").show();
    });
}

function ToPath(Coordinates) {
    var Path = [];
    if (Coordinates === '' || Coordinates === null)
        {
        return Path;
    }
    else
        {
    var aLatLng = Coordinates.split(',');
    for (var i = 0; i < aLatLng.length; i++) {
        var Points = aLatLng[i].split(' ');
        Path.push({ lat: parseFloat(Points[0]), lng: parseFloat(Points[1]) });
    }
    return Path;
    }
}

function RemovezonePolygon()
{
    for (var i = 0; i < zonespoly.length; i++) {
        if (EditZone.ID === zonespoly[i].mapid)
        {
            zonespoly.splice($.inArray(zonespoly[i], zonespoly), 1);
            AllZones.splice($.inArray(EditZone, AllZones), 1);
            
            break;
        }
    }
}