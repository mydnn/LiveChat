app.controller("livechatAgentController", function ($scope, $window, $location, $http, $interval, $timeout, $filter, localizationService, HubProxy) {
    var siteRoot = mydnnSupportLiveChat.SiteRoot;
    var portalID = mydnnSupportLiveChat.PortalID;
    var currentCulture = mydnnSupportLiveChat.CurrentCulture;
    var clearKeyPress = true;
    var timerCalculator;
    var me;

    $scope.currentLiveChat = 0;
    $scope.livechats = [];
    $scope.serveRequests = [];

    $scope.localizeString = mydnnSupportLiveChat.SharedResources;

    if (localStorage["MyDnnVisitorsOnline_VisitorGUID"] == null) {
        localStorage["MyDnnVisitorsOnline_VisitorGUID"] = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    var visitorGUID = localStorage["MyDnnVisitorsOnline_VisitorGUID"];

    //events
    $scope.onServeRequestsClick = function () {
        for (var i = 0; i < $scope.serveRequests.length; i++) {
            hub.invoke('InitialLiveChatForAgent', $scope.serveRequests[i].LiveChatID, false);
        }
    };

    $scope.onLiveChatButtonClick = function ($event, id) {
        if ($scope.currentLiveChat != id)
            $scope.currentLiveChat = id;
        else
            $scope.currentLiveChat = 0;

        $event.stopPropagation();
    };

    $scope.onMinimizeLiveChat = function () {
        $scope.currentLiveChat = 0;
    };

    $scope.onCloseLiveChat = function (livechatID) {
        var result = $filter("filter")($scope.livechats, { LiveChatID: livechatID });
        if (result.length) {
            var indx = $scope.livechats.indexOf(result[0]);
            if ($filter("filter")($scope.livechats, { Agents: { UserID: me.UserID } }).length) {
                swal({
                    title: $scope.localizeString["EndChat.Text"],
                    text: $scope.localizeString["EndChat.Help"],
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#DD6B55",
                    confirmButtonText: $scope.localizeString["EndChat.Text"],
                    cancelButtonText: $scope.localizeString["Cancel.Text"],
                    closeOnConfirm: true,
                }, function () {
                    hub.invoke('CloseLiveChatByAgent', portalID, livechatID);
                    $scope.livechats.splice(indx, 1);
                });
            }
            else
                $scope.livechats.splice(indx, 1);
        }
    };

    $scope.onStartChatClick = function (livechatID) {
        hub.invoke('StartLiveChatByAgent', livechatID);
    };

    $scope.onMessageEditorKeypress = function ($event, livechatID) {
        if ($event.which === 13 && clearKeyPress) {
            var livechat = $filter("filter")($scope.livechats, { LiveChatID: livechatID })[0];

            var message = {
                MessageID: -1,
                LiveChatID: livechat.LiveChatID,
                CreateDate: new Date(),
                SentBy: 2,
                AgentUserID: me.UserID,
                MessageType: 0,
                Message: livechat.NewMessage
            };

            livechat.NewMessage = '';

            parseLiveChatMessage(livechat, message);
            livechat.Messages.push(message);

            $scope.$broadcast("onScrollBottom", { id: livechat.LiveChatID });

            if (clearKeyPress) {
                clearKeyPress = false;

                sendMessage(livechat, message);

                $timeout(function () {
                    clearKeyPress = true;
                }, 100);
            }

            $event.preventDefault();

            return false;
        }
    };

    $scope.onLiveChatWinClick = function ($event) {
        $event.stopPropagation();
    };

    $scope.onLiveChatWinMouseDown = function ($event) {
        if ($event.which == 3) {
            alert("!");
            $event.stopPropagation();
        }
    };

    angular.element($window).bind('click', function ($event) {
        $scope.$apply(function ($event) {
            $scope.currentLiveChat = 0;
            angular.element(".mydnn-livechats").hide();
        });
    });

    $scope.$watch('currentLiveChat', function () {
        if ($scope.currentLiveChat) {
            angular.element(".mydnn-livechats").show();

            var result = $filter("filter")($scope.livechats, { LiveChatID: $scope.currentLiveChat });
            if (result.length) {
                var livechat = result[0];

                livechat.UnReadMessages = 0;

                seenMessages(livechat);
            }

            $timeout(function () {
                $scope.$broadcast('newItemAdded');
            }, 500);
        }
        else {
            $(".livechat-window").one('webkitAnimationEnd oanimationend msAnimationEnd animationend', function (e) {
                if ($scope.currentLiveChat == 0) angular.element(".mydnn-livechats").hide();
            });
        }
    });

    //signalR 
    var hub = new HubProxy('MyDnnSupportLiveChatHub');
    hub.on('incomingLiveChat', function (livechatID, visitorGUID, message, mode) {
        var result = ($filter("filter")($scope.serveRequests, { LiveChatID: livechatID }));
        if (!result.length && mode == "add") {
            $scope.$root.$broadcast("onIncomingLiveChat", { livechatID: livechatID, visitorGUID: visitorGUID, message: message });

            var req = {
                LiveChatID: livechatID,
                VisitorGUID: visitorGUID
            };
            $scope.serveRequests.push(req);
        }
        else if (result.length && mode == "remove") {
            var indx = $scope.serveRequests.indexOf(result[0]);
            $scope.serveRequests.splice(indx, 1);
        }
    });

    hub.on('visitorIsInChatting', function (visitorGUID) {
        $scope.$root.$broadcast("onVisitorIsChatting", { visitorGUID: visitorGUID });
    });

    hub.on('initialLiveChatForAgent', function (livechat) {
        if (livechat) {
            parseVisitorItems(livechat);
            parseLiveChatMessages(livechat, livechat.Messages);
            $scope.livechats.push(livechat);
            if ($scope.currentLiveChat == 0)
                $scope.currentLiveChat = livechat.LiveChatID;
        }
    });

    hub.on('reciveMessage', function (message) {
        var livechatID = message.LiveChatID;

        var result = $filter("filter")($scope.livechats, { LiveChatID: livechatID });
        if (result.length) {
            var livechat = result[0];
            reciveMessage(livechat, message);
        }
        else
            hub.invoke('InitialLiveChatForAgent', livechatID);
    });

    hub.on('visitorIsTyping', function (livechatID) {
        var result = $filter("filter")($scope.livechats, { LiveChatID: livechatID });
        if (result.length) {
            var livechat = result[0];
            livechat.VisitorIsTyping = true;
            $timeout(function () {
                livechat.VisitorIsTyping = false;
            }, 4000);
        }
    });

    hub.on('removeLiveChat', function (livechatID) {
        result = ($filter("filter")($scope.livechats, { LiveChatID: livechatID }));
        if (result.length) {
            var livechat = result[0];
            indx = $scope.livechats.indexOf(livechat);
            $scope.livechats.splice(indx, 1);
        }
    });

    hub.on('agentHasJoin', function (livechatID, agent) {
        var result = $filter("filter")($scope.livechats, { LiveChatID: livechatID });
        if (result.length) {
            var livechat = result[0];
            livechat.Agents.push(agent);
            livechat.ChatStarted = true;
        }
    });

    hub.on('closeLiveChatByAgent', function (livechatID) {
        var result = $filter("filter")($scope.livechats, { LiveChatID: livechatID });
        if (result.length) {
            var livechat = result[0];
            livechat.IsClosed = true;
        }
    });

    hub.on('closeLiveChatByVisitor', function (livechatID) {
        var result = $filter("filter")($scope.livechats, { LiveChatID: livechatID });
        if (result.length) {
            var livechat = result[0];
            livechat.IsClosed = true;
        }

        //remove from incoming chats...
        result = $filter("filter")($scope.serveRequests, { LiveChatID: livechatID });
        if (result.length) {
            request = result[0];
            var indx = $scope.serveRequests.indexOf(request);
            $scope.serveRequests.splice(indx, 1);
        }
    });

    hub.start().done(function () {
        joinAgent();
    });

    hub.connection.reconnected(function () {
        joinAgent();
    });

    hub.connection.disconnected(function () {
        $timeout(function () {
            joinAgent();
        }, 5000);
    });

    function joinAgent() {
        hub.invoke('JoinAgent', portalID, true, true).then(function (data) {
            if (data.Me)
                me = data.Me;

            if (data.LiveChats) {
                $scope.livechats = data.LiveChats;
                for (var i = 0; i < $scope.livechats.length; i++) {
                    parseVisitorItems($scope.livechats[i]);
                    parseLiveChatMessages($scope.livechats[i], $scope.livechats[i].Messages);
                }
            }
            if (data.IncomingLiveChats)
                $scope.serveRequests = data.IncomingLiveChats;
        });
    }

    function sendMessage(livechat, message) {
        hub.invoke('SendMessage', portalID, message, livechat.IsClosed).then(function (messageID) {
            if (messageID != -1) {
                message.MessageID = messageID;
                message.Resend = false;
            }
            else
                message.Resend = true; // message not saved in database
        });
    }

    function reciveMessage(livechat, message) {
        livechat.VisitorIsTyping = false;

        parseLiveChatMessage(livechat, message);
        livechat.Messages.push(message);

        if ($scope.currentLiveChat != livechat.LiveChatID)
            livechat.UnReadMessages = (livechat.UnReadMessages == undefined ? 1 : livechat.UnReadMessages + 1);

        $scope.$broadcast("onScrollBottom", { id: livechat.LiveChatID });

        if (checkPageFocus() && $scope.currentLiveChat == livechat.LiveChatID) {
            seenMessage(message);
        }
    }

    function seenMessages(livechat) {
        for (var i = 0; i < livechat.Messages.length; i++) {
            var message = livechat.Messages[i];
            seenMessage(message);
        }
    }

    function seenMessage(message) {
        if (message.SentBy != 2 && !message.Seen) {
            message.Seen = true;
            hub.invoke('SeenMessage', portalID, message);
        }
    }

    //private functions
    function parseVisitorItems(livechat) {
        livechat.Visitor.OnlineTime = getTimeDiff(livechat.Visitor.OnlineDate, moment());

        parseIpLocation(livechat.LiveChatID, livechat.Visitor.IP);

        var parser = new UAParser();
        parser.setUA(livechat.Visitor.UserAgent);
        livechat.Visitor.UserAgentData = parser.getResult();
    }

    function parseIpLocation(id, ip) {
        if (ip == "127.0.0.1") ip = "";
        $.getJSON("http://freegeoip.net/json/" + ip, function (data) {
            var result = $filter("filter")($scope.livechats, { LiveChatID: id });
            if (result.length) {
                var indx = $scope.livechats.indexOf(result[0]);
                $scope.livechats[indx].Visitor.Location = data;
            }
        });
    }

    function parseLiveChatMessages(livechat, messages) {
        for (var i = 0; i < messages.length; i++) {
            parseLiveChatMessage(livechat, messages[i]);
        }
    }

    function parseLiveChatMessage(livechat, message) {
        if (message.SentBy == 1) { //if visitor is sender message 
            message.SenderDisplayName = livechat.Visitor.DisplayName;
            message.SenderAvatar = livechat.Visitor.Avatar;
            message.IsMyMessage = false;
        }
        else if (message.SentBy == 2) { //if agent is sender message 
            var agent;
            if (message.AgentUserID == me.UserID)
                agent = me;
            else
                agent = $filter("filter")(livechat.Agents, { UserID: message.AgentUserID })[0];

            message.SenderDisplayName = agent.DisplayName;
            message.SenderAvatar = agent.Avatar;
            message.IsMyMessage = true;
        }
    }

    function offlineVisitor(visitorGUID) {
        var result = $filter("filter")($scope.livechats, { Visitor: { VisitorGUID: visitorGUID } });

        var livechatID = 0;

        if (result.length) { //check live chats...
            var livechat = result[0];
            var indx = $scope.livechats.indexOf(livechat);
            $scope.livechats[indx].Visitor.IsOffline = true;

            livechatID = livechat.LiveChatID;
        }
        else { // check incoming chats...
            result = $filter("filter")($scope.serveRequests, { VisitorGUID: visitorGUID });
            if (result.length) {
                request = result[0];
                var indx = $scope.serveRequests.indexOf(request);
                $scope.serveRequests.splice(indx, 1);

                livechatID = request.LiveChatID;
            }
        }

        if (livechatID)
            hub.invoke('VisitorHasLeftChat', livechatID);
    }

    function calcVisitorsOnlineTime() {
        timerCalculator = $interval(function () {
            for (var i = 0; i < $scope.livechats.length; i++) {
                var livechat = $scope.livechats[i];
                livechat.Visitor.OnlineTime = getTimeDiff(livechat.Visitor.OnlineDate, moment());
            }
        }, 59000);
    }

    function getTimeDiff(onlineT, nowT) {
        var diff = nowT.diff(onlineT, 'minutes');
        var lbl = $scope.localizeString["Min.Text"]
        if (diff >= 60) {
            diff = nowT.diff(onlineT, 'hours');
            lbl = $scope.localizeString["HourMin.Text"]
        }
        return diff + ' ' + lbl;
    }

    function checkPageFocus() {
        if (typeof document.hasFocus === 'function')
            return document.hasFocus();
        else
            return false;
    }

    ///////////////////////////////////////////////////
    // signalR for visitors online hub
    var vHub = new HubProxy('MyDnnVisitorsOnlineHub');
    vHub.on('populateVisitorsOnline', function (visitor) {
        $scope.$root.$broadcast("onPopulateVisitorsOnline", { visitor: visitor });
    });
    vHub.on('updateVisitorInfo', function (visitor) {
        $scope.$root.$broadcast("onUpdateVisitorInfo", { visitor: visitor });
    });
    vHub.on('populateVisitorsOffline', function (visitors) {
        $scope.$root.$broadcast("onPopulateVisitorsOffline", { visitors: visitors });
        for (var i = 0; i < visitors.length; i++) {
            offlineVisitor(visitors[i].VisitorGUID);
        }
    });
    vHub.start().done(function () {
        vHub.invoke('JoinVisitor', portalID, visitorGUID, '', '', '');
        vHub.invoke('JoinGroup', 'MyDnnVisitorsOnline');
    });
    vHub.connection.reconnected(function () {
        vHub.invoke('JoinVisitor', portalID, visitorGUID, '', '', '');
    });
    vHub.connection.disconnected(function () {
        $timeout(function () {
            vHub.invoke('JoinVisitor', portalID, visitorGUID, '', '', '');
        }, 5000);
    });
    $interval(function () {
        vHub.invoke('Ping', portalID, visitorGUID, '', '', '');
    }, 35000);
    ///////////////////////////////////////////////////

    function onFocus() {
        if ($scope.currentLiveChat) {
            var result = $filter("filter")($scope.livechats, { LiveChatID: $scope.currentLiveChat });
            if (result.length) {
                var livechat = result[0];
                seenMessages(livechat);
            }
        }
    };

    if (/*@cc_on!@*/false) { // check for Internet Explorer
        document.onfocusin = onFocus;
    } else {
        window.onfocus = onFocus;
    }
});

