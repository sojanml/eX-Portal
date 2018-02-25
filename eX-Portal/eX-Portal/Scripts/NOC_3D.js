$(document).ready(function () {
  NOCView.Init();

  $('LI.noc-li-item').on("click", function () {
    NOCView.SetActive($(this));
  });

  $('#btnSwitch3D').on("click", function () {
    $('#btnSwitch3D').hide();
    $('#btnSwitch2D').show();

    $('#GoogleMap').hide();
    $('#cesiumContainer').show();
  });

  $('#btnSwitch2D').on("click", function () {
    $('#btnSwitch2D').hide();
    $('#btnSwitch3D').show();

    $('#GoogleMap').show();
    $('#cesiumContainer').hide();

    GoogleMap.Reset();

  });


});//$(document).ready()


var NOCView = function () {
  var _setActive = function(elem) {
    var nocid = elem.attr("data-nocid");
    GoogleMap.SetPath(NOCDetails[nocid].Coordinates, NOCDetails[nocid].OuterCoordinates);
    NOC_3D.DrawPolygon(NOCDetails[nocid]);
    $('LI.noc-li-item').removeClass("active");
    elem.addClass("active");

    $('#content').html($('#noc-section-' + nocid).html());
  }

  var _Init = function () {
    var IDs = Object.keys(NOCDetails);
    var firstKey = IDs[0];

    GoogleMap.init();
    NOC_3D.Init(NOCDetails[firstKey]);

    GoogleMap.SetPath(NOCDetails[firstKey].Coordinates, NOCDetails[firstKey].OuterCoordinates);
    $('LI.noc-li-item:first-child').addClass("active");

    $('#content').html($('#noc-section-' + firstKey).html());
  };

  return {
    SetActive: _setActive,
    Init: _Init
  };

}();

var GoogleMap = function () {

  var _map = {};
  var _poly = {};
  var _outerPoly = {};
  var _outer = [];
  var _innerPath = [];
  var _outerPath = [];


  var _SetPath = function (inner, outer) {
    _inner = getLatLngArray(inner);    
    _outer = getLatLngArray(outer);
    _outerPath = _holoPolygon(_inner, _outer);

    _poly.setPath(_inner);
    _poly.setMap(_map);

    _outerPoly.setPath(_outerPath);
    _outerPoly.setMap(_map);

    _map.fitBounds(getBounds(_outer));
  }

  var _Reset = function () {
    _poly.setPath(_inner);
    _poly.setMap(_map);

    _outerPoly.setPath(_outerPath);
    _outerPoly.setMap(_map);

    _map.fitBounds(getBounds(_outer));

  }

  var _init = function (CenterPoint) {
    _map = new google.maps.Map(document.getElementById('GoogleMap'), {
      center: { lat: 25.05569, lng: 55.44882 },
      zoom: 10,
      styles: getADSBMapStyle()
    });


    // Construct the polygon.
    _poly = new google.maps.Polygon({
      paths: _innerPath,
      strokeColor: '#FF0000',
      strokeWeight: 0,
      fillColor: '#FF0000',
      fillOpacity: 0.2

    });

    // Construct the polygon.
    _outerPoly = new google.maps.Polygon({
      paths: _outerPath,
      strokeWeight: 0,
      fillColor: '#00FF00',
      fillOpacity: 0.2
    });

    _map.fitBounds(getBounds(_outer));

  };
  

  var _holoPolygon = function (inner, outer) {
    var nArray = inner.concat(inner[0]);

    for (var i = outer.length - 1; i >= 0; i--) {
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
  
  return {
    init: _init,
    SetPath: _SetPath,
    Reset: _Reset
  };

}();

var NOC_3D = function () {
  var viewer = {};
  var AddedOuterPolygon = {};
  var AddedInnerPolygon = {};
  var NoFlyZones = {};

  var _ViewIn3D = function (elem) {
    var Key = elem.attr('data-nocid');
    NOC_3D.DrawPolygon(NOCDetails[Key]);


  }

  var _init = function (Coordinates) {
    viewer = new Cesium.Viewer('cesiumContainer', {
      animation: false,
      fullscreenButton: false,
      vrButton: false,
      geocoder: false,
      homeButton: false,
      infoBox: false,
      timeline: false
    });
    var InnerCoordinates = GPS.ToCesium(Coordinates.Coordinates, Coordinates.Altitude, true);
    var OuterCoordinates = GPS.ToCesium(Coordinates.OuterCoordinates, Coordinates.Altitude, false);
    var InnerPolygon = new Cesium.Entity({
      id: 'InnerPolygon',
      polygon: {
        hierarchy: InnerCoordinates,
        extrudedHeight: 0,
        perPositionHeight: true,
        material: Cesium.Color.BLUE.withAlpha(0.2),
        outline: true,
        outlineColor: Cesium.Color.BLACK,
        show: true
      }
    });

    var OuterPolygon = new Cesium.Entity({
      id: 'OuterPolygon',
      polygon: {
        hierarchy: 
        {
          positions: OuterCoordinates,
          holes: [{
            positions: InnerCoordinates
          }]
        },
        extrudedHeight: 0,
        perPositionHeight: true,
        material: Cesium.Color.YELLOW.withAlpha(0.5),
        outline: true,
        outlineColor: Cesium.Color.BLACK,
        show: true
      }
    });
    AddedInnerPolygon = viewer.entities.add(InnerPolygon);
    AddedOuterPolygon = viewer.entities.add(OuterPolygon);
    viewer.zoomTo(AddedOuterPolygon);  

    _loadNoFlyZone();
  };

  var _loadNoFlyZone = function () {
    $.ajax({
      type: "GET",
      url: '/NOC/NoFlyZone',
      contentType: "application/json",
      success: _drawNoFlyZone,
      error: function (data, text) {
        //alert('Failed to fetch flight: ' + data);
      },
      complete: function () {
      }
    });
  };

  var _drawNoFlyZone = function (data) {
    for (var i = 0; i < data.length; i++) {
      if (data[i].Coordinates === null) continue;
      var Coordinates = GPS.ToCesium(data[i].Coordinates, 1200);
      var id = 'NoFlyZone' + i;
      var Color = Cesium.Color.BLUE.withAlpha(0.3);
      switch (data[i].Color.toLowerCase()) {
        case 'red':
          Color = Cesium.Color.RED.withAlpha(0.3);
          break;
        case 'green':
          Color = Cesium.Color.GREEN.withAlpha(0.3);
          break;
        case 'orange':
          Color = Cesium.Color.ORANGE.withAlpha(0.3);
          break;
      }
      NoFlyZones[id] = new Cesium.Entity({
        id: id,
        polygon: {
          hierarchy: Coordinates ,
          extrudedHeight: 0,
          perPositionHeight: true,
          material: Color,
          outline: true,
          outlineColor: Cesium.Color.BLACK,
          show: true
        }
      });
      viewer.entities.add(NoFlyZones[id]);
    }
  };

  var _drawPolygon = function (Coordinates) {
    
    var InnerCoordinates = GPS.ToCesium(Coordinates.Coordinates, Coordinates.Altitude);
    var OuterCoordinates = GPS.ToCesium(Coordinates.OuterCoordinates, Coordinates.Altitude);
    AddedInnerPolygon.polygon.hierarchy = InnerCoordinates;
    AddedOuterPolygon.polygon.hierarchy = {
      positions: OuterCoordinates,
      holes: [{
        positions: InnerCoordinates
      }]
    };
    viewer.zoomTo(AddedOuterPolygon)
  }

  var _addToolBar = function (NOCDetails) {
    var DropDown = $('<select id="ToolBarSelect"></select>');
    for (var i = 0; i < NOCDetails.length; i++) {
      DropDown.append('<option value="' + i + '">' + NOCDetails[i].Toolbar + '</option>');
    }
    DropDown.on("change", function () {
      var v = $(this).val();
      var iV = parseInt(v);
      NOC_3D.DrawPolygon(NOCDetails[iV]);
      //alert("Selected: " + v);
    })

    $('#cesiumContainer').append(DropDown);
    
  };

  return {
    Init: _init,
    DrawPolygon: _drawPolygon,
    AddTollBar: _addToolBar,
    ViewIn3D: _ViewIn3D
  };
}();

var GPS = function () {

  var _toCesiumCartesian3 = function (Coordinates, Altitude, IsClosePolygon) {
    if (Altitude + '' === 'undefiend') Altitude = 50;
    var CesiumCoordinates = [];
    var aCoordinates = Coordinates.split(',');
    for (var i = 0; i < aCoordinates.length; i++) {
      var LatLng = aCoordinates[i].split(" ");
      var Lat = parseFloat(LatLng[0]);
      var Lng = parseFloat(LatLng[1]);
      CesiumCoordinates.push(Lng, Lat, Altitude);
    }
    if (IsClosePolygon) {
      var fLng = CesiumCoordinates[0];
      var fLat = CesiumCoordinates[1];
      var fAlt = CesiumCoordinates[2];
      CesiumCoordinates.push(fLng, fLat, fAlt);
    }
    return Cesium.Cartesian3.fromDegreesArrayHeights(CesiumCoordinates);
  };


  var _bounds = function () {
    return Cesium.Rectangle.fromDegrees(MinLat, MinLng);
  }

  return {
    ToCesium: _toCesiumCartesian3,
    Bounds: _bounds
  };

}();