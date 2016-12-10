$(".custom-cbox").on('click', function () {
    var checked = ($(this).attr("data-checked") == "true" ? "false" : "true");
    $(this).attr("data-checked", checked);
    $(this).find("input").attr("value", checked);
});
$(".custom-radio").on('click', function () {
    if ($(this).attr("data-checked") == "false") {
        $("div[data-radio-name='" + $(this).attr("data-radio-name") + "']").attr("data-checked", "false");
        $(this).attr("data-checked", "true");
    }
});
function getRadioValue(name) {
    var value = null;
    $("div[data-radio-name='" + name + "']").each(function () {
        if ($(this).attr("data-checked") == "true") {
            value = $(this).attr("data-radio-value");
        }
    });
    return value;
}
function getCboxValue(name) {
    var value = Array();
    $("div[data-cbox-name='" + name + "']").each(function () {
        if ($(this).attr("data-checked") == "true") {
            value.push($(this).attr("data-cbox-value"));
        }
    });
    return JSON.stringify(value);
}
function bindRadioCbox() {
    $(".custom-cbox").on('click', function () {
        var checked = ($(this).attr("data-checked") == "true" ? "false" : "true");
        $(this).attr("data-checked", checked);
        $(this).find("input").attr("value", checked);
    });
    $(".custom-radio").on('click', function () {
        if ($(this).attr("data-checked") == "false") {
            $("div[data-radio-name='" + $(this).attr("data-radio-name") + "']").attr("data-checked", "false");
            $(this).attr("data-checked", "true");
        }
    });
}