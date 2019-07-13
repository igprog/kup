angular.module('app', [])
.config(['$httpProvider', function ($httpProvider) {
    //*******************disable catche**********************
        if (!$httpProvider.defaults.headers.get) {
            $httpProvider.defaults.headers.get = {};
        }
        $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
        $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
        $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
    //*******************************************************
}])

.factory('f', ['$http', function ($http) {
    return {
        post: (service, method, data) => {
            return $http({
                url: '../' + service + '.asmx/' + method,
                method: 'POST',
                data: data
            })
            .then((response) => {
                return JSON.parse(response.data.d);
            },
            (response) => {
                return response.data.d;
            });
        },
        currTpl: (tpl) => {
            return 'assets/partials/' + tpl + '.html';
        }
    }
}])


.controller('appCtrl', ['$scope', '$http', 'f', function ($scope, $http, f) {

    window.onbeforeunload = () => {
        return "Your work will be lost.";
    };

    $scope.today = new Date();

	var reloadPage = () => {
        if (typeof (Storage) !== 'undefined') {
            if (localStorage.version) {
                if (localStorage.version !== $scope.config.version) {
                    localStorage.version = $scope.config.version;
                    window.location.reload(true);
                }
            } else {
                localStorage.version = $scope.config.version;
            }
        }
    }

    var getConfig = () => {
        $http.get('../config/config.json').then(function (response) {
              $scope.config = response.data;
              reloadPage();
          });
    };
    getConfig();

    var data = {
        admin: {
            userName: null,
            password: null
        },
        isLogin: false,
        currTpl: f.currTpl('login'),
        currTplTitle: null
    }
    $scope.d = data;

    $scope.login = (x) => {
        f.post('Admin', 'Login', { username: x.userName, password: x.password }).then((d) => {
            if (d == true) {
                $scope.d.currTpl = f.currTpl('dashboard');
                $scope.d.currTplTitle = 'Dashboard';
            } else {
                $scope.d.currTpl = f.currTpl('login');
            }
           // $scope.d.currTpl = d == true ? f.currTpl('dashboard') : f.currTpl('login');
            $scope.d.isLogin = d;
            //TODO: save user to session storage
        });
    }

    $scope.logout = () => {
        $scope.d.currTpl = f.currTpl('login');
        $scope.d.isLogin = false;
        //TODO: clear session storage
    }

    $scope.toggleTpl = (tpl, title) => {
        $scope.d.currTpl = f.currTpl(tpl);
        $scope.d.currTplTitle = title;
    }

}])


.controller('userCtrl', ['$scope', '$http', 'f', function ($scope, $http, f) {

    var data = {
        user: {},
        buisinessUnits: []
    }
    $scope.d = data;

    var init = () => {
        f.post('Users', 'Init', {}).then((d) => {
            $scope.d.user = d;
        });
    }
    init();

    var loadBuisinessUnit = () => {
        f.post('BuisinessUnit', 'Load', {}).then((d) => {
            $scope.d.buisinessUnits = d;
        });
    }
    loadBuisinessUnit();

    $scope.save = (x) => {
        f.post('Users', 'Save', { x: x }).then((d) => {
            alert(d);
        });
    }

}])

.controller('usersCtrl', ['$scope', '$http', 'f', function ($scope, $http, f) {

    var data = {
        users: {}
    }
    $scope.d = data;

    var load = () => {
        f.post('Users', 'Load', {}).then((d) => {
            $scope.d.users = d;
        });
    }
    load();

}])

.controller('buisinessUnitCtrl', ['$scope', '$http', 'f', function ($scope, $http, f) {
    var service = 'BuisinessUnit';

    $scope.save = (x) => {
        f.post(service, 'Save', { x: x }).then((d) => {
            alert(d);
        });
    }

    var load = () => {
        f.post(service, 'Load', {}).then((d) => {
            $scope.d = d;
        });
    }
    load();

    var remove = (id) => {
        f.post(service, 'Delete', { id: id }).then((d) => {
        });
    }

    $scope.add = () => {
        f.post(service, 'Init', {}).then((d) => {
            $scope.d.push(d);
        });
    }

    $scope.remove = (id, idx) => {
        if (confirm('Briši?')) {
            $scope.d.splice(idx, 1);
            remove(id);
        }
    }

}])

.controller('accountCtrl', ['$scope', '$http', 'f', function ($scope, $http, f) {
    var service = 'Account';
    var data = {
        users: [],
        account: {},
        month: new Date().getMonth() + 1,
        year: new Date().getFullYear()
    }
    $scope.d = data;

    var GetMonthlyRecords = (month, year) => {
        f.post(service, 'GetMonthlyRecords', { month: month, year: year }).then((d) => {
            $scope.d.users = d;
        });
    }
    GetMonthlyRecords(data.month, data.year);

    $scope.GetMonthlyRecords = (month, year) => {
        return GetMonthlyRecords(month, year);
    }

    $scope.save = (x) => {
        f.post(service, 'Save', { x: x }).then((d) => {
            alert(d);
        });
    }

    //var init = () => {
    //    f.post(service, 'Init', {}).then((d) => {
    //        $scope.d.account = d;
    //    });
    //}
    //init();



}])

.controller('loanCtrl', ['$scope', '$http', 'f', function ($scope, $http, f) {
    var service = 'Loan';
    var data = {

    }
    $scope.d = data;

    var init = () => {
        f.post(service, 'Init', {}).then((d) => {
            $scope.d = d;
        });
    }
    init();

}])



//.directive('sortDirective', function () {
//    return {
//        restrict: 'E',
//        templateUrl: 'partials/filterCtrl.html'
//    };
//})

//.directive('productDirective', function () {
//    return {
//        restrict: 'E',
//        scope: {
//            filtercategory: '=',
//            products: '=',
//            filtervalue: '=',
//            ordervalue: '='
//        },
//        templateUrl: 'partials/products.html'
//    };
//})

//.directive('modalDirective', function () {
//    return {
//        restrict: 'E',
//        scope: {
//            img: '='
//        },
//        templateUrl: 'partials/popup/modal.html'
//    };
//})

//.directive('checkImage', function ($http) {
//    return {
//        restrict: 'A',
//        link: function (scope, element, attrs) {
//            attrs.$observe('ngSrc', function (ngSrc) {
//                $http.get(ngSrc).success(function () {
//                }).error(function () {
//                    element.attr('src', './img/default.png'); // set default image
//                });
//            });
//        }
//    };
//});


;
