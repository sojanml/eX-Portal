var lineChart = null;
var DataTempStore = {
  Altitudes: null,
  Satellites: null,
  Pitch: null,
  Roll: null,
  Speed: null
};

$(document).ready(function () {


  $.ajax({
    type: "GET",
    url: ChartDataURL + "&LastFlightDataID=" + "0" + '&MaxRecords=' + "20",
    contentType: "application/json;charset=utf-8",
    dataType: "json",
    async: true,
    success: OnSuccess_,
    error: OnErrorCall_
  });

  function OnSuccess_(reponse) {
    var aData = reponse;
    var aLabels = aData[0];
    var aDatasets1 = aData[1];
    var aDatasets2 = aData[2];
    var aDatasets3 = aData[3];
    var aDatasets4 = aData[4];
    var aDatasets5 = aData[5];


    var data = {
      labels: aLabels,
      datasets: [{
        label: "Altitudes",
        //  fillColor: "rgba(220,220,220,0.2)",
        strokeColor: "rgb(219, 211, 1)",
        pointColor: "rgb(219, 211, 1)",
        pointStrokeColor: "#fff",
        pointHighlightStroke: "rgb(219, 211, 1)",
        data: aDatasets1
      }, {
        label: "Satellites",
        strokeColor: "rgba(101, 186, 25,1)",
        pointColor: "rgba(101, 186, 25,1)",
        pointStrokeColor: "#fff",
        pointHighlightStroke: "rgba(101, 186, 25,1)",
        data: aDatasets2
      }, {
        label: "Pitch",
        strokeColor: "rgba(255, 89, 0,1)",
        pointColor: "rgba(255, 89, 0,1)",
        pointStrokeColor: "#fff",
        pointHighlightStroke: "rgba(255, 89, 0,0.5)",
        data: aDatasets3
      }, {
        label: "Roll",
        strokeColor: "rgb(153, 131, 199)",
        pointColor: "rgb(153, 131, 199)",
        pointStrokeColor: "#fff",
        pointHighlightStroke: "rgb(153, 131, 199)",
        data: aDatasets4
      }, {
        label: "Speed",
        strokeColor: "rgb(11, 144, 118)",
        pointColor: "rgb(11, 144, 118)",
        pointStrokeColor: "#fff",
        pointHighlightStroke: "rgba(11, 144, 118,1)",
        data: aDatasets5
      }]
    };

    var ctx = $("#myChart").get(0).getContext('2d');
    ctx.canvas.height = 300;  // setting height of canvas
    ctx.canvas.width = 1000; // setting width of canvas
    lineChart = new Chart(ctx).Line(data, {
      bezierCurve: true,
      datasetFill: false,
      animateScale: false,
      // String - Template string for single tooltips
      tooltipTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].datasetLabel){%><%=datasets[i].value%><%}%></li><%}%></ul>",
      //String - A legend template
      multiTooltipTemplate: "<%= datasetLabel %> : <%= value %>",
      legendTemplate:
      '<ul id=\"line-legend">\n' +
      '  <% for (var i=0; i<datasets.length; i++){%>\n' +
      '  <li class="active" data-label="<%=datasets[i].label%>">\n' +
      '    <span class="legend" style=\"background-color:<%=datasets[i].strokeColor%>\">' +
      '     <span class="icon">&#xf00c;</span>\n' +
      '    </span>\n' +
      '    <span><%=datasets[i].label%></span>\n' +
      '  </li>\n' +
      '  <%}%>\n' +
      '</ul>'
    });
    var legend = lineChart.generateLegend();
    $('#map-legent').append(legend);
  }

  function OnErrorCall_(repo) {
    alert("Woops something went wrong, pls try later !");
  }

  $(document).on("click", "ul#line-legend li", function () {
    var index = -1;
    var thisLI = $(this);
    var thisLabel = thisLI.attr("data-label");
    if (DataTempStore[thisLabel] == null) {
      for (index = 0; index < lineChart.datasets.length; index++) {
        if (lineChart.datasets[index].label == thisLabel) {
          DataTempStore[thisLabel] = lineChart.datasets.splice(index, 1);
          thisLI.removeClass("active");
        }
      }//for
    } else {//if (lineChart.datasets[0].label == thisLabel)
      lineChart.datasets.push(DataTempStore[thisLabel][0]);
      DataTempStore[thisLabel] = null;
      thisLI.addClass("active");
    } //if (lineChart.datasets[0].label == thisLabel)

    lineChart.render();
  });

});