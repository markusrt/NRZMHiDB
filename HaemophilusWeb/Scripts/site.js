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

        if($(inputToClearOnOtherValue).is(':checkbox'))
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

function queryHealthOfficeViaRkiTool(patientPostalCode) {
    $.ajax({
        url: "https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20htmlpost%20where%0Aurl%3D'https%3A%2F%2Ftools.rki.de%2FPLZTool%2Fde-DE%2FHome%2FSearch'%20%0Aand%20postdata%3D%22RequestString%3D" + patientPostalCode + "%22%20and%20xpath%3D%22%2F%2Fdiv%5B%40class%3D'tab-row'%5D%22&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys",
        dataType: 'jsonp',
        data: {
            jsonCompat: "new",
            format: "json"
        },
        success: function (data) {
            if (data['error']) {
                console.log("Error querying RKI-Tool: " + data['error'].description);
            } else {
                var divs = data.query.results.postresult.div;
                var address = divs[0].textarea.content ? divs[0].textarea.content : "";
                $("#healthOfficeAddress").append(address.replace(/(?:\r\n|\r|\n)/g, '<br />'));

                var phoneText = divs[1].textarea.content ? divs[1].textarea.content : "";
                var phoneNumber = phoneText.replace(/(?:\s|-)/g, '');
                $("#healthOfficePhone").append("<a href='tel:" + phoneNumber + "'>" + phoneText + "</a>");

                var faxText = divs[2].textarea.content ? divs[2].textarea.content : "";
                var faxNumber = faxText ? '0' + faxText.replace(/(?:\s|-)/g, '') : "-";
                $("#healthOfficeFax").append("<a href='mailto:" + faxNumber + "@fax'>" + faxText + "</a>");

                var email = divs[3].a.content;
                $("#healthOfficeEmail").append("<a href='mailto:" + email + "'>" + email + "</a>");
            }

        }
    });
}

function preventSiteNavigationWithPendingChanges() {
    var installBeforeUnloadListener = function () {
        if (window.installedBeforeUnloadListenerOnPendingChanges) {
            return;
        }
        $(window).on("beforeunload", function () {
            return "Sie besitzen ungespeicherte Änderungen!";
        });
        window.installedBeforeUnloadListenerOnPendingChanges = true;
    };

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