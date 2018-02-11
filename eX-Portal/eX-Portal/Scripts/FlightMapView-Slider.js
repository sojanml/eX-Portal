var FlightMapSlider = function () {
  var _slider = null;
  var _OnSlideEvent = null;
  var _SliderData = null;
  var _SliderActiveTime = null;

  var _init = function () {
    _slider = $("#FlightSliderBar").slider({
      slide: _OnSlide
    })
  };

  var _OnSlide = function (event, ui) {
    //console.log(ui.value);
    var NowTime = _SliderData[ui.value].FlightTime;
    $('div#FlightSliderCenter').html(_FmtTime(NowTime));
    if (_OnSlideEvent !== null) _OnSlideEvent(event, ui)
  }

  var _SetOnSlide = function (EventListner) {
    _OnSlideEvent = EventListner;
  };

  var _setData = function (_TheData) {
    var DataFirst = _TheData[0];
    var DataLast = _TheData[_TheData.length - 1];
    var StartTime = _FmtTime(DataFirst.FlightDateTime);
    var EndTime = _FmtTime(DataLast.FlightDateTime);
    var Max = _TheData.length - 1;

    if (_SliderActiveTime == null) _SliderActiveTime = StartTime;

    _slider.slider("option", "max", Max);
    $('#FlightSliderStart').html(StartTime);
    $('#FlightSliderEnd').html(EndTime);
    $('div#FlightSliderCenter').html(_SliderActiveTime);
    _SliderData = _TheData;
  };

  var _setIndex = function (Index) {
    var NowTime = _SliderData[Index].FlightTime;
    $('div#FlightSliderCenter').html(_FmtTime(NowTime));
    _slider.slider('value', Index);
  }

  var _FmtTime = function (sNetDate) {
    var nDate = new Date();
    if (sNetDate instanceof Date) {
      nDate = sNetDate;
    } else if (sNetDate !== null) {
      var r = /\/Date\(([0-9]+)\)\//i
      var matches = sNetDate.match(r);
      if (matches.length === 2) {
        nDate = new Date(parseInt(matches[1]));
      }
    }
    //Convert date to UTC
    //nDate.setMinutes(nDate.getMinutes() + nDate.getTimezoneOffset());
    return _pad(nDate.getHours()) + ':' + _pad(nDate.getMinutes()) + ':' + _pad(nDate.getSeconds());
  };


  var _pad = function (Num) {
    if (Num >= 10) return Num;
    return '0' + Num;
  };

  return {
    Init: _init,
    SetData: _setData,
    SetOnSlide: _SetOnSlide,
    SetIndex: _setIndex
  };
}();
