var map;
var DroneID = 0;
var isSetData = false;
$(document).ready(function () {
  initializeMap();
  initLoadTraffic();
});

function initializeMap() {
  var MarkerPosition = { lat: 25.0955354, lng: 55.1527025 };

  var mapOptions = {
    zoom: 14,
    mapTypeControl: false,
    streetViewControl: false,
    center: MarkerPosition,
    styles: getMapStyle()
  };

  map = new google.maps.Map(document.getElementById('DmatMap'), mapOptions);
}

function initLoadTraffic() {
  var URL = 'http://api.exponent-ts.com/api/trafficmonitor/?id=100&callback=jQuery';
  //var URL = 'http://localhost:63975/api/trafficmonitor/?id=100';
  var AJAX = $.ajax({
    url: URL,
    type: 'GET',
    success: function (data) {
      ShowTrafficData(data);
    }
  }).done(function() {
    window.setTimeout(initLoadTraffic, 1000);
  });//$.ajax
}

function ShowTrafficData(TheData) {
  $.each(TheData, function (key, value) {
    var SpanID = '#data-' + key.toLowerCase();
    if (!isSetData) $(SpanID).html('');
    if (key === 'CreatedOn') {
      var d = new Date(value.replace('T', ' '));      
      value = dFormat(d, true);
    }    
    setHtmlTo(SpanID, value);
  });

  var d = new Date();
  if (!isSetData) $('#data-date').html('');
  setHtmlTo('#data-date', dFormat(d, true));
  isSetData = true;
}

function setHtmlTo(SpanID, html) {
  var Span = $(SpanID);
  //hide element if already present
  var HideElem = Span.find('span.active');
  if (HideElem.length) {
    HideElem.fadeOut(300, function () {
      $(this).remove();
    });
  }
  var newElem = $('<span style="display:none">' + html + '</span>');
  Span.append(newElem);

  newElem.fadeIn(100, function () {
    $(this).addClass('active');
  });
}