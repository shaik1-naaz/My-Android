

$(document).ready(function () {
    $("#addInsurance").validate({
        rules: {
            CourseName: {
                required: true
            }
        },
        messages: {
            CourseName: {
                required: "Please Enter Course Module Name"

            }
        }
    });
});
$(document).ready(function () {
    $("#adminid").validate({
        rules: {
            CourseName: {
                required: true,
            }
        },
        messages: {
            CourseName: {
                required: "Please Enter Course Module Name"
            }
        }
    });
});

$(document).ready(function () {
    $("#addTaxCourse").validate({
        rules: {
            CourseName: {
                required: true
            }
        },
        messages: {
            CourseName: {
                required: "Please Enter Course Module Name"
            }
        }
    });
});

$(document).ready(function () {
    $("#AddAdminNote").validate({
        rules: {
            Note: {
                required: true                
            }
        },
        messages: {
            Note: {
                required: "Please Enter Some Note to Add"
            }
        }
    });
});

$(document).ready(function () {
    debugger;
    $("#adduserid").validate({        
        rules: {
            Name: {
                required: true
            },
            Email: {
                required: true, email: true
            },
            Password: {
                required: true,
                minlength: 6,
                maxlength: 12
            },
            ConfirmPassword: {                
                equalTo: "#password",
                required: true,
                minlength: 6,
                maxlength: 12
            },
            UserType: {
                required: true
            }
        },
        messages: {
            Name: {
                required: "Please Enter Name"
            },
            Email: {
                required: "Please Enter Valid Email Id"
            },
            Password: {
                required: "Please Enter Password"
            },
            ConfirmPassword: {
                required: "Please Enter  Same Password For Confirmation"
            },
            UserType: {
                required: "Please Select UserType"
            }
        }
    });
});

$(document).ready(function () {
    
    $("#nameid").keydown(function (e) {
        if (e.shiftKey || e.ctrlKey || e.altKey) {
            e.preventDefault();
        } else {
            var key = e.keyCode;
            if (!((key == 8) || (key == 32) || (key == 46) || (key >= 35 && key <= 40) || (key >= 65 && key <= 90))) {
                e.preventDefault();
            }
        }
    });
});



$(document).ready(function () {
    
    $("#newtest").validate({
        rules: {
            Formid: {
                required: true
            }
        },
        messages: {
            Formid: {
                required: "Please Select Form"
            }
        }
    });
});

$(document).ready(function () {
    $('#type_sec').change(function () {
        var value = $('#type_sec').find('option:selected').text();
        if (value == "Video") {
            $('#File').show();
            $('#content').hide();
        }
        else if (value == "Content") {
            $('#File').hide();
            $('#content').show();
        }
        else {
            $('#content').hide();
            $('#File').hide();
        }
    });

});

$(document).ready(function () {
    $("#userid").validate({
        rules: {
            UserName: "required",

            Password: "required"

        },
        messages: {
            UserName:
                "Please Enter UserName",
            Password:
                 "Please Enter Password"
        }
    });
});

$(document).ready(function () {
    $("#registerid").validate({
        rules: {
            FirstName: {
                required: true
            },
            LastName: {
                required: true
            },
            Gender: {
                required: true
            },
            UserName: {
                required: true
            },
            Email: {
                required: true,
                email: true
            },
            Password: {
                required: true,
                maxlength: 12,
                minlength: 6
            },
            ConfirmPassword: {
                equalTo: "#password",
                required: true,
                maxlength: 12,
                minlength: 6
            },
            UserType: {
                required: true
            }
        },
        messages: {
            FirstName: {
                required: "Please Enter FirstName"
            },
            LastName: {
                required: "Please Enter LastName"
            },
            Gender: {
                required: "Please Select Gender"
            },
            UserName: {
                required: "Please Enter UserName "
            },
            Email: {
                required: "Please Enter Valid Email Id "
            },
            Password: {
                required: "Please Enter Password "
            },
            ConfirmPassword: {
                required: "Please Enter Same Password For Confirmation"
            },
            UserType: {
                required: "Please Select UserType"
            }
        }
    });
});

$(document).ready(function () {
    $("#addrefid").validate({
        rules: {
            FirstName: {
                required: true
            },
            LastName: {
                required: true
            },
            Phone: {
                required: true
            },
            Email: {
                required: true,
                email: true
            },
            Address: {
                required: true
            },
            State: {
                required: true
            },

        },
        messages: {
            FirstName: {
                required: "Please Enter FirstName"
            },
            LastName: {
                required: "Please Enter LastName"
            },
            Phone: {
                required: "Please Enter Phone Number "
            },
            Email: {
                required: "Please Enter Valid Email Id "
            },
            Address: {
                required: "Please Enter Address "
            },
            State: {
                required: "Please Select State"
            }
        }
    });
});

$(document).ready(function () {

    jQuery.validator.addMethod("validCard", function (value, element) {       
        return $(element).validateCreditCard().valid;
    }, "Not a Valid Card Number");

    $("#addCard").validate({
        rules: {
            CardNumber: {
                required: true,
                number: true,
                validCard: true
            },           
            NameOnCard: {
                required: true
            },
            ExpirationMonth: {
                required: true
            },
            ExpirationYear: {
                required: true
            },
            CVV: {
                required: true
            },       

        },
        messages: {
            CardNumber: {
                required: "Please Enter Card Number",
                number: "Only Numbers in the Card Number",
                validCard: "Not a Valid Card Number"
            },            
            NameOnCard: {
                required: "Please Enter Name on the Card"
            },
            ExpirationMonth: {
                required: "Please Select Exipry Month "
            },
            ExpirationYear: {
                required: "Please Select Expiry Year "
            },
            CVV: {
                required: "Please Enter CVV "
            }
        }
    });
});







//$(document).ready(function () {
//    $('#newtest').validate({
//        rules: {
//            Formid: {
//                required: true
//            }
//        },
//        messages: {
//            Formid: {
//                required: "Please Enter SectionTitle"
//            }
//        }, errorclass: "customerror"
//    });
//});

$(document).ready(function () {
    $('#module').keyup(function () {
        var val = $(this).val().toLowerCase();
        $('#instable tbody tr').hide();
        var trs = $('table tbody tr').filter(function (d) {
            return $(this).text().toLowerCase().indexOf(val) != -1;
        });
        trs.show();
    });
    $('#users').keyup(function () {
        var val = $(this).val().toLowerCase();
        $('#instable tbody tr').hide();
        var trs = $('table tbody tr').filter(function (d) {
            return $(this).text().toLowerCase().indexOf(val) != -1;
        });
        trs.show();
    });
    $('#steps').keyup(function () {
        var val = $(this).val().toLowerCase();
        $('#instable tbody tr').hide();
        var trs = $('table tbody tr').filter(function (d) {
            return $(this).text().toLowerCase().indexOf(val) != -1;
        });
        trs.show();
    });
});

$(document).ready(function () {
    $('#modulename').keyup(function () {
        var val = $(this).val().toLowerCase();
        $('#tablegrid tbody tr').hide();
        var trs = $('table tbody tr').filter(function (d) {
            return $(this).text().toLowerCase().indexOf(val) != -1;
        });
        trs.show();
    });
    $('#users').keyup(function () {
        var val = $(this).val().toLowerCase();
        $('#tablegrid tbody tr').hide();
        var trs = $('table tbody tr').filter(function (d) {
            return $(this).text().toLowerCase().indexOf(val) != -1;
        });
        trs.show();
    });
    $('#steps').keyup(function () {
        var val = $(this).val().toLowerCase();
        $('#tablegrid tbody tr').hide();
        var trs = $('table tbody tr').filter(function (d) {
            return $(this).text().toLowerCase().indexOf(val) != -1;
        });
        trs.show();
    });
    $('#tests').keyup(function () {
        var val = $(this).val().toLowerCase();
        $('#tablegrid tbody tr').hide();
        var trs = $('table tbody tr').filter(function (d) {
            return $(this).text().toLowerCase().indexOf(val) != -1;
        });
        trs.show();
    });
});


$(document).ready(function () {
    $('#name').keyup(function () {
        var val = $(this).val().toLowerCase();
        $('#gridtable tbody tr').hide();
        var trs = $('table tbody tr').filter(function (d) {
            return $(this).text().toLowerCase().indexOf(val) != -1;
        });
        trs.show();
    });
    $('#email').keyup(function () {
        var val = $(this).val().toLowerCase();
        $('#gridtable tbody tr').hide();
        var trs = $('table tbody tr').filter(function (d) {
            return $(this).text().toLowerCase().indexOf(val) != -1;
        });
        trs.show();
    });
    $('#UserType').change(function () {
        var val = $(this).val().toLowerCase();
        $('#gridtable tbody tr').hide();
        var trs = $('table tbody tr').filter(function (d) {
            return $(this).text().toLowerCase().indexOf(val) != -1;
        });
        trs.show();
    });
    $('#modules').keyup(function () {
        var val = $(this).val().toLowerCase();
        $('#gridtable tbody tr').hide();
        var trs = $('table tbody tr').filter(function (d) {
            return $(this).text().toLowerCase().indexOf(val) != -1;
        });
        trs.show();
    });
    $('#tests').keyup(function () {
        var val = $(this).val().toLowerCase();
        $('#gridtable tbody tr').hide();
        var trs = $('table tbody tr').filter(function (d) {
            return $(this).text().toLowerCase().indexOf(val) != -1;
        });
        trs.show();
    });
});

//$(document).ready(function () {
//    $(".draggable").draggable({
//        revert: true,
//        helper: 'clone',
//        start: function (event, ui) {
//            $(this).fadeTo('fast', 0.5);
//        },
//        stop: function (event, ui) {
//            $(this).fadeTo(0, 1);
//        }
//    });

//    $(".droppable").droppable({
//        hoverClass: 'active',
//        drop: function (event, ui) {
//            debugger;
//            if ($(ui.draggable).attr("id") == $(this).attr("id")) {
//                $('.correct').addClass('efct');
//                $('.overlay').show();
//                setTimeout(function () {
//                    $('.correct').removeClass('efct');
//                    $('.overlay').hide();
//                }, 1000);

//                $(this).html($(ui.draggable).text().trim());
//                //   $(ui.draggable).fadeTo(0,0.5);
//                return;
//            }
//            else {
//                $('.wrong').addClass('efct');
//                $('.overlay').show();
//                setTimeout(function () {
//                    $('.wrong').removeClass('efct');
//                    $('.overlay').hide();
//                }, 1000);

//                //  $(ui.draggable).fadeTo(0, 1);
//                $(this).html("");
//                return;
//            }
//        }
//    });
//});
