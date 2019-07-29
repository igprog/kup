angular.module('app', [])
.config(['$httpProvider', ($httpProvider) => {
    //*******************disable catche**********************
        if (!$httpProvider.defaults.headers.get) {
            $httpProvider.defaults.headers.get = {};
        }
        $httpProvider.defaults.headers.get['If-Modified-Since'] = 'Mon, 26 Jul 1997 05:00:00 GMT';
        $httpProvider.defaults.headers.get['Cache-Control'] = 'no-cache';
        $httpProvider.defaults.headers.get['Pragma'] = 'no-cache';
    //*******************************************************
}])

.factory('f', ['$http', ($http) => {
    return {
        post: (service, method, data) => {
            return $http({
                url: './' + service + '.asmx/' + method,
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
            return './assets/partials/' + tpl + '.html';
        },
        days: () => {
            var days = [];
            for (var i = 1; i <= 31; i++) {
                days.push(i);
            }
            return days;
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
            for (var i = 2018; i <= year; i++) {
                years.push(i);
            }
            return years;
        },
        day: () => {
            return new Date().getDay();
        },
        month: () => {
            return  new Date().getMonth() + 1;
        },
        year: () => {
            return new Date().getFullYear();
        },
        setDate: (x) => {
            var day = x.getDate();
            var mo = x.getMonth() < 10 ? '0' + (x.getMonth() + 1) : x.getMonth() + 1;
            var yr = x.getFullYear();
            return yr + '-' + mo + '-' + day;
        },
        pdfTempPath: (x) => {
            return './upload/pdf/temp/' + x + '.pdf';
        },
        defined: (x) => {
            return x === undefined ? false : true;
        }
    }
}])


.controller('appCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {

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
        $http.get('./config/config.json').then((response) => {
              $scope.config = response.data;
              reloadPage();
          });
    };
    getConfig();

    var loadGlobalData = () => {
        var data = {
            currTpl: f.currTpl('login'),
            currTplTitle: null,
            date: new Date(),
            month: new Date().getMonth() + 1,
            year: new Date().getFullYear(),
            months: f.months(),
            years: f.years(),
            buisinessUnits: []
        }
        f.post('BuisinessUnit', 'Load', {}).then((d) => {
            data.buisinessUnits = d;
        });
        $scope.g = data;
    }
    loadGlobalData();

    var data = {
        admin: {
            userName: null,
            password: null
        },
        isLogin: false
    }
    $scope.d = data;

    $scope.login = (x) => {
        f.post('Admin', 'Login', { username: x.userName, password: x.password }).then((d) => {
            if (d === true) {
                $scope.g.currTpl = f.currTpl('dashboard');
                $scope.g.currTplTitle = $scope.config.apptitle;
            } else {
                $scope.g.currTpl = f.currTpl('login');
            }
            $scope.d.isLogin = d;
            //TODO: save user to session storage
        });
    }

    $scope.logout = () => {
        $scope.g.currTpl = f.currTpl('login');
        $scope.d.isLogin = false;
        //TODO: clear session storage
    }

    $scope.toggleTpl = (tpl, title) => {
        $scope.g.currTpl = f.currTpl(tpl);
        $scope.g.currTplTitle = title;
    }

}])

.controller('userCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'User'
    var init = () => {
        f.post('User', 'Init', {}).then((d) => {
            $scope.d.user = d;
        });
    }

    var load = (bu) => {
        if (f.defined(bu)) {
            f.post(service, 'Load', { buisinessUnitCode: bu }).then((d) => {
                $scope.d.users = d;
                $scope.d.year = f.year();
            });
        }
    }

    $scope.load = (bu) => {
        load(bu);
    }
   
    if ($scope.g.currTpl == './assets/partials/newuser.html') {
        var data = {
            user: {},
            users: [],
            buisinessUnitCode: null,
            year: f.year(),
            records: [],
            pdf: null,
            loadingPdf: false
        }
        $scope.d = data;
        init();
    } else {
        $scope.d.buisinessUnitCode = null;
        $scope.d.pdf = null;
        $scope.d.loadingPdf = false;
        load(null);
    }

    $scope.save = (x) => {
        x.accessDate = f.setDate(x.accessDate);
        x.birthDate = f.setDate(x.birthDate);
        f.post(service, 'Save', { x: x }).then((d) => {
            alert(d);
        });
    }

    $scope.get = (tpl, title, id, year) => {
        f.post(service, 'Get', { id: id, year: year }).then((d) => {
            $scope.d.user = d;
            $scope.g.currTpl = f.currTpl(tpl);
            $scope.g.currTplTitle = title;
        });
    }

    $scope.cancelUser = (x) => {
        if (confirm('Deaktiviraj korisnika: ' + x.lastName + ' ' + x.firstName)) {
            f.post('User', 'Cancel', { id: x }).then((d) => {
                alert(d);
            });
        }
    }

    $scope.saveRecord = (x, idx) => {
        if (x.repayment > x.restToRepayment) {
            alert('Rata je veća od duga!');
            return false;
        }
        f.post('Account', 'Save', { x: x }).then((d) => {
            $scope.d.user.records[idx].repaid = d.repaid;
            $scope.d.user.records[idx].restToRepayment = d.restToRepayment;
        });
    }

    $scope.printCard = (x) => {
        if (f.defined(x.user.records.length)) {
            if (x.user.records.length == 0) { return false; }
        }
        $scope.d.pdf = null;
        $scope.d.loadingPdf = true;
        f.post('Pdf', 'Card', { year: x.year, user: x.user }).then((d) => {
            $scope.d.pdf = f.pdfTempPath(d);
            $scope.d.loadingPdf = false;
        });
    }

    $scope.removePdfLink = () => {
        $scope.d.pdf = null;
    }


}])

.controller('buisinessUnitCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
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

.controller('loanCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Loan';
    var data = {
        users: [],
        loan: {},
        date: new Date()
    };
    $scope.d = data;

    var loadUsers = (bu) => {
        f.post('User', 'Load', { buisinessUnitCode: bu}).then((d) => {
            $scope.d.users = d;
        });
    }

    var init = () => {
        f.post(service, 'Init', {}).then((d) => {
            $scope.d.loan = d;
            loadUsers(null);
        });
    }
    init();

    $scope.calculate = (x) => {
        if (x.loan > 0) {
            $scope.d.loan.manipulativeCosts = (x.loan * x.manipulativeCostsCoeff).toFixed(2);
            $scope.d.loan.repayment = (x.loan / x.dedline).toFixed(2);
        }
    }

    $scope.save = (x, date) => {
        x.loanDate = f.setDate(date);
        f.post(service, 'Save', { x: x }).then((d) => {
            alert(d);
        });
    }

}])

.controller('loansCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Loan';
    var data = {
        loans: [],
        month: f.month(),
        year: f.year(),
        buisinessUnitCode: null,
        pdf: null,
        loadingPdf: false
    };
    $scope.d = data;

    $scope.load = (x) => {
        f.post('Loan', 'Load', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode }).then((d) => {
            $scope.d.loans = d;
        });
    }

    $scope.print = (x) => {
        if (f.defined(x.loans.length)) {
            if(x.loans.length == 0) { return false; }
        }
        $scope.d.pdf = null;
        $scope.d.loadingPdf = true;
        f.post('Pdf', 'Loans', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode, loans: x.loans }).then((d) => {
            $scope.d.pdf = f.pdfTempPath(d);
            $scope.d.loadingPdf = false;
        });
    }

    $scope.removePdfLink = () => {
        $scope.d.pdf = null;
    }

}])

.controller('accountCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Account';
    var data = {
        records: {},
        account: {},
        month: $scope.g.month,
        year: $scope.g.year,
        buisinessUnitCode: null
    }
    $scope.d = data;

    var getMonthlyRecords = (x) => {
        f.post(service, 'GetMonthlyRecords', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode }).then((d) => {
            $scope.d.records = d;
        });
    }

    $scope.getMonthlyRecords = (x) => {
        return getMonthlyRecords(x);
    }

    $scope.save = (x, idx) => {
        if (x.repayment > x.restToRepayment) {
            alert('Rata je veća od duga!');
            return false;
        }
        f.post(service, 'Save', { x: x }).then((d) => {
            $scope.d.records.data[idx].repaid = d.repaid;
            $scope.d.records.data[idx].restToRepayment = d.restToRepayment;
        });
    }

}])

.controller('suspensionCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
        var service = 'Account';
        var data = {
            records: {},
            month: $scope.g.month,
            year: $scope.g.year,
            buisinessUnitCode: null,
            pdf: null,
            loadingPdf: false
        }
        $scope.d = data;

        var getMonthlyRecords = (x) => {
            f.post(service, 'GetMonthlyRecords', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode }).then((d) => {
                $scope.d.records = d;
            });
        }

        $scope.getMonthlyRecords = (x) => {
            return getMonthlyRecords(x);
        }

        $scope.print = (x) => {
            if (f.defined(x.records.data.length)) {
                if (x.records.data.length == 0) { return false; }
            }
            $scope.d.pdf = null;
            $scope.d.loadingPdf = true;
            f.post('Pdf', 'Suspension', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode, records: x.records }).then((d) => {
                $scope.d.pdf = f.pdfTempPath(d);
                $scope.d.loadingPdf = false;
            });
        }

        $scope.removePdfLink = () => {
            $scope.d.pdf = null;
        }

}])

.controller('entryCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Entry';
    var data = {
        records: {},
        month: $scope.g.month,
        year: $scope.g.year,
        pdf: null,
        loadingPdf: false
    }
    $scope.d = data;

    var load = (x) => {
        f.post(service, 'Load', { month: x.month, year: x.year }).then((d) => {
            $scope.d.records = d;
        });
    }

    $scope.load = (x) => {
        return load(x);
    }

    $scope.print = (x) => {
        alert('TODO');
        /*
        if (f.defined(x.records.data.length)) {
            if (x.records.data.length == 0) { return false; }
        }
        $scope.d.pdf = null;
        $scope.d.loadingPdf = true;
        f.post('Pdf', 'Entry', { month: x.month, year: x.year, records: x.records }).then((d) => {
            $scope.d.pdf = f.pdfTempPath(d);
            $scope.d.loadingPdf = false;
        });
        */
    }

    $scope.removePdfLink = () => {
        $scope.d.pdf = null;
    }

}])

.controller('adminCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Admin';
    
    var data = {
        admin: {
            userName: null,
            password: null
        },
        isLogin: false,
        userId: null,
        truncateTbl: null,
        dropTbl: null
    }
    $scope.d = data;

    $scope.login = (x) => {
        f.post('Admin', 'LoginSupervisor', { username: x.userName, password: x.password }).then((d) => {
            $scope.d.isLogin = d;
        });
    }

    $scope.removeUser = (x) => {
        if (confirm('Dali ste sigurni da želite izbrisati korisnika: ' + x + '?')) {
            f.post('User', 'Delete', { id: x }).then((d) => {
                alert(d);
            });
        }
    }

    $scope.sql = (x, tbl) => {
        if (confirm('Dali ste sigurni da želite izbrisati tablicu: ' + x + '?')) {
            f.post('Admin', 'Sql', { method: x, tbl: tbl }).then((d) => {
                alert(d);
            });
        }
    }

}])

.directive('modalDirective', () => {
    return {
        restrict: 'E',
        scope: {
            id: '=',
            title: '=',
            data: '=',
            src: '='
        },
        templateUrl: './assets/partials/directive/modal.html'
    };
})

.directive('loadingDirective', () => {
    return {
        restrict: 'E',
        scope: {
            title: '=',
            loadingtitle: '=',
            value: '='
        },
        templateUrl: './assets/partials/directive/loading.html'
    };
})

.directive('linkDirective', () => {
    return {
        restrict: 'E',
        scope: {
            id: '=',
            href: '=',
            src: '=',
            title: '=',
            ico: '='
        },
        templateUrl: './assets/partials/directive/link.html'
    };
})


.directive('dateDirective', () => {
    return {
        restrict: 'E',
        scope: {
            fromYear: '=',
        },
        templateUrl: './assets/partials/directive/date.html',
        controller: 'dateCtrl'
    };
})
.controller('dateCtrl', ['$scope', 'f', ($scope, f) => {
    var d = {
        days: f.days(),
        months: f.months()
    }
    $scope.d = d;

    var format = (x) => {
        return (x < 10 ? '0' + x : x);
    }

    $scope.getDate = () => {
        $scope.$parent.date = $scope.yr + '-' + format($scope.mo) + '-' + format($scope.day);
    }
}])

.directive('checkLink', ($http) => {
    return {
        restrict: 'A',
        link: (scope, element, attrs) => {
            attrs.$observe('href', (href) => {
                $http.get(href).success(() => {
                }).error(() => {
                    element.attr('class', 'btn btn-warning');
                    element.attr('disabled', 'disabled');
                });
            });
        }
    };
})

;
