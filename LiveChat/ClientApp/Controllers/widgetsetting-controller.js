app.controller("widgetSettingController", function ($scope, $http, $timeout, $filter, $location, cfpLoadingBar, Upload, activeMenu) {
    activeMenu.setActiveMenu({ Parent: 'settings', Child: 'widget' });

    var siteRoot = mydnnSupportLiveChat.SiteRoot;
    var moduleSettings;
    var timeOut;
    var $self = {
        Headers: {
            ModuleId: mydnnSupportLiveChat.ModuleID,
            TabId: mydnnSupportLiveChat.TabID,
        }
    };

    $scope.localizeString = mydnnSupportLiveChat.SharedResources;
    $scope.livechat = {};
    $scope.widgetSettings = {};
    $scope.minButtonSettings = {};
    $scope.isVisibleWidget = true;
    $scope.isAgentOnline = true;

    $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/GetWidgetSettings", { headers: $self.Headers }).success(function (data) {
        $scope.livechat.WidgetHtml = data.LiveChatWidget;
        $scope.livechat.HtmlTemplate = data.LiveChatWidget;

        moduleSettings = data.Settings;

        setDefaultValue();

        $('body').append('<link href="' + siteRoot + 'DesktopModules/MVC/MyDnnSupport/LiveChat/Templates/' + data.Template + '/style.css" type="text/css" rel="stylesheet"/>');
        $('body').append('<style id="mydnnLiveChatWidgetStyles" type="text/css">.livechat-theme-bgcolor{background-color:' + moduleSettings.LiveChatThemeColor + ';}.livechat-theme-color{color:' + moduleSettings.LiveChatThemeColor + ';}.livechat-theme-titlecolor{color:' + moduleSettings.LiveChatTitleColor + ';}.livechat-focusable:focus{border-color:' + moduleSettings.LiveChatThemeColor + ' !important;}</style>');
        $('body').append('<style id="mydnnLiveChatMinButtonStyles" type="text/css"></style>');
        $('[data-toggle="popover"]').popover();
    });

    $scope.onStepTabClick = function (tabName) {
        if (tabName == 'appearance') {
            $scope.isVisibleWidget = true;
            $scope.livechat.Step = 'chatstarted';
        }
        else if (tabName == 'forms') {
            $scope.isVisibleWidget = true;
            $scope.livechat.Step = 'prechat';
        }
        else if (tabName == 'minbutton') {
            $scope.isVisibleWidget = false;
        }
    };

    $scope.onFormElementFocus = function (visibility) {
        if (visibility == 'online')
            $scope.isAgentOnline = true;
        else if (visibility == 'offline')
            $scope.isAgentOnline = false;
    };

    function setDefaultValue() {
        $scope.livechat.Step = 'chatstarted',
        $scope.livechat.Visitor =
        {
            UserID: -1,
            Avatar: moduleSettings.VisitorDefaultAvatar ? moduleSettings.VisitorDefaultAvatar : siteRoot + 'DesktopModules/MVC/MyDnnSupport/LiveChat/Styles/Images/visitor-avatar.png'
        },
        $scope.livechat.AgentDefaultAvatar = moduleSettings.AgentDefaultAvatar ? moduleSettings.AgentDefaultAvatar : siteRoot + 'DesktopModules/MVC/MyDnnSupport/LiveChat/Styles/Images/agent-avatar.png'
        $scope.livechat.Messages = [
        {
            SentBy: 0,
            Message: 'Chat started'
        },
        {
            SentBy: 1,
            SenderDisplayName: 'Paola',
            SenderAvatar: moduleSettings.VisitorDefaultAvatar,
            MessageType: 0,
            Message: 'Hi there, I`m looking for green and pink shirts.',
            IsMyMessage: true,
            MsgTime: moment().format("hh:mm"),
            Seen: true
        },
        {
            SentBy: 0,
            Message: 'Ian Borland joined the chat'
        },
        {
            SentBy: 2,
            SenderDisplayName: 'Ian Borland ',
            SenderAvatar: moduleSettings.VisitorDefaultAvatar,
            MessageType: 0,
            Message: 'Hi Paola, welcome to our store!',
            IsMyMessage: false,
            MsgTime: moment().format("hh:mm")
        },
        {
            SentBy: 1,
            SenderDisplayName: 'Paola',
            SenderAvatar: moduleSettings.VisitorDefaultAvatar,
            MessageType: 0,
            Message: 'I have one problem on my portal :(. I can`t login into website',
            IsMyMessage: true,
            MsgTime: moment().format("hh:mm"),
            Seen: true
        },
        {
            SentBy: 2,
            SenderDisplayName: 'Ian Borland ',
            SenderAvatar: moduleSettings.VisitorDefaultAvatar,
            MessageType: 0,
            Message: 'Please send your user info to support email (info@email.com)!',
            IsMyMessage: false,
            MsgTime: moment().format("hh:mm")
        },
        {
            SentBy: 0,
            Message: 'Ian Borland has left chat'
        },
        {
            SentBy: 1,
            SenderDisplayName: 'Paola',
            SenderAvatar: moduleSettings.VisitorDefaultAvatar,
            MessageType: 0,
            Message: 'eeee koja rafti baba jan man karet dashtama zood bargard lanati lanati lanati',
            IsMyMessage: true,
            MsgTime: moment().format("hh:mm"),
            Seen: false
        }],
        $scope.widgetSettings = {
            LiveChatThemeColor: moduleSettings.LiveChatThemeColor ? moduleSettings.LiveChatThemeColor : '#1D91E5',
            LiveChatTitleColor: moduleSettings.LiveChatTitleColor ? moduleSettings.LiveChatTitleColor : '#fff',
            LiveChatWindowSize: moduleSettings.LiveChatWindowSize ? moduleSettings.LiveChatWindowSize : 'Medium',
            LiveChatWidgetPosition: moduleSettings.LiveChatWidgetPosition ? moduleSettings.LiveChatWidgetPosition : 'BottomRight',
            LiveChatEnableRating: moduleSettings.LiveChatEnableRating ? JSON.parse(moduleSettings.LiveChatEnableRating.toLowerCase()) : true,
            LiveChatShowAvatar: moduleSettings.LiveChatShowAvatar ? JSON.parse(moduleSettings.LiveChatShowAvatar.toLowerCase()) : true,
            LiveChatMessageStyle: moduleSettings.LiveChatMessageStyle ? moduleSettings.LiveChatMessageStyle : 'SpeechBubbles',
            VisitorDefaultAvatar: moduleSettings.VisitorDefaultAvatar ? moduleSettings.VisitorDefaultAvatar : siteRoot +
                'DesktopModules/MVC/MyDnnSupport/LiveChat/Styles/images/visitor-avatar.png',
            AgentDefaultAvatar: moduleSettings.AgentDefaultAvatar ? moduleSettings.AgentDefaultAvatar : siteRoot +
                'DesktopModules/MVC/MyDnnSupport/LiveChat/Styles/images/agent-avatar.png'
        },
        $scope.minButtonSettings = {
            OnlineButton: moduleSettings.LiveChatMinBtnOnline ? moduleSettings.LiveChatMinBtnOnline : '<div class="m-left">Chat with us</div>' + '\n' +
                '<div class="m-right">' + '\n' +
                '<img src="/DesktopModules/MVC/MyDnnSupport/LiveChat/Styles/images/logo3.png" />' + '\n' +
                '</div>' + '\n' +
                '<div class="clear"></div>' + '\n' +
                '<div class="btm">Click here to start chat</div>',
            OfflineButton: moduleSettings.LiveChatMinBtnOffline ? moduleSettings.LiveChatMinBtnOffline : '<div class="m-left"></div>' + '\n' +
                '<div class="m-right">Leave a message</div>',
            OnlineButtonBGColor: moduleSettings.LiveChatMinBtnOnlineBgColor ? moduleSettings.LiveChatMinBtnOnlineBgColor : '#4285f4',
            OnlineButtonColor: moduleSettings.LiveChatMinBtnOnlineColor ? moduleSettings.LiveChatMinBtnOnlineColor : '#fff',
            OfflineButtonBGColor: moduleSettings.LiveChatMinBtnOfflineBgColor ? moduleSettings.LiveChatMinBtnOfflineBgColor : '#4487ff',
            OfflineButtonColor: moduleSettings.LiveChatMinBtnOfflineColor ? moduleSettings.LiveChatMinBtnOfflineColor : '#fff',
            HorizontalPosition: moduleSettings.LiveChatMinBtnHPos ? moduleSettings.LiveChatMinBtnHPos : 'Right',
            VerticalPosition: moduleSettings.LiveChatMinBtnVPos ? moduleSettings.LiveChatMinBtnVPos : 'Bottom',
            Rotate: moduleSettings.LiveChatMinBtnRotate ? moduleSettings.LiveChatMinBtnRotate : '0',
            CssStyle: moduleSettings.LiveChatMinBtnCssStyle ? moduleSettings.LiveChatMinBtnCssStyle : '.livechat-minbutton {cursor:pointer;padding: 0;margin-right:7px;}' + '\n' +
                '.livechat-minbutton .online-button {border-radius: 5px 5px 0 0;box-shadow: 0 0 3px 2px rgba(0, 0, 0, 0.1);height: 150px;padding: 0;position: relative;width: 240px;}' + '\n' +
                '.livechat-minbutton .online-button .m-left {float: left;font-family: verdana;font-size: 1.5em;margin: 45px 0 0 10px;}' + '\n' +
                '.livechat-minbutton .online-button .m-right {float: right;margin: 35px 10px 0 0;}' +
                '.livechat-minbutton .online-button .btm {background: #fff url("/DesktopModules/MVC/MyDnnSupport/LiveChat/Styles/images/send.png") no-repeat scroll 98% center;bottom: 0;box-sizing: border-box;color: #aaa;font-family: verdana;font-size: 12px;font-style: italic;height: 35px;left: 0;padding: 6px;position: absolute;width: 100%;}' + '\n' +
                '.livechat-minbutton .offline-button {padding: 10px;}' + '\n' +
                '.offline-button {border-radius: 5px 5px 0 0;height: 35px;padding: 0 !important;}' + '\n' +
                '.offline-button .m-left {background: rgba(0, 0, 0, 0.14) url("/DesktopModules/MVC/MyDnnSupport/LiveChat/Styles/images/email.png") no-repeat scroll 10px center;border-radius: 5px 0 0;float: left;height: 100%;width: 35px;}' + '\n' +
                '.offline-button .m-right {float: right;font-size:13px;font-weight: bold;line-height:18px;padding: 8px 10px;}',
        }
    }

    $scope.$watch("widgetSettings.LiveChatThemeColor", function (val, oldVal) {
        if (timeOut) $timeout.cancel(timeOut);

        timeOut = $timeout(function () {
            reWriteWidgetColorsStyles();
        }, 250);
    });

    $scope.$watch("widgetSettings.LiveChatTitleColor", function (val, oldVal) {
        if (timeOut) $timeout.cancel(timeOut);

        timeOut = $timeout(function () {
            reWriteWidgetColorsStyles();
        }, 250);
    });

    function reWriteWidgetColorsStyles() {
        angular.element('#mydnnLiveChatWidgetStyles').html('.livechat-theme-bgcolor{background-color:' + $scope.widgetSettings.LiveChatThemeColor + ';}.livechat-theme-color{color:' + $scope.widgetSettings.LiveChatThemeColor + ';}.livechat-theme-titlecolor{color:' + $scope.widgetSettings.LiveChatTitleColor + ';}.livechat-focusable:focus{border-color:' + $scope.widgetSettings.LiveChatThemeColor + ' !important;}');
    }

    $scope.$watch("minButtonSettings.CssStyle", function (val, oldVal) {
        if (timeOut) $timeout.cancel(timeOut);

        timeOut = $timeout(function () {
            angular.element('#mydnnLiveChatMinButtonStyles').html(val);
        }, 250);
    });

    $scope.onUpdateSettingsClick = function () {
        $scope.minButtonSettings.HtmlTemplate = '<style type="text/css">' + angular.element('#mydnnLiveChatMinButtonStyles').html() + '</style>';
        $scope.minButtonSettings.HtmlTemplate += angular.element("#livechat-minbutton-panel").html();
        var data = {
            WidgetSettings: $scope.widgetSettings,
            MinButtonSettings: $scope.minButtonSettings,
            Locales: $scope.localizeString
        };
        $http.post(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/UpdateWidgetSettings", data, { headers: $self.Headers }).success(function (data) {
            location.reload();
        }).error(function (data, status, headers, config) {
            swal($scope.localizeString["Error.Text"], data, "error");
        });
    };

    $scope.onDropFiles = function ($files, x) {
        switch (x) {
            case 1:
                $scope.widgetSettings.VisitorDefaultAvatar = $files[0];
                break;
            case 2:
                $scope.widgetSettings.AgentDefaultAvatar = $files[0];
                break;
        }
        $scope.onUploadPhoto($files, x);
    };

    $scope.onUploadPhoto = function ($files, x) {
        var file = $files[0];
        Upload.upload({
            url: mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/UploadFile",
            file: file,
            headers: {
                ModuleId: mydnnSupportLiveChat.ModuleID,
                TabId: mydnnSupportLiveChat.TabID,
                'Content-Type': file.type
            }
        }).success(function (data) {
            $scope.isUpdateDisabled = false;
            switch (x) {
                case 1:
                    $scope.widgetSettings.VisitorDefaultAvatar = data;
                    $scope.livechat.Visitor.Avatar = data;
                    for (var i = 0; i < $scope.livechat.Messages.length; i++) {
                        var message = $scope.livechat.Messages[i];
                        if (message.SentBy == 1)
                            message.SenderAvatar = data;
                    }
                    break;
                case 2:
                    $scope.widgetSettings.AgentDefaultAvatar = data;
                    $scope.livechat.AgentDefaultAvatar = data;
                    for (var i = 0; i < $scope.livechat.Messages.length; i++) {
                        var message = $scope.livechat.Messages[i];
                        if (message.SentBy == 2)
                            message.SenderAvatar = data;
                    }
                    break;
            }
        }).progress(function (evt) {
            console.log('percent: ' + parseInt(100.0 * evt.loaded / evt.total));
        });
    }
});

