$(document).ready(function () {
    var gridJson = null;
    var mode_edit = false;
    currentPage = 1;
    pageSize = 10;
    //#region Ajax Loader Code
    var loader = $("#ajaxLoader");
    function resetLoaderPosition() {
        var left = $('.table-responsive').scrollLeft();
        var width = $('.table-responsive').width();
        var total = left + (width / 2) - 40;
        loader.children("img").css({ 'left': total + 'px' });
    }

    $(window).on('resize', function () {
        resetLoaderPosition();
    });

    $('.table-responsive').on('scroll', function () {
        resetLoaderPosition();
    });

    function LoadAjaxAnimation(isLoadingData) {
        if (isLoadingData) {
            $("#ajaxLoader").removeClass("hidden");
        }
        else {
            $("#ajaxLoader").addClass("hidden");
        }
    }
    //#endregion
    //#region Grid load

    window.init = function () {

        LoadAjaxAnimation(true);
        var tablebody = $('#custom_panel table tbody');

        if (tablebody.children().children("td").length < 2) {
            tablebody.append('<tr><td colspan="27"><div style="min-height:120px;"></div></td><tr>');
        }

        loader.width(tablebody.width());

        $.ajax({
            cache: false,
            type: "GET",
            dataType: 'json',
            url: $('#rootpath').text() + 'GetGridContent',
            data: {
                "currentPage": currentPage,
                "pageSize": pageSize,
                "SchoolName": $("#SchoolName").val(),
                "SchoolAddress": $("#SchoolAddress").val(),
                "SchoolReg": $("#SchoolReg").val()
            },
            success: function (response) {
                
                gridJson = response;
                // ;
                tablebody.find('tr:gt(0)').remove();
                if (response.ItemDetails && response.ItemDetails.length) {
                    if (!response.CanEdit && !response.CanDelete) {
                        var f_child = tablebody.children('tr:first').children("th:first");
                        // f_child.remove();
                    }
                    $.each(response.ItemDetails, function (index, item) {
                        var rowContent = "";
                        var edit_span = '';
                        var delete_span = '';
                        var saperator = '';
                        var view_span = '';
                       
                        rowContent +=
                            //'<td><span id=' + item.StateId + ' class="btn-link edit" style="cursor:pointer" >Edit</span> | <span id=' + item.StateId + ' class="btn-link delete" style="cursor:pointer" >Delete</span></td>' +
                            //
                            '<td><span id=' + item.SchoolId + ' class="glyphicon glyphicon-pencil edit"></span>&nbsp;&nbsp;/&nbsp;&nbsp;<span id=' + item.SchoolId + ' class="glyphicon glyphicon-trash deleteSchool"></span></td>' +
                            '<td class="text-center">' + parseInt(parseInt((currentPage - 1) * pageSize) + parseInt(index + 1)) + '</td>' +
                            '<td>' + item.SchoolName + '</td>' +
                            '<td>' + item.Address + '</td>' +
                            '<td>' + item.Phone + '</td>' +
                             '<td>' + item.IsActive + '</td>' +
                            //'<td>' + formatAMPM(item.Endtime) + '</td>'+
                            '<td>' + item.SchRegNo + '</td>';


                        tablebody.append('<tr>' + rowContent + '<tr>');
                    })
                }
                else {
                    tablebody.append('<tr><td colspan="27"><h3 style="width:400px;min-height:120px;">No Data Found!!!</h3></td><tr>');
                }
                totalRecords = response.TotalCount;
                setupControls();
                pagingDetails();
                var elem = $('#pagingButtons li a');
                $('#pagingButtons li a').removeClass('active');
                $('#pagingButtons li a:contains(' + currentPage + ')').parent('li').addClass("active")
                $('#pagingButtons li a:contains(' + currentPage + ')').css("cursor", "not-allowed")
                LoadAjaxAnimation(false);
                loader.width(tablebody.width());
            },
            error: function (error) {
                if (error.status == 302) {
                    //alert(302);
                    return
                }
                alert(error.statusText);
                console.log(error);
                LoadAjaxAnimation(false);
                loader.width(tablebody.width());
            }
        })

    }

    //#endregion
    window.init();

    //#region Edit
    $('body').on('click', 'span.edit,span.view', function (e) {
        
        var rowId = $(this).attr("id");
        $('#EditModelPopup').modal('show');
        $(".modal-backdrop.in").hide();
        var selectedSchoolDetails = $.grep(gridJson.ItemDetails, function (e) {
            return e.SchoolId == rowId;
        });

        if (!selectedSchoolDetails.length) {
            alert("Please try to refresh the page and try again.")
            return
        }
        
        var selectedSchool = selectedSchoolDetails[0];
        $('#SchoolId').val(selectedSchool.SchoolId)
        $('#SchoolName').val(selectedSchool.SchoolName)
        $('#SchoolAddress').val(selectedSchool.Address)
        $('#SchoolPhone').val(selectedSchool.Phone)
        $('#SchoolStartTime').val(formatAMPM(selectedSchool.StratTime))
        $('#SchoolEndTime').val(formatAMPM(selectedSchool.Endtime))
        $('#SchoolRegId').val(selectedSchool.SchRegNo)
        if (selectedSchool.IsHavingBranch == true) {
            $('input:radio[name="MainBranch"][value="True"]').prop('checked', true);
            $('#parentSchoolList').addClass('hidden')
        }
        else {
            $('input:radio[name="MainBranch"][value="False"]').prop('checked', true);
            $('#parentSchoolList').removeClass('hidden')
        }
        
        // pending work
        
        
    })

    $('input[type=radio][name=MainBranch]').change(function () {
        if (this.value == 'False') {
            $('#parentSchoolList').removeClass('hidden')
        }
        else if (this.value == 'True') {
            $('#parentSchoolList').addClass('hidden')
        }
    });
    $('#SchoolSrchBtn').on('click', function (e) {
        window.init();
    })

    //#region Edit
    $('#EditSubmit').on('click', function (e) {
        
        e.preventDefault();
        var model = {
            "SchoolId": $('#SchoolId').val(),
            "SchoolName": $('#SchoolName').val(),
            "Address": $('#SchoolAddress').val(),
            "Phone": $('#SchoolPhone').val(),
            "StratTime": $('#SchoolStartTime').val(),
            "Endtime": $('#SchoolEndTime').val(),
            "IsHavingBranch": $('input[name=MainBranch]:checked').val(),
            "ParentId": $('#SchoolId').val()
        };

        $.ajax({
            cache: false,
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: $('#rootpath').text() + 'EditSchool',
            data: JSON.stringify(model),
            dataType: "json",
            beforeSend: function () { },
            success: function (data) {
                if (data == true) {
                    alert("record updated succesfully!")
                    setTimeout(function () { location.reload(); }, 100);
                }
                else {
                    alert('There is some error');
                }
            }
        });
    });
    //#endregion

    //#region Delete
    $('body').on('click', 'span.deleteSchool', function (e) {
        
        var rowId = $(this).attr("id");
        var flag = confirm('Are you sure to delete?');
        if (flag) {
            e.preventDefault();
            var model = { "SchoolId": rowId };

            $.ajax({
                cache: false,
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: $('#rootpath').text() + 'DeleteSchool',
                data: JSON.stringify(model),
                dataType: "json",
                beforeSend: function () { },
                success: function (data) {
                    if (data == true) {
                        alert("record deleted succesfully!")
                        setTimeout(function () { location.reload(); }, 100);
                    }
                    else {
                        alert('There is some error');
                    }
                }
            });
        }
    });
    //#endregion

    //#region Change PageSize

    $('#gridmemberCount').on('change', function () {
        pageSize = parseInt($(this).val());
        currentPage = 1;
        window.init();
    })

    //#endregion

    function formatAMPM(date) {
        var myDate = new Date(parseInt(date.replace('/Date(', '')))
        var hours = myDate.getHours();
        var minutes = myDate.getMinutes();
        var ampm = hours >= 12 ? 'pm' : 'am';
        hours = hours % 12;
        hours = hours ? hours : 12; // the hour '0' should be '12'
        minutes = minutes < 10 ? '0' + minutes : minutes;
        var strTime = hours + ':' + minutes + ' ' + ampm;
        return strTime;
    }
});