function ShowDivIfDropDownHasSpecificSelectedValueOrClearInputOtherwise(
                divToShow, dropDown, valueOnWhichToShowDiv, inputToClearOnOtherValue)
{
    if ($(dropDown).val() === valueOnWhichToShowDiv)
    {
        $(divToShow).removeClass("hidden");
    }
    else {
        $(divToShow).addClass("hidden");
    }

    $(dropDown).change(function ()
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