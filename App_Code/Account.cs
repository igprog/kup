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
        public int month;
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
    }

    public class Total {
        public double monthlyFee;
        public double repayment;
        public double totalObligation;
    }

    public class Accounts {
        public List<NewAccount> data;
        public Total total;
    }

    public enum RecordType {
        loan, manipulativeCosts, withdraw, monthlyFee, bankFee, interest, otherFee
    };

    public string giroaccount = "giroaccount";

    public class Recapitulation {
        public string date;
        public int month;
        public int year;
        public double input;  // duguje
        public double output;  // potrazuje
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

    // ***** Konto *****
    public class AccountNo {
        public string giroAccount;
        public string loan;
        public string monthlyFee;
        public string manipulativeCosts;
        public string bankFee;
        public string otherFee;
    }

    [WebMethod]
    public string Init() {
        NewAccount x = new NewAccount();
        x.id = Guid.NewGuid().ToString();
        x.user = new User.NewUser();
        x.amount = 0;
        x.recordDate = g.Date(DateTime.Now);
        x.month = 1;
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
            x.recordType = RecordType.monthlyFee.ToString();
            x.amount = x.monthlyFee;
            return JsonConvert.SerializeObject(Save(x), Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string SaveRepayment(NewAccount x) {
        try {
            x.recordType = RecordType.loan.ToString();
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
    public string GetMonthlyFee(int month, int year, string buisinessUnitCode) {
        try {
            User u = new User();
            List<User.NewUser> users = u.GetUsers(buisinessUnitCode);
            db.Account();
            Accounts xx = new Accounts();
            xx.data = new List<NewAccount>();
            foreach(User.NewUser user in users) {
                NewAccount x = new NewAccount();
                x = GetRecord(user.id, month, year, RecordType.monthlyFee.ToString());
                if (string.IsNullOrEmpty(x.id)) {
                    x.id = Guid.NewGuid().ToString();
                    x.user = user;
                    x.amount = 0;
                    x.recordDate = g.Date(DateTime.Now);
                    x.recordType = null;
                    x.loanId = null;
                    x.note = null;
                    //x = CheckLoan(x, user.id, month, year);  //TODO
                }
                x = CheckMonthlyFee(x, user.id, RecordType.monthlyFee.ToString());
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
    public string GetLoanUsers(int month, int year, string buisinessUnitCode) {
        try {
            User u = new User();
            List<User.NewUser> users = u.GetLoanUsers(buisinessUnitCode);
            db.Account();
            Accounts xx = new Accounts();
            xx.data = new List<NewAccount>();
            foreach(User.NewUser user in users) {
                NewAccount x = new NewAccount();
                x = GetRecord(user.id, month, year, RecordType.loan.ToString());
                if (string.IsNullOrEmpty(x.id)) {
                    x.id = Guid.NewGuid().ToString();
                    x.user = user;
                    x.amount = 0;
                    x.recordDate = g.Date(DateTime.Now);
                    x.month = month;
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
    public string GetMonthlyRecords(int month, int year, string buisinessUnitCode) {
        try {
            User u = new User();
            List<User.NewUser> users = u.GetUsers(buisinessUnitCode);
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
                    x.month = month;
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
                x = CheckMonthlyFee(x, user.id, RecordType.monthlyFee.ToString());
                x.totalObligation = x.monthlyFee + x.repayment;
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
            string sql = string.Format(@"SELECT SUM(CONVERT(decimal, a.amount)), a.recordType, a.note FROM Account a WHERE yr = '{0}' and mo = '{1}'
                                        GROUP BY a.recordType, a.note", year, month);
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
                            x.note = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
                            xx.data.Add(x);
                        }
                    }
                }
                connection.Close();
                xx.data = PrepareEntryData(xx.data);
                xx.total = new Recapitulation();
                xx.total.input = xx.data.Sum(a => a.input);
                xx.total.output = xx.data.Sum(a => a.output);
            }
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    private List<Recapitulation> PrepareEntryData(List<Recapitulation> xx) {
        List<Recapitulation> xxx = new List<Recapitulation>();
        Recapitulation r = new Recapitulation();
        r.note = "Žiro račun";
        r.output = 99999;  // TODO
        r.input = 66666;  // TODO
        r.account = GetAccountNo(giroaccount);
        xxx.Add(r);
        foreach (Recapitulation x in xx) {
            x.account = GetAccountNo(x.recordType);
            if (x.recordType == RecordType.loan.ToString()) {
                x.output = xx.Where(a => a.recordType == "withdraw").Sum(a => a.input);
                x.note = "Pozajmice";
            }
            if (x.recordType == RecordType.monthlyFee.ToString()) {
                x.output = GetMonthlyFeeRequired(x.month, x.year);
                x.input = 0;
                x.note = "Ulozi";
            }
            if (x.recordType == RecordType.bankFee.ToString() || x.recordType == RecordType.otherFee.ToString()) {
                x.output = x.input;
                x.input = 0;
            }
            if (x.recordType == RecordType.manipulativeCosts.ToString()) {
                x.note = string.Format("Manipulativni troškovni {0}%", g.manipulativeCostsPerc());
            }
            if(x.recordType != RecordType.withdraw.ToString()) {
                xxx.Add(x);
            }
        }
        return xxx;
    }


    [WebMethod]
    public string LoadRecapitulation(int year, string type) {
        // TODO: Konto
        try {
            db.Account();
            string sql = null;
            string _sql = string.Format("SELECT a.mo, a.amount, a.recordType, a.note FROM Account a WHERE yr = '{0}'", year);
            if (type == RecordType.loan.ToString() || type == RecordType.withdraw.ToString()) {
                sql = string.Format(@"{0} AND a.recordType = 'withdraw' OR a.recordType = 'loan'", _sql);
            } else if (type == giroaccount) {
                sql = _sql;
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
            }
            return JsonConvert.SerializeObject(xxx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    private string GetAccountNo(string type) {
        string x = null;
        if (type == giroaccount) {
            x = s.Data().account.giroAccount;
        }
        if (type == RecordType.loan.ToString()) {
            x = s.Data().account.loan;
        }
        if (type == RecordType.monthlyFee.ToString()) {
            x = s.Data().account.monthlyFee;
        }
        if (type == RecordType.manipulativeCosts.ToString()) {
            x = s.Data().account.manipulativeCosts;
        }
        if (type == RecordType.bankFee.ToString()) {
            x = s.Data().account.bankFee;
        }
        if (type == RecordType.otherFee.ToString()) {
            x = s.Data().account.otherFee;
        }
        return x;
    }

    private List<RecapMonthlyTotal> GetRecapMonthleyTotal(List<Recapitulation> data, string type, int year) {
        string inputType = null, outputType = null;
        if (type == RecordType.loan.ToString()) {
            inputType = RecordType.loan.ToString();
            outputType = RecordType.withdraw.ToString();
        } else if (type == RecordType.bankFee.ToString() || type == RecordType.otherFee.ToString()) {
            inputType = null;
            outputType = type;
        } else {
            inputType = type;
            outputType = null;
        }
        List<RecapMonthlyTotal> xx = new List<RecapMonthlyTotal>();

        //TODO: Pocetno stanje
        if(type == RecordType.loan.ToString() || type == RecordType.monthlyFee.ToString() || type == giroaccount) {
            RecapMonthlyTotal x = new RecapMonthlyTotal();
            x.month = "PS";
            x.total = new Recapitulation();
            x.total.date = "01.01";
            x.total.note = "Početno stanje";
            if (type == RecordType.loan.ToString()) {
                x.total.output = GetStartBalance(year, type);  // duguje
            }
            if (type == RecordType.monthlyFee.ToString()) {
                x.total.input = GetStartBalance(year, type) + s.Data().startAccountBalance;  // potražuje 
            }
            if(type == giroaccount) {
                x.total.input = GetStartBalance(year, type) + s.Data().startAccountBalance;
            }
            xx.Add(x);
        }

        for (int i = 1; i <= 12; i++) {
            RecapMonthlyTotal x = new RecapMonthlyTotal();
            x.month = i.ToString();
            x.total = new Recapitulation();
            x.total.date = g.SetDayMonthDate(g.GetLastDayInMonth(year, i), i);
            if (!string.IsNullOrEmpty(inputType)) {
                var input = inputType != giroaccount
                    ? data.Where(a => a.month.ToString() == x.month && a.recordType == inputType)
                    : data.Where(a => a.month.ToString() == x.month && a.recordType == RecordType.loan.ToString()
                                                                    || a.recordType == RecordType.monthlyFee.ToString()
                                                                    || a.recordType == RecordType.manipulativeCosts.ToString()
                                                                    || a.recordType == RecordType.interest.ToString());
                x.total.input = input.Sum(a => a.input);
            }
            if (!string.IsNullOrEmpty(outputType)) {
                var output = data.Where(a => a.month.ToString() == x.month && a.recordType == outputType);
                x.total.output = output.Sum(a => a.input);  // !!! Ovo nije greška (uvijek se vrijednost (amount iz Account.tbl) sprema u (a.input)
            }
            if (x.total.input > 0 || x.total.output > 0) {
                if (type == RecordType.monthlyFee.ToString()) {
                    x.total.output = GetMonthlyFeeRequired(i, year);
                }
                if (type == RecordType.loan.ToString()) {
                    x.total.note = string.Format("Promet pozajmica {0}/{1}", x.month, year);
                } else if (type == RecordType.monthlyFee.ToString()) {
                    x.total.note = string.Format("Promet uloga {0}/{1}", x.month, year);
                }else if (type == RecordType.manipulativeCosts.ToString()) {
                    x.total.note = string.Format("{0}% Manipulativni troškovni {1}/{2}", g.manipulativeCostsPerc(), x.month, year);
                } else if (type == giroaccount) {
                    x.total.note = string.Format("Promet Žiro-Računa {0}/{1}", x.month, year);
                } else {
                    x.total.note = data.Where(a => a.month.ToString() == i.ToString()).FirstOrDefault().note;
                }
                x.month = g.Month(i);
                xx.Add(x);
            }
        }
        return xx;
    }

    private double GetMonthlyFeeRequired(int month, int year) {
        double x = 0;
        string sql = string.Format("SELECT SUM(CONVERT(DECIMAL, u.monthlyFee)) FROM Users u WHERE CONVERT(DATETIME, u.accessDate) <= '{0}' AND u.isActive = 1", g.ReffDate(month == null ? 1 : month, year));
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
        //string sql = string.Format("SELECT SUM(CONVERT(DECIMAL, a.amount)) FROM Account a WHERE CONVERT(DATETIME, CONCAT(a.yr, '-', a.mo, '-01')) <= '{0}' AND a.recordType = '{1}'"
        //    , g.ReffDate(1, year), type);

        string sql = string.Format(@"SELECT SUM(CONVERT(DECIMAL, a.amount)) FROM Account a WHERE CONVERT(DATETIME, CONCAT(a.yr, '-', a.mo, '-01')) <= '{0}' {1}"
         , g.ReffDate(1, year)
         , type != giroaccount ? string.Format("AND a.recordType = '{0}'", type) : string.Format("AND (a.recordType = '{0}' OR a.recordType = '{1}' OR a.recordType = '{2}' OR a.recordType = '{3}')"
                                                                                             , RecordType.loan.ToString()
                                                                                             , RecordType.monthlyFee.ToString()
                                                                                             , RecordType.manipulativeCosts.ToString()
                                                                                             , RecordType.interest.ToString()));

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
            x += type == "input" ? i.total.input : i.total.output;
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
                                        COMMIT TRAN", x.id, x.user.id, x.amount, x.recordDate, x.month, x.year, x.recordType, x.loanId, x.note);
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
        x.month = reader.GetValue(4) == DBNull.Value ? 1 : reader.GetInt32(4);
        x.year = reader.GetValue(5) == DBNull.Value ? DateTime.Now.Year :reader.GetInt32(5);
        x.recordType = reader.GetValue(6) == DBNull.Value ? null : reader.GetString(6);
        x.loanId = reader.GetValue(7) == DBNull.Value ? null : reader.GetString(7);
        x.note = reader.GetValue(8) == DBNull.Value ? null : reader.GetString(8);

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
        string sql = string.Format("SELECT * FROM Account WHERE userId = '{0}' {1}"
            , userId
            , year > 0 ? string.Format("AND yr = '{0}' ORDER BY mo ASC", year) : "");
        List<NewAccount> xx = new List<NewAccount>();
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        NewAccount x = new NewAccount();
                        x = ReadData(reader);
                        x = CheckLoan(x, userId);
                        x = CheckMonthlyFee(x, userId, RecordType.monthlyFee.ToString());
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
            , g.Month(x.month)
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

    public NewAccount CheckMonthlyFee(NewAccount x, string userId, string recordType) {
        string sql = string.Format(@"
                    SELECT a.userId, a.amount, u.monthlyFee FROM Account a
                    LEFT OUTER JOIN Users u
                    ON a.userId = u.id
                    WHERE a.userId = '{0}' AND mo = '{1}' AND yr = '{2}' {3}"
                        , userId
                        , x.month
                        , x.year
                        , !string.IsNullOrEmpty(recordType) ? string.Format("AND recordType = '{0}'", recordType) : "");
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x.user.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                        x.monthlyFee = reader.GetValue(1) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(1));
                        x.user.monthlyFee = reader.GetValue(2) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(2));
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

    public double Repaid(NewAccount x) {
        double repaid = 0;
        string sql = string.Format(@"
                    SELECT SUM(CONVERT(decimal, a.amount)) FROM Account a
                    WHERE a.userId = '{0}' AND a.recordType = 'loan' AND (CAST(CONCAT(a.yr, '-', a.mo, '-', '01') AS datetime) <= CAST('{1}-{2}-01' AS datetime)) AND a.loanId = '{3}'"
                       , x.user.id, x.year, g.Month(x.month), x.loanId);
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

}
