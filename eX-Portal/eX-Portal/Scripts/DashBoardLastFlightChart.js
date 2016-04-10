var lineChart = null;
var DataTempStore = {
  Altitudes: null,
  Satellites: null,
  Pitch: null,
  Roll: null,
  Speed: null
};

var xLabel = new Object();
var ToolTipTimeout = null;
$(document).ready(function () {


  $.ajax({
    type: "GET",
    url: DashBoardLastFlightChartDataURL,
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
    var aDatasets2 = aData[3];
    var aLabelsShort = aData[2];
      //long name stored in global variable
    for (var i = 0; i < aData[0].length; i++) {
        var name = aLabelsShort[i];
        var value = aLabels[i];

        xLabel[name] = value;
    }
    // var aDatasets3 = aData[3];
    //var aDatasets4 = aData[4];
    //var aDatasets5 = aData[5];


    var data = {
        labels: aLabelsShort,
      datasets: [{
        label: "Total Flight Time",
         
       
        fillColor: "rgba(151,187,205,0.5)",
        strokeColor: "rgba(151,187,205,0.8)",
        highlightFill: "rgba(151,187,205,0.75)",
        highlightStroke: "rgba(151,187,205,1)",
        
        data: aDatasets1

      }]
    };

    var ctx = $("#DsChart3").get(0).getContext('2d');
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

    //  customTooltips: function (tooltip) {
    //   customToolTip(tooltip, event);
    //},
     
     
        // tooltipTemplate: _tooltipTemplate,
        // tooltipTemplate:    "<ul <li><span style=\"background-color:blue\"></span>'abcd'</li</ul>",

   tooltipTemplate: "<%if (label){%><%=xLabel[label]%>   Total Flight Time: <%}%><%= value   %>(Minutes)",
      // tooltipTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].datasetLabel){%><%=datasets[i].value%><%}%></li><%}%></ul>",
      //String - A legend template
      multiTooltipTemplate: "<%= value %>",
      legendTemplate : "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>"
     
    });
    var legend = lineChart.generateLegend();
    $('#map-legent').append(legend);
  }



  function _tooltipTemplate(Label) {
     // alert(Label);
      return
  }
   

  function customToolTip(tooltip, e) {
      var tooltipEl = $('#chartjs-tooltip');
      if (ToolTipTimeout) window.clearTimeout(ToolTipTimeout);
      if (!tooltip) {
          ToolTipTimeout = window.setTimeout(function () {
              $('#chartjs-tooltip').fadeOut("fast");
          }, 100);
          return false;
      }
      var HTML = xLabel[tooltip.title];
    
      console.log(HTML);
      for (var i = 0; i < tooltip.labels.length; i++) {
          HTML += "<br>\n" +
          '<span class="icon" style="color:' + tooltip.legendColors[i].fill + '">&#xf111;</span>\n' +
          '<span style="font-weight:bold; color:' + tooltip.legendColors[i].fill + '">' + tooltip.labels[i] + '</span>\n';
      }
      tooltipEl.html(HTML);
      /*
      console.log(
      tooltip.title +
      " - x: " + tooltip.x + ", y: " + tooltip.y +
      " - xPadding: " + tooltip.xPadding + ", yPadding: " + tooltip.yPadding +
      " - xOffset: " + tooltip.xOffset + ", yOffset: " + tooltip.yOffset
      );
      */
      tooltipEl.css({
          display: 'block',
          opacity: 1,
          left: tooltip.chart.canvas.offsetLeft + tooltip.x + 'px',
          top: tooltip.chart.canvas.offsetTop + tooltip.y + 'px'
      });

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