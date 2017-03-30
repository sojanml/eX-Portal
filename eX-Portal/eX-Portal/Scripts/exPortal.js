var qViewRowIndex = -1;

$(document).ready(function () {

  $(document).on("click", function () {
    $('table.report UL.qViewMenu').fadeOut(200, function () { $(this).remove(); });
    $('#user-menu-items').slideUp();
    qViewRowIndex = -1;
    $('table.report tbody tr.active').removeClass('active');
  });

  $(document).on('click', 'span.refresh', function (e) {
    e.stopPropagation();
    qViewDataTable.ajax.reload(null, false);
  });


  $('table.report').on('click', 'tbody tr', function (e) {
    e.stopPropagation();
    var TR = $(this);    
    var thisRowIndex = TR[0]._DT_RowIndex;
    if (qViewRowIndex === thisRowIndex) return;

    $('table.report tbody tr.active').removeClass('active');
    $('table.report UL.qViewMenu').fadeOut(200, function () { $(this).remove();});

    qViewRowIndex = thisRowIndex;
    TR.addClass('active');
    var data = qViewDataTable.row(TR).data();
    var UL = $(getqViewMenu(data));
    UL.attr("id", "qViewMenu" + thisRowIndex);
    UL.css('display', 'none');
    TR.find('td.menu').append(UL);
    UL.fadeIn(100);
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

});



function getqViewMenu(data) {
  var List = $('<UL class="qViewMenu"></UL>');
  for (var i = 0; i < qViewMenu.length; i++) {
    var Menu = qViewMenu[i];
    var URL = Menu.url;
    //replace all the variables in query string
    for (var key in data) {
      var KeyResult = data[key];
      var RegPattern = '(^|=|/)(' + key + ')($|&|\\?|\/)';
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
  qViewRowIndex = -1;
}



function fmtDtHeader(date) {
  if (!(date instanceof Date)) {
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

