using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using Igprog;

/// <summary>
/// Account
/// </summary>
[WebService(Namespace = "http://igprog.hr/kup/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Account : System.Web.Services.WebService {
    DataBase db = new DataBase();
    Global g = new Global();
    Settings s = new Settings();
    public Account() {
    }

    public class NewAccount {
        public string id;
        public User.NewUser user;
        public double amount;
        public string recordDate;
        public string month;
        public int year;
        public string recordType;
        public string loanId;
        public string note;
        public double monthlyFee;
        public double loan;
        public string loanDate;
        public double repayment;
        public double currRepayment;
        public double lastRepayment;
        public string repaymentDate;
        public double restToRepayment;
        public double totalObligation;
        public double repaid;
        public double lastMonthObligation;
        public bool changed;
        public List<UserPayment> userPayment;
        public double userPaymentTotal;
        public string lastDayInMonth;
        public double monthlyFeeStartBalance;
        public double loanStartBalance;
        public double activatedLoan;
        public Amortization amortization;
    }

    public class Total {
        public double monthlyFee;
        public double userPayment;
        public double userPaymentWithMonthlyFee;
        public double userPaymentWithMonthlyFeeTotal;
        public double repayment;
        public double userRepayment;
        public double terminationRepayment;
        public double repaymentTotal;
        public double restToRepayment;
        public double totalObligation;
        public double terminationWithdraw;
        public double terminationWithdrawWithTerminationRepayment;
        public double userPaymentBalance;  // stanje na racunu
        public double activatedLoan;
        public double withdraw;
        public double loanToRepaid;
        public double bankFee;
        public double otherFee;
        public double interest;
        public double input;
        public double output;
        public double manipulativeCosts;
        public double currRepayment; // uplacena pozajmica
        public double amortization;
        public double softwareInvestment;

        public List<RecapMonthlyTotal> monthlyTotalList;  // TODO
    }

    public class Accounts {
        public List<NewAccount> data;
        public Total total;
    }

    public class Recapitulation {
        public string date;
        public int month;
        public int year;
        public double input;  // duguje
        public double inputAccumulation;  // duguje  (Akumulirano, ukupno do zadnjeg dana u mjesecu)
        public double output;  // potrazuje
        public double outputAccumulation;  // potrazuje  (Akumulirano, ukupno do zadnjeg dana u mjesecu)
        public double accountBalance;  // stanje racuna
        public string recordType;
        public string note;
        //public string account;  // konto  // TODO: CodeTitle
        public CodeTitle account;
    }

    public class RecapMonthlyTotal {
        public string month;
        public Recapitulation total;
    }

    public class RecapYearlyTotal {
        public int year;
        public string type;
        public CodeTitle account;  // Konto
        public List<RecapMonthlyTotal> data;
        public Recapitulation total;
    }

    public class EntryTotal {
        public List<Recapitulation> data;
        public Recapitulation total;
    }

    public class UserPayment {
        public string id;
        public string userId;
        public string recordDate;
        public double amount;
        public string month;
        public int year;
        public string note;
        public string type;
        public string loanId;
    }

    // ***** Konto *****
    public class AccountNo {
        public CodeTitle giroAccount;
        public CodeTitle loan;
        public CodeTitle monthlyFee;
        public CodeTitle manipulativeCosts;
        public CodeTitle bankFee;
        public CodeTitle interest;
        public CodeTitle otherFee;
        public CodeTitle income;
        public CodeTitle expense;
        public CodeTitle incomeExpenseDiff;
        public CodeTitle softwareInvestment;
        public CodeTitle amortization;
        public CodeTitle correction;
    }

    public class CodeTitle {
        public string code;
        public string title;
    }

    public class Amortization {
        public double perc;
        public double amount;
        public double amortized;
        public double capitalAssetsAmount;
    }

    [WebMethod]
    public string Init(string type) {
        NewAccount x = new NewAccount();
        x.id = null;                       //Guid.NewGuid().ToString();
        x.user = new User.NewUser();
        x.amount = 0;
        x.recordDate = g.Date(DateTime.Now);
        x.month = "01";
        x.year = DateTime.Now.Year;
        x.recordType = type;
        x.loanId = null;
        x.note = null;
        x.monthlyFee = 0;
        x.loan = 0;
        x.loanDate = null;
        x.repayment = 0;
        x.repaymentDate = null;
        x.repaid = 0;
        x.restToRepayment = 0;
        x.totalObligation = 0;
        x.amortization = new Amortization();
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string SaveMonthlyFee(NewAccount x) {
        try {
            x.recordType = g.monthlyFee;
            x.amount = x.monthlyFee;
            return JsonConvert.SerializeObject(Save(x), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string SaveUserPayment(string userId, UserPayment y) {
        try {
            NewAccount x = new NewAccount();
            x.user = new User.NewUser();
            x.user.id = userId;
            x.recordType = y.type; // g.userPayment;
            x.amount = y.amount;
            x.id = y.id;
            x.recordDate = y.recordDate;
            x.month = y.month;
            x.year = y.year;
            x.note = y.note;
            x.loanId = y.loanId;
            return JsonConvert.SerializeObject(Save(x), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string SaveRepayment(NewAccount x) {
        try {
            x.recordType = g.repayment;
            x.amount = x.currRepayment;
            return JsonConvert.SerializeObject(Save(x), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string SaveOtherFee(NewAccount x) {
        try {
            return JsonConvert.SerializeObject(Save(x), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Load(int year, string type) {
        try {
            return JsonConvert.SerializeObject(LoadData(year, type), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string LoadCapitalAssets() {
        try {
            db.Account();
            //TODO: Osnovna sredstava, za sada samo ulaganje u racunalne programe, treba dodati i druge vrste osnovnih sredstava ako se pojave
            string sql = string.Format(@"SELECT * FROM Account WHERE recordType = '{0}'", g.softwareInvestment);
            Accounts xx = new Accounts();
            xx.data = new List<NewAccount>();
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewAccount x = ReadData(reader);
                            xx.data.Add(x);
                        }
                    }
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    public Accounts LoadData(int year, string type) {
        db.Account();
        string sql = string.Format(@"SELECT * FROM Account WHERE yr = '{0}' AND recordType = '{1}' ORDER BY mo", year, type);
        Accounts xx = new Accounts();
        xx.data = new List<NewAccount>();
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewAccount x = ReadData(reader);
                            xx.data.Add(x);
                        }
                    }
                }
                connection.Close();
            }
        xx.total = new Total();
        xx.total.otherFee = xx.data.Where(a => a.recordType == g.otherFee).Sum(a => a.amount);
        xx.total.bankFee = xx.data.Where(a => a.recordType == g.bankFee).Sum(a => a.amount);
        xx.total.interest = xx.data.Where(a => a.recordType == g.interest).Sum(a => a.amount);
        xx.total.amortization = xx.data.Where(a => a.recordType == g.amortization).Sum(a => a.amount);
        xx.total.softwareInvestment = xx.data.Where(a => a.recordType == g.softwareInvestment).Sum(a => a.amount);

        xx.data = CheckAmortization(xx.data);

        return xx;
    }

    private List<NewAccount> CheckAmortization(List<NewAccount> data) {
        foreach (NewAccount x in data) {
            if (x.recordType == g.amortization) {
                x.amortization = new Amortization();
                x.amortization.perc = s.Data().capitalAssets.Where(a => a.recordType == g.softwareInvestment).FirstOrDefault().amortization;
                x.amortization.amount = GetAmortizationAmount(x.loanId, x.amortization.perc);
                x.amortization.amortized = GetAmortizedAmount(x.loanId);
                x.amortization.capitalAssetsAmount = GetCapitalAssetsAmount(x.loanId);
                if (x.amortization.amortized >= x.amortization.capitalAssetsAmount) {
                    x.amortization.amount = 0;
                }
            }
        }
        return data;
    }

    [WebMethod]
    public string GetAmortization(string id) {
        try {
            Amortization x = new Amortization();
            x.perc = s.Data().capitalAssets.Where(a => a.recordType == g.softwareInvestment).FirstOrDefault().amortization;
            x.amount = GetAmortizationAmount(id, x.perc);
            x.amortized = GetAmortizedAmount(id);
            x.capitalAssetsAmount = GetCapitalAssetsAmount(id);
            if (x.amortized >= x.capitalAssetsAmount) {
                x.amount = 0;
            }
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    private double GetAmortizationAmount(string capitalAssetId, double amortization) {
        double x = 0, amount = 0;
        string sql = string.Format(@"SELECT amount FROM Account WHERE id = '{0}'", capitalAssetId);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader())  {
                    while (reader.Read()) {
                        amount = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(0));
                    }
                }
            }
            connection.Close();
        }
        x = Math.Round((amount * (amortization / 100) / 12), 2);
        return x;
    }

    private double GetAmortizedAmount(string capitalAssetId) {
        double x = 0;
        string sql = string.Format(@"SELECT SUM(CAST(amount AS DECIMAL(10,2))) FROM Account WHERE loanId = '{0}'", capitalAssetId);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader())  {
                    while (reader.Read()) {
                        x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    private double GetCapitalAssetsAmount(string capitalAssetId) {
        double x = 0;
        string sql = string.Format(@"SELECT SUM(CAST(amount AS DECIMAL(10,2))) FROM Account WHERE id = '{0}'", capitalAssetId);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    [WebMethod]
    public string GetMonthlyFee(int month, int year, string buisinessUnitCode, string search) {
        try {
            User u = new User();
            List<User.NewUser> users = u.GetMonthlyFeeUsers(buisinessUnitCode, search, false, g.ReffDate(month, year));
            db.Account();
            Accounts xx = new Accounts();
            xx.data = new List<NewAccount>();
            foreach(User.NewUser user in users) {
                NewAccount x = new NewAccount();
                x = GetRecord(user.id, month, year, g.monthlyFee);
                if (string.IsNullOrEmpty(x.id)) {
                    x.id = Guid.NewGuid().ToString();
                    x.user = user;
                    x.amount = 0;
                    x.recordDate = g.Date(DateTime.Now);
                    x.recordType = null;
                    x.loanId = null;
                    x.note = string.Format("OD {0}", g.Month(month));
                    x.month = month.ToString();
                    x.year = year;
                    //x = CheckLoan(x, user.id, month, year);  //TODO
                }
                x = CheckMonthlyFee(x, user.id, g.monthlyFee);
                x.userPayment = GetUserPayment(x, user.id, g.userPayment);
                x.userPaymentTotal = x.userPayment.Sum(a => a.amount);
                x.user = user;
                xx.data.Add(x);
            }
            xx.total = new Total();
            xx.total.monthlyFee = xx.data.Sum(a => a.monthlyFee);
            xx.total.userPayment = xx.data.Sum(a => a.userPaymentTotal);
            xx.total.userPaymentWithMonthlyFee = xx.total.monthlyFee + xx.total.userPayment;
            xx.total.repayment = xx.data.Sum(a => a.repayment);
            xx.total.totalObligation = xx.data.Sum(a => a.totalObligation);
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string GetLoanUsers(int month, int year, string buisinessUnitCode, string search) {
        try {
            User u = new User();
            List<User.NewUser> users = u.GetLoanUsers(buisinessUnitCode, search, g.ReffDate(month, year));
            db.Account();
            Accounts xx = new Accounts();
            xx.data = new List<NewAccount>();
            foreach(User.NewUser user in users) {
                NewAccount x = new NewAccount();
                x = GetRecord(user.id, month, year, g.repayment);
                if (string.IsNullOrEmpty(x.id)) {
                    x.id = null; // Guid.NewGuid().ToString();
                    x.user = user;
                    x.amount = 0;
                    x.recordDate = g.Date(DateTime.Now);
                    x.month = month.ToString();
                    x.year = year;
                    x.recordType = null;
                    x.loanId = null;
                    x.note = string.Format("OD {0}", g.Month(month));
                    x.monthlyFee = 0;
                    x.loan = 0;
                    x.loanDate = null;
                    x.repayment = 0;
                    x.currRepayment = 0;
                    x.repaymentDate = null;
                    x.repaid = 0;
                    x.restToRepayment = 0;
                    x.totalObligation = 0;
                    //x = CheckLoan(x, user.id, month, year);  //TODO
                }
                x = CheckLoan(x, user.id);  //TODO

                x.userPayment = GetUserPayment(x, user.id, g.userRepayment);
                x.userPaymentTotal = x.userPayment.Sum(a => a.amount);

                if (x.recordType == g.repayment) {
                    x.currRepayment = x.amount;
                }
                //x = CheckLastRepayment(x, user.id);  //TODO???
                x.user = user;
                xx.data.Add(x);
            }
            xx.total = new Total();
            //xx.total.monthlyFee = xx.data.Sum(a => a.monthlyFee);

            // xx.total.repayment = xx.data.Sum(a => a.amount);
            xx.total.currRepayment = xx.data.Sum(a => a.currRepayment);
            xx.total.restToRepayment = xx.data.Sum(a => a.restToRepayment);
            xx.total.userRepayment = xx.data.Sum(a => a.userPaymentTotal);
            xx.total.repaymentTotal = xx.total.currRepayment + xx.total.userRepayment;
          //  xx.total.totalObligation = xx.data.Sum(a => a.totalObligation);
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string GetMonthlyRecords(int month, int year, string buisinessUnitCode, string search, bool changed) {
        try {
            User u = new User();
            List<User.NewUser> users = u.GetUsers(buisinessUnitCode, search, false);
            db.Account();
            Accounts xx = new Accounts();
            xx.data = new List<NewAccount>();
            foreach(User.NewUser user in users) {
                NewAccount x = new NewAccount();
                //x = GetRecord(user.id, month, year, null);
                List<NewAccount> xxx = new List<NewAccount>();
                xxx = GetRecords(user.id, month, year, null);
                if (xxx.Count > 0) {
                    x = xxx.First();
                    x.monthlyFee = xxx.Where(a => a.recordType == g.monthlyFee || a.recordType == g.userPayment).Sum(a => a.amount);
                    x.currRepayment = xxx.Where(a => a.recordType == g.repayment).Sum(a => a.amount);
                }
                if (string.IsNullOrEmpty(x.id)) {
                    x.id = Guid.NewGuid().ToString();
                    x.user = user;
                    x.amount = 0;
                    x.recordDate = g.Date(DateTime.Now);
                    x.month = month.ToString();
                    x.year = year;
                    x.recordType = null;
                    x.loanId = null;
                    x.note = null;
                    x.monthlyFee = 0;
                    x.loan = 0;
                    x.loanDate = null;
                    x.repayment = 0;
                    x.currRepayment = 0;
                    x.repaymentDate = null;
                    x.repaid = 0;
                    x.restToRepayment = 0;
                    x.totalObligation = 0;
                    //x = CheckLoan(x, user.id, month, year);
                }
                x = CheckLoan(x, user.id);
                x = CheckMonthlyFee(x, user.id, g.monthlyFee);
                x.totalObligation = x.monthlyFee + x.currRepayment;

                // ***** Check last month obligation *****
                NewAccount lastMonth = new NewAccount();
                lastMonth = GetRecord(user.id, month == 1 ? 12 : month - 1, month == 1 ? year - 1 : year, null);
                if (!string.IsNullOrEmpty(lastMonth.id)) {
                    //lastMonth = CheckLoan(lastMonth, user.id);
                    //lastMonth = CheckMonthlyFee(lastMonth, user.id, g.monthlyFee);
                    lastMonth.monthlyFee = GetRecord(user.id, month == 1 ? 12 : month - 1, month == 1 ? year - 1 : year, g.monthlyFee).amount;
                    lastMonth.currRepayment = GetRecord(user.id, month == 1 ? 12 : month - 1, month == 1 ? year - 1 : year, g.repayment).amount;
                    x.lastMonthObligation = lastMonth.monthlyFee + lastMonth.currRepayment;
                } else {
                    x.lastMonthObligation = 0;
                }
                if (x.totalObligation != x.lastMonthObligation) {
                    x.changed = true;
                } else {
                    x.changed = false;
                }
                //****************************************

                x.user = user;
                xx.data.Add(x);
            }
            xx.total = new Total();
            xx.total.monthlyFee = xx.data.Sum(a => a.monthlyFee);
            //xx.total.repayment = xx.data.Sum(a => a.repayment);
            xx.total.currRepayment = xx.data.Sum(a => a.currRepayment);
            xx.total.totalObligation = xx.data.Sum(a => a.totalObligation);
            //xx.total.amount = xx.data.Sum(a => a.amount);
            if (changed) {
                xx.data = xx.data.Where(a => a.changed == true).ToList();
            }
            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Delete(string id) {
        try {
            string msg = null;
            if (!string.IsNullOrEmpty(id)) {
                string sql = string.Format("DELETE FROM Account WHERE id = '{0}'", id);
                using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection)) {
                        command.ExecuteReader();
                    }
                    connection.Close();
                }
                msg = "Obrisano";
            } else {
                msg = "Greška";
            }
            return JsonConvert.SerializeObject(msg, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    /***** Temeljnica *****/
    [WebMethod]
    public string LoadEntry(int year, int month) {
        try {
            db.Account();
            string sql = string.Format(@"SELECT SUM(CAST(a.amount AS DECIMAL(10,2))), a.recordType FROM Account a WHERE yr = '{0}' {1}
                                        GROUP BY a.recordType"
                                        , year
                                        , month == 0 ? "" : string.Format("AND mo = '{0}'" ,month));
            EntryTotal xx = new EntryTotal();
            xx.data = new List<Recapitulation>();
            EntryTotal et = new EntryTotal();
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            Recapitulation x = new Recapitulation();
                            x.date = null;
                            x.month = month;
                            x.year = year;
                            x.input = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                            x.recordType = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
                            xx.data.Add(x);
                        }
                    }
                }
                connection.Close();
                if (month == 0) {
                    et.data = PrepareEntryData_PS(year);  // Pocetno stanje
                } else {
                    et.data = PrepareEntryData(xx.data, year, month);
                }       
                
                et.total = new Recapitulation();
                et.total.input = et.data.Sum(a => a.input);
                et.total.output = et.data.Sum(a => a.output);
            }
            return JsonConvert.SerializeObject(et, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    private List<Recapitulation> PrepareEntryData(List<Recapitulation> xx, int year, int month) {
        List<Recapitulation> xxx = new List<Recapitulation>();
        Recapitulation x = new Recapitulation();
        //x.note = "Žiro račun";
        x.output = xx.Where(a => a.recordType == g.monthlyFee).Sum(a => a.input)
            + xx.Where(a => a.recordType == g.userPayment).Sum(a => a.input)
            + xx.Where(a => a.recordType == g.userRepayment).Sum(a => a.input)
            + xx.Where(a => a.recordType == g.repayment).Sum(a => a.input)
            + xx.Where(a => a.recordType == g.interest).Sum(a => a.input);
        x.input = xx.Where(a => a.recordType == g.withdraw).Sum(a => a.input)
            + xx.Where(a => a.recordType == g.bankFee).Sum(a => a.input)
            + xx.Where(a => a.recordType == g.terminationWithdraw).Sum(a => a.input)
            + xx.Where(a => a.recordType == g.otherFee).Sum(a => a.input);
        //x.account = GetAccountNo(g.giroaccount);
        x.account = GetAccount(g.giroaccount);
        x.note = x.account.title;
        if (x.output > 0 || x.input > 0) {
            xxx.Add(x);
        }

        x = new Recapitulation();
        //x.note = "Pozajmice";
        Loan l = new Loan();
        x.output = l.GetLoansTotal(month, year);
        Loan.Loans loans = l.LoadData(month, year, null, null);
        //x.input = xx.Where(a => a.recordType == g.repayment).Sum(a => a.input)
        //    + xx.Where(a => a.recordType == g.userRepayment).Sum(a => a.input)
        //    + xx.Where(a => a.recordType == g.loan).Sum(a => a.input);
        x.input = xx.Where(a => a.recordType == g.repayment).Sum(a => a.input)
            + xx.Where(a => a.recordType == g.userRepayment).Sum(a => a.input)
            + xx.Where(a => a.recordType == g.loan).Sum(a => a.input)
            + xx.Where(a => a.recordType == g.terminationRepayment).Sum(a => a.input);
        x.account = GetAccount(g.loan);
        x.note = x.account.title;
        if (x.output > 0 || x.input > 0) {
            xxx.Add(x);
        }

        x = new Recapitulation();
        //x.note = "Ulozi";
        //x.output = xx.Where(a => a.recordType == g.terminationWithdraw).Sum(a => a.input);
        x.output = xx.Where(a => a.recordType == g.terminationWithdraw || a.recordType == g.terminationRepayment).Sum(a => a.input);
        x.input = xx.Where(a => a.recordType == g.monthlyFee || a.recordType == g.userPayment).Sum(a => a.input);
        x.account = GetAccount(g.monthlyFee);
        x.note = x.account.title;
        if (x.output > 0 || x.input > 0) {
            xxx.Add(x);
        }

        x = new Recapitulation();
        //x.note = string.Format("Manipulativni troškovi {0}%", g.manipulativeCostsPerc());
        x.output = 0;
        x.input = xx.Where(a => a.recordType == g.manipulativeCosts).Sum(a => a.input);
        x.account = GetAccount(g.manipulativeCosts);
        x.note = x.account.title;
        if (x.output > 0 || x.input > 0) {
            xxx.Add(x);
        }

        x = new Recapitulation();
        //x.note = "Troškovi održavanja računa";
        x.output = xx.Where(a => a.recordType == g.bankFee).Sum(a => a.input);
        x.input = 0;
        x.account = GetAccount(g.bankFee);
        x.note = x.account.title;
        if (x.output > 0 || x.input > 0) {
            xxx.Add(x);
        }

        x = new Recapitulation();
        //x.note = "Kamate po štednji";
        x.output = 0;
        x.input = xx.Where(a => a.recordType == g.interest).Sum(a => a.input);
        x.account = GetAccount(g.interest);
        x.note = x.account.title;
        if (x.output > 0 || x.input > 0) {
            xxx.Add(x);
        }

        x = new Recapitulation();
        //x.note = "Razni materijalni troškovi";
        x.output = xx.Where(a => a.recordType == g.otherFee).Sum(a => a.input);
        x.input = 0;
        x.account = GetAccount(g.otherFee);
        x.note = x.account.title;
        if (x.output > 0 || x.input > 0) {
            xxx.Add(x);
        }

        x = new Recapitulation();
        x.output = xx.Where(a => a.recordType == g.amortization).Sum(a => a.input);
        x.input = 0;
        x.account = GetAccount(g.amortization);
        x.note = x.account.title;
        if (x.output > 0 || x.input > 0) {
            xxx.Add(x);
        }

        x = new Recapitulation();
        x.output = 0;
        x.input = xx.Where(a => a.recordType == g.amortization).Sum(a => a.input);
        x.account = GetAccount(g.correction);
        x.note = x.account.title;
        if (x.output > 0 || x.input > 0) {
            xxx.Add(x);
        }

        return xxx;
    }

    private List<Recapitulation> PrepareEntryData_PS(int year) {
        List<Recapitulation> xx = new List<Recapitulation>();
        Recapitulation x = new Recapitulation();
        //x.note = "Žiro račun";
        double val = GetStartBalance(year, g.giroaccount);
        if (val > 0) {
            x.output = val;
        } else {
            x.output = Math.Abs(val);
        }
        x.account = GetAccount(g.giroaccount);
        x.note = x.account.title;
        xx.Add(x);

        x = new Recapitulation();
        //x.note = "Pozajmice";
        val = GetLoanStartBalance(null, year);
        if (val > 0) {
            x.output = val;
        } else {
            x.output = Math.Abs(val);
        }
        x.account = GetAccount(g.loan);
        x.note = x.account.title;
        xx.Add(x);

        x = new Recapitulation();
        //x.note = "Ulozi";
        val = GetStartBalance(year, g.monthlyFee);
        if (val > 0) {
            x.input = val;
        } else {
            x.output = Math.Abs(val);
        }
        x.account = GetAccount(g.monthlyFee);
        x.note = x.account.title;
        xx.Add(x);

        x = new Recapitulation();
        //x.note = "Prihodi";
        val = GetStartBalance(year, g.income) - GetTotalSaldo(year);
        if (val > 0) {
            x.input = val;
        } else {
            x.output = Math.Abs(val);
        }
        x.account = GetAccount(g.income);
        x.note = x.account.title;
        xx.Add(x);

        return xx;
    }
    /***** Temeljnica *****/

    /***** Temeljnica Bilanca *****/
    [WebMethod]
    public string LoadBalanceEntry(int year, string type) {
        try {
            EntryTotal xx = new EntryTotal();
            xx.data = new List<Recapitulation>();
            EntryTotal et = new EntryTotal();

            et.data = new List<Recapitulation>();
            Recapitulation x = new Recapitulation();
            double input = 0;
            double output = 0;
            if (type == g.entry_I) {
                input = LoadBalanceSql(year, g.otherFee);
                string note = string.Format("Zatv. kart. Prijenos na {0}", GetAccount(g.incomeExpenseDiff).code);
                if (input > 0 ) {
                    x = new Recapitulation();
                    x.note = note;
                    x.input = input;
                    x.account = GetAccount(g.otherFee);
                    et.data.Add(x);
                }

                input = LoadBalanceSql(year, g.bankFee);
                if (input > 0) {
                    x = new Recapitulation();
                    x.note = note;
                    x.input = input;
                    x.account = GetAccount(g.bankFee);
                    et.data.Add(x);
                }

                input = LoadBalanceSql(year, g.amortization);
                if (input > 0) {
                    x = new Recapitulation();
                    x.note = note;
                    x.input = input;
                    x.account = GetAccount(g.amortization);
                    et.data.Add(x);
                }

                output = LoadBalanceSql(year, g.interest);
                if (output > 0) {
                    x = new Recapitulation();
                    x.note = note;
                    x.output = output;
                    x.account = GetAccount(g.interest);
                    et.data.Add(x);
                }

                output = LoadBalanceSql(year, g.manipulativeCosts);
                if (output > 0) {
                    x = new Recapitulation();
                    x.note = note;
                    x.output = output;
                    x.account = GetAccount(g.manipulativeCosts);
                    et.data.Add(x);
                }

                input = LoadBalanceSql(year, g.income);
                output = LoadBalanceSql(year, g.expense);
                x = new Recapitulation();
                x.note = "Donos sa kl.3 i kl.4";
                x.input = input;
                x.output = output;
                x.account = GetAccount(g.incomeExpenseDiff);
                et.data.Add(x);
            } else if (type == g.entry_II) {
                input = LoadBalanceSql(year, g.income);
                output = LoadBalanceSql(year, g.expense);
                x = new Recapitulation();

                double diff = output - input;
                if (diff > 0) {
                    x.note = "Prijenos rashoda";
                    x.input = diff;
                } else {
                    x.note = "Obračun prihoda i rashoda";
                    x.output = input - output;
                }

                x.account = GetAccount(g.incomeExpenseDiff);
                et.data.Add(x);

                x = new Recapitulation();
                if (diff > 0) {
                    x.note = "Donos rashoda";
                    x.output = diff;
                } else {
                    x.note = "Donos na prihode";
                    x.input = input - output;
                }
                x.account = GetAccount(g.income);
                et.data.Add(x);
            } else if (type == g.entry_III) {  // Kad su rashodi veci od prihoda u tekucoj godini
                input = LoadBalanceSql(year, g.income);
                output = LoadBalanceSql(year, g.expense);
                x = new Recapitulation();

                double diff = output - input;
                if (diff > 0) {
                    x.note = "Prijenos rashoda, zatvaranje kartice";
                    x.input = diff;
                } else {
                    x.note = "Prijenos viška prihoda";  // TODO ???
                    x.output = input - output;
                }

                x.account = GetAccount(g.incomeExpenseDiff);
                et.data.Add(x);

                x = new Recapitulation();
                if (diff > 0) {
                    x.note = "Donos rashoda na ukupni prihod";
                    x.output = diff;
                } else {
                    x.note = "Donos viška prihoda";  // TODO ???
                    x.input = input - output;
                }
                x.account = GetAccount(g.income);
                et.data.Add(x);
            } else if (type == g.amortization) {  // Amortizacija - ulaganje u racunalne programe
                input = LoadBalanceSql(year, g.amortization);
                //output = LoadBalanceSql(year, g.amortization);
                x = new Recapitulation();
                x.note = s.Data().account.amortization.title;
                x.output = input;
                //double diff = output - input;
                //if (diff > 0) {
                //    x.note = "Prijenos rashoda, zatvaranje kartice";
                //    x.input = diff;
                //} else {
                //    x.note = "Prijenos viška prihoda";  // TODO ???
                //    x.output = input - output;
                //}

                x.account = GetAccount(g.amortization);
                et.data.Add(x);

                x = new Recapitulation();
                x.note = s.Data().account.correction.title;  // Ispravak vrijednosti dugotrajne imovine
                x.input = input;
                //if (diff > 0) {
                //    x.note = "Donos rashoda na ukupni prihod";
                //    x.output = diff;
                //} else {
                //    x.note = "Donos viška prihoda";  // TODO ???
                //    x.input = input - output;
                //}
                x.account = GetAccount(g.income);
                et.data.Add(x);
            }

            et.total = new Recapitulation();
            et.total.input = et.data.Sum(a => a.input);
            et.total.output = et.data.Sum(a => a.output);

            return JsonConvert.SerializeObject(et, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }
    /***** Temeljnica Bilanca *****/

    [WebMethod]
    public string LoadRecapitulation(int year, string type) {
        try {
            db.Account();
            string sql = null;
            string _sql = string.Format("SELECT a.mo, a.amount, a.recordType, a.note FROM Account a WHERE yr = '{0}'", year);
            if (type == g.repayment || type == g.withdraw || type == g.loan) {
                sql = string.Format(@"{0} AND (a.recordType = '{1}' OR a.recordType = '{2}' {3})"
                                    , _sql
                                    , g.withdraw
                                    , type
                                    , type == g.loan ? string.Format("OR a.recordType = '{0}' OR a.recordType = '{1}' OR a.recordType = '{2}'", g.repayment, g.userRepayment, g.terminationRepayment) : "");

            } else if (type == g.giroaccount) {
                sql = _sql;
            } else if (type == g.monthlyFee) {
                sql = string.Format(@"{0} AND (a.recordType = '{1}' OR a.recordType = '{2}' OR a.recordType = '{3}' OR a.recordType = '{4}')", _sql, g.monthlyFee, g.userPayment, g.terminationWithdraw, g.terminationRepayment);
            } else if (type == g.correction) {
                sql = string.Format(@"{0} AND a.recordType = '{1}'", _sql, g.amortization);
            } else {
                sql = string.Format(@"{0} AND a.recordType = '{1}'", _sql, type);
            }
            
            List<Recapitulation> xx = new List<Recapitulation>();
            RecapYearlyTotal xxx = new RecapYearlyTotal();
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            Recapitulation x = new Recapitulation();
                            x.date = null;
                            x.month = reader.GetValue(0) == DBNull.Value ? 0 : reader.GetInt32(0);
                            x.year = year;
                            x.input = reader.GetValue(1) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(1));
                            x.recordType = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
                            x.note = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
                            xx.Add(x);
                        }
                    }
                }
                connection.Close();
                xxx.year = year;
                xxx.type = type;
                xxx.account = GetAccount(type); // "TODO";  // TODO: Konto
                xxx.data = GetRecapMonthleyTotal(xx, type, year);
                xxx.total = new Recapitulation();
                xxx.total.input = GetYearlyTotal(xxx.data, "input"); // xxx.data.Sum(a => a.total.input);
                xxx.total.output = GetYearlyTotal(xxx.data, "output");  // xxx.data.Sum(a => a.total.output);
                xxx.total.accountBalance = GetYearlyTotal(xxx.data, "accountBalance");
            }
            return JsonConvert.SerializeObject(xxx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string LoadBalance(int year, string type) {
        try {
            RecapYearlyTotal xx = new RecapYearlyTotal();
            RecapMonthlyTotal x = new RecapMonthlyTotal();
            xx.data = new List<RecapMonthlyTotal>();
            //double balance = LoadBalanceSql(year, g.expense) - LoadBalanceSql(year, g.income);   // saldo
            double balance = LoadBalanceSql(year, g.income) - LoadBalanceSql(year, g.expense);   // saldo
            int nowYear = DateTime.Now.Year;

            if (type == g.income) {
                x.month = "PS";
                x.total = new Recapitulation();
                x.total.date = "01.01";
                x.total.note = "Početno stanje";
                //double lastYearBalance = LoadBalanceSql(year-1, g.expense) - LoadBalanceSql(year-1, g.income); // saldo od prethodne godine
                x.total.input = GetStartBalance(year, type) - GetTotalSaldo(year);
                //x.total.input = GetStartBalance(year, type);
                xx.data.Add(x);

                if (nowYear > year) {
                    x = new RecapMonthlyTotal();
                    x.month = "12";
                    x.total = new Recapitulation();
                    x.total.date = "31.12";
                    x.total.note = string.Format("Donos viška prihoda za {0} g.", year);
                    if (balance > 0) {
                        x.total.input = balance;
                    }
                    //x.total.output = balance;
                    //x.total.input = LoadBalanceSql(year, type);
                    xx.data.Add(x);
                }

            } else if (type == g.expense) {
                x.month = "12";
                x.total = new Recapitulation();
                x.total.date = "31.12";
                x.total.note = "Donos rashoda";
                if (balance < 0) {
                    x.total.output = balance;
                }
                xx.data.Add(x);

                if (nowYear > year) {
                    x = new RecapMonthlyTotal();
                    x.month = "12";
                    x.total = new Recapitulation();
                    x.total.date = "31.12";
                    x.total.note = string.Format("Zatvaranje kartice i prijenos na {0}", s.Data().account.income.code);  // (konto 9320)
                    if (balance < 0) {
                        x.total.input = balance;
                    }
                    xx.data.Add(x);
                }
            } else if (type == g.incomeExpenseDiff) {
                double input = LoadBalanceSql(year, g.income);
                double output = LoadBalanceSql(year, g.expense);
                x.month = "12";
                x.total = new Recapitulation();
                x.total.date = "31.12";
                x.total.note = "Donos sa kl.3 i kl.4";
                x.total.input = input;
                x.total.output = output;
                xx.data.Add(x);
                x = new RecapMonthlyTotal();
                x.month = "12";
                x.total = new Recapitulation();
                x.total.date = "31.12";
                double diff = output - input;
                if (diff > 0) {
                    x.total.note = "Prijenos rashoda";
                    x.total.input = diff;
                } else {
                    x.total.note = "Prijenos viška prihoda";
                    x.total.output = input - output;
                }

                //x.total.note = "Prijenos viška prihoda";
                //x.total.output = input - output;
                xx.data.Add(x);
            }

            xx.year = year;
            xx.type = type;
            xx.account = GetAccount(type); 
            xx.total = new Recapitulation();
            xx.total.input = GetYearlyTotal(xx.data, "input");
            xx.total.output = GetYearlyTotal(xx.data, "output");

            return JsonConvert.SerializeObject(xx, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    public double LoadBalanceSql(int year, string type) {
        double val = 0;
        db.Account();
        string sql = null;
        string _sql = string.Format("SELECT SUM(CAST(a.amount AS DECIMAL(10,2))) FROM Account a WHERE yr = '{0}'", year);
        if (type == g.income) {
            sql = string.Format(@"{0} AND (a.recordType = '{1}' OR a.recordType = '{2}')", _sql, g.interest, g.manipulativeCosts);
        } else if (type == g.expense) {
            sql = string.Format(@"{0} AND (a.recordType = '{1}' OR a.recordType = '{2}' OR a.recordType = '{3}')", _sql, g.bankFee, g.otherFee, g.amortization);
        } else {
            sql = string.Format(@"{0} AND a.recordType = '{1}'", _sql, type);
        }
          
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        val = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        return val;
    }

    [WebMethod]
    public string LoadTotal(int? year) {
        try {
            db.Account();
            string year_ = year != null && year > 0 ? string.Format("AND a.yr = {0}", year) : "";
            string sql = string.Format("SELECT SUM(CAST(a.amount AS DECIMAL(10,2))) FROM Account a WHERE (a.recordType = '{0}' OR a.recordType = '{1}') {2}"
                , g.monthlyFee
                , g.userPayment
                , year != null && year > 0 ? string.Format("AND a.yr = {0}", year) : "");
            Total x = new Total();
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.userPaymentWithMonthlyFee = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                        }
                    }
                }
                connection.Close();
            }

            sql = string.Format("SELECT SUM(CAST(loan AS DECIMAL(10,2))) FROM Loan {0}", year != null && year > 0 ? string.Format("WHERE YEAR(CONVERT(DATETIME, loanDate)) = {0}", year) : "");
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.activatedLoan = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                        }
                    }
                }
                connection.Close();
            }

            x.bankFee = GetTotalVal(g.bankFee, year);
            x.interest = GetTotalVal(g.interest, year);
            x.otherFee = GetTotalVal(g.otherFee, year);
            x.manipulativeCosts = GetTotalVal(g.manipulativeCosts, year);

            sql = string.Format("SELECT amount, mo, yr, recordType FROM Account {0}", year != null && year > 0 ? string.Format("WHERE yr = {0}", year) : "");
            List<Recapitulation> rr = new List<Recapitulation>();
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            Recapitulation r = new Recapitulation();
                            r.input = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(0));
                            r.month = reader.GetValue(1) == DBNull.Value ? 0 : Convert.ToInt32(reader.GetInt32(1));
                            r.year = reader.GetValue(2) == DBNull.Value ? 0 : Convert.ToInt32(reader.GetInt32(2));
                            r.recordType = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
                            rr.Add(r);
                        }
                    }
                }
                connection.Close();
            }
            x.monthlyTotalList = new List<RecapMonthlyTotal>();
            for (int i = 1; i <= 12; i++) {
                RecapMonthlyTotal rmt = new RecapMonthlyTotal();
                rmt.month = i.ToString();
                rmt.total = new Recapitulation();
                rmt.total.input = rr.Where(a => a.month == i && a.year == year
                                                           && (a.recordType == g.manipulativeCosts
                                                           || a.recordType == g.interest)).Sum(a => a.input);
                rmt.total.output = rr.Where(a => a.month == i && a.year == year
                                                            && (a.recordType == g.bankFee
                                                            || a.recordType == g.otherFee)).Sum(a => a.input);
                x.monthlyTotalList.Add(rmt);
            }
            x.input = x.monthlyTotalList.Sum(a => a.total.input);
            x.output = x.monthlyTotalList.Sum(a => a.total.output);
            return JsonConvert.SerializeObject(x, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    public class MonthlyPayment {
        public Loan.Loans loans;
        public List<Fee> terminationWithdraw_;  //TODO: napraviti listu (dodati ulog, duguje, za isplatu)
        public TerminationWithdraw terminationWithdraw;
        public List<Fee> otherFee;
        public List<Fee> bankFee;
        public double total;
    }

    public class TerminationWithdraw {
        public List<TermWithdraw> data;
        public TermWithdraw total;
    }

    public class Fee {
        public string title;
        public double val;
    }


    public class TermWithdraw {
        public User.NewUser user;
        public double userPayment;
        public double debt;
        public double terminationWithdraw;
    }

    [WebMethod]
    public string LoadMonthlyPayment(int? month, int year) {
        try {
            MonthlyPayment x = new MonthlyPayment();
            Loan l = new Loan();
            x.loans = l.LoadData(month, year, null, null);
            x.terminationWithdraw = GetTerminationWithdraw(month, year);
            x.bankFee = GetMonthlyPayment(month, year, g.bankFee);
            x.otherFee = GetMonthlyPayment(month, year, g.otherFee);
            x.total = x.loans.total.withdraw + x.terminationWithdraw.data.Sum(a => a.terminationWithdraw) + x.bankFee.Sum(a => a.val) + x.otherFee.Sum(a => a.val);
            return JsonConvert.SerializeObject(x, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    public List<Fee> GetMonthlyPayment(int? month, int year, string recordType) {
        List<Fee> xx = new List<Fee>();
        string sql, uid, firstName, lastName, note = null;
        if (recordType == g.terminationWithdraw) {
            sql = string.Format(@"SELECT u.id, u.firstName, u.lastName, a.note, a.amount FROM Account a
                                LEFT OUTER JOIN Users u ON a.userId = u.id
                                WHERE a.mo = '{0}' AND a.yr = '{1}' AND a.recordType = '{2}'", month, year, recordType);
        } else {
            sql = string.Format(@"SELECT amount, note FROM Account WHERE mo = '{0}' AND yr = '{1}' AND recordType = '{2}'", month, year, recordType);
        }
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        Fee x = new Fee();
                        if (recordType == g.terminationWithdraw) {
                            x.val = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
                            uid = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                            firstName = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
                            lastName = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
                            note = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
                            x.title = string.Format("{0} - {1} {2}", uid, lastName, firstName);
                        } else {
                            x.val = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(0));
                            x.title = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
                        }
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public TerminationWithdraw GetTerminationWithdraw(int? month, int year) {
        TerminationWithdraw xx = new TerminationWithdraw();
        xx.data = new List<TermWithdraw>();
        string sql = string.Format(@"SELECT u.id, u.firstName, u.lastName, a.amount FROM Account a
                                LEFT OUTER JOIN Users u ON a.userId = u.id
                                WHERE a.mo = '{0}' AND a.yr = '{1}' AND a.recordType = '{2}'"
                                , month, year, g.terminationWithdraw);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        TermWithdraw x = new TermWithdraw();
                        x.user = new User.NewUser();
                        x.user.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                        x.user.firstName = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
                        x.user.lastName = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
                        x.terminationWithdraw = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                        xx.data.Add(x);
                    }
                }
            }
            connection.Close();

            foreach (var t in xx.data) {
                t.userPayment = GetUserPaymentTotal(t.user.id);
                t.debt = t.userPayment - t.terminationWithdraw;
            }

            xx.total = new TermWithdraw();
            xx.total.userPayment = xx.data.Sum(a => a.userPayment);
            xx.total.debt = xx.data.Sum(a => a.debt);
            xx.total.terminationWithdraw = xx.data.Sum(a => a.terminationWithdraw);
        }
        return xx;
    }

    private double GetUserPaymentTotal(string id) {
        double x = 0;
        string sql = string.Format(@"SELECT SUM(CAST(a.amount AS DECIMAL(10,2))) FROM Account a WHERE a.userId = '{0}' AND (a.recordType = '{1}' OR a.recordType = '{2}' OR a.recordType = '{3}')"
                                , id , g.monthlyFee, g.userPayment, g.userRepayment);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
            return x;
        }
    }

    private double GetTotalVal(string type, int? year) {
        double x = 0;
        string sql = string.Format("SELECT SUM(CAST(a.amount AS DECIMAL(10,2))) FROM Account a WHERE a.recordType = '{0}' {1}"
            , type
            , year != null && year > 0 ? string.Format("AND a.yr = {0}", year) : "");
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                        }
                    }
                }
                connection.Close();
            }
        return x;
    }

    private CodeTitle GetAccount(string type) {
        CodeTitle x = new CodeTitle();
        if (type == g.giroaccount) {
            x = s.Data().account.giroAccount;
        }
        if (type == g.loan) {
            x = s.Data().account.loan;
        }
        if (type == g.monthlyFee) {
            x = s.Data().account.monthlyFee;
        }
        if (type == g.manipulativeCosts) {
            x = s.Data().account.manipulativeCosts;
        }
        if (type == g.bankFee) {
            x = s.Data().account.bankFee;
        }
        if (type == g.interest) {
            x = s.Data().account.interest;
        }
        if (type == g.otherFee) {
            x = s.Data().account.otherFee;
        }
        if (type == g.income) {
            x = s.Data().account.income;
        }
        if (type == g.expense) {
            x = s.Data().account.expense;
        }
        if (type == g.incomeExpenseDiff) {
            x = s.Data().account.incomeExpenseDiff;
        }
        if (type == g.amortization) {
            x = s.Data().account.amortization;
        }
        if (type == g.correction) {
            x = s.Data().account.correction;
        }
        return x;
    }

    private List<RecapMonthlyTotal> GetRecapMonthleyTotal(List<Recapitulation> data, string type, int year) {
        string inputType = null, outputType = null;
        double startBalance = 0;
        if (type == g.loan) {
            inputType = g.repayment;
            outputType = type;
        } else if (type == g.repayment) {
            inputType = g.repayment;
            outputType = g.withdraw;
        } else if (type == g.bankFee || type == g.otherFee || type == g.amortization) {
            inputType = null;
            outputType = type;
        } else if (type == g.giroaccount) {
            inputType = type;
            outputType = type;
        } else if (type == g.monthlyFee) {
            inputType = type;
            outputType = g.terminationWithdraw;
        } else if (type == g.correction) {
            inputType = g.amortization;
            outputType = null;
        }
        else {
            inputType = type;
            outputType = null;
        }
        List<RecapMonthlyTotal> xx = new List<RecapMonthlyTotal>();

        if (type == g.loan || type == g.repayment || type == g.monthlyFee || type == g.giroaccount || type == g.loan || type == g.correction || type == g.softwareInvestment) {
            RecapMonthlyTotal x = new RecapMonthlyTotal();
            x.month = "PS";
            x.total = new Recapitulation();
            x.total.date = "01.01";
            x.total.note = "Početno stanje";
            startBalance = GetStartBalance(year, type);
            if (type == g.loan) {
                startBalance = GetLoanStartBalance(null, year);
                x.total.output = startBalance;
            }
            if (type == g.monthlyFee || type == g.correction) {
                x.total.input = startBalance;  // potražuje 
            }
            if (type == g.giroaccount && year >= Convert.ToDateTime(s.Data().startBalance.date).Year) {
                x.total.output = g.GetYear(s.Data().startBalance.date) == year.ToString() ? 0 : startBalance;
                x.total.input = 0;
               // x.total.output = s.Data().startBalance.giroAccountOutput;  // TODO: + ukupno ziro racun do te godine, metoda: getGiroAccountOutputTotal(year)
               // x.total.input = s.Data().startBalance.giroAccountInput; // TODO: + ukupno ziro racun do te godine, metoda: getGiroAccountInputTotal(year)
            }
            if (type == g.softwareInvestment) {
                x.total.output = startBalance;
            }
            xx.Add(x);
        }

        for (int i = 1; i <= 12; i++) {
            RecapMonthlyTotal x = new RecapMonthlyTotal();
            x.month = i.ToString();
            x.total = new Recapitulation();
            x.total.date = g.SetDayMonthDate(g.GetLastDayInMonth(year, i), i);

            // Dugovna strana
            if (!string.IsNullOrEmpty(inputType)) {
                List<Recapitulation> input = new List<Recapitulation>();
                if (inputType == g.giroaccount) {
                     input = data.Where(a => a.month.ToString() == x.month && a.year == year && g.DateDiff(g.SetDate(g.GetLastDayInMonth(year, Convert.ToInt32(a.month)), Convert.ToInt32(a.month), year), s.Data().startBalance.date, false) > 0
                                                                    && (a.recordType == g.withdraw
                                                                    || a.recordType == g.bankFee
                                                                    || a.recordType == g.otherFee
                                                                    || a.recordType == g.terminationWithdraw)).ToList();
                } else if (inputType == g.monthlyFee) {
                    input = data.Where(a => a.month.ToString() == x.month && a.year == year && (a.recordType == inputType || a.recordType == g.userPayment)).ToList();
                } else if (inputType == g.repayment) {
                    input = data.Where(a => a.month.ToString() == x.month && a.year == year && (a.recordType == inputType || a.recordType == g.loan || a.recordType == g.userRepayment || a.recordType == g.terminationRepayment)).ToList();
                    //input = data.Where(a => a.month.ToString() == x.month && a.year == year && (a.recordType == inputType || a.recordType == g.loan || a.recordType == g.userRepayment)).ToList();
                } else {
                     input = data.Where(a => a.month.ToString() == x.month && a.year == year && a.recordType == inputType).ToList();
                }

                x.total.input = input.Sum(a => a.input);

                List<Recapitulation> inputAccumulation = new List<Recapitulation>();
                if (inputType == g.giroaccount) {
                    inputAccumulation = data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && g.DateDiff(g.SetDate(g.GetLastDayInMonth(year, Convert.ToInt32(a.month)), Convert.ToInt32(a.month), year), s.Data().startBalance.date, false) > 0
                                                                    && (a.recordType == g.withdraw
                                                                    || a.recordType == g.bankFee
                                                                    || a.recordType == g.otherFee
                                                                    || a.recordType == g.terminationWithdraw)).ToList();
                } else if (inputType == g.monthlyFee) {
                    inputAccumulation = data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && (a.recordType == inputType || a.recordType == g.userPayment)).ToList();
                } else if (inputType == g.repayment) {
                    inputAccumulation = data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && (a.recordType == inputType || a.recordType == g.loan || a.recordType == g.userRepayment || a.recordType == g.terminationRepayment)).ToList();
                    //inputAccumulation = data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && (a.recordType == inputType || a.recordType == g.loan || a.recordType == g.userRepayment)).ToList();
                } else {
                    inputAccumulation = data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && a.recordType == inputType).ToList();
                }

                if (inputType == g.giroaccount && g.DateDiff(g.SetDate(g.GetLastDayInMonth(year, Convert.ToInt32(x.month)), Convert.ToInt32(x.month), year), s.Data().startBalance.date, false) >= 0) {
                    x.total.inputAccumulation = inputAccumulation.Sum(a => a.input) + (g.GetYear(s.Data().startBalance.date) == year.ToString() ? s.Data().startBalance.giroAccountInput : 0);
                } else if (inputType == g.giroaccount && g.DateDiff(g.SetDate(g.GetLastDayInMonth(year, Convert.ToInt32(x.month)), Convert.ToInt32(x.month), year), s.Data().startBalance.date, false) < 0) {
                    x.total.inputAccumulation = 0;
                } else if (inputType == g.repayment) {
                    x.total.inputAccumulation = inputAccumulation.Sum(a => a.input);
                } else {
                    x.total.inputAccumulation = inputAccumulation.Sum(a => a.input) + startBalance;
                }
            }

            // Potrazna strana
            if (!string.IsNullOrEmpty(outputType)) {
                if (outputType == g.loan) {
                    x.total.output = GetActivatedLoan(Convert.ToInt32(x.month), year, null);
                    x.total.outputAccumulation = GetActivatedLoanAccu(Convert.ToInt32(x.month), year, null) + startBalance;
                } else {
                    List<Recapitulation> output = new List<Recapitulation>();
                    if (outputType == g.giroaccount) {
                        output = data.Where(a => a.month.ToString() == x.month && a.year == year && g.DateDiff(g.SetDate(g.GetLastDayInMonth(year, Convert.ToInt32(a.month)), Convert.ToInt32(a.month), year), s.Data().startBalance.date, false) > 0
                                                                       && (a.recordType == g.repayment
                                                                        || a.recordType == g.userRepayment
                                                                        || a.recordType == g.monthlyFee
                                                                        || a.recordType == g.userPayment
                                                                        || a.recordType == g.interest)).ToList();
                    } else if (outputType == g.monthlyFee) {
                        output = data.Where(a => a.month.ToString() == x.month && a.year == year && (a.recordType == outputType || a.recordType == g.userPayment)).ToList();
                    } else if (outputType == g.terminationWithdraw) {
                        output = data.Where(a => a.month.ToString() == x.month && a.year == year && (a.recordType == outputType || a.recordType == g.userPayment || a.recordType == g.terminationRepayment)).ToList();
                    } else {
                        output = data.Where(a => a.month.ToString() == x.month && a.year == year && a.recordType == outputType).ToList();
                    }

                    x.total.output = output.Sum(a => a.input);

                    List<Recapitulation> outputAccumulation = new List<Recapitulation>();
                    if (outputType == g.giroaccount) {
                        outputAccumulation = data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && g.DateDiff(g.SetDate(g.GetLastDayInMonth(year, Convert.ToInt32(a.month)), Convert.ToInt32(a.month), year), s.Data().startBalance.date, false) > 0
                                                                        && (a.recordType == g.repayment
                                                                        || a.recordType == g.userRepayment
                                                                        || a.recordType == g.monthlyFee
                                                                        || a.recordType == g.userPayment
                                                                        || a.recordType == g.interest)).ToList();
                    } else if (outputType == g.monthlyFee) {
                        outputAccumulation = data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && (a.recordType == outputType || a.recordType == g.userPayment)).ToList();
                    } else if (outputType == g.terminationWithdraw) {
                        outputAccumulation = data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && (a.recordType == outputType || a.recordType == g.userPayment || a.recordType == g.terminationRepayment)).ToList();
                    } else {
                        outputAccumulation = data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && a.recordType == outputType).ToList();
                    }

                    if (outputType == g.giroaccount && g.DateDiff(g.SetDate(g.GetLastDayInMonth(year, Convert.ToInt32(x.month)), Convert.ToInt32(x.month), year), s.Data().startBalance.date, false) >= 0) {
                        x.total.outputAccumulation = outputAccumulation.Sum(a => a.input) + (g.GetYear(s.Data().startBalance.date) == year.ToString() ? s.Data().startBalance.giroAccountOutput : startBalance);
                    } else {
                        x.total.outputAccumulation = outputAccumulation.Sum(a => a.input);
                    }

                }
            }

            if (x.total.input > 0 || x.total.output > 0 || x.total.inputAccumulation > 0 || x.total.outputAccumulation > 0) {
                if (type == g.monthlyFee.ToString()) {
                    //x.total.output = GetMonthlyFeeRequired(null, i, year);
                    //x.total.outputAccumulation = GetMonthlyFeeRequiredAccu(null, i, year);
                }
                if (type == g.loan) {
                    x.total.note = string.Format("Promet pozajmica {0}/{1}", g.Month(i), year);
                } else if (type == g.monthlyFee) {
                    x.total.note = string.Format("Promet uloga {0}/{1}", g.Month(i), year);
                } else if (type == g.manipulativeCosts) {
                    x.total.note = string.Format("{0}% Manipulativni troškovi {1}/{2}", g.manipulativeCostsPerc(), g.Month(i), year);
                } else if (type == g.giroaccount) {
                    x.total.note = string.Format("Promet Žiro-Računa {0}/{1}", g.Month(i), year);
                    //x.total.accountBalance = x.total.outputAccumulation - x.total.inputAccumulation;
                } else if (type == g.otherFee) {
                    x.total.note = "Razni materijalni troškovi";
                } else if (type == g.amortization) {
                    x.total.note = s.Data().account.amortization.title;
                } else if (type == g.correction) {
                    x.total.note = s.Data().account.correction.title;
                } else {
                    x.total.note = null; // data.Where(a => a.month.ToString() == i.ToString()).FirstOrDefault().note;
                }
                x.total.accountBalance = Math.Abs(x.total.outputAccumulation - x.total.inputAccumulation);
                x.month = g.Month(i);
                xx.Add(x);
            }
        }
        return xx;
    }

    public double GetMonthlyFeeRequired(string userId, int? month, int year) {
        double x = 0;
        int month_ = month == null ? 1: Convert.ToInt32(month);
        string sql = string.Format(@"SELECT SUM(CAST(u.monthlyFee AS DECIMAL(10,2)))
                                    FROM Users u WHERE CONVERT(DATETIME, u.accessDate) <= CONVERT(DATETIME, '{0}') {1}"
                                , g.SetDate(g.GetLastDayInMonth(year, month_), month_, year)
                                , !string.IsNullOrEmpty(userId) ? string.Format("AND u.id = '{0}'", userId) : "");

        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    public double GetMonthlyFeeRequiredAccu(string userId, int month, int year) {
        double x = 0;
        string sql = string.Format(@"SELECT SUM(CONVERT(DECIMAL,(DATEDIFF(month, CONVERT(DATETIME, u.accessDate), CONVERT(DATETIME, '{0}')) * u.monthlyFee)))
                                    FROM Users u WHERE CONVERT(DATETIME, u.accessDate) <= CONVERT(DATETIME, '{0}') {1}"
                                , g.SetDate(g.GetLastDayInMonth(year, month), month, year)
                                , !string.IsNullOrEmpty(userId) ? string.Format("AND u.id = '{0}'", userId) : "");
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    private double GetStartBalance(int year, string type) {
        double x = 0;
        string sql = null;
        string _sql = string.Format(@"SELECT SUM(CAST(a.amount AS DECIMAL(10,2))) FROM Account a WHERE a.yr < {0}", year);
        if (type == g.monthlyFee) {
            sql = string.Format("{0} AND a.recordType = '{1}' OR a.recordType = '{2}'", _sql, type, g.userPayment);
        } else if (type == g.income) {
            sql = string.Format("{0} AND (a.recordType = '{1}' OR a.recordType = '{2}')", _sql, g.interest, g.manipulativeCosts);
        } else if (type == g.expense) {
            sql = string.Format("{0} AND (a.recordType = '{1}' OR a.recordType = '{2}')", _sql, g.bankFee, g.otherFee);
        } else if(type == g.giroaccount) {
            sql = string.Format(@"{0} AND CAST(a.recordDate AS DATETIME) > '{1}' AND 
                                (a.recordType = '{2}' OR a.recordType = '{3}' OR a.recordType = '{4}'
                                OR  a.recordType = '{5}' OR a.recordType = '{6}')"
                                , _sql, s.Data().startBalance.date, g.repayment, g.userRepayment, g.monthlyFee, g.userPayment, g.interest);
        } else if (type == g.correction) {
            sql = string.Format("{0} AND a.recordType = '{1}'", _sql, g.amortization);
        } else if (type == g.softwareInvestment) {
            sql = string.Format("{0} AND a.recordType = '{1}'", _sql, g.softwareInvestment);
        } else {
            sql = _sql;
        }
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        if (type == g.income) {
            x = s.Data().startBalance1.income;
        } else if (type == g.expense) {
            x = s.Data().startBalance1.expense;
        } else if (type == g.monthlyFee) {
            double terminationWithdraw = 0;
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                sql = string.Format("{0} AND a.recordType = '{1}'", _sql, g.terminationWithdraw);
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            terminationWithdraw = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                        }
                    }
                }
                connection.Close();
            }
            if (year.ToString() == g.GetYear(s.Data().startBalance.date)) {
                x = 0;  // samo pocetna godina (2019)
            } else {
                x = x - terminationWithdraw;
            }
            //x = x - terminationWithdraw;
        } else if (type == g.giroaccount) {
            double expense = 0;
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                sql = string.Format(@"{0} AND CAST(a.recordDate AS DATETIME) > '{1}' AND 
                                (a.recordType = '{2}' OR a.recordType = '{3}' OR a.recordType = '{4}' OR  a.recordType = '{5}')"
                                , _sql, s.Data().startBalance.date, g.withdraw, g.bankFee, g.otherFee, g.terminationWithdraw);
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            expense = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                        }
                    }
                }
                connection.Close();
            }
            if (year.ToString() == g.GetYear(s.Data().startBalance.date)) {
                x = s.Data().startBalance.giroAccountOutput - s.Data().startBalance.giroAccountInput;  // samo pocetna godina (2019)
            } else {
                x = (x + s.Data().startBalance.giroAccountOutput) - (expense + s.Data().startBalance.giroAccountInput);
            }
        }
        return x;
    }

    private double GetTotalSaldo(int year) {
        double incomeTotal = 0;
        double expenseTotal = 0;
        double x = 0;
        string sql = null;
        string sql_ = string.Format("SELECT SUM(CAST(a.amount AS DECIMAL(10,2))) FROM Account a WHERE a.yr < {0}", year);

        sql = string.Format("{0} {1}"
            , sql_
            , string.Format("AND (a.recordType = '{0}' OR a.recordType = '{1}')", g.interest, g.manipulativeCosts));
        //sql = string.Format("{0} {1}"
        //    , sql_
        //    , string.Format("AND (a.recordType = '{0}' OR a.recordType = '{1}' OR a.recordType = '{2}')", g.interest, g.manipulativeCosts, g.amortization));
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        incomeTotal = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }

        //sql = string.Format("{0} {1}"
        //    , sql_
        //    , string.Format("AND (a.recordType = '{0}' OR a.recordType = '{1}')", g.bankFee, g.otherFee));
        sql = string.Format("{0} {1}"
            , sql_
            , string.Format("AND (a.recordType = '{0}' OR a.recordType = '{1}' OR a.recordType = '{2}')", g.bankFee, g.otherFee, g.amortization));
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        expenseTotal = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        x = expenseTotal - incomeTotal;
        return x;
    }

    private double GetYearlyTotal(List<RecapMonthlyTotal> data, string type) {
        double x = 0;
        foreach(var i in data) {
            if (type == "input") {
                x += i.total.input;
            } else if (type == "output") {
                x += i.total.output;
            } else if (type == "accountBalance") {
                //x += i.total.accountBalance;
                x += i.total.output - i.total.input;
            } else {
                x = 0;
            }
        }
        return Math.Abs(x);
    }

    public NewAccount Save(NewAccount x) {
        try {
            db.Account();
            bool isNewRecord = false;
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                isNewRecord = true;
            }
            //  double lastRepayment = GetRecord(x.user.id, x.month, x.year).repayment;  // ***** in case of update repaiment  *****

            string sql = null;

            if (x.amount == 0 && !isNewRecord) {
                sql = string.Format(@"BEGIN TRAN
                                            IF EXISTS (SELECT * from Account WITH (updlock,serializable) WHERE id = '{0}')
                                                BEGIN
                                                   DELETE FROM Account WHERE id = '{0}' AND userId = '{1}'
                                                END
                                        COMMIT TRAN", x.id, x.user.id);
            } else if (x.amount == 0 && isNewRecord) {
                // ***** DO NOTHING !!! (Ne spremaj vrijednost 0) *****
            } else {
                sql = string.Format(@"BEGIN TRAN
                                            IF EXISTS (SELECT * from Account WITH (updlock,serializable) WHERE id = '{0}')
                                                BEGIN
                                                   UPDATE Account SET userId = '{1}', amount = '{2}', recordDate = '{3}', mo = '{4}', yr = '{5}', recordType = '{6}', loanId = '{7}', note = N'{8}' WHERE id = '{0}'
                                                END
                                            ELSE
                                                BEGIN
                                                   INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                                   VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', N'{8}')
                                                END
                                        COMMIT TRAN", x.id, x.user.id, x.amount, x.recordDate, x.month, x.year, x.recordType, x.loanId, x.note);
            }

            if (!string.IsNullOrEmpty(sql)) {
                using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }

            double loan = x.loan;
            if (x.loan == 0) {
                loan = GetLoanAmount(x.loanId);
            }

            //x.restToRepayment = loan - Repaid(x);
            User U = new User();
            x.restToRepayment = U.GetLoanAmount(x.user.id) - U.GetAmount(x.user.id, g.repayment) - U.GetAmount(x.user.id, g.userRepayment) - U.GetAmount(x.user.id, g.terminationRepayment) - U.GetAmount(x.user.id, g.loan);
            SetLoanIsRepaid(x);

            return x;
        } catch (Exception e) {
            return new NewAccount();
        }
    }


    private double GetLoanAmount(string id) {
        double x = 0;
        string sql = string.Format("SELECT loan FROM Loan WHERE id = '{0}'", id);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(0));
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    private void SetLoanIsRepaid(NewAccount x) {
        int isRepaid = x.restToRepayment == 0 ? 1 : 0;
        string sql = string.Format(@"BEGIN TRAN
                                BEGIN
                                    UPDATE Loan SET isRepaid = {0} WHERE id = '{1}'
                                END
                            COMMIT TRAN", isRepaid, x.loanId);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    NewAccount ReadData(SqlDataReader reader) {
        NewAccount x = new NewAccount();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.user = new User.NewUser();
        x.user.id = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.amount = reader.GetValue(2) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(2));
        x.recordDate = reader.GetValue(3) == DBNull.Value ? null :reader.GetString(3);
        x.month = reader.GetValue(4) == DBNull.Value ? "01" : g.Month(reader.GetInt32(4));
        x.year = reader.GetValue(5) == DBNull.Value ? DateTime.Now.Year :reader.GetInt32(5);
        x.recordType = reader.GetValue(6) == DBNull.Value ? null : reader.GetString(6);
        x.loanId = reader.GetValue(7) == DBNull.Value ? null : reader.GetString(7);
        x.note = reader.GetValue(8) == DBNull.Value ? null : reader.GetString(8);
        x.lastDayInMonth = g.LastDayInMonth(x.year, Convert.ToInt32(x.month));
        return x;
    }
 
    public NewAccount GetRecord(string userId, int month, int year, string recordType) {
        string sql = string.Format(@"SELECT * FROM Account WHERE userId = '{0}' AND mo = '{1}' AND yr = '{2}' {3}"
                                , userId
                                , month
                                , year
                                , !string.IsNullOrEmpty(recordType) ? string.Format("AND recordType = '{0}'", recordType) : "");
        NewAccount x = new NewAccount();
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = ReadData(reader);
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    public List<NewAccount> GetRecords(string userId, int month, int year, string recordType) {
        string sql = string.Format(@"SELECT * FROM Account WHERE userId = '{0}' AND mo = '{1}' AND yr = '{2}' {3}"
                                , userId
                                , month
                                , year
                                , !string.IsNullOrEmpty(recordType) ? string.Format("AND recordType = '{0}'", recordType) : "");
        List<NewAccount> xx = new List<NewAccount>();
        NewAccount x = new NewAccount();
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = ReadData(reader);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public List<NewAccount> GetRecords(string userId, int? year) {
        db.Account();
        db.Loan();
        List<NewAccount> xx = new List<NewAccount>();
        NewAccount x = new NewAccount();
        x.lastDayInMonth = "01.01";
        x.month = "PS";
        x.note = "Početno stanje";
        x.monthlyFeeStartBalance = GetMonthlyFeeStartBalance(userId, year);
        x.loanStartBalance = GetLoanStartBalance(userId, year); 
        xx.Add(x);
        string sql = string.Format("SELECT * FROM Account WHERE userId = '{0}' AND recordType <> '{1}' {2}"
                                  , userId
                                  , g.manipulativeCosts
                                  , year > 0 ? string.Format("AND yr = '{0}' ORDER BY mo ASC", year) : "");
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = new NewAccount();
                        x = ReadData(reader);
                        x = CheckLoan(x, userId);
                        x.activatedLoan = GetActivatedLoan(Convert.ToInt32(x.month), x.year, userId);
                        x = CheckMonthlyFee(x, userId, g.monthlyFee);
                        x.userPayment = GetUserPayment(x, userId, g.userPayment);
                        x.userPaymentTotal = x.userPayment.Sum(a => a.amount);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public NewAccount CheckLoan(NewAccount x, string userId) {
        User U = new User();
        string sql = string.Format(@"
                    SELECT l.id, l.loan, l.repayment FROM Loan l
                    WHERE l.userId = '{0}' AND (CAST(l.loanDate AS datetime) <= CAST('{1}-{2}-{3}' AS datetime)) {4} AND l.isRepaid = 0"
                    , userId
                    , x.year
                    , g.Month(Convert.ToInt32(x.month))
                    , g.GetLastDayInMonth(x.year, Convert.ToInt32(x.month))
                    , !string.IsNullOrEmpty(x.loanId) ? string.Format(" AND l.id = '{0}'", x.loanId) : "");
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x.loanId = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                        x.loan = reader.GetValue(1) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(1));
                        x.repayment = reader.GetValue(2) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(2));
                        x.repaid = Repaid(x);
                        //x.restToRepayment = x.loan - x.repaid;
                        x.restToRepayment = U.GetLoanAmount(userId) - U.GetAmount(userId, g.repayment) - U.GetAmount(userId, g.userRepayment) - U.GetAmount(userId, g.terminationRepayment) - U.GetAmount(userId, g.loan);
                        if (x.restToRepayment > 0 && x.restToRepayment < x.repayment && x.repayment > 0) {
                            x.repayment = x.restToRepayment;
                        }
                        x.totalObligation = x.monthlyFee + x.repayment;
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    public NewAccount CheckLastRepayment(NewAccount x, string userId) {
        double val = 0;
        string sql = string.Format(@"
                       SELECT a.amount FROM Account a WHERE a.userId = '{1}' AND a.mo = {1} AND a.yr = {2} AND recordType = '{3}'"
                    , userId , x.month , x.year, g.repayment);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        val = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(0));
                    }
                }
            }
            connection.Close();
        }
        if (val > 0) {
            x.repayment = val;
        }
        return x;
    }

    public double GetActivatedLoan(int month, int year, string userId) {
        double loan = 0;
        string sql = string.Format(@"
                    SELECT SUM(CAST(l.loan AS decimal)) FROM Loan l
                    {0} ((CAST(l.loanDate AS datetime) >= CAST('{1}-{2}-01' AS datetime)) AND (CAST(l.loanDate AS datetime) <= CAST('{1}-{2}-{3}' AS datetime)))"
                     , string.IsNullOrEmpty(userId) ? "WHERE" : string.Format("WHERE l.userId = '{0}' AND", userId)
                     , year
                     , g.Month(month)
                     , g.GetLastDayInMonth(year, month));
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        loan = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        return loan;
    }

    public double GetActivatedLoanAccu(int month, int year, string userId) {
        double loan = 0;
        string sql = string.Format(@"
                    SELECT SUM(CAST(l.loan AS decimal)) FROM Loan l
                    {0} ((CAST(l.loanDate AS datetime) <= CAST('{1}-{2}-{3}' AS datetime) AND CAST(l.loanDate AS datetime) >= CAST('{1}-01-01' AS datetime)))"
                       , string.IsNullOrEmpty(userId) ? "WHERE" : string.Format("WHERE l.userId = '{0}' AND", userId)
                       , year
                       , g.Month(month)
                       , g.GetLastDayInMonth(year, month));
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        loan = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        return loan;
    }

    public NewAccount CheckMonthlyFee(NewAccount x, string userId, string recordType) {
        string sql = string.Format(@"
                    SELECT a.userId, a.amount, u.monthlyFee FROM Account a
                    LEFT OUTER JOIN Users u
                    ON a.userId = u.id
                    WHERE a.userId = '{0}' AND a.mo = '{1}' AND a.yr = '{2}' AND a.recordType = '{3}'"
                       , userId
                       , x.month
                       , x.year
                       , g.monthlyFee);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x.user.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                        if(recordType == g.monthlyFee) {
                            x.monthlyFee = reader.GetValue(1) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(1));
                            x.user.monthlyFee = x.monthlyFee;
                        }
                        if (recordType == g.terminationWithdraw) {
                            x.amount = reader.GetValue(1) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(1));
                        }
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    public List<UserPayment> GetUserPayment(NewAccount x, string userId, string type) {
        string sql = string.Format(@"
                    SELECT a.id, a.recordDate, a.amount, a.note, a.recordType FROM Account a
                    WHERE a.userId = '{0}' AND a.mo = '{1}' AND a.yr = '{2}' AND a.recordType = '{3}'"
                    , userId
                    , x.month
                    , x.year
                    , type);
        List<UserPayment> xx = new List<UserPayment>();
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        UserPayment up = new UserPayment();
                        up.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                        up.recordDate = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
                        up.amount = reader.GetValue(2) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(2));
                        up.note = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
                        up.type = reader.GetValue(4) == DBNull.Value ? null : reader.GetString(4);
                        xx.Add(up);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public double Repaid(NewAccount x) {
        double repaid = 0;
        string sql = string.Format(@"
                    SELECT SUM(CAST(a.amount AS DECIMAL(10,2))) FROM Account a
                    WHERE a.userId = '{0}' AND (a.recordType = '{1}' OR a.recordType = '{2}')  AND (CAST(CONCAT(a.yr, '-', a.mo, '-', '01') AS datetime) <= CAST('{3}-{4}-01' AS datetime)) AND a.loanId = '{5}'"
                       , x.user.id, g.repayment, g.userRepayment, x.year, g.Month(Convert.ToInt32(x.month)), x.loanId);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        repaid = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        return repaid;
    }

    public double GetMonthlyFeeStartBalance(string userId, int? year) {
        double x = 0;
        string sql = string.Format(@"SELECT SUM(CAST(a.amount AS DECIMAL(10,2))) FROM Account a
                                    WHERE a.userId = '{0}' AND (a.recordType = '{1}' OR a.recordType = '{2}') AND (CAST(CONCAT(a.yr, '-', a.mo, '-', '01') AS datetime) < CAST('{3}-01-01' AS datetime))"
                                    , userId, g.monthlyFee, g.userPayment, year);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        x = x - GetTerminationWithdraw(userId, year);
        return x;
    }

    private double GetTerminationWithdraw(string userId, int? year) {
        double x = 0;
        string sql = string.Format(@"SELECT SUM(CAST(a.amount AS DECIMAL(10,2))) FROM Account a
                                    WHERE a.userId = '{0}' AND a.recordType = '{1}' AND (CAST(CONCAT(a.yr, '-', a.mo, '-', '01') AS datetime) < CAST('{2}-01-01' AS datetime))"
                                   , userId, g.terminationWithdraw, year);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    public double GetLoanStartBalance(string userId, int? year) {
        double x = 0;
        double repayed = 0;
        double loan = 0;
        double startLoan = 0; // ukupno pozajmica na dan 30.09.2019 
        string sql = string.Format(@"
                    SELECT SUM(CAST(a.amount AS DECIMAL(10,2))) FROM Account a
                    {0} (a.recordType = '{1}' OR a.recordType = '{2}' OR a.recordType = '{3}') AND (CAST(CONCAT(a.yr, '-', a.mo, '-', '01') AS datetime) < CAST('{4}-01-01' AS datetime))"
                       , string.IsNullOrEmpty(userId) ? "WHERE" : string.Format("WHERE a.userId = '{0}' AND", userId)
                       , g.repayment
                       , g.userRepayment
                       , g.loan
                       , year);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        repayed = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        sql = string.Format(@"
                SELECT SUM(CAST(l.loan AS DECIMAL(10,2))) FROM Loan l
                {0} (CAST(l.loanDate AS datetime) < CAST('{1}-01-01' AS datetime))"
                    , string.IsNullOrEmpty(userId) ? "WHERE" : string.Format("WHERE l.userId = '{0}' AND", userId)
                    , year);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        loan = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }

        startLoan = GetStartLoan(userId);
        if (year.ToString() == g.GetYear(s.Data().startBalance.date)) {
            if (string.IsNullOrEmpty(userId)) {
                x = 0;  // Samo pocetna godina (2019) ???
            } else {
                x = loan - repayed + startLoan;
            }
            //x = 0; // loan - repayed + startLoan;  // Samo pocetna godina (2019) ???
        } else {
            x = loan - repayed;
        }
        return x;
    }

    private double GetStartLoan(string userId) {
        double x = 0;
        string sql = string.Format(@"SELECT SUM(CAST(l.loan AS DECIMAL(10,2))) FROM Loan l WHERE l.loanDate = '{0}' {1}"
                                , s.Data().startBalance.date
                                , string.IsNullOrEmpty(userId) ? "" : string.Format("AND l.userId = '{0}'", userId));
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    //private double GetLoanBalance(string userId) {
    //    double x = 0;
    //    string sql = string.Format(@"SELECT l.loan FROM Loan l WHERE l.loanDate = '{0}' AND l.userId = '{1}'", s.Data().startBalance.date, userId);
    //    using (SqlConnection connection = new SqlConnection(g.connectionString))
    //    {
    //        connection.Open();
    //        using (SqlCommand command = new SqlCommand(sql, connection))
    //        {
    //            using (SqlDataReader reader = command.ExecuteReader())
    //            {
    //                while (reader.Read())
    //                {
    //                    x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(0));
    //                }
    //            }
    //        }
    //        connection.Close();
    //    }
    //    return x;
    //}





}
