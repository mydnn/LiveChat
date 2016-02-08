
/*
* Layout
*/

(function () {
    $(document).ready(function () {
        //Get saved layout type from LocalStorage
        var layoutStatus = localStorage.getItem('ma-layout-status');
        if (layoutStatus == null || layoutStatus == undefined || layoutStatus == 1) {
            $('.body').addClass('sw-toggled');
            $('#tw-switch').prop('checked', true);
        }

        $('body').on('change', '#toggle-width input:checkbox', function () {
            if ($(this).is(':checked')) {
                setTimeout(function () {
                    $('.body').addClass('toggled sw-toggled');
                    localStorage.setItem('ma-layout-status', 1);
                }, 250);
            }
            else {
                setTimeout(function () {
                    $('.body').removeClass('toggled sw-toggled');
                    localStorage.setItem('ma-layout-status', 0);
                    $('.main-menu > li').removeClass('animated');
                }, 250);
            }
        });
    });
})();


/*
 * Detact Mobile Browser
 */
if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
    $('html').addClass('ismobile');
}

$(document).ready(function () {

    /*
     * Sidebar
     */
    (function () {
        //Toggle
        $('.body').on('click', '#menu-trigger, #log-trigger', function (e) {
            e.preventDefault();
            var x = $(this).data('trigger');

            $(x).toggleClass('toggled');
            $(this).toggleClass('open');
            $('.body').toggleClass('modal-open');

            //Close opened sub-menus
            $('.sub-menu.toggled').not('.active').each(function () {
                $(this).removeClass('toggled');
                $(this).find('ul').hide();
            });

            $('.profile-menu .main-menu').hide();

            if (x == '#sidebar') {
                $elem = '#sidebar';
                $elem2 = '#menu-trigger';

                $('#log-trigger').removeClass('open');

                if (!$('#action-center').hasClass('toggled')) {
                    $('#header').toggleClass('sidebar-toggled');
                }
                else {
                    $('#action-center').removeClass('toggled');
                }
            }

            if (x == '#action-center') {
                $elem = '#action-center';
                $elem2 = '#log-trigger';

                $('#menu-trigger').removeClass('open');

                if (!$('#sidebar').hasClass('toggled')) {
                    $('#header').toggleClass('sidebar-toggled');
                }
                else {
                    $('#sidebar').removeClass('toggled');
                }
            }

            //When clicking outside
            if ($('#header').hasClass('sidebar-toggled')) {
                $(document).on('click', function (e) {
                    if (($(e.target).closest($elem).length === 0) && ($(e.target).closest($elem2).length === 0)) {
                        setTimeout(function () {
                            $('.body').removeClass('modal-open');
                            $($elem).removeClass('toggled');
                            $('#header').removeClass('sidebar-toggled');
                            $($elem2).removeClass('open');
                        });
                    }
                });
            }
        })

        //Submenu
        $('.body').on('click', '.sub-menu > a', function (e) {
            e.preventDefault();
            $(this).next().slideToggle(200);
            $(this).parent().toggleClass('toggled');
        });
    })();

    /*
     * Clear Notification
     */
    $('.body').on('click', '[data-clear="notification"]', function (e) {
        e.preventDefault();

        var x = $(this).closest('.listview');
        var y = x.find('.lv-item');
        var z = y.size();

        $(this).parent().fadeOut();

        x.find('.list-group').prepend('<i class="grid-loading hide-it"></i>');
        x.find('.grid-loading').fadeIn(1500);


        var w = 0;
        y.each(function () {
            var z = $(this);
            setTimeout(function () {
                z.addClass('animated fadeOutRightBig').delay(1000).queue(function () {
                    z.remove();
                });
            }, w += 150);
        })

        //Popup empty message
        setTimeout(function () {
            $('#notifications').addClass('empty');
        }, (z * 150) + 200);
    });

    /*
     * Dropdown Menu
     */
    if ($('.dropdown')[0]) {
        //Propagate
        $('.body').on('click', '.dropdown.open .dropdown-menu', function (e) {
            e.stopPropagation();
        });

        $('.dropdown').on('shown.bs.dropdown', function (e) {
            if ($(this).attr('data-animation')) {
                $animArray = [];
                $animation = $(this).data('animation');
                $animArray = $animation.split(',');
                $animationIn = 'animated ' + $animArray[0];
                $animationOut = 'animated ' + $animArray[1];
                $animationDuration = ''
                if (!$animArray[2]) {
                    $animationDuration = 500; //if duration is not defined, default is set to 500ms
                }
                else {
                    $animationDuration = $animArray[2];
                }

                $(this).find('.dropdown-menu').removeClass($animationOut)
                $(this).find('.dropdown-menu').addClass($animationIn);
            }
        });

        $('.dropdown').on('hide.bs.dropdown', function (e) {
            if ($(this).attr('data-animation')) {
                e.preventDefault();
                $this = $(this);
                $dropdownMenu = $this.find('.dropdown-menu');

                $dropdownMenu.addClass($animationOut);
                setTimeout(function () {
                    $this.removeClass('open')

                }, $animationDuration);
            }
        });
    }

    /*
     * Custom Scrollbars
     */
    function scrollbar(className, color, cursorWidth) {
        $(className).niceScroll({
            cursorcolor: color,
            cursorborder: 0,
            cursorborderradius: 0,
            cursorwidth: cursorWidth,
            bouncescroll: true,
        });
    }

    //Scrollbar for HTML(not mobile) but not for login page
    if (!$('html').hasClass('ismobile')) {
        if (!$('.login-content')[0]) {
            scrollbar('html', 'rgba(0,0,0,0.3)', '5px');
        }

        //Scrollbar Tables
        if ($('.table-responsive')[0]) {
            scrollbar('.table-responsive', 'rgba(0,0,0,0.5)', '5px');
        }

        //Scrill bar for Chosen
        if ($('.chosen-results')[0]) {
            scrollbar('.chosen-results', 'rgba(0,0,0,0.5)', '5px');
        }

        //Scroll bar for tabs
        if ($('.tab-nav')[0]) {
            scrollbar('.tab-nav', 'rgba(0,0,0,0)', '1px');
        }

        //Scroll bar for dropdowm-menu
        if ($('.dropdown-menu .c-overflow')[0]) {
            scrollbar('.dropdown-menu .c-overflow', 'rgba(0,0,0,0.5)', '0px');
        }

        //Scrollbar for rest
        if ($('.c-overflow')[0]) {
            scrollbar('.c-overflow', 'rgba(0,0,0,0.5)', '5px');
        }
    }

    /*
    * Profile Menu
    */
    $('.body').on('click', '.profile-menu > a', function (e) {
        e.preventDefault();
        $(this).parent().toggleClass('toggled');
        $(this).next().slideToggle(200);
    });

    /*
     * Bootstrap Growl - Notifications popups
     */
    function notify(message, type) {
        $.growl({
            message: message
        }, {
            type: type,
            allow_dismiss: false,
            label: 'Cancel',
            className: 'btn-xs btn-inverse',
            placement: {
                from: 'top',
                align: 'right'
            },
            delay: 2500,
            animate: {
                enter: 'animated bounceIn',
                exit: 'animated bounceOut'
            },
            offset: {
                x: 20,
                y: 85
            }
        });
    };

    /*
     * Waves Animation
     */
    (function () {
        Waves.attach('.btn:not(.btn-icon)');
        Waves.attach('.btn-icon', ['waves-circle', 'waves-float']);
        Waves.init();
    })();

    /*
     * Collaspe Fix
     */
    if ($('.collapse')[0]) {

        //Add active class for opened items
        $('.collapse').on('show.bs.collapse', function (e) {
            $(this).closest('.panel').find('.panel-heading').addClass('active');
        });

        $('.collapse').on('hide.bs.collapse', function (e) {
            $(this).closest('.panel').find('.panel-heading').removeClass('active');
        });

        //Add active class for pre opened items
        $('.collapse.in').each(function () {
            $(this).closest('.panel').find('.panel-heading').addClass('active');
        });
    }

    /*
     * Tooltips
     */
    $('.body').tooltip({
        selector: '[data-toggle="tooltip"]'
    });
    /*
     * Popover
     */
    if ($('[data-toggle="popover"]')[0]) {
        $('[data-toggle="popover"]').popover();
    }

    /*
     * Message
     */

    //Actions
    if ($('.on-select')[0]) {
        var checkboxes = '.lv-avatar-content input:checkbox';
        var actions = $('.on-select').closest('.lv-actions');

        $('.body').on('click', checkboxes, function () {
            if ($(checkboxes + ':checked')[0]) {
                actions.addClass('toggled');
            }
            else {
                actions.removeClass('toggled');
            }
        });
    }

    if ($('#ms-menu-trigger')[0]) {
        $('.body').on('click', '#ms-menu-trigger', function (e) {
            e.preventDefault();
            $(this).toggleClass('open');
            $('.ms-menu').toggleClass('toggled');
        });
    }

    /*
     * Fullscreen Browsing
     */
    if ($('[data-action="fullscreen"]')[0]) {
        var fs = $("[data-action='fullscreen']");
        fs.on('click', function (e) {
            e.preventDefault();

            //Launch
            function launchIntoFullscreen(element) {

                if (element.requestFullscreen) {
                    element.requestFullscreen();
                } else if (element.mozRequestFullScreen) {
                    element.mozRequestFullScreen();
                } else if (element.webkitRequestFullscreen) {
                    element.webkitRequestFullscreen();
                } else if (element.msRequestFullscreen) {
                    element.msRequestFullscreen();
                }
            }

            //Exit
            function exitFullscreen() {

                if (document.exitFullscreen) {
                    document.exitFullscreen();
                } else if (document.mozCancelFullScreen) {
                    document.mozCancelFullScreen();
                } else if (document.webkitExitFullscreen) {
                    document.webkitExitFullscreen();
                }
            }

            launchIntoFullscreen(document.documentElement);
            fs.closest('.dropdown').removeClass('open');
        });
    }

    /*
     * Clear Local Storage
     */
    if ($('[data-action="clear-localstorage"]')[0]) {
        var cls = $('[data-action="clear-localstorage"]');

        cls.on('click', function (e) {
            e.preventDefault();

            swal({
                title: "Are you sure?",
                text: "All your saved localStorage values will be removed",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Yes, delete it!",
                closeOnConfirm: false
            }, function () {
                localStorage.clear();
                swal("Done!", "localStorage is cleared", "success");
            });
        });
    }

    /*
    * Date Time Picker
    */

    //Date Time Picker
    if ($('.date-time-picker')[0]) {
        $('.date-time-picker').datetimepicker();
    }

    //Time
    if ($('.time-picker')[0]) {
        $('.time-picker').datetimepicker({
            format: 'LT'
        });
    }

    /*
     * Text Feild
     */

    //Add blue animated border and remove with condition when focus and blur
    $('.body').on('focus', '.form-control', function () {
        $(this).closest('.fg-line').addClass('fg-toggled');
    })

    $('.body').on('blur', '.form-control', function () {
        var p = $(this).closest('.form-group');
        var i = p.find('.form-control').val();

        if (p.hasClass('fg-float')) {
            if (i.length == 0) {
                $(this).closest('.fg-line').removeClass('fg-toggled');
            }
        }
        else {
            $(this).closest('.fg-line').removeClass('fg-toggled');
        }
    });

    //Add blue border for pre-valued fg-flot text feilds
    $('.fg-float .form-control').each(function () {
        var i = $(this).val();

        if (!i.length == 0) {
            $(this).closest('.fg-line').addClass('fg-toggled');
        }

    });

    $('.sidebar-inner').jScrollPane({
        autoReinitialise: true
    });
});

(function ($, Sys) {
    mydnnNotify = function (message, type, delay, label, allow_dismiss, animate, placement, offset, className) {
        $.growl({
            message: message
        }, {
            type: type,
            allow_dismiss: allow_dismiss,
            label: label,
            className: className,
            placement: placement,
            delay: delay,
            animate: animate,
            offset: offset
        });
    };
}(jQuery, window.Sys));
