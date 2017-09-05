$(document).ready(function () {
    $('.input_sec input').each(function () {
        if ($(this).val() != '') {
            $(this).prev().addClass('adlbl');
        }
    });
       

    $('.stp_lt ul li a,.nxt_prv a').on('click', function (e) {
        
        var currentAttrValue = $(this).attr('href');
        // Show/Hide Tabs
        $('.stp_rt ' + currentAttrValue).show().siblings().hide();
        // Change/remove current tab to active
        $(this).parent('li').addClass('active').siblings().removeClass('active');
    });
    var url = location.href.split("#");
    var tab = url[1];
    $('a[href="#' + tab + '"]').parent().addClass('active').siblings().removeClass('active');
    $('#' + tab + '').show().siblings().hide();
    $('.nxt_prv a').click(function () {
        var hrf = $(this).attr('href');
        $('.stp_lt ul li a').each(function () {
            var hrfLi = $(this).attr('href');
            if (hrf == hrfLi) {
                $(this).parent().addClass('active').siblings().removeClass('active');
            }
        });
    });




    $('.tbs .tb_links a').on('click', function (e) {
        
        var currentAttrValue = $(this).attr('href');
        $('.tab_content div' + currentAttrValue).show().siblings().hide();
        $(this).parent('li').addClass('active').siblings().removeClass('active');
        e.preventDefault();
    });

    /*steps*/
    
        $('.edit_pr').click(function () {
            $('.prf_tbl').removeClass('prf_edt').addClass('prf_edt');
            $('.prf_tbl input,.prf_tbl textarea').removeAttr('readonly');
            $('#UserName,#Email').attr('readonly', 'readonly');
            $('.img_upld,.cng_pwd').show();
            $('#SelectState select').attr("disabled", false);
        });
        $('.cng_pwd').click(function () {
            $('.pwd_cng').show();
            $(this).hide();
        });
        $('.dsn_pwd').click(function () {
            $('.pwd_cng').hide();
            $('.cng_pwd').show();
        }); 
        $('.dsn_frm').click(function () {
            $('.prf_tbl').removeClass('prf_edt');
            $('.cng_pwd,.img_upld').hide();
            $('.prf_tbl input,.prf_tbl textarea').attr('readonly', 'readonly');
            $('#SelectState select').attr("disabled", true);
        });
        $('.sv_pf').click(function () {
            $('.prf_tbl').removeClass('prf_edt');
            $('.cng_pwd,.img_upld').hide();
            $('.prf_tbl input,.prf_tbl textarea').attr('readonly', 'readonly');
        });

        $('.input_sec input,.input_sec textarea').focusin(function () {
            $(this).prev().addClass('adlbl');
        });
        $('.input_sec input,.input_sec textarea').focusout(function () {
            if ($(this).val() == '') {
                $(this).prev().removeClass('adlbl');
            }
        });
        $('.prf_tbl.prf_edt input,.prf_tbl.prf_edt select,.prf_tbl.prf_edt textarea').removeAttr("readonly");
        $('.prf_tbl input,.prf_tbl select,.prf_tbl textarea').attr("readonly", "readonly");
    /*steps*/
        $(document).ready(function () {
            $('#type_sec').on('change', function () {
                if (this.value == '1')
                {
                    $("#content").show();
                    $("#video").hide();
                }
                else if (this.value == '2') {
                    $("#content").hide();
                    $("#video").show();
                }
            });
        });
        $('.ed_hd').click(function () {
            $('.ed_hd,.dlt').hide();
            $('.ed_sh').show();
            $('ed_hd')
        });
        $('.ed_sh').click(function () {
            $('.ed_sh').hide();
            $('.ed_hd,.dlt').show();
        });
        $('.dlt').click(function () {
            $(this).closest('.step_m').hide();
            $(this).closest('.step_m').next().show();
        });
        $('.shw_res,span.cls').click(function () {
            $('.answ_sec').toggleClass('bt_mn');
        });
        $('body').on('click', '.crs', function () {
            window.location = "http://183.82.112.191:9252/user/course.html";
        });

      

    // on page load...
        moveProgressBar();
    // on browser resize...
        $(window).resize(function () {
            moveProgressBar();
        });

    // SIGNATURE PROGRESS
        function moveProgressBar() {
            console.log("moveProgressBar");
            var getPercent = ($('.progress-wrap').data('progress-percent') / 100);
            var getProgressWrapWidth = $('.progress-wrap').width();
            var progressTotal = getPercent * getProgressWrapWidth;
            var animationLength = 2500;

            // on page load, animate percentage bar to data percentage length
            // .stop() used to prevent animation queueing
            $('.progress-bar').stop().animate({
                left: progressTotal
            }, animationLength);
        }


    
});