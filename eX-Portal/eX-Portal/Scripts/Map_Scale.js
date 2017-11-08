var MapScale = function () {
    var dismarkers = [];
    var DistanceToggler = 0;
    var KmlUrl = '';
    var kmlOptions = null;
    var DistanceDiv = document.createElement('div');
    var controlText = document.createElement('div');
    var ClearDiv = document.createElement('div');
    var length_in_km = 0;
    var rulerpoly;
    var IsClearAdded = 0;

    var _InitializeScale = function (map) {
        _map = map;
        rulerpoly = new google.maps.Polyline({
            strokeColor: '#000000',
            strokeOpacity: 1.0,
            strokeWeight: 3
        });
        rulerpoly.setMap(_map);
        google.maps.LatLng.prototype.kmTo = function (a) {
            var e = Math, ra = e.PI / 180;
            var b = this.lat() * ra, c = a.lat() * ra, d = b - c;
            var g = this.lng() * ra - a.lng() * ra;
            var f = 2 * e.asin(e.sqrt(e.pow(e.sin(d / 2), 2) + e.cos(b) * e.cos
                (c) * e.pow(e.sin(g / 2), 2)));
            return f * 6378.137;
        }

        google.maps.Polyline.prototype.inKm = function (n) {
            var a = this.getPath(n), len = a.getLength(), dist = 0;
            for (var i = 0; i < len - 1; i++) {
                dist += a.getAt(i).kmTo(a.getAt(i + 1));
            }
            return dist;
        }

        var distanceControl = new CenterControl(DistanceDiv, _map, length_in_km, controlText);
        DistanceDiv.index = 1;
        _map.controls[google.maps.ControlPosition.TOP_CENTER].push(DistanceDiv);
        DistanceDiv.hidden = true;

        AddScale(_map);
    };

    function AddMarkers() {
        _map.addListener('click', addLatLng);
    };
    function addLatLng(event) {

        var path = rulerpoly.getPath();

        // Because path is an MVCArray, we can simply append a new coordinate
        // and it will automatically appear.
        path.push(event.latLng);

        // Add a new marker at the new plotted point on the polyline.
        var marker = new google.maps.Marker({
            position: event.latLng,
            title: '#' + path.getLength(),
            map: map,
            overlay: true,
           icon: {
               url: '/images/scaler.png',
                size: new google.maps.Size(20,30),
                anchor: new google.maps.Point(10, 20)
            }
        });
        dismarkers.push(marker);

        length_in_km = (rulerpoly.inKm() * 1000).toFixed(2);;
        setdistancediv(DistanceDiv, map, length_in_km, controlText)
        if (IsClearAdded === 0) {
            SetClearMarkersDiv(map, path, rulerpoly, controlText);
            IsClearAdded = 1;
        } else {
            ClearDiv.hidden = false;
        }
    };
    function setdistancediv(controlDiv, map, length_in_km, controlText) {
        controlText.innerHTML = length_in_km + ' m';

    };
    function SetClearMarkersDiv(map, path, rulerpoly, controlText) {

        var ClearTextDiv = document.createElement('div');
        _map.controls[google.maps.ControlPosition.TOP_CENTER].push(ClearDiv);
        // DistanceDiv.appendChild(ClearDiv);
        CenterControl(ClearDiv, map, 'CLEAR', ClearTextDiv);
        // Setup the click event listeners: simply set the map to Chicago.
        ClearTextDiv.addEventListener('click', function () {
            path.clear();
            deleteMarkers();
            // _map.controls[google.maps.ControlPosition.TOP_CENTER].pop();
            controlText.innerHTML = '0';
            ClearDiv.hidden = true;
        });

    };
    function CenterControl(controlDiv, map, length_in_km, controlText) {

        // Set CSS for the control border.
        var controlUI = document.createElement('div');
        controlUI.style.backgroundColor = '#fff';
        controlUI.style.border = '2px solid #fff';
        controlUI.style.borderRadius = '3px';
        controlUI.style.boxShadow = '0 2px 6px rgba(0,0,0,.3)';
        controlUI.style.cursor = 'pointer';
        controlUI.style.marginTop = '10px';
        controlUI.style.textAlign = 'center';
        controlUI.style.width = '60px';

        controlUI.style.padding = '6px';
        controlUI.title = '';
        controlDiv.appendChild(controlUI);

        // Set CSS for the control interior.
        controlText.style.color = 'rgb(25,25,25)';
        controlText.style.fontFamily = 'Roboto,Arial,sans-serif';
        controlText.style.fontSize = '11px';
        //  controlText.style.padding= '8px';

        controlText.innerHTML = length_in_km;
        controlUI.appendChild(controlText);



    };

    function AddScale(map) {
        // Set CSS for the control border.

        var ScaleDiv = document.createElement('div');
        // var controlUI = document.createElement('div');
        var controlImg = document.createElement('img');

        ScaleDiv.style.backgroundColor = '#fff';
        //  controlUI.style.backgroundImage = "url(/Images/Ruler.ico)";
        ScaleDiv.style.border = '2px solid #fff';
        ScaleDiv.style.borderRadius = '3px';
        // controlUI.style.boxShadow = '0 2px 6px rgba(0,0,0,.3)';
        ScaleDiv.style.cursor = 'pointer';
        ScaleDiv.style.marginRight = '16px';
        ScaleDiv.style.textAlign = 'center';
        ScaleDiv.style.width = '20px';
        ScaleDiv.title = '';
        // controlUI.style.height = '38px';
        // ScaleDiv.appendChild(controlUI);

        // Set CSS for the control interior.
        controlImg.src = '/images/ruler2.png';
        controlImg.style.height = '20px';
        ScaleDiv.appendChild(controlImg);

        ScaleDiv.addEventListener('click', function () {
            if (DistanceToggler === 0) {
                ScaleDiv.children[0].src = '/images/Ruler.ico';
                DistanceToggler = 1;
                NoFlyZone.clickable = false;
                NoFlyZone.suppressInfoWindows = true;
                DistanceDiv.hidden = false;
                // ClearDiv.hidden = false;
                controlText.innerText = '0';
                AddMarkers();
                //  NoFlyZone.Clickable = false;
                NoFlyZone.setMap(map);
            }
            else {
                ScaleDiv.children[0].src = '/images/ruler2.png';
                DistanceToggler = 0;
                DistanceDiv.hidden = true;
                ClearDiv.hidden = true;
                NoFlyZone.clickable = true;
                NoFlyZone.suppressInfoWindows = false;
                var path = rulerpoly.getPath();

                path.clear();

                // _map.controls[google.maps.ControlPosition.TOP_CENTER].pop();
                controlText.innerHTML = '0';

                deleteMarkers();
                NoFlyZone.setMap(map);
            }
        });

        _map.controls[google.maps.ControlPosition.RIGHT_TOP].push(ScaleDiv);
    };

    function setMapOnAll(map) {
        for (var i = 0; i < dismarkers.length; i++) {
            dismarkers[i].setMap(map);
        }
    }

    // Removes the markers from the map, but keeps them in the array.
    function clearMarkers() {
        setMapOnAll(null);
        ClearDiv.hidden = true;
    };

    // Deletes all markers in the array by removing references to them.
    function deleteMarkers() {
        clearMarkers();
        dismarkers = [];

    };
    return {
        InitializeScale:_InitializeScale

    };

}();