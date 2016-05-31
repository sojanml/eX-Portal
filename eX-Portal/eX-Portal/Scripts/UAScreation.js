
$(document).ready(function () {
    $('#MakeID').on("change", function () {
        if ($(this).val() == '-1') {
            $('#divMakeDetails').slideDown();
        } else {
            $('#divMakeDetails').slideUp();
        }//if
    });
    $('#IsCamara').on("change", function () {
        if ($(this).val() == '1') {
            $('#divCamaraDetails').slideDown();
        } else {
            $('#divCamaraDetails').slideUp();
        }//if
    });

    $('#ModelID').on("change", function () {
        if ($(this).val() == '-1') {
            $('#divModelDetails').slideDown();
        } else {
            $('#divModelDetails').slideUp();
        }//if
    });
    $('#ManufactureId').on("change", function () {
        if ($(this).val() == '-1') {
            $('#divManuDetails').slideDown();
        } else {
            $('#divManuDetails').slideUp();
        }//if
    });
});




