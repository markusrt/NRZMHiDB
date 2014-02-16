function ShowDivIfInputHasSpecificSelectedValueOrClearInputOtherwise(
                divToShow, inputSelector, valueOnWhichToShowDiv, inputToClearOnOtherValue)
{
    if ($(inputSelector).val() === valueOnWhichToShowDiv)
    {
        $(divToShow).removeClass("hidden");
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

function ShowError(message)
{
    var errorHtml =
        '<div class="alert alert-danger alert-dismissable">' +
          '<button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' +
          '<strong>Fehler!</strong> ' + message +
        '</div>';

    $("#errorMessages").append(errorHtml);
}