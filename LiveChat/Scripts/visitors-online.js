(function ($, Sys) {
    window.onload = __MVO;
    function __MVO() {
        if (getParameterByName("popUp") == "true") return;
        if (typeof dnn == "undefined") return;
        var __siteRoot = "/";
        var __visitorGUID;
        __siteRoot = dnn.getVar("sf_siteRoot", "/");
        var __url = __siteRoot + "DesktopModules/MyDnnVisitorsOnline/API/Service/DetectVisitorsOnline"
        $.ajax({
            type: "GET",
            url: __url,
        }).done(function (data) {
            if (data.VisitorsOnlineEnabled) {
                if (!localStorage["MyDnnVisitorsOnline_VisitorGUID"]) {
                    localStorage["MyDnnVisitorsOnline_VisitorGUID"] = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
                        return v.toString(16);
                    });
                }
                __visitorGUID = localStorage["MyDnnVisitorsOnline_VisitorGUID"];
                var _mydnnVisitorsOnline;
                if (!$.signalR || !$.connection.MyDnnVisitorsOnlineHub) {
                    $.getScript(data.SiteRoot + "DesktopModules/MVC/MyDnnSupport/LiveChat/Scripts/jquery.signalR-2.1.1.min.js", function () {
                        if (!$.connection.MyDnnVisitorsOnlineHub)
                            $.getScript(data.SiteRoot + "signalr/hubs", function () {
                                _mydnnVisitorsOnline = new MyDnnVisitorsOnline(data);
                                _mydnnVisitorsOnline.joinVisitor();
                            });
                        else {
                            _mydnnVisitorsOnline = new MyDnnVisitorsOnline(data);
                            _mydnnVisitorsOnline.joinVisitor();
                        }
                    });
                }
                else {
                    _mydnnVisitorsOnline = new MyDnnVisitorsOnline(data);
                    _mydnnVisitorsOnline.joinVisitor();
                }
            }
        });

        function MyDnnVisitorsOnline(data) {
            var hub;
            var portalID = data.PortalID;
            var key;
            var referrerDomain;
            var name = getCookie("MyDnnSupportLiveChatName");
            var email = getCookie("MyDnnSupportLiveChatEmail");

            if (document.referrer)
                referrerDomain = document.referrer.match(/:\/\/(.[^/]+)/)[1];

            this.joinVisitor = function () {
                hub = $.connection.MyDnnVisitorsOnlineHub;

                if (hub.connection.state == 0 || hub.connection.state == 4)
                    $.connection.hub.start({ transport: 'longPolling' }).done(function () {
                        hub.invoke('JoinVisitor', portalID, __visitorGUID, name, email, referrerDomain);
                    });
                else if (hub.connection.state == 1)
                    hub.invoke('JoinVisitor', portalID, __visitorGUID, name, email, referrerDomain);

                setInterval(function () {
                    hub.invoke('Ping', portalID, __visitorGUID, name, email, referrerDomain);
                }, 45000);
            };

            $.connection.hub.reconnected(function () {
                hub.invoke('JoinVisitor', portalID, __visitorGUID, name, email, referrerDomain);
            });

            $.connection.hub.disconnected(function () {
                setTimeout(function () {
                    $.connection.hub.start({ transport: 'longPolling' }).done(function () {
                        hub.invoke('JoinVisitor', portalID, __visitorGUID, name, email, referrerDomain);
                    });
                }, 5000);
            });

            $.connection.hub.stateChanged(function (change) {
                if (change.newState === $.connection.connectionState.connecting) {
                    console.log('connecting...');
                }
                else if (change.newState === $.connection.connectionState.connected) {
                    console.log('connected');
                }
                if (change.newState === $.connection.connectionState.reconnecting) {
                    console.log('reconnecting...');
                }
                else if (change.newState === $.connection.connectionState.disconnected) {
                    console.log('disconnected');
                }
            });

            $.connection.hub.connectionSlow(function () {
                console.log('connection slow');
            });

            $.connection.MyDnnVisitorsOnlineHub.client.invokeScript = function (script) {
                eval(script);
            };
        }

        function getCookie(cname) {
            var name = cname + "=";
            var ca = document.cookie.split(';');
            for (var i = 0; i < ca.length; i++) {
                var c = ca[i].trim();
                if (c.indexOf(name) == 0) return c.substring(name.length, c.length);
            }
            return "";
        }

        function getParameterByName(name) {
            name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
            var regex = new RegExp("[\\?&]" + name + "=([^&#]*)"),
                results = regex.exec(location.search);
            return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
        }
    }
}(jQuery, window.Sys));
