app.controller("visitorListController", function ($scope, $location, $http, $interval, $timeout, $filter, $q, cfpLoadingBar, activeMenu) {
    activeMenu.setActiveMenu({ Parent: 'visitorlist' });

    var siteRoot = mydnnSupportLiveChat.SiteRoot;
    var $self = {
        Headers: {
            ModuleId: mydnnSupportLiveChat.ModuleID,
            TabId: mydnnSupportLiveChat.TabID,
        }
    };

    $scope.localizeString = mydnnSupportLiveChat.SharedResources;
    $scope.visitor = {};
    $scope.visitors = [];
    $scope.incomingLiveChats = [];

    $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/GetVisitorList", { headers: $self.Headers }).success(function (visitors) {
        $scope.visitors = visitors;
        $scope.incomingLiveChats = [];
        for (var i = 0; i < $scope.visitors.length; i++) {
            var visitor = $scope.visitors[i];
            parseVisitorItems(visitor);
            if (visitor.IncomingLiveChat)
                $scope.incomingLiveChats.push(visitor);
        }
        calcVisitorsOnlineTime();
    }).error(function (data, status, headers, config) { });

    //scope functions
    $scope.$on('$destroy', function () {
        $interval.cancel(timerCalculator);
        timerCalculator = undefined;
    });

    //Search functions
    $scope.sortModel = '1';
    $scope.$watch('sortModel', function () {
        sortVisitors();
    });

    //broadcasting from livechatAgentController
    $scope.$on("onPopulateVisitorsOnline", function (event, args) {
        var visitor = args.visitor;
        result = $filter("filter")($scope.visitors, { VisitorGUID: visitor.VisitorGUID });
        if (result.length == 0) {
            $scope.visitors.push(visitor);
            parseVisitorItems(visitor);
            sortVisitors();
        }
    });

    $scope.$on("onUpdateVisitorInfo", function (event, args) {
        var visitor = args.visitor;
        result = $filter("filter")($scope.visitors, { VisitorGUID: visitor.VisitorGUID });
        if (result.length) {
            var indx = $scope.visitors.indexOf(result[0]);
            if (visitor.UserName != '') {
                $scope.visitors[indx].UserID = visitor.UserID;
                $scope.visitors[indx].UserName = visitor.UserName;
                $scope.visitors[indx].Avatar = getVisitorAvatar(visitor.UserID);
            }
            if (visitor.DisplayName != '') {
                $scope.visitors[indx].DisplayName = visitor.DisplayName;
                $scope.visitors[indx].Email = visitor.Email;
            }
            $scope.visitors[indx].LastURL = visitor.LastURL;
        }
    });

    $scope.$on("onPopulateVisitorsOffline", function (event, args) {
        var visitors = args.visitors;
        for (var i = 0; i < visitors.length; i++) {
            removeVisitor(visitors[i]);
            removeIncomingChat(visitors[i]);
        }
    });

    $scope.$on("onIncomingLiveChat", function (event, args) {
        var livechatID = args.livechatID, visitorGUID = args.visitorGUID, message = args.message;

        result = $filter("filter")($scope.visitors, { VisitorGUID: visitorGUID });
        if (result.length) {
            result[0].IncomingLiveChat = true;
            if ($filter("filter")($scope.incomingLiveChats, { VisitorGUID: visitorGUID }).length == 0) {
                result[0].Message = message;
                $scope.incomingLiveChats.push(result[0]);
            }
        }
    });

    $scope.$on("onVisitorIsChatting", function (event, args) {
        var visitorGUID = args.visitorGUID;

        removeIncomingChat(visitorGUID);
        result = $filter("filter")($scope.visitors, { VisitorGUID: visitorGUID });
        if (result.length) {
            result[0].IncomingLiveChat = false;
            result[0].CurrentLiveChat = true;
        }
    });

    //private functions
    function parseVisitorItems(visitor) {
        visitor.DisplayName = getDisplayName(visitor);
        visitor.Avatar = getVisitorAvatar(visitor.UserID);
        visitor.OnlineTime = getTimeDiff(visitor.OnlineDate, moment());

        if (!visitor.Location)
            parseIpLocation(visitor.VisitorGUID, visitor.IP);

        if (!visitor.UserAgentData) {
            var parser = new UAParser();
            parser.setUA(visitor.UserAgent);
            visitor.UserAgentData = parser.getResult();
        }
    }

    function parseIpLocation(id, ip) {
        if (ip == "127.0.0.1")
            ip = "";
        $.getJSON("http://freegeoip.net/json/" + ip, function (data) {
            var visitor = $filter("filter")($scope.visitors, { VisitorGUID: id });
            var indx = $scope.visitors.indexOf(visitor[0]);
            $scope.visitors[indx].Location = data;
            $timeout(function () {
            }, 100);
        });
    }

    function getDisplayName(visitor) {
        if (visitor.DisplayName == '')
            return $scope.localizeString["Anonymous.Text"] + " #" + visitor.VisitorGUID.substr(0, 4);
        else
            return visitor.DisplayName;
    }

    function getVisitorAvatar(userID) {
        if (userID <= 0)
            return siteRoot + "images/no_avatar.gif";
        else
            return siteRoot + "dnnimagehandler.ashx?mode=profilepic&userid=" + userID;
    };

    function getTimeDiff(onlineT, nowT) {
        var diff = nowT.diff(onlineT, 'minutes');
        var lbl = $scope.localizeString["Min.Text"]
        if (diff >= 60) {
            diff = nowT.diff(onlineT, 'hours');
            lbl = $scope.localizeString["Hours.Text"]
        }
        return diff + ' ' + lbl;
    }

    function parseDate(d) {
        return new Date(d);
    }

    function parseCountry(c) {
        return c.country_code;
    }

    function parseBrowser(b) {
        return b.browser.name;
    }

    function sortList(field, reverse, primer) {
        var key = primer ?
            function (x) { return primer(x[field]) } :
            function (x) { return x[field] };

        reverse = [-1, 1][+!!reverse];

        return function (a, b) {
            return a = key(a), b = key(b), reverse * ((a > b) - (b > a));
        }
    }

    function sortVisitors() {
        if ($scope.sortModel == '1')
            $scope.visitors = $scope.visitors.sort(sortList('OnlineDate', true, parseDate))
        else if ($scope.sortModel == '2')
            $scope.visitors = $scope.visitors.sort(sortList('OnlineDate', false, parseDate))
        else if ($scope.sortModel == '3')
            $scope.visitors = $scope.visitors.sort(sortList('Location', false, parseCountry))
        else if ($scope.sortModel == '4')
            $scope.visitors = $scope.visitors.sort(sortList('UserAgentData', true, parseBrowser))
    };

    var timerCalculator;
    function calcVisitorsOnlineTime() {
        timerCalculator = $interval(function () {
            for (var i = 0; i < $scope.visitors.length; i++) {
                var visitor = $scope.visitors[i];
                visitor.OnlineTime = getTimeDiff(visitor.OnlineDate, moment());
            }
        }, 59000);
    }

    function removeVisitor(id) {
        result = $filter("filter")($scope.visitors, { VisitorGUID: id });
        if (result.length) {
            var indx = $scope.visitors.indexOf(result[0]);
            $scope.visitors.splice(indx, 1);
        }
    }

    function removeIncomingChat(id) {
        result = $filter("filter")($scope.incomingLiveChats, { VisitorGUID: id });
        if (result.length) {
            var indx = $scope.incomingLiveChats.indexOf(result[0]);
            $scope.incomingLiveChats.splice(indx, 1);
        }
    }
});