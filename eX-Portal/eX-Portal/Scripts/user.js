var FileName = '';
$(document).ready(function () {
  $('#User_IsPilot').on("change", function () {
    if ($(this).val() == 'true') {
      $('#PilotInfo').slideDown();
    } else {
      $('#PilotInfo').slideUp();
    }//if
  })

  $('#fileinput').on("change", startUploadImage);
   //$("input[id^='fileinputs']").on('change', startUploadImage);

});


function startUploadImage() {
  for (var i = 0; i < this.files.length; i++) {
    var file = this.files[i];

    if (file.name.length < 1) {
    } else if (file.size > 5 * 1024 * 1024) {
      alert("File is to big");
    } else if (file.type == "image/jpeg" && file.type == "image/png" && file.ext != "image/gif") {
      alert("Invalid Format")
    } else {
      setPreview(file);
      SubmitFileImage(file);
    }
  }
  this.value = "";
}


function SubmitFileImage(file) {


   FileName = file.name;
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
        document.forms[0]['User.PhotoUrl'].value = data.addFile[0].name;
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