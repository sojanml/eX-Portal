
$(document).ready(function () {

  // $('Table.report tbody').on("click", 'tr', function () {
  //   var data = qViewDataTable.row(this).data();
  //   var URL = '@Url.Action("Detail")/' + data.ID;
  //   top.location.href = URL;
  //console.log(data);
  // });

  $(document).on("click", function () {
    $('UL.qViewMenu').fadeOut();
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


  $('input.date-picker').datepicker({
    dateFormat: 'dd-M-yy',
  });

});


function processDelete(Link) {
  var TR = Link.closest('TR');

  var Request = $.ajax({
    method: "GET",
    url: Link.attr("href")
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
      var Exp = new RegExp(key, "ig");
      URL = URL.replace(Exp, data[key]);
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
  if($('#qViewTable_filter').find('.refresh').length <= 0) 
    $('#qViewTable_filter').append('<span class="refresh">&#xf021;</span>');

}