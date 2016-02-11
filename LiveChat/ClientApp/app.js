var app = angular.module('MyDnnSupportApp', ['ngRoute', 'ngAnimate', 'ui.bootstrap', 'angular-loading-bar', 'localytics.directives', 'ngFileUpload', 'angular-bootstrap-select', 'ngAudio', 'farbtastic', 'ngResource', 'ngSanitize', 'ngMyDnnServices', 'services.hub'])
app.config(['$routeProvider', function ($routeProvider) {
    $routeProvider
    .when('/visitorlist', {
        templateUrl: 'visitorlist.html',
        controller: 'visitorListController',
    })
    .when('/history', {
        templateUrl: 'history.html',
        controller: 'historyController',
    })
    .when('/departments', {
        templateUrl: 'departments.html',
        controller: 'departmentController',
    })
    .when('/agents', {
        templateUrl: 'agents.html',
        controller: 'agentController',
    })
    .when('/settings', {
        templateUrl: 'basicsettings.html',
        controller: 'settingController',
    })
    .when('/widgetsettings', {
        templateUrl: 'widgetsettings.html',
        controller: 'widgetSettingController',
    })
    .otherwise({
        redirectTo: '/visitorlist'
    });
}]);

app.controller("adminPanelController", function ($scope, $http, $timeout, cfpLoadingBar, activeMenu) {
    $scope.activeMenu = activeMenu.getActiveMenu();
    $scope.$watch(function () { return activeMenu.getActiveMenu(); }, function (newValue, oldValue) {
        if (newValue !== oldValue) $scope.activeMenu = newValue;
    });
});