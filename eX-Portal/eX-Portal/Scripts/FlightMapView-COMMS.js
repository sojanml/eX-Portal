var COMMS = function () {
  var _GetMessageTimer = null;
  var _FilterDate = new Date(1980,1,1);

  var _GetMessages = function () {
    var uDate = Util.toString(_FilterDate);
    var _URL = "/COMMS/GetPilotMessages/?FilterDate=" + uDate  + "&FlightID=" + FlightInfo.FlightID;
    $.ajax({
      type: "GET",
      url: _URL,
      contentType: "application/json;charset=utf-8",
      dataType: "json",
      success: _MessagesLoaded,
      failure: function (msg) {
        alert('Live Drone Position Error' + msg);
      },
      complete: function (msg) {
        if (_GetMessageTimer) window.clearTimeout(_GetMessageTimer);
        _GetMessageTimer = window.setTimeout(_GetMessages, 2000);
      }
    });
  };

  var _MessagesLoaded = function (Data) {
    for (var i = 0; i < Data.length; i++) {
      var Message = Data[i];
      Message.CreatedOn = Util.toDateTime(Message.CreatedOn);
      var LI = $('<LI></LI>')
        .append(
        '<div class="message">' + Message.Message + '</div>' +
        '<div class="date">' + Util.FmtTime(Message.CreatedOn) + '</div>'
        );
      $('#ComsList').append(LI);
      _FilterDate = Message.CreatedOn;
    }
  };

  var _Post = function (e) {
    e.preventDefault();
    var _URL = "/COMMS/CreateMessage/";

    if (_GetMessageTimer) window.clearTimeout(_GetMessageTimer);
    var TheData =
      'Message=' + $('#ComsMessage').val() +
      '&FlightID=' + FlightInfo.FlightID;

    $.ajax({
      method: "POST",
      data: TheData,
      url: _URL,
      dataType: "json",
      success: _MessagesLoaded,
      failure: function (msg) {
        alert('Live Drone Position Error' + msg);
      },
      complete: function (msg) {
        if (_GetMessageTimer) window.clearTimeout(_GetMessageTimer);
        _GetMessageTimer = window.setTimeout(_GetMessages, 20);
      }
    });

  }

  var _Init = function () {
    $('#ComSubmitForm').on("submit", _Post);

    if (_GetMessageTimer) window.clearTimeout(_GetMessageTimer);
    _GetMessageTimer = window.setTimeout(_GetMessages, 10);
  };

  return {
    Init: _Init
  };

}();
