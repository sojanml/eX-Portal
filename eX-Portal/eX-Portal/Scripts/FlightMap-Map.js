﻿var map;

var Timers = {
  LoadMapData: null
};
var IsInitilized = false;
var _FlightVideoPlayer = null;

var _FlightData = [];
var _FlightPath = null;
var _FlightCoordinates = [];
var _FlightDataID = 0;
var _FlightBoundBox = null;
var _FlightStartMarker = null;
var _FlightEndMarker = null;
var _FlightReplayPath = null;
var _RPASIcon = null;

var _ReplayIndex = 0;
var _ReplayTimer = null;
var _IsGetNextDataSet = true;

$(document).ready(function () {
  initializeMap();
  LoadPolygons();
  InitChart();
  LoadMapData();
  InitVideos();

  $('#FlightReplay').on("click", StartFlightReplay);

});

function LoadPolygons() {
  var InnerPolyPath = ToPath(Boundaries[0]);
  var OuterPath = ToPath(Boundaries[1]);
  if (InnerPolyPath.length < 1) return;
  if (OuterPath.length < 1) return;

  //Generate Holo Poly
  var HoloPath = InnerPolyPath;
  //Close the polygon
  HoloPath.push(InnerPolyPath[0]);
  HoloPath = HoloPath.concat(OuterPath.reverse());

  // Construct the polygon.
  var InnerPoly = new google.maps.Polygon({
    paths: InnerPolyPath,
    strokeWeight: 0,
    fillColor: 'rgb(101, 186, 25)',
    fillOpacity: 0.2,
    map: map
  });
  var OuterBorder =  new google.maps.Polyline({
    path: OuterPath,
    geodesic: true,
    strokeColor: 'red',
    strokeOpacity: 0.3,
    strokeWeight: 3,
    map: map
  });
  var OuterPoly = new google.maps.Polygon({
    paths: HoloPath,
    strokeWeight: 0,
    fillColor: '#fd2525',
    fillOpacity: 0.2,
    map: map
  });
}

function ToPath(Coordinates) {
  var Path = [];
  if (Coordinates == '' || Coordinates == 'null') return Path;
  var aLatLng = Coordinates.split(',');
  for (var i = 0; i < aLatLng.length; i++) {
    var Points = aLatLng[i].split(' ');
    Path.push({ lat: parseFloat(Points[0]), lng: parseFloat(Points[1])});
  }
  return Path;
}

function StartFlightReplay() {
  //clear chart
  ClearChart();
  $('#FlightDataScroll').empty();
  if (_FlightVideoPlayer) _FlightVideoPlayer.stop();

  _ReplayIndex = 0;
  _ReplayTimer = window.setTimeout(RpasReplayTimer, 500);
  _FlightPath.setOptions({ 'strokeOpacity': 0.1 });

  var Path = _FlightReplayPath.getPath();
  Path.clear();  
}

function RpasReplayTimer() {
  var thisData = _FlightData[_ReplayIndex];
  var ReplayDelay = 500;
  var PositionDateTime = dtFromJSON(thisData.FlightTime);
  var position = {
    lat: thisData.Lat,
    lng: thisData.Lng,
  };
  var Pos = new google.maps.LatLng(thisData.Lat, thisData.Lng);
  _RPASIcon.setPosition(Pos);
  ShowFlightInformation(thisData);
  AddToTable([thisData]);
  AddChart(thisData);

  //add to path completed polyline
  var Path = _FlightReplayPath.getPath();
  Path.push(Pos);

  if (_FlightVideos.length) {
    ReplayDelay = 200;
    var FirstVideoDate = new Date(_FlightVideoPlayer.getPlaylistItem(_FlightVideoPlayer.getPlaylistIndex()).title);
    var VideoPosition = _FlightVideoPlayer.getPosition();
    var VideoPositionTime = FirstVideoDate;
    VideoPositionTime.setSeconds(FirstVideoDate.getSeconds() + VideoPosition);
    if (PositionDateTime < VideoPositionTime) {
      _ReplayIndex++;
    } else {
      var VideoState = _FlightVideoPlayer.getState();
      console.log("Waiting at index: " + _ReplayIndex + ', VideoState: ' + VideoState);
      if (VideoState == 'idle') {
        _FlightVideoPlayer.play();
      }
      
    }
  } else {
    ReplayDelay = 1000;
    _ReplayIndex++;
  }
  
  //Play the next item
  if (_ReplayIndex >= _FlightData.length) {
    RpasReplayCompleted();
  } else {
    if (_ReplayTimer) window.clearTimeout(_ReplayTimer);
    _ReplayTimer = window.setTimeout(RpasReplayTimer, ReplayDelay);
  }


}



function RpasReplayCompleted() {
  //finishing touches.
  _FlightPath.setOptions({ 'strokeOpacity': 1 });
  _FlightReplayPath.setMap(null);
}

function InitVideos() {
  if (IsLive) {
    //live video
  } else if (_FlightVideos.length < 1) {
    return;
  }

  var VideoSetup = {
    width: 240,
    height: 160,
    description: 'Click on play to start video.',
    mediaid: 'b669e4e759db46e996db1432cdff433c'
  };

  if (IsLive) {
    VideoSetup.file = "rtmp:" + "//52.29.242.123/live/drone" + DroneID;
  } else {
    VideoSetup.playlist = GetVideoPlaylist()
  }

  jwplayer.key = "vYTpeN5XOdY1qcyCv75ibloaO/VRGoOeHn6CsA==";
  _FlightVideoPlayer = jwplayer("FlightVideo");
  _FlightVideoPlayer.setup(VideoSetup);

  //playerInstance.on("play", fn_on_play);
  //playerInstance.on("pause", fn_on_pause);
}

function GetVideoPlaylist() {
  var PlaySources = [];
  for (var i = 0; i < _FlightVideos.length; i++) {
    var VideoURL = "http://exponent-s3.s3.amazonaws.com/MP4/" + _FlightVideos[i].VideoName.replace(".flv", ".mp4");
    var Source = {
      sources: [{ file: VideoURL }],
      title: dtConvFromJSON(_FlightVideos[i].VideoDate, true)
    };
    PlaySources.push(Source);
  }
  return PlaySources;
}

function initializeMap() {
  var MarkerPosition = { lat: 25.0955354, lng: 55.1527025 };

  var mapOptions = {
    zoom: 14,
    mapTypeControl: false,
    streetViewControl: false,
    center: MarkerPosition,
    styles: getADSBMapStyle()
  };

  map = new google.maps.Map(document.getElementById('flightmap'), mapOptions);
  map.setTilt(45);

  _FlightPath = new google.maps.Polyline({
    path: [],
    geodesic: true,
    strokeColor: '#12ffaf',
    strokeOpacity: 1.0,
    strokeWeight: 2,
    map: map
  });
  _FlightBoundBox = new google.maps.LatLngBounds();

  _FlightReplayPath = new google.maps.Polyline({
    path: [],
    geodesic: true,
    strokeColor: 'red',
    strokeOpacity: 1.0,
    strokeWeight: 2,
    map: map
  });

  var KmlUrl = 'http://test.exponent-ts.com/Map/NoFlyzone';
  var kmlOptions = {
    preserveViewport: true,
    map: map
  };
  NoFlyZone = new google.maps.KmlLayer(KmlUrl, kmlOptions);

}

function LoadMapData() {
  $.ajax({
    type: "GET",
    url: '/FlightMap/Data/' + FlightID + '?FlightMapDataID=' + _FlightDataID,
    contentType: "application/json;charset=utf-8",
    dataType: "json",
    success: function (msg) {
      if (msg.Data.length > 0) {
        _FlightData = _FlightData.concat(msg.Data);
        if (!IsInitilized) InitilizeMapData();
        AddDataToMap(msg.Data);
        AddChart(msg.Data);
        AddToTable(msg.Data);
        IsInitilized = true;
      } else {
        _IsGetNextDataSet = IsDataLoadCompleted()
      }
    },
    failure: function (msg) {
      alert('Live Drone Position Error' + msg);
    },
    complete: function (msg) {      
      if (Timers[LoadMapData]) window.clearTimeout(Timers[LoadMapData]);
      if (_IsGetNextDataSet) 
        Timers[LoadMapData] = window.setTimeout(LoadMapData, 1000);
    }
  });
}

function IsDataLoadCompleted() {
  if (_FlightData.length < 1) return false;
  var DateOfLastItem = dtFromJSON(_FlightData[_FlightData.length - 1].FlightTime);
  var Now = new Date();
  //set to 2 mins before
  //convert to UTC time
  Now.setMinutes(Now.getMinutes() - 3 + Now.getTimezoneOffset());
  if (DateOfLastItem >= Now) return true;
  return false;
}

function InitilizeMapData() {
  _FlightStartMarker = new google.maps.Marker({
    position: {
      lat: _FlightData[0].Lat,
      lng: _FlightData[0].Lng,
    },
    icon: {
      url :'/images/flag-RpasStart.png',
      size: new google.maps.Size(25, 25),
      anchor: new google.maps.Point(4, 25)
    },
    map: map
  });

  var LastItem = _FlightData.length - 1;
  _FlightEndMarker = new google.maps.Marker({
    position: {
      lat: _FlightData[LastItem].Lat,
      lng: _FlightData[LastItem].Lng,
    },
    icon: {
      url: '/images/flag-RpasEnd.png',
      size: new google.maps.Size(25, 25),
      anchor: new google.maps.Point(4, 25)
    },
    map: map
  });

  _RPASIcon = new google.maps.Marker({
    position: {
      lat: _FlightData[LastItem].Lat,
      lng: _FlightData[LastItem].Lng,
    },
    icon: {
      url: '/images/Drone.png',
      size: new google.maps.Size(26, 26),
      anchor: new google.maps.Point(12, 12)
    },
    map: map
  });
}

function AddToTable(TheData) {
  if (TheData.length <= 0) return;
  var MaxRows = 10;
  var UL = $('#FlightDataScroll');
  var ExistingCount = $('#FlightDataScroll > LI').length;
  //Remove the number of Items from the list
  if (TheData.length > MaxRows) {
    UL.empty();
  } else if (TheData.length + ExistingCount > MaxRows) {
    var RowsToRemove = TheData.length + ExistingCount - MaxRows;
    if (RowsToRemove > 0) $('#FlightDataScroll > LI:nth-child(' + (MaxRows - RowsToRemove) + 'n)').nextAll('LI').remove();
  }

  var RowStartAt = TheData.length > MaxRows ? TheData.length - MaxRows : 0;

  for (var i = RowStartAt; i < TheData.length; i++) {
    var DataItem = TheData[i]
    var FlightTime = dtConvFromJSON(DataItem.FlightTime, true);

    var HTML =
      '<li>\n' +
      '<ul class="FlightDataRow">\n' +
      '<li>' + FlightTime.substring(FlightTime.length - 5) + '</li>\n' +
      '<li>' + DataItem.Lat.toFixed(4) + '</li>\n' +
      '<li>' + DataItem.Lng.toFixed(4) + '</li>\n' +
      '<li>' + toHour(DataItem.FlightDuration) + '</li>\n' +
      '<li>' + DataItem.Altitude.toFixed(1) + '</li>\n' +
      '<li>' + DataItem.Speed.toFixed(2) + '</li>\n' +
      '<li>' + DataItem.Distance.toFixed(2) + '</li>\n' +
      '<li>' + DataItem.Satellites.toFixed(0) + '</li>\n' +
      '<li>' + DataItem.Pich.toFixed(1) + '</li>\n' +
      '<li>' + DataItem.Roll.toFixed(1) + '</li>\n' +
      "</ul></li>";

    UL.prepend($(HTML));

  }
}

function AddDataToMap(TheData) {
  if (TheData.length <= 0) return;

  for (var i = 0; i < TheData.length; i++) {
    var LatLng = { lat: TheData[i].Lat, lng: TheData[i].Lng};
    _FlightCoordinates.push(LatLng);
    _FlightBoundBox.extend(LatLng);
  }

  var LastDataItem = TheData[TheData.length - 1];
  //When live, sometime distance is shown as null
  if (LastDataItem.Distance == null) {
    for (var i = _FlightData.length - 1; i > 0; i--) {
      if (_FlightData[i].Distance != null) {
        LastDataItem.Distance = _FlightData[i].Distance;
        break;
      }
    }
  }

  var LastPosition = { lat: LastDataItem.Lat, lng: LastDataItem.Lng };
  _FlightDataID = LastDataItem.FlightMapDataID;
  _FlightPath.setPath(_FlightCoordinates);
  _FlightEndMarker.setPosition(LastPosition);

  if (IsLive) {
    
  } else {
    map.fitBounds(_FlightBoundBox);
  }

  ShowFlightInformation(LastDataItem);
  map.setCenter(LastPosition);
  _RPASIcon.setPosition(LastPosition);
  
}


function ShowFlightInformation(TheDataItem) {
  $('#FlightInfo_Altitude').html(TheDataItem.Altitude.toFixed(1));
  $('#FlightInfo_FlightDuration').html(toHour(TheDataItem.FlightDuration));
  $('#FlightInfo_Speed').html(TheDataItem.Speed.toFixed(2));
  $('#FlightInfo_Distance').html(TheDataItem.Distance.toFixed(2));
  $('#FlightInfo_FlightDate').html(dtConvFromJSON(TheDataItem.FlightTime, true));
}




function toHour(Seconds) {
  var Hours = Math.floor(Seconds / 60);
  var RestSeconds = Seconds % 60;
  var sSeconds = '0' + RestSeconds;
  var sHour = '0' + Hours;
  return sHour.substring(sHour.length - 2) + ':' + sSeconds.substring(sSeconds.length - 2);
}