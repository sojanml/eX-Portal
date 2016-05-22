$(document).ready(function () {
    $('#IsUseCamara').on("change", function () {
        if ($(this).val() == '1') {
            $('#UploadInfo').slideDown();
        } else {
            $('#UploadInfo').slideUp();
        }//if
    })
});