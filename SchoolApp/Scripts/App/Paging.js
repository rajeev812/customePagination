var pageSize = 10;
var currentPage = 1;
var totalRecords = 1;


function pageItemsPerPage(e) {
    currentPage = 1;
    selectedValue = e.options[e.selectedIndex].value;
    if (selectedValue == 'All') {
        pageSize = totalRecords;
    } else {
        pageSize = selectedValue;
    }
    submitForm();
    setupControls();
    return false;
}
function pageGrid(selectedItem) {
    if (selectedItem == currentPage) {
        return
    }
    switch (selectedItem) {
        case "first":
            currentPage = 1;
            break;
        case "previous":
            if (currentPage > 1) {
                currentPage = currentPage - 1;
            }
            break;
        case "last":
            if (totalRecords > pageSize) {
                currentPage = totalRecords / pageSize;
                if (currentPage % 1 != 0) {
                    currentPage = Math.floor(currentPage + 1);
                }
            }
            break;
        case "next":
            var lastpage = totalRecords / pageSize;
            if (lastpage % 1 != 0) {
                lastpage = Math.floor(lastpage + 1);
            }
            if (currentPage < lastpage) {
                currentPage = currentPage + 1;
            }
            break;
        default:
            currentPage = selectedItem;
            break;
    }
    submitForm();
    setupControls();
    return false;
}
function setupControls() {
    var navButtons = $("#pagingButtons");
    navButtons[0].innerHTML = "";
    //#region Enable/Disable
    var disableTextForPrev = "";
    var disableTextForNext = "";
    var eventTextForPrev = "";
    var eventTextForNext = "";
    var eventTextForFirst = "";
    var eventTextForLast = "";
    var startItem = currentPage;
    if (startItem == 1) {
        disableTextForPrev = ' style="cursor: not-allowed;"';
        eventTextForPrev = "";
        eventTextForFirst = "";
    }
    else {
        //disableTextForPrev = "";
        eventTextForPrev = "return pageGrid('previous');";
        eventTextForFirst = "return pageGrid('first');";
    }
    var endItem = ((currentPage) * pageSize);
    if (endItem >= totalRecords) {
        disableTextForNext = ' style="cursor: not-allowed;"';
        eventTextForNext = "";
        eventTextForLast = "";
    }
    else {
        //disableTextForNext = "";
        eventTextForNext = "return pageGrid('next');";
        eventTextForLast = "return pageGrid('last');";
    }
    //#endregion
    var eleString = $("<li class=\"paginate_button previous\"><a class=\"glyphicon glyphicon-backward\" href=\"javascript:void(0)\" onclick=\"" + eventTextForFirst + "\"  " + disableTextForPrev + "></a></li>")
    navButtons.append(eleString);
    eleString = $("<li class=\"paginate_button previous\"><a href=\"javascript:void(0)\" class=\"glyphicon glyphicon-step-backward\" onclick=\"" + eventTextForPrev + "\" " + disableTextForPrev + "></a></li>")
    navButtons.append(eleString);
    var numberButtonCount = 5;
    var start = currentPage - Math.floor(numberButtonCount / 2);
    if (start < 1) {
        start = start + (1 - start);
    }
    end = start + numberButtonCount - 1;
    var lastpage = totalRecords / pageSize;
    if (lastpage % 1 != 0) {
        lastpage = Math.floor(lastpage + 1);
    }
    if (end > lastpage) {
        start = start - (end - lastpage);
        if (start < 1) {
            start = start + (1 - start);
        }
        end = start + numberButtonCount - 1;
        if (end > lastpage) {
            end = lastpage;
        }
    }
    for (var i = start; i <= end; i++) {
        eleString = $("<li class=\"paginate_button\"><a href=\"javascript:void(0)\" onclick=\"return pageGrid(" + i + ");\">" + i + "</a></li>")
        navButtons.append(eleString);
    }
    eleString = $("<li class=\"paginate_button next\"><a class=\"glyphicon glyphicon-step-forward\" href=\"javascript:void(0)\" onclick=\"" + eventTextForNext + "\" " + disableTextForNext + "></a></li>")
    navButtons.append(eleString);
    eleString = $("<li class=\"paginate_button previous\"><a class=\"glyphicon glyphicon-forward\" href=\"javascript:void(0)\" onclick=\"" + eventTextForLast + "\" " + disableTextForNext + "></a></li>")
    navButtons.append(eleString);
    //pagingDetails();
}
function pagingDetails() {

    var currentItem = ((currentPage - 1) * pageSize);
    if (totalRecords == 0) {
        currentItem = -1;
    }
    var endItem = ((currentPage) * pageSize);
    if (endItem > totalRecords) {
        endItem = totalRecords;
    }
    var elementString = "<span>" + "Showing " + parseInt(currentItem + 1) + " - " + endItem + " of " + totalRecords + " entries</span>";
    var element = $(elementString);
    $("#pagingDetails")[0].innerHTML = "";
    $("#pagingDetails").append(element);
}
function submitForm() {
    window.init()
}

