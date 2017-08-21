var map;
var DroneID = 0;
var isSetData = false;
var _VideoPlayTime = 0;
var _LoadedPlayTime = _VideoPlayTime;
var _PlayListPlayTime = 0;
var _PlayListIndex = 0;
var playerInstance = null;
var _FlightVideoPlayer;

$(document).ready(function () {
  InitVideoJs();
  initLoadTraffic();
});

function InitVideoJs() {

  if ($('#FlightVideo').length < 1) {
    return;
  } else if (_FlightVideos.length < 1) {
    return;
  }

  var player = videojs('FlightVideo');
  player.ready(function () {
    _FlightVideoPlayer = this;
  });

  if (DroneID > 0) {
    //live video
    player.playlist([{
      src: 'http://52.34.136.76/live/drone' + DroneID + '/index.m3u8',
      type: 'application/x-mpegURL'
    }]);
  } else {
    var VideoPlayList = GetVideoPlaylistJS();
    player.playlist(VideoPlayList);
  }

  // Play through the playlist automatically.
  player.playlist.autoadvance(0);

}


function GetVideoPlaylistJS() {
  var PlaySources = [];
  for (var i = 0; i < _FlightVideos.length; i++) {
    var VideoURL = "http://exponent-s3.s3.amazonaws.com/MP4/" + _FlightVideos[i].VideoURL.replace(".flv", ".mp4");
    var Source = {
      sources: [{
        src: VideoURL,
        type: 'video/mp4'
      }]
    };
    PlaySources.push(Source);
  }
  return PlaySources;
}

function fn_on_play(theData) {
  console.log(theData);
}

function fn_on_pause(theData) {
  console.log(theData);
}

function fn_on_time(theData) {
  //console.log(theData);
  _VideoPlayTime = VideoFiles[_PlayListIndex].StartTime + theData.position;
}

function fn_on_playlistitem(theData) {
  console.log(theData);
  _PlayListIndex = theData.index;
  //_PlayListPlayTime = VideoFiles[_PlayListIndex].StartTime;  
}

function initLoadTraffic() {

  //If the playtime is same as refreshed, call the timer again and return
  if (_LoadedPlayTime === _VideoPlayTime && DroneID === 0) {
    window.setTimeout(initLoadTraffic, 1000);
    return;
  }

  //Load the data for specific time
  _LoadedPlayTime = _VideoPlayTime;
  var URL = 'http://portalapi.exponent-ts.com/api/trafficmonitor' + (
    DroneID > 0 ?
      '/json/' + DroneID :
      '/flight/json/' + FlightID + '?Video=' + (_LoadedPlayTime * 1000));
  
  var AJAX = $.ajax({
    url: URL,
    type: 'GET',
    success: function (data) {
      ShowTrafficData(data);
    }
  }).done(function() {
    window.setTimeout(initLoadTraffic, 1000);
  });//$.ajax
}

function ShowTrafficData(TheData) {
    if (TheData === null) return;
    var d = new Date();
  $.each(TheData, function (key, value) {
      var SpanID = '#data-' + key.toLowerCase();
     
    if (!isSetData) $(SpanID).html('');
    switch (key) {
      case 'CreatedOn':
        d = new Date(value.replace('T', ' '));
        value = dFormat(d, false);     
      
        break;
      case 'FrameTime':
        value = TimeFromMs(value);
        break;
    }    
    setHtmlTo(SpanID, value);
    //isSetData = true;
  });

  
  if (!isSetData) $('#data-date').html('');
  setHtmlTo('#data-date', dFormat(d, false));
  isSetData = true;
}

function TimeFromMs(s) {
  // Pad to 2 or 3 digits, default is 2
  function pad(n, z) {
    z = z || 2;
    return ('00' + n).slice(-z);
  }

  var ms = s % 1000;
  s = (s - ms) / 1000;
  var secs = s % 60;
  s = (s - secs) / 60;
  var mins = s % 60;
  var hrs = (s - mins) / 60;

  return pad(hrs) + ':' + pad(mins) + ':' + pad(secs); //+ '.' + pad(ms, 3);
}

function setHtmlTo(SpanID, html) {
  var Span = $(SpanID);
  //hide element if already present
  var HideElem = Span.find('span.active');
  if (HideElem.length) {
    HideElem.fadeOut(300, function () {
      $(this).remove();
    });
  }
  var newElem = $('<span style="display:none">' + html + '</span>');
  Span.append(newElem);

  newElem.fadeIn(100, function () {
    $(this).addClass('active');
  });
}



/*



function InitilizeVideo() {
  var PlayerSource = {
    width: '100%',
    description: 'Click on play to start video.',
    mediaid: '50d509642b4f4ccb81ccdcc26ca594c6'
  };

  if (VideoFiles.length > 0) {
    if (VideoFiles[0].VideoURL.substr(0, 7).toLowerCase() === 'rtmp://') {
      PlayerSource['file'] = VideoFiles[0].VideoURL;
    } else {
      PlayerSource['playlist'] = getPlayList();
    }
  }

  playerInstance = jwplayer("map-video");
  playerInstance.setup(PlayerSource);

  playerInstance.on("play", fn_on_play);
  playerInstance.on("pause", fn_on_pause);
  playerInstance.on("time", fn_on_time);
  playerInstance.on("playlistItem", fn_on_playlistitem);
}

function getPlayList() {
  var PlayList = [];

  for (var i = 0; i < VideoFiles.length; i++) {
    var File = VideoFiles[i];
    var VideoURL = "http://exponent-s3.s3.amazonaws.com/MP4/" + File.VideoURL.replace(".flv", ".mp4");
    var Source = {
      sources: [ {
        file: VideoURL,
          type: "mp4"
        }
      ],
      title: File.Title
    };
    PlayList.push(Source);
  }
  return PlayList;
}

*/