var MaxRecords = 2000;
var map;
var _Location = [];
var PlotTimer = null;
var PlotTimerDelay = 100;
var isReplayMode = false;
var mytimer = null;

var initLat = 24.9899106;
var initLng = 55.0034188;
var defaultZoom = 18;
var gpsGrpID;
var timezone;
var clientID;
var programName;

var livemarkers = [];
var datemarkers = [];
var secureDateMarker = [];
var historymarkers = [];
var allHistorymarkers = [];
var InfoMarker = null;
var trafficLayer = new google.maps.TrafficLayer();
var contentString;
var browser = false;
var enableRefreshIntervalID;
var pickupStatus = false;
var truckStatus = false;
var geocoder;
var LastPayLoadDataID = 0;
var INTERVAL = 100;
var poly;
var TimerValue = 0;
var LastDatas = [];
var i = 0;
var myInterval;
var myLatLong = [];
var MyLastLatLong = null;
var MyLastMarker = null;
var service = new google.maps.DirectionsService();
var path = new google.maps.MVCArray();
var bounds = new google.maps.LatLngBounds();
var infowindow = new google.maps.InfoWindow();
var aData = [];
var aLabels = [];
var aDatasets1 = [];
var aDatasets2 = [];
var aDatasets3 = [];
var aDatasets4 = [];
var aDatasets5 = [];
var data = [];
var lineChart = null;
var ldata = [];

var isGraphReplayMode = false;


$(document).ready(function () {

    initialize();
});

function initialize() {
    geocoder = new google.maps.Geocoder();

    var mapOptions = {
        zoom: defaultZoom,
        center: new google.maps.LatLng(initLat, initLng),
        panControl: false,
        mapTypeControl: true,
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

        mapTypeId: google.maps.MapTypeId.HYBRID
    };
    map = new google.maps.Map(document.getElementById('map_canvas'),
        mapOptions);

    poly = new google.maps.Polyline({
        strokeColor: '#000000',
        strokeOpacity: 1.0,
        strokeWeight: 3
    });
    poly.setMap(map);
    var loctr = '<thead><tr><th>RFID</th><th>RSSI</th>'
                + '<th>ReadTime</th><th>Latitude</th>'
                + '<th>Longitude</th><th>GPSFix</th>'
                + '<th>ReadCount</th><th>ReadFrequency</th>'
                + '</tr></thead>';
    var firsttr = '<tr style="display:none"><td></td><td></td>'
               + '<td></td><td></td>'
               + '<td></td><td></td>'
               + '<td></td><td></td>'
               +'</tr>';
    $('#MapData table').append(loctr);
    $('#MapData table').append(firsttr);
    $('#MapData table').addClass('report');
   
    GetPayLoadData();
}

function GetPayLoadData() {
    var _locVal = [];
    $.ajax({
        type: "GET",
        url: MapDataURL + "&LastFlightDataID=" + LastPayLoadDataID + '&MaxRecords=' + MaxRecords,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        async: true,
        success: function (msg) {
            //   try {

            msg = msg.hasOwnProperty('d') ? msg.d : msg;
            $.each(msg, function (index, obj) {
                _Location.push(obj);
                LastPayLoadDataID = obj['PayLoadDataMapID'];
            });


            if (isReplayMode) {
                plotPoints();
            } else {
                directPlotPoints();
            }
            //LastDatas = _Location;

            // }
            //catch (err) {
            //    alert('Live Drone Position Error' + err);
            //}

        },
        failure: function (msg) {
            alert('Live Drone Position Error' + msg);
        },
        complete: function (msg) {
            if (mytimer) window.clearTimeout(mytimer);
       //     mytimer = window.setTimeout(GetPayLoadData, 1000);
        }
    });

}

function directPlotPoints() {
    var locationLength = _Location.length;
    if (locationLength < 1)
        isGraphReplayMode = true;
    while (1) {
        if (_Location.length < 1) break;
        var thisPoint = _Location.shift();

        setMarker(map, thisPoint);
      //  thisPoint = SetCurrentValues(thisPoint);
        SetMapTable(thisPoint);

    }
   


}
function plotPoints() {
    if (PlotTimer) window.clearTimeout(PlotTimer);
    if (_Location.length < 1) return;

    var thisPoint = _Location.shift();


    setMarker(map, thisPoint);
  //  thisPoint = SetCurrentValues(thisPoint);
    SetMapTable(thisPoint);

    var delay = getDelay(thisPoint);
    PlotTimer = window.setTimeout(plotPoints, delay);

}


function setMarker(map, loc) {
   
    var body = '' +
           '<b>' + loc['RFID'] + '</b><br>\n' +
           'Latitude: ' + loc['Latitude'] + '<br>\n' +
           'Longitude: ' + loc['Longitude'] + '<br>\n';
           
    var myLatLng = new google.maps.LatLng(loc['Latitude'], loc['Longitude']);
    var marker = createMarker(map, myLatLng, loc['RFID'], body, i);
}


function createMarker(map, latlng, heading, body, zindex) {
    if (poly.map == null)
    {
        //addLines();
    }
    var path = poly.getPath();

  //  path.push(latlng);
    var image = '/bullet_blue.png';
    var marker = new google.maps.Marker({
        position: latlng,
        map: map,
        icon: image,
        title: heading,
        zIndex: 9999
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

   // var ibLabel = new InfoBox(myOptions);
    //ibLabel.open(map);

    marker.addListener('click', function () {
        var infowindow = new google.maps.InfoWindow({
            content: body
        });
        infowindow.open(map, marker);
    });
    if (MyLastMarker != null) {
        MyLastMarker.setIcon('/bullet_blue.png');
    }
    MyLastMarker = marker;
    map.setCenter(latlng);
    closeMargin = '120px';
    livemarkers.push(marker);
   
  
}

function ShowInfo(marker, i) {
    return function () {
        infowindow.open(map, marker);
    }
}


function deleteMarkers() {
    clearMarkers(null);
    livemarkers = [];
}

function getDelay(TheObj) {
    if (TheObj == null) return PlotTimerDelay;
    if (isReplayMode) {
        var delay = TheObj['Speed'] * 5000;
        if (delay < PlotTimerDelay) delay = PlotTimerDelay;
        return delay;
    } else {
        return PlotTimerDelay
    }
}




function Replay() {
    //mytimer = null;
    _Location = [];
    if (PlotTimer) window.clearTimeout(PlotTimer);
    PlotTimer = null;
    isReplayMode = true;
    MaxRecords = 20;

    clearTimeout(mytimer);
    MyLastLatLong = null;
    MyLastMarker = null;
    myLatLong = null;
    LastDroneDataID = 0;
    deleteMarkers();
    removeLines();
    GetPayLoadData();
 //   ClearChartValues();
   // GetChartData();
    //lineChart.initialize(data);


    // addLines();
}
function addLines() {
    path = new google.maps.MVCArray();
    poly.setPath(path);
    poly.setMap(map);
}

function clearMarkers(map) {
    for (var i = 0; i < livemarkers.length; i++) {
        livemarkers[i].setMap(map);
    }

}
function removeLines() {
    poly.setMap(null);
    poly.latLngs.clear();
    // path = [];
    //    setTimeout()
    // flightPath.setMap(null);
}

function SetCurrentValues(_LastValue) {
    var date;

    $.each(_LastValue, function (key, value) {
        if (value == null) value = '';
        switch (key) {
            case "ReadTime":
                var iDt = parseInt(_LastValue['ReadTime'].substr(6));
                var theDate = new Date(iDt);
                value = fmtDt(theDate);
                break;
            case "Distance":
                value = parseInt(value);
                if (isNaN(value)) value = 0;
                break;
            case "avg_Altitude":
            case "Min_Altitude":
            case "Max_Altitude":
            case "Altitude":
                value = parseFloat(value);
                if (isNaN(value)) value = 0;
                value = value.toFixed(2);
                break;
            case "Speed":
            case "Avg_Speed":
            case "Min_Speed":
            case "Max_Speed":
                value = parseFloat(value);
                if (isNaN(value)) value = 0;
                //if (value > 0) value = value / (60 * 60) * 1000;
                value = value.toFixed(2);
                break;
            case "TotalFlightTime":
                value = parseFloat(value);
                if (isNaN(value)) value = 0;
                if (value > 0) value = value / (60 * 60);
                value = value.toFixed(2);
                break;
            case "Heading":
                value = parseFloat(value);
                if (isNaN(value)) value = 0;
                if (value < 0) value = value + 360;
                value = value.toFixed(2);
                break;


        }
        _LastValue[key] = value;
        $('#data_' + key).html(value);
    });

    MyLastLatLong = new google.maps.LatLng(_LastValue['Latitude'], _LastValue['Longitude']);
    return _LastValue;
    // var oCompaniesTable = $('#MapData Table')
}

function SetMapTable(_LastValue) {

    if (_LastValue != null) {
        var vdate = new Date( parseInt(_LastValue['ReadTime'].substr(6)));
        var theDate = fmtDt(vdate);
        var tRFID = '<td>' + _LastValue['RFID'] + '</td>';
        var tRSSI = '<td>' + _LastValue['RSSI'] + '</td>';
        var tDrTime = '<td>' + theDate + '</td>';
        var tLatitude = '<td>' + _LastValue['Latitude'] + '</td>';
        var tLongitude = '<td>' + _LastValue['Longitude'] + '</td>';
        var tGPSFix = '<td>' + _LastValue['GPSFix'] + '</td>';
        var tReadcount = '<td>' + _LastValue['ReadCount'] + '</td>';
       
        var tReadFreq = '<td>' + _LastValue['ReadFreq'] + '</td>';
     
        var tTotFlightTimeData = '';// '<td>' + _LastValue['TotalFlightTime'] + '</td>';
        var loctr = '<tr>' + tRFID + tRSSI + tDrTime + tLatitude + tLongitude + tGPSFix + tReadcount + tReadFreq + '</tr>';
        $('#MapData table > tbody > tr:first').after(loctr);
        /*
        
        if ($("#MapData > table > tbody > tr").length > 20)
            $('#MapData > table > tbody > tr:last').remove();*/
    }

    //get the last item
    $('#map-info').html(
      theDate + ', ' +
      'Lat: ' + _LastValue['Latitude'] + ', Lon: ' + _LastValue['Longitude']);
}

function fmtDt(date) {
    if (date instanceof Date) {

    } else {
        return 'Invalid';
    }
    var hours = date.getHours();
    var minutes = date.getMinutes();
    var Months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
    //var ampm = hours >= 12 ? 'pm' : 'am';
    //hours = hours % 12;
    //hours = hours ? hours : 12; // the hour '0' should be '12'
    minutes = minutes < 10 ? '0' + minutes : minutes;
    var strTime = hours + ':' + minutes + ':' + date.getSeconds();
    var strDate = date.getDate() + "-" + Months[date.getMonth()] + "-" + date.getFullYear();
    return strDate + " " + strTime;
}
