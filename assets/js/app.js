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
        },
        months: () => {
            return [
                { id: 1, title: 'siječanj' },
                { id: 2, title: 'veljača' },
                { id: 3, title: 'ožujak' },
                { id: 4, title: 'travanj' },
                { id: 5, title: 'svibanj' },
                { id: 6, title: 'lipanj' },
                { id: 7, title: 'srpanj' },
                { id: 8, title: 'kolovoz' },
                { id: 9, title: 'rujan' },
                { id: 10, title: 'listopad' },
                { id: 11, title: 'studeni' },
                { id: 12, title: 'prosinac' }
            ]
        },
        years: () => {
            var year = new Date().getFullYear();
            var years = [];
            for (var i = 2016; i <= year; i++) {
                years.push(i);
            }
            return years;
        },
        setDate: (x) => {
            debugger;
            var day = x.getDate();
            var mo = x.getMonth() < 10 ? '0' + (x.getMonth() + 1) : x.getMonth() + 1;
            var yr = x.getFullYear();
            return yr + '-' + mo + '-' + day;
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
            if (d === true) {
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
        f.post('User', 'Init', {}).then((d) => {
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
        f.post('User', 'Save', { x: x }).then((d) => {
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
        f.post('User', 'Load', {}).then((d) => {
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
        year: new Date().getFullYear(),
        months: f.months(),
        years: f.years(),
        buisinessUnits: [],
        buisinessUnitCode: null
    }
    $scope.d = data;

    var getMonthlyRecords = (x) => {
        debugger;
        f.post(service, 'GetMonthlyRecords', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode }).then((d) => {
            $scope.d.users = d;
        });
    }

    var loadBuisinessUnit = () => {
        f.post('BuisinessUnit', 'Load', {}).then((d) => {
            $scope.d.buisinessUnits = d;
        });
    }
    loadBuisinessUnit();

    $scope.getMonthlyRecords = (x) => {
        return getMonthlyRecords(x);
    }

    $scope.save = (x, idx) => {
        if (x.repayment > x.restToRepayment) {
            alert('Rata je veća od duga!');
            return false;
        }
        f.post(service, 'Save', { x: x }).then((d) => {
            $scope.d.users[idx].repaid = d.repaid;
            $scope.d.users[idx].restToRepayment = d.restToRepayment;
        });
    }

}])

.controller('loanCtrl', ['$scope', '$http', 'f', function ($scope, $http, f) {
    var service = 'Loan';
    var data = {
        users: [],
        loan: {},
        date: new Date()
    };
    $scope.d = data;

    var loadUsers = () => {
        f.post('User', 'Load', {}).then((d) => {
            $scope.d.users = d;
        });
    }

    var init = () => {
        f.post(service, 'Init', {}).then((d) => {
            $scope.d.loan = d;
            loadUsers();
        });
    }
    init();

    $scope.save = (x, date) => {
        debugger;
        x.loanDate = f.setDate(date);
        f.post(service, 'Save', { x: x }).then((d) => {
            alert(d);
        });
    }


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
