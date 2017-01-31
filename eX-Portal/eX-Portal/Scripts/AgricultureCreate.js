var map = null;
var LandLocation = { lat: 25.0955354, lng: 55.1527025 };
var LandLocationMarker = null;

$(document).ready(function () {
  initializeMap();

  if (jQuery.ui) $('input.date-picker').datepicker({
    dateFormat: 'dd-M-yy',
    changeYear: true,
    changeMonth: true
  });

  $('#frmEdit').on("submit", SubmitForm)

});

function SubmitForm(e) {
  e.preventDefault();
  e.stopPropagation();

  var isValid = $('#frmEdit').valid();
  if (!isValid) return false;

  var FormData = $('#frmEdit').serialize();
  $('#frmEdit').prop("disabled", true);
  $('#lblStatus').html("Saving Customer. Please Wait...");

  $.ajax({
    method: "POST",
    url: "/Agriculture/Create",
    data: FormData,
    dataType: 'json'
  })
  .done(function (Result) {
    switch (Result.Status) {
      case 'ok':
        $('#lblStatus').html("Done. Redirecting to Images...");
        top.location.href = '/Agriculture/Images/' + Result.ID;
        break;
      default:
        $('#lblValidateSummary').html(toList(Result.Message));
        $('#internal_scroll').scrollTop(0);
        $('#lblStatus').html("");
        break;
    }
  });

}

function toList(TheList) {
  var List = '<UL id="ValidationError">';
  for (var i = 0; i < TheList.length; i++) {
    List += '<Li>' + TheList[i] + '</LI>\n';
  }
  List += '</UL>'
  return List;
}

function initializeMap() {
  $('#Lat').val(LandLocation['lat'].toFixed(5));
  $('#Lng').val(LandLocation['lng'].toFixed(5));

  var mapOptions = {
    zoom: 14,
    mapTypeControl: false,
    streetViewControl: false,
    center: LandLocation,
    styles: getMapStyle()
  };

  map = new google.maps.Map(document.getElementById('AgricultureMap'), mapOptions);
  LandLocationMarker = new google.maps.Marker({
    position: LandLocation,
    map: map,
    draggable: true,
    title: 'Land Location'
  });

  google.maps.event.addListener(LandLocationMarker, "dragend", function (event) {
    var lat = event.latLng.lat() + 0;
    var lng = event.latLng.lng() + 0;
    LandLocation = { lat: lat, lng: lng };
    $('#Lat').val(lat.toFixed(5));
    $('#Lng').val(lng.toFixed(5));
  });

  $('#Lat').on("change", OnLandLocationChange);
  $('#Lng').on("change", OnLandLocationChange);
}

function OnLandLocationChange() {
  LandLocation = getNewLandLocation();
  LandLocationMarker.setPosition(LandLocation);
  map.setCenter(LandLocation);
}

function getNewLandLocation() {
  var Lat = $('#Lat').val();
  var Lng = $('#Lng').val();
  Lat = parseFloat(Lat);
  Lng = parseFloat(Lng);
  if (isNaN(Lat)) Lat = 25.0955354;
  if (isNaN(Lng)) Lng = 55.1527025;
  return { lat: Lat, lng: Lng }

}
