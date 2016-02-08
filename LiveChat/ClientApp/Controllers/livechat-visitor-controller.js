angular.module('MyDnnSupportLiveChatApp', ['services.hub', 'ngMyDnnServices'])
.controller("livechatController", function ($scope, $location, $http, $interval, $timeout, $window, $compile, $filter, HubProxy) {
    var rootUrl = mydnnLiveChatBaseData.RootUrl;
    var visitorGUID = mydnnLiveChatBaseData.VisitorGUID;
    var portalID;
    var isTyping = false;

    $scope.livechat = {};
    $scope.livechatFormValidate = false;
    $scope.waiting = false;

    $http.get(rootUrl + "DesktopModules/MyDnnSupport.LiveChat/API/VisitorService/InitialLiveChat", { params: { visitorGUID: visitorGUID } }).success(function (data) {
        portalID = data.PortalID;

        $scope.WidgetHtml = data.Widget;

        $scope.localizeString = data.Resources;
        $scope.widgetSettings = data.Settings;
        $scope.isAgentOnline = data.IsAgentOnline;
        $scope.departments = data.Departments;

        $scope.livechat.Step = "prechat";
        $scope.livechat.ConnectionState = 1;
        $scope.livechat.LiveChatID = -1;
        $scope.livechat.Visitor = { VisitorGUID: visitorGUID, UserID: data.UserID };
        $scope.livechat.Departments = [];
        $scope.livechat.Agents = [];
        $scope.livechat.WidgetMinimized = false;

        if (data.UserID != -1) {
            $scope.livechat.Visitor.DisplayName = data.DisplayName;
            $scope.livechat.Visitor.Email = data.Email;
        }
        else {
            var name = getCookie("MyDnnSupportLiveChatName");
            var email = getCookie("MyDnnSupportLiveChatEmail");
            if (name) $scope.livechat.Visitor.DisplayName = name;
            if (email) $scope.livechat.Visitor.Email = email;
        }

        $('body').append('<style type="text/css">.livechat-theme-bgcolor{background-color:' + data.Settings.LiveChatThemeColor + ';}.livechat-theme-color{color:' + data.Settings.LiveChatThemeColor + ';}.livechat-theme-titlecolor{color:' + data.Settings.LiveChatTitleColor + ';}.livechat-focusable:focus{border-color:' + data.Settings.LiveChatThemeColor + ' !important;}</style>');
        $timeout(function () {
            if (typeof jQuery.ui) $(".mydnn-livechat-widget").draggable({ handle: '.livechat-move' });
        }, 200);
    });

    $scope.reloadLiveChat = function () {
        $scope.livechat.WidgetMinimized = false;
        $http.get(rootUrl + "DesktopModules/MyDnnSupport.LiveChat/API/VisitorService/GetDepartmentsForLiveChat").success(function (data) {
            $scope.departments = data.Departments;
            $scope.isAgentOnline = data.IsAgentOnline;
            $scope.department = null;
        });
    };

    //events 
    $scope.onLiveChatMinimizeClick = function () {
        $scope.livechat.WidgetMinimized = true;
        angular.element("#mydnnLiveChatMinButton").show();
    };

    $scope.onDepartmentChanged = function () {
        if ($scope.department)
            $http.get(rootUrl + "DesktopModules/MyDnnSupport.LiveChat/API/VisitorService/IsAgentOnlineByDepartment", { params: { departmentID: $scope.department.DepartmentID } }).success(function (isAgentOnline) {
                var result = $filter("filter")($scope.departments, { DepartmentID: $scope.department.DepartmentID });
                if (result.length) {
                    var indx = $scope.departments.indexOf(result[0]);
                    $scope.departments[indx].IsAgentOnline = isAgentOnline;
                }
                $scope.isAgentOnline = isAgentOnline;
            });
    };

    $scope.onStartChatClick = function () {
        if (!$scope.isAgentOnline) // send offline message
        {
            sendOfflineMessageEmail();
            return;
        }

        $scope.livechatFormValidate = true;
        if ($scope.department && $scope.livechatForm.$valid) {
            $scope.livechat.Departments = [$scope.department];
            hub.invoke('StartLiveChatByVisitor', $scope.livechat).then(function (livechatID) {
                localStorage["MyDnnLiveChatCurrentChat"] = livechatID;
                if (livechatID)
                    $scope.livechat.LiveChatID = livechatID;
                else
                    window.location.reload();
            });
            setCookie("MyDnnSupportLiveChatName", $scope.livechat.Visitor.DisplayName, 1000);
            setCookie("MyDnnSupportLiveChatEmail", $scope.livechat.Visitor.Email, 1000);
        }
    };

    $scope.onResendMessage = function (message) {
        sendMessage(message);
    };

    $scope.onBlurInput = function ($event) {
        angular.element($event.target).addClass('control-focused');
    };

    $scope.onMessageEditorKeypress = function ($event) {
        if ($event.which === 13) {
            if (!$scope.livechat.NewMessage) return; //empty message

            if (hub.connection.state != 1) { //check hub connected
                $scope.livechat.ConnectionState = 2;
                return false;
            }

            var message = {
                MessageID: -1,
                LiveChatID: $scope.livechat.LiveChatID,
                CreateDate: new Date(),
                SentBy: 1,
                MessageType: 0,
                Message: $scope.livechat.NewMessage
            };

            $scope.livechat.NewMessage = '';

            sendMessage(message);

            $event.preventDefault();
        }
        else { //visitor is typing...
            if (!isTyping) {
                isTyping = true;

                visitorIsTyping();

                $timeout(function () {
                    isTyping = false;
                }, 4000);
            }
        }
    };

    $scope.onChatRateClick = function (rate) {
        if (rate == $scope.livechat.Rate)
            rate = 0;

        hub.invoke('RateChat', $scope.livechat.LiveChatID, rate).then(function (isSuccess) {
            $scope.livechat.Rate = rate;

            var message = {
                MessageID: -1,
                LiveChatID: $scope.livechat.LiveChatID,
                CreateDate: new Date(),
                SentBy: 0,
                MessageType: 0,
                Message: rate == 0 ? $scope.localizeString["ChatRatingRemoved.Text"] : (rate == 1 ? $scope.localizeString["ChatRatedGood.Text"] : $scope.localizeString["ChatRatedBad.Text"])
            };

            sendMessage(message);
        });
    };

    $scope.onOptionsClick = function ($event) {
        angular.element(".livechat-email-transcript").hide();

        if (!angular.element(".livechat-popupmenu").is(":visible"))
            angular.element(".livechat-popupmenu").fadeIn(50);
        else
            angular.element(".livechat-popupmenu").fadeOut(50);

        $event.stopPropagation();
    };

    $scope.onShowEmailClick = function ($event) {
        angular.element(".livechat-popupmenu").hide();

        angular.element(".livechat-email-transcript").fadeIn(50);

        $event.stopPropagation();
    };

    $scope.onEmailInputClick = function ($event) {
        $event.stopPropagation();
    };

    $scope.onSendEmailClick = function ($event) {
        sendTranscriptEmail();

        $event.stopPropagation();
    };

    $scope.onBtnEndChatClick = function ($event) {
        if (confirm($scope.localizeString["VisitorEndChatConfirm.Text"])) {
            hub.invoke('CloseLiveChatByVisitor', portalID, $scope.livechat.LiveChatID);
            angular.element(".livechat-popupmenu").fadeOut(50);
        }

        $event.stopPropagation();
    };

    angular.element($window).bind('click', function ($event) {
        $scope.$apply(function ($event) {
            angular.element(".livechat-popupmenu").fadeOut(50);
            angular.element(".livechat-email-transcript").fadeOut(50);
        });
    });

    //signalR 
    var hub = new HubProxy('MyDnnSupportLiveChatHub');
    hub.on('startLiveChat', function (livechat) {
        $scope.livechat = livechat;
        parseLiveChatMessages($scope.livechat.Messages);
        $scope.livechat.Step = "chatstarted";
    });

    hub.on('agentHasJoin', function (livechatID, agent) {
        if (livechatID == $scope.livechat.LiveChatID) {
            var result = $filter("filter")($scope.livechat.Agents, { UserID: agent.UserID });
            if (!result.length) {
                $scope.livechat.Agents.push(agent);
            }
        }
    });

    hub.on('reciveMessage', function (message) {
        reciveMessage(message);
    });

    hub.on('seenMessage', function (messageID) {
        if ($scope.livechat.LiveChatID) {
            var result = $filter("filter")($scope.livechat.Messages, { MessageID: messageID });
            if (result.length) {
                result[0].Seen = true;
            }
        }
    });

    hub.on('closeLiveChatByAgent', function (livechatID) {
        closeChat(livechatID);
    });

    hub.on('closeLiveChatByVisitor', function (livechatID) {
        closeChat(livechatID);
    });

    hub.start().done(function () {
        hub.invoke('JoinVisitor', portalID, visitorGUID).then(function (livechat) {
            if (livechat) {
                $scope.department = { DepartmentID: livechat.DepartmentID };

                $scope.livechat = livechat;
                $scope.livechat.Step = "chatstarted";
                $scope.livechat.WidgetMinimized = false;

                parseLiveChatMessages($scope.livechat.Messages);

                localStorage["MyDnnLiveChatCurrentChat"] = livechat.LiveChatID;
            }
            else
                localStorage.removeItem("MyDnnLiveChatCurrentChat");
        });
    });

    hub.connection.reconnecting(function () {
        if ($scope.livechat.LiveChatID) {
            $scope.livechat.ConnectionState = 2;
        }
    });

    hub.connection.reconnected(function () {
        if ($scope.livechat.LiveChatID) {
            reconnectToLiveChat();
        }
    });

    hub.connection.disconnected(function () {
        if ($scope.livechat.LiveChatID) {
            $timeout(function () {
                hub.start().done(function () {
                    hub.invoke('JoinVisitor', portalID, visitorGUID);
                    reconnectToLiveChat();
                });
            }, 5000);
        }
    });

    function reconnectToLiveChat() {
        $scope.livechat.ConnectionState = 1;
        if ($scope.livechat.LiveChatID && !$scope.livechat.IsClosed) {
            var lastMessage = $scope.livechat.Messages[$scope.livechat.Messages.length - 1];
            if (lastMessage) {
                var lastMessageID = lastMessage.MessageID;
                hub.invoke('VisitorReconnectedToLiveChat', portalID, $scope.livechat.LiveChatID, lastMessageID).then(function (data) {
                    if (data) {
                        if (data.Messages && data.Messages.length) {
                            parseLiveChatMessages(data.Messages);
                            $scope.livechat.Messages.push.apply($scope.livechat.Messages, data.Messages);
                        }
                        $scope.livechat.IsClosed = data.IsClosed;
                    }
                });
            }
        }
    }

    function sendMessage(message) {
        $scope.livechat.Messages.push(message);
        parseLiveChatMessage(message);

        $scope.$broadcast("onScrollBottom", { id: $scope.livechat.LiveChatID });

        hub.invoke('SendMessage', portalID, message, $scope.livechat.IsClosed).then(function (messageID) {
            if (messageID != -1) {
                message.MessageID = messageID;
                message.Resend = false;
            }
            else
                message.Resend = true; // message not saved in database
        });
    }

    function reciveMessage(message) {
        var livechatID = message.LiveChatID;

        if (livechatID == $scope.livechat.LiveChatID) {
            $scope.livechat.Messages.push(message);
            parseLiveChatMessage(message);

            $scope.$broadcast("onScrollBottom", { id: $scope.livechat.LiveChatID });
        }
    }

    function visitorIsTyping() {
        hub.invoke('VisitorIsTyping', portalID, $scope.livechat.LiveChatID).then(function (messageID) {
        });
    }

    function closeChat(livechatID) {
        if ($scope.livechat.LiveChatID == livechatID) {
            $scope.livechat.IsClosed = true;
        }

        localStorage.removeItem("MyDnnLiveChatCurrentChat");
    }

    function sendTranscriptEmail() {
        $scope.waiting = true;

        var emailTemplate = $scope.widgetSettings["TranscriptEmailTemplate"];

        var html = $compile(emailTemplate)($scope);
        $('body').append($('<div id="mydnnLiveChatTemp" style="display:none;"></div>'));
        $('#mydnnLiveChatTemp').append(html);

        $timeout(function () {
            var data = {
                To: $scope.livechat.Visitor.Email,
                Subject: $scope.localizeString["ChatTranscript.Text"],
                Body: $('#mydnnLiveChatTemp').html()
            }

            $http.post(rootUrl + "DesktopModules/MyDnnSupport.LiveChat/API/VisitorService/SendEmail", data).success(function () {
                $scope.waiting = false;
                alert($scope.localizeString["EmailSentMessage.Text"] + " " + $scope.livechat.Visitor.Email);
                angular.element(".livechat-email-transcript").fadeOut(50);
            }).error(function (data, status, headers, config) {
                $scope.waiting = false;
                alert(data);
                angular.element(".livechat-email-transcript").fadeOut(50);
            });

            $('#mydnnLiveChatTemp').remove();
        }, 1000);
    }

    function sendOfflineMessageEmail() {
        alert($scope.localizeString["AfterSendOfflineMessage.Text"]);

        $scope.livechat.WidgetMinimized = true;
        angular.element("#mydnnLiveChatMinButton").show();
        $scope.livechatFormValidate = false;

        var emailTemplate = $scope.widgetSettings["OfflineEmailTemplate"];

        var html = $compile(emailTemplate)($scope);
        $('body').append($('<div id="mydnnLiveChatTemp" style="display:none;"></div>'));
        $('#mydnnLiveChatTemp').append(html);

        $timeout(function () {
            var data = {
                DepartmentID: $scope.department.DepartmentID,
                To: $scope.livechat.Visitor.Email,
                Subject: $scope.localizeString["OfflineMessageFrom.Text"] + " " + $scope.livechat.Visitor.DisplayName,
                Body: $('#mydnnLiveChatTemp').html()
            }

            $http.post(rootUrl + "DesktopModules/MyDnnSupport.LiveChat/API/VisitorService/SendOfflineMessageEmail", data).success(function () {
            }).error(function (data, status, headers, config) {
            });

            $scope.livechat.Message = '';

            $('#mydnnLiveChatTemp').remove();
        }, 1000);
    }


    //private functions
    function parseLiveChatMessages(messages) {
        for (var i = 0; i < messages.length; i++) {
            parseLiveChatMessage(messages[i]);
        }
    }

    function parseLiveChatMessage(message) {
        if (message.SentBy == 1) { //if visitor is sender message 
            message.SenderDisplayName = $scope.livechat.Visitor.DisplayName;
            message.SenderAvatar = $scope.livechat.Visitor.Avatar;
            message.IsMyMessage = true;
        }
        else if (message.SentBy == 2) { //if agent is sender message 
            var agent;
            agent = $filter("filter")($scope.livechat.Agents, { UserID: message.AgentUserID })[0];
            message.SenderDisplayName = agent.DisplayName;
            message.SenderAvatar = agent.Avatar;
            message.IsMyMessage = false;
        }
        message.MsgTime = moment(message.CreateDate).format("hh:mm")

        var indx = $scope.livechat.Messages.indexOf(message);
        if (indx > 0) {
            var prevMessage = $scope.livechat.Messages[indx - 1];
            if (prevMessage && prevMessage.SentBy == message.SentBy)
                message.SenderPrevMessage = true;
        }
    }

    function setCookie(cname, cvalue, exdays) {
        var d = new Date();
        d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
        var expires = "expires=" + d.toGMTString();
        document.cookie = cname + "=" + cvalue + "; " + expires;
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
});
