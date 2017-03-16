var _ADSBLayer = null;
var RefreshTimer = null;
var LastProcessedID = null;
var ChartIndex = 0;
var UpdateDelay = 5 * 1000;
var Timers = {
  getADSB: null,
  getStatus: null,
  getChartData: null
}

$(document).ready(function () {
  initializeMap();
  InitChart();
  InitScroll();

  var spinner = $("input.spinner").spinner({
    change: function (event, ui) {
      if (Timers['getStatus']) window.clearTimeout(Timers['getStatus']);
    Timers['getStatus'] = window.setTimeout(RequestFilterData, 1 * 1000);
    //console.log("Setting Timer ID : " + RefreshTimer);
  }
  });

  var CheckBox = $('input.query').on("change", function () {
    if (Timers['getADSB']) window.clearTimeout(Timers['getADSB']);
    Timers['getADSB'] = window.setTimeout(getADSB, 1 * 1000, _ADSBLayer);
  });


  getChartData();

  Timers['getADSB'] = window.setTimeout(getADSB, UpdateDelay, _ADSBLayer);
  Timers['getStatus'] = window.setTimeout(getStatus, UpdateDelay, _ADSBLayer);
  //Timers['getChartData'] = window.setTimeout(getChartData, UpdateDelay);

});

function InitScroll() {
  var DisplayArea = 40;
  var LineHeight = 19;

  $('div.scroll-rows div.scroll-down').on("click", function () {
    var ScrollObj = $(this).parent().find('div.scroll');
    var top = ScrollObj.position().top;
    var height = ScrollObj.height();
    if (Math.abs(top) + DisplayArea >= height) return false;
    ScrollObj.animate({ top: top - LineHeight });
  });
  $('div.scroll-rows div.scroll-up').on("click", function () {
    var ScrollObj = $(this).parent().find('div.scroll');
    var top = ScrollObj.position().top;
    if (top == 0) return;
    var nTop = top + LineHeight;
    if (nTop > 0) nTop = 0;
    ScrollObj.animate({ top: nTop });
  })
}


function RequestFilterData() {
  //console.log("Running Timer ID : " + RefreshTimer);
  if (RefreshTimer) window.clearTimeout(RefreshTimer);
  RefreshTimer = null;
  
  getStatus(_ADSBLayer);
}

function initializeMap() {
  var MarkerPosition = { lat: 25.2532, lng: 55.3657 };
  
  var mapOptions = {
    zoom: 10,
    mapTypeControl: false,
    streetViewControl: false,
    center: MarkerPosition,
    styles: getMapStyle()
  };

  map = new google.maps.Map(document.getElementById('adsb_map'), mapOptions);
  _ADSBLayer = new ADSBOverlay({ map: map }, []);
  
}

function setLiveSummary(ChartData) {
  if (ChartData.length < 1) return;
  var Data = ChartData[ChartData.length - 1];
  $('#Summary-Breach24H').html(Data.Breach24H);
  $('#Summary-TotalRPAS').html(Data.TotalRPAS);
  $('#Summary-Area').html(Data.Area.toFixed(1));
}

function setChartData(ChartData) {
  //console.log("Position : " + Position + ", ChartIndex:" + ChartIndex);
  if (ChartData.length < 1) return;

  for (var i = 0; i < ChartData.length; i++) {
    ChartIndex++;


    isRemovePoints = false;
    var DataItem = ChartData[i];
    var categories = theChartSeries[0].xAxis.categories;
    var isRemovePoints = categories.length  >= 20 ? true : false;
    categories.shift;
    categories.push(DataItem.SummaryDate);

    var BarColor = 'red';
    var theData = {
      y: DataItem.Alert,
      categories
    }; //{ y: , x: ], color: 'yellow' };

    LastProcessedID = DataItem.ID;
    //theChartSeries[0].xAxis.categories = categories;
    theChartSeries[1].addPoint(theData, false, isRemovePoints, false);
    theChartSeries[0].addPoint([categories, DataItem.Breach], true, isRemovePoints, true);
  }

}


function getChartData() {
  //download data from the server
  var URL = '/Adsb/Summary?LastProcessedID=' + LastProcessedID;
  var AJAX = $.ajax({
    url: URL,
    type: 'GET',
    success: function (data) {
      setChartData(data);
      setLiveSummary(data);
    }, //succes
    complete: function () {
      Timers['getChartData'] = window.setTimeout(getChartData, 30 * 1000);
    }    
  });//$.ajax
}


function InitChart() {
  var GeneralStyle = {
    color: '#222222',
    fontSize: '8px'
  };

  theChart = $('#BarGraph').highcharts({
    chart: {
      spacing: [5,0,0,0],
      renderTo: 'container',
      backgroundColor: 'white',
      events: {
        load: function () {
          // set up the updating of the chart each second
          theChartSeries = this.series;
        }
      }
    },

    credits: {
      enabled: false
    },
    title: {
      text: ''
    },
    subtitle: {
      text: ''
    },
    exporting: {
      enabled: false
    },
    plotOptions: {
      area: {
        marker: {
          enabled: false,
          symbol: 'circle',
          radius: 2,
          states: {
            hover: {
              enabled: true
            }
          }
        }
      }
    },
    xAxis: [{
      categories: [],
      labels: {
        distance: 0,
        rotation: 0,
        style: GeneralStyle,
      },

    }],
    yAxis: [{ // Primary yAxis
      labels: {
        format: '{value}',
        style: GeneralStyle
      },
      title: {
        text: ''
      },
      gridLineColor: '#6c8393',
      lineColor: '#222222',
      lineWidth: 2
    }, { // Secondary yAxis
      lineColor: '#222222',
      lineWidth: 2,

      title: {
        text: '',
      },
      labels: {
        format: '{value}',
        style: GeneralStyle
      },
      opposite: true,
      gridLineColor: '#6c8393',
      gridLineWidth: 0
    }],
    tooltip: {
      shared: true
    },
    legend: {
      enabled: false
    },


    series: [ {
      name: 'Alerts',
      type: 'area',
      data: [],
      color: '#E7E129',
      lineWidth: 1,
      fillOpacity: 0.5
    }, {
      name: 'Breaches',
      type: 'area',
      data: [],
      color: '#fe0000',
      lineWidth: 1,
      fillOpacity: 0.5

    }]
  });


}




function getMapStyle() {
  return [
  {
    "elementType": "geometry",
    "stylers": [
      {
        "color": "#212121"
      }
    ]
  },
  {
    "elementType": "labels.icon",
    "stylers": [
      {
        "visibility": "off"
      }
    ]
  },
  {
    "elementType": "labels.text.fill",
    "stylers": [
      {
        "color": "#757575"
      }
    ]
  },
  {
    "elementType": "labels.text.stroke",
    "stylers": [
      {
        "color": "#212121"
      }
    ]
  },
  {
    "featureType": "administrative",
    "elementType": "geometry",
    "stylers": [
      {
        "color": "#757575"
      }
    ]
  },
  {
    "featureType": "administrative.country",
    "elementType": "labels.text.fill",
    "stylers": [
      {
        "color": "#9e9e9e"
      }
    ]
  },
  {
    "featureType": "administrative.land_parcel",
    "stylers": [
      {
        "visibility": "off"
      }
    ]
  },
  {
    "featureType": "administrative.locality",
    "elementType": "labels.text.fill",
    "stylers": [
      {
        "color": "#bdbdbd"
      }
    ]
  },
  {
    "featureType": "administrative.neighborhood",
    "stylers": [
      {
        "visibility": "off"
      }
    ]
  },
  {
    "featureType": "landscape",
    "stylers": [
      {
        "color": "#5c5c5c"
      }
    ]
  },
  {
    "featureType": "poi",
    "elementType": "labels.text",
    "stylers": [
      {
        "visibility": "off"
      }
    ]
  },
  {
    "featureType": "poi",
    "elementType": "labels.text.fill",
    "stylers": [
      {
        "color": "#757575"
      }
    ]
  },
  {
    "featureType": "poi.business",
    "stylers": [
      {
        "visibility": "off"
      }
    ]
  },
  {
    "featureType": "poi.park",
    "elementType": "geometry",
    "stylers": [
      {
        "color": "#181818"
      }
    ]
  },
  {
    "featureType": "poi.park",
    "elementType": "labels.text",
    "stylers": [
      {
        "visibility": "off"
      }
    ]
  },
  {
    "featureType": "poi.park",
    "elementType": "labels.text.fill",
    "stylers": [
      {
        "color": "#616161"
      }
    ]
  },
  {
    "featureType": "poi.park",
    "elementType": "labels.text.stroke",
    "stylers": [
      {
        "color": "#1b1b1b"
      }
    ]
  },
  {
    "featureType": "road",
    "elementType": "geometry.fill",
    "stylers": [
      {
        "color": "#2c2c2c"
      }
    ]
  },
  {
    "featureType": "road",
    "elementType": "labels",
    "stylers": [
      {
        "visibility": "off"
      }
    ]
  },
  {
    "featureType": "road",
    "elementType": "labels.text.fill",
    "stylers": [
      {
        "color": "#8a8a8a"
      }
    ]
  },
  {
    "featureType": "road.arterial",
    "elementType": "geometry",
    "stylers": [
      {
        "color": "#373737"
      }
    ]
  },
  {
    "featureType": "road.arterial",
    "elementType": "labels",
    "stylers": [
      {
        "visibility": "off"
      }
    ]
  },
  {
    "featureType": "road.highway",
    "elementType": "geometry",
    "stylers": [
      {
        "color": "#3c3c3c"
      }
    ]
  },
  {
    "featureType": "road.highway",
    "elementType": "labels",
    "stylers": [
      {
        "visibility": "off"
      }
    ]
  },
  {
    "featureType": "road.highway.controlled_access",
    "elementType": "geometry",
    "stylers": [
      {
        "color": "#4e4e4e"
      }
    ]
  },
  {
    "featureType": "road.local",
    "stylers": [
      {
        "visibility": "off"
      }
    ]
  },
  {
    "featureType": "road.local",
    "elementType": "labels.text.fill",
    "stylers": [
      {
        "color": "#616161"
      }
    ]
  },
  {
    "featureType": "transit",
    "elementType": "labels.text.fill",
    "stylers": [
      {
        "color": "#757575"
      }
    ]
  },
  {
    "featureType": "water",
    "elementType": "geometry",
    "stylers": [
      {
        "color": "#222222"
      }
    ]
  },
  {
    "featureType": "water",
    "elementType": "labels.text",
    "stylers": [
      {
        "visibility": "off"
      }
    ]
  },
  {
    "featureType": "water",
    "elementType": "labels.text.fill",
    "stylers": [
      {
        "color": "#3d3d3d"
      }
    ]
  }
  ];
}