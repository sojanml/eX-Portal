$(document).ready(function () {
 
  $('#fileinput').on("change", startUploadImage);
  $("#S3UploadFile").on('change', startUpload);

});


function startUploadImage() {
  for (var i = 0; i < this.files.length; i++) {
    var file = this.files[i];
    if (file.name.length < 1) {
    } else if (file.size > 5 * 1024 * 1024) {
      alert("File is to big");
    } else {
      setPreview(file);
      SubmitFileImage(file);
    }
  }
  this.value = "";
}


function SubmitFileImage(file) {

  var FileName = file.name;
  var HTML = 'Uploading ' + FileName + ' (' + Math.floor(file.size / 1024) + ' KB)';
  var Elem = $('#photo-upload-msg');
  Elem.show().html(HTML);

  var formData = new FormData();
  formData.append("upload-file", file);
  $.ajax({
    url: PhotoUploadURL,  //server script to process data
    type: 'POST',
    xhr: function () {  // custom xhr
      myXhr = $.ajaxSettings.xhr();
      if (myXhr.upload) { // if upload property exists
        myXhr.upload.addEventListener('progress', function (evt) {
          var percentComplete = Math.floor(evt.loaded / evt.total * 100);
          Elem.html('Uploading: ' + percentComplete.toFixed(0) + '% done');
        }, false); // progressbar
      }
      return myXhr;
    },
    //Ajax events
    success: completeHandler = function (data) {
      if (data.status == 'success') {
        document.forms['createForm']['PhotoUrl'].value = data.addFile[0].name;
        Elem.html("Completed...").hide();
      } else {
        Elem.html(HTML + ' - ' + data.message);

      }
    },
    error: errorHandler = function (data) {
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

function setPreview(theFile) {
  var reader = new FileReader();

  reader.onload = function (e) {
    // get loaded data and render thumbnail.
    document.getElementById("img-user-photo").src = e.target.result;
  };

  // read the image file as a data URL.
  reader.readAsDataURL(theFile);
}




function startUpload() {
  var File = document.forms['createForm']['file'];
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
  return 'RPAS_TradeLicence/' +  KeyName + "_" + fixName(FileName);
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
  document.forms['createForm']['TradeLicenceCopyUrl'].value = S3KeyUrl;
 // alert('S3KeyUrl: ' + S3KeyUrl);
}