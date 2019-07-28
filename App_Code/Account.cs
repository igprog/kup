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
    string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
    DataBase db = new DataBase();
    Global g = new Global();
    public Account() {
    }

    public class NewAccount {


        //TODO: list data, total

        public string id;
        public User.NewUser user;
        public int month;
        public int year;
        public double monthlyFee;
        public string loanId;
        public double loan;
        public string loanDate;
        public double repayment;
        public string repaymentDate;
        public double restToRepayment;
        public double totalObligation;
        public double repaid;
        public double accountBalance;
        public string note;
    }

    [WebMethod]
    public string Init() {
        NewAccount x = new NewAccount();
        x.id = Guid.NewGuid().ToString();
        x.user = new User.NewUser();
        x.month = 1;
        x.year = DateTime.Now.Year;
        x.monthlyFee = 0;
        x.loanId = null;
        x.loan = 0;
        x.loanDate = null;
        x.repayment = 0;
        x.repaymentDate = null;
        x.repaid = 0;
        x.restToRepayment = 0;
        x.totalObligation = 0;
        x.accountBalance = 0;
        x.note = null;
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string Save(NewAccount x) {
        try {
            db.Account();
            double lastRepayment = GetRecord(x.user.id, x.month, x.year).repayment;  // ***** in case of update repaiment  *****
            string sql = string.Format(@"BEGIN TRAN
                                            IF EXISTS (SELECT * from Account WITH (updlock,serializable) WHERE id = '{0}')
                                                BEGIN
                                                   UPDATE Account SET userId = '{1}', mo = '{2}', yr = '{3}', monthlyFee = '{4}', loanId = '{5}', loan = '{6}', loanDate = '{7}', repayment = '{8}', repaymentDate = '{9}', repaid = '{10}', restToRepayment = '{11}', accountBalance = '{12}', note = '{13}' WHERE id = '{0}'
                                                END
                                            ELSE
                                                BEGIN
                                                   INSERT INTO Account (id, userId, mo, yr, monthlyFee, loanId, loan, loanDate, repayment, repaymentDate, repaid, restToRepayment, accountBalance, note)
                                                   VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}')
                                                END
                                        COMMIT TRAN", x.id, x.user.id, x.month, x.year, x.monthlyFee, x.loanId, x.loan, x.loanDate, x.repayment, x.repaymentDate, x.repaid - lastRepayment + x.repayment, x.restToRepayment + lastRepayment - x.repayment, x.accountBalance, x.note);
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(CheckLoan(x, x.user.id, x.month, x.year), Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string Load() {
        try {
            db.Account();
            string sql = "SELECT * FROM Account";
            List<NewAccount> xx = new List<NewAccount>();
            using (SqlConnection connection = new SqlConnection(connectionString)) {
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
    public string GetMonthlyRecords(int month, int year, string buisinessUnitCode) {
        try {
            User u = new User();
            List<User.NewUser> users = u.GetUsers(buisinessUnitCode);
            db.Account();
            List<NewAccount> xx = new List<NewAccount>();
            foreach(User.NewUser user in users) {
                NewAccount x = new NewAccount();
                x = GetRecord(user.id, month, year);
                if (string.IsNullOrEmpty(x.id)) {
                    x.id = Guid.NewGuid().ToString();
                    x.user = user;
                    x.month = month;
                    x.year = year;
                    x.monthlyFee = user.monthlyFee;
                    x.loanId = null;
                    x.loan = 0;
                    x.loanDate = null;
                    x.repayment = 0;
                    x.repaymentDate = null;
                    x.repaid = 0;
                    x.restToRepayment = 0;
                    x.totalObligation = 0;
                    x.accountBalance = 0;
                    x.note = null;
                    x = CheckLoan(x, user.id, month, year);
                }
                x.user = user;
                xx.Add(x);
            }
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }


    //TODO:
    
    //[WebMethod]
    //public string GetYearlyRecordsByUserId(string userId, int year) {
    //    try {
    //        User u = new User();
    //        //List<User.NewUser> users = u.GetUsers(buisinessUnitCode);
    //        db.Account();
    //        List<NewAccount> xx = new List<NewAccount>();
    //       // foreach(User.NewUser user in users) {
    //            NewAccount x = new NewAccount();
    //            x = GetRecord(userId, null, year);
    //            xx.Add(x);
    //       // }
    //        return JsonConvert.SerializeObject(xx, Formatting.Indented);
    //    } catch (Exception e) {
    //        return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
    //    }
    //}
   

    [WebMethod]
    public string Delete(string id) {
        try {
            string sql = string.Format("DELETE FROM Account WHERE id = '{0}'", id);
            using (SqlConnection connection = new SqlConnection(connectionString)) {
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

    NewAccount ReadData(SqlDataReader reader) {
        NewAccount x = new NewAccount();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.user = new User.NewUser();
        x.user.id = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.month = reader.GetValue(2) == DBNull.Value ? 1 : reader.GetInt32(2);
        x.year = reader.GetValue(3) == DBNull.Value ? DateTime.Now.Year :reader.GetInt32(3);
        x.monthlyFee = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
        x.loanId = reader.GetValue(5) == DBNull.Value ? null : reader.GetString(5);
        x.loan = reader.GetValue(6) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(6));
        x.loanDate = reader.GetValue(7) == DBNull.Value ? null : reader.GetString(7);
        x.repayment = reader.GetValue(8) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(8));
        x.repaymentDate = reader.GetValue(9) == DBNull.Value ? null : reader.GetString(9);
        x.repaid = reader.GetValue(10) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(10));
        x.restToRepayment = reader.GetValue(11) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(11));
        x.totalObligation = x.monthlyFee + x.repaid;
        x.accountBalance = reader.GetValue(12) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(12));
        x.note = reader.GetValue(13) == DBNull.Value ? null : reader.GetString(13);
        return x;
    }

    public NewAccount GetRecord(string userId, int month, int year) {
        string sql = string.Format("SELECT * FROM Account WHERE userId = '{0}' AND mo = '{1}' AND yr = '{2}'", userId, month, year);
        NewAccount x = new NewAccount();
        using (SqlConnection connection = new SqlConnection(connectionString)) {
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
        string sql = string.Format("SELECT * FROM Account WHERE userId = '{0}' {1}"
            , userId
            , year > 0 ? string.Format("AND yr = '{0}' ORDER BY mo ASC", year) : "");
        List<NewAccount> xx = new List<NewAccount>();
        using (SqlConnection connection = new SqlConnection(connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        NewAccount x = new NewAccount();
                        x = ReadData(reader);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public NewAccount CheckLoan(NewAccount x, string userId, int month, int year) {
        string sql = string.Format(@"
                    SELECT l.id, l.loan, l.repayment, a.repaid, a.restToRepayment FROM Loan l
                    LEFT OUTER JOIN Account a
                    ON l.id = a.loanId
                    WHERE l.userId = '{0}' AND l.isRepaid = 0 AND CONVERT(datetime, l.loanDate) <= '{1}'
                    GROUP BY l.id, l.loan, l.repayment, a.repaid, a.restToRepayment", userId, g.ReffDate(month, year));
        using (SqlConnection connection = new SqlConnection(connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x.loanId = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                        x.loan = reader.GetValue(1) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(1));
                        x.repayment = reader.GetValue(2) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(2));
                        x.repaid = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
                        x.restToRepayment = reader.GetValue(4) == DBNull.Value ? x.loan : Convert.ToDouble(reader.GetString(4));
                        x.totalObligation = x.monthlyFee + x.repayment;
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

}
