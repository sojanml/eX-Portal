var NOCProcess = function () {
  var NocDetails = {};
  var _init = function (NOCs) {
    for (var i = 0; i < NOCs.length; i++) {
      var Detail = NOCs[i];
      var ID = Detail.ID;
      NocDetails[ID] = new NOCDetail();
      NocDetails[ID].init(Detail);
      
    }
  };

  return {
    init: _init
  };
}();

var NOCDetail = function () {
  var _id = 0;
  var _data = {};

  var _map = {};
  var _poly = {};
  var _outerPoly = {};

  var _ajaxBoundary = new Ajax();
  var SetCoordinatesTimer = 0;


  var _init = function (Data) {
    _data = Data;
    _id = _data.id;
    _map = new google.maps.Map(document.getElementById('GoogleMapLayer_' + _id), {
      center: {lat: 25.05569, lng: 55.44882},
      zoom: 10,
      styles: getADSBMapStyle()
    });  

    var _outer = getLatLngArray(_data.outerCoordinates);
    var _inner = getLatLngArray(_data.coordinates);    

    // Construct the polygon.
    _poly = new google.maps.Polygon({
      paths: _inner,
      strokeColor: '#FF0000',
      strokeOpacity: 0.7,
      strokeWeight: 1,
      fillColor: '#FF0000',
      fillOpacity: 0.2,
      editable: true,
      draggable: true,
      map: _map
    });


    google.maps.event.addListener(_poly.getPath(), 'set_at', setCoordinatesEvent);
    google.maps.event.addListener(_poly.getPath(), 'insert_at', setCoordinatesEvent);

    // Construct the polygon.
    _outerPoly = new google.maps.Polygon({
      paths: holoPolygon(_inner, _outer),
      strokeColor: '#00FF00',
      strokeOpacity: 1,
      strokeWeight: 0,
      fillColor: '#00FF00',
      fillOpacity: 0.5,
      map: _map
    });

    _map.fitBounds(getBounds(_outer));

    setupSlider('altitude', _data.alt, 0, 500);
    setupSlider('buffer', _data.buffer, 30, 200);

  };


  var setCoordinatesEvent = function () {
    if (SetCoordinatesTimer) window.clearTimeout(SetCoordinatesTimer);
    SetCoordinatesTimer = window.setTimeout(function () {
      var Distance = $('#noc-nocbuffer-' + _id).val();
      _ajaxBoundary.Run(_poly, Distance, OnBoundaryUpdate);
      setAmmended();
      $('#noc-coordinates-' + _id).val(_ajaxBoundary.getLatLng(_poly));
    }, 100);
  }

  var holoPolygon = function (inner, outer) {
    var nArray = inner;
    nArray.push(inner[0]);
    for (var i = outer.length - 1; i >= 0 ; i--) {
     nArray.push(outer[i]);
    }
    nArray.push(outer[0]);
    return nArray;
  }

  var getLatLngArray = function (Cordinates) {
    var Bounds = [];
    var LatLng = Cordinates.split(',');
    for (var i = 0; i < LatLng.length; i++) {
      var Bound = LatLng[i].split(" ");
      Bounds.push({ lat: parseFloat(Bound[0]), lng: parseFloat(Bound[1]) });
    }
    return Bounds;
  };

  var getBounds = function (Coordinates) {
    var bounds = new google.maps.LatLngBounds();
    for (var i = Coordinates.length - 1; i >= 0; i--) {
      bounds.extend(Coordinates[i]);
    }
    return bounds;
  }


  var setupSlider = function (SliderName, SliderValue, Min, Max) {

    var SliderDiv = $("#slider-" + SliderName + "-" + _id);

    var options = {
      range: "max",
      min: Min,
      max: Max,
      value: SliderValue,
      slide: SliderName === 'altitude' ? setupSliderAltitude : setupSliderBuffer
    };

    SliderDiv.slider(options);
  }

  var setupSliderAltitude = function (event, ui) {
    var SliderVal = $("#slider-altitude-" + _id + "-label");
    SliderVal.html(ui.value);
    $('#noc-maxaltitude-' + _id).val(ui.value);
    setAmmended();
  }

  var setupSliderBuffer = function (event, ui) {
    var SliderVal = $("#slider-buffer-" + _id + "-label");
    SliderVal.html(ui.value);
    $('#noc-nocbuffer-' + _id).val(ui.value);
    _ajaxBoundary.Run(_poly, ui.value, OnBoundaryUpdate);
    setAmmended();
  }

  var OnBoundaryUpdate = function (Zone) {
    var nPath = holoPolygon(getPath(_poly), Zone.Path);
    _outerPoly.setPath(nPath);
  }


  var getPath = function (_poly) {
    var LatLng = [];
    var paths = _poly.getPaths();
    for (var i = 0; i < paths.getLength(); i++) {
      var path = paths.getAt(i);
      for (var j = 0; j < path.getLength(); j++) {
        var inPath = path.getAt(j);
        LatLng.push({ lat: inPath.lat(), lng: inPath.lng()});
      }
    }
    return LatLng;
  }

  var setAmmended = function () {
    $('#noc-status-' + _id + '-Amended').prop('checked', true);
  }

  return {
    init: _init,
    Slider: setupSlider
  };

};

var Ajax = function () {
  var _ExecTimer = null;

  var _run = function (_polygon, Distance, OnSuccess) {
    if (_ExecTimer) window.clearTimeout(_ExecTimer);
    _ExecTimer = window.setTimeout(function () {
      _execCommand(_polygon, Distance, OnSuccess)
    }, 500);
  };

  var _execCommand = function (_polygon, Distance, OnSuccess) {
    var URL = '/NOC/ExtentCoordinates?Coordinates=' + getLatLng(_polygon) + '&Distance=' + Distance;
    $.ajax({
      type: 'GET',
      url: URL,
      success: OnSuccess,
      error: function () {
      }
    });
  };



  var getLatLng = function (_outerPoly) {
    var LatLng = '';
    var paths = _outerPoly.getPaths();
    for (var i = 0; i < paths.getLength(); i++) {
      var path = paths.getAt(i);
      for (var j = 0; j < path.getLength(); j++) {
        if (LatLng !== '') LatLng += ',';
        var inPath = path.getAt(j);
        LatLng += inPath.lat().toFixed(7) + ' ' + inPath.lng().toFixed(7);
      }
    }
    return LatLng;
  }





  return {
    Run: _run,
    getLatLng: getLatLng
  }
}