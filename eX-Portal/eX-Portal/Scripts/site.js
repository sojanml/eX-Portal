
    $(document).ready(function () {

     // $('Table.report tbody').on("click", 'tr', function () {
     //   var data = qViewDataTable.row(this).data();
     //   var URL = '@Url.Action("Detail")/' + data.ID;
     //   top.location.href = URL;
        //console.log(data);
     // });

      $(document).on("click", function() {
        $('UL.qViewMenu').fadeOut();
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


      $('input.date-picker').datepicker({
        dateFormat: 'dd-M-yy',
      });

    });


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
        var LI = $('<LI><A href="' + URL + '">' + Menu.caption + '</a></LI>');
        List.append(LI);
      };
      return List;
    }