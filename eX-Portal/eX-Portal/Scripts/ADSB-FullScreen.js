var _ADSBLayer = null;
var RefreshTimer = null;
var LastProcessedID = null;
var ChartIndex = 0;
var UpdateDelay = 1 * 1000;
var IsQueryChanged = 0;
var timeZoneOffset = 0; //(new Date()).getTimezoneOffset();
var TheChartObject = null;

var Timers = {
  getADSB: null,
  getChartData: null
}

$(document).ready(function () {
  initializeMap();
  
  Timers['getADSB'] = window.setTimeout(getADSB, 100, _ADSBLayer);

});


function ReGetAdsb() {
  IsQueryChanged = 1;
  if (Timers['getADSB']) window.clearTimeout(Timers['getADSB']);
  Timers['getADSB'] = window.setTimeout(getADSB, 400, _ADSBLayer);
}


function RequestFilterData() {
  //console.log("Running Timer ID : " + RefreshTimer);
  if (RefreshTimer) window.clearTimeout(RefreshTimer);
  RefreshTimer = null;
  
  //getStatus(_ADSBLayer);
}

function initializeMap() {
  var MarkerPosition = { lat: 25.2532, lng: 55.3657 };
  
  var mapOptions = {
    zoom: 10,
    mapTypeControl: true,
    streetViewControl: false,
    center: MarkerPosition,
    styles: getADSBMapStyle()
  };

  map = new google.maps.Map(document.getElementById('adsb_map'), mapOptions);
  _ADSBLayer = new ADSBOverlay({ map: map }, []);
  
}


