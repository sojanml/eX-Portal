$(document).ready(function () {
  FlightMapData.Init();
  FlightMapSlider.Init();
  FlightMapSlider.SetOnSlide(FlightMapData.OnSlide);
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
      FlightMap.Init({ lat: lat, lng: lng });
    }
    _IsMapInit = true;
      
    //if there is no data, then do not add anything
    if (Data.Data.length <= 0) {
      FlightMap.AutoZoom();
      FlightMapSlider.SetData(_FullData);
      FlightMapTable.SetData(_FullData);

      //Set date time to initilize
      PlayPositionTime = _FullData[0].FlightDateTime;
      PlayIndex = 0;
      _setScreenTextToIndex(0);
      return;
    }

    //Convert Date time to DateTime Object
    for (var i = 0; i < Data.Data.length; i++) {
      Data.Data[i].FlightDateTime = toDateTime(Data.Data[i].FlightTime);
    }

    //add the loaded data to map
    FlightMap.AddMapData(Data.Data);
    FlightCharts.AddChartData(Data.Data);
    _FullData = _FullData.concat(Data.Data);

    //Load next set of data
    if (_OnMapLoadDataTimer) window.clearTimeout(_OnMapLoadDataTimer);
    _FlightMapDataID = Data.Data[Data.Data.length - 1].FlightMapDataID;
    _LoadMapData(_onMapDataLoad);
  };

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
      _MoveToIndex(PlayIndex);
    }, 100)
  };

  var toDateTime = function (sNetDate) {
    var nDate = new Date();
    if (sNetDate !== null) {
      var r = /\/Date\(([0-9]+)\)\//i
      var matches = sNetDate.match(r);
      if (matches.length === 2) {
        nDate = new Date(parseInt(matches[1]));
      }
    }
    //Convert date to UTC
    //nDate.setMinutes(nDate.getMinutes() + nDate.getTimezoneOffset());
    return nDate;
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
          $(tween.elem).text(toTime(now));
        }
      },
      200
      );
  };

  var toTime = function (time) {
    var sec_num = parseInt(time, 10); // don't forget the second param
    var hours = Math.floor(sec_num / 3600);
    var minutes = Math.floor((sec_num - (hours * 3600)) / 60);
    var seconds = sec_num - (hours * 3600) - (minutes * 60);

    if (hours < 10) { hours = "0" + hours; }
    if (minutes < 10) { minutes = "0" + minutes; }
    if (seconds < 10) { seconds = "0" + seconds; }
    return hours + ':' + minutes + ':' + seconds;
  };

  var _fnBtnPlayClick = function (e) {
    $('#btnPlay').css({ 'display': 'none' });
    $('#btnPause').css({ 'display': 'inline-block' });

    if (_MapPlayTimer) window.clearInterval(_MapPlayTimer);
    _MapPlayTimer = window.setInterval(_fnBtnPlayOnTimer, _MapPlayInterval);
  };

  var _fnBtnPauseClick = function (e) {
    $('#btnPlay').css({ 'display': 'inline-block' });
    $('#btnPause').css({ 'display': 'none' });
    if (_MapPlayTimer) window.clearInterval(_MapPlayTimer);
    _MapPlayTimer = null;
  };

  var _fnChangeSpeedClick = function () {
    var btn = $(this).text();
    switch (btn) {
      case '1X':
        _MapPlayInterval = 1000;
        break;
      case '2X':
        _MapPlayInterval = 500;
        break;
      case '4X':
        _MapPlayInterval = 250;
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


  var _init = function () {
    _LoadChartData(_onChartDataLoad);
    _LoadMapData(_onMapDataLoad)
    $('#btnPlay').on("click", _fnBtnPlayClick);
    $('#btnPause').on("click", _fnBtnPauseClick);
    $('div.round-button').on("click", _fnChangeSpeedClick);
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
      '  <div class="col1">' + _FmtTime(Data.FlightDateTime) + '</div>\n' +
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



  var _FmtTime = function (nDate) {
    return _pad(nDate.getHours()) + ':' + _pad(nDate.getMinutes()) + ':' + _pad(nDate.getSeconds());
  };

  var _pad = function (Num) {
    if (Num >= 10) return Num;
    return '0' + Num;
  };

  return {
    AddRow: _AddRow,
    SetData: _AddEndRows
  };
}();

var Util = function () {
  this._FmtTime = function (nDate) {
    return _pad(nDate.getHours()) + ':' + _pad(nDate.getMinutes()) + ':' + _pad(nDate.getSeconds());
  };

  var _pad = function (Num) {
    if (Num >= 10) return Num;
    return '0' + Num;
  };

  return {
    FmtTime: _FmtTime
  };
}();
