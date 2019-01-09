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

    $(window).on('load', function () {
        //load child schools 
        $.ajax({
            type: "GET",
            url: $('#rootpath').text() + 'GetBranchSchool',
            data: { "SchoolId": parseInt($("#SchooldId").val(), 10) },
            dataType: "json",
            contentType: "application/json",
            success: function (res) {
                if (res.length > 0) {
                    $.each(res, function (data, value) {
                        $("#SchName").append($("<option></option>").val(value.SchooldChildId).html(value.SchoolsChildName));
                    })
                } else {
                    //$("#Schoolbranch").addClass('hidden');
                   // alert('Parent school having no branch assinged')
                }

            }

        });
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
                "stName": $("#StName").val(),
                "SchName": $("#SchName").val(),
                "MobileNo": $("#MobileNo").val(),
                "SchooldId": $("#SchooldId").val(),
                "chldId": $("#SchooldChildId").val()
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
                            '<td><span id=' + item.StudentId + ' class="glyphicon glyphicon-pencil edit"></span>&nbsp;&nbsp;/&nbsp;&nbsp;<span id=' + item.StudentId + ' class="glyphicon glyphicon-trash deleteStudent"></span></td>' +
                            '<td class="text-center">' + parseInt(parseInt((currentPage - 1) * pageSize) + parseInt(index + 1)) + '</td>' +
                            '<td>' + item.StudentName + '</td>' +
                            '<td>' + item.AddCls + '</td>' +
                            '<td>' + item.FatherName + '</td>' +
                             '<td>' + item.FatherMobileNo + '</td>' +
                              '<td>' + item.MotherName + '</td>' +
                               '<td>' + item.MotherMobileNo + '</td>' +
                             '<td>' + item.EmailId + '</td>' +
                            '<td>' + item.SchoolsName + '</td>';

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
        var selectedStudentDetails = $.grep(gridJson.ItemDetails, function (e) {
            return e.StudentId == rowId;
        });

        if (!selectedStudentDetails.length) {
            alert("Please try to refresh the page and try again.")
            return
        }
        var selectedStudent = selectedStudentDetails[0];
        $('#StudentId').val(selectedStudent.StudentId)
        $('#StudentName').val(selectedStudent.StudentName)
        $('#StudentRegisteration').val(selectedStudent.StudentRegNumber)
        $('#StudentClass').val(selectedStudent.AddCls)
        $('#StudentFatherName').val(selectedStudent.FatherName)
        $('#StudentFatherMobile').val(selectedStudent.FatherMobileNo)
        $('#StudentMotherName').val(selectedStudent.MotherName)
        $('#StudentMotherMobile').val(selectedStudent.MotherMobileNo)
        $('#StudentEmailID').val(selectedStudent.EmailId)
        // pending work


    })
    $('#EditSubmit').on('click', function (e) {

        e.preventDefault();
        var model = {
            "StudentId": $('#StudentId').val(),
            "StudentName": $('#StudentName').val(),
            "StudentRegNumber": $('#StudentRegisteration').val(),
            "AddCls": $('#StudentClass').val(),
            "FatherName": $('#StudentFatherName').val(),
            "FatherMobileNo": $('#StudentFatherMobile').val(),
            "MotherName": $('#StudentMotherName').val(),
            "MotherMobileNo": $('#StudentMotherMobile').val(),
            "EmailId": $('#StudentEmailID').val()
        };

        $.ajax({
            cache: false,
            type: "POST",
            contentType: "application/json; charset=utf-8",
            url: $('#rootpath').text() + 'EditStudent',
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

    $('#StudnetSrchBtn').on('click', function (e) {
        e.preventDefault();
       // $("#SchooldChildId").val(parseInt($('#SchName').val(),10))
        window.init();
    });
    //#region Delete
    $('body').on('click', 'span.deleteStudent', function (e) {

        var rowId = $(this).attr("id");
        var flag = confirm('Are you sure to delete?');
        if (flag) {
            e.preventDefault();
            var model = { "StudentId": rowId };

            $.ajax({
                cache: false,
                type: "POST",
                contentType: "application/json; charset=utf-8",
                url: $('#rootpath').text() + 'DeleteStudent',
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