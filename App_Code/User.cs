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
/// User
/// </summary>
[WebService(Namespace = "http://janaf.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class User : System.Web.Services.WebService {
    DataBase db = new DataBase();
    Global g = new Global();
    Account a = new Account();
    Settings s = new Settings();
    string sqlString = @"SELECT u.id, u.buisinessUnitCode, u.firstName, u.lastName, u.pin, u.birthDate, u.accessDate, u.terminationDate, u.isActive, u.monthlyFee, b.id, b.title FROM Users u
                        LEFT OUTER JOIN BuisinessUnit b
                        ON u.buisinessUnitCode = b.code";

    public User() {
    }

      public class NewUser {
        public string id;
        //public string buisinessUnitCode;
        public BuisinessUnit.NewUnit buisinessUnit;
        public string firstName;
        public string lastName;
        public string pin;
        public string birthDate;
        public string accessDate;
        public string terminationDate;
        public int isActive;
        public double monthlyFee;
        public double restToRepayment;
        //public List<UserStatus> statusHistory;

        //public string activeLoanId;

        //TODO: list<Card>
        public List<Account.NewAccount> records;
    }

    public class UserStatus {
        public int status;
        public string statusDate;
    }

    [WebMethod]
    public string Init() {
        NewUser x = new NewUser();
        x.id = null;
        //x.buisinessUnitCode = null;
        x.buisinessUnit = new BuisinessUnit.NewUnit();
        x.firstName = null;
        x.lastName = null;
        x.pin = null;
        x.birthDate = null;
        x.accessDate = null;
        x.terminationDate = null;
        x.isActive = 1;
        x.monthlyFee = s.Data().monthlyFee;
        x.restToRepayment = 0;
        //x.statusHistory = new List<UserStatus>();
        //x.activeLoanId = null;
        x.records = new List<Account.NewAccount>();
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string Save(NewUser x) {
        try {
            db.Users();
            string sql = string.Format(@"BEGIN TRAN
                                            IF EXISTS (SELECT * from Users WITH (updlock,serializable) WHERE id = '{0}')
                                                BEGIN
                                                   UPDATE Users SET buisinessUnitCode = '{1}', firstName = '{2}', lastName = '{3}', pin = '{4}', birthDate = '{5}', accessDate = '{6}', terminationDate = '{7}', isActive = '{8}', monthlyFee = '{9}' WHERE id = '{0}'
                                                END
                                            ELSE
                                                BEGIN
                                                   INSERT INTO Users (id, buisinessUnitCode, firstName, lastName, pin, birthDate, accessDate, terminationDate, isActive, monthlyFee)
                                                   VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')
                                                END
                                        COMMIT TRAN", x.id, x.buisinessUnit.code, x.firstName, x.lastName, x.pin, x.birthDate, x.accessDate, x.terminationDate, x.isActive, x.monthlyFee);
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
    public string Load(string buisinessUnitCode) {
        try {
            return JsonConvert.SerializeObject(GetUsers(buisinessUnitCode), Formatting.Indented);
        }catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string Get(string id, int? year) {
        try {
            db.Users();
            string sql = string.Format("{0} WHERE u.id = '{1}'", sqlString, id);
            NewUser x = new NewUser();
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
            x.restToRepayment = GetLoanAmount(id) - GetRepayedAmount(id);
            //x.activeLoanId = GetActiveLoanId(id);
            if (year != null) {
                x.records = a.GetRecords(x.id, year);
            }
            return JsonConvert.SerializeObject(x, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string Cancel(string id) {
        try {
            db.Users();
            string sql = string.Format(@"UPDATE Users SET isActive = 0 WHERE id = '{0}'", id);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject("Korisnik izbrisan", Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string Delete(string id) {
        try {
            db.Users();
            string sql = string.Format(@"DELETE FROM Users WHERE id = '{0}';
                        DELETE FROM Account WHERE userId = '{0}';
                        DELETE FROM Loan WHERE userId = '{0}';", id);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject("Korisnik izbrisan", Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    NewUser ReadData(SqlDataReader reader) {
        NewUser x = new NewUser();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.buisinessUnit = new BuisinessUnit.NewUnit();
        x.buisinessUnit.code = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.firstName = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
        x.lastName = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
        x.pin = reader.GetValue(4) == DBNull.Value ? null : reader.GetString(4);
        x.birthDate = reader.GetValue(5) == DBNull.Value ? null : reader.GetString(5);
        x.accessDate = reader.GetValue(6) == DBNull.Value ? null : reader.GetString(6);
        x.terminationDate = reader.GetValue(7) == DBNull.Value ? null : reader.GetString(7);
        x.isActive = reader.GetValue(8) == DBNull.Value ? 1 : reader.GetInt32(8);
        x.monthlyFee = reader.GetValue(9) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(9));
        x.buisinessUnit.id = reader.GetValue(10) == DBNull.Value ? null : reader.GetString(10);
        x.buisinessUnit.title = reader.GetValue(11) == DBNull.Value ? null : reader.GetString(11);
        //x.statusHistory = new List<UserStatus>();  // TODO GetUserStatusHistory

        return x;
    }

    public List<NewUser> GetUsers(string buisinessUnitCode) {
        db.Users();
        string sql = string.Format(@"{0} {1}"
                        , sqlString 
                        , !string.IsNullOrEmpty(buisinessUnitCode) ? string.Format("WHERE u.buisinessUnitCode = '{0}'", buisinessUnitCode): "");
        List<NewUser> xx = new List<NewUser>();
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        NewUser x = ReadData(reader);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public List<NewUser> GetLoanUsers(string buisinessUnitCode) {
        db.Users();
        string sql = string.Format(@"{0}
                        LEFT OUTER JOIN Loan l on u.id = l.userId
                        {1}"
                        , sqlString 
                        , !string.IsNullOrEmpty(buisinessUnitCode) ? string.Format("WHERE u.buisinessUnitCode = '{0}' AND l.isRepaid = 0", buisinessUnitCode): "");
        List<NewUser> xx = new List<NewUser>();
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        NewUser x = ReadData(reader);
                        xx.Add(x);
                    }
                }
            }
            connection.Close();
        }
        return xx;
    }

    public double GetRepayedAmount(string id) {
        db.Account();
        double x = 0;
        string sql = string.Format(@"SELECT SUM(CONVERT(decimal, amount)) FROM Account WHERE userId = '{0}' and recordType = '{1}'", id, "repayment");
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

    public double GetLoanAmount(string id) {
        db.Loan();
        double x = 0;
        string sql = string.Format(@"SELECT SUM(CONVERT(decimal, loan)) FROM Loan WHERE userId = '{0}'", id);
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

    //public string GetActiveLoanId(string id) {
    //    string x = null;
    //    string sql = string.Format(@"SELECT id FROM Loan WHERE userId = '{0}' AND isRepaid = 0", id);
    //    using (SqlConnection connection = new SqlConnection(connectionString)) {
    //        connection.Open();
    //        using (SqlCommand command = new SqlCommand(sql, connection)) {
    //            using (SqlDataReader reader = command.ExecuteReader()) {
    //                while (reader.Read()) {
    //                    x = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
    //                }
    //            }
    //        }
    //        connection.Close();
    //    }
    //    return x;
    //}

}
