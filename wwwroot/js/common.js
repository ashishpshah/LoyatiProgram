﻿var filesToUpload = null;

$(document).ready(function () {

    setTimeout(function () {

        try { $('.preloader').addClass('d-none'); } catch { }

    }, 1000);

    $('[data-toggle="tooltip"]').tooltip()

    $('body').on('keypress', '.isNumberKey', function (evt) {
        var theEvent = evt || window.event;

        // Handle paste
        if (theEvent.type === 'paste') {
            key = event.clipboardData.getData('text/plain');
        } else {
            // Handle key press
            var key = theEvent.keyCode || theEvent.which;
            key = String.fromCharCode(key);
        }
        var regex = /^[0-9]*$/;
        if (!regex.test(key)) {
            theEvent.returnValue = false;
            if (theEvent.preventDefault) theEvent.preventDefault();
        }
    });

    $('body').on('keypress', '.isNumberKey_Decimal', function (evt, obj) {

        var theEvent = evt || window.event;
        var charCode = (evt.which) ? evt.which : event.keyCode;

        // Handle paste
        if (theEvent.type === 'paste') {
            key = event.clipboardData.getData('text/plain');
        } else {
            // Handle key press
            var key = theEvent.keyCode || theEvent.which;
            key = String.fromCharCode(key);
        }

        var dotcontains = theEvent.target.value.indexOf(".") != -1;
        if (dotcontains)
            if (charCode == 46) {
                theEvent.returnValue = false;
                if (theEvent.preventDefault) theEvent.preventDefault();
                return false;
            };

        var regex = /^[0-9]*$/;
        if (!regex.test(key) && !(charCode == 46)) {
            theEvent.returnValue = false;
            if (theEvent.preventDefault) theEvent.preventDefault();
            return false;
        }

    });

    $('body').on('click', '.btnSubmit', function (e) {
        e.preventDefault();
        fnSubmitForm($(this).parents('form').attr('id'));
    });

    $('.modal').on('hide.bs.modal', function (e) {
        if (!($(document.activeElement)[0].type == 'button' && $($(document.activeElement)[0]).hasClass('close')))
            e.preventDefault();
    });
});

function fnFileUpload($selector) {

    $($selector).before(function () {
        if (!$(this).prev().hasClass("input-ghost")) {

            var element = $("<input type='file' class='input-ghost' style='visibility:hidden; height:0; display: none;'>");

            if (($(this).find("input")[0]).hasAttribute('id'))
                element.attr("id", ($(this).find("input")[0]).getAttribute('id').replace('temp_', ''));

            if (($(this).find("input")[0]).hasAttribute('name'))
                element.attr("name", ($(this).find("input")[0]).getAttribute('name').replace('temp_', ''));

            if (($(this).find("input")[0]).hasAttribute('data-required'))
                element[0].setAttribute("data-required", null);

            if (($(this).find("input")[0]).hasAttribute('data-msg'))
                element.attr("data-msg", ($(this).find("input")[0]).getAttribute('data-msg'));

            element.change(function () { element.next(element).find("input").val(element.val().split("\\").pop()); });

            $(this).find("button.btn-choose").click(function () { element.click(); });
            $(this).find("button.btn-reset").click(function () { element.val(null); $(this).parents($selector).find("input").val(""); });
            $(this).find("input").css("cursor", "pointer");
            $(this).find("input").mousedown(function () { $(this).parents($selector).prev().click(); return false; });
            $(this).find("input").keypress(function (evt) {
                debugger;
                var theEvent = evt || window.event;

                // Handle key press
                var key = theEvent.keyCode || theEvent.which;

                // Handle paste
                if (theEvent.type === 'paste') {
                    theEvent.returnValue = false;
                } else {
                    key = theEvent.keyCode || theEvent.which;
                }

                if (key != 13) {
                    theEvent.returnValue = false;
                    if (theEvent.preventDefault) theEvent.preventDefault();
                }
                else {
                    $(this).parents($selector).prev().click(); return false;
                }
            });

            return element;
        }
    });
}

function GetMonthName(monthNumber) {
    var months = ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December'];
    return months[monthNumber - 1];
}

function fnShow_Password($id, $id_toggle) {

    const togglePassword = document.querySelector('#' + $id_toggle);
    const password = document.querySelector('#' + $id);
    const type = password.getAttribute('type') === 'password' ? 'text' : 'password';
    password.setAttribute('type', type);
    togglePassword.classList.toggle('fa-eye-slash');
}

function ShowLoader(isShow) {
    if (isShow == true) {
        $('.preloader').removeClass('d-none');
    }
    else {
        $('.preloader').addClass('d-none');
    }
}

function fnLoadParialView($id, url) {
    ShowLoader(true);

    try {
        if (typeof $id != 'undefined' && $id != null && $id.length > 0) {

            //$('#' + $id).load(url, function () {

            //});

            //window.scrollTo(0, 0);

            $.ajax({
                type: "GET",
                url: url,
                data: null,
                contentType: "application/json; charset=utf-8",
                dataType: "html",
                success: function (response) {

                    $('#' + $id).html('');
                    $('#' + $id).append(response);

                    setTimeout(function () {

                        $('.select2').select2();

                        try { fnFileUpload(".input-file-partial"); } catch { }

                        try { $('#' + $id).closest('.' + $id + '_Display').removeClass('d-none'); } catch { }

                        try { $('#' + $id).closest('.' + $id + '_Hide').addClass('d-none'); } catch { }

                        try { fnParialView_Loaded_Success($id); } catch { }

                        ShowLoader(false);

                    }, 1000);

                },
                failure: function (response) {
                    CommonAlert_Error(null)
                },
                error: function (response) {
                    CommonAlert_Error(null)
                }
            });
        }
    }
    catch (err) {
        ShowLoader(false);
    }

}

function fnCloseParialView($id) {

    $('#' + $id).html('');
    $('.' + $id + '_Hide').removeClass('d-none');
    $('.' + $id + '_Display').addClass('d-none');
}

function formValidate($id) {

    var IsValid = true;
    //$.each($('#' + $this.closest('form').id + ' input[data-required]'), function (key, input) {
    $.each($($id + ' input[data-required]'), function (key, input) {
        if ((typeof input.value == 'undefined' || input.value == null || input.value.length == 0) && !input.hasAttribute('disabled') && !$(input).hasClass('temp_fileUpload')) {
            Swal.fire({ icon: 'error', title: input.getAttribute('data-msg') });
            IsValid = false;
            input.focus();
            return IsValid;
        }
    });

    $.each($($id + ' select[data-required]'), function (key, input) {

        if ((typeof input.value == 'undefined' || input.value == null || input.value.length == 0 || input.value == "0") && !$(input)[0].hasAttribute('disabled')) {
            Swal.fire({ icon: 'error', title: input.getAttribute('data-msg') });
            IsValid = false;
            input.focus();
            return IsValid;
        }
    });

    //if (IsValid)
    //    fnSubmitForm($this.closest('form').id);
    //return true;
    return IsValid;
}

function fnSubmitForm($id) {
    ShowLoader(true);

    var $form = $('#' + $id);

    if (formValidate('#' + $id)) {

        let formData = new FormData();

        const array = $form.serializeArray();

        $.each($('#' + $id + ' select'), function (key, input) {
            if ((typeof input.value != 'undefined' && input.value != null && input.value.length > 0) && !input.hasAttribute('disabled'))
                array.filter(function (obj) {
                    if (obj['name'] == $(input).attr('name')) {
                        obj['value'] = $('#' + $(input).attr('id') + ' option:selected').val()
                    }
                })
        });

        $.each($('#' + $id + ' input[type="checkbox"]'), function (key, input) {
            if ((typeof input.value != 'undefined' && input.value != null && input.value.length > 0) && !input.hasAttribute('disabled') && !$(input).hasClass('temp_fileUpload'))
                array.filter(function (obj) {
                    if (obj['name'] == $(input).attr('name')) {
                        obj['value'] = $(input).is(':checked')
                    }
                })
        });

        $.each($('#' + $id + ' input[type="radio"]'), function (key, input) {
            if ((typeof input.value != 'undefined' && input.value != null && input.value.length > 0) && !input.hasAttribute('disabled') && !$(input).hasClass('temp_fileUpload'))
                array.filter(function (obj) {
                    if (obj['name'] == $(input).attr('name')) {
                        obj['value'] = $('input[name="' + $(input).attr('name') + '"]:checked').val()
                    }
                })
        });

        $.each(array, function (key, input) {
            if (typeof input.name != 'undefined' && input.name != null && input.name.length > 0 && input.name != "__RequestVerificationToken")
                formData.append(input.name, input.value);
        });

        var inputFiles = $('#' + $id + ' input[type="file"]');

        if (typeof inputFiles != 'undefined' && inputFiles != null && inputFiles.length > 0)
            $.each(inputFiles, function (key, input) {
                if ((typeof input.value != 'undefined' && input.value != null && input.value.length > 0) && !input.hasAttribute('disabled') && !$(input).hasClass('temp_fileUpload')) {
                    var file = document.getElementById('' + input.getAttribute('id')).files[0];
                    if (typeof input.getAttribute('name') == 'undefined' || input.getAttribute('name') == null || input.getAttribute('name').length == 0) {
                        formData.append("files", file);
                    } else {
                        formData.append(input.getAttribute('name'), file);
                    }
                }
            });

        $.ajax({
            type: 'POST',
            url: $form.attr('action'),
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            dataType: "json",
            success: function (response) {

                ShowLoader(false);

                if (response.StatusCode === 1) {
                    if (typeof response.IsConfirm != 'undefined' && response.IsConfirm != '' && response.IsConfirm != null && response.IsConfirm == true)
                        if (typeof response.RedirectURL != 'undefined' && response.RedirectURL != '' && response.RedirectURL != null)
                            CommonConfirmed_Success(response.Message, response.RedirectURL, null);
                        else {
                            try { CommonConfirmed_Success(response.Message, fnFormData_Saved_Success, [response, $id]); } catch { window.location.reload(); }
                        }
                    else
                        if (typeof response.RedirectURL != 'undefined' && response.RedirectURL != '' && response.RedirectURL != null)
                            window.location = response.RedirectURL;
                        else
                            try { fnFormData_Saved_Success(response, $id); } catch { window.location.reload(); }

                }
                else {
                    CommonAlert_Error(response.Message);
                }
            },
            //xhr: function () {
            //    var fileXhr = $.ajaxSettings.xhr();
            //    if (fileXhr.upload) {
            //        $("progress").show();
            //        fileXhr.upload.addEventListener("progress", function (e) {
            //            if (e.lengthComputable) {
            //                $("#fileProgress").attr({
            //                    value: e.loaded,
            //                    max: e.total
            //                });
            //            }
            //        }, false);
            //    }
            //    return fileXhr;
            //},
            failure: function (response) {
                ShowLoader(false);
                Swal.fire({ icon: 'error', title: 'Oops...! Something went wrong!' })
            },
            error: function (response) {
                ShowLoader(false);
                Swal.fire({ icon: 'error', title: 'Oops...! Something went wrong!' })
            }
        });

    }
    else { ShowLoader(false); return false; }

}

function fnSubmitForm_Confirm($id) {

    Swal.fire({
        icon: 'warning',
        title: "You are not able to update any data after submit application. Are you sure?",
        showDenyButton: false,
        showCancelButton: true,
        confirmButtonText: 'OK'
    }).then((result) => {
        debugger;
        if (result.isConfirmed) {

            $('#' + $id + ' input[name="Is_Confirm_to_Submit"]').val(true);
            $('#' + $id + ' input[name="Is_Confirm_to_Submit"]').trigger("change");

            fnSubmitForm($id);
        } else if (result.isDenied) {
            window.location.reload();
        }
    })
}

function fnDelete_Confirm(url) {

    Swal.fire({
        icon: "warning",
        title: "Are you sure to delete this data?",
        //type: "error",
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes!',
        showCancelButton: true,
        //closeOnConfirm: false
    }).then((result) => {
        if (result.isConfirmed) {
            ShowLoader(true);

            $.ajax({
                type: "POST",
                url: url,
                data: null,
                success: function (response) {

                    ShowLoader(false);

                    if (response.StatusCode === 1) {

                        if (typeof response.RedirectURL != 'undefined' && response.RedirectURL != '' && response.RedirectURL != null)
                            window.location = response.RedirectURL;
                        else
                            try { fnDelete_Success(response); } catch { window.location.reload(); }

                    }
                    else {
                        CommonAlert_Error(response.Message);
                    }
                },
                //xhr: function () {
                //    var fileXhr = $.ajaxSettings.xhr();
                //    if (fileXhr.upload) {
                //        $("progress").show();
                //        fileXhr.upload.addEventListener("progress", function (e) {
                //            if (e.lengthComputable) {
                //                $("#fileProgress").attr({
                //                    value: e.loaded,
                //                    max: e.total
                //                });
                //            }
                //        }, false);
                //    }
                //    return fileXhr;
                //},
                failure: function (response) {
                    ShowLoader(false);
                    Swal.fire({ icon: 'error', title: 'Oops...! Something went wrong!' })
                },
                error: function (response) {
                    ShowLoader(false);
                    Swal.fire({ icon: 'error', title: 'Oops...! Something went wrong!' })
                }
            });
        }
    });
}

function CommonAlert_Error(msg) {

    if (msg == null || msg == "")
        msg = "Oops...! Something went wrong!";

    Swal.fire({ icon: 'error', title: msg })
}

function CommonAlert_Error(msg, redirectUrl) {

    if (msg == null || msg == "")
        msg = "Oops...! Something went wrong!";

    Swal.fire({
        icon: 'error',
        title: msg,
        showDenyButton: false,
        showCancelButton: false,
        confirmButtonText: 'OK'
    }).then((result) => {

        if (typeof redirectUrl != 'undefined' && redirectUrl != null && redirectUrl != '')
            window.location = redirectUrl;
    })
}

function CommonAlert_Success(msg) {


    if (msg == null || msg == "")
        msg = "Data Successfully saved.";

    Swal.fire({
        icon: 'success',
        title: msg,
        showDenyButton: false,
        showCancelButton: false,
        confirmButtonText: 'OK',
    })
}

function CommonConfirmed_Success(msg, functionName, functionParams) { //params = [2, 3, 'xyz'];

    if (msg == null || msg == "")
        msg = "Data Successfully saved.";

    Swal.fire({
        icon: 'success',
        title: msg,
        showDenyButton: false,
        showCancelButton: false,
        confirmButtonText: 'OK'
    }).then((result) => {

        if (typeof functionName != 'undefined' && functionName != null && functionName != '')
            if (typeof functionParams != 'undefined' && functionParams != null)
                if (Array.isArray(functionParams) && functionParams.length > -1) {
                    this.callback = functionName;
                    this.callback.apply(this, functionParams);
                }
                else this.callback = functionName;
            else window.location = functionName;

        /* Read more about isConfirmed, isDenied below
        if (result.isConfirmed) {
            Swal.fire('Saved!', '', 'success')
        } else if (result.isDenied) {
            Swal.fire('Changes are not saved', '', 'info')
        } */
    })
}

function CallBack(fn, data, Id) {
    return fn(data, Id);
}


function fnChange_Switch($this, $labelId) {
    debugger;
    var attr_true = $("#" + $labelId).attr('data-true');
    var attr_false = $("#" + $labelId).attr('data-false');

    if ($this.checked) {
        $("#" + $labelId).html(attr_true);  // checked
    }
    else {
        $("#" + $labelId).html(attr_false);  // unchecked
    }

    //try { fnChange_Switch_Success($this, $labelId); } catch { }
}


//function LoadCommonTable() {
//    debugger;
//    if ($.fn.DataTable.isDataTable('#table_Policy_Master_List')) {
//        $('#table_Policy_Master_List').DataTable().destroy();
//    }


//    dtLoadPolicyList = $('#table_Policy_Master_List').DataTable({
//        "sAjaxSource": "@(IndexPageUrl.Replace("Index", ""))LoadPolicyList",
//        "language": {
//            "emptyTable": "No record found.",
//            "processing": '<i class="fa fa-spinner fa-spin fa-3x fa-fw" style="color:#2a2b2b;"></i><span class="sr-only">Loading...</span>'
//        },
//        "initComplete": function (settings, json) { ShowLoader(false); loadDatatablePage('#table_Policy_Master_List'); },
//        "bServerSide": true,
//        "bProcessing": true,
//        "bSearchable": true,
//        "paging": true,
//        "lengthChange": true,
//        "searching": true,
//        "ordering": true,
//        "info": true,
//        "autoWidth": true,
//        "responsive": true,
//        "columnDefs": [{ "targets": 0, "className": "text-center", "width": "3%" }],
//        "fixedColumns": true,
//        "fnServerParams": function (aoData) { aoData.push({ "name": "From_Date", "value": $('#From_Date').val() }); aoData.push({ "name": "To_Date", "value": $('#To_Date').val() }); },
//        "fnCreatedRow": function (nRow, aData, iDataIndex) {
//            $('td:eq(0)', nRow).html('' + aData['rowNumDisplay']) == null ? "0" : aData['rowNumDisplay'];
//            $('td:eq(1)', nRow).html('' + aData['policy_No'] == null ? "" : aData['policy_No']);
//            $('td:eq(2)', nRow).html('' + aData['insured_Name'] == null ? "" : aData['insured_Name']);
//            $('td:eq(3)', nRow).html('' + aData['risk_Start_Date'] == null ? "" : aData['risk_Start_Date']);
//            $('td:eq(4)', nRow).html('' + aData['risk_End_Date'] == null ? "" : aData['risk_End_Date']);
//            $('td:eq(5)', nRow).html('' + aData['insurerName'] == null ? "" : aData['insurerName']);
//            $('td:eq(6)', nRow).html('' + aData['category_Subcategory'] == null ? "" : aData['category_Subcategory']);
//            $('td:eq(7)', nRow).html('' + aData['plan_Name'] == null ? "" : aData['plan_Name']);
//            $('td:eq(8)', nRow).html('' + aData['total_Sum_Assured'] == null ? "" : aData['total_Sum_Assured']);
//            $('td:eq(9)', nRow).html('' + aData['isActive'] == 'true' ? "Active" : '<span class="text-danger">' + 'InActive' + '</span>');
//            $('td:eq(10)', nRow).html('<div class="btn-group">' +
//                '<button type="button" class="btn btn-info btn-flat btn-sm" onclick="fnLoadParialViewForPolicyDetail(\'divForm_Add\', \'@Url.Action("AddEditPartial","PolicyMaster")?id=' + aData['id'] + '\',' + aData['id'] + ')">' +
//                '<i class="fas fa-edit"></i></button>' +
//                '<button type="button" class="btn btn-info btn-sm ml-1" onclick="location.href=\'@Url.Action("GetDocuments","PolicyMaster")?Id=' + aData['id'] + '\'">Documents</button>' +
//                '<button type="button" class="btn btn-info btn-sm ml-1" onclick="location.href=\'@Url.Action("EndorsementIndex","PolicyMaster")?Policy_Detial_Id=' + aData['id'] + '\'">Endorsement</button></div>');
//            $(nRow).attr('id', 'tr_' + iDataIndex);
//        },
//        "columns": [
//            { "title": "#", "autoWidth": false, "searchable": false },
//            { "title": "Policy No.", "autoWidth": true, "searchable": true },
//            { "title": "Client Name", "autoWidth": true, "searchable": true },
//            { "title": "Risk Start Date", "autoWidth": true, "searchable": true },
//            { "title": "Risk End Date", "autoWidth": true, "searchable": true },
//            { "title": "Insurer Name", "autoWidth": true, "searchable": true },
//            { "title": "Category / Subcategory", "autoWidth": true, "searchable": true },
//            { "title": "Product Name", "autoWidth": true, "searchable": true },
//            { "title": "Total Sum Assured", "autoWidth": true, "searchable": true },
//            { "title": "Status", "autoWidth": false, "searchable": false },
//            { "title": "Action", "autoWidth": false, "searchable": false }
//        ],
//        //dom: 'Bfrtip',
//        dom: "<'row'<'col-sm-6'B>>" +
//            "<'row'<'col-sm-12 col-md-6'l><'col-sm-12 col-md-6'f>>" +
//            "<'row'<'col-sm-12'tr>>" +
//            "<'row'<'col-sm-12 col-md-5'i><'col-sm-12 col-md-7'p>>",
//        //buttons: ['copy', 'csv', 'excel', 'pdf', 'print'],
//        "buttons": [ //"excel",
//            {
//                extend: 'excelHtml5',
//                exportOptions: {
//                    columns: [1, 2, 3, 4, 5, 6, 7, 8, 9]
//                }
//            },
//            {
//                extend: 'excel',
//                exportOptions: {
//                    columns: [1, 2, 3, 4, 5, 6, 7, 8, 9]
//                }
//            },
//            {
//                extend: 'pdf',
//                exportOptions: {
//                    columns: [1, 2, 3, 4, 5, 6, 7, 8, 9]
//                }
//            },]//"buttons": ["csv", "excel", "pdf", "print"]
//    }).buttons().container().appendTo('#table_Policy_Master_List ' + '_wrapper .col-md-6:eq(0)');
//    // });
//    $.fn.DataTable.ext.errMode = 'none';

//}