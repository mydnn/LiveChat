app.controller("agentController", function ($scope, $http, $timeout, $filter, cfpLoadingBar, activeMenu) {
    activeMenu.setActiveMenu({ Parent: 'manage', Child: 'agents' });

    var timeOut = false;
    var $self = {
        Headers: {
            ModuleId: mydnnSupportLiveChat.ModuleID,
            TabId: mydnnSupportLiveChat.TabID,
        }
    };

    $scope.localizeString = mydnnSupportLiveChat.SharedResources;
    $scope.agent = {};
    $scope.checkValidate = false;
    $scope.winTitle = "";
    $scope.isSearchReady = false;
    $scope.isAgentEdit = false;

    $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/GetAgents", { headers: $self.Headers }).success(function (data) {
        $scope.agents = data;
        $timeout(function () { $scope.isSearchReady = true; }, 1000);
    }).error(function (data, status, headers, config) {
        angular.element('#tblagents > tbody').html('<tr><td colspan="7"><div role="alert" class="alert alert-danger">{0}</div></td></tr>'.replace("{0}", data));
    });

    $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/GetNamesOfDepartments", { headers: $self.Headers }).success(function (data) {
        $scope.departments = data;
    });

    //page events
    $scope.onEditAgentClick = function (agent) {
        if (!agent) {
            $scope.agent = {
                Enabled: true
            };
            $scope.winTitle = $scope.localizeString["AddNewAgent.Text"];
            $scope.isAgentEdit = false;
        }
        else {
            $scope.agent = angular.copy(agent);
            $scope.winTitle = $scope.localizeString["Edit.Text"] + " " + $scope.agent.DisplayName;
            $scope.isAgentEdit = true;
        }

        $scope.checkValidate = false;
        $('#wnEditAgent').modal('show');

        setTimeout(function () {
            $scope.$broadcast('newItemAdded');
        }, 500);
    };

    $scope.onDeleteAgentClick = function (agentID, index) {
        swal({
            title: $scope.localizeString["AreYouSure.Text"],
            text: $scope.localizeString["DeleteAgentConfirm.Text"],
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: $scope.localizeString["Yes.Text"],
            cancelButtonText: $scope.localizeString["No.Text"],
            closeOnConfirm: false,
            animation: false
        }, function () {
            var data = {
                ID: agentID
            };
            $http.post(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/DeleteAgent", data, { headers: $self.Headers }).success(function (data) {
                $scope.agents.splice(index, 1);
                $scope.agent = {};
                swal({
                    title: $scope.localizeString["Done.Text"],
                    text: $scope.localizeString["DeleteAgentMsg.Text"],
                    type: "success",
                    timer: 3000
                });
            }).error(function (data, status, headers, config) {
                swal({
                    title: $scope.localizeString["Error.Text"],
                    text: data,
                    type: "error",
                });
                swal($scope.localizeString["Error.Text"], data, "error");
            });
        });
    };

    $scope.onUpdateAgentClick = function () {
        $scope.checkValidate = true;
        if ($scope.agentForm.$valid && $scope.agent.UserID) {
            if ($scope.agent.AgentID)
                $http.post(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/UpdateAgent", $scope.agent, { headers: $self.Headers }).success(function (data) {
                    swal({
                        title: $scope.localizeString["Done.Text"],
                        text: $scope.localizeString["UpdateAgentMsg.Text"],
                        type: "success",
                        showCancelButton: true,
                        confirmButtonColor: "#4E9A36",
                        confirmButtonText: $scope.localizeString["RefreshPage.Text"],
                        cancelButtonText: $scope.localizeString["No.Text"],
                        closeOnConfirm: false,
                    }, function () {
                        location.reload();
                    });

                    $('#wnEditAgent').modal('hide');

                    var agent = $filter('filter')($scope.agents, function (d) { return d.AgentID === $scope.agent.AgentID; })[0];
                    var index = $scope.agents.indexOf(agent);
                    $scope.agents[index] = $scope.agent;

                    $scope.agent = {};
                }).error(function (data, status, headers, config) {
                    swal({
                        title: $scope.localizeString["Error.Text"],
                        text: data,
                        type: "error",
                    });
                });
            else
                $http.post(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/AddAgent", $scope.agent, { headers: $self.Headers }).success(function (data) {
                    swal({
                        title: $scope.localizeString["Done.Text"],
                        text: $scope.localizeString["AddAgentMsg.Text"],
                        type: "success",
                        showCancelButton: true,
                        confirmButtonColor: "#4E9A36",
                        confirmButtonText: $scope.localizeString["RefreshPage.Text"],
                        cancelButtonText: $scope.localizeString["No.Text"],
                        closeOnConfirm: false,
                    }, function () {
                        location.reload();
                    });

                    $('#wnEditAgent').modal('hide');

                    $scope.agent = data.Agent;
                    $scope.agents.push($scope.agent);

                    $scope.agent = {};
                }).error(function (data, status, headers, config) {
                    swal({
                        title: $scope.localizeString["Error.Text"],
                        text: data,
                        type: "error",
                    });
                });

            $scope.departmentsFilter = undefined;
        }
    };

    // autocomplete for displayname 
    $scope.getUserByDisplayName = function (val) {
        return $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/GetUserByDisplayName", {
            headers: $self.Headers,
            params: {
                name: val
            }
        }).then(function (response) {
            return response.data;
        });

        $scope.agent.UserID = 0;
    };

    $scope.setUserID = function (agent) {
        $scope.agent.UserID = agent.UserID;
    }

    //Search functions
    $scope.filterModel = '';
    $scope.$watch('filterModel', function () {
        if (timeOut) $timeout.cancel(timeOut)

        timeOut = $timeout(function () {
            if ($scope.isSearchReady)
                $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/SearchAgents", { params: { searchType: 1, filter: $scope.filterModel }, headers: $self.Headers }).success(function (data) {
                    $scope.agents = data;
                });
        }, 500)
    });

    $scope.onClearFilterClick = function () {
        $scope.filterModel = "";
    };

    $scope.$watch('departmentsFilter', function () {
        if ($scope.departmentsFilter) {
            $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/SearchAgents", { params: { searchType: 2, filter: $scope.departmentsFilter.join(",") }, headers: $self.Headers }).success(function (data) {
                $scope.agents = data;
            });
        }
    });
});

