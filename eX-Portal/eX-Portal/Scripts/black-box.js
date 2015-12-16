var QueID = 0;

$(document).ready(function () {
  $.ajax({
    url: '/BlackBox/getFiles',  //server script to process data
    type: 'GET',
    dataType: 'json',
    success: completeHandler = function (data) {
      if (data.status == 'success') {
        addFileToList(data.addFile, true);
      } else {
        $('#UploadedDatabaseMessage').html(data.message);
      }
    },
    error: errorHandler = function (data) {
      $('#UploadedDatabaseMessage').html(data.message);
    }
  });

  $(document).on('click', 'a.import', function (e) {
    e.preventDefault();
    var LI = $(this).closest('LI');
    LI.attr("class", "processing");
    $(this).parent().hide();
    $(this).parent().parent().find('.recordinfo').html('Processing....');
    ImportFile($(this), true);
  });

  $(document).on('click', 'a.delete', function (e) {
    e.preventDefault();

    DeleteFile($(this));
  });

});



$(':file').change(function () {
  for (var i = 0; i < this.files.length; i++) {
    var file = this.files[i];
    if (file.name.length < 1) {
    } else if (file.size > 5 * 1024 * 1024) {
      alert("File is to big");
    } else {
      SubmitFile(file);
    }
  }
  this.value = "";
});

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
  var FileName = Obj.parent().attr("data-file");
  var URL = '/BlackBox/Delete?file=' + FileName;
  var LI = Obj.closest('LI');
  $.ajax({
    url: URL,  //server script to process data
    type: 'GET',
    dataType: 'json',
    success: completeHandler = function (data) {
      Obj.parent().parent().find('.recordinfo').html(data.message);
      if (data.status == "ok") LI.slideUp();
    }, error: errorHandler = function (data) {
      Obj.parent().parent().find('.recordinfo').html(data.status + ' - ' + data.statusText);
    }
  });
}

function ImportFile(Obj, isReset) {
  var FileName = Obj.parent().attr("data-file");
  var URL = '/BlackBox/Import?file=' + FileName;
  var LI = Obj.closest('LI');

  $.ajax({
    url: URL,  //server script to process data
    type: 'GET',
    dataType: 'json',
    success: completeHandler = function (data) {
      Obj.parent().parent().find('.recordinfo').html(data.message);
      if (data.status == "ok") {
        LI.attr("class", "success");
        window.setTimeout(function () {
          LI.slideUp();
        }, 2000)
      } else {
        LI.attr("class", "error");
        Obj.parent().show();
      }

    },
    error: errorHandler = function (data) {
      Obj.parent().parent().find('.recordinfo').html(data.message);
    }
  });

}

function SubmitFile(file) {
  QueID++;

  var HTML = 'Uploading ' + file.name + ' (' + Math.floor(file.size / 1024) + ' KB)';
  var Elem = $('<LI>' + HTML + '</LI>');
  $('#FileUploadProgress').append(Elem);

  var formData = new FormData();
  formData.append("upload-file", file);
  $.ajax({
    url: '/BlackBox/Save',  //server script to process data
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
      if (data.status == 'success') {
        addFileToList(data.addFile, true);
        Elem.remove();
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

function progressHandlingFunction(evt, Elem, HTML) {
  var percentComplete = evt.loaded / evt.total * 100;
  Elem.html(HTML + ' - ' + percentComplete.toFixed(0) + '% done');
}

function addFileToList(FileInfo, isAddToList) {
  if (!isAddToList) {
    $('#UploadedFilesList').empty();
  }
  for (var i = 0; i < FileInfo.length; i++) {
    QueID++;
    var thisFileInfo = FileInfo[i];
    var newElem = $('<LI>' +
      '<div class="info">' +
      '<div>' + thisFileInfo.name + '</div>' +
      '<div>' + thisFileInfo.size + ' KB</div>' +
      '<div>' + thisFileInfo.created + '</div>' +
      '</div>' +
      getToolBar(thisFileInfo) +
      '<div style="clear:both"></div>'
      );
    $('#UploadedFilesList').append(newElem);
  }
}

function getToolBar(thisFileInfo) {
  return'<div class="right-section">' +
  '  <div data-file="' + thisFileInfo.name + '" class="toolbar">\n' +
  '    <a href="#" class="delete" >Delete</a>' +
  '    <a href="#" class="import">Import</a>' +
  '  </div>' +
  '  <div class="recordinfo" id="RecordInfo' + QueID + '">Lines:' + thisFileInfo.records + '</div>\n' +
  '</div>';
}

function getIcon(Ext) {
  var Icon = 'no-icon.png';
  switch (Ext) {
    case '.db3':
      Icon = 'icon-db3.png';
      break;
  }
  return '<div class="icon"><img src="images/' + Icon + '"></div>\n';
}