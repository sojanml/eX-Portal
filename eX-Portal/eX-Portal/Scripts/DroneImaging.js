var QueID = 0;
var FilesInQueue = [];
var map;
var bounds = "";
var Markers = {};
var refID = 0;
var count = 0;
var Pinfowindow = false;
var FileDateUtc;
$(document).ready(function () {
    $(document).on('click', 'a.delete', function (e) {
        e.preventDefault();
        DeleteFile($(this));
    });
    $(document).on('click', 'img#ThumbNail', function (e) {
        count += 1;
        var img = document.getElementById('ThumbNail');
        var ActualSrc = img.getAttribute('src').replace(".t.png", ".jpg");
        $('#dialog').append('<img id="abc" src="' + ActualSrc + '" height="400px" width="400px"/><br/>').append($(this).html());
        $("#dialog").dialog({
            autoOpen: false,
            maxWidth: 600,
            maxHeight: 500,
            width: 1000,
            height: 600,
            modal: true,
            
            buttons: {
                "DownLoad": function () {

                    var ActualUrl = img.getAttribute('src').replace(".t.png", ".jpg");
                    var Link = document.createElement('a');
                    Link.href = ActualUrl;  // use realtive url 

                    Link.download = ActualUrl.substring(ActualUrl.lastIndexOf("~") + 1)

                    document.body.appendChild(Link);
                    Link.click();

                },
                Cancel: function () {
                    $(this).dialog("close");
                }
            },
            close: function (event, ui) {

                $("img#abc").remove();

            }

        });



        $("#dialog").dialog('open');
    });
    //$(document).on('click', '#GPS-Images', function (e) {
    //    popup.slideUp();
    //});


    $(':file').change(AddToUploadQueue);

  
    //initialize();

});

function showimg(img)
{
    var ActualSrc = img.getAttribute('src').replace(".t.png", ".jpg");
    $('#dialog').append('<img id="abc" src="' + ActualSrc + '" />').append($(this).html());
    $("#dialog").dialog({
        autoOpen: false,
        height:'auto',
        width: 'auto',
        modal: true,
        open: function (event, ui) {
            $(".ui-dialog-content").css("padding", 0);
            $(".ui-dialog-content").dialog({ minHeight:'50'});
              $('.ui-widget-overlay').bind('click', function () {
                $('#dialog').dialog("close");
                $('img#abc').remove();
                
            });
        }
    });

   
    $(".ui-dialog-titlebar").hide();   
    $("#dialog").dialog('open')
   
}





function ShowImageDialog(img) {
  
    var ActualSrc = img.getAttribute('src').replace(".t.png", ".jpg");
    $('#dialog').append('<img id="abc" src="' + ActualSrc + '" height="400px" width="400px"/><br/>').append($(this).html());
    $("#dialog").dialog({
        autoOpen: false,

        maxWidth: 600,
        maxHeight: 500,
        width: 1000,
        height: 600,
        modal: true, buttons: {
            "DownLoad": function () {

                var ActualUrl = img.getAttribute('src').replace(".t.png", ".jpg");
                var Link = document.createElement('a');
                Link.href = ActualUrl;  // use realtive url 

                Link.download = ActualUrl.substring(ActualUrl.lastIndexOf("~") + 1)

                document.body.appendChild(Link);
                Link.click();

            },
            Cancel: function () {
                $(this).dialog("close");
            }
        },
        close: function (event, ui) {

            $("img#abc").remove();

        }

    });



    $("#dialog").dialog('open');


   
}
// marker position
//var factory = new google.maps.LatLng(25.9899106, 55.0034188;);
//function initialize() {
//    var initLat = 24.9899106;
//    var initLng = 55.0034188;
//    var center = "";

//    var defaultZoom = 10;

//    var mapOptions = {
//        center: center,
//        zoom: defaultZoom,
//        mapTypeId: ""
//    };

//    map = new google.maps.Map(document.getElementById("map-canvas"), mapOptions);

//    GetGeoTagInfo();
//}








function resetBounds() {
    bounds = new google.maps.LatLngBounds();
    $.each(Markers, function (index, theMarker) {
        if (theMarker.getMap() != null) bounds.extend(theMarker.position)
    });
    map.fitBounds(bounds);
}



//Upload functions


function AddToUploadQueue() {
    for (var i = 0; i < this.files.length; i++) {
        QueID++;
        var file = this.files[i];
        file.uploadKey = QueID;
        FilesInQueue.push(file);

        //add information in que
        var FileName = file.name;
        var HTML = 'Waiting... ' + FileName + ' (' + Math.floor(file.size / 1024) + ' KB)';
        var Elem = $('<LI id="file_' + QueID + '">' + HTML + '</LI>');
        $('#FileUploadProgress').append(Elem);

    }
    window.setTimeout(startUploadQueue, 100);
}

function startUploadQueue() {
    if (FilesInQueue.length <= 0) return;
    var file = FilesInQueue.shift();
    SubmitFile(file);
}


function DeleteFile(Obj) {
    $("#delete-confirm").dialog({
        resizable: false,
        modal: true,
        buttons: {
            "Delete": function () {
                $(this).dialog("close");
                processDeleteFile(Obj);
            },
            Cancel: function () {
                $(this).dialog("close");
            }
        }
    });

}

function processDeleteFile(Obj) {
    var FileName = Obj.attr("data-file");
    var DocumentID = Obj.attr("data-documentid");
    //var FlightId = Obj.attr("data-FlightID");

    //DeleteURL = "/Drone/DeleteFile/" + FlightId + "?DocumentType=Geo%20Tag";
    var URL = DeleteDroneURL + '&file=' + FileName;

    var LI = Obj.closest('LI');
    LI.fadeTo(200, 0.2);
    //return;
    $.ajax({
        url: URL,  //server script to process data
        type: 'GET',
        dataType: 'json',
        success: completeHandler = function (data) {
            if (data.status == "success") {
                LI.fadeOut().remove();
                if (Markers[DocumentID]) {
                    Markers[DocumentID].setMap(null);

                    resetBounds();
                }
            }//if (data.status == "success")
        }, //success
        error: errorHandler = function (data) {
            alert(data.status + ' - ' + data.statusText);
            LI.fadeTo(1);
        }
    });
}


function SubmitFile(file) {
  var Elem = $('#file_' + file.uploadKey);

  if (file.size > 1024 * 1024 * 2)
  {
    var HTML = "Please upload image files with size 2 Mb or  less.'";
    Elem.addClass("error");
    Elem.html(HTML);
    setTimeout(function () {
      Elem.slideUp().remove();

    }, 2000);
    return false;
  }
    var HTML = 'Uploading ' + file.name + ' (' + Math.floor(file.size / 1024) + ' KB)';

    Elem.html(HTML);
    var formData = new FormData();
    var FileDate = file.lastModifiedDate.toUTCString();
    FileDateUtc = file.lastModifiedDate;
    formData.append("upload-file", file);
    $.ajax({
        url: UploadDroneURL + '&CreatedOn=' + encodeURI(FileDate),  //server script to process data
        type: 'POST',
        xhr: function () {  // custom xhr
            myXhr = $.ajaxSettings.xhr();
            if (myXhr.upload) { // if upload property exists
                myXhr.upload.addEventListener('progress', function (evt) {
                    progressHandlingFunction(evt, Elem, HTML);
                }, false); // progressbar
            }
            return myXhr;
        },
        //Ajax events
        success: completeHandler = function (data) {
            Elem.html("");
            if (data.status == 'success') {
                AddToThumbnail(data);
                Elem.append(file.name + " - Upload Completed.<br>" + data.GPS.Info);
                Elem.addClass("success");
            } else {
                Elem.addClass("error");
                Elem.html(HTML + ' - ' + data.message);
            }
            window.setTimeout(function () {
                Elem.slideUp("slow", function () { $(this).remove() });
            }, 2000);
            window.setTimeout(startUploadQueue, 1000);
        },
        error: errorHandler = function (data) {
            Elem.addClass("error");
            Elem.html(HTML + ' - error in uploading file');
            window.setTimeout(function () {
                Elem.slideUp().remove();
            }, 2000);
            window.setTimeout(startUploadQueue, 1000);
        },
        // Form data
        data: formData,
        //Options to tell JQuery not to process data or worry about content-type
        cache: false,
        contentType: false,
        processData: false
    }, 'json');

}

function progressHandlingFunction(evt, Elem, HTML) {
    var percentComplete = evt.loaded / evt.total * 100;
    Elem.html(HTML + ' - ' + percentComplete.toFixed(0) + '% done');
}

function AddToThumbnail(theData) {
    var Thump = theData.url.replace(".jpg", ".t.png");
    var theID = "x" + refID++;
    var HTML = '  <li>\n' +
    '<div class="delete-icon"><a href="#" class="delete"    data-documentid="' + theID + '"  data-file="' + theData.addFile[0].name + '"><span class="delete icon">&#xf057;</span></a></div>\n' +
        '<div class="thumbnail">\n' +
        '  <img id="' + theID + '" onclick="ShowImageDialog(this)" width="50px" height="50px" src="' + Thump + '" />\n' +
        '</div>\n' +
       
      '</li>\n';
    $('#GPS-Images').append(HTML);

    var myDate = new Date(); // Set this to your date in whichever timezone.
    var utcDate = myDate.toUTCString();
    //GeoInfo = {
    //    DocumentID: theID,
    //    Thumbnail: Thump,
    //    Latitude: theData.GPS.Latitude,
    //    Longitude: theData.GPS.Longitude,
    //    Altitude: theData.GPS.Altitude,
    //    FlightID: theData.GPS.FlightID,
    //    UpLoadedDate: theData.GPS.CreatedDate
    //};
    //setMarkerOne(GeoInfo);
    //resetBounds();
}