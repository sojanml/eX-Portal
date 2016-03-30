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
            url: DashBoardLastFlightChartDataURL ,
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
           // var aDatasets3 = aData[3];
            //var aDatasets4 = aData[4];
            //var aDatasets5 = aData[5];


            var data = {
                labels: aLabels,
                datasets: [{
        label: "Total Flight Time",
                  //  fillColor: "rgba(220,220,220,0.2)",
                    strokeColor: "rgba(220,220,220,1)",
                    pointColor: "rgba(220,220,220,1)",
                    pointStrokeColor: "#fff",
                  //  pointHighlightFill: "#fff",
                    pointHighlightStroke: "rgba(220,220,220,1)",
                    fillColor: "rgba(255,102,102,0.5)",
                    strokeColor: "rgba(255,102,102,0.8)",
                    highlightFill: "rgba(255,102,102,0.75)",
                    highlightStroke: "rgba(255,102,102,1)",
                    data: aDatasets1
               
                }]
            };

            var ctx = $("#DsChart3").get(0).getContext('2d');
            ctx.canvas.height = 300;  // setting height of canvas
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
        tooltipTemplate: "<%if (label){%><%=label %>: <%}%><%= value   %>",
               // tooltipTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].datasetLabel){%><%=datasets[i].value%><%}%></li><%}%></ul>",
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