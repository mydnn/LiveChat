app.controller("widgetSettingController", function ($scope, $http, $timeout, $filter, $location, cfpLoadingBar, activeMenu) {
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
        $('body').append('<style id="mydnnLiveChatWidgetStyles" type="text/css">.livechat-theme-bgcolor{background-color:' + data.Settings.LiveChatThemeColor + ';}.livechat-theme-color{color:' + data.Settings.LiveChatThemeColor + ';}.livechat-theme-titlecolor{color:' + data.Settings.LiveChatTitleColor + ';}.livechat-focusable:focus{border-color:' + data.Settings.LiveChatThemeColor + ' !important;}</style>');
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
        },
        $scope.minButtonSettings = {
            OnlineButton: moduleSettings.LiveChatMinBtnOnline ? moduleSettings.LiveChatMinBtnOnline : 'Live Chat: Online',
            OnlineButtonBGColor: moduleSettings.LiveChatMinBtnOnlineBgColor ? moduleSettings.LiveChatMinBtnOnlineBgColor : '#4caf50',
            OnlineButtonColor: moduleSettings.LiveChatMinBtnOnlineColor ? moduleSettings.LiveChatMinBtnOnlineColor : '#fff',
            OfflineButton: moduleSettings.LiveChatMinBtnOffline ? moduleSettings.LiveChatMinBtnOffline : 'Send Message',
            OfflineButtonBGColor: moduleSettings.LiveChatMinBtnOfflineBgColor ? moduleSettings.LiveChatMinBtnOfflineBgColor : '#c7cec7',
            OfflineButtonColor: moduleSettings.LiveChatMinBtnOfflineColor ? moduleSettings.LiveChatMinBtnOfflineColor : '#333',
            HorizontalPosition: moduleSettings.LiveChatMinBtnHPos ? moduleSettings.LiveChatMinBtnHPos : 'Right',
            VerticalPosition: moduleSettings.LiveChatMinBtnVPos ? moduleSettings.LiveChatMinBtnVPos : 'Bottom',
            Rotate: moduleSettings.LiveChatMinBtnRotate ? moduleSettings.LiveChatMinBtnRotate : '0',
            CssStyle: moduleSettings.LiveChatMinBtnCssStyle ? moduleSettings.LiveChatMinBtnCssStyle : '.livechat-minbutton{\n} \n.livechat-minbutton .online-button {\npadding: 10px;} \n.livechat-minbutton .offline-button {\npadding: 10px;}',
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
            $('#mydnnLiveChatMinButtonStyles').html(val);
        }, 250);
    });

    $scope.onUpdateSettingsClick = function () {
        $scope.minButtonSettings.HtmlTemplate = angular.element("#livechat-minbutton-panel").html();
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
});

