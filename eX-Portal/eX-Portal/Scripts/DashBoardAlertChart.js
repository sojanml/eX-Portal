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
        url: DashBoardAlertChartDataURL,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        async: true,
        success: OnSuccess_,
        error: OnErrorCall_
    });

    function OnSuccess_(reponse) {
      
        var aData = reponse;
        var aLabels = aData[0][0];       
        var aDatasets1 = aData[1][0];
        var aDatasets2 = aData[2][0];
        var aDatasets3 = aData[3][0];     
      
        //altitude

        var data = [
           
            {
                label: "Total Alerts",
                value: aDatasets1,
                color:"rgba(151,187,205,0.5)",
                labelColor : 'white',
                labelFontSize : '16'
            },
            {
                label: "Current Month Alert",
                value: aDatasets2,
                color: "#FF6384",
                labelColor : 'white',
                labelFontSize : '16'
            },
            {
                label: "Last Flight Alert",
                value: aDatasets3,
                color: "#FFCE56",
                labelColor : 'white',
                labelFontSize : '16'
            }

        ];
      

        var ctx1 = $("canvas#AlertAltitude").get(0).getContext('2d');
        ctx1.canvas.height = 210;  // setting height of canvas
        ctx1.canvas.width = 250; // setting width of canvas
     var  PieChartAltitude = new Chart(ctx1).Pie(data,

            {

                segmentShowStroke: true,
                segmentStrokeColor: "#fff",
                segmentStrokeWidth: 2,
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
            })

     var legend = PieChartAltitude.generateLegend();



       // $('canvas#AlertAltitude').append(legend);
      
        document.getElementById('js-legend').innerHTML = legend;

     


//Boundary
          
        aLabels = aData[0][1];
        aDatasets1 = aData[1][1];
        aDatasets2 = aData[2][1];
        aDatasets3 = aData[3][1];

        var data = [

            {
                label: "Total Alerts",
                value: aDatasets1,
                color: "rgba(151,187,205,0.5)",
                labelColor: 'white',
                labelFontSize: '16'
            },
            {
                label: "Current Month Alert",
                value: aDatasets2,
                color: "#FF6384",
                labelColor: 'white',
                labelFontSize: '16'
            },
            {
                label: "Last Flight Alert",
                value: aDatasets3,
                color: "#FFCE56",
                labelColor: 'white',
                labelFontSize: '16'
            }

        ];

        var ctx2 = $("canvas#AlertBoundary").get(0).getContext('2d');
        ctx2.canvas.height = 210;  // setting height of canvas
        ctx2.canvas.width = 250; // setting width of canvas
        var chart = new Chart(ctx2).Pie(data,

            {

                segmentShowStroke: true,
                segmentStrokeColor: "#fff",
                segmentStrokeWidth: 2,
                animation: true,
                animationSteps: 200,
                animationEasing: "easeOutBounce",
                animateRotate: true,
                animateScale: true,
              
                labelFontFamily: "'Arial'",
                labelFontStyle: "normal",
                labelFontSize: 12,
                labelFontColor: "white",
                labelVisible: true,
                onAnimationComplete: null
            })

        //proximity
        aLabels =    aData[0][2];
        aDatasets1 = aData[1][2];
        aDatasets2 = aData[2][2];
        aDatasets3 = aData[3][2];

        var data = [

            {
                label: "Total Alerts",
                value: aDatasets1,
                color: "rgba(151,187,205,0.5)",
                labelColor: 'white',
                labelFontSize: '16'
            },
            {
                label: "Current Month Alert",
                value: aDatasets2,
                color: "#FFCE56",
                labelColor: 'white',
                labelFontSize: '16'
            },
            {
                label: "Last Flight Alert",
                value: aDatasets3,
                color: "#FFCE56",
                labelColor: 'white',
                labelFontSize: '16'
            }

        ];

        var ctx3 = $("canvas#AlertProximity").get(0).getContext('2d');
        ctx3.canvas.height = 210;  // setting height of canvas
        ctx3.canvas.width = 250; // setting width of canvas
        var chartProximity = new Chart(ctx3).Pie(data,

            {

                segmentShowStroke: true,
                segmentStrokeColor: "#fff",
                segmentStrokeWidth: 2,
                animation: true,
                animationSteps: 200,
               animationEasing: "easeOutBounce",
                animateRotate: true,
               animateScale: true,
              
                labelFontFamily: "'Arial'",
                labelFontStyle: "normal",
                labelFontSize: 12,
                labelFontColor: "white",
                labelVisible: true,
               onAnimationComplete: null
            })


      




    









    };



   
    function OnErrorCall_(repo) {
         alert("Woops something went wrong, pls try later !");
    }
   

 

    

});