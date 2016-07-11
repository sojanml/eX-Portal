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
    url: DashBoardPilotChartDataURL,
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
    //var aDatasets4 = aData[4];
    //var aDatasets5 = aData[5];


    var data = {
      labels: aLabels,
      datasets: [{
        label: "Total Flight Time",
        fillColor: "rgba(151,187,205,0.5)",
        strokeColor: "rgba(151,187,205,0.8)",
        highlightFill: "rgba(151,187,205,0.75)",
        highlightStroke: "rgba(151,187,205,1)",



        data: aDatasets1
      }, {
        label: "Current Month Flight Time",
        fillColor: "rgba(255,119,119,0.5)",
        strokeColor: "rgba(255,119,119,0.8)",
        highlightFill: "rgba(255,119,119,0.75)",
        highlightStroke: "rgba(255,119,119,1)",
        data: aDatasets2
      },
      {
        label: "Last Flight Time",
        fillColor: "rgba(236,215,101,0.5)",
        strokeColor: "rgba(236,215,101,0.8)",
        highlightFill: "rgba(236,215,101,0.75)",
        highlightStroke: "rgba(236,215,101,1)",
        data: aDatasets3
      }]
    };

    var ctx = $("#DsChart2").get(0).getContext('2d');
    ctx.canvas.height = 235;  // setting height of canvas
    ctx.canvas.width = 500; // setting width of canvas
    lineChart = new Chart(ctx).Bar(data, {
      //bezierCurve: true,
      // datasetFill: false,
      // animateScale: false,
      // String - Template string for single tooltips
      scaleBeginAtZero: true,

      //Boolean - Whether grid lines are shown across the chart
      scaleShowGridLines: true,

      //String - Colour of the grid lines
      scaleGridLineColor: "rgba(0,0,0,.05)",

      //Number - Width of the grid lines
      scaleGridLineWidth: 1,

      //Boolean - Whether to show horizontal lines (except X axis)
      scaleShowHorizontalLines: true,

      //Boolean - Whether to show vertical lines (except Y axis)
      scaleShowVerticalLines: true,

      //Boolean - If there is a stroke on each bar
      barShowStroke: true,

      //Number - Pixel width of the bar stroke
      barStrokeWidth: 2,

      //Number - Spacing between each of the X value sets
      barValueSpacing: 5,

      //Number - Spacing between data sets within X values
      barDatasetSpacing: 1,

      multiTooltipTemplate: "<%= datasetLabel %> : <%= value %>",
      tooltipTemplate: "<%if (label){%><%=label%>: <%}%><%= value %>",
      customTooltips:  function (tooltip) {
        customToolTip(tooltip, event);
      }

    });
    var legend = lineChart.generateLegend();
    $('#map-legent').append(legend);
  }


  function customToolTip(tooltip, e) {
    var tooltipEl = $('#chartjs-tooltip');
    var ToolLabels = ['']

    if (ToolTipTimeout) window.clearTimeout(ToolTipTimeout);
    if (!tooltip) {
      ToolTipTimeout = window.setTimeout(function () {
        $('#chartjs-tooltip').fadeOut("fast");
      }, 100);
      return false;
    }

    if (tooltip.text) {
      var ToolParts = tooltip.text.split(":");
      tooltip.title = ToolParts[0].trim();
      tooltip.labels = [ToolParts[1].trim()];
      tooltip.legendColors = [{ fill: 'rgba(236,215,101,0.5)' }];
    }
    var HTML = tooltip.title + ' (Minutes)';
    for (var i = 0; i < tooltip.labels.length; i++) {
      HTML += "<br>\n" +
      '<span class="icon" style="color:' + tooltip.legendColors[i].fill + '">&#xf111;</span>\n' +
      '<span style="font-weight:bold; color:' + tooltip.legendColors[i].fill + '">' + tooltip.labels[i] + ' </span>\n';
    }
    tooltipEl.html(HTML);

    tooltipEl.css({
      display: 'block',
      opacity: 1,
      left: getLeft(tooltip.chart.canvas) + tooltip.x + 'px',
      top: getTop(tooltip.chart.canvas) + tooltip.y - 30 + 'px'
    });

  }

  function getLeft(Elem) {
    var offsetLeft = 0;
    while (1) {
      if (!Elem) return offsetLeft;
      if (Elem.className == 'dash-row') return offsetLeft;
      offsetLeft += Elem.offsetLeft;
      Elem = Elem.parentElement;
    }
    return offsetLeft
  }

  function getTop(Elem) {
    var offsetTop = 0;
    while (1) {
      if (!Elem) return offsetTop;
      if (Elem.className == 'dash-row') return offsetTop;
      offsetTop += Elem.offsetTop;
      Elem = Elem.parentElement;
    }
    return offsetTop
  }

  function OnErrorCall_(repo) {
    // alert("Woops something went wrong, pls try later !");
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