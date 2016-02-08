app.controller("settingController", function ($scope, $http, $timeout, $filter, $location, cfpLoadingBar, activeMenu) {
    activeMenu.setActiveMenu({ Parent: 'settings', Child: 'basic' });

    var siteRoot = mydnnSupportLiveChat.SiteRoot;
    var $self = {
        Headers: {
            ModuleId: mydnnSupportLiveChat.ModuleID,
            TabId: mydnnSupportLiveChat.TabID,
        }
    };

    $scope.localizeString = mydnnSupportLiveChat.SharedResources;
    $scope.portalSettings = {};
    $scope.moduleSettings = {};

    $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/GetBasicSettings", { headers: $self.Headers }).success(function (data) {
        $scope.portalSettings = data.PortalSettings;
        $scope.moduleSettings = data.ModuleSettings;

        $scope.portalSettings.VisitorsOnlineEnabled = JSON.parse($scope.portalSettings.VisitorsOnlineEnabled.toLowerCase());
        $scope.portalSettings.LiveChatEnabled = JSON.parse($scope.portalSettings.LiveChatEnabled.toLowerCase());

        $scope.moduleSettings.PlaySoundWhenNewMsg = $scope.moduleSettings.PlaySoundWhenNewMsg ? JSON.parse($scope.moduleSettings.PlaySoundWhenNewMsg.toLowerCase()) : false;
        $scope.moduleSettings.ShowDekstopNotificationForIncoming = $scope.moduleSettings.ShowDekstopNotificationForIncoming ? JSON.parse($scope.moduleSettings.ShowDekstopNotificationForIncoming.toLowerCase()) : false;
        $scope.moduleSettings.ShowDekstopNotificationForNewMsg = $scope.moduleSettings.ShowDekstopNotificationForNewMsg ? JSON.parse($scope.moduleSettings.ShowDekstopNotificationForNewMsg.toLowerCase()) : false;
        $scope.moduleSettings.AgentsViewPermission = $scope.moduleSettings.AgentsViewPermission ? $scope.moduleSettings.AgentsViewPermission : 'OnlyCurrentAgents',
        $scope.moduleSettings.SendEmailForOffline = $scope.moduleSettings.SendEmailForOffline ? JSON.parse($scope.moduleSettings.SendEmailForOffline.toLowerCase()) : false;
        $scope.moduleSettings.SendEmailAfterChat = $scope.moduleSettings.SendEmailAfterChat ? JSON.parse($scope.moduleSettings.SendEmailAfterChat.toLowerCase()) : false;
        $scope.moduleSettings.TranscriptEmailTemplate = $scope.moduleSettings.TranscriptEmailTemplate ? $scope.moduleSettings.TranscriptEmailTemplate : '<div><h1>Chat Transcript with  {{livechat.Visitor.DisplayName}}</h1><br /><ul><li ng-repeat="message in livechat.Messages"><span>{{message.SenderDisplayName}}: </span><span>{{message.Message}}</span></li></ul><br /><hr /><table><tr><td>{{localizeString["Name.Text"]}}</td><td>{{livechat.Visitor.DisplayName}}</td></tr><tr><td>{{localizeString["Email.Text"]}}</td><td>{{livechat.Visitor.Email}}</td></tr></table></div>';
        $scope.moduleSettings.OfflineEmailTemplate = $scope.moduleSettings.OfflineEmailTemplate ? $scope.moduleSettings.OfflineEmailTemplate : '<div><h1>{{localizeString["OfflineMessageFrom.Text"]}}  {{livechat.Visitor.DisplayName}}</h1><br /><br>{{livechat.Message}}<hr /><br><table><tr><td>{{localizeString["Name.Text"]}}</td><td>{{livechat.Visitor.DisplayName}}</td></tr><tr><td>{{localizeString["Email.Text"]}}</td><td>{{livechat.Visitor.Email}}</td></tr></table></div>';
    });

    $scope.onUpdateSettingsClick = function () {
        var data = {
            PortalSettings: $scope.portalSettings,
            ModuleSettings: $scope.moduleSettings,
        };
        $http.post(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/UpdateBasicSettings", data, { headers: $self.Headers }).success(function (data) {
            location.reload();
        }).error(function (data, status, headers, config) {
            swal($scope.localizeString["Error.Text"], data, "error");
        });
    };
});

