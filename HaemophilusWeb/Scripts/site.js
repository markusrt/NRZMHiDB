function ShowDivIfInputHasSpecificSelectedValueOrClearInputOtherwise(
    divToShow, inputSelector, valueSelector, valueOnWhichToShowDiv, inputToClearOnOtherValue)
    {
    if ($(valueSelector).val() === valueOnWhichToShowDiv)
    {
        $(divToShow).removeClass("hidden");
        $(inputToClearOnOtherValue).focus();
    }
    else {
        $(divToShow).addClass("hidden");
    }

    $(inputSelector).change(function ()
    {
        if ($(this).val() === valueOnWhichToShowDiv)
        {
            $(divToShow).removeClass("hidden");
            $(inputToClearOnOtherValue).focus();
        }
        else
        {
            $(divToShow).addClass("hidden");
            $(inputToClearOnOtherValue).val("");
        }
    });
}

function hideAll(selector) {
    $.each($(selector), function (index, value) {
        $(value).addClass("hidden");
    });
}

function showAll(selector) {
    $.each($(selector), function (index, value) {
        $(value).removeClass("hidden");
    });
}

function ShowError(message)
{
    var errorHtml =
        '<div class="alert alert-danger alert-dismissable">' +
          '<button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' +
          '<strong>Fehler!</strong> ' + message +
        '</div>';

    $("#errorMessages").append(errorHtml);
}