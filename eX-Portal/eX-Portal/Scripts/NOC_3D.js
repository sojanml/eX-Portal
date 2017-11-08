$(document).ready(function () {
  NOC_3D.AddTollBar(NOCDetails);
  NOC_3D.Init(NOCDetails[0]);
});//$(document).ready()


var NOC_3D = function () {
  var viewer = {};
  var AddedOuterPolygon = {};
  var AddedInnerPolygon = {};
  var NoFlyZones = {};

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
    var InnerCoordinates = GPS.ToCesium(Coordinates.Coordinates, Coordinates.Altitude);
    var OuterCoordinates = GPS.ToCesium(Coordinates.OuterCoordinates, Coordinates.Altitude);
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
        hierarchy: {
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
      if (data[i].Coordinates == null) continue;
      var Coordinates = GPS.ToCesium(data[i].Coordinates, 120);
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
    AddTollBar: _addToolBar
  };
}();

var GPS = function () {

  var _toCesiumCartesian3 = function (Coordinates, Altitude) {
    if (Altitude + '' == 'undefiend') Altitude = 50;
    var CesiumCoordinates = [];
    var aCoordinates = Coordinates.split(',');
    for (var i = 0; i < aCoordinates.length; i++) {
      var LatLng = aCoordinates[i].split(" ");
      var Lat = parseFloat(LatLng[0]);
      var Lng = parseFloat(LatLng[1]);
      CesiumCoordinates.push(Lng, Lat, Altitude);
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