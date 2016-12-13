function getADSB(ADSBObj){
  $.ajax({
    type: "GET",
    url: '/ADSB/',
    contentType: "application/json",
    success: function(data) {
      ADSBObj.setADSB(data);      
    },
    error: function (data, text) {
      //alert('Failed to fetch flight: ' + data);
    }
  });
}

function ADSBOverlay(options, ADSBData) {
  this.setValues(options);
  this.markerLayer = $('<div />').addClass('overlay');
  this.ADSBData = ADSBData;
  this.gADSBData = {};
  this.DrawCount = 0;
  this.setADSB = function (ADSBData) {
    this.ADSBData = ADSBData;
    this.DrawCount++;
    this.draw();
  }


  this.getIconColor = function (Height) {
    var Colors = [
    '#FF0000', //Red
    '#FF9600', //Yellow
    '#00ff10' //Green
    ];
    if (Height < 1000) {
      return Colors[0];
    } else if (Height < 2000) {
      return Colors[1];
    } else {
      return Colors[2];
    }
  }

  this.getIconClass = function (Height) {
    if (Height < 1000) {
      return 'breach';
    } else if (Height < 2000) {
      return 'alert';
    } else {
      return 'safe';
    }
  }


};

ADSBOverlay.prototype = new google.maps.OverlayView;


ADSBOverlay.prototype.onAdd = function () {
  var $pane = $(this.getPanes().overlayImage); // Pane 3  
  $pane.append(this.markerLayer);
};

ADSBOverlay.prototype.onRemove = function () {
  this.markerLayer.remove();
};

ADSBOverlay.prototype.draw = function () {
  var projection = this.getProjection();
  if (!projection) return false;
  this.DrawCount++;

  for (var i = 0; i < this.ADSBData.length; i++) {
    var lat = this.ADSBData[i].Lat;
    var lng = this.ADSBData[i].Lon;
    var alt = this.ADSBData[i].Altitude;
    var title = this.ADSBData[i].FlightID.trim();
    var heading = this.ADSBData[i].Heading;
    heading = parseFloat(heading);
    if (isNaN(heading)) heading = 0;

    // Determine a random location from the bounds set previously  
    var IconGeoPos = new google.maps.LatLng(lat, lng)
    var IconLocation = projection.fromLatLngToDivPixel(IconGeoPos);
    var DivID = 'adsb-' + title;
    var IconClass = this.getIconClass(alt);
    var $point = $('#' + DivID);
    var RotateAngle = 45;

    if (heading == 0) {
      //Landed flight - Ignore movement
    } else if (this.gADSBData[DivID]) {      
      $point.animate({ left: IconLocation.x, top: IconLocation.y });
      $point.attr('class', 'adsb-point ' + IconClass);
      $point.find(".icon").css({ transform: 'rotate(' + (heading - RotateAngle) + 'deg)' });
      $point.attr({ 'data-lat': lat, 'data-lng': lng, 'data-alt': alt });
    } else {
      var $NewPoint = $(
          '<div  class="adsb-point ' + IconClass + '" id="' + DivID + '" title="' + title + '" '
        + 'data-lat="' + lat + '" '
        + 'data-lng="' + lng + '" '
        + 'data-alt="' + alt + '" '
        + 'data-ident="' + title + '" '
        + 'style="left:' + IconLocation.x + 'px; top:' + IconLocation.y + 'px;">'
        + '<span class="icon FlightIcon" style=" transform: rotate(' + (heading - RotateAngle) + 'deg);">&#xf072;</span>'
        + '<span class="flight-title" style="">' + title + '</span>' +
        + '</div>'
      );
      // Append the HTML to the fragment in memory  
      this.markerLayer.append($NewPoint.get(0));
      $point = $('#' + DivID);
    }

    var TailHTML = getTail(this.ADSBData[i]);
    $point.find('.tail').remove();
    $point.append(TailHTML);
    
    this.gADSBData[DivID] = this.DrawCount;




  }//for (var i = 0)

  //if the item does not exists in 20 request, then remove it from the screen
  var AllKeys = Object.keys(this.gADSBData);
  for (i = 0; i <= AllKeys.length; i++) {
    var TheKey = AllKeys[i];
    if (this.gADSBData[TheKey] != this.DrawCount ) {
      $('#' + TheKey).remove();
      $('div.' + DivID + '-history').remove();
      delete this.gADSBData[TheKey];
    }//if (gADSBData[key] > 20) 
  }//for (var key in gADSBData)



};



function getTail(ADSBData) {
  var HTML = '';
  var History = ADSBData.History;
  if (History.length < 1) History.push(ADSBData.Heading);

  //If the history length is less than 5, push the last pos untill it reaches 5
  var LastHeading = History[History.length - 1];
  for (var i = History.length - 1; i < 5; i++) {
    History.push(LastHeading);
  }

  //ok. now generate html for each point
  var Center = 50;
  var Distance = 15;
  for (var i = 1; i < History.length; i++) {
    var xy = lineToAngle(Center, Center, Distance, History[i]);
    HTML += '<span class="tail" style="left:' + xy.x + 'px; top:' + xy.y + 'px"></span>';
    Distance += 4;
  }

  return HTML;
}



function lineToAngle(x1, y1, length, angle) {

  angle = angle + 90;
  if (angle > 360) angle = angle - 360;

  angle *= Math.PI / 180;

  var x2 = x1 + length * Math.cos(angle),
      y2 = y1 + length * Math.sin(angle);


  return { x: x2, y: y2 };
}