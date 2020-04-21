function ShowOnSpecificRadioValue(idToShow, toggleRadio, toggleValue, inputToClearOnOtherValue, secondInputToClearOnOtherValue ) {
    ShowDivIfInputHasSpecificSelectedValueOrClearInputOtherwise(
        "#"+idToShow,
        "input:radio[name$='" + toggleRadio + "']",
        "input:radio[name$='" + toggleRadio + "']:checked",
        toggleValue,
        inputToClearOnOtherValue, secondInputToClearOnOtherValue
    );
}

function RadioInput(name)
{
    return "input:radio[name$='" + name + "']";
}

function ExactInput(id) {
    return "[id='" + id + "']";
}

function GeneralInput(id) {
    return "[id$='" + id + "']";
}

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
        if ($(inputToClearOnOtherValue).val().length == 0 && !$(inputToClearOnOtherValue).hasClass("no-focus"))
        {
            $(inputToClearOnOtherValue).focus();
        }
    }
    else {
        $(divToShow).addClass("hidden");

        if ($(inputToClearOnOtherValue).is(':checkbox') || $(inputToClearOnOtherValue).is(':radio'))
        {
            $(inputToClearOnOtherValue).removeAttr('checked');
            $(inputToClearOnOtherValue).parent().removeClass('active');
        }
        else
        {
            $(inputToClearOnOtherValue).val("");
            $(secondInputToClearOnOtherValue).val("");
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