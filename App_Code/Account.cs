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
[WebService(Namespace = "http://janaf.hr/")]
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
        public string repaymentDate;
        public double restToRepayment;
        public double totalObligation;
        public double repaid;
        public double lastMonthObligation;
        public List<UserPayment> userPayment;
        public double userPaymentTotal;
        public string lastDayInMonth;
        public double monthlyFeeStartBalance;
        public double loanStartBalance;
        public double activatedLoan;
    }

    public class Total {
        public double monthlyFee;
        public double userPayment;
        public double userPaymentWithMonthlyFee;
        public double userPaymentWithMonthlyFeeTotal;
        public double repayment;
        public double totalObligation;
        public double terminationWithdraw;
        public double activatedLoan;
        public double loanToRepaid;
        public double bankFee;
        public double otherFee;


    }

    public class Accounts {
        public List<NewAccount> data;
        public Total total;
    }

    //public enum RecordType {
    //    repayment, manipulativeCosts, withdraw, monthlyFee, bankFee, interest, otherFee
    //};

    //public string giroaccount = "giroaccount";

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
        public string account;  // konto
    }

    public class RecapMonthlyTotal {
        public string month;
        public Recapitulation total;
    }

    public class RecapYearlyTotal {
        public int year;
        public string type;
        public string account;  // Konto
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
    }

    // ***** Konto *****
    public class AccountNo {
        public string giroAccount;
        public string repayment;
        public string monthlyFee;
        public string manipulativeCosts;
        public string bankFee;
        public string otherFee;
    }

    [WebMethod]
    public string Init() {
        NewAccount x = new NewAccount();
        x.id = null; Guid.NewGuid().ToString();
        x.user = new User.NewUser();
        x.amount = 0;
        x.recordDate = g.Date(DateTime.Now);
        x.month = "01";
        x.year = DateTime.Now.Year;
        x.recordType = null;
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
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string SaveMonthlyFee(NewAccount x) {
        try {
            x.recordType = g.monthlyFee;
            x.amount = x.monthlyFee;
            return JsonConvert.SerializeObject(Save(x), Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string SaveUserPayment(string userId, UserPayment y) {
        try {
            NewAccount x = new NewAccount();
            x.user = new User.NewUser();
            x.user.id = userId;
            x.recordType = g.userPayment;
            x.amount = y.amount;
            x.id = y.id;
            x.recordDate = y.recordDate;
            x.month = y.month;
            x.year = y.year;
            x.note = y.note;
            return JsonConvert.SerializeObject(Save(x), Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string SaveRepayment(NewAccount x) {
        try {
            x.recordType = g.repayment;
            return JsonConvert.SerializeObject(Save(x), Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string SaveOtherFee(NewAccount x) {
        try {
            return JsonConvert.SerializeObject(Save(x), Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string Load(int year, string type) {
        try {
            db.Account();
            string sql = string.Format(@"SELECT * FROM Account WHERE yr = '{0}' AND recordType = '{1}'", year, type);
            List<NewAccount> xx = new List<NewAccount>();
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewAccount x = ReadData(reader);
                            xx.Add(x);
                        }
                    }
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }


    [WebMethod]
    public string GetMonthlyFee(int month, int year, string buisinessUnitCode, string search) {
        try {
            User u = new User();
            List<User.NewUser> users = u.GetUsers(buisinessUnitCode, search);
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
                    //x = CheckLoan(x, user.id, month, year);  //TODO
                }
                x = CheckMonthlyFee(x, user.id, g.monthlyFee);
                x.userPayment = GetUserPayment(x, user.id);
                x.user = user;
                xx.data.Add(x);
            }
            xx.total = new Total();
          //  xx.total.monthlyFee = xx.data.Sum(a => a.monthlyFee);
           // xx.total.repayment = xx.data.Sum(a => a.repayment);
          //  xx.total.totalObligation = xx.data.Sum(a => a.totalObligation);
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }


    [WebMethod]
    public string GetLoanUsers(int month, int year, string buisinessUnitCode, string search) {
        try {
            User u = new User();
            List<User.NewUser> users = u.GetLoanUsers(buisinessUnitCode, search);
            db.Account();
            Accounts xx = new Accounts();
            xx.data = new List<NewAccount>();
            foreach(User.NewUser user in users) {
                NewAccount x = new NewAccount();
                x = GetRecord(user.id, month, year, g.repayment);
                if (string.IsNullOrEmpty(x.id)) {
                    x.id = Guid.NewGuid().ToString();
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
                    x.repaymentDate = null;
                    x.repaid = 0;
                    x.restToRepayment = 0;
                    x.totalObligation = 0;
                    //x = CheckLoan(x, user.id, month, year);  //TODO
                }
                x = CheckLoan(x, user.id);  //TODO
                x.user = user;
                xx.data.Add(x);
            }
            xx.total = new Total();
          //  xx.total.monthlyFee = xx.data.Sum(a => a.monthlyFee);
           // xx.total.repayment = xx.data.Sum(a => a.repayment);
          //  xx.total.totalObligation = xx.data.Sum(a => a.totalObligation);
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }


    [WebMethod]
    public string GetMonthlyRecords(int month, int year, string buisinessUnitCode, string search) {
        try {
            User u = new User();
            List<User.NewUser> users = u.GetUsers(buisinessUnitCode, search);
            db.Account();
            Accounts xx = new Accounts();
            xx.data = new List<NewAccount>();
            foreach(User.NewUser user in users) {
                NewAccount x = new NewAccount();
                x = GetRecord(user.id, month, year, null);
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
                    x.repaymentDate = null;
                    x.repaid = 0;
                    x.restToRepayment = 0;
                    x.totalObligation = 0;
                    //x = CheckLoan(x, user.id, month, year);
                }
                x = CheckLoan(x, user.id);
                x = CheckMonthlyFee(x, user.id, g.monthlyFee);
                x.totalObligation = x.monthlyFee + x.repayment;

                // ***** Check last month obligation *****
                NewAccount lastMonth = new NewAccount();
                lastMonth = GetRecord(user.id, month == 1 ? 12 : month - 1, month == 1 ? year - 1 : year, null);
                if(!string.IsNullOrEmpty(lastMonth.id)) {
                    lastMonth = CheckLoan(lastMonth, user.id);
                    lastMonth = CheckMonthlyFee(lastMonth, user.id, g.monthlyFee);
                    x.lastMonthObligation = lastMonth.monthlyFee + lastMonth.repayment;
                } else {
                    x.lastMonthObligation = 0;
                }
                //****************************************

                x.user = user;
                xx.data.Add(x);
            }
            xx.total = new Total();
            xx.total.monthlyFee = xx.data.Sum(a => a.monthlyFee);
            xx.total.repayment = xx.data.Sum(a => a.repayment);
            xx.total.totalObligation = xx.data.Sum(a => a.totalObligation);
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string Delete(string id) {
        try {
            string sql = string.Format("DELETE FROM Account WHERE id = '{0}'", id);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    command.ExecuteReader();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject("Obrisano", Formatting.Indented);
        }catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    /***** Temeljnica *****/
    [WebMethod]
    public string LoadEntry(int year, int month) {
        try {
            db.Account();
            string sql = string.Format(@"SELECT SUM(CONVERT(decimal, a.amount)), a.recordType FROM Account a WHERE yr = '{0}' and mo = '{1}'
                                        GROUP BY a.recordType", year, month);
            EntryTotal xx = new EntryTotal();
            xx.data = new List<Recapitulation>();
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
                xx.data = PrepareEntryData(xx.data, year, month);
                xx.total = new Recapitulation();
                xx.total.input = xx.data.Sum(a => a.input);
                xx.total.output = xx.data.Sum(a => a.output);
            }
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    private List<Recapitulation> PrepareEntryData(List<Recapitulation> xx, int year, int month) {
        List<Recapitulation> xxx = new List<Recapitulation>();
        Recapitulation r = new Recapitulation();
        r.note = "Žiro račun";
        r.output = xx.Where(a => a.recordType == g.monthlyFee).Sum(a => a.input) + xx.Where(a => a.recordType == g.repayment).Sum(a => a.input);
        r.input = xx.Where(a => a.recordType == g.withdraw).Sum(a => a.input) + xx.Where(a => a.recordType == g.bankFee).Sum(a => a.input) + xx.Where(a => a.recordType == g.terminationWithdraw).Sum(a => a.input);
        //r.output = xx.Where(a => a.recordType == g.bankFee || a.recordType == g.otherFee || a.recordType == g.terminationWithdraw).Sum(a => a.input);  // TODO
        //r.input = xx.Where(a => a.recordType == g.monthlyFee || a.recordType == g.repayment || a.recordType == g.interest).Sum(a => a.input);   // TODO
        r.account = GetAccountNo(g.giroaccount);
        xxx.Add(r);
        Loan l = new Loan();
        foreach (Recapitulation x in xx) {
            x.account = GetAccountNo(x.recordType);
            if (x.recordType == g.repayment) {
                x.output = l.GetLoansTotal(month, year);
               // x.output = xx.Where(a => a.recordType == g.withdraw).Sum(a => a.input);  // TODO: sve odobrene pozajmice
                x.note = "Pozajmice";
            }
            if (x.recordType == g.monthlyFee) {
                x.output = xx.Where(a => a.recordType == g.terminationWithdraw).Sum(a => a.input);
                x.input = xx.Where(a => a.recordType == g.monthlyFee).Sum(a => a.input);
                //x.output = GetMonthlyFeeRequired(x.month, x.year);
                //x.input = 0;
                x.note = "Ulozi";
            }
            if (x.recordType == g.bankFee) {
                x.output = x.input;
                x.input = 0;
                x.note = "Troškovi održavanja računa";
            }
            //if (x.recordType == g.otherFee) {
            //    x.output = x.input;
            //    x.input = 0;
            //    x.note = "Ostali troškovi";
            //}
            if (x.recordType == g.manipulativeCosts) {
                x.note = string.Format("Manipulativni troškovni {0}%", g.manipulativeCostsPerc());
            }
            //if(x.recordType == g.terminationWithdraw) {
            //    x.output = x.input;
            //    x.note = "Isplata uloga";
            //}
            if(x.recordType != g.withdraw && x.recordType != g.otherFee && x.recordType != g.terminationWithdraw) {
                xxx.Add(x);
            }
        }
        return xxx;
    }


    [WebMethod]
    public string LoadRecapitulation(int year, string type) {
        try {
            db.Account();
            string sql = null;
            string _sql = string.Format("SELECT a.mo, a.amount, a.recordType, a.note FROM Account a WHERE yr = '{0}'", year);
            if (type == g.repayment || type == g.withdraw) {
                sql = string.Format(@"{0} AND a.recordType = '{1}' OR a.recordType = '{2}'", _sql, g.withdraw, g.repayment);
            } else if (type == g.giroaccount) {
                sql = _sql;
            } else if (type == g.monthlyFee) {
                sql = string.Format(@"{0} AND a.recordType = '{1}' OR a.recordType = '{2}'", _sql, g.monthlyFee, g.terminationWithdraw);
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
                xxx.account = GetAccountNo(type); // "TODO";  // TODO: Konto
                xxx.data = GetRecapMonthleyTotal(xx, type, year);
                xxx.total = new Recapitulation();
                xxx.total.input = GetYearlyTotal(xxx.data, "input"); // xxx.data.Sum(a => a.total.input);
                xxx.total.output = GetYearlyTotal(xxx.data, "output");  // xxx.data.Sum(a => a.total.output);
                xxx.total.accountBalance = GetYearlyTotal(xxx.data, "accountBalance");
            }
            return JsonConvert.SerializeObject(xxx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

     [WebMethod]
    public string LoadTotal() {
        try {
            db.Account();
            string sql = string.Format("SELECT SUM(CONVERT(decimal, a.amount)) FROM Account a WHERE a.recordType = '{0}' OR a.recordType = '{1}'", g.monthlyFee, g.userPayment);
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

            sql = string.Format("SELECT SUM(CONVERT(decimal, a.amount)) FROM Account a WHERE a.recordType = '{0}'", g.bankFee);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.bankFee = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                        }
                    }
                }
                connection.Close();
            }

            sql = string.Format("SELECT SUM(CONVERT(decimal, a.amount)) FROM Account a WHERE a.recordType = '{0}'", g.otherFee);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            x.otherFee = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                        }
                    }
                }
                connection.Close();
            }

            sql = "SELECT SUM(CONVERT(decimal, loan)) FROM Loan";
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

            return JsonConvert.SerializeObject(x, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    private string GetAccountNo(string type) {
        string x = null;
        if (type == g.giroaccount) {
            x = s.Data().account.giroAccount;
        }
        if (type == g.repayment) {
            x = s.Data().account.repayment;
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
        if (type == g.otherFee) {
            x = s.Data().account.otherFee;
        }
        return x;
    }

    private List<RecapMonthlyTotal> GetRecapMonthleyTotal(List<Recapitulation> data, string type, int year) {
        string inputType = null, outputType = null;
        if (type == g.repayment) {
            inputType = g.repayment;
            outputType = g.withdraw;
        } else if (type == g.bankFee || type == g.otherFee) {
            inputType = null;
            outputType = type;
        } else if (type == g.giroaccount) {
            inputType = type;
            outputType = type;
        } else if (type == g.monthlyFee) {
            inputType = type;
            outputType = g.terminationWithdraw;
        } else {
            inputType = type;
            outputType = null;
        }
        List<RecapMonthlyTotal> xx = new List<RecapMonthlyTotal>();

        //TODO: Pocetno stanje
        if (type == g.repayment || type == g.monthlyFee || type == g.giroaccount) {
            RecapMonthlyTotal x = new RecapMonthlyTotal();
            x.month = "PS";
            x.total = new Recapitulation();
            x.total.date = "01.01";
            x.total.note = "Početno stanje";
            if (type == g.repayment.ToString()) {
                x.total.output = GetStartBalance(year, type);  // duguje
            }
            if (type == g.monthlyFee) {
                x.total.input = GetStartBalance(year, type);  // potražuje 
            }
            if (type == g.giroaccount) {
                //x.total.accountBalance = GetStartBalance(year, type) + s.Data().startAccountBalance;
                x.total.output = GetStartBalance(year, type) + s.Data().startAccountBalance;
            }
            xx.Add(x);
        }

        for (int i = 1; i <= 12; i++) {
            RecapMonthlyTotal x = new RecapMonthlyTotal();
            x.month = i.ToString();
            x.total = new Recapitulation();
            x.total.date = g.SetDayMonthDate(g.GetLastDayInMonth(year, i), i);

            if (!string.IsNullOrEmpty(inputType)) {
                var input = inputType != g.giroaccount
                    ? data.Where(a => a.month.ToString() == x.month && a.year == year && a.recordType == inputType)
                    : data.Where(a => a.month.ToString() == x.month && a.year == year
                                                                    && (a.recordType == g.repayment
                                                                    || a.recordType == g.monthlyFee
                                                                    || a.recordType == g.interest));
                x.total.input = input.Sum(a => a.input);

                var inputAccumulation = inputType != g.giroaccount
                   ? data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && a.recordType == inputType)
                   : data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year))
                                                                    && (a.recordType == g.repayment
                                                                    || a.recordType == g.monthlyFee
                                                                    || a.recordType == g.interest));
                x.total.inputAccumulation = inputAccumulation.Sum(a => a.input) + (type == g.giroaccount ? s.Data().startAccountBalance : 0);
            }

            if (!string.IsNullOrEmpty(outputType)) {

                //List<Recapitulation> output = new List<Recapitulation>();
                //if (outputType == g.giroaccount) {
                //    output = data.Where(a => a.month.ToString() == x.month && a.year == year
                //                        && (a.recordType == g.withdraw
                //                        || a.recordType == g.bankFee
                //                        || a.recordType == g.otherFee
                //                        || a.recordType == g.terminationWithdraw)).ToList();
                //} else if (outputType == g.monthlyFee) {
                //    output = data.Where(a => a.month.ToString() == x.month && a.year == year && a.recordType == g.terminationWithdraw).ToList();
                //} else {
                //    output = data.Where(a => a.month.ToString() == x.month && a.year == year && a.recordType == outputType).ToList();
                //}


                var output = outputType != g.giroaccount
                    ? data.Where(a => a.month.ToString() == x.month && a.year == year && a.recordType == outputType)
                    : data.Where(a => a.month.ToString() == x.month && a.year == year
                                                                    && (a.recordType == g.withdraw
                                                                    || a.recordType == g.bankFee
                                                                    || a.recordType == g.otherFee
                                                                    || a.recordType == g.terminationWithdraw));
                x.total.output = output.Sum(a => a.input);  // ***** Ovo nije greška (uvijek se vrijednost (amount iz Account.tbl) sprema u (a.input) *****




                //List<Recapitulation> outputAccumulation = new List<Recapitulation>();
                //if (outputType == g.giroaccount)
                //{
                //    outputAccumulation = data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year))
                //                        && (a.recordType == g.withdraw
                //                        || a.recordType == g.bankFee
                //                        || a.recordType == g.otherFee
                //                        || a.recordType == g.terminationWithdraw)).ToList();
                //}
                //else if (outputType == g.monthlyFee)
                //{
                //    outputAccumulation = data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && a.recordType == outputType).ToList();
                //}
                //else
                //{
                //    outputAccumulation = data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && a.recordType == outputType).ToList();
                //}

                var outputAccumulation = outputType != g.giroaccount
                   ? data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year)) && a.recordType == outputType)
                   : data.Where(a => Convert.ToDateTime(g.SetDate(1, a.month, a.year)) <= Convert.ToDateTime(g.SetDate(g.GetLastDayInMonth(year, i), i, year))
                                                                    && (a.recordType == g.withdraw
                                                                    || a.recordType == g.bankFee
                                                                    || a.recordType == g.otherFee
                                                                    || a.recordType == g.terminationWithdraw));
                x.total.outputAccumulation = outputAccumulation.Sum(a => a.input);
            }

            if (x.total.input > 0 || x.total.output > 0) {
                if (type == g.monthlyFee.ToString()) {
                    //x.total.output = GetMonthlyFeeRequired(null, i, year);
                    //x.total.outputAccumulation = GetMonthlyFeeRequiredAccu(null, i, year);
                }
                if (type == g.repayment) {
                    x.total.note = string.Format("Promet pozajmica {0}/{1}", g.Month(i), year);
                } else if (type == g.monthlyFee) {
                    x.total.note = string.Format("Promet uloga {0}/{1}", g.Month(i), year);
                }else if (type == g.manipulativeCosts) {
                    x.total.note = string.Format("{0}% Manipulativni troškovni {1}/{2}", g.manipulativeCostsPerc(), g.Month(i), year);
                } else if (type == g.giroaccount) {
                    x.total.note = string.Format("Promet Žiro-Računa {0}/{1}", g.Month(i), year);
                    x.total.accountBalance = x.total.inputAccumulation - x.total.outputAccumulation;
                } else {
                    x.total.note = data.Where(a => a.month.ToString() == i.ToString()).FirstOrDefault().note;
                }
                x.month = g.Month(i);
                xx.Add(x);
            }
        }
        return xx;
    }

    public double GetMonthlyFeeRequired(string userId, int? month, int year) {
        double x = 0;
        int month_ = month == null ? 1: Convert.ToInt32(month);
        string sql = string.Format(@"SELECT SUM(CONVERT(DECIMAL, u.monthlyFee))
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
        string sql = string.Format(@"SELECT SUM(CONVERT(DECIMAL, a.amount)) FROM Account a WHERE CONVERT(DATETIME, CONCAT(a.yr, '-', a.mo, '-01')) <= '{0}' {1}"
         , g.ReffDate(1, year)
         , type != g.giroaccount ? string.Format("AND a.recordType = '{0}'", type)
                                 : string.Format("AND (a.recordType = '{0}' OR a.recordType = '{1}' OR a.recordType = '{2}' OR a.recordType = '{3}')"
                                                , g.repayment
                                                , g.monthlyFee
                                                , g.manipulativeCosts
                                                , g.interest));

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

    private double GetYearlyTotal(List<RecapMonthlyTotal> data, string type) {
        double x = 0;
        foreach(var i in data) {
            if (type == "input") {
                x += i.total.input;
            } else if (type == "output") {
                x += i.total.output;
            } else if (type == "accountBalance") {
                x += i.total.accountBalance;
            } else {
                x = 0;
            }
            //x += type == "input" ? i.total.input : i.total.output;
        }
        return x;
    }

    public NewAccount Save(NewAccount x) {
        try {
            db.Account();
            //  double lastRepayment = GetRecord(x.user.id, x.month, x.year).repayment;  // ***** in case of update repaiment  *****

            string sql = string.Format(@"BEGIN TRAN
                                            IF EXISTS (SELECT * from Account WITH (updlock,serializable) WHERE id = '{0}')
                                                BEGIN
                                                   UPDATE Account SET userId = '{1}', amount = '{2}', recordDate = '{3}', mo = '{4}', yr = '{5}', recordType = '{6}', loanId = '{7}', note = '{8}' WHERE id = '{0}'
                                                END
                                            ELSE
                                                BEGIN
                                                   INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                                   VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')
                                                END
                                        COMMIT TRAN", string.IsNullOrEmpty(x.id) ? Guid.NewGuid().ToString() : x.id, x.user.id, x.amount, x.recordDate, x.month, x.year, x.recordType, x.loanId, x.note);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return x;
        } catch (Exception e) {
            return new NewAccount();
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

        //if(x.recordType == RecordType.loan.ToString()) {
        //    x.loan = x.amount;
        //    x.repayment = x.amount;
        //    x.repaid = Repaid(x.loanId);
        //    x.restToRepayment = x.loan - x.repaid;
        //    x.totalObligation = x.monthlyFee + x.repayment;
        //}


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
                        x.activatedLoan = GetActivatedLoan(x, userId);
                        x = CheckMonthlyFee(x, userId, g.monthlyFee);
                        x.userPayment = GetUserPayment(x, userId);
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
        string sql = string.Format(@"
                    SELECT l.id, l.loan, l.repayment FROM Loan l
                    WHERE l.userId = '{0}' AND (CAST(l.loanDate AS datetime) <= CAST('{1}-{2}-01' AS datetime)) {3}"
            , userId
            , x.year
            , g.Month(Convert.ToInt32(x.month))
            , !string.IsNullOrEmpty(x.loanId) ? string.Format(" AND l.id = '{0}'", x.loanId) : "");

        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x.loanId = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                        x.loan = reader.GetValue(1) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(1));
                        x.repayment = reader.GetValue(2) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(2));
                        x.repaid = Repaid(x); // reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                        x.restToRepayment = x.loan - x.repaid; // RestToRepayment(x.loanId); // reader.GetValue(4) == DBNull.Value ? x.loan : Convert.ToDouble(reader.GetString(4));
                        x.totalObligation = x.monthlyFee + x.repayment;
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    public double GetActivatedLoan(NewAccount x, string userId) {
        double loan = 0;
        string sql = string.Format(@"
                    SELECT l.loan FROM Loan l
                    WHERE l.userId = '{0}' AND ((CAST(l.loanDate AS datetime) >= CAST('{1}-{2}-01' AS datetime)) AND (CAST(l.loanDate AS datetime) <= CAST('{1}-{2}-{3}' AS datetime)))"
                        , userId
                        , x.year
                        , g.Month(Convert.ToInt32(x.month))
                        , g.GetLastDayInMonth(x.year, Convert.ToInt32(x.month)));
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        loan = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(0));
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
                    WHERE a.userId = '{0}' AND a.mo = '{1}' AND a.yr = '{2}' AND (a.recordType = '{3}' OR a.recordType = '{4}')"
                       , userId
                       , x.month
                       , x.year
                       , g.monthlyFee
                       , g.terminationWithdraw);
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

    public List<UserPayment> GetUserPayment(NewAccount x, string userId) {
        string sql = string.Format(@"
                    SELECT a.id, a.recordDate, a.amount, a.note FROM Account a
                    WHERE a.userId = '{0}' AND a.mo = '{1}' AND a.yr = '{2}' AND a.recordType = '{3}'"
                       , userId
                       , x.month
                       , x.year
                       , g.userPayment);
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
                    SELECT SUM(CONVERT(decimal, a.amount)) FROM Account a
                    WHERE a.userId = '{0}' AND a.recordType = '{1}' AND (CAST(CONCAT(a.yr, '-', a.mo, '-', '01') AS datetime) <= CAST('{2}-{3}-01' AS datetime)) AND a.loanId = '{4}'"
                       , x.user.id, g.repayment, x.year, g.Month(Convert.ToInt32(x.month)), x.loanId);
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
        string sql = string.Format(@"
                    SELECT SUM(CONVERT(decimal, a.amount)) FROM Account a
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
        return x;
    }

    public double GetLoanStartBalance(string userId, int? year) {
        double x = 0;
        double repayed = 0;
        double loan = 0;
        string sql = string.Format(@"
                    SELECT SUM(CONVERT(decimal, a.amount)) FROM Account a
                    WHERE a.userId = '{0}' AND a.recordType = '{1}' AND (CAST(CONCAT(a.yr, '-', a.mo, '-', '01') AS datetime) < CAST('{2}-01-01' AS datetime))"
                       , userId, g.repayment, year);
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
                SELECT SUM(CONVERT(decimal, l.loan)) FROM Loan l
                WHERE l.userId = '{0}' AND (CAST(l.loanDate AS datetime) < CAST('{1}-01-01' AS datetime))"
                    , userId, year);
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
        x = loan - repayed;
        return x;
    }



}
