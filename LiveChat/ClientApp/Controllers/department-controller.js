app.controller("departmentController", function ($scope, $location, $http, $timeout, $filter, cfpLoadingBar, activeMenu) {
    activeMenu.setActiveMenu({ Parent: 'manage', Child: 'departments' });

    var $self = {
        Headers: {
            ModuleId: mydnnSupportLiveChat.ModuleID,
            TabId: mydnnSupportLiveChat.TabID,
        }
    };

    $scope.localizeString = mydnnSupportLiveChat.SharedResources;
    $scope.department = {};
    $scope.checkValidate = false;
    $scope.winTitle = "";
    $scope.isSearchReady = false;

    $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/GetDepartments", { headers: $self.Headers }).success(function (data) {
        $scope.departments = data;
        $timeout(function () { $scope.isSearchReady = true; }, 1000);
    }).error(function (data, status, headers, config) {
        angular.element('#tblDepartments > tbody').html('<tr><td colspan="7"><div role="alert" class="alert alert-danger">{0}</div></td></tr>'.replace("{0}", data));
    });

    //page events
    $scope.onEditDepartmentClick = function (department) {
        if (!department) {
            $scope.department = {};
            $scope.winTitle = $scope.localizeString["AddNewDepartment.Text"];
        }
        else {
            $scope.department = angular.copy(department);
            $scope.winTitle = $scope.localizeString["Edit.Text"] + " " + $scope.department.DisplayName;
        }

        $scope.checkValidate = false;
        $('#wnEditDepartment').modal('show');

        setTimeout(function () {
            $scope.$broadcast('newItemAdded');
        }, 500);
    };

    $scope.onDeleteDepartmentClick = function (departmentID, index) {
        swal({
            title: $scope.localizeString["AreYouSure.Text"],
            text: $scope.localizeString["DeleteDepartmentConfirm.Text"],
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: $scope.localizeString["Yes.Text"],
            cancelButtonText: $scope.localizeString["No.Text"],
            closeOnConfirm: false,
            animation: false
        }, function () {
            var data = {
                ID: departmentID
            };
            $http.post(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/DeleteDepartment", data, { headers: $self.Headers }).success(function (data) {
                $scope.departments.splice(index, 1);
                $scope.department = {};
                swal({
                    title: $scope.localizeString["Done.Text"],
                    text: $scope.localizeString["DeleteDepartmentMsg.Text"],
                    type: "success",
                    timer: 3000
                });
            }).error(function (data, status, headers, config) {
                swal({
                    title: $scope.localizeString["Error.Text"],
                    text: data,
                    type: "error",
                });
            });
        });
    };

    $scope.onUpdateDepartmentClick = function () {
        $scope.checkValidate = true;
        if ($scope.departmentForm.$valid) {
            if ($scope.department.DepartmentID) {
                $http.post(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/UpdateDepartment", $scope.department, { headers: $self.Headers }).success(function (data) {
                    swal({
                        title: $scope.localizeString["Done.Text"],
                        text: $scope.localizeString["UpdateDepartmentMsg.Text"],
                        type: "success",
                        timer: 3000
                    });

                    $('#wnEditDepartment').modal('hide');

                    var department = $filter('filter')($scope.departments, function (d) { return d.DepartmentID === $scope.department.DepartmentID; })[0];
                    var index = $scope.departments.indexOf(department);
                    $scope.departments[index] = $scope.department;

                    $scope.department = {};
                }).error(function (data, status, headers, config) {
                    swal({
                        title: $scope.localizeString["Error.Text"],
                        text: data,
                        type: "error",
                    });
                });
            }
            else {
                $http.post(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/AddDepartment", $scope.department, { headers: $self.Headers }).success(function (data) {
                    swal({
                        title: $scope.localizeString["Done.Text"],
                        text: $scope.localizeString["AddDepartmentMsg.Text"],
                        type: "success",
                        timer: 3000
                    });

                    $('#wnEditDepartment').modal('hide');

                    $scope.department.DepartmentID = data.DepartmentID;
                    $scope.departments.push($scope.department);

                    $scope.department = {};
                }).error(function (data, status, headers, config) {
                    swal({
                        title: $scope.localizeString["Error.Text"],
                        text: data,
                        type: "error",
                    });
                });
            }
        }
    };

    //Search functions
    var timer = false;
    $scope.filterModel = '';
    $scope.$watch('filterModel', function () {
        if (timer) {
            $timeout.cancel(timer)
        }
        timer = $timeout(function () {
            if ($scope.isSearchReady)
                $http.get(mydnnGetServiceRoot("MyDnnSupport.LiveChat") + "AgentService/SearchDepartments", { headers: $self.Headers, params: { filter: $scope.filterModel } }).success(function (data) {
                    $scope.departments = data;
                });
        }, 500)
    });
    $scope.onClearFilterClick = function () {
        $scope.filterModel = "";
    };
});

