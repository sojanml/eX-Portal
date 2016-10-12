var AlertHideTimer = null;
var _isUTCFormat = false;

$(document).ready(function () {

  // $('Table.report tbody').on("click", 'tr', function () {
  //   var data = qViewDataTable.row(this).data();
  //   var URL = '@Url.Action("Detail")/' + data.ID;
  //   top.location.href = URL;
  //console.log(data);
  // });


  $(document).on("click", function () {
    $('UL.qViewMenu').fadeOut();
    $('#user-menu-items').slideUp();
  });

  $(document).on('click', 'span.refresh', function (e) {
    e.stopPropagation();
    qViewDataTable.ajax.reload(null, false);
  });

  var LastButton = null;
  $('table.report').on('click', 'img.button', function (e) {
    e.stopPropagation();
    if (LastButton) LastButton.parent().find('UL').remove();
    var Btn = $(this);
    LastButton = Btn;
    var data = qViewDataTable.row($(this).parents('tr')).data();
    //var pKey = data['_PKey'];
    //Btn.parent().find('UL').remove();
    Btn.parent().append(getqViewMenu(data));
    //alert('Key: ' + pKey)
  });

  $('#user-menu-link').on("click", function (e) {
    e.preventDefault();
    e.stopPropagation();
    $('#user-menu-items').slideToggle();
  });

  $('table.report').on('click', 'a._delete', function (e) {
    e.stopPropagation();
    e.preventDefault();
    $('UL.qViewMenu').fadeOut();
    var Link = $(this);
    var TR = Link.closest('TR');
    TR.addClass("to-delete");

    $("#delete-confirm").dialog({
      resizable: false,
      modal: true,
      buttons: {
        "Delete": function () {
          $(this).dialog("close");
          processDelete(Link);
        },
        Cancel: function () {
          $(this).dialog("close");
          TR.removeClass("to-delete");
        }
      }
    });

    //alert('Key: ' + pKey)
  });


  if (jQuery.ui) $('input.date-picker').datepicker({
    dateFormat: 'dd-M-yy',
    changeYear: true,
    changeMonth: true
  });

  checkTime();
  setAlertTimer();


});



function setAlertTimer() {
  window.setInterval(checkAlert, 10 * 1000);
  window.setInterval(setClockTime, 60 * 1000);
}

function checkAlert() {
  var URL = '/map/checkalert/' + LoginUserID;
  $.ajax({
    url: URL
  }).done(function (data) {
    //if (data != "") alertBox(data);
    if (data != "") alertLine(data);
  });
}

function checkTime() {
  var URL = '/Dashboard/getCurrentTime';
  var d = ''
  $.ajax({
    url: URL
  }).done(function (data) {
    var dt = new Date(data);
    d = data;
    ClockTime = dt;
    //  var t = dt.getHours() + ':' + dt.getMinutes();

    $("#Clock").html(fmtDtHeader(dt) + ' UTC');
  });
}


function setClockTime() {
  var dt = new Date(ClockTime);

  dt.setMinutes(dt.getMinutes() + 1, 0, 0);
  ClockTime = dt;
  //  var t = dt.getHours() + ':' + dt.getMinutes();
  $("#Clock").html(fmtDtHeader(dt) + ' UTC');
}


function alertLine(Message) {
  $('#alertLine').html("<ul>" + Message + "</ul>").slideDown();
  if (AlertHideTimer) window.clearTimeout(AlertHideTimer);
  AlertHideTimer = window.setTimeout(function () {
    $('#alertLine').slideUp();
  }, 10 * 1000);
}

function alertBox(Message) {
  $('#alert-message').html('<ul id="alert-drone">' + Message + '</ul>' +
  '<audio id="alert-audio" style="display:none" controls autoplay=true>\n' +
  '<source src="/audio/notify.mp3" type="audio/mpeg">\n' +
  'Your browser does not support the audio element.\n' +
  '</audio>\n');

  $("#alert-box").dialog({
    resizable: false,
    height: 400,
    minWidth: 600,
    modal: true,
    buttons: {
      Cancel: function () {
        $(this).dialog("close");
      }
    }
  });

  var Alert = document.getElementById('alert-audio');
  Alert.autoplay = true;
  Alert.load();

}


function processDelete(Link) {
  var TR = Link.closest('TR');

  var Request = $.ajax({
    method: "GET",
    url: Link.attr("href"),
    dataType: 'json'
  });
  Request.done(function (data) {
    if (data.status == "OK") {
      TR.slideUp();
    } else {
      Alert(data.message);
      TR.removeClass("to-delete");
    }
  });
  Request.fail(function (jqXHR, textStatus) {
    Alert("Connection to server failed.");
    TR.removeClass("to-delete");
  });
}

function Alert(Text) {
  $('#alert-message').html(Text);

  $("#alert-box").dialog({
    resizable: false,
    modal: true,
    buttons: {
      Close: function () {
        $(this).dialog("close");
      }
    }
  });

}

function getqViewMenu(data) {
  var List = $('<UL class="qViewMenu"></UL>');
  for (var i = 0; i < qViewMenu.length; i++) {
    var Menu = qViewMenu[i];
    var URL = Menu.url;
    //replace all the variables in query string
    for (var key in data) {
      var KeyResult = data[key];
      var RegPattern = '(^|=|/)(' + key + ')($|&|/)';
      var Exp = new RegExp(RegPattern, "i");
      URL = URL.replace(Exp, '$1' + KeyResult + '$3');
    }
    var LI = $('<LI><A class="' + Menu.class + '" href="' + URL + '">' + Menu.caption + '</a></LI>');
    List.append(LI);
  };
  return List;
}

function _fnFooterCallback(nFoot, aData, iStart, iEnd, aiDisplay) {

}

function _fnDrawCallback() {
  $('#qViewTable_paginate').append('<span class="refresh">&#xf021;</span>');
  if ($('#qViewTable_filter').find('.refresh').length <= 0)
    $('#qViewTable_filter').append('<span class="refresh">&#xf021;</span>');

}



function fmtDtHeader(date) {
  if (date instanceof Date) {

  } else {
    return 'Invalid';
  }
  var day = date.getDate();
  var hours = _isUTCFormat ? date.getUTCHours() : date.getHours();
  var minutes = _isUTCFormat ? date.getUTCMinutes() : date.getMinutes();
  var seconds = _isUTCFormat ? date.getUTCSeconds() : date.getSeconds();
  var Months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  //var ampm = hours >= 12 ? 'pm' : 'am';
  //hours = hours % 12;
  //hours = hours ? hours : 12; // the hour '0' should be '12'
  seconds = seconds < 10 ? '0' + seconds : seconds;
  hours = hours < 10 ? '0' + hours : hours;
  minutes = minutes < 10 ? '0' + minutes : minutes;
  day = day < 10 ? '0' + day : day;
  var strTime = hours + ':' + minutes;
  var strDate = day + "-" + Months[date.getMonth()] + "-" + date.getFullYear();
  return strDate + " " + strTime;
}

/*
var closing = true;
$(function () {
    $("a,input[type=submit]").click(function () { closing = false; });
    $(window).unload(function () {
        if (closing) {
            jQuery.ajax({ url: "/User/Logout", async: false });
        }
    });
});

*/