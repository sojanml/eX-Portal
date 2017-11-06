$(document).ready(function () {
  NOC_3D.AddTollBar(NOCDetails);
  NOC_3D.Init(NOCDetails[0]);
});//$(document).ready()


var NOC_3D = function () {
  var viewer = {};
  var AddedOuterPolygon = {};
  var AddedInnerPolygon = {};
  var _init = function (Coordinates) {
    viewer = new Cesium.Viewer('cesiumContainer');
    var InnerCoordinates = GPS.ToCesium(Coordinates.Coordinates);
    var OuterCoordinates = GPS.ToCesium(Coordinates.OuterCoordinates);
    var InnerPolygon = new Cesium.Entity({
      id: 'InnerPolygon',
      polygon: {
        material: Cesium.Color.GREEN,
        hierarchy: InnerCoordinates,
        extrudedHeight: 0,
        perPositionHeight: true,
        material: Cesium.Color.GREEN.withAlpha(0.7),
        outline: true,
        outlineColor: Cesium.Color.GREEN,
        show: true
      }
    });

    var OuterPolygon = new Cesium.Entity({
      id: 'OuterPolygon',
      polygon: {
        material: Cesium.Color.ORANGE,
        hierarchy: {
          positions: OuterCoordinates,
          holes: [{
            positions: InnerCoordinates
          }]
        },
        extrudedHeight: 0,
        perPositionHeight: true,
        material: Cesium.Color.ORANGE.withAlpha(0.7),
        outline: true,
        outlineColor: Cesium.Color.ORANGE,
        show: true
      }
    });

    AddedInnerPolygon = viewer.entities.add(InnerPolygon);
    AddedOuterPolygon = viewer.entities.add(OuterPolygon);

    viewer.zoomTo(AddedOuterPolygon);

  };

  var _drawPolygon = function (Coordinates) {
    var InnerCoordinates = GPS.ToCesium(Coordinates.Coordinates);
    var OuterCoordinates = GPS.ToCesium(Coordinates.OuterCoordinates);
    AddedInnerPolygon.polygon.hierarchy = InnerCoordinates;
    AddedOuterPolygon.polygon.hierarchy = {
      positions: OuterCoordinates,
      holes: [{
        positions: InnerCoordinates
      }]
    };
    AddedOuterPolygon.extrudedHeight = Coordinates.Altitude;
    OuterCoordinates.extrudedHeight = Coordinates.Altitude;

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

  var MinLat = -360;
  var MinLng = -360;
  var MaxLat = -360;
  var MaxLng = -360;

  var _toCesiumCartesian3 = function (Coordinates) {
    _resetBounds();

    var CesiumCoordinates = [];
    var aCoordinates = Coordinates.split(',');
    for (var i = 0; i < aCoordinates.length; i++) {
      var LatLng = aCoordinates[i].split(" ");
      var Lat = parseFloat(LatLng[0]);
      var Lng = parseFloat(LatLng[1]);
      CesiumCoordinates.push(Lng, Lat, 50);
      if (MinLat < Lat || MinLat == -360) MinLat = Lat;
      if (MinLng < Lng || MinLng == -360) MinLng = Lng;
      if (MaxLat > Lat || MaxLat == -360) MaxLat = Lat;
      if (MaxLng > Lng || MaxLng == -360) MaxLng = Lng;
    }
    return Cesium.Cartesian3.fromDegreesArrayHeights(CesiumCoordinates);
  };

  var _resetBounds = function () {
    MinLat = -360;
    MinLng = -360;
    MaxLat = -360;
    MaxLng = -360;
  };

  var _bounds = function () {
    return Cesium.Rectangle.fromDegrees(MinLat, MinLng);
  }

  return {
    ToCesium: _toCesiumCartesian3,
    Bounds: _bounds
  };

}();