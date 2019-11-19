using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using Igprog;

/// <summary>
/// Admin
/// </summary>
[WebService(Namespace = "http://janaf.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Admin : System.Web.Services.WebService {
    DataBase db = new DataBase();
    Global g = new Global();
    Settings s = new Settings();
    public Admin() {
    }

    [WebMethod]
    public bool Login(string username, string password) {
        string adminUserName = ConfigurationManager.AppSettings["adminUserName"];
        string adminPassword = ConfigurationManager.AppSettings["adminPassword"];
        if (username == adminUserName && password == adminPassword) {
            return true;
        } else {
            return false;
        }
    }

    [WebMethod]
    public bool LoginSupervisor(string username, string password) {
        string supervisorUserName = ConfigurationManager.AppSettings["supervisorUserName"];
        string supervisorPassword = ConfigurationManager.AppSettings["supervisorPassword"];
        if (username == supervisorUserName && password == supervisorPassword) {
            return true;
        } else {
            return false;
        }
    }

    [WebMethod]
    public string Sql(string method, string tbl) {
        try {
            string sql = string.Format(@"{0} TABLE {1}", method, tbl);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject("OK", Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string ImportUsersCsv(DateTime date, string note) {
        try {
            db.Users();
            string recordDate = g.Date(date);
            string path = Server.MapPath("~/upload/users.csv");
            List<User.NewUser> xx = new List<User.NewUser>();
            using (var reader = new StreamReader(path, Encoding.Default)) {
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    var val = line.Split(';');
                    int n;
                    if(int.TryParse(val[0], out n)) { 
                        User.NewUser x = new User.NewUser();
                        x.id = val[0];
                        string[] fullName = val[1].Split(' ');
                        x.firstName = fullName[1];
                        x.lastName = fullName[0];
                        x.pin = val[2];
                        x.birthDate = g.ConvertDate(val[3]);
                        x.accessDate = g.ConvertDate(val[4]);
                        x.terminationDate = g.ConvertDate(val[5]);
                        x.buisinessUnit = new BuisinessUnit.NewUnit();
                        x.buisinessUnit.code = val[6];
                        x.total = new Account.Total();
                        x.total.userPaymentWithMonthlyFee = g.ConvertToDouble(val[7]);
                        x.total.terminationWithdraw = g.ConvertToDouble(val[8]);
                        x.total.activatedLoan = g.ConvertToDouble(val[10]);
                        x.total.withdraw = x.total.activatedLoan - (x.total.activatedLoan * s.Data().manipulativeCostsCoeff); //g.ConvertToDouble(val[11]);
                        x.total.repayment = g.ConvertToDouble(val[11]);
                        x.monthlyRepayment = g.ConvertToDouble(val[12]);
                        x.monthlyFee = g.ConvertToDouble(val[13]);
                        if (string.IsNullOrWhiteSpace(val[5])) {
                            x.isActive = 1;
                        } else {
                            x.isActive = 0;
                            x.terminationDate = g.ConvertDate(val[5]);
                            x.total.terminationWithdraw = g.ConvertToDouble(val[8]);
                        }
                        xx.Add(x);
                    }
                }
            }
            string sql = null;
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand()) {
                    command.Connection = connection;
                    using (SqlTransaction transaction = connection.BeginTransaction()) {
                        sql = string.Format(@"DELETE FROM Users;
                                                DELETE FROM Loan;
                                                DELETE FROM Account WHERE recordType = '{0}' OR recordType = '{1}' OR recordType = '{2}' OR recordType = '{3}' OR recordType = '{4}' OR recordType = '{5}';"
                                                , g.withdraw, g.monthlyFee, g.userPayment, g.manipulativeCosts, g.repayment, g.terminationWithdraw);
                        command.CommandText = sql;
                        command.Transaction = transaction;
                        command.ExecuteNonQuery();
                        foreach (User.NewUser u in xx) {
                            sql = string.Format(@"INSERT INTO Users (id, buisinessUnitCode, firstName, lastName, pin, birthDate, accessDate, terminationDate, isActive, monthlyFee)
                                                 VALUES ('{0}', '{1}', N'{2}', N'{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')"
                                                , u.id, u.buisinessUnit.code, u.firstName, u.lastName, u.pin, u.birthDate, u.accessDate, u.terminationDate, u.isActive, u.monthlyFee);
                            command.CommandText = sql;
                            command.Transaction = transaction;
                            command.ExecuteNonQuery();

                            Loan.NewLoan l = new Loan.NewLoan();
                            l.id = Guid.NewGuid().ToString();
                            l.user = new User.NewUser();
                            l.user.id = u.id;
                            l.loan = u.total.activatedLoan;
                            l.loanDate = recordDate;
                            l.withdraw = u.total.withdraw;
                            l.manipulativeCosts = l.loan - l.withdraw;
                            l.repayment = u.total.repayment; // u.monthlyRepayment;
                            if ( u.monthlyRepayment > 0) {
                                l.dedline = Math.Round((l.loan - l.repayment) / u.monthlyRepayment, 0);
                            } else {
                                l.dedline = 0;
                            }
                            l.note = note; // "PS";  // Pocetno stanje

                            if (l.loan > 0) {
                                if (l.loan - l.repayment == 0) {  // Saldo
                                    l.isRepaid = 1;
                                }
                                string manipulativeCostsId = Guid.NewGuid().ToString();
                                string withdrawId = Guid.NewGuid().ToString();
                                string monthlyPaymentId = Guid.NewGuid().ToString();
                                string repaidId = Guid.NewGuid().ToString();
                                sql = string.Format(@"BEGIN
                                                   INSERT INTO Loan (id, userId, loan, loanDate, repayment, manipulativeCosts, withdraw, dedline, isRepaid, note)
                                                   VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')

                                                   INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                                   VALUES ('{17}', '{1}', '{13}', '{3}', '{10}', '{11}', 'repayment', '{0}', '{9}')
                                                  END"
                                                            , l.id
                                                            , l.user.id
                                                            , l.loan
                                                            , l.loanDate
                                                            , u.monthlyRepayment
                                                            , l.manipulativeCosts
                                                            , l.withdraw
                                                            , l.dedline
                                                            , l.isRepaid
                                                            , l.note
                                                            , g.GetMonth(l.loanDate)
                                                            , g.GetYear(l.loanDate)
                                                            , u.total.userPaymentWithMonthlyFee
                                                            , l.repayment
                                                            , manipulativeCostsId
                                                            , withdrawId
                                                            , monthlyPaymentId
                                                            , repaidId);
                                command.CommandText = sql;
                                command.Transaction = transaction;
                                command.ExecuteNonQuery();
                            }

                            if (u.total.userPaymentWithMonthlyFee > 0) {
                                string monthlyPaymentId = Guid.NewGuid().ToString();
                                string repaidId = Guid.NewGuid().ToString();
                                sql = string.Format(@"BEGIN
                                                       INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                                       VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '', '{7}')
                                                      END"
                                                            , monthlyPaymentId
                                                            , l.user.id
                                                            , u.total.userPaymentWithMonthlyFee
                                                            , recordDate
                                                            , g.GetMonth(recordDate)
                                                            , g.GetYear(recordDate)
                                                            , g.monthlyFee
                                                            , note);
                                command.CommandText = sql;
                                command.Transaction = transaction;
                                command.ExecuteNonQuery();
                            }

                            if (u.total.terminationWithdraw > 0) {
                                string terminationWithdrawId = Guid.NewGuid().ToString();
                                sql = string.Format(@"INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                                   VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '', '{7}')"
                                                , terminationWithdrawId, u.id, u.total.terminationWithdraw, recordDate, g.GetMonth(recordDate), g.GetYear(recordDate), g.terminationWithdraw, note);
                                command.CommandText = sql;
                                command.Transaction = transaction;
                                command.ExecuteNonQuery();
                            }

                        }
                        transaction.Commit();
                    }
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject("Spremljeno", Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string SaveStartBalance(string type, double x, DateTime date, string note) {
        try {
            string date_ = g.Date(date);
            string sql = string.Format(@"INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                        VALUES ('{1}', '', '{2}', '{3}', '{4}', '{5}', '{0}', '', '{6}');"
                                      , type
                                      , Guid.NewGuid().ToString()
                                      , x
                                      , date_
                                      , g.GetMonth(date_)
                                      , g.GetYear(date_)
                                      , note);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject("Spremljeno", Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string BackupDB(string date) {
        try {
            var backupFolder = s.Data().backupFolder;
            //var backupFolder = Server.MapPath("~/upload/");
            var sqlConStrBuilder = new SqlConnectionStringBuilder(g.connectionString);
            var backupFileName = string.Format("{0}{1}_{2}.bak", backupFolder, sqlConStrBuilder.InitialCatalog, date);
            using (var connection = new SqlConnection(sqlConStrBuilder.ConnectionString)) {
                var query = string.Format("BACKUP DATABASE {0} TO DISK='{1}'",
                    sqlConStrBuilder.InitialCatalog, backupFileName);
                using (var command = new SqlCommand(query, connection)) {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            return JsonConvert.SerializeObject("Spremljeno", Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }


}
