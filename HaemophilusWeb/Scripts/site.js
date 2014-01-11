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