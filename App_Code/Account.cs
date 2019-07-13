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
    public Account() {
    }

    public class NewAccount {
        public string id;
        public Users.NewUser user;
        public int month;
        public int year;
        public double input;
        public double loan;
        public string loanDate;
        public double repayment;
        public string repaymentDate;
        public double restToRepayment;
        public double accountBalance;
        public string note;
    }

    [WebMethod]
    public string Init() {
        NewAccount x = new NewAccount();
        x.id = Guid.NewGuid().ToString();
        x.user = new Users.NewUser();
        x.month = 1;
        x.year = DateTime.Now.Year;
        x.input = 0;
        x.loan = 0;
        x.loanDate = null;
        x.repayment = 0;
        x.repaymentDate = null;
        x.restToRepayment = 0;
        x.accountBalance = 0;
        x.note = null;
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string Save(NewAccount x) {
        try {
            db.Account();
            string sql = string.Format(@"BEGIN TRAN
                                            IF EXISTS (SELECT * from Account WITH (updlock,serializable) WHERE id = '{0}')
                                                BEGIN
                                                   UPDATE Account SET userId = '{1}', mo = '{2}', yr = '{3}', input = '{4}', loan = '{5}', loanDate = '{6}', repayment = '{7}', repaymentDate = '{8}', restToRepayment = '{9}', accountBalance = '{10}', note = '{11}' WHERE id = '{0}'
                                                END
                                            ELSE
                                                BEGIN
                                                   INSERT INTO Account (id, userId, mo, yr, input, loan, loanDate, repayment, repaymentDate, restToRepayment, accountBalance, note)
                                                   VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}')
                                                END
                                        COMMIT TRAN", x.id, x.user.id, x.month, x.year, x.input, x.loan, x.loanDate, x.repayment, x.repaymentDate, x.restToRepayment, x.accountBalance, x.note);
            using (SqlConnection connection = new SqlConnection(connectionString)) {
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
    public string GetMonthlyRecords(int month, int year) {
        try {
            //db.Account();
            Users u = new Users();
            List<Users.NewUser> users = u.GetUsers();
            db.Account();
            List<NewAccount> xx = new List<NewAccount>();
            foreach(Users.NewUser user in users) {
                NewAccount x = new NewAccount();
                //TODO: provjeriti dali postoje rekordi za odabrani mjesec i godinu, ako postoje onda učitati iz baze, a ako ne postoje onda init =>
                x = GetRecord(user.id, month, year);
                if (string.IsNullOrEmpty(x.id)) {
                    x.id = Guid.NewGuid().ToString();
                    x.user = user;
                    x.month = month;
                    x.year = year;
                    x.input = user.monthlyPayment;
                    x.loan = 0;
                    x.loanDate = null;
                    x.repayment = 0;
                    x.repaymentDate = null;
                    x.restToRepayment = 0;
                    x.accountBalance = 0;
                    x.note = null;
                }
                x.user = user;
                xx.Add(x);
            }
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

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
        x.user = new Users.NewUser();
        x.user.id = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.month = reader.GetValue(2) == DBNull.Value ? 1 : reader.GetInt32(2);
        x.year = reader.GetValue(3) == DBNull.Value ? DateTime.Now.Year :reader.GetInt32(3);
        x.input = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
        x.loan = reader.GetValue(5) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(5));
        x.loanDate = reader.GetValue(6) == DBNull.Value ? null : reader.GetString(6);
        x.repayment = reader.GetValue(7) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(7));
        x.repaymentDate = reader.GetValue(8) == DBNull.Value ? null : reader.GetString(8);
        x.restToRepayment = reader.GetValue(9) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(9));
        x.accountBalance = reader.GetValue(10) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(10));
        x.note = reader.GetValue(11) == DBNull.Value ? null : reader.GetString(11);
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

}
