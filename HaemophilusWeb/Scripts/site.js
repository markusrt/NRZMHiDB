function ShowDivIfInputHasSpecificSelectedValueOrClearInputOtherwise(
    divToShow, inputSelector, valueSelector, valueOnWhichToShowDiv, inputToClearOnOtherValue, secondInputToClearOnOtherValue)
{
    var showDivCallback = function()
    {
        var showDiv = $(valueSelector).val() === valueOnWhichToShowDiv;
        return showDiv;
    }

    EnableShowDivIfCallbackReturnsTrueOrClearInputOtherwise(
        showDivCallback, divToShow, inputSelector, valueOnWhichToShowDiv, inputToClearOnOtherValue, secondInputToClearOnOtherValue);
}

function ShowDivIfCheckBoxHasSpecificSelectedValueOrClearInputOtherwise(
    divToShow, inputSelector, checkedValuesSelector, valueOnWhichToShowDiv, inputToClearOnOtherValue)
{
    var showDivCallback = function ()
    {
        var showDiv = false;
        $.each($(checkedValuesSelector), function (index, value) {
            if ($(value).val() === valueOnWhichToShowDiv) {
                showDiv = true;
            }
        });
        return showDiv;
    }

    EnableShowDivIfCallbackReturnsTrueOrClearInputOtherwise(
        showDivCallback, divToShow, inputSelector, valueOnWhichToShowDiv, inputToClearOnOtherValue);
}

function EnableShowDivIfCallbackReturnsTrueOrClearInputOtherwise(
    showDivCallback, divToShow, inputSelector, valueOnWhichToShowDiv, inputToClearOnOtherValue, secondInputToClearOnOtherValue) {
    ShowDivOrClearInput(showDivCallback, divToShow, inputToClearOnOtherValue, secondInputToClearOnOtherValue);

    $(inputSelector).change(function () {
        ShowDivOrClearInput(showDivCallback, divToShow, inputToClearOnOtherValue, secondInputToClearOnOtherValue);
    });
}

function ShowDivOrClearInput(
    showDivCallback, divToShow, inputToClearOnOtherValue, secondInputToClearOnOtherValue)
{
    var showDiv = showDivCallback();

    if (showDiv) {
        $(divToShow).removeClass("hidden");
        if ($(inputToClearOnOtherValue).val().length == 0)
        {
            $(inputToClearOnOtherValue).focus();
        }
    }
    else {
        $(divToShow).addClass("hidden");
        $(inputToClearOnOtherValue).val("");
        $(secondInputToClearOnOtherValue).val("");
    }
}

function hideAll(selector)
{
    $.each($(selector), function (index, value) {
        $(value).addClass("hidden");
    });
}

function showAll(selector)
{
    $.each($(selector), function (index, value) {
        $(value).removeClass("hidden");
    });
}

function ShowError(message)
{
    var errorHtml = CreateAlertHtml("alert-danger", "Fehler", message);
    $("#errorMessages").append(errorHtml);
}

function ShowWarning(message)
{
    var warningHtml = CreateAlertHtml("alert-warning", "Warnung", message);
    $("#errorMessages").append(warningHtml);
}

function CreateAlertHtml(alertStyle, title, message) {
    return '<div class="alert ' + alertStyle + ' alert-dismissable">' +
        '<button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' +
        '<strong>' + title + '</strong> ' + message +
      '</div>';
}