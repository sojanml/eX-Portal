$(document).ready(function () {

  $('body').on('focus', ".date-picker", function () {
    $(this).datepicker({
      dateFormat: 'dd-M-yy',
      changeYear: true,
      changeMonth: true
    });
  });
  $('body').on('focus', ".time-picker", function () {
    $(this).timepicker();
  });
  $(document).on("click", "div.btnAddNoc", function () {
    NOC.AddNoc($(this));
  });

  $(document).on("click", "div.btnSetCoordinates", function () {
    NOC.MoveGoogleMapTo($(this));
  });

  $(document).on("click", "div.btnDeleteNoc", function () {
    NOC.DeleteNocRow($(this));
  });
  

  $(document).on("change", "select.list-PilotID", Ajax.OnUserChange);
  $(document).on("change", "select.list-DroneID", Ajax.OnDroneChange);

  $('#btnCenterLatLng').on("click", function () {
    NOCMap.MovePolygonByCenter();
    NOCMap.SetNOCCoordinates(true);
  });

  $('#btnLatLngPoints').on("click", function () {
    NOCMap.ResetPolygonByLatLng();
    NOCMap.SetNOCCoordinates(true);
  });

  $('#btnLatLngCancel').on("click", function () {
    NOCMap.SetNOCCoordinates(true);
  });

  $('#frmNocAppliation').on("submit", frmValidator.OnSubmit)


  NOC.InitilizeForm(IsOrgAdmin);


});//$(document).ready()


var NOC = function () {
  var LastIndex = 0;
  var ActiveIndex = 0;
  var _isOrgAdmin = false;


  var _initilizeForm = function (IsOrgAdmin) {

    _isOrgAdmin = IsOrgAdmin;

    NOCMap.InitilizeMap();

    if (NOCDetails.length === 0) {
      NOCDetails = _getNocDefaults();
    }

    for (var i = 0; i < NOCDetails.length; i++) {
      _setNocDefault(i);
      _addNoc({
        Data: NOCDetails[i],
        Index: i
      });
      LastIndex = i;
    }

  };

  var _setNocDefault = function (index) {
    NOCDetails[index]._StartDate = fnDate.ToDate(NOCDetails[index].StartDate);
    NOCDetails[index]._EndDate = fnDate.ToDate(NOCDetails[index].EndDate);
    NOCDetails[index]._StartTime = fnDate.ToTime(NOCDetails[index].StartTime);
    NOCDetails[index]._EndTime = fnDate.ToTime(NOCDetails[index].EndTime);
    return NOCDetails;
  }

  var _getNocDefaults = function () {
    var Defaults = [{
      "NocID": 0,
      "PilotID": 0,
      "DroneID": 0,
      "StartDate": "\/Date(1508902808256)\/",
      "EndDate": "\/Date(1508902808256)\/",
      "StartTime": { "Ticks": 216000000000, "Days": 0, "Hours": 6, "Milliseconds": 0, "Minutes": 0, "Seconds": 0, "TotalDays": 0.25, "TotalHours": 6, "TotalMilliseconds": 21600000, "TotalMinutes": 360, "TotalSeconds": 21600 },
      "EndTime": { "Hours": 18, "Minutes": 0, "Seconds": 0, "Milliseconds": 0, "Ticks": 648000000000, "Days": 0, "TotalDays": 0.75, "TotalHours": 18, "TotalMilliseconds": 64800000, "TotalMinutes": 1080, "TotalSeconds": 64800 },
      "MinAltitude": 0,
      "MaxAltitude": 40,
      "LOS": null,
      "IsUseCamara": false,
      "MSTR_NOC": null,
      "Coordinates": "",
      _StartDate: fnDate.Today,
      _EndDate: fnDate.Tomorrow,
      _StartTime: '06:00',
      _EndTime: '18:00'
    }];
  }
  var _getDataFromRow = function (RowIndex) {
    var Data = {
      "PilotID": $('#PilotID_' + RowIndex).val(),
      "DroneID": $('#DroneID_' + RowIndex).val(),
      "MinAltitude": $('#MinAltitude_' + RowIndex).val(),
      "MaxAltitude": $('#MaxAltitude_' + RowIndex).val(),
      "Coordinates": $('#Coordinates_' + RowIndex).val(),
      "LOS": null,
      "IsUseCamara": false,
      "MSTR_NOC": null,
      _StartDate: fnDate.NextDayOf($('#StartDate_' + RowIndex).val()),
      _EndDate: fnDate.NextDayOf($('#StartDate_' + RowIndex).val()),
      _StartTime: $('#StartTime_' + RowIndex).val(),
      _EndTime: $('#EndTime_' + RowIndex).val()
    }
    return Data;
  }

  var _addNocFromButton = function (Button) {
    LastIndex++;
    var id = toID(Button.attr('data-id'));
    var NewLI = _addNoc({
      Data: _getDataFromRow(id),
      Index: LastIndex
    });

  }

  var _moveGoogleMapTo = function (Button) {
    var ID = toID(Button.attr('data-id'));
    var LI = $('#NOCDetail_' + ID);
    $('#GoogleMapLayer').appendTo(LI).show();
    $('#NocSections LI').removeClass("active");
    LI.addClass("active")

    NOCMap.SetCoordinatesOf(ID);
    return LI;
  }

  var _addNoc = function (Options) {
    var ID = Options.Index;
    var Data = Options.Data;

    var thisNoc = NOCDetails[ID];
    var HTML = $('#NOCDetails').html();
    HTML = HTML.replace(/\[\#\]/g, '_' + ID);
    HTML = HTML.replace(/\[\$\]/g, '[' + ID + ']');
    HTML = HTML.replace(/\[sl#\]/g, '[' + (ID + 1) + ']');
    HTML = HTML.replace(/hasDatepicker/g, "");
    var LI = $('<LI id="NOCDetail_' + ID + '" data-id="' + ID + '"></LI>').append(HTML);
    LI.appendTo('#NocSections');

    LI.find('#StartDate_' + ID).val(Data._StartDate);
    LI.find('#StartTime_' + ID).val(Data._StartTime);
    LI.find('#EndDate_' + ID).val(Data._EndDate);
    LI.find('#EndTime_' + ID).val(Data._EndTime);
    LI.find('#MinAltitude_' + ID).val(Data.MinAltitude);
    LI.find('#MaxAltitude_' + ID).val(Data.MaxAltitude);
    LI.find('#Coordinates_' + ID).val(Data.Coordinates);
    if (Data.PilotID !== 0) {
      LI.find('#PilotID_' + ID).val(Data.PilotID);
      LI.find('#PilotID_' + ID).trigger("change", [Data.DroneID + '']);
    }

    if (Data.LOS === 'BLOS') {
      LI.find('#LOS_' + ID + '_BVLOS').attr({ checked: 'checked' });
    } else {
      LI.find('#LOS_' + ID + '_VLOS').attr({ checked: 'checked' });
    }
    if (Data.IsUseCamara) {
      LI.find('#IsUseCamara_' + ID + '_v1').attr({ checked: 'checked' });
    } else {
      LI.find('#IsUseCamara_' + ID + '_v0').attr({ checked: 'checked' });
    }


    //Move the google map to first item
    $('#GoogleMapLayer').appendTo(LI);
    $('#NocSections LI').removeClass("active");
    LI.addClass("active")

    if (ID !== 0) LI.find('input.first-field').focus();

    NOCMap.SetCoordinatesTo(Data.Coordinates);
    return LI;

  };

  var _deleteNOCRow = function (Button) {
    if ($('#NocSections LI').length <= 1) {
      window.alert('Can not remove the last NOC');
      return;
    };

    if (!window.confirm('Are you Sure?')) return;
    var ID = toID(Button.attr('data-id'));

    var LI = $('#NOCDetail_' + ID);
    if (LI.find('#GoogleMapLayer').length > 0) {
      $('#GoogleMapLayer').hide().appendTo('body');
    }

    LI.remove();

    _ReIndexRows();

  }

  var _ReIndexRows = function () {
    var RowIndex = -1;
    $('#NocSections').children('LI').each(function () {
      var LI = $(this);
      var ID = LI.attr('data-id');
      RowIndex++;
      LI.find('input').each(function () {
        var Input = $(this);
        var InputName = Input.attr('name');
        InputName = InputName.replace(/\[\d+\]$/, '[' + RowIndex + ']')
        console.log(Input.attr('id') + ' -> ' + Input.attr('name') + ' -> ' + InputName);
      });

    });

  }


  function toID(Index) {
    Index = Index.replace('_', '');
    var n = parseInt(Index);
    if (isNaN(n)) n = 0;
    return n;
  }

  _isOrgAdminfn = function () {
    return _isOrgAdmin;
  }

  return {
    InitilizeForm: _initilizeForm,
    AddNoc: _addNocFromButton,
    MoveGoogleMapTo: _moveGoogleMapTo,
    DeleteNocRow: _deleteNOCRow,
    IsOrgAdmin: _isOrgAdminfn
  };

}();


var NOCMap = function () {
  var _DefaultCoordinates = '25.05569 55.44882,25.12533 55.52759,25.16790 55.50150,25.09580 55.41586';
  var _MapCoordinates = '';
  var BoundaryBox = {};
  var map = {};
  var ItemIndex = 0;
  var SetCoordinatesTimer = null;
  var LatLngItemsCount = 0;

  var _initilizeMap = function () {
    var BoundaryBoxCoordinates = getLatLngArray(_DefaultCoordinates);
    var myLatLng = BoundaryBoxCoordinates[0];
    var mapDiv = document.getElementById('GoogleMap');
    map = new google.maps.Map(mapDiv, {
      center: myLatLng,
      zoom: 10,
      styles: getADSBMapStyle()
    });    

    // Construct the polygon.
    BoundaryBox = new google.maps.Polygon({
      paths: BoundaryBoxCoordinates,
      strokeColor: '#FF0000',
      strokeOpacity: 0.7,
      strokeWeight: 1,
      fillColor: '#FF0000',
      fillOpacity: 0.1,
      editable: true,
      draggable: true
    });
    BoundaryBox.setMap(map);

    google.maps.event.addListener(BoundaryBox.getPath(), 'set_at', setCoordinatesEvent);
    google.maps.event.addListener(BoundaryBox.getPath(), 'insert_at', setCoordinatesEvent);

    //code for no fly zones
    var KmlUrl = 'http://dcaa.exponent-ts.com/Map/NoFlyzone?R=' + Math.random();
    var kmlOptions = {
      preserveViewport: true,
      map: map
    };
    NoFlyZone = new google.maps.KmlLayer(KmlUrl, kmlOptions);
    NoFlyZone.setValues({ map: map });

    $(document).on("click", "tr.LatLngRow > td > span.delete", _deleteCooridateItem)
    
  };


  var _deleteCooridateItem = function () {
    var Span = $(this);
    if (window.confirm('Are you sure?')) {
      Span.closest('TR').remove();
      $('#btnLatLngCancel').fadeIn();
    }
  }

  var _setCoordinatesOf = function (ID) {
    ItemIndex = ID;
    var e = document.getElementById('Coordinates_' + ItemIndex);
    _MapCoordinates = e.value;
    if (_MapCoordinates === "") _MapCoordinates = _DefaultCoordinates;
    //var BoundaryBoxCoordinates = getLatLngArray(_MapCoordinates);
    resetCoordinates(_MapCoordinates);
    setNOCCoordinates(true);
  }

  var _setCoordinatesTo = function (Coordinates) {
    if (typeof Coordinates !== 'string') Coordinates = '';
    if (Coordinates === "") Coordinates = _DefaultCoordinates;

    resetCoordinates(Coordinates);
    setNOCCoordinates(true);
  }

  var _getCoordinates = function() {
    var Bounds = BoundaryBox.getPath().getArray();
    var LatLng = '';
    for (var i = 0; i < Bounds.length; i++) {
      if (LatLng !== '') LatLng += ',';
      var Lat = Bounds[i].lat();
      var Lng = Bounds[i].lng();
      LatLng = LatLng + Lat.toFixed(5) + ' ' + Lng.toFixed(5);
    }
    return LatLng;
  }

  var setCoordinatesEvent = function () {
    if (SetCoordinatesTimer) window.clearTimeout(SetCoordinatesTimer);
    SetCoordinatesTimer = window.setTimeout(function () {
      console.log("Calling setCoordinates - " + SetCoordinatesTimer);
      setNOCCoordinates(false);
    }, 200);    
  }

  var setNOCCoordinates = function (isZoom) {
    var bounds = new google.maps.LatLngBounds();
    var paths = BoundaryBox.getPaths();
    var LatLng = '';
    var TBody = $('#GoogleMapCoordinates > tbody.LatLng');
    $('#GoogleMapCoordinates > tbody.LatLng > tr.LatLngRow').empty();
    for (var i = 0; i < paths.getLength(); i++) {
      var path = paths.getAt(i);
      LatLngItemsCount = path.getLength() - 1;
      for (var ii = 0; ii < path.getLength(); ii++) {
        if (LatLng !== '') LatLng += ',';
        var inPath = path.getAt(ii);
        bounds.extend(inPath);
        LatLng += inPath.lat().toFixed(7) + ' ' + inPath.lng().toFixed(7);
        var TR =
          '<tr class="LatLngRow">\n' +
          '  <td>' + (ii + 1) + '.</td>\n' +
          '  <td><input type="text" name="lat_' + ii + '" id="lat_' + ii + '" value="' + inPath.lat().toFixed(7) + '" /></td>\n' +
          '  <td><input type="text" name="lng_' + ii + '" id="lng_' + ii + '" value="' + inPath.lng().toFixed(7) + '" /></td>\n' +
          '  <td><span data-id="' + ii + '" class="icon delete">&#xf057;</span></td>\n' +
          '</tr>\n';
        TBody.append(TR);
      }
    }
    if (isZoom) map.fitBounds(bounds);
    var Center = bounds.getCenter();
    $('#lat').val(Center.lat().toFixed(7));
    $('#lng').val(Center.lng().toFixed(7));
    $('#GoogleMapLayer').parent('LI').find('input.Coordinates').val(LatLng);

    $('#btnLatLngCancel').fadeOut();
  };

  var resetCoordinates = function (Coordinates) {
    var BoundaryBoxCoordinates = getLatLngArray(Coordinates);
    BoundaryBox.setPath(BoundaryBoxCoordinates);
    google.maps.event.addListener(BoundaryBox.getPath(), 'set_at', setCoordinatesEvent);
    google.maps.event.addListener(BoundaryBox.getPath(), 'insert_at', setCoordinatesEvent);
  };

  var getLatLngArray = function (Cordinates) {
    var Bounds = [];
    var LatLng = Cordinates.split(',');
    for (var i = 0; i < LatLng.length; i++) {
      var Bound = LatLng[i].split(" ");
      Bounds.push({ lat: parseFloat(Bound[0]), lng: parseFloat(Bound[1]) });
    }
    return Bounds;
  };

  var _movePolygonByCenter = function () {
    var nLat = parseFloat($('#lat').val());
    var nLng = parseFloat($('#lng').val());
    if (isNaN(nLat)) return;

    var bounds = new google.maps.LatLngBounds();
    var paths = BoundaryBox.getPaths();
    var path;
    for (var i = 0; i < paths.getLength(); i++) {
      path = paths.getAt(i);
      for (var ii = 0; ii < path.getLength(); ii++) {
        bounds.extend(path.getAt(ii));
      }
    }
    var Center = bounds.getCenter();
    var cLat = Center.lat();
    var cLng = Center.lng();
    var nBounds = new google.maps.LatLngBounds();

    var nPath = [];
    path = paths.getAt(0);
    for (var j = 0; j < path.length; j++) {
      var pPath = path.getAt(j);
      var dLat = cLat - pPath.lat();
      var dLng = cLng - pPath.lng();

      var tLat = nLat + dLat;
      var tLng = nLng + dLng;
      //console.log("dLat: " + dLat + ", lat: " + pPath.lat() + ", lng: " + pPath.lng() + ", tLat: " + tLat + ", tLng: " + tLng);
      var LN = { lat: tLat, lng: tLng };
      nPath.push(LN);
      nBounds.extend(LN);
    }
    BoundaryBox.setPath(nPath);
    google.maps.event.addListener(BoundaryBox.getPath(), 'set_at', setCoordinatesEvent);
    google.maps.event.addListener(BoundaryBox.getPath(), 'insert_at', setCoordinatesEvent);
    map.fitBounds(nBounds);
  };

  var _resetPolygonByLatLng = function () {
    var Bounds = [];
    var bounds = new google.maps.LatLngBounds();

    for (var Index = 0; Index <= LatLngItemsCount; Index++) {
      var oLat = $('#lat_' + Index);
      var oLng = $('#lng_' + Index);
      var lat = parseFloat(oLat.val());
      var lng = parseFloat(oLng.val());
      if (isNaN(lat) || isNaN(lng)) {
        //missing item - skip this
      } else {
        var LatLng = { lat: lat, lng: lng };
        Bounds.push(LatLng);
        bounds.extend(LatLng);
      }
    }

    if (Bounds.length === 0) return;
    BoundaryBox.setPath(Bounds);
    google.maps.event.addListener(BoundaryBox.getPath(), 'set_at', setCoordinatesEvent);
    google.maps.event.addListener(BoundaryBox.getPath(), 'insert_at', setCoordinatesEvent);
    map.fitBounds(bounds);

  }

  return {
    InitilizeMap: _initilizeMap,
    SetCoordinatesOf: _setCoordinatesOf,
    SetCoordinatesTo: _setCoordinatesTo,
    GetCoordinates: _getCoordinates,
    MovePolygonByCenter: _movePolygonByCenter,
    SetNOCCoordinates: setNOCCoordinates,
    ResetPolygonByLatLng: _resetPolygonByLatLng
  };

}();


var Ajax = function () {

  var _onUserChange = function (e, DroneID) {
    var SelectBox = $(this);
    var UserID = SelectBox.val();
    var ID = toID(SelectBox.attr('data-id'));

    var TheList = document.getElementById('DroneID_' + ID);
    TheList.options.length = 1;
    TheList.options[0].text = "Reading...";

    if (UserID === '') {
      $('#val-PilotID_' + ID).fadeIn();
      TheList.options[0].text = "Please Select a UAS...";
      return;
    } else {
      $('#val-PilotID_' + ID).fadeOut();
    }


    $.ajax({
      type: 'GET',
      url: '/Rpas/NOCRegisterRPAS/' + UserID,
      success: function (data) {
        if (data.length > 0) {
          TheList.options.length = data.length + 1;
          for (var i = 0; i < data.length; i++) {
            TheList.options[i + 1].value = data[i].Value;
            TheList.options[i + 1].text = data[i].Text;
          }
          if (typeof DroneID === 'string') TheList.value = DroneID;
        }
        TheList.options[0].text = "Please Select a UAS...";

      },
      error: function () {

      }
    });

  };

  var _onDroneChange = function () {
    var SelectBox = $(this);
    var DroneID = SelectBox.val();
    var ID = toID(SelectBox.attr('data-id'));

    if (DroneID === '') {
      $('#val-DroneID_' + ID).fadeIn();
    } else {
      $('#val-DroneID_' + ID).fadeOut();
    }
  }

  function toID(Index) {
    Index = Index.replace('_', '');
    var n = parseInt(Index);
    if (isNaN(n)) n = 0;
    return n;
  };

  return {
    OnUserChange: _onUserChange,
    OnDroneChange: _onDroneChange
  };

}();



var fnDate = function () {

  var _tomorrow = function () {
    var tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    return _ToString(tomorrow);
  }

  var _today = function () {
    var today = new Date();
    return _ToString(today);
  }

  var _nextDay = function (objDate) {
    if (isString(objDate)) objDate = new Date(Date.parse(objDate));
    objDate.setDate(objDate.getDate() + 1);
    return _ToString(objDate);
  }


  var _ToDate = function (strDate) {
    if (strDate === null) return new _today();

    var r = /\/Date\(([0-9]+)\)\//i
    var matches = strDate.match(r);
    if (matches !== null && matches.length === 2) {
      var fDate = new Date(parseInt(matches[1]));
      return _ToString(fDate);
    }

    var nDate = new Date(Date.parse(strDate));
    if (nDate === null || isNaN(nDate)) return new _today();

    return _ToString(nDate);
  }


  var _ToTime = function (timeObj) {
    //"Hours": 6, "Minutes": 0, "Seconds": 0, 
    var Time = '00:00';
    if (timeObj.Hours && timeObj.Minutes) {
      var H = (timeObj.Hours <= 9 ? '0' : '') + timeObj.Hours;
      var M = (timeObj.Minutes <= 9 ? '0' : '') + timeObj.Minutes;
      Time = H + ':' + M;
    } else if (timeObj.length === 8){
      Time = timeObj.substring(0, 5);
    }
    return Time;
    
  };

  var _ToString = function (objDate) {
    var Months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
    var sDate = 
      objDate.getDate() + '-' +
      Months[objDate.getMonth()] + '-' +
      objDate.getFullYear();

    return sDate;
  }

  function isString(value) {
    return typeof value === 'string' || value instanceof String;
  };

  return {
    Today: _today,
    Tomorrow: _tomorrow,
    NextDayOf: _nextDay,
    ToDate: _ToDate,
    ToTime: _ToTime
  }

}();


var frmValidator = function () {

  var _validate = function (e) {
    //e.preventDefault();
    var ErrorFields = 0;

    $('#NocSections').children('LI').each(function () {
      var LI = $(this);
      var ID = LI.attr('data-id');
      
      if (NOC.IsOrgAdmin() && $('#PilotID_' + ID).val() === '') {
        $('#val-PilotID_' + ID).fadeIn();
        ErrorFields++;
      }

      if ($('#DroneID_' + ID).val() === '') {
        $('#val-DroneID_' + ID).fadeIn();
        ErrorFields++;
      }

    });



    if (ErrorFields > 0)
      return false;

    return true;

  };

  return {
    OnSubmit: _validate
  };

}();