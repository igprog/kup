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
    public string ImportUsersCsv() {
        try {
            db.Users();
            string path = Server.MapPath("~/upload/users.csv");
            List<User.NewUser> xx = new List<User.NewUser>();
            using (var reader = new StreamReader(path, Encoding.ASCII)) {
                while (!reader.EndOfStream) {
                    var line = reader.ReadLine();
                    var val = line.Split(';');
                    if (!string.IsNullOrEmpty(val[0]) && val[0] != "id") {
                        User.NewUser x = new User.NewUser();
                        x.id = val[0];
                        x.firstName = val[1];
                        x.lastName = val[2];
                        x.pin = val[3];
                        x.buisinessUnit = new BuisinessUnit.NewUnit();
                        x.buisinessUnit.code = val[4];
                        x.total = new Account.Total();
                        x.total.userPaymentWithMonthlyFee = Convert.ToInt32(val[5]);
                        x.total.terminationWithdraw = Convert.ToInt32(val[6]);
                        x.total.activatedLoan = Convert.ToInt32(val[7]);
                        x.total.withdraw = Convert.ToInt32(val[8]);
                        x.total.repayment = Convert.ToInt32(val[9]);
                        x.monthlyRepayment = Convert.ToInt32(val[10]);
                        //x.totalMebershipFees = Convert.ToInt32(val[5]);
                        //x.restToRepayment = Convert.ToInt32(val[6]);
                        x.birthDate = g.Date(DateTime.Now);  //TODO
                        x.accessDate = g.Date(DateTime.Now);  //TODO
                        x.isActive = 1;
                        x.monthlyFee = s.Data().monthlyFee;  //TODO
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
                                                DELETE FROM Account WHERE recordType = '{0}' OR recordType = '{1}' OR recordType = '{2}' OR recordType = '{3}' OR recordType = '{4}';"
                                                , g.withdraw, g.monthlyFee, g.userPayment, g.manipulativeCosts, g.repayment);
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
                            l.loanDate = u.accessDate;
                            l.withdraw = u.total.withdraw;
                            l.manipulativeCosts = l.loan - l.withdraw;
                            l.repayment = u.monthlyRepayment;
                            l.dedline = Math.Round(l.loan / l.repayment, 0);
                            l.note = "PS";  // Pocetno stanje

                            string manipulativeCostsId = Guid.NewGuid().ToString();
                            string withdrawId = Guid.NewGuid().ToString();
                            string monthlyPaymentId = Guid.NewGuid().ToString();
                            sql = string.Format(@"BEGIN
                                                   INSERT INTO Loan (id, userId, loan, loanDate, repayment, manipulativeCosts, withdraw, dedline, isRepaid, note)
                                                   VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')

                                                   INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                                   VALUES ('{12}', '{1}', '{5}', '{3}', '{10}', '{11}', 'manipulativeCosts', '{0}', 'PS')

                                                   INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                                   VALUES ('{13}', '{1}', '{6}', '{3}', '{10}', '{11}', 'withdraw', '{0}', 'PS')

                                                   INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                                   VALUES ('{14}', '{1}', '{15}', '{3}', '{10}', '{11}', 'monthlyFee', '', 'PS')
                                                  END", l.id, l.user.id, l.loan, l.loanDate, l.repayment, l.manipulativeCosts, l.withdraw, l.dedline, l.isRepaid, l.note, g.GetMonth(l.loanDate), g.GetYear(l.loanDate), manipulativeCostsId, withdrawId, monthlyPaymentId, u.total.userPaymentWithMonthlyFee);

                            command.CommandText = sql;
                            command.Transaction = transaction;
                            command.ExecuteNonQuery();
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
    public string SaveStartBalance(string type, double x) {
        try {
            string sql = string.Format(@"DELETE FROM Account WHERE recordType = '{0}';
                                        INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                        VALUES ('{1}', '', '{2}', '{3}', '{4}', '{5}', '{0}', '', '{6}');"
                                        , type
                                        , Guid.NewGuid().ToString()
                                        , x
                                        , g.Date(DateTime.Now)
                                        , g.GetMonth(g.Date(DateTime.Now))
                                        , g.GetYear(g.Date(DateTime.Now))
                                        , "PS");
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


}
