$(document).ready(function () {
  FlightMapData.Init();
  FlightMapSlider.Init();
  FlightMapSlider.SetOnSlide(FlightMapData.OnSlide);
  COMMS.Init();
  ``

})

var FlightMapData = function () {
  var _AttitudeMeter = null;
  var _FlightMapDataID = 0;
  var _OnMapLoadDataTimer = null;
  var _IsMapInit = false;
  var _MapPlayTimer = null;
  var _FullData = [];
  var _DataAddedIndex = 0;
  var _IsPaused = false;


  var PlayIndex = -1;
  var PlayDateTime = null;
  var PlayPositionTime = null;
  var _MapPlayInterval = 1000;

  var _setScreenTextToIndex = function (Index) {
    var Data = _FullData[Index];
    var Pitch = 0;
    var Roll = 0;
    Pitch = Data.Pich;
    Roll = Data.Roll;
    _AnimateAmountTo($('#AttitudePitch').find('span.Number'), Pitch, 0);
    _AnimateAmountTo($('#AttitudeRoll').find('span.Number'), Roll, 0);
    _AnimateTimeTo($('#FlightInfo-Duration'), Data.FlightDuration);
    _AnimateAmountTo($('#FlightInfo-Speed'), Data.Speed, 2);
    _AnimateAmountTo($('#FlightInfo-Altitude'), Data.Altitude, 0);
    _AnimateAmountTo($('#FlightInfo-Distance'), Data.Distance, 0);

    _AttitudeMeter.setRoll(Roll);
    _AttitudeMeter.setPitch(Pitch);
  };

  var _LoadChartData = function (OnSuccess) {
    $.ajax({
      type: "GET",
      url: '/FlightMap/ChartSummaryData/' + FlightInfo.FlightID,
      contentType: "application/json;charset=utf-8",
      dataType: "json",
      success: function (Data) {
        OnSuccess(Data);
      },
      failure: function (msg) {
        alert('Live Drone Position Error' + msg);
      }
    });
  };

  var _LoadMapData = function (OnSuccess) {
    $.ajax({
      type: "GET",
      url: '/FlightMap/Data/' + FlightInfo.FlightID + '?FlightMapDataID=' + _FlightMapDataID,
      contentType: "application/json;charset=utf-8",
      dataType: "json",
      success: function (Data) {
        OnSuccess(Data);
      },
      failure: function (msg) {
        alert('Error loading map position data' + msg);
      }
    });
  };

  var _LoadAttitudeMeter = function (Data) {
    var Pitch = 0;
    var Roll = 0;
    if (Data.length > 0) {
      Pitch = parseInt(Data[0].Pich,0);
      Roll = parseInt(Data[0].Roll,0);
    }
    _AttitudeMeter = $.flightIndicator('#attitude', 'attitude', {
      roll: Roll,
      pitch: Pitch,
      size: 200,
      showBox: false,
      img_directory: '/Scripts/jQueryFlightIndicators/img/'
    });
  };


  var _onChartDataLoad = function (Data) {
    FlightCharts.Init(Data);
  };

  var _onMapDataLoad = function (Data) {
    var FirstData = Data.Data[0];
    var lat = 25;
    var lng = 55;
    if (Data.Data.length > 0) {
      lat = FirstData.Lat;
      lng = FirstData.Lng;
    } 
    //If the map is not initlized, do it
    if (!_IsMapInit) {
      _LoadAttitudeMeter(Data.Data);
      FlightMap.Init(FirstData);
      StatusIcons.Init(lat, lng);
    }
    _IsMapInit = true;
      
    //if there is no data, then do not add anything
    if (Data.Data.length <= 0) {
      FlightMap.AutoZoom();
      FlightMapSlider.SetData(_FullData);
      FlightMapTable.SetData(_FullData);
      FlightMapView3D.AddData(_FullData);

      PlayIndex = 0;
      PlayPositionTime = _FullData[PlayIndex].FlightDateTime;
      _setScreenTextToIndex(PlayIndex);

      if (FlightInfo.IsLive) {
        _InitilizeLive();
      }
      return;
    } 

    //Convert Date time to DateTime Object
    for (var i = 0; i < Data.Data.length; i++) {
      Data.Data[i].FlightDateTime = Util.toDateTime(Data.Data[i].FlightTime);
    }

    //add the loaded data to map
    FlightMap.AddMapData(Data.Data);
    FlightCharts.AddChartData(Data.Data);
    FlightSummary.AddData(Data.Data);
    _FullData = _FullData.concat(Data.Data);
    //Update the last flight ID
    if (_FullData.length > 0) _FlightMapDataID = _FullData[_FullData.length - 1].FlightMapDataID;


    //Load next set of data
    if (_OnMapLoadDataTimer) window.clearTimeout(_OnMapLoadDataTimer);
    _LoadMapData(_onMapDataLoad);    
  };

  var _onMapDataLoadLive = function (SourceData) {
    var Data = SourceData.Data;
    if (Data.length < 1) {
      _LoadNextLiveData();
      return;
    }

    //Convert Date time to DateTime Object
    for (var i = 0; i < Data.length; i++) {
      Data[i].FlightDateTime = Util.toDateTime(Data[i].FlightTime);
    }

    FlightMap.AddMapData(Data);
    FlightCharts.AddChartData(Data);
    FlightMapView3D.AddDataLive(Data);

    _FullData = _FullData.concat(Data);
    FlightMapSlider.SetData(_FullData);
    FlightMapTable.SetData(_FullData);

    PlayIndex = _FullData.length - 1;
    PlayPositionTime = _FullData[PlayIndex].FlightDateTime;

    _setScreenTextToIndex(PlayIndex);
    FlightMapSlider.SetIndex(PlayIndex);
    FlightMap.MoveToIndex(PlayIndex);
    _FlightMapDataID = _FullData[PlayIndex].FlightMapDataID;

    _LoadNextLiveData();

  }


  var _InitilizeLive = function () {
    PlayIndex = _FullData.length - 1;
    PlayPositionTime = _FullData[PlayIndex].FlightDateTime;
    FlightMapView3D.Play(PlayPositionTime);

    if (_OnMapLoadDataTimer) window.clearTimeout(_OnMapLoadDataTimer);
    _LoadMapData(_onMapDataLoadLive);
  }

  var _LoadNextLiveData = function () {
    if (_OnMapLoadDataTimer) window.clearTimeout(_OnMapLoadDataTimer);
    _OnMapLoadDataTimer = window.setTimeout(function () {
      _LoadMapData(_onMapDataLoadLive)
    }, 1000);
  }

  var _OnSlideTimer = null;
  var _OnSlide = function (event, ui) {
    _IsPaused = true;
    //console.log("Value: " + ui.value);
    if (_OnSlideTimer) window.clearTimeout(_OnSlideTimer);
    _OnSlideTimer = window.setTimeout(function () {
      _IsPaused = false;
      var index = ui.value;
      //Set date time to initilize
      var DataValue = _FullData[index];
      PlayIndex = index;
      PlayPositionTime = _FullData[PlayIndex].FlightDateTime;
      FlightMapView3D.MoveTo(PlayPositionTime);
      _MoveToIndex(PlayIndex);
      FlightMap.ClearADSB();
    }, 100)
  };



  var _AnimateAmountTo = function (Element, Amount, FixedPosition) {
    var Now = parseFloat(Element.text());
    if (isNaN(Now)) Now = 0;
    Element
      .prop('number', Now)
      .animateNumber({
        number: Amount,
        numberStep: function (now, tween) {
          $(tween.elem).text(now.toFixed(FixedPosition));
        }
      },
      200
      );
  };

  var _AnimateTimeTo = function (Element, ToSeconds) {
    var hms = Element.text();   // your input string
    var a = hms.split(':'); // split it at the colons
    // minutes are worth 60 seconds. Hours are worth 60 minutes.
    var seconds = (+a[0]) * 60 * 60 + (+a[1]) * 60 + (+a[2]); 

    Element
      .prop('number', seconds)
      .animateNumber({
        number: ToSeconds,
        numberStep: function (now, tween) {
          $(tween.elem).text(Util.toTime(now));
        }
      },
      200
      );
  };


  var _fnBtnPlayClick = function (e) {
    $('#btnPlay').css({ 'display': 'none' });
    $('#btnPause').css({ 'display': 'inline-block' });

    if (_MapPlayTimer) window.clearInterval(_MapPlayTimer);
    _MapPlayTimer = window.setInterval(_fnBtnPlayOnTimer, _MapPlayInterval);
    FlightMapView3D.Play(PlayPositionTime);
  };

  var _fnBtnPauseClick = function (e) {
    $('#btnPlay').css({ 'display': 'inline-block' });
    $('#btnPause').css({ 'display': 'none' });
    if (_MapPlayTimer) window.clearInterval(_MapPlayTimer);
    _MapPlayTimer = null;
    FlightMapView3D.Pause(PlayPositionTime);
  };

  var _fnChangeSpeedClick = function () {
    var btn = $(this).text();
    switch (btn) {
      case '1X':
        _MapPlayInterval = 1000;
        FlightMapView3D.setSpeed(1);
        break;
      case '2X':
        _MapPlayInterval = 500;
        FlightMapView3D.setSpeed(2);
        break;
      case '4X':
        _MapPlayInterval = 250;
        FlightMapView3D.setSpeed(4);
        break;        
    }
    if (_MapPlayTimer) {
      window.clearInterval(_MapPlayTimer);
      _MapPlayTimer = window.setInterval(_fnBtnPlayOnTimer, _MapPlayInterval);
    }

    $('div.round-button.active').removeClass('active');
    $(this).addClass('active');
  };



  var _fnBtnPlayOnTimer = function () {
    if (_IsPaused) return;
    if (PlayIndex >= _FullData.length - 1) return;

    //Check the time is ready to play
    PlayPositionTime.setSeconds(PlayPositionTime.getSeconds() + 1);
    var NextIndexTime = _FullData[PlayIndex + 1].FlightDateTime;
    $('#FlightSliderCenter').html(Util.FmtTime(PlayPositionTime));
    if (PlayPositionTime < NextIndexTime) return;

    PlayIndex++;
    FlightMapSlider.SetIndex(PlayIndex);    
    _MoveToIndex(PlayIndex);
  };

  var _MoveToIndex = function (index) {
    _setScreenTextToIndex(PlayIndex);
    FlightMap.MoveToIndex(PlayIndex);
    FlightCharts.MoveToIndex(index);
    FlightMapTable.AddRow(_FullData, index);
  }

  var _fn2DClick = function (e) {
    $('#btn2D').hide();
    $('#btn3D').show();
    $('#GoogleMap').show();
    $('#CesiumMap').hide();
  }

  var _fn3DClick = function (e) {
    $('#btn2D').show();
    $('#btn3D').hide();
    $('#GoogleMap').hide();
    $('#CesiumMap').show();
    FlightMapView3D.Init();
    if (_MapPlayTimer != null) {
      FlightMapView3D.MoveTo(PlayPositionTime);
    }
    if (FlightInfo.IsLive) FlightMapView3D.Play(PlayPositionTime);

  }

  var _init = function () {
    _LoadChartData(_onChartDataLoad);
    _LoadMapData(_onMapDataLoad)
    $('#btnPlay').on("click", _fnBtnPlayClick);
    $('#btnPause').on("click", _fnBtnPauseClick);
    $('div.round-button').on("click", _fnChangeSpeedClick);
    $('#btn2D').on("click", _fn2DClick);
    $('#btn3D').on("click", _fn3DClick);
  };

  

  return {
    Init: _init,
    OnSlide: _OnSlide
  };
}();


var FlightMapTable = function () {

  var _AddEndRows = function (FullData) {
    $('ul#FlightMapTableData').empty();
    var StartIndex = FullData.length - 12;
    if (StartIndex < 0) StartIndex = 0;
    for (var i = StartIndex; i < FullData.length; i++) {
      _AddRow(FullData, i);
    }
  }

  var _AddRow = function (FullData, Index) {
    var Data = FullData[Index];
    if ($('ul#FlightMapTableData').children().length > 12) {
      $('ul#FlightMapTableData').find("li:last-child").remove();
    }
    $('ul#FlightMapTableData').prepend(_getRow(Data));
  };

  var _getRow = function (Data) {
    var HTML = $(
      '<li>\n' +
      '  <div class="col1">' + Util.FmtTime(Data.FlightDateTime) + '</div>\n' +
      '  <div class="col2">' + Data.Lat.toFixed(6) + '</div>\n' +
      '  <div class="col3">' + Data.Lng.toFixed(6) + '</div>\n' +
      '  <div class="col4">' + Data.Altitude.toFixed(1) + '</div>\n' +
      '  <div class="col5">' + Data.Speed.toFixed(2) + '</div>\n' +
      '  <div class="col6">' + Data.Pich.toFixed(0) + '</div>\n' +
      '  <div class="col7">' + Data.Roll.toFixed(0) + '</div>\n' +
      '  <div class="col8">' + Data.Heading.toFixed(0) + '</div>\n' +
      '  <div class="col9  hide-if-requred">' + Data.Distance.toFixed(0) + '</div>\n' +
      '</li>\n'
    );
    return HTML;
  }



  return {
    AddRow: _AddRow,
    SetData: _AddEndRows
  };
}();


var StatusIcons = function () {
  var _lat = null;
  var _lng = null;

  var _Init = function (lat, lng) {
    _lat = lat;
    _lng = lng;

    $('#FlightStatusBar > ul > li > div').on("click", function () {
      var elem = $(this);
      _setInfo(elem)
    });
    $('#MetarRefresh').on("click", function (e) {
      e.preventDefault();
      _LoadMetarInfo();
    });
    _LoadMetarInfo();
  };

  var _setInfo = function (elem) {
    var data = elem.attr("data-for");
    $('#FlightStatusBar > ul > li > div').removeClass("active");
    elem.addClass("active");
    $('#FlightStatusIconInfo > span').hide();
    $('#FlightStatusIcon' + data + 'Text').fadeIn();
  }

  var _LoadMetarInfo = function () {
    $.ajax({
      type: 'GET',
      url: 'https://api.checkwx.com/metar/lat/' + _lat + '/lon/' + _lng +'/decoded',
      headers: { 'X-API-Key': '57786cac657c977ca5aa70a898' },
      dataType: 'json',
      success: function (result) {
        $('#FlightStatusIconWindText').html('Wind Speed: ' + (result.data[0].wind.speed_mps * 3.6) + ' Km/H (kmph)');
        $('#FlightStatusIconTempText').html('Temperature: ' + result.data[0].temperature.celsius + '&deg;C');
        $('#FlightStatusIconAirlineText').html('Airport: <b>' + result.data[0].icao + '</b> (' + result.data[0].name + ')');
        $('#MetarInfo').html(result.data[0].raw_text);
      },
      error: function (error) {
        // CallBack(error);
      }
    });
  }

  return {
    Init: _Init
  };
}();


var Util = function () {
  this._FmtTime = function (nDate) {
    return _pad(nDate.getHours()) + ':' + _pad(nDate.getMinutes()) + ':' + _pad(nDate.getSeconds());
  };


  var _toDateTime = function (sNetDate) {
    var nDate = new Date(); 
    if (sNetDate instanceof Date && isFinite(sNetDate)) {
      nDate = sNetDate;
    } else if (sNetDate !== null) {
      var r = /\/Date\(([0-9]+)\)\//i
      var matches = sNetDate.match(r);
      if (matches.length === 2) {
        var TimeInSec = parseInt(matches[1]);
        nDate = new Date(TimeInSec);
      }
    }
    //Convert date to UTC
    //nDate.setMinutes(nDate.getMinutes() + nDate.getTimezoneOffset());
    return nDate;
  };


  var _pad = function (Num) {
    if (Num >= 10) return Num;
    return '0' + Num;
  };

  var _toDateString = function (nDate) {
    var data = 
    _pad(nDate.getFullYear()) + '-' + _pad(nDate.getMonth() + 1) + '-' + _pad(nDate.getDate()) + " " +
      _pad(nDate.getHours()) + ':' + _pad(nDate.getMinutes()) + ':' + _pad(nDate.getSeconds()) + '.' + nDate.getMilliseconds();

    return data;
  };


  var _toTime = function (time) {
    var sec_num = parseInt(time, 10); // don't forget the second param
    var hours = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
    var seconds = sec_num - (hours * 3600) - (minutes * 60);

    if (hours < 10) { hours = "0" + hours; }
    if (minutes < 10) { minutes = "0" + minutes; }
    if (seconds < 10) { seconds = "0" + seconds; }
    return hours + ':' + minutes + ':' + seconds;
  };


  var rad = function (x) {
    return x * Math.PI / 180;
  };

  var _getDistance = function (p1, p2) {
    var R = 6378137; // Earth’s mean radius in meter
    var dLat = rad(p2.lat - p1.lat);
    var dLong = rad(p2.lng - p1.lng);
    var a = Math.sin(dLat / 2) * Math.sin(dLat / 2) +
      Math.cos(rad(p1.lat)) * Math.cos(rad(p2.lat)) *
      Math.sin(dLong / 2) * Math.sin(dLong / 2);
    var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
    var d = R * c;
    return d; // returns the distance in meter
  };

  return {
    FmtTime: _FmtTime,
    toDateTime: _toDateTime,
    toTime: _toTime,
    toString: _toDateString,
    getDistance: _getDistance
  };
}();

var FlightSummary = function () {
  var _satellite = new Summary();
  var _altitude = new Summary();
  var _addData = function (data) {
    for (var i = 0; i < data.length; i++) {
      _satellite.Add(data[i].Satellites);
      _altitude.Add(data[i].Altitude);
    }

    $('#info-Altitude-min').html(_altitude.Min());
    $('#info-Altitude-max').html(_altitude.Max());
    $('#info-Satellite-min').html(_satellite.Min());
    $('#info-Satellite-max').html(_satellite.Max());
    $('#MapInfoContentAltitude').html(_altitude.Max());
    
  };

  return {
    AddData: _addData
  };
  
}();

function Summary () {
  var _min = 0;
  var _max = 0;
  var _AddData = function (num) {
    if (num < _min) _min = num;
    if (num > _max) _max = num;
  };
  return {
    Add: _AddData,
    Min: function () { return _min },
    Max: function () { return _max }
  };
};