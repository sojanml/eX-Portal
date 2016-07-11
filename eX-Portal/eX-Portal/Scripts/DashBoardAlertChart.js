$(document).ready(function () {

  $.ajax({
    type: "GET",
    url: DashBoardAlertChartDataURL,
    contentType: "application/json;charset=utf-8",
    dataType: "json",
    async: true,
    success: OnSuccess_,
    error: OnErrorCall_
  });

  function OnSuccess_(reponse) {

    var aData = reponse;
    var TotalAlerts = {};
    var LastFlightAlert = {};
    var ThisMonth = {};

    for (var i = 0; i < aData.length; i++) {
      var data = aData[i];
      TotalAlerts[data.AlertType] = data.TotalAlert;
      LastFlightAlert[data.AlertType] = data.LastFlightAlert;
      ThisMonth[data.AlertType] = data.CurrentMonthAlert;
    }

    var ctx1 = $("canvas#TotalAlerts").get(0).getContext('2d');
    var data1 = getChartData(TotalAlerts);
    ctx1.canvas.height = 210;  // setting height of canvas
    ctx1.canvas.width = 250; // setting width of canvas
    var PieChartAltitude1 = new Chart(ctx1).Pie(data1, getChartOptions())

    var ctx2 = $("canvas#ThisMonthAlerts").get(0).getContext('2d');
    var data2 = getChartData(ThisMonth);
    ctx2.canvas.height = 210;  // setting height of canvas
    ctx2.canvas.width = 250; // setting width of canvas
    if (data2.length < 1) {
      ctx2.beginPath();
      ctx2.arc(125, 105, 105, 0, 2 * Math.PI, false);
      ctx2.fillStyle = 'rgb(230,230,230)';
      ctx2.fill();
    } else {
      var PieChartAltitude2 = new Chart(ctx2).Pie(data2, getChartOptions())
    }

    var ctx3 = $("canvas#LastFlightAlerts").get(0).getContext('2d');
    ctx3.canvas.height = 210;  // setting height of canvas
    ctx3.canvas.width = 250; // setting width of canvas
    var PieChartAltitude3 = new Chart(ctx3).Pie(getChartData(LastFlightAlert), getChartOptions())

  };

  function getChartData(TheData) {

    if (TheData.Height == 0 && TheData.Boundary == 0 && TheData.Proximity == 0) {
      var Chart_Data = [];
      return Chart_Data;

    }

    var Chart_Data = [
      {
        label: "Altitude",
        value: TheData.Height,
        color: "rgba(151,187,205,0.5)",
      }, {
        label: "Boundary",
        value: TheData.Boundary,
        color: "#FF6384",
      }, {
        label: "Proximity",
        value: TheData.Proximity,
        color: "#FFCE56",
      }
    ];


    return Chart_Data;
  }

  function getChartOptions() {
    return {
      segmentShowStroke: true,
      segmentStrokeColor: "#fff",
      segmentStrokeWidth: 0,
      animation: true,
      animationSteps: 200,
      animationEasing: "easeOutBounce",
      animateRotate: true,
      animateScale: false,
      onAnimationComplete: null,
      labelFontFamily: "'Arial'",
      labelFontStyle: "normal",
      labelFontSize: 12,
      labelFontColor: "white",
      labelVisible: true,
      onAnimationComplete: null
    };
  }

  function OnErrorCall_(repo) {
    alert("Woops something went wrong, pls try later !");
  }
});