﻿var FlightMapView3D = function () {
  var _viewer = null;
  var _entity = {};
  var _positions = new Cesium.SampledPositionProperty();
  var _orientations = new Cesium.SampledProperty(Cesium.Quaternion);
  var _startTime = null;
  var _endTime = null;
  var _activeTime = null;
  var _initPosition = null;

  var _ClockSpeed = 1;

  var _init = function () {
    //do not intilize again.
    if (_viewer !== null) return;

    //Start initilization
    Cesium.BingMapsApi.defaultKey = 'Avg1KA_O2lc_wdCXabCC60Zo6XWEF6WGHTqg57W3LmyHfvoop60OPvRkqVLcNL_X';
    _viewer = new Cesium.Viewer('CesiumMap', {
      scene3DOnly: true,
      selectionIndicator: false,
      baseLayerPicker: false,
      geocoder: false,
      homeButton: true,
      infoBox: false,
      vrButton: false,
      animation: false,
      timeline: true
    });

    // Add Bing imagery
    _viewer.imageryLayers.addImageryProvider(new Cesium.BingMapsImageryProvider({
      url: 'https://dev.virtualearth.net',
      mapStyle: Cesium.BingMapsStyle.AERIAL // Can also use Cesium.BingMapsStyle.ROADS
    }));

    // Load STK World Terrain
    _viewer.terrainProvider = new Cesium.CesiumTerrainProvider({
      url: 'https://assets.agi.com/stk-terrain/world',
      requestWaterMask: true, // required for water effects
      requestVertexNormals: true // required for terrain lighting
    });

    _viewer.scene.globe.depthTestAgainstTerrain = true;
    _viewer.scene.globe.enableLighting = true;

    var initialOrientation = new Cesium.HeadingPitchRoll.fromDegrees(7.1077496389876024807, -31.987223091598949054, 0.025883251314954971306);
    var homeCameraView = {
      destination: _initPosition,
      orientation: {
        heading: initialOrientation.heading,
        pitch: initialOrientation.pitch,
        roll: initialOrientation.roll
      }
    };
    // Set the initial view
    _viewer.scene.camera.setView(homeCameraView);

    // Add some camera flight animation options
    homeCameraView.duration = 2.0;
    homeCameraView.maximumHeight = 200;
    homeCameraView.pitchAdjustHeight = 2000;
    homeCameraView.endTransform = Cesium.Matrix4.IDENTITY;

    // Set up clock and timeline.
    if (_activeTime === null) _activeTime = _startTime;
    _viewer.clock.startTime = _startTime;
    _viewer.clock.stopTime = _endTime;
    _viewer.clock.currentTime = _activeTime;
    _viewer.clock.multiplier = _ClockSpeed; // sets a speedup
    _viewer.clock.clockStep = Cesium.ClockStep.SYSTEM_CLOCK_MULTIPLIER; // tick computation mode
    _viewer.clock.clockRange = Cesium.ClockRange.CLAMPED; // loop at the end
    _viewer.clock.shouldAnimate = false;
    if (_viewer.timeline)
      _viewer.timeline.zoomTo(_startTime, _endTime); // set visible range
    //Set bounds of our simulation time 

    //viewer.entities.add(drone);
    _viewer.entities.removeAll();

    _entity = _viewer.entities.add({
      name: 'CesiumDrone',
      position: _positions,
      orientation: _orientations,
      model: {
        uri: '/Cesium/Images/CesiumDrone.gltf',
        minimumPixelSize: 32,
        maximumPixelSize:128
      },
      path: {
        width: 2,
        leadTime: 1,
        trailTime: 60,
        resolution: 2
      }
    });

    _viewer.trackedEntity = _entity;

  };


  var _AddData = function (dataSource) {
    // Attach a 3D model
    var orientationProperty = new Cesium.SampledProperty(Cesium.Quaternion);

    for (var i = 0; i < dataSource.length; i++) {
      var Data = dataSource[i];
      var time = _toUTC(Data.FlightDateTime);
      var position = Cesium.Cartesian3.fromDegrees(Data.Lng, Data.Lat, Data.Altitude);
      var hpRoll = Cesium.HeadingPitchRoll.fromDegrees(Data.Heading, 0, 0);
      //var orientation = Cesium.Quaternion.fromHeadingPitchRoll(hpRoll);
      var orientation = Cesium.Transforms.headingPitchRollQuaternion(position, hpRoll);
      if (_initPosition === null) _initPosition = position;
      _positions.addSample(time, position);
      _orientations.addSample(time, orientation);

    }

    if (_startTime === null) _startTime = _toUTC(dataSource[0].FlightDateTime);
    if (_activeTime === null) _activeTime = _startTime;
    _endTime = _toUTC(dataSource[dataSource.length - 1].FlightDateTime);

    if (_viewer !== null) {
      _viewer.clock.startTime = _startTime;
      _viewer.clock.currentTime = _activeTime;
      _viewer.clock.stopTime = _endTime;    
      if (_viewer.timeline)
        _viewer.timeline.zoomTo(_startTime, _endTime); // set visible range
      _entity.position = _positions;
    }
    //drone.viewFrom = new Cesium.Cartesian3(-30, 0, 0);
    //homeCameraView.destination = initialPosition;
  };

  var _AddDataLive = function (dataSource) {
    // Attach a 3D model
    var orientationProperty = new Cesium.SampledProperty(Cesium.Quaternion);

    for (var i = 0; i < dataSource.length; i++) {
      var Data = dataSource[i];
      var time = _toUTC(Data.FlightDateTime);
      var position = Cesium.Cartesian3.fromDegrees(Data.Lng, Data.Lat, Data.Altitude + 200);
      var hpRoll = Cesium.HeadingPitchRoll.fromDegrees(Data.Heading, 0, 0);
      //var orientation = Cesium.Quaternion.fromHeadingPitchRoll(hpRoll);
      var orientation = Cesium.Transforms.headingPitchRollQuaternion(position, hpRoll);
      if (_initPosition === null) _initPosition = position;
      _positions.addSample(time, position);
      _orientations.addSample(time, orientation);
    }

    _endTime = _toUTC(dataSource[dataSource.length - 1].FlightDateTime);
    if (_viewer !== null) {
      _viewer.clock.stopTime = _endTime;
      _viewer.clock.shouldAnimate = true;
      if (_viewer.timeline)
        _viewer.timeline.zoomTo(_startTime, _endTime); // set visible range
    }

    //drone.viewFrom = new Cesium.Cartesian3(-30, 0, 0);
    //homeCameraView.destination = initialPosition;
  };

  var _MoveToTime = function (SetTime) {
    var d1 = _toUTC(SetTime);
    _activeTime = d1;
    if (_viewer) _viewer.clock.currentTime = _activeTime;
  };

  var _play = function (SetTime) {
    var d1 = _toUTC(SetTime);
    _activeTime = d1;
    if (_viewer !== null) {
      _viewer.clock.currentTime = _activeTime;
      _viewer.clock.shouldAnimate = true;
    }      
  };

  var _pause = function (SetTime) {
    var d1 = _toUTC(SetTime);
    _activeTime = d1;
    if (_viewer !== null)
      _viewer.clock.shouldAnimate = false;
  };

  var _setSpeed = function (speed) {
    _ClockSpeed = speed;
    if (_viewer !== null)
      _viewer.clock.multiplier = _ClockSpeed;
  };

  var _toUTC = function (date) {
    //var timeOffsetInMS = date.getTimezoneOffset() * 60000;
    //date.setTime(date.getTime() - timeOffsetInMS);
    //date.setMinutes(date.getMinutes() - date.getTimezoneOffset());
    var nDt = new Cesium.JulianDate.fromDate(date);
    var nDt2 = new Cesium.JulianDate.fromDate(date);
    var FixTime = date.getTimezoneOffset() * -60;
    Cesium.JulianDate.addSeconds(nDt, FixTime, nDt2);
    return nDt2;
  };

  return {
    Init: _init,
    AddData: _AddData,
    AddDataLive: _AddDataLive,
    MoveTo: _MoveToTime,
    Play: _play,
    Pause: _pause,
    setSpeed: _setSpeed
  };
}();
