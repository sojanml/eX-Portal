function getFileTitle(FileName) {
  FileName = FileName.replace('.t.png', '');
  var LastHypon = FileName.lastIndexOf("-");
  if (LastHypon > 0) return FileName.substr(LastHypon + 1);
  return FileName;
}

function dtConvFromJSON(sNetDate, isAddTime = false) {
  var sDate = '';
  if (sNetDate === null) return "N/A";
  var r = /\/Date\(([0-9]+)\)\//i
  var matches = sNetDate.match(r);
  if (matches.length === 2) {
    var tDate = new Date(parseInt(matches[1]));
    tDate.setMinutes(tDate.getMinutes() + tDate.getTimezoneOffset());
    sDate = dFormat(tDate, isAddTime )
  } else {
    sDate =  "N/A";
  }
  return sDate;
}

function dtFromJSON(sNetDate) {
  if (sNetDate === null) return new Date();
  var r = /\/Date\(([0-9]+)\)\//i
  var matches = sNetDate.match(r);
  if (matches.length === 2) {
    return new Date(parseInt(matches[1]));
  } 
  return new Date();
}

function ToLocalTime(TheDate) {
  //TheDate.setMinutes(TheDate.getMinutes() + TheDate.getTimezoneOffset());
  return TheDate;
}


function dFormat(tDate, isAddTime) {
  var Months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']
  sDate = (tDate.getDate()) + '-' + Months[tDate.getMonth()] + '-' + tDate.getFullYear();
  if (isAddTime) {
    sDate = sDate + ' ' + pad(tDate.getHours()) + ":" + pad(tDate.getMinutes()) + ':' + pad(tDate.getSeconds());
  }

  return sDate;
}

function nFormat(n, dp) {
  var w = n.toFixed(dp), k = w | 0, b = n < 0 ? 1 : 0,
      u = Math.abs(w - k), d = ('' + u.toFixed(dp)).substr(2, dp),
      s = '' + k, i = s.length, r = '';
  while ((i -= 3) > b) { r = ',' + s.substr(i, 3) + r; }
  return s.substr(0, i + 3) + r + (d ? '.' + d : '');
};



function pad(Num) {
  if (Num >= 10) return Num;
  return '0' + Num;

}

function getMapStyle() {
  return [{
    "elementType": "all",
    "featureType": "all",
    "stylers": [{ "visibility": "on" }]
  }, {
    "elementType": "labels",
    "featureType": "all",
    "stylers": [{ "visibility": "off" },
        { "saturation": "-100" }
    ]
  }, {
    "elementType": "labels.text.fill",
    "featureType": "all",
    "stylers": [{ "saturation": 36 },
        { "color": "#000000" },
        { "lightness": 40 },
        { "visibility": "off" }
    ]
  }, {
    "elementType": "labels.text.stroke",
    "featureType": "all",
    "stylers": [{ "visibility": "off" },
        { "color": "#000000" },
        { "lightness": 16 }
    ]
  }, {
    "elementType": "labels.icon",
    "featureType": "all",
    "stylers": [{ "visibility": "off" }]
  }, {
    "elementType": "geometry.fill",
    "featureType": "administrative",
    "stylers": [{ "color": "#000000" },
        { "lightness": 20 }
    ]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "administrative",
    "stylers": [{ "color": "#000000" },
        { "lightness": 17 },
        { "weight": 1.2 }
    ]
  }, {
    "elementType": "geometry",
    "featureType": "landscape",
    "stylers": [{ "color": "#000000" },
        { "lightness": 20 }
    ]
  }, {
    "elementType": "geometry.fill",
    "featureType": "landscape",
    "stylers": [{ "color": "#4d6059" }]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "landscape",
    "stylers": [{ "color": "#4d6059" }]
  }, {
    "elementType": "geometry.fill",
    "featureType": "landscape.natural",
    "stylers": [{ "color": "#4d6059" }]
  }, {
    "elementType": "geometry",
    "featureType": "poi",
    "stylers": [{ "lightness": 21 }]
  }, {
    "elementType": "geometry.fill",
    "featureType": "poi",
    "stylers": [{ "color": "#4d6059" }]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "poi",
    "stylers": [{ "color": "#4d6059" }]
  }, {
    "elementType": "geometry",
    "featureType": "road",
    "stylers": [{ "visibility": "on" },
        { "color": "#7f8d89" }
    ]
  }, {
    "elementType": "geometry.fill",
    "featureType": "road",
    "stylers": [{ "color": "#7f8d89" }]
  }, {
    "elementType": "geometry.fill",
    "featureType": "road.highway",
    "stylers": [{ "color": "#7f8d89" },
        { "lightness": 17 }
    ]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "road.highway",
    "stylers": [{ "color": "#7f8d89" },
        { "lightness": 29 },
        { "weight": 0.20000000000000001 }
    ]
  }, {
    "elementType": "geometry",
    "featureType": "road.arterial",
    "stylers": [{ "color": "#000000" },
        { "lightness": 18 }
    ]
  }, {
    "elementType": "geometry.fill",
    "featureType": "road.arterial",
    "stylers": [{ "color": "#7f8d89" }]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "road.arterial",
    "stylers": [{ "color": "#7f8d89" }]
  }, {
    "elementType": "geometry",
    "featureType": "road.local",
    "stylers": [{ "color": "#000000" },
        { "lightness": 16 }
    ]
  }, {
    "elementType": "geometry.fill",
    "featureType": "road.local",
    "stylers": [{ "color": "#7f8d89" }]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "road.local",
    "stylers": [{ "color": "#7f8d89" }]
  }, {
    "elementType": "geometry",
    "featureType": "transit",
    "stylers": [{ "color": "#000000" },
        { "lightness": 19 }
    ]
  }, {
    "elementType": "all",
    "featureType": "water",
    "stylers": [{ "color": "#2b3638" },
        { "visibility": "on" }
    ]
  }, {
    "elementType": "geometry",
    "featureType": "water",
    "stylers": [{ "color": "#2b3638" },
        { "lightness": 17 }
    ]
  }, {
    "elementType": "geometry.fill",
    "featureType": "water",
    "stylers": [{ "color": "#24282b" }]
  }, {
    "elementType": "geometry.stroke",
    "featureType": "water",
    "stylers": [{ "color": "#24282b" }]
  }, {
    "elementType": "labels",
    "featureType": "water",
    "stylers": [{ "visibility": "off" }]
  }, {
    "elementType": "labels.text",
    "featureType": "water",
    "stylers": [{ "visibility": "off" }]
  }, {
    "elementType": "labels.text.fill",
    "featureType": "water",
    "stylers": [{ "visibility": "off" }]
  }, {
    "elementType": "labels.text.stroke",
    "featureType": "water",
    "stylers": [{ "visibility": "off" }]
  }, {
    "elementType": "labels.icon",
    "featureType": "water",
    "stylers": [{ "visibility": "off" }]
  }
  ];
}



