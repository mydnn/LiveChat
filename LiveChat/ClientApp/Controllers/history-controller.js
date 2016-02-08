app.controller("historyController", function ($scope, $http, $timeout, $filter, $location, cfpLoadingBar, activeMenu) {
    activeMenu.setActiveMenu({ Parent: 'history' });

    var filterTimeOut;
    var $self = {
        Headers: {
            ModuleId: mydnnSupportLiveChat.ModuleID,
            TabId: mydnnSupportLiveChat.TabID,
        }
    };

    $scope.localizeString = mydnnSupportLiveChat.SharedResources;
    $scope.history = {
        LiveChats: [],
        Paging: {
            PageIndex: 0,
            PageSize: 10,
            TotalCount: 0
        },
    };
    $scope.filters = {
        Type: 0,
        Departments: '',
        Agents: '',
        VisitorEmail: '',
        FromDate: '',
        ToDate: '',
        GoodRate: false,
        BadRate: false
    };
    $scope.selectedItems = [];
    $scope.selectedLiveChat = {};
    $scope.transcriptTab = true;
    $scope.filterByRating = false;

    $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/GetDepartments", { headers: $self.Headers }).success(function (data) {
        $scope.departments = data;
    });

    $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/GetAgents", { headers: $self.Headers }).success(function (data) {
        $scope.agents = data;
    });

    //page events
    $scope.onTypeTabClick = function (type) {
        $scope.filters.Type = type;
    };

    $scope.onSearchToggleClick = function () {
        var $panel = angular.element(".advanced-search");
        if (!$panel.is(':visible')) {
            $panel.show();
            $panel.removeClass("animated fadeOutUp");
            $panel.addClass("animated fadeInDown");
            angular.element(".history i.md-expand-more").addClass("hidden");
            angular.element(".history i.md-expand-less").removeClass("hidden");
        }
        else {
            $panel.removeClass("animated fadeInDown");
            $panel.addClass("animated fadeOutUp");
            angular.element(".history i.md-expand-less").addClass("hidden");
            angular.element(".history i.md-expand-more").removeClass("hidden");
            $timeout(function () {
                $panel.hide();
            }, 500);
        }
    };

    $scope.onSelectLiveChatChange = function (isAll) {
        if (isAll == "true") {
            if ($scope.history.AllSelected)
                angular.forEach($scope.history.LiveChats, function (object) {
                    object.Selected = true;
                });
            else
                angular.forEach($scope.history.LiveChats, function (object) {
                    object.Selected = false;
                });
        }

        $scope.selectedItems = $filter("filter")($scope.history.LiveChats, { Selected: true });
    };

    $scope.onChangePageSizeClick = function (count) {
        $scope.history.Paging.PageSize = count;
        runQuery();
    };

    $scope.onRowLiveChatClick = function (liveChatID) {
        if ($scope.selectedLiveChat.LiveChatID != liveChatID) {
            $scope.selectedLiveChat.LiveChatID = liveChatID;
            $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/GetMessages", { headers: $self.Headers, params: { livechatID: liveChatID } }).success(function (data) {
                var result = $filter("filter")($scope.history.LiveChats, { LiveChatID: liveChatID });
                if (result.length)
                    $scope.selectedLiveChat = angular.copy(result[0]);

                $scope.selectedLiveChat.Messages = data.Messages;
                parseLiveChatMessages($scope.selectedLiveChat, data.Messages);
            });
        }
    };

    $scope.onActionChange = function () {
        if ($scope.action == 'delete') {
            swal({
                title: $scope.localizeString["PurgeHistory.Text"],
                text: $scope.localizeString["PurgeHistory.Help"],
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: $scope.localizeString["Delete.Text"],
                cancelButtonText: $scope.localizeString["Cancel.Text"],
                closeOnConfirm: true,
            }, function () {
                var items = [];
                angular.forEach($filter("filter")($scope.history.LiveChats, { Selected: true }), function (object) {
                    items.push(object.LiveChatID);
                });

                var data = {
                    Items: items
                }

                $http.post(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/PurgeHistory", data, { headers: $self.Headers }).success(function () {
                    angular.forEach(data.Items, function (livechatID) {
                        var result = $filter("filter")($scope.history.LiveChats, { LiveChatID: livechatID });
                        if (result.length) {
                            var livechat = result[0];
                            var indx = $scope.history.LiveChats.indexOf(livechat);
                            $scope.history.LiveChats.splice(indx, 1);
                        }
                    });
                    swal($scope.localizeString["Done.Text"], $scope.localizeString["DeleteAgentMsg.Text"], "success");
                    $scope.action = "";

                    $scope.history.AllSelected = false;
                    runQuery();
                }).error(function (data, status, headers, config) {
                    swal($scope.localizeString["Error.Text"], data, "error");
                });
            });
        }
    };

    $scope.onTranscriptTabClick = function (type) {
        if (type == "0")
            $scope.transcriptTab = true;
        if (type == "1")
            $scope.transcriptTab = false;
    };

    for (var key in $scope.filters) {
        if ($scope.filters.hasOwnProperty(key)) {
            $scope.$watch("filters." + key, function (val, oldVal) {
                if (val != oldVal)
                    runQuery();
            });
        }
    }

    $scope.onPageClick = function ($index, state) {
        if ($scope.history.Paging.PageIndex > 0 && state == 'prev')
            $scope.history.Paging.PageIndex--;
        else if ($scope.history.Paging.PageIndex < $scope.history.Paging.PageCount && state == 'next')
            $scope.history.Paging.PageIndex++;
        else
            $scope.history.Paging.PageIndex = $index;

        runQuery();
    };

    $scope.onCloseTranscriptClick = function () {
        $scope.selectedLiveChat = {};
    };

    $scope.$watch("filterByRating", function (val, oldVal) {
        if (!val) {
            $scope.filters.GoodRate = false;
            $scope.filters.BadRate = false;
        }
    });

    function runQuery() {
        if (filterTimeOut) $timeout.cancel(filterTimeOut);

        filterTimeOut = $timeout(function () {
            var f = $scope.filters;

            var departments = '';
            if (f.Departments)
                departments = createStringByArray(f.Departments, "DepartmentID");

            var agents = '';
            if (f.Agents)
                agents = createStringByArray(f.Agents, "UserID");

            var rate = 0;
            if ($scope.filterByRating && $scope.filters.GoodRate)
                rate++;
            if ($scope.filterByRating && $scope.filters.BadRate)
                rate += 2;

            $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/GetChatHistory", {
                headers: $self.Headers,
                params: {
                    type: f.Type,
                    departments: departments,
                    agents: agents,
                    visitorEmail: (f.VisitorEmail ? f.VisitorEmail : ''),
                    fromDate: (f.FromDate ? f.FromDate : ''),
                    toDate: (f.ToDate ? f.ToDate : ''),
                    rating: rate,
                    unread: false,
                    pageIndex: $scope.history.Paging.PageIndex,
                    pageSize: $scope.history.Paging.PageSize,
                    totalCount: $scope.history.Paging.TotalCount
                }
            }).success(function (data) {
                $scope.selectedLiveChat = {};

                $scope.history.LiveChats = data.LiveChats;
                $scope.history.Paging = data.Paging;

                for (var i = 0; i < $scope.history.LiveChats.length; i++) {
                    parseVisitorItems($scope.history.LiveChats[i]);
                }
            });
        }, 250); // delay 250 ms
    }

    function createStringByArray(array, key) {
        var output = '';
        angular.forEach(array, function (object) {
            output += object[key] + ',';
        });
        return output;
    }

    runQuery();

    function parseLiveChatMessages(livechat, messages) {
        for (var i = 0; i < messages.length; i++) {
            parseLiveChatMessage(livechat, messages[i]);
        }
    }

    function parseLiveChatMessage(livechat, message) {
        if (message.SentBy == 1) { //if visitor is sender message 
            message.SenderDisplayName = livechat.Visitor.DisplayName;
        }
        else if (message.SentBy == 2) { //if agent is sender message 
            var agent = $filter("filter")(livechat.Agents, { UserID: message.AgentUserID })[0];
            message.SenderDisplayName = agent.DisplayName;
        }
        message.Time = moment(message.CreateDate).format('h:mm a');
    }

    function parseVisitorItems(livechat) {
        parseIpLocation(livechat.LiveChatID, livechat.Visitor.IP);

        var parser = new UAParser();
        parser.setUA(livechat.Visitor.UserAgent);
        livechat.Visitor.UserAgentData = parser.getResult();
    }

    function parseIpLocation(id, ip) {
        if (ip == "127.0.0.1") ip = "";
        $.getJSON("http://freegeoip.net/json/" + ip, function (data) {
            var result = $filter("filter")($scope.history.LiveChats, { LiveChatID: id });
            if (result.length) {
                var indx = $scope.history.LiveChats.indexOf(result[0]);
                $scope.history.LiveChats[indx].Visitor.Location = data;
                if ($scope.selectedLiveChat.Visitor) $scope.selectedLiveChat.Visitor.Location = data;
            }
        });
    }
});

