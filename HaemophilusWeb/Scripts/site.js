function ShowOnSpecificRadioValue(idToShow, toggleRadio, toggleValue, inputToClearOnOtherValue, secondInputToClearOnOtherValue, thirdInputToClearOnOtherValue ) {
    ShowDivIfInputHasSpecificSelectedValueOrClearInputOtherwise(
        "#"+idToShow,
        "input:radio[name$='" + toggleRadio + "']",
        "input:radio[name$='" + toggleRadio + "']:checked",
        toggleValue,
        inputToClearOnOtherValue, secondInputToClearOnOtherValue, thirdInputToClearOnOtherValue
    );
}

function RadioInput(name)
{
    return "input:radio[name$='" + name + "']";
}

function CheckInput(name)
{
    return "input:checkbox[name$='" + name + "']";
}

function ExactInput(id) {
    return "[id='" + id + "']";
}

function GeneralInput(id) {
    return "[id$='" + id + "']";
}

function ShowDivIfInputHasSpecificSelectedValueOrClearInputOtherwise(
    divToShow, inputSelector, valueSelector, valueOnWhichToShowDiv, inputToClearOnOtherValue, secondInputToClearOnOtherValue, thirdInputToClearOnOtherValue)
{
    var showDivCallback = function()
    {
        var showDiv = $(valueSelector).val() === valueOnWhichToShowDiv;
        return showDiv;
    }

    EnableShowDivIfCallbackReturnsTrueOrClearInputOtherwise(
        showDivCallback, divToShow, inputSelector, inputToClearOnOtherValue, secondInputToClearOnOtherValue, thirdInputToClearOnOtherValue);
}

function ShowDivIfInputMatchesValueOrClearInputOtherwise(
    divToShow, inputSelector, valueSelector, regexOnWhichToShowDiv, inputToClearOnOtherValue, secondInputToClearOnOtherValue, thirdInputToClearOnOtherValue)
{
    var showDivCallback = function()
    {
        var showDiv = $(valueSelector).val().match(regexOnWhichToShowDiv) !== null;
        return showDiv;
    }

    EnableShowDivIfCallbackReturnsTrueOrClearInputOtherwise(
        showDivCallback, divToShow, inputSelector, inputToClearOnOtherValue, secondInputToClearOnOtherValue, thirdInputToClearOnOtherValue);
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
        showDivCallback, divToShow, inputSelector, inputToClearOnOtherValue);
}

function EnableShowDivIfCallbackReturnsTrueOrClearInputOtherwise(
    showDivCallback, divToShow, inputSelector, inputToClearOnOtherValue, secondInputToClearOnOtherValue, thirdInputToClearOnOtherValue) {
    ShowDivOrClearInput(showDivCallback, divToShow, inputToClearOnOtherValue, secondInputToClearOnOtherValue, thirdInputToClearOnOtherValue);

    $(inputSelector).change(function () {
        ShowDivOrClearInput(showDivCallback, divToShow, inputToClearOnOtherValue, secondInputToClearOnOtherValue, thirdInputToClearOnOtherValue);
    });
}

function ShowDivOrClearInput(
    showDivCallback, divToShow, inputToClearOnOtherValue, secondInputToClearOnOtherValue, thirdInputToClearOnOtherValue)
{
    var showDiv = showDivCallback();

    if (showDiv) {
        $(divToShow).removeClass("hidden");
        if ($(inputToClearOnOtherValue).val().length == 0 && !$(inputToClearOnOtherValue).hasClass("no-focus"))
        {
            $(inputToClearOnOtherValue).focus();
        }
    }
    else {
        $(divToShow).addClass("hidden");

        if ($(inputToClearOnOtherValue).is(':checkbox') || $(inputToClearOnOtherValue).is(':radio'))
        {
            $(inputToClearOnOtherValue).prop('checked', false);
            $(inputToClearOnOtherValue).parent().removeClass('active');
        }
        else
        {
            $(inputToClearOnOtherValue).val("");
            $(secondInputToClearOnOtherValue).val("");
            $(thirdInputToClearOnOtherValue).val("");
        }
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

function preventSiteNavigationWithPendingChanges() {
    $(document).ready(function () {
        $('input').on("change keydown", installBeforeUnloadListener);
        $('select').on("change", installBeforeUnloadListener);
        $('textarea').on("change keydown", installBeforeUnloadListener);
    });

    $(document).ready(function () {
        $('form').on("submit", function (e) {
            $(window).off("beforeunload");
            delete window.installedBeforeUnloadListenerOnPendingChanges;
            return true;
        });
    });
}

function installBeforeUnloadListener() {
    if (window.installedBeforeUnloadListenerOnPendingChanges) {
        return;
    }
    $(window).on("beforeunload", function () {
        return "Sie besitzen ungespeicherte Änderungen!";
    });
    window.installedBeforeUnloadListenerOnPendingChanges = true;
};