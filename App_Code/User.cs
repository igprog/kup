using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.IO;
using System.Text;
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
    string sqlString = @"SELECT DISTINCT u.id, u.buisinessUnitCode, u.firstName, u.lastName, u.pin, u.birthDate, u.accessDate, u.terminationDate, u.isActive, u.monthlyFee, b.id, b.title FROM Users u
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
        public double totalMebershipFees;
        public double totalUserPayment;
        public double totalMebershipFeesWithUserPayment;
        public double totalWithdrawn;  // ukupno povućeno kod isčlanjenja
        //public double totalMebershipFeesRequired;
        public double terminationWithdraw;  // iznos preostao za povući kod isčlanjenja
        public double monthlyRepayment;
        //public List<UserStatus> statusHistory;

        public string activeLoanId;

        //TODO: list<Card>
        public List<Account.NewAccount> records;
        public Account.Total total;
    }

    public class UserStatus {
        public int status;
        public string statusDate;
    }

    public class Response {
        public bool status;
        public string msg;
        public NewUser user; 
    }

    [WebMethod]
    public string Init() {
        NewUser x = new NewUser();
        x.id = null;
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
        x.totalMebershipFees = 0;
        x.totalUserPayment = 0;
        x.totalMebershipFeesWithUserPayment = 0;
        x.totalWithdrawn = 0;
        //x.totalMebershipFeesRequired = 0;
        x.terminationWithdraw = 0;
        x.monthlyRepayment = 0;
        //x.statusHistory = new List<UserStatus>();
        x.activeLoanId = null;
        x.records = new List<Account.NewAccount>();
        x.total = new Account.Total();
        return JsonConvert.SerializeObject(x, Formatting.None);
    }

    [WebMethod]
    public string Load(string buisinessUnitCode, string search) {
        try {
            return JsonConvert.SerializeObject(GetUsers(buisinessUnitCode, search, false), Formatting.None);
        }catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.None);
        }
    }

    [WebMethod]
    public string Save(NewUser x, bool newUser) {
        try {
            Response r = new Response();
            db.Users();
            if (newUser) {
                r = CheckUser(x);
            } else {
                r.status = true;
                r.user = new NewUser();
            }
            if (r.status == true) {
                string sql = string.Format(@"BEGIN TRAN
                                            IF EXISTS (SELECT * from Users WITH (updlock,serializable) WHERE id = '{0}')
                                                BEGIN
                                                   UPDATE Users SET buisinessUnitCode = '{1}', firstName = N'{2}', lastName = N'{3}', pin = '{4}', birthDate = '{5}', accessDate = '{6}', terminationDate = '{7}', isActive = '{8}', monthlyFee = '{9}' WHERE id = '{0}'
                                                END
                                            ELSE
                                                BEGIN
                                                   INSERT INTO Users (id, buisinessUnitCode, firstName, lastName, pin, birthDate, accessDate, terminationDate, isActive, monthlyFee)
                                                   VALUES ('{0}', '{1}', N'{2}', N'{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')
                                                END
                                        COMMIT TRAN", x.id, x.buisinessUnit.code, x.firstName, x.lastName, x.pin, x.birthDate, x.accessDate, x.terminationDate, x.isActive, x.monthlyFee);
                using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
                r.status = true;
                r.msg = "Spremljeno";
            }
            return JsonConvert.SerializeObject(r, Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.None);
        }
    }

    private Response CheckUser(NewUser x) {
        Response r = new Response();
        r.status = true;
        r.user = new NewUser();
        string sql = null;
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            sql = string.Format(@"SELECT id FROM Users WHERE id = '{0}'", x.id);
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        r.user.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                    }
                }
            }
            sql = string.Format(@"SELECT pin FROM Users WHERE pin = '{0}'", x.pin);
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        r.user.pin = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                    }
                }
            }
            connection.Close();
        }
        if (!string.IsNullOrEmpty(r.user.id)) {
            r.msg = string.Format("Korisnik sa matičnim brojem {0} je već registriran.", r.user.id);
            r.status = false;
        }
        if (!string.IsNullOrEmpty(r.user.pin)) {
            r.msg = string.Format("{0}Korisnik sa OIB-om {1} je već registriran."
                , !string.IsNullOrEmpty(r.msg) ? string.Format(@"{0}
", r.msg) : "", r.user.pin);
            r.status = false;
        }
        return r;
    }

    [WebMethod]
    public string SaveUserStatus(NewUser x) {
        try {
            db.Users();
            string sql = string.Format(@"BEGIN TRAN
                                            IF EXISTS (SELECT * from Users WITH (updlock,serializable) WHERE id = '{0}')
                                                BEGIN
                                                   UPDATE Users SET terminationDate = '{1}', isActive = '{2}' WHERE id = '{0}'                                         
                                                END
                                        COMMIT TRAN", x.id, x.terminationDate, x.isActive);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
            if(x.isActive == 0) {
                if (x.restToRepayment > 0) {
                    SaveTerminationRepayment(x);  // Otplata ostatka pozajmice sa ulogom
                    SubtractUserPayment(x);   // Oduzimanje uloga za iznos otplate duga pozajmice
                }
                SaveTerminationWithdraw(x);
            }
            return JsonConvert.SerializeObject("Spremljeno", Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.None);
        }
    }

    private void SaveTerminationWithdraw(NewUser x) {
        string sql = string.Format(@"BEGIN TRAN                                    
                                        INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, note)
                                        VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}')
                                    COMMIT TRAN", Guid.NewGuid().ToString(), x.id, x.terminationWithdraw, x.terminationDate, g.GetMonth(x.terminationDate), g.GetYear(x.terminationDate), g.terminationWithdraw, "Isplata uloga");
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    private void SaveTerminationRepayment(NewUser x) {
        string sql = string.Format(@"BEGIN TRAN 
                                        BEGIN                                   
                                            INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                            VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}');
                                            UPDATE Loan SET isRepaid = 1 WHERE id = '{7}';
                                        END
                                    COMMIT TRAN", Guid.NewGuid().ToString(), x.id, x.restToRepayment, x.terminationDate, g.GetMonth(x.terminationDate), g.GetYear(x.terminationDate), g.repayment, x.activeLoanId, "Dug odbijen od uloga");
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    private void SubtractUserPayment(NewUser x) {
        string sql = string.Format(@"BEGIN TRAN                                    
                                        INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                        VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')
                                    COMMIT TRAN", Guid.NewGuid().ToString(), x.id, -x.restToRepayment, x.terminationDate, g.GetMonth(x.terminationDate), g.GetYear(x.terminationDate), g.userPayment, null, "Odbijeno od uloga za otplatu duga");
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                command.ExecuteNonQuery();
            }
            connection.Close();
        }
    }

    [WebMethod]
    public string Get(string id, int? year) {
        try {
            return JsonConvert.SerializeObject(GetUserData(id, year), Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.None);
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
            return JsonConvert.SerializeObject("Korisnik izbrisan", Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.None);
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
            return JsonConvert.SerializeObject("Korisnik izbrisan", Formatting.None);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.None);
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

    public List<NewUser> GetUsers(string buisinessUnitCode, string search, bool activeUsers) {
        db.Users();
        string sql = string.Format(@"{0} {1} {2} {3}"
                        , sqlString
                        , !string.IsNullOrEmpty(buisinessUnitCode) || !string.IsNullOrEmpty(search) ? "WHERE" : ""
                        , !string.IsNullOrEmpty(buisinessUnitCode) ? string.Format("u.buisinessUnitCode = '{0}'", buisinessUnitCode): ""
                        , !string.IsNullOrEmpty(search) ? string.Format("{0} u.id LIKE '%{1}%' OR u.firstName LIKE N'{1}%' OR u.lastName LIKE N'{1}%'"
                                                                        , !string.IsNullOrEmpty(buisinessUnitCode) && !string.IsNullOrEmpty(search) ? "AND" : ""                                             
                                                                        , search) : "");
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
        if (activeUsers) {
            xx = xx.Where(a => a.isActive == 1).ToList();
        }
        xx = xx.OrderBy(a => a.lastName).ToList();
        return xx;
    }

    public List<NewUser> GetMonthlyFeeUsers(string buisinessUnitCode, string search, bool activeUsers, string date) {
        List<NewUser> xx = GetUsers(buisinessUnitCode, search, activeUsers);
        xx = xx.Where(a => g.DateDiff(a.accessDate, date, false) <= 31).ToList();
        return xx;
    }


    public List<NewUser> GetLoanUsers(string buisinessUnitCode, string search, string date) {
        db.Users();
        string sql = string.Format(@"{0}
                        LEFT OUTER JOIN Loan l on u.id = l.userId
                        {1} {2} {3} {4} l.isRepaid = 0 AND u.isActive = 1"
                        , sqlString
                        , !string.IsNullOrEmpty(buisinessUnitCode) || !string.IsNullOrEmpty(search) ? "WHERE" : ""
                        , !string.IsNullOrEmpty(buisinessUnitCode) ? string.Format("u.buisinessUnitCode = '{0}'", buisinessUnitCode) : ""
                        , !string.IsNullOrEmpty(search) ? string.Format("{0} (u.id LIKE '%{1}%' OR u.firstName LIKE N'{1}%' OR u.lastName LIKE N'{1}%')"
                                                                      , !string.IsNullOrEmpty(buisinessUnitCode) && !string.IsNullOrEmpty(search) ? "AND" : ""
                                                                      , search) : ""
                        , string.IsNullOrEmpty(buisinessUnitCode) && string.IsNullOrEmpty(search) ? "WHERE" : "AND" );
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
        xx = xx.Where(a => g.DateDiff(a.accessDate, date, false) <= 31).ToList();
        return xx;
    }

    public NewUser GetUserData(string id, int? year) {
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
        x.restToRepayment = GetLoanAmount(id) - GetAmount(id, g.repayment) - GetAmount(id, g.userRepayment);
        x.totalMebershipFees = GetAmount(id, g.monthlyFee);
        x.totalUserPayment = GetAmount(id, g.userPayment);
        x.totalWithdrawn = GetAmount(id, g.terminationWithdraw);
        x.totalMebershipFeesWithUserPayment = x.totalMebershipFees + x.totalUserPayment;
        DateTime now = DateTime.UtcNow;
        //x.totalMebershipFeesRequired = a.GetMonthlyFeeRequiredAccu(x.id, now.Month, now.Year);  //TODO: provjeriti dali to treba???
        x.terminationWithdraw = x.totalMebershipFeesWithUserPayment - x.restToRepayment - x.totalWithdrawn;
        x.activeLoanId = GetActiveLoanId(id);

        if (year != null) {
            x.records = a.GetRecords(x.id, year);
            x.total = new Account.Total();
            x.total.monthlyFee = x.records.Where(r => r.recordType == g.monthlyFee).Sum(r => r.amount);
            x.total.userPayment = x.records.Where(r => r.recordType == g.userPayment).Sum(r => r.amount);
            x.total.userPaymentWithMonthlyFee = x.total.monthlyFee + x.total.userPayment;
            x.total.userPaymentWithMonthlyFeeTotal = a.GetMonthlyFeeStartBalance(id, year) + x.total.userPaymentWithMonthlyFee;
            //x.total.repayment = x.records.Where(r => r.recordType == g.repayment).Sum(r => r.amount);
            x.total.repayment = x.records.Where(r => r.recordType == g.repayment || r.recordType == g.loan).Sum(r => r.amount);
            x.total.userRepayment = x.records.Where(r => r.recordType == g.userRepayment).Sum(r => r.amount);
            x.total.repaymentTotal = x.total.repayment + x.total.userRepayment;
            x.total.terminationWithdraw = x.records.Where(r => r.recordType == g.terminationWithdraw).Sum(r => r.amount);
            x.total.activatedLoan = x.records.Where(r => r.recordType == g.withdraw).Sum(r => r.activatedLoan);
            x.total.loanToRepaid = a.GetLoanStartBalance(id, year) + x.total.activatedLoan;
            x.total.totalObligation = x.total.loanToRepaid - x.total.repaymentTotal;
        }
        //TODO: Totals:
        return x;
    }

     public double GetAmount(string id, string type) {
        db.Account();
        double x = 0;
        string sql = string.Format(@"SELECT SUM(CONVERT(decimal(10,2), amount)) FROM Account WHERE userId = '{0}' AND recordType = '{1}'", id, type);
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

    //public double GetRepayedAmount(string id) {
    //    db.Account();
    //    double x = 0;
    //    string sql = string.Format(@"SELECT SUM(CONVERT(decimal, amount)) FROM Account WHERE userId = '{0}' and recordType = '{1}'", id, g.repayment);
    //    using (SqlConnection connection = new SqlConnection(g.connectionString)) {
    //        connection.Open();
    //        using (SqlCommand command = new SqlCommand(sql, connection)) {
    //            using (SqlDataReader reader = command.ExecuteReader()) {
    //                while (reader.Read()) {
    //                    x = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
    //                }
    //            }
    //        }
    //        connection.Close();
    //    }
    //    return x;
    //}

    public double GetLoanAmount(string id) {
        db.Loan();
        double x = 0;
        string sql = string.Format(@"SELECT SUM(CONVERT(decimal(10,2), loan)) FROM Loan WHERE userId = '{0}'", id);
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

    public string GetActiveLoanId(string id) {
        string x = null;
        string sql = string.Format(@"SELECT id FROM Loan WHERE userId = '{0}' AND isRepaid = 0", id);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        x = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                    }
                }
            }
            connection.Close();
        }
        return x;
    }

}
