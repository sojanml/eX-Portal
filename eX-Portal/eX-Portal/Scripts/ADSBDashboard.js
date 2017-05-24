var _ADSBLayer = null;
var RefreshTimer = null;
var LastProcessedID = null;
var ChartIndex = 0;
var UpdateDelay = 1 * 1000;
var IsQueryChanged = 0;
var timeZoneOffset = 0; //(new Date()).getTimezoneOffset();

var Timers = {
  getADSB: null,
  getStatus: null,
  getChartData: null
}

$(document).ready(function () {
  initializeMap();
  InitChart();
  InitScroll();
  InitSliders();

  var spinner = $("input.spinner").spinner({
    change: function (event, ui) {
      if (Timers['getStatus']) window.clearTimeout(Timers['getStatus']);
      Timers['getStatus'] = window.setTimeout(RequestFilterData, 1 * 1000);

      AutoUpdateZone($(this));
      IsQueryChanged = 1;
      //console.log("Setting Timer ID : " + RefreshTimer);
    }
  });

  var CheckBox = $('input.query').on("change", function () {
    if (Timers['getADSB']) window.clearTimeout(Timers['getADSB']);
    Timers['getADSB'] = window.setTimeout(getADSB, 1 * 1000, _ADSBLayer);
    IsQueryChanged = 1;
    //d.getTimezoneOffset()
  });

  $('#TimeZone').on("change", function () {
    var d = new Date();
    LastProcessedID = 0;
    timeZoneOffset = ($(this).val() == "utc" ? 0 : d.getTimezoneOffset());
    if (Timers['getChartData']) window.clearTimeout(Timers['getChartData']);
    Timers['getChartData'] = window.setTimeout(getChartData, 100);    
  });
  

  Timers['getADSB'] = window.setTimeout(getADSB, UpdateDelay, _ADSBLayer);
  //Timers['getStatus'] = window.setTimeout(getStatus, UpdateDelay, _ADSBLayer);
  Timers['getChartData'] = window.setTimeout(getChartData, 100);


});

function InitSliders() {

  $("#SliderAltitude").slider(
    {
      range: true,
      values: [InitQuery.minAltitude, InitQuery.maxAltitude],
      min: 0,
      max: 6000,
      step:1,
      slide: function (event, ui) {
        $('#span_minAltitude').html(ui.values[0]);
        $('#span_maxAltitude').html(ui.values[1]);
        $('#minAltitude').val(ui.values[0]);
        $('#maxAltitude').val(ui.values[1]);
        ReGetAdsb();
      }
    }
  );

  $("#SliderSpeed").slider({
    range: true,
    values: [InitQuery.minSpeed, InitQuery.maxSpeed],
    min: 0,
    max: 50,
    step: 0.5,
    slide: function (event, ui) {
      $('#span_minSpeed').html(ui.values[0].toFixed(1));
      $('#span_maxSpeed').html(ui.values[1].toFixed(1));
      $('#minSpeed').val(ui.values[0]);
      $('#maxSpeed').val(ui.values[1]);     
      ReGetAdsb();
    }
  });


}

function ReGetAdsb() {
  IsQueryChanged = 1;
  if (Timers['getADSB']) window.clearTimeout(Timers['getADSB']);
  Timers['getADSB'] = window.setTimeout(getADSB, 1 * 1000, _ADSBLayer);
}

function AutoUpdateZone(Elem) {
  if (Elem.length < 1) return;
  var Value = parseFloat(Elem.val());
  if (isNaN(Value)) Value = 0;
  var AlertPercentage = 75;
  var BreachPercentage = 50;
  switch (Elem[0].id) {
    case 'hSafe':
      if (Value == 0) Value = 10;
      $('#hAlert').val((Value * AlertPercentage / 100).toFixed(1));
      $('#hBreach').val((Value * BreachPercentage / 100).toFixed(1));
      break;
      case 'hAlert':
          if (Value == 0) Value = 8;
          var Safe = Value * (1 / AlertPercentage) * 100;
          var Breach = Safe * BreachPercentage / 100;
          $('#hSafe').val(Safe.toFixed(1));
          $('#hBreach').val(Breach.toFixed(1));
          break;
      case 'hBreach':
          if (Value == 0) Value = 5;
          var Safe = Value * (1 / BreachPercentage) * 100;
          var Alert = Safe * AlertPercentage / 100;
          $('#hSafe').val(Safe.toFixed(1));
          $('#hAlert').val(Alert.toFixed(1));
          break;
      case 'vSafe':
          if (Value == 0) Value = 10;
          $('#vAlert').val((Value * AlertPercentage / 100).toFixed(1));
          $('#vBreach').val((Value * BreachPercentage / 100).toFixed(1));
          break;
      case 'vAlert':
          if (Value == 0) Value = 8;
          var Safe = Value * (1 / AlertPercentage) * 100;
          var Breach = Safe * BreachPercentage / 100;
          $('#vSafe').val(Safe.toFixed(1));
          $('#vBreach').val(Breach.toFixed(1));
      case 'vBreach':
          if (Value == 0) Value = 5;
          var Safe = Value * (1 / BreachPercentage) * 100;
          var Alert = Safe * AlertPercentage / 100;
          $('#vSafe').val(Safe.toFixed(1));
          $('#vAlert').val(Alert.toFixed(1));
  }
}




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

function setLiveSummary(ChartData) {
  if (ChartData.length < 1) return;
  var Data = ChartData[ChartData.length - 1];
  $('#Summary-Breach24H').html(Data.Breach24H);
  $('#Summary-TotalRPAS').html(Data.TotalRPAS);
  $('#Summary-Area').html(Data.Area.toFixed(0));
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
      y: DataItem.AircraftBreach,
      categories
    }; //{ y: , x: ], color: 'yellow' };

    LastProcessedID = DataItem.ID;
    //theChartSeries[0].xAxis.categories = categories;
    theChartSeries[1].addPoint(theData, false, isRemovePoints, false);
    theChartSeries[0].addPoint([categories, DataItem.AircraftAlert], true, isRemovePoints, true);
  }

}


function getChartData() {
  //download data from the server
  var d = new Date();
  var URL = '/Adsb/Summary?LastProcessedID=' + LastProcessedID + '&TimezoneOffset=' + (-1 * timeZoneOffset);
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
    },
    { // Secondary yAxis
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
        }    ],

    tooltip: {
      shared: true
    },
    legend: {
      enabled: false
    },


    series: [ {
      name: 'Alerts',
      type: 'areaspline',
      data: [],
      color: '#ff4e00',
      lineWidth: 1,
     // fillOpacity: 0.5
    }, {
      name: 'Breaches',
      type: 'areaspline',
      data: [],
      color: '#fe0000',
      lineWidth: 1,
    //  fillOpacity: 0.5

    }]
  });


}


