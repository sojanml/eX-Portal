$(document).ready(function () {
  
      /*  var mb_one = $("#ddl_one").val();
        var mb_two = $("#ddl_two").val();
        var getYear = $("#ddlYear").val();

        var jsonData = JSON.stringify({
            mobileId_one: mb_one,
            mobileId_two: mb_two,
            year: getYear
        });
        */
        $.ajax({
            type: "GET",
            url: MapDataURL + "&LastFlightDataID=" + "0" + '&MaxRecords=' + "20",
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
                    label:"Altitudes",
                  //  fillColor: "rgba(220,220,220,0.2)",
                    strokeColor: "rgba(220,220,220,1)",
                    pointColor: "rgba(220,220,220,1)",
                    pointStrokeColor: "#fff",
                  //  pointHighlightFill: "#fff",
                    pointHighlightStroke: "rgba(220,220,220,1)",
                    data: aDatasets1
                },
                {
                    label: "Satellites",
                   // fillColor: "rgba(151,187,205,0.2)",
                    strokeColor: "rgba(151,187,205,1)",
                    pointColor: "rgba(151,187,205,1)",
                    pointStrokeColor: "#fff",
                   // pointHighlightFill: "#fff",
                    pointHighlightStroke: "rgba(151,187,205,1)",
                    data: aDatasets2
                },
                {
                label: "Pitch",
               // fillColor: "rgba(151,187,205,0.2)",
                strokeColor: "rgba(255,119,119,1)",
                pointColor: "rgba(255,119,119,1)",
                pointStrokeColor: "#fff",
               // pointHighlightFill: "#fff",
                pointHighlightStroke: "rgba(255,119,119,1)",
                data: aDatasets3
                },
                {
                    label: "Roll",
                 //   fillColor: "rgba(151,187,205,0.2)",
                    strokeColor: "rgba(100,50,205,1)",
                    pointColor: "rgba(100,50,205,1)",
                    pointStrokeColor: "#fff",
                  //  pointHighlightFill: "#fff",
                    pointHighlightStroke: "rgba(100,50,205,1)",
                    data: aDatasets4
                },
                {
                    label: "Speed",
                   // fillColor: "rgba(151,187,205,0.2)",
                    strokeColor: "rgba(187,17,17,1)",
                    pointColor: "rgba(187,17,17,1)",
                    pointStrokeColor: "#fff",
                   // pointHighlightFill: "#fff",
                    pointHighlightStroke: "rgba(187,17,17,1)",
                    data: aDatasets5
                }]
            };

            var ctx = $("#myChart").get(0).getContext('2d');
            ctx.canvas.height = 300;  // setting height of canvas
            ctx.canvas.width = 1000; // setting width of canvas
            var lineChart = new Chart(ctx).Line(data, {
                bezierCurve: false,
                datasetFill: false,
                animateScale: false,
               // String - Template string for single tooltips
                tooltipTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].datasetLabel){%><%=datasets[i].value%><%}%></li><%}%></ul>",
                //String - A legend template
                multiTooltipTemplate: "<%= datasetLabel %> : <%= value %>",
                legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].datasetLabel){%><%=datasets[i].value%><%}%></li><%}%></ul>"
            });
            var legend = lineChart.generateLegend();
            $('#myChart').append(legend);
        }

        function OnErrorCall_(repo) {
            alert("Woops something went wrong, pls try later !");
        }
    
});