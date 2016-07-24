var QueID = 0;

$(document).ready(function () {
    $(document).on('click', 'span.delete', function (e) {
        e.preventDefault();
        DeleteFile($(this));
    });

    $(':file').change(startUpload);

    $('#AuthorityName').on("change", function (e) {
        var thisVal = $(this).val();
        thisVal = thisVal.trim();
        $('#fileinput_Other').prop("disabled", (thisVal.length < 3));
    })

});


function startUpload(e) {
    var ID = e.target.id;
    var n = ID.lastIndexOf("_");
    var Sufix = ID.substring(n + 1);

    for (var i = 0; i < this.files.length; i++) {
        var file = this.files[i];
        if (file.name.length < 1) {
        } else if (file.size > 5 * 1024 * 1024) {
            alert("File is to big");
        } else {
            SubmitFile(file, Sufix);
        }
    }
    this.value = "";
}


function DeleteFile(Obj) {
    var LI = Obj.closest('LI');
    LI.attr("class", "delete");
    $("#delete-confirm").dialog({
        resizable: false,
        modal: true,
        buttons: {
            "Delete": function () {
                $(this).dialog("close");

                LI.attr("class", "processing");
                Obj.parent().hide();
                LI.find('.recordinfo').html('Deleting....');
                processDeleteFile(Obj);
            },
            Cancel: function () {
                $(this).dialog("close");
                LI.attr("class", "");
            }
        }
    });

}

function processDeleteFile(Obj) {
    var FileName = Obj.attr("data-file");
    var URL = '/Rpas/DeleteFile/' + FlightID + '?file=' + FileName;
    var LI = Obj.closest('LI');
    $.ajax({
        url: URL,  //server script to process data
        type: 'GET',
        dataType: 'json',
        success: completeHandler = function (data) {
            LI.find('.recordinfo').html(data.message);
            //if (data.status == "ok") LI.slideUp();
        }, error: errorHandler = function (data) {
            LI.find('.recordinfo').html(data.status + ' - ' + data.statusText);
        }
    });
}


function SubmitFile(file, Sufix) {
    QueID++;

    var FORM = document.forms['FileUpload_' + Sufix];
    var PostURL = DroneUploadURL + "&DocumentTitle=" + Sufix;
    if (FORM['AuthorityName']) PostURL += "&DocumentDesc=" + FORM['AuthorityName'].value;

    var FileName = file.name;
    var HTML = 'Uploading ' + FileName + ' (' + Math.floor(file.size / 1024) + ' KB)';
    var Elem = $('<LI>' + HTML + '</LI>');
    $('#FileUploadProgress_' + Sufix).append(Elem);

    var formData = new FormData();
    formData.append("upload-file", file);
    $.ajax({
        url: PostURL,  //server script to process data
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
                if (data.addFile[0].savename) {
                    Elem.append('<div style="float: right"><span data-file="' + data.FileURL + '" class="delete icon">&#xf057;</span></div>');
                   // Elem.append('<div style="float: right"><span data-file="' + data.addFile[0].name + '" class="delete icon">&#xf057;</span></div>');
                    Elem.append('<input type="hidden" name="FileName" Value="' + data.addFile[0].name + '">');
                }

                Elem.append(
                  ((data.DocumentDesc && data.DocumentDesc != "") ? data.DocumentDesc + " - " : "") +
                  '<a href="/Upload/Flight/' + data.FileURL + '">' + FileName + '</a>' +
                  '<span class="recordinfo"></span>'
                );
                Elem.addClass("success");
            } else {
                Elem.addClass("error");
                Elem.html(HTML + ' - ' + data.message);

            }
        },
        error: errorHandler = function (data) {
            Elem.addClass("error");
            Elem.html(HTML + ' - error in uploading file');
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
    var Status = (percentComplete >= 100 ? ' Processing...' : percentComplete.toFixed(0) + '% done');
    Elem.html(HTML + ' - ' + Status);
}
