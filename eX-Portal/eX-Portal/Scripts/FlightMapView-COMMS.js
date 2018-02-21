var COMMS = function () {
  var _GetMessageTimer = null;
  var _MessageID = 0;
  var _PostURL = "/COMMS/CreateMessage/";

  var _GetMessages = function () {
    var _URL = "/COMMS/GetPilotMessages/?MessageID=" + _MessageID  + "&FlightID=" + FlightInfo.FlightID;
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
      var ClassName = "tome";
      if (Message.FromID == FlightInfo.UserID) {
        ClassName = "fromme";
        Message.FromUser = "You";
      }
      var ID = 'message-' + Message.MessageID;
      var LI = $('<LI id="' + ID + '" class="' + ClassName + '"></LI>')
        .append(
        '<div class="message">' +
        '<div class="name">' + Message.FromUser + '</div>' + 
        '<div class="content">' +  Message.Message + '</div>' +
        '</div>' +
        '<div class="date">' + Util.FmtTime(Message.CreatedOn) + '</div>'
        );
      $('#ComsList').append(LI);
      document.getElementById(ID).scrollIntoView();
      _MessageID = Message.MessageID;
    }
  };

  var _Post = function (e) {
    e.preventDefault();
   

    if (_GetMessageTimer) window.clearTimeout(_GetMessageTimer);
    var TheData =
      'Message=' + $('#ComsMessage').val() +
      '&FlightID=' + FlightInfo.FlightID;

    $.ajax({
      method: "POST",
      data: TheData,
      url: _PostURL,
      dataType: "json",
      success: function (data) {
        $('#ComsMessage').val("");
      },
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
  var _setPostUrl = function (url) {
      _PostURL = url;

  };
  return {
      Init: _Init,
      SetPostUrl: _setPostUrl
  };

}();
