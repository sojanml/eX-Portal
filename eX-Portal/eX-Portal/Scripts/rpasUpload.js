var QueID = 0;
var ULElem = null;
var FileInfo = '';

$(document).ready(function () {
    $(':input').on("change", checkForm);

    $('#formS3').on("submit", function (e) {
        e.stopPropagation();
        e.preventDefault();
        if (!checkForm()) return false;
        startUpload();
    });


});

function checkForm() {
    var ReturnValue = true;
    var Form = document.forms['formS3'];
    var File = Form['file'];
    var DocumentType = DocType;
    if (Form['DocumentType']) DocumentType = Form['DocumentType'].value;

  
    if (File.files.length < 1) {
        $('#file-Required').fadeIn();
        ReturnValue = false;
    } else {
        $('#file-Required').fadeOut();
    }

    if (DocumentType.length <= 0) {
        $('#DocumentType-Required').fadeIn();
        ReturnValue = false;
    } else {
        $('#DocumentType-Required').fadeOut();
    }



    return ReturnValue;
}


function startUpload() {
    var File = document.forms['formS3']['file'];
    for (var i = 0; i < File.files.length; i++) {
        var file = File.files[i];
        if (file.name.length < 1) {
        } else if (file.size > 200 * 1024 * 1024) {
            alert("File is to big");
        } else {
            SubmitFile(file);
        }
    }
    File.value = "";
}

function getKeyName(FileName) {
    var Dt = new Date();
    var KeyName = Dt.toISOString().replace(/[^0-9]/g, "");
    var DroneName = fixName(Drone.options[Drone.selectedIndex].text);
    DroneName = DroneName.replace(/\./g, '');
    return 'rpas/' + DocType + '/' + KeyName + "_" + fixName(FileName);

}

function fixName(theStr) {
    theStr = theStr.replace(/[^A-Z0-9\.]/ig, "-");
    theStr = theStr.replace(/\-+/g, "-");
    theStr = theStr.replace(/\-+$/g, "");
    theStr = theStr.replace(/^\-+/g, "");
    return theStr;
}

function SubmitFile(file) {
    QueID++;

    var FileName = file.name;
    var FileInfo = FileName + ' (' + Math.floor(file.size / 1024) + ' KB)';
    var KeyName = getKeyName(file.name);
    var FORM = document.forms['formS3'];
    var S3UploadUrl = $('#formS3').prop("action");
    var fd = new FormData();
    var AjaxData = $('#formS3').serialize() + '&S3Url=' + KeyName;

    $("#formS3 :input").prop("disabled", true);
    $('#btn-submit').val("Uploading...");

    $('#file-input').hide();
    $('#file-info-name').html(FileInfo);

    fd.append('key', KeyName);
    fd.append('AWSAccessKeyId', FORM['AWSAccessKeyId'].value);
    fd.append('acl', 'public-read');
    fd.append('policy', FORM['policy'].value)
    fd.append('signature', FORM['signature'].value);

    fd.append("file", file);

    $.ajax({
        url: S3UploadUrl,  //server script to process data
        type: 'POST',
        xhr: function () {  // custom xhr
            myXhr = $.ajaxSettings.xhr();
            if (myXhr.upload) { // if upload property exists
                myXhr.upload.addEventListener('progress', progressHandlingFunction, false); // progressbar
            }
            return myXhr;
        },
        //Ajax events
        success: completeHandler = function (res, status, xhr) {
            uploadComplete(res, status, xhr);
        },
        complete: completeHandler = function (res, status, xhr) {
            $('#file-info-status').html('Completed');
            saveDocument(AjaxData);
        },
        // Form data
        data: fd,
        //Options to tell JQuery not to process data or worry about content-type
        cache: false,
        contentType: false,
        processData: false
    }, 'json');

    /*
    var xhr = new XMLHttpRequest();
  
    xhr.upload.addEventListener("progress", progressHandlingFunction, false);
    xhr.addEventListener("load", uploadComplete, false);
    //xhr.addEventListener("error", uploadFailed, false);
    //xhr.addEventListener("abort", uploadCanceled, false);
  
    xhr.open('POST', S3UploadUrl, true); //MUST BE LAST LINE BEFORE YOU SEND 
  
    xhr.send(fd);
  
    */
}

function saveDocument(AjaxData) {
    $.ajax({
        url: UploadUrl,  //server script to process data
        type: 'POST',
        data: AjaxData,
        success: completeHandler = function (response) {
            top.location.href = response;
        }
    });
}

function progressHandlingFunction(evt) {
    var percentComplete = evt.loaded / evt.total * 100;
    $('#file-info-status').html(percentComplete.toFixed(0) + '% done');
}

function uploadComplete(res, status, xhr) {
    console.log(status);
}