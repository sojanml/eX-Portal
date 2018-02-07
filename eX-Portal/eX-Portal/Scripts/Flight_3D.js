$(document).ready(function () {
    var drone = {};
    var czmlDataSource = new Cesium.CzmlDataSource();
    flight.Init(drone, czmlDataSource);
});//$(document).ready()

var flight = function (drone, czmlDataSource) {
   
    var viewer = {};
   
    var _init = function (drone, czmlDataSource) {
        Cesium.BingMapsApi.defaultKey = 'Avg1KA_O2lc_wdCXabCC60Zo6XWEF6WGHTqg57W3LmyHfvoop60OPvRkqVLcNL_X';
        var viewer = new Cesium.Viewer('cesiumContainer', {
            scene3DOnly: true,
            selectionIndicator: false,
            baseLayerPicker: false,
            geocoder: false,
            homeButton: true,
            infoBox: false,
            vrButton: false
        });

        // Add Bing imagery
        viewer.imageryLayers.addImageryProvider(new Cesium.BingMapsImageryProvider({
            url: 'https://dev.virtualearth.net',
            mapStyle: Cesium.BingMapsStyle.AERIAL // Can also use Cesium.BingMapsStyle.ROADS
        }));

        // Load STK World Terrain
        viewer.terrainProvider = new Cesium.CesiumTerrainProvider({
            url: 'https://assets.agi.com/stk-terrain/world',
            requestWaterMask: true, // required for water effects
            requestVertexNormals: true // required for terrain lighting
        });
        //var terrainProvider = new Cesium.VRTheWorldTerrainProvider({
        //    url: 'http://www.vr-theworld.com/vr-theworld/tiles1.0.0/73/'
        //});
        //viewer.terrainProvider = terrainProvider;

        // Enable depth testing so things behind the terrain disappear.
        viewer.scene.globe.depthTestAgainstTerrain = true;

        // Enable lighting based on sun/moon positions
        viewer.scene.globe.enableLighting = true;

        var initialPosition = new Cesium.Cartesian3.fromDegrees(HomeLong, HomeLat, 200);
        var initialOrientation = new Cesium.HeadingPitchRoll.fromDegrees(7.1077496389876024807, -31.987223091598949054, 0.025883251314954971306);
        var homeCameraView = {
            destination: initialPosition,
            orientation: {
                heading: initialOrientation.heading,
                pitch: initialOrientation.pitch,
                roll: initialOrientation.roll
            }
        };
        // Set the initial view
        viewer.scene.camera.setView(homeCameraView);

        // Add some camera flight animation options
        homeCameraView.duration = 2.0;
        homeCameraView.maximumHeight = 200;
        homeCameraView.pitchAdjustHeight = 2000;
        homeCameraView.endTransform = Cesium.Matrix4.IDENTITY;
        // Override the default home button
        viewer.homeButton.viewModel.command.beforeExecute.addEventListener(function (e) {
            e.cancel = true;
            viewer.scene.camera.flyTo(homeCameraView);
        });

        // Set up clock and timeline.
        viewer.clock.shouldAnimate = true; // default
        viewer.clock.startTime = Cesium.JulianDate.fromIso8601("2017-07-10T07:00:00Z");
        viewer.clock.stopTime = Cesium.JulianDate.fromIso8601("2017-07-10T07:20:00Z");
        viewer.clock.currentTime = Cesium.JulianDate.fromIso8601("2017-07-10T07:00:00Z");
        viewer.clock.multiplier = 2; // sets a speedup
        viewer.clock.clockStep = Cesium.ClockStep.SYSTEM_CLOCK_MULTIPLIER; // tick computation mode
        viewer.clock.clockRange = Cesium.ClockRange.LOOP_STOP; // loop at the end
        viewer.timeline.zoomTo(viewer.clock.startTime, viewer.clock.stopTime); // set visible range
        //Set bounds of our simulation time 
        var start = Cesium.JulianDate.fromDate(new Date(2015, 2, 25, 16));
        var kmlOptions = {
            camera: viewer.scene.camera,
            canvas: viewer.scene.canvas,
            clampToGround: true
        };
        // Load geocache points of interest from a KML file
        // Data from : http://catalog.opendata.city/dataset/pediacities-nyc-neighborhoods/resource/91778048-3c58-449c-a3f9-365ed203e914
        //var geocachePromise = Cesium.KmlDataSource.load('SampleData/sampleGeocacheLocations.kml', kmlOptions);
        //// Add geocache billboard entities to scene and style them
        //geocachePromise.then(function (dataSource) {
        //    // Add the new data as entities to the viewer
        //    viewer.dataSources.add(dataSource);
        //});

        var labelText = new Cesium.TimeIntervalCollectionProperty();
       
        var dronePromise = czmlDataSource.load('..\\info\\' + FlightID);
        var LastDataID = 0;
        //var i=50;
        //var orientationProperty = new Cesium.SampledProperty(Cesium.Quaternion); 
        dronePromise.then(function (dataSource) {
            viewer.dataSources.add(dataSource);
            drone = dataSource.entities.values[0];
            var newentity = dataSource.entities.values[1];

            // Attach a 3D model
            var orientationProperty = new Cesium.SampledProperty(Cesium.Quaternion); 
            drone.model = {
                uri: '../Images/CesiumDrone.gltf',
                minimumPixelSize: 40,
                maximumScale: 20
            };
            var positions = drone.position;
            var rolls = newentity.properties.roll;
            var headings = newentity.properties.heading;
            var pitchs = newentity.properties.pitch;
            var lastitemidproperty = newentity.properties.lastdataid;
            LastDataID = lastitemidproperty.getValue();
           
            
            for (var i = 0; i < positions._property._times.length; i++) {
              
                var time = positions._property._times[i];
                // compute orientations
                var heading = headings.getValue(time);
                var pitch = 0;// pitchs.getValue(time);
                  //  
                var roll = 0;// rolls.getValue(positions._property._times[i]);


                var hpRoll= Cesium.HeadingPitchRoll.fromDegrees(heading, pitch, roll);
                var orientation = Cesium.Transforms.headingPitchRollQuaternion(positions.getValue(time), hpRoll);

               // var orientation = Cesium.Quaternion.fromHeadingPitchRoll(hpRoll);
            orientationProperty.addSample(time, orientation);
            if (i === 0)
                initialPosition = positions.getValue(time);
            //  i++;
            }
          
            // Add computed orientation based on sampled positions
            drone.orientation = orientationProperty;
           // drone.description = new Cesium.CallbackProperty(updateLabels, false);
                // //
        //  drone.orientation = new Cesium.VelocityOrientationProperty(drone.position);
            //new Cesium.VelocityOrientationProperty(drone.position);
            // Smooth path interpolation
            drone.position.setInterpolationOptions({
                interpolationDegree:5,
                interpolationAlgorithm: Cesium.HermitePolynomialApproximation
            });
            drone.viewFrom = new Cesium.Cartesian3(-30, 0, 0);
            homeCameraView.destination = initialPosition;
        });

        
        function updateLabels(time) {
            var position = drone.position.getValue(time);

           // var d = labelText.findDataForIntervalContainingDate(time);
            
            if (position === '1')
                position = '2';
            else
                position = '1';
            var updateText = 'Latitude :' + String(lat) + '/n Longitude :' + String(lon) ;

            return updateText;
        }

        viewer.trackedEntity = drone;
       

       // viewer.scene.camera.flyTo(homeCameraView);
        // Create a follow camera by tracking the drone entity
       
        viewer.clock.onTick.addEventListener(function (clock) {

            if (typeof drone.position !== 'undefined')
            {
            var position = drone.position.getValue(clock.currentTime);
            var carto = Cesium.Ellipsoid.WGS84.cartesianToCartographic(position);
            var lon = Cesium.Math.toDegrees(carto.longitude).toFixed(6);
            var lat = Cesium.Math.toDegrees(carto.latitude).toFixed(6); 
            var updateText =+ '/n Longitude :' + String(lon);

            lonlabel.textContent = 'Latitude :' + String(lat);
            latlabel.textContent = ' Longitude :' + String(lon);
            if (IsLive)
            {
                drone.position = position;
            }
            }
        });

        if (IsLive)
        {
            
        
        setInterval(function () {
            dronePromise = czmlDataSource.process('..\\info\\' + FlightID + "?lastdataid=" + LastDataID);
            

        //    dronePromise.then(function (dataSource) {
        //        var newdrone = {};
        //        newdrone = dataSource.entities.values[0];
        //        var newentity = dataSource.entities.values[1];

        //        // Attach a 3D model
        //        var orientationProperty = new Cesium.SampledProperty(Cesium.Quaternion);
              
        //        var positions = newdrone.position;
        //        var rolls = newentity.properties.roll;
        //        var headings = newentity.properties.heading;
        //        var pitchs = newentity.properties.pitch;
        //        var lastitemidproperty = newentity.properties.lastdataid;
        //        LastDataID = lastitemidproperty.getValue();
        //        for (var i = 0; i < positions._property._times.length; i++) {

        //            var time = positions._property._times[i];
        //            // compute orientations 
        //            var heading = headings.getValue(time);
        //            var pitch = pitchs.getValue(time);
        //            //  
        //            var roll = rolls.getValue(positions._property._times[i]);
        //            //  var hpRoll = new Cesium.HeadingPitchRoll(0,0,0);
        //            var hpRoll = Cesium.HeadingPitchRoll.fromDegrees(heading, pitch, roll);
        //            var orientation = Cesium.Transforms.headingPitchRollQuaternion(positions.getValue(time), hpRoll);
        //            // var orientation = Cesium.Quaternion.fromHeadingPitchRoll(hpRoll);
        //            orientationProperty.addSample(time, orientation);
        //            //  i++;
        //        }

        //        drone.position.addSample(positions);
        //        // Add computed orientation based on sampled positions
        //        drone.orientation.addSample(orientationProperty);
        //        // drone.orientation = new Cesium.VelocityOrientationProperty(drone.position);
        //        //new Cesium.VelocityOrientationProperty(drone.position);
        //        // Smooth path interpolation
        //        drone.position.setInterpolationOptions({
        //            interpolationDegree: 3,
        //            interpolationAlgorithm: Cesium.HermitePolynomialApproximation
        //        });
        //        drone.viewFrom = new Cesium.Cartesian3(-30, 0, 0);
        //    });



        }, 20000);
        }
    };

    return {
        Init: _init
    };
}();
