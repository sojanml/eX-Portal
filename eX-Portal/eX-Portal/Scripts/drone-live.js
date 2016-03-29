var RefreshID = 0;
var LiveDrones = {};

$(document).ready(initDroneLive);

function initDroneLive() {
  var RequestURL = '/Drone/LiveData';
  $.ajax({
    method: 'GET',
    contentType: 'text/json',
    url: RequestURL
  })
  .done(DroneLivePopulate)
  .fail(function (jqXHR, textStatus, errorThrown) {
    alert(textStatus + "\n\n" + errorThrown);
  })
}

function DroneLivePopulate(data, textStatus, jqXHR) {
  if (RefreshID == 0) {
    $('#live-drones-load').slideUp();
  }
  RefreshID++;

  data.forEach(function (element, index, array) {
    var HTML = getTile(element);
    var Elem = typeof LiveDrones[element.DroneID] === "undefined" ?
      $('<LI style="display:none" id="uas-' + element.DroneID + '"></LI>') :
      $('#uas-' + element.DroneID);
    
    Elem.html(HTML);
    if (!LiveDrones[element.DroneID]) $('#live-drones').append(Elem);
    Elem.fadeIn();

    LiveDrones[element.DroneID] = RefreshID;
  });
}

function getTile(Drone) {
  var HTML = 
  '<div class="heading">' + Drone.DroneName + '</div>\n' +
  '<div class="location">' + getLocation(Drone) + '</div>\n';
  return HTML
}

function getLocation(Drone) {
  var HTML = 
  Drone.Latitude.toFixed(3) + '&deg;' + 
  (Drone.Latitude >= 0 ? 'N' : 'S') +
  ' ' +
  Drone.Longitude.toFixed(3) + '&deg;' +
  (Drone.Longitude >= 0 ? 'E' : 'W');

  return HTML;
}