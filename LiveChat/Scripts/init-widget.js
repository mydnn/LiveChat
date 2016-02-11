var mydnnLiveChatBaseData;
var mydnnLiveChatRequests;
(function ($, Sys) {
    $(document).ready(function () {
        var __mydnnLiveChatRequests = [];
        var __isAgentOnline = false;
        var __livechatIsLoaded = false;
        var __requestsString;
        var __adminPanelUrl;
        var __siteRoot = "/";
        var __visitorGUID;
        var __portalID;
        var __me = this;
        var __counter = 0;

        if (typeof dnn == "undefined" || typeof mydnnSupportLiveChat != "undefined") return;

        $.ajax({
            type: "GET",
            url: dnn.getVar("sf_siteRoot", "/") + "DesktopModules/MyDnnSupport.LiveChat/API/VisitorService/LiveChatWidget",
        }).done(function (data) {
            if (typeof mydnnSupportLiveChat != "undefined") return; // This means that i am in adminpanel page

            __siteRoot = data.SiteRoot;
            __portalID = data.PortalID;

            if (data.LiveChatEnabled) {
                $('body').append('<link href="' + __siteRoot + 'DesktopModules/MVC/MyDnnSupport/LiveChat/Templates/' + data.Template + '/style.css" type="text/css" rel="stylesheet"/>');

                __visitorGUID = localStorage["MyDnnVisitorsOnline_VisitorGUID"];
                __isAgentOnline = data.IsAgentOnline;

                __me.initialLiveChat(data);
            }
        });

        this.initialLiveChat = function (data) {
            if (__counter++ < 50 && (typeof $.connection == "undefined")) {
                setTimeout(function () {
                    __me.initialLiveChat(data);
                }, 1000);
            }
            else {
                var $minButton = $('<div id="mydnnLiveChatMinButton"></div>').html(data.LiveChatMinButton);
                $('body').append($minButton);

                //check agent is online or offline and show button by visibility
                $('#mydnnLiveChatMinButton').find('[data-livechat-isonline]').hide();
                var status = (__isAgentOnline ? "online" : "offline");
                $('#mydnnLiveChatMinButton').find('[data-livechat-isonline="' + status + '"]').show();

                //when min button click
                $('#mydnnLiveChatMinButton').on('click', function () {
                    if (__livechatIsLoaded == false) {
                        __me.loadAngularAndScripts(data);
                        $("#mydnnLiveChatMinButton").hide();
                        __livechatIsLoaded = true;
                    }
                    else {
                        $("#mydnnLiveChatMinButton").hide();
                        angular.element(document.getElementById('mydnnLiveChatWidget')).scope().reloadLiveChat();
                    }
                });

                //if visitor is chatting 
                if (localStorage["MyDnnLiveChatCurrentChat"])
                    $('#mydnnLiveChatMinButton').trigger('click');

                //if visitor is dnn user and agent 
                if (data.IsAgent) {
                    __requestsString = data.RequestsString;
                    __adminPanelUrl = data.AdminPanelUrl;
                    __me.joinAgent();
                }
            }
        }

        this.joinAgent = function () {
            if (__counter++ < 50 && (typeof $.connection == "undefined" || $.connection.MyDnnSupportLiveChatHub.connection.state != 1)) {
                setTimeout(function () {
                    __me.joinAgent();
                }, 1000);
            }
            else {
                $.connection.MyDnnSupportLiveChatHub.invoke('JoinAgent', __portalID, true, false).done(function (data) {
                    if (data.IncomingLiveChats && data.IncomingLiveChats.length) {
                        __mydnnLiveChatRequests = data.IncomingLiveChats;
                        __me.liveChatRequests(0, "add");
                    }
                });
            }
        }

        mydnnLiveChatRequests = this.liveChatRequests = function (livechatID, mode) {
            var result = $.grep(__mydnnLiveChatRequests, function (e) { return e.LiveChatID == livechatID; });
            if (livechatID && !result.length && mode == "add") {
                var req = {
                    LiveChatID: livechatID,
                };
                __mydnnLiveChatRequests.push(req);
            }
            else if (result.length && mode == "remove") {
                var indx = __mydnnLiveChatRequests.indexOf(result[0]);
                __mydnnLiveChatRequests.splice(indx, 1);
                if (!__mydnnLiveChatRequests.length)
                    $("#mydnnSupportLiveChatRequests").fadeOut(100);
            }

            if (__mydnnLiveChatRequests.length) {
                if (!$('body').find('#mydnnSupportLiveChatRequests').length) {
                    $('body').append('<div id="mydnnSupportLiveChatRequests" class="livechat-serve-requests"></div>');
                    $("#mydnnSupportLiveChatRequests").click(function () {
                        window.location = __adminPanelUrl;
                    });
                }
                setTimeout(function () {
                    $("#mydnnSupportLiveChatRequests").html(__requestsString + "(" + __mydnnLiveChatRequests.length + ")");
                    $("#mydnnSupportLiveChatRequests").fadeIn(100);
                }, 500);
            }
        }

        this.loadAngularAndScripts = function (data) {
            if (typeof angular == "undefined")
                $.getScript(__siteRoot + "DesktopModules/MVC/MyDnnSupport/LiveChat/ClientComponents/angularjs/angular.min.js", function () {
                    __me.loadLiveChatScripts(data);
                });
            else
                __me.loadLiveChatScripts(data);
        }

        this.loadLiveChatScripts = function (data) {
            $.getScript(__siteRoot + "DesktopModules/MVC/MyDnnSupport/LiveChat/ClientApp/Services/signalr.service.js", function () {
                $.getScript(__siteRoot + "DesktopModules/MVC/MyDnnSupport/LiveChat/ClientApp/Services/ng-mydnn-services.js", function () {
                    $.getScript(__siteRoot + "DesktopModules/MVC/MyDnnSupport/LiveChat/ClientComponents/moment.js/moment.min.js", function () {
                        $.getScript(__siteRoot + "DesktopModules/MVC/MyDnnSupport/LiveChat/ClientApp/Controllers/livechat-visitor-controller.js", function () {
                            mydnnLiveChatBaseData = { SiteRoot: __siteRoot, VisitorGUID: __visitorGUID };
                            var $ang = $('<div id="mydnnSupportLiveChat" ng-app="MyDnnSupportLiveChatApp"><div ng-controller="livechatController"><div id="mydnnLiveChatWidget" dynamic="WidgetHtml"></div></div></div>');
                            $ang.appendTo($('body'));
                            angular.bootstrap(document.getElementById('mydnnSupportLiveChat'), ['MyDnnSupportLiveChatApp']);
                        });
                    });
                });
            });
        }

        this.getCookie = function (cname) {
            var name = cname + "=";
            var ca = document.cookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i].trim();
                if (c.indexOf(name) == 0) return c.substring(name.length, c.length);
            }
            return "";
        }
    });
}(jQuery, window.Sys));
