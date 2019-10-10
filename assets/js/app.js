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
                days.push({
                    id: i,
                    title: i < 10 ? '0' + i : i
                })
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
        years: (fromYear) => {
            var year = new Date().getFullYear();
            var years = [];
            for (var i = fromYear; i <= year; i++) {
                years.push(i);
            }
            return years;
        },
        day: () => {
            return new Date().getDay();
        },
        month: () => {
            return new Date().getMonth() + 1;
        },
        year: () => {
            return new Date().getFullYear();
        },
        setDate: (x) => {
            var day = x.getDate();
            day = day < 10 ? '0' + day : day;
            var mo = x.getMonth();
            mo = mo + 1 < 10 ? '0' + (mo + 1) : mo + 1;
            var yr = x.getFullYear();
            return yr + '-' + mo + '-' + day;
        },
        pdfTempPath: (x) => {
            return './upload/pdf/temp/' + x + '.pdf';
        },
        defined: (x) => {
            return x === undefined ? false : true;
        },
        isValidDate(str) {
            if (str == null) { return false; }
            var parts = str.split('-');
            if (parts.length < 3)
                return false;
            else {
                var year = parseInt(parts[0]);
                var month = parseInt(parts[1]);
                var day = parseInt(parts[2]);
                if (isNaN(day) || isNaN(month) || isNaN(year)) { return false; }
                if (day < 1 || year < 1) return false;
                if (month > 12 || month < 1) return false;
                if ((month == 1 || month == 3 || month == 5 || month == 7 || month == 8 || month == 10 || month == 12) && day > 31) return false;
                if ((month == 4 || month == 6 || month == 9 || month == 11) && day > 30) return false;
                if (month == 2) {
                    if (((year % 4) == 0 && (year % 100) != 0) || ((year % 400) == 0 && (year % 100) == 0)) {
                        if (day > 29) return false;
                    } else {
                        if (day > 28) return false;
                    }
                }
                return true;
            }
        },
        recordTypes: () => {
            return [
                { id: 'bankFee', title: 'Troškovi održavanja računa' },
                { id: 'interest', title: 'Kamata po štednji' },
                { id: 'otherFee', title: 'Ostalo' }
            ]
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

	var loadGlobalData = () => {
	    var data = {
	        currTpl: sessionStorage.getItem('islogin') == 'true' ? f.currTpl('dashboard') : f.currTpl('login'),
	        currTplTitle: sessionStorage.getItem('islogin') == 'true' ? 'Naslovna' : null,
	        currTplType: null,
	        date: new Date(),
	        month: new Date().getMonth() + 1,
	        year: new Date().getFullYear(),
	        months: f.months(),
	        years: f.years($scope.config.fromYear),
	        buisinessUnits: [],
	        recordTypes: f.recordTypes(),
	        clearView: false,
	        total: null
	    }
	    f.post('BuisinessUnit', 'Load', {}).then((d) => {
	        data.buisinessUnits = d;
	    });
	    $scope.g = data;
	}

    var getConfig = () => {
        $http.get('./config/config.json').then((response) => {
            $scope.config = response.data;
            reloadPage();
            loadGlobalData();
        });
    };
    getConfig();

    var data = {
        admin: {
            userName: null,
            password: null
        },
        isLogin: sessionStorage.getItem('islogin') == 'true' ? true : false
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
            sessionStorage.setItem('islogin', d);
        });
    }

    $scope.logout = () => {
        $scope.g.currTpl = f.currTpl('login');
        $scope.d.isLogin = false;
        sessionStorage.setItem('islogin', false);
    }

    $scope.toggleTpl = (tpl, title, currTplType) => {
        if (tpl==null) {return false;}
        $scope.g.currTpl = f.currTpl(tpl);
        $scope.g.currTplTitle = title;
        if (f.defined(currTplType)) {   // ***** Only for recapitualtion and fee *****
            $scope.g.currTplType = currTplType;
            $scope.g.clearView = true;
        }
    }

}])

.controller('dashboardCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {

    $scope.d = {
        total: null
    }

    var currency = (x) => {
        return x.toFixed(2) + ' ' + $scope.config.currency;
    }

    google.charts.load("current", { packages: ["corechart"] });
    google.charts.setOnLoadCallback(drawPieChart);
    function drawPieChart() {
        if ($scope.d.total == null) { return false; }
        if (!f.defined(google.visualization)) { return false;}
        var data = google.visualization.arrayToDataTable([
          ['Task', ''],
          ['Ulozi: ' + currency($scope.d.total.userPaymentWithMonthlyFee), $scope.d.total.userPaymentWithMonthlyFee],
          ['Pozajmice: ' + currency($scope.d.total.activatedLoan), $scope.d.total.activatedLoan],
          ['Troškovi održavanja računa: ' + currency($scope.d.total.bankFee), $scope.d.total.bankFee],
          ['Kamate po štednji: ' + currency($scope.d.total.interest), $scope.d.total.interest],
          ['Ostali troškovi: ' + currency($scope.d.total.otherFee), $scope.d.total.otherFee]
        ]);
        var options = {
            title: 'Ukupni promet',
            is3D: true
        };
        var chart = new google.visualization.PieChart(document.getElementById('piechart_3d'));
        chart.draw(data, options);
    }

    google.charts.load('current', { 'packages': ['corechart'] });
    google.charts.setOnLoadCallback(drawChart);
    function drawChart() {
        var x = $scope.d.total.monthlyTotalList;
        var data = google.visualization.arrayToDataTable([
          ['Mjesec', 'Prihodi', 'Rashodi'],
          ['01', x[0].total.input, x[0].total.output],
          ['02', x[1].total.input, x[1].total.output],
          ['03', x[2].total.input, x[2].total.output],
          ['04', x[3].total.input, x[3].total.output],
          ['05', x[4].total.input, x[4].total.output],
          ['06', x[5].total.input, x[5].total.output],
          ['07', x[6].total.input, x[6].total.output],
          ['08', x[7].total.input, x[7].total.output],
          ['09', x[8].total.input, x[8].total.output],
          ['10', x[9].total.input, x[9].total.output],
          ['11', x[10].total.input, x[10].total.output],
          ['12', x[11].total.input, x[11].total.output]
        ]);

        var options = {
            title: 'Prihodi i rashodi u ' + f.year(),
            hAxis: { title: 'Mjesec', titleTextStyle: { color: '#333' } },
            vAxis: { title: 'Iznos (' + $scope.config.currency + ')', minValue: 0 }
        };

        var chart = new google.visualization.AreaChart(document.getElementById('chart_div'));
        chart.draw(data, options);
    }

    var loadTotal = () => {
        f.post('Account', 'LoadTotal', {}).then((d) => {
            $scope.d.total = d;
            drawPieChart();
            drawChart();
        });
    }
    loadTotal();

}])

.controller('userCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'User';
    var statusChange = false;
    var init = () => {
        f.post('User', 'Init', {}).then((d) => {
            $scope.d.user = d;
            if (d.accessDate == null) {
                $scope.d.user.accessDate = new Date();
            }
        });
    }

    var load = (x) => {
        if (f.defined(x.buisinessUnitCode)) {
            $scope.d.loading = true;
            f.post(service, 'Load', { buisinessUnitCode: x.buisinessUnitCode, search: x.search }).then((d) => {
                $scope.d.users = d;
                $scope.d.year = f.year();
                $scope.d.loading = false;
            });
        }
    }

    $scope.load = (d) => {
        load(d);
    }
   
    if ($scope.g.currTpl == './assets/partials/newuser.html') {
        var data = {
            user: {},
            users: [],
            buisinessUnitCode: null,
            search: null,
            year: f.year(),
            records: [],
            statusDate: new Date(),
            pdf: null,
            loadingPdf: false,
            showPdf: false,
            loading: false
        }
        $scope.d = data;
        init();
    } else {
        $scope.d.buisinessUnitCode = null;
        $scope.d.search = null;
        $scope.d.year = null;
        $scope.d.pdf = null;
        $scope.d.loadingPdf = false;
        $scope.d.showPdf = true;
        $scope.d.loading = true;
        load($scope.d);
    }

    var validate = (x) => {
        if (x.buisinessUnit.code == null) {
            alert('Odaberite poslovnu jedinicu!');
            return false;
        }
        if (x.id == null || x.firstName == null || x.lastName == null ) {
            alert('Nepotpuni unos!');
            return false;
        }
        //x.accessDate = document.getElementById('accessDate').innerText;
        //x.birthDate = document.getElementById('birthDate').innerText;
        if (!f.isValidDate(x.accessDate)) {
            alert('Neispravan datum pristupa!');
            return false;
        } else if (!f.isValidDate(x.birthDate)) {
            alert('Neispravan datum rođenja!');
            return false;
        } else {
            return true;
        }
    }

    $scope.save = (x) => {
        x.accessDate = f.setDate(x.accessDate);
        if (x.birthDate != null) {
            x.birthDate = f.setDate(x.birthDate);
        }
        if (!validate(x)) {
            $scope.d.user.accessDate = new Date($scope.d.user.accessDate);
            if ($scope.d.user.birthDate != null) {
                $scope.d.user.birthDate = new Date($scope.d.user.birthDate);
            }
            return false;
        }
        f.post(service, 'Save', { x: x }).then((d) => {
            alert(d);
            $scope.d.user.accessDate = new Date($scope.d.user.accessDate);
            $scope.d.user.birthDate = new Date($scope.d.user.birthDate);
            $scope.d.showPdf = true;
        });
    }

    $scope.changeUserStatus = () => {
        statusChange = !statusChange;
    }

    $scope.saveUserStatus = (x) => {
        if (!statusChange) { return false;}
        x.terminationDate = f.setDate(x.terminationDate);
        if (!f.isValidDate(x.terminationDate)) {
            alert('Neispravan datum!');
            return false;
        }
        if (x.terminationWithdraw < 0) {
            alert('Dug nije podmiren. Iznos duga: ' + x.terminationWithdraw + ' ' + $scope.config.currency);
            return false;
        }
        f.post(service, 'SaveUserStatus', { x: x }).then((d) => {
            alert(d);
        });
    }

    $scope.get = (tpl, title, id, year) => {
        f.post(service, 'Get', { id: id, year: year }).then((d) => {
            $scope.d.user = d;
            $scope.d.user.accessDate = new Date(d.accessDate);
            $scope.d.user.birthDate = new Date(d.birthDate);
            $scope.d.user.terminationDate = new Date(d.terminationDate);
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

    $scope.print = (x, method) => {
        if (method != 'NewUser') {
            if (f.defined(x.user.records.length)) {
                if (x.user.records.length == 0) { return false; }
            }
        }
        $scope.d.pdf = null;
        $scope.d.loadingPdf = true;
        if (method == 'NewUser') {
            x.user.accessDate = f.setDate(x.user.accessDate);
            x.user.birthDate = f.setDate(x.user.birthDate);

            //x.user.accessDate = document.getElementById('accessDate').innerText;
            //x.user.birthDate = document.getElementById('birthDate').innerText;
            if (!validate(x.user)) {
                $scope.d.loadingPdf = false;
                return false;
            }
            f.post('Pdf', method, { user: x.user }).then((d) => {
                $scope.d.pdf = f.pdfTempPath(d);
                $scope.d.loadingPdf = false;
            });
        } else {
            f.post('Pdf', method, { year: x.year, user: x.user }).then((d) => {
                $scope.d.pdf = f.pdfTempPath(d);
                $scope.d.loadingPdf = false;
            });
        }
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

    var loadUsers = (bu, search) => {
        $scope.d.loading = true;
        f.post('User', 'Load', { buisinessUnitCode: bu, search: search }).then((d) => {
            $scope.d.users = d;
            $scope.d.loading = false;
        });
    }

    var init = () => {
        f.post(service, 'Init', {}).then((d) => {
            $scope.d.loan = d;
            $scope.d.loan.loanDate = new Date();
            loadUsers(null, null);
        });
    }

    if ($scope.g.currTpl == './assets/partials/newloan.html') {
        var data = {
            users: [],
            loan: {},
            records: {},
            month: f.month(),
            year: f.year(),
            buisinessUnitCode: null,
            search: null,
            pdf: null,
            loadingPdf: false,
            loading: false
        };
        $scope.d = data;
        init();
    } else {
        $scope.d.records = null;
        $scope.d.month = f.month();
        $scope.d.year = f.year();
        $scope.d.buisinessUnitCode = null,
        $scope.d.search = null,
        $scope.d.pdf = null,
        $scope.d.loadingPdf = false;
        $scope.d.showPdf = true;
        $scope.d.loading = false;
        loadUsers(null, null);
    }

    $scope.calculate = (x) => {
        if (x.loan > 0) {
            $scope.d.loan.manipulativeCosts = (x.loan * x.manipulativeCostsCoeff).toFixed(2);
            $scope.d.loan.repayment = (x.loan / x.dedline).toFixed(2);
            //$scope.d.loan.actualLoan = x.loan - $scope.d.loan.manipulativeCosts;
            $scope.d.loan.withdraw = $scope.d.loan.loan - $scope.d.loan.user.restToRepayment - $scope.d.loan.manipulativeCosts;
            //$scope.d.loan.user.activeLoanId = $scope.d.user.activeLoanId;
        }
    }

    $scope.calculateDedline = (x) => {
        if (x.loan > 0) {
            $scope.d.loan.dedline = (x.loan / x.repayment).toFixed(0);
        }
    }

    var validate = (x) => {
        if (x.user.id == null) {
            alert('Odaberite korisnika!');
            return false;
        }
         if (parseInt(x.loan) <= 0 || parseInt(x.repayment) <= 0 || parseInt(x.withdraw) <= 0 || parseInt(x.dedline) <= 0) {
			alert('Neisprava unos!');
            return false;
        }
        if (!f.isValidDate(x.loanDate)) {
            alert('Neispravan datum!');
            return false;
        } else {
            return true;
        }
    }

    $scope.save = (x) => {
        //x.loan.loanDate = document.getElementById('loanDate').innerText; // f.setDate(date);
        x.loan.loanDate = f.setDate(x.loan.loanDate);
        if (!validate(x.loan)) {
            $scope.d.loan.loanDate = new Date($scope.d.loan.loanDate);
            return false;
        }
        x.loan.restToRepayment = x.loan.user.restToRepayment;
        f.post(service, 'Save', { x: x.loan }).then((d) => {
            $scope.d.loan.loanDate = new Date($scope.d.loan.loanDate);
            alert(d);
        });
    }

    $scope.remove = (x) => {
        if (confirm('Briši pozajmicu?')) {
            f.post('Loan', 'Delete', { id: x.id }).then((d) => {
                alert(d);
            });
        }
    }

    $scope.getUser = (id) => {
        if (id !== null) {
            f.post('User', 'Get', { id: id, year: null }).then((d) => {
                $scope.d.loan.user = d;
            });
        }
    }

    $scope.print = (x) => {
        //x.loan.loanDate = document.getElementById('loanDate').innerText;
        x.loan.loanDate = f.setDate(x.loan.loanDate);
        if (!validate(x.loan)) {
            $scope.d.loan.loanDate = new Date($scope.d.loan.loanDate);
            return false;
        }
        $scope.d.pdf = null;
        $scope.d.loadingPdf = true;
        f.post('Pdf', 'Loan', { loan: x.loan }).then((d) => {
            $scope.d.pdf = f.pdfTempPath(d);
            $scope.d.loadingPdf = false;
        });
    }

    $scope.removePdfLink = () => {
        $scope.d.pdf = null;
    }

    var load = (x) => {
        $scope.d.loading = true;
        f.post(service, 'Load', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode }).then((d) => {
            $scope.d.records = d;
            $scope.d.loading = false;
        });
    }

    $scope.load = (x) => {
        load(x);
    }

    $scope.search = (x) => {
        f.post(service, 'Search', { search: x }).then((d) => {
            $scope.d.records = d;
        });
    }

    $scope.get = (id) => {
        f.post(service, 'Get', { id: id }).then((d) => {
            $scope.d.loan = d;
            $scope.d.loan.loanDate = new Date(d.loanDate);
            $scope.g.currTpl = f.currTpl('loan');
            $scope.g.currTplTitle = "Pozajmica";
        });
    }

    $scope.printLoans = (x) => {
        if (f.defined(x.records.length)) {
            if (x.records.length == 0) { return false; }
        }
        $scope.d.pdf = null;
        $scope.d.loadingPdf = true;
        f.post('Pdf', 'Loans', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode, records: x.records }).then((d) => {
            $scope.d.pdf = f.pdfTempPath(d);
            $scope.d.loadingPdf = false;
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
            search: null,
            pdf: null,
            loadingPdf: false
        }
        $scope.d = data;

        var getMonthlyRecords = (x) => {
            f.post(service, 'GetMonthlyRecords', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode, search: x.search }).then((d) => {
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

.controller('monthlyFeeCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Account';
    var data = {
        records: {},
        month: $scope.g.month,
        year: $scope.g.year,
        buisinessUnitCode: null,
        search: null,
        pdf: null,
        loadingPdf: false,
        loading: false
    }
    $scope.d = data;

    var getMonthlyRecords = (x) => {
        if (x.month == null) { x.month = $scope.g.month; }
        if (x.year == null) { x.year = $scope.g.year; }
        $scope.d.loading = true;
        f.post(service, 'GetMonthlyFee', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode, search: x.search }).then((d) => {
            $scope.d.records = d;
            $scope.d.loading = false;
        });
    }

    $scope.getMonthlyRecords = (x) => {
        return getMonthlyRecords(x);
    }

    $scope.setMonthlyFee = (x, idx) => {
        $scope.d.records.data[idx].monthlyFee = x.user.monthlyFee;
    }

    $scope.save = (x, d, idx) => {
        x.month = d.month;
        x.year = d.year;
        f.post('Account', 'SaveMonthlyFee', { x: x }).then((d) => {
            getMonthlyRecords($scope.d);
        });
    }

    $scope.add = (x, idx) => {
        x.userPayment.push({
            id: null,
            userId: x.user.id,
            recordDate: f.setDate(new Date()),
            amount: null,
            note: null
        });
    }

    $scope.saveUserPayment = (x, y, d, idx) => {
        if (y.amount <= 0) {
            alert('Unesit iznos.');
            return false;
        }
        y.month = d.month;
        y.year = d.year;
        f.post('Account', 'SaveUserPayment', { userId: x.user.id, y: y }).then((d) => {
            getMonthlyRecords($scope.d);
        });
    }

    $scope.removeUserPayment = (y) => {
        if (confirm('Briši uplatu ' + y.amount + ' ' + $scope.config.currency + '?')) {
            f.post('Account', 'Delete', { id: y.id }).then((d) => {
                getMonthlyRecords($scope.d);
            });
        }
    }

    $scope.print = (x) => {
        if (f.defined(x.records.data.length)) {
            if (x.records.data.length == 0) { return false; }
        }
        $scope.d.pdf = null;
        $scope.d.loadingPdf = true;
        f.post('Pdf', 'MonthlyFee', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode, records: x.records }).then((d) => {
            $scope.d.pdf = f.pdfTempPath(d);
            $scope.d.loadingPdf = false;
        });
    }

    $scope.removePdfLink = () => {
        $scope.d.pdf = null;
    }

}])

.controller('repaymentCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Account';
    var data = {
        records: {},
        month: $scope.g.month,
        year: $scope.g.year,
        buisinessUnitCode: null,
        search: null,
        pdf: null,
        loadingPdf: false,
        loading: false
    }
    $scope.d = data;

    var getMonthlyRecords = (x) => {
        if (x.month == null) { x.month = $scope.g.month; }
        if (x.year == null) { x.year = $scope.g.year; }
        $scope.d.loading = true;
        f.post(service, 'GetLoanUsers', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode, search: x.search }).then((d) => {
            $scope.d.records = d;
            $scope.d.loading = false;
        });
    }

    $scope.getMonthlyRecords = (x) => {
        return getMonthlyRecords(x);
    }

    $scope.setRepayment = (x, idx) => {
        $scope.d.records.data[idx].amount = x.repayment;
    }

    $scope.save = (x, d, idx) => {
        x.month = d.month;
        x.year = d.year;
        if (x.amount > x.restToRepayment) {
            alert('Rata je veća od duga!');
            return false;
        }
        f.post('Account', 'SaveRepayment', { x: x }).then((d) => {
            $scope.d.records.data[idx].repaid = d.repaid;
            $scope.d.records.data[idx].amount = d.amount;
            $scope.d.records.data[idx].restToRepayment = d.restToRepayment;
        });
    }

    $scope.print = (x) => {
        if (f.defined(x.records.data.length)) {
            if (x.records.data.length == 0) { return false; }
        }
        $scope.d.pdf = null;
        $scope.d.loadingPdf = true;
        f.post('Pdf', 'Repayment', { month: x.month, year: x.year, buisinessUnitCode: x.buisinessUnitCode, records: x.records }).then((d) => {
            $scope.d.pdf = f.pdfTempPath(d);
            $scope.d.loadingPdf = false;
        });
    }

    $scope.removePdfLink = () => {
        $scope.d.pdf = null;
    }

}])

.controller('entryCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Account';
    var data = {
        records: {},
        month: $scope.g.month,
        year: $scope.g.year,
        type: $scope.g.currTplType,
        title: $scope.g.currTplTitle,
        pdf: null,
        loadingPdf: false
    }
    $scope.d = data;

    var load = (x) => {
        f.post(service, 'LoadEntry', { month: x.month, year: x.year }).then((d) => {
            $scope.d.records = d;
            $scope.g.clearView = false;
        });
    }

    var loadBalance = (x) => {
        f.post(service, 'LoadBalanceEntry', { year: x.year, type: x.type }).then((d) => {
            $scope.d.records = d;
            $scope.g.clearView = false;
        });
    }

    $scope.load = (x) => {
        x.type = $scope.g.currTplType;
        x.title = $scope.g.currTplTitle;
        if (x.type == 'entry_I' || x.type == 'entry_II') {
            return loadBalance(x);
        } else {
            return load(x);
        }
    }

    $scope.print = (x) => {
        if (f.defined(x.records.data.length)) {
            if (x.records.data.length == 0) { return false; }
        }
        $scope.d.pdf = null;
        $scope.d.loadingPdf = true;
        f.post('Pdf', 'Entry', { month: x.month, year: x.year, records: x.records, type: null }).then((d) => {
            $scope.d.pdf = f.pdfTempPath(d);
            $scope.d.loadingPdf = false;
        });
       
    }

    $scope.removePdfLink = () => {
        $scope.d.pdf = null;
    }

}])

.controller('entryBalanceCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Account';
    var data = {
        records: {},
        year: $scope.g.year,
        type: $scope.g.currTplType,
        title: $scope.g.currTplTitle,
        pdf: null,
        loadingPdf: false
    }
    $scope.d = data;

    var load = (x) => {
        x.type = $scope.g.currTplType;
        x.title = $scope.g.currTplTitle;
        f.post(service, 'LoadBalanceEntry', { year: x.year, type: x.type }).then((d) => {
            $scope.d.records = d;
            $scope.g.clearView = false;
        });
    }

    $scope.load = (x) => {
        return load(x);
    }

    $scope.print = (x) => {
        if (f.defined(x.records.data.length)) {
            if (x.records.data.length == 0) { return false; }
        }
        $scope.d.pdf = null;
        $scope.d.loadingPdf = true;
        f.post('Pdf', 'Entry', { month: 12, year: x.year, records: x.records, type: x.type }).then((d) => {
            $scope.d.pdf = f.pdfTempPath(d);
            $scope.d.loadingPdf = false;
        });

    }

    $scope.removePdfLink = () => {
        $scope.d.pdf = null;
    }

}])

.controller('feeCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Account';
    var data = {
        fee: {},
        records: {},
        date: new Date(),
        month: $scope.g.month,
        year: $scope.g.year,
        type: $scope.g.currTplType,
        pdf: null,
        loadingPdf: false
    }
    $scope.d = data;

    $scope.save = (x, d, idx) => {
        x.recordType = d.type;
        x.year = d.year;
        if (x.id === null) {
            x.recordDate = f.setDate(x.recordDate);
        }
        f.post(service, 'SaveOtherFee', { x: x }).then((d) => {
            $scope.d.records.data[idx] = d;
            load(x, x.recordType);
        });
    }

    var load = (x) => {
        x.type = $scope.g.currTplType;
        f.post(service, 'Load', { year: x.year, type: x.type }).then((d) => {
            $scope.d.records = d;
            angular.forEach(d.data, function (value, key) {
                $scope.d.records.data[key].recordDate = new Date(value.recordDate);
                $scope.d.records.data[key].month = parseInt(value.month);
            });
            $scope.g.clearView = false;
        });
    }

    $scope.load = (x) => {
        return load(x);
    }

    $scope.add = (type) => {
        f.post(service, 'Init', { type: type }).then((d) => {
            d.recordDate = new Date(d.recordDate);
            $scope.d.records.data.push(d);
        });
    }

    $scope.remove = (x) => {
        if (confirm('Briši unos ' + x.note + ' (' + x.amount + ' ' + $scope.config.currency + ')?')) {
            f.post('Account', 'Delete', { id: x.id }).then((d) => {
                load(x);
            });
        }
    }

    $scope.print = (x) => {
        if (f.defined(x.records.data.length)) {
            if (x.records.data.length == 0) { return false; }
        }
        $scope.d.pdf = null;
        $scope.d.loadingPdf = true;
        f.post('Pdf', 'Fee', { month: x.month, year: x.year, records: x.records, type: x.type }).then((d) => {
            $scope.d.pdf = f.pdfTempPath(d);
            $scope.d.loadingPdf = false;
        });

    }

    $scope.removePdfLink = () => {
        $scope.d.pdf = null;
    }

}])

.controller('recapitulationCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Account';
    var data = {
        records: {},
        date: new Date(),
        month: $scope.g.month,
        year: $scope.g.year,
        type: $scope.g.currTplType,
        title: $scope.g.currTplTitle,
        pdf: null,
        loadingPdf: false
    }
    $scope.d = data;

    load = (x) => {
        x.type = $scope.g.currTplType;
        x.title = $scope.g.currTplTitle;
        var method = null;
        if (x.type == 'income' || x.type == 'incomeExpenseDiff') {
            method = 'LoadBalance';
        } else {
            method = 'LoadRecapitulation';
        }
        f.post(service, method, { year: x.year, type: x.type }).then((d) => {
            $scope.d.records = d;
            $scope.g.clearView = false;
        });
    }

    $scope.load = (x) => {
        return load(x);
    }

    $scope.print = (x) => {
        if (f.defined(x.records.length)) {
            if (x.records.length == 0) { return false; }
        }
        $scope.d.pdf = null;
        $scope.d.loadingPdf = true;
        f.post('Pdf', 'Recapitulation', { month: x.month, records: x.records, title: x.title }).then((d) => {
            $scope.d.pdf = f.pdfTempPath(d);
            $scope.d.loadingPdf = false;
        });
    }

    $scope.removePdfLink = () => {
        $scope.d.pdf = null;
    }

}])

.controller('settingsCtrl', ['$scope', '$http', 'f', ($scope, $http, f) => {
    var service = 'Settings';

    $scope.save = (x) => {
        f.post(service, 'Save', { x: JSON.stringify(x) }).then((d) => {
            $scope.d = d;
        });
    }

    var load = () => {
        f.post(service, 'Load', {}).then((d) => {
            $scope.d = d;
        });
    }
    load();


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
        dropTbl: null,
        date: new Date(),
        manipulativeCosts: null,
        bankFee: null,
        interest: null,
        otherFee: null,
        loading: false
    }
    $scope.d = data;

    $scope.login = (x) => {
        f.post(service, 'LoginSupervisor', { username: x.userName, password: x.password }).then((d) => {
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

    $scope.sql = (method, tbl) => {
        if (confirm('Dali ste sigurni da želite izbrisati tablicu: ' + tbl + '?')) {
            f.post(service, 'Sql', { method: method, tbl: tbl }).then((d) => {
                alert(d);
            });
        }
    }

    $scope.upload = function (date) {
        $scope.d.loading = true;
        var content = new FormData(document.getElementById("formUpload"));
        $http({
            url: 'UploadHandler.ashx',
            method: 'POST',
            headers: { 'Content-Type': undefined },
            data: content,
        }).then(function (response) {
            f.post(service, 'ImportUsersCsv', { date: date }).then((d) => {
                $scope.d.loading = false;
                alert(d);
            });
        },
       function (response) {
           alert(response.data);
       });
    }

    $scope.saveStartBalance = (type, x, date) => {
        f.post(service, 'SaveStartBalance', { type: type, x: x, date: date }).then((d) => {
            alert(d);
        });
    }



}])


/********** Directives **********/
.directive('modalDirective', () => {
    return {
        restrict: 'E',
        scope: {
            id: '=',
            headertitle: '=',
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
            value: '=',
            pdf: '=',
            size: '='
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
            tpl: '=',
            desc: '=',
            ico: '=',
            type: '='
        },
        templateUrl: './assets/partials/directive/link.html'
    };
})


    /*
.directive('dateDirective', () => {
    return {
        restrict: 'E',
        scope: {
            fromyear: '=',
            id: '=',
            date: '=',
            todaybtn: '='
        },
        templateUrl: './assets/partials/directive/date.html',
        controller: 'dateCtrl'
    };
})
.controller('dateCtrl', ['$scope', 'f', ($scope, f) => {

    var getYears = () => {
        return f.years(f.defined($scope.fromyear) ? $scope.fromyear : new Date().getFullYear());
    }

    var format = (x) => {
        return (x < 10 ? '0' + x : x);
    }

    var d = {
        day: f.day(),
        mo: f.month(),
        yr: f.year(),
        days: f.days(),
        months: f.months(),
        years: getYears(),
        alert: null
    }
    $scope.d = d;

    $scope.getDate = (id) => {
        if (f.defined($scope.d.yr) && f.defined($scope.d.mo) && f.defined($scope.d.day)) {
            var date = $scope.d.yr + '-' + format($scope.d.mo) + '-' + format($scope.d.day);
            document.getElementById(id).innerText = $scope.d.yr + '-' + format($scope.d.mo) + '-' + format($scope.d.day);
            if (!f.isValidDate(date)) {
                $scope.d.alert = 'Datum nije ispravan!';
            } else {
                $scope.d.alert = null;
            }
        }
    }

    $scope.today = (id) => {
        $scope.d.day = f.day();
        $scope.d.mo = f.month();
        $scope.d.yr = f.year();
        document.getElementById(id).innerText = $scope.d.yr + '-' + format($scope.d.mo) + '-' + format($scope.d.day);
    }


    //TODO
    //$scope.getDay = (id) => {
    //    console.log(f.days().find(a => a.id === id).title);
    //    return f.days().find(a => a.id === id);
    //}
    //$scope.getMonth = (id) => {
    //    console.log(f.months().find(a => a.id === id).title);
    //    return f.months().find(a => a.id === id);
    //}

}])
*/

.directive('allowOnlyNumbers', function () {
    return {
        restrict: 'A',
        link: function (scope, elm, attrs, ctrl) {
            elm.on('keydown', function (event) {
                var $input = $(this);
                var value = $input.val();
                value = value.replace(',', '.');
                $input.val(value);
                if (event.which == 64 || event.which == 16) {
                    return false;
                } else if (event.which >= 48 && event.which <= 57) {
                    return true;
                } else if (event.which >= 96 && event.which <= 105) {
                    return true;
                } else if ([8, 13, 27, 37, 38, 39, 40].indexOf(event.which) > -1) {
                    return true;
                } else if (event.which == 110 || event.which == 188 || event.which == 190) {
                    return true;
                } else if (event.which == 46) {
                    return true;
                } else {
                    event.preventDefault();
                    return false;
                }
            });
        }
    }
})
/********** Directives **********/

;
