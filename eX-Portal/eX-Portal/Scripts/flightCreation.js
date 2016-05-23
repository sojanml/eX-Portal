var UploadUrl = '/rpas/FlightRegister';
var DocType = 'rpasApproval';

function getCoordinates() {
  return $('#Coordinates').val();
}
function setCoordinates(Cord) {
  $('#Coordinates').val(Cord);
}


$(document).ready(function () {
  $('#StartTime').timepicker({});
  $('#EndTime').timepicker({});

  // Create a map object and specify the DOM element for display.
  var map = null;
  $('#btnCoordinates').on("click", function () {
    var iFrame = window.frames['iFrameMap']
    if (iFrame.updateCordinates) iFrame.updateCordinates();
    $("#map-dialog").dialog({
      resizable: false,
      height: 600,
      minWidth: 1000,
      modal: true,
      buttons: {
        Close: function () {
          $(this).dialog("close");
        }
      }
    });
  });


  $('#IsUseCamara').on("change", function () {
    if ($(this).val() == '1') {
      $('#UploadInfo').slideDown();
    } else {
      $('#UploadInfo').slideUp();
    }//if
  })


  //S3 Upload code 
  $("#S3UploadFile").on('change', startUpload);


  $('#RegisterForm').on("submit", function (e) {
    e.preventDefault();
    var IsChecked = $('#IsUseCamara').val();
    var file = $('#MOD_ApprovalURL').val();
    if (file == '' && IsChecked == 1) {
      $('#ApprovalFileUrl-msg').slideDown();
      return false;
    } else {
      document.forms['RegisterForm'].submit();
      $('#ApprovalFileUrl-msg').slideUp();
      return true;
    }
  });

});



function startUpload() {
  var File = document.forms['RegisterForm']['file'];
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
  return 'MOD_Approval/' + KeyName + "_" + fixName(FileName);
}

function fixName(theStr) {
  theStr = theStr.replace(/[^A-Z0-9\.]/ig, "-");
  theStr = theStr.replace(/\-+/g, "-");
  theStr = theStr.replace(/\-+$/g, "");
  theStr = theStr.replace(/^\-+/g, "");
  return theStr;
}

function SubmitFile(file) {

  var FileName = file.name;
  var FileInfo = FileName + ' (' + Math.floor(file.size / 1024) + ' KB)';
  var KeyName = getKeyName(file.name);
  var FORM = document.forms['formS3'];
  var S3UploadUrl = $('#formS3').prop("action");
  var fd = new FormData();
  var AjaxData = $('#formS3').serialize() + '&S3Url=' + KeyName;

  $("#RegisterForm :input").prop("disabled", true);
  //$('#btn-submit').val("Uploading...");

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
      CompleteSubmitForm(AjaxData, KeyName);
    },
    // Form data
    data: fd,
    //Options to tell JQuery not to process data or worry about content-type
    cache: false,
    contentType: false,
    processData: false
  }, 'json');


}

function CompleteSubmitForm(AjaxData, S3KeyUrl) {
  document.forms['RegisterForm']['MOD_ApprovalURL'].value = S3KeyUrl;
  // alert('S3KeyUrl: ' + S3KeyUrl);
}

function progressHandlingFunction(evt) {
  var percentComplete = evt.loaded / evt.total * 100;
  $('#file-info-status').html(percentComplete.toFixed(0) + '% done');
}
function uploadComplete(res, status, xhr) {
  //console.log(status);
  $("#RegisterForm :input").prop("disabled", false);
}