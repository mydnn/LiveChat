var ngMyDnnServices = angular.module('ngMyDnnServices', [])
    .factory('localizationService', ['$http', function ($http) {
        var sdo = {
            getResources: function (moduleApi, controller, resource, culture) {
                return $http.get(moduleApi + "/" + controller + "/GetResources", { params: { resource: resource, culture: culture } });
            }
        }
        return sdo;
    }])
    .factory('activeMenu', function () {
        var activeMenu = {
            Parent: "home",
            Child: undefined
        };
        return {
            getActiveMenu: function () {
                return activeMenu;
            },
            setActiveMenu: function (val) {
                activeMenu = val;
            }
        };
    }).directive('focusOn', function () {
        return function (scope, elem, attr) {
            scope.$on(attr.focusOn, function (e) {
                elem[0].focus();
            });
        };
    }).directive('dynamic', function ($compile) {
        return {
            restrict: 'A',
            replace: true,
            link: function (scope, ele, attrs) {
                scope.$watch(attrs.dynamic, function (html) {
                    ele.html(html);
                    $compile(ele.contents())(scope);
                });
            }
        };
    }).directive('intNumber', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, element, attr, ctrl) {
                function inputValue(val) {
                    if (val) {
                        var digits = val.replace(/[^0-9]/g, '');

                        if (digits !== val) {
                            ctrl.$setViewValue(digits);
                            ctrl.$render();
                        }
                        return parseInt(digits, 10);
                    }
                    return undefined;
                }
                ctrl.$parsers.push(inputValue);
            }
        };
    }).directive('realNumber', function () {
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function (scope, element, attr, ctrl) {
                function inputValue(val) {
                    if (val) {
                        var digits = val.replace(/[^0-9.]/g, '');

                        if (digits !== val) {
                            ctrl.$setViewValue(digits);
                            ctrl.$render();
                        }
                        return parseFloat(digits);
                    }
                    return undefined;
                }
                ctrl.$parsers.push(inputValue);
            }
        };
    }).directive('numberMask', function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                $(element).numeric();
            }
        }
    }).directive('schrollBottom', function () {
        return function (scope, elem, attr) {
            scope.$on(attr.schrollBottom, function (event, args) {
                var $element = angular.element('[data-livechat-messages=' + args.id + ']');
                $element.animate({ scrollTop: $element.prop("scrollHeight") }, 500);
            });
        };
    }).filter('cmdate', ['$filter', function ($filter) {
        return function (input, format) {
            return moment(input).format(format);
        };
    }
    ]);
