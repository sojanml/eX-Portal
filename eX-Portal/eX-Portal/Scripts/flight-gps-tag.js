var QueID = 0;
var FilesInQueue = [];

$(document).ready(function () {

  $(document).on('click', 'a.delete', function (e) {
    e.preventDefault();
    DeleteFile($(this));
  });

  $(':file').change(AddToUploadQueue);
});


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
  var URL = DeleteURL + '&file=' + FileName;
  var LI = Obj.closest('LI');
  LI.fadeTo( 200 , 0.2);
  //return;
  $.ajax({
    url: URL,  //server script to process data
    type: 'GET',
    dataType: 'json',
    success: completeHandler = function (data) {
      if (data.status == "success") LI.fadeOut().remove();
    },
    error: errorHandler = function (data) {
      alert(data.status + ' - ' + data.statusText);
      LI.fadeTo(1);
    }
  });
}


function SubmitFile(file) {
  var Elem = $('#file_' + file.uploadKey);
  var HTML = 'Uploading ' + file.name + ' (' + Math.floor(file.size / 1024) + ' KB)';
  Elem.html(HTML);
  var formData = new FormData();
  var FileDate = file.lastModifiedDate.toUTCString();
  formData.append("upload-file", file);
  $.ajax({
    url: UploadURL + '&CreatedOn=' + encodeURI(FileDate),  //server script to process data
    type: 'POST',
    xhr: function () {  // custom xhr
      myXhr = $.ajaxSettings.xhr();
      if (myXhr.upload) { // if upload property exists
        myXhr.upload.addEventListener('progress', function (evt) {
          progressHandlingFunction(evt, Elem, HTML);
        },  false); // progressbar
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
        Elem.slideUp("slow", function () { $(this).remove()});
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


  var HTML = '  <li>\n' +
  '<div class="delete-icon"><a href="#" class="delete"  data-file="' + theData.addFile[0].name + '"><span class="delete icon">&#xf057;</span></a></div>\n' +
      '<div class="thumbnail">\n' +
      '  <img src="'  + Thump + '" />\n' +
      '</div>\n' +
      '<div class="gps">' + theData.GPS.Info + '</div>\n' +
    '</li>\n';
  $('#GPS-Images').append(HTML);
}