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
    var TotalAlerts = {
      Height: 0,
      Boundary: 0,
      Proximity: 0
    };
    var LastFlightAlert = {
      Height: 0,
      Boundary: 0,
      Proximity: 0
    };
    var ThisMonth = {
      Height: 0,
      Boundary: 0,
      Proximity: 0
    };
    var Sum = {
      TotalAlert: 0,
      LastFlightAlert: 0,
      CurrentMonthAlert: 0
    };

    for (var i = 0; i < aData.length; i++) {
      var data = aData[i];
      TotalAlerts[data.AlertType] = data.TotalAlert;
      LastFlightAlert[data.AlertType] = data.LastFlightAlert;
      ThisMonth[data.AlertType] = data.CurrentMonthAlert;

      switch (data.AlertType) {
        case "Height":
        case "Boundary":
        case "Proximity":
          
          Sum.TotalAlert += data.TotalAlert;
          Sum.LastFlightAlert += data.LastFlightAlert;
          Sum.CurrentMonthAlert += data.CurrentMonthAlert;
          break;
      }
    }


    var ctx1 = $("canvas#TotalAlerts").get(0).getContext('2d');
   var data1 = getChartData(TotalAlerts);
   
    ctx1.canvas.height = 210;  // setting height of canvas
    ctx1.canvas.width = 250; // setting width of canvas
    if (data1.length < 1) {
      drawCircle(ctx1);
    } else {
      var PieChartAltitude1 = new Chart(ctx1).Pie(data1, getChartOptions())
    }
  

    var ctx2 = $("canvas#ThisMonthAlerts").get(0).getContext('2d');
    var data2 = getChartData(ThisMonth);  
    ctx2.canvas.height = 210;  // setting height of canvas
    ctx2.canvas.width = 250; // setting width of canvas
    if (data2.length < 1) {
      drawCircle(ctx2);
    } else {
      var PieChartAltitude2 = new Chart(ctx2).Pie(data2, getChartOptions())
    }

    var ctx3 = $("canvas#LastFlightAlerts").get(0).getContext('2d');
    var data3 = getChartData(LastFlightAlert);   
    ctx3.canvas.height = 210;  // setting height of canvas
    ctx3.canvas.width = 250; // setting width of canvas
    if (data3.length < 1) {
      drawCircle(ctx3);
    } else {
      var PieChartAltitude3 = new Chart(ctx3).Pie(data3, getChartOptions())
    }
    

    $('#TotalAlerts_Sum').html("<span>" + Sum.TotalAlert + "</span> Alerts");
    $('#ThisMonthAlerts_Sum').html("<span>" + Sum.CurrentMonthAlert + "</span> Alerts");
    $('#LastFlightAlerts_Sum').html("<span>" + Sum.LastFlightAlert + "</span> Alerts");

  };

  function drawCircle(context) {
    context.beginPath();
    context.arc(125, 105, 105, 0, 2 * Math.PI, false);
    context.fillStyle = 'rgb(230,230,230)';
    context.fill();
  }

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
      //segmentShowStroke: true,
      //segmentStrokeColor: "/#fff",
     // segmentStrokeWidth:3,
      //animation: true,
  
    //  onAnimationComplete: null,
      labelFontFamily: "'Arial'",
      labelFontStyle: "normal",
      labelFontSize: 6,
      labelFontColor: "white",
      labelVisible: true,
     // segmentShowStroke: true,
    //  segmentStrokeColor: "#fff",     
     // animationSteps: 200,
     // animationEasing: "easeOutBounce",
     // animateRotate: true,
     // animateScale: true,
      responsive: true,
     // maintainAspectRatio: true,
     // scaleSteps: 60,
     // onAnimationComplete: null
    };
  }

  function OnErrorCall_(repo) {
    //alert("Woops something went wrong, pls try later !");
  }
});