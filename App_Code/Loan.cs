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
/// Loan
/// </summary>
[WebService(Namespace = "http://igprog.hr/kup/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Loan : System.Web.Services.WebService {
    DataBase db = new DataBase();
    Global g = new Global();
    Settings s = new Settings();
    public Loan() {
    }

    public class NewLoan {
        public string id;
        public User.NewUser user;
        public double loan;
        public string loanDate;
        public double repayment;
        public double manipulativeCosts;
        public double actualLoan;  // pozajmica - manipulativni troskovi
        public double withdraw;  // za isplatu (pozajmica - neotplacena pozajmica)
        public double lastLoanToRepaid; // neotplacena stara pozajmica kod preuzimanja nove pozajmice
        public double dedline;
        public double restToRepayment;  //TODO
        public int isRepaid;
        public string note;
        public double manipulativeCostsCoeff;
        public bool includeManipulativeCosts; // ovo je za slucaj ako se greckom uplati na neciji žiro racun iako nije trazio pozajmicu, u tom slucaju treba biti false
        public bool repaidOldDebt;  // ovo je za slucaj ako se greckom uplati na neciji žiro racun iako nije trazio pozajmicu, u tom slucaju treba biti false
        public BuisinessUnit.NewUnit buisinessUnit;
    }

    public class Total {
        public double loan;
        public double repayment;
        public double manipulativeCosts;
        public double actualLoan;
        public double withdraw;
        public double restToRepayment;
        public double lastLoanToRepaid;
    }

    public class MonthlyTotal {
        public string month;
        public Total total;
    }

    public class Loans {
        public List<NewLoan> data;
        public Total total;
        public List<MonthlyTotal> monthlyTotal; 
    }

    [WebMethod]
    public string Init() {
        NewLoan x = new NewLoan();
        x.id = null; // Guid.NewGuid().ToString();
        x.user = new User.NewUser();
        x.loan = 0;
        x.loanDate = null; // g.Date(DateTime.Now);
        x.repayment = 0;
        x.manipulativeCosts = 0;
        //x.actualLoan = 0;
        x.withdraw = 0;
        x.dedline = s.Data().defaultDedline;
        x.restToRepayment = 0;
        x.isRepaid = 0;
        x.note = null;
        x.manipulativeCostsCoeff = s.Data().manipulativeCostsCoeff;
        x.includeManipulativeCosts = true;
        x.repaidOldDebt = true;
        x.buisinessUnit = new BuisinessUnit.NewUnit();
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string Load(int? month, int year, string buisinessUnitCode, string search) {
        try {
            Loans xx = LoadData(month, year, buisinessUnitCode, search);
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string Save(NewLoan x) {
        try {
            db.Loan();
            db.Account();
            bool isNewLoan = false;
            string manipulativeCostsId = Guid.NewGuid().ToString();
            string withdrawId = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(x.id)) {
                x.id = Guid.NewGuid().ToString();
                isNewLoan = true;
            }

            if (isNewLoan && x.user.restToRepayment > 0 && x.repaidOldDebt) {  // x.repaidOldDebt je za slucaj ako se greckom uplati na neciji žiro racun iako nije trazio pozajmicu, u tom slucaju treba biti false
                UpdateActiveLoan(x);  //********** Ako postoji pozajmica koja nije otplacena onda se ona otplacuje sa dijelom nove pozajmice.
            }

            //TODO: Update Account manipulativeCosts & withdraw
            string sql = string.Format(@"BEGIN TRAN
                                            IF EXISTS (SELECT * from Loan WITH (updlock,serializable) WHERE id = '{0}')
                                                BEGIN
                                                   UPDATE Loan SET userId = '{1}', loan = '{2}', loanDate = '{3}', repayment = '{4}', manipulativeCosts = '{5}', withdraw = '{6}', dedline = '{7}', isRepaid = '{8}', note = '{9}' WHERE id = '{0}';
                                                   UPDATE Account SET userId = '{1}', amount = '{5}', recordDate = '{3}', mo = '{10}', yr = '{11}', note = 'Manipulativni troškovi' WHERE loanId = '{0}' AND recordType = 'manipulativeCosts';
                                                   UPDATE Account SET userId = '{1}', amount = '{6}', recordDate = '{3}', mo = '{10}', yr = '{11}', note = 'Isplata pozajmice' WHERE loanId = '{0}' AND recordType = 'withdraw';
                                                   UPDATE Account SET userId = '{1}', recordDate = '{3}', mo = '{10}', yr = '{11}' WHERE loanId = '{0}' AND recordType = 'loan';
                                                END
                                            ELSE
                                                BEGIN
                                                   INSERT INTO Loan (id, userId, loan, loanDate, repayment, manipulativeCosts, withdraw, dedline, isRepaid, note)
                                                   VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')

                                                   INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                                   VALUES ('{12}', '{1}', '{5}', '{3}', '{10}', '{11}', 'manipulativeCosts', '{0}', 'Manipulativni troškovi')

                                                   INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                                   VALUES ('{13}', '{1}', '{6}', '{3}', '{10}', '{11}',  'withdraw', '{0}', 'Isplata pozajmice')
                                                END
                                        COMMIT TRAN", x.id, x.user.id, x.loan, x.loanDate, x.repayment, x.manipulativeCosts, x.withdraw, x.dedline, x.isRepaid, x.note, g.GetMonth(x.loanDate), g.GetYear(x.loanDate), manipulativeCostsId, withdrawId);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }

            return JsonConvert.SerializeObject(x, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject("Error: " + e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string Get(string id) {
        try {
            db.Loan();
            string sql = string.Format(@"SELECT l.id, l.userId, l.loan, l.loanDate, l.repayment, l.manipulativeCosts, l.withdraw, l.dedline, l.isRepaid, l.note, u.firstName, u.lastName, b.code, b.title FROM Loan l
                        LEFT OUTER JOIN Users u
                        ON l.userId = u.id
                        LEFT OUTER JOIN BuisinessUnit b
                        ON u.buisinessUnitCode = b.code WHERE l.id = '{0}'", id);
            NewLoan x = new NewLoan();
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
            return JsonConvert.SerializeObject(x, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string Delete(string id) {
        try {
            string sql = string.Format(@"DELETE FROM Loan WHERE id = '{0}';
                                        DELETE FROM Account WHERE loanId = '{0}'", id);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    command.ExecuteReader();
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject("Obrisano", Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    public Loans LoadData(int? month, int year, string buisinessUnitCode, string search) {
        db.Loan();
            string sql = string.Format(@"SELECT l.id, l.userId, l.loan, l.loanDate, l.repayment, l.manipulativeCosts, l.withdraw, l.dedline, l.isRepaid, l.note, u.firstName, u.lastName, b.code, b.title FROM Loan l
                        LEFT OUTER JOIN Users u
                        ON l.userId = u.id
                        LEFT OUTER JOIN BuisinessUnit b
                        ON u.buisinessUnitCode = b.code
                        WHERE {0} {1} {2}"
                            , string.Format("CONVERT(datetime, l.loanDate) >= '{0}' AND CONVERT(datetime, l.loanDate) < '{1}'", g.ReffDate(month == null ? 1 : month, year), g.ReffDate(month == null ? 12 : month + 1, year))
                            , string.IsNullOrEmpty(buisinessUnitCode) ? "" : string.Format("AND u.buisinessUnitCode = '{0}'", buisinessUnitCode)
                            , string.IsNullOrEmpty(search) ? "" : string.Format("AND u.id LIKE '%{0}%' OR u.firstName LIKE N'{0}%' OR u.lastName LIKE N'{0}%'", search));
            Loans xx = new Loans();
            xx.data = new List<NewLoan>();
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewLoan x = ReadData(reader);
                            xx.data.Add(x);
                        }
                    }
                }
                connection.Close();
            }

        xx.data = xx.data.OrderBy(a => a.user.lastName).ToList();
        xx.total = new Total();
        xx.total.loan = xx.data.Sum(a => a.loan);
        xx.total.repayment = xx.data.Sum(a => a.repayment);
        xx.total.manipulativeCosts = xx.data.Sum(a => a.manipulativeCosts);
        xx.total.actualLoan = xx.data.Sum(a => a.actualLoan);
        xx.total.withdraw = xx.data.Sum(a => a.withdraw);
        xx.total.lastLoanToRepaid = xx.total.actualLoan - xx.total.withdraw;
        xx.total.restToRepayment = xx.data.Sum(a => a.restToRepayment);
        xx.monthlyTotal = new List<MonthlyTotal>();
        xx.monthlyTotal = GetMonthlyTotal(xx.data);
        return xx;
    }

    NewLoan ReadData(SqlDataReader reader) {
        NewLoan x = new NewLoan();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.user = new User.NewUser();
        x.user.id = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.loan = reader.GetValue(2) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(2));
        x.loanDate = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
        x.repayment = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
        x.manipulativeCosts = reader.GetValue(5) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(5));
        x.manipulativeCostsCoeff = s.Data().manipulativeCostsCoeff;
        x.actualLoan = x.loan - x.manipulativeCosts;
        x.withdraw = reader.GetValue(6) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(6));
        x.lastLoanToRepaid = x.actualLoan - x.withdraw;
        User u = new User();
        x.restToRepayment = GetTotalLoan(x) - GetTotalLoanRepayed(x);  // TOOD: provjeriti
        x.dedline = reader.GetValue(7) == DBNull.Value ? s.Data().defaultDedline : Convert.ToDouble(reader.GetString(7));
        x.isRepaid = reader.GetValue(8) == DBNull.Value ? 0 : reader.GetInt32(8);
        x.note = reader.GetValue(9) == DBNull.Value ? null : reader.GetString(9);
        x.user.firstName = reader.GetValue(10) == DBNull.Value ? null : reader.GetString(10);
        x.user.lastName = reader.GetValue(11) == DBNull.Value ? null : reader.GetString(11);
        x.user.buisinessUnit = new BuisinessUnit.NewUnit();
        x.user.buisinessUnit.code = reader.GetValue(12) == DBNull.Value ? null : reader.GetString(12);
        x.user.buisinessUnit.title = reader.GetValue(13) == DBNull.Value ? null : reader.GetString(13);
        return x;
    }

    private List<MonthlyTotal> GetMonthlyTotal(List<NewLoan> data) {
        List<MonthlyTotal> xx = new List<MonthlyTotal>();
        for(int i=1; i<=12; i++) {
            MonthlyTotal x = new MonthlyTotal();
            x.month = g.Month(i);
            var aa = data.Where(a => a.loanDate.Substring(5, 2) == x.month);
            x.total = new Total();
            x.total.loan = aa.Sum(a => a.loan);
            x.total.repayment = aa.Sum(a => a.repayment);
            x.total.manipulativeCosts = aa.Sum(a => a.manipulativeCosts);
            x.total.actualLoan = aa.Sum(a => a.actualLoan);
            x.total.withdraw = aa.Sum(a => a.withdraw);
            x.total.lastLoanToRepaid = aa.Sum(a => a.lastLoanToRepaid);
            x.total.restToRepayment = aa.Sum(a => a.loan) - aa.Sum(a => a.repayment); // aa.Sum(a => a.restToRepayment);
            xx.Add(x);
        }
        return xx;
    }

    public double GetLoansTotal(int month, int year) {
        string sql = string.Format("SELECT SUM(CONVERT(decimal, loan)) FROM Loan WHERE {0}"
                                , string.Format("CONVERT(datetime, loanDate) >= CONVERT(datetime, '{0}') AND CONVERT(datetime, loanDate) <= CONVERT(datetime, '{1}')", g.SetDate(1, month, year), g.SetDate(g.GetLastDayInMonth(year, month), month, year)));
        double x = 0;
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

    private void UpdateActiveLoan(NewLoan x) {

        //Provjeri dali postoji aktivna pozajmica
        string sql = null;
        string loanId = null;
        sql = string.Format(@"SELECT id FROM Loan WHERE userId = '{0}' AND isRepaid = 0", x.user.id);
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        loanId = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
                    }
                }
            }
            connection.Close();
        }

        //U Account tbl uplati razliku za otplatu
        //sql = string.Format(@"BEGIN TRAN
        //                        BEGIN
        //                            INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
        //                            VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')
        //                            UPDATE Loan SET isRepaid = 1 WHERE id = '{7}'
        //                        END
        //                    COMMIT TRAN", Guid.NewGuid().ToString(), x.user.id, x.user.restToRepayment, x.loanDate, g.GetMonth(x.loanDate), g.GetYear(x.loanDate), g.repayment, loanId, "Otplata novom pozajmicom");

        sql = string.Format(@"BEGIN TRAN
                                BEGIN
                                    INSERT INTO Account (id, userId, amount, recordDate, mo, yr, recordType, loanId, note)
                                    VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')
                                    UPDATE Loan SET isRepaid = 1 WHERE id = '{9}'
                                END
                            COMMIT TRAN", Guid.NewGuid().ToString(), x.user.id, x.user.restToRepayment, x.loanDate, g.GetMonth(x.loanDate), g.GetYear(x.loanDate), g.loan, x.id, "Otplata novom pozajmicom", loanId);

        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                command.ExecuteNonQuery();
            }
            connection.Close();
        }

        //U Loan tbl stavi isRepaid = 1
        //sql = string.Format(@"BEGIN TRAN
        //                        BEGIN
        //                            UPDATE Loan SET isRepaid = 1 WHERE id = '{0}'
        //                        END
        //                    COMMIT TRAN", x.id);
        //using (SqlConnection connection = new SqlConnection(connectionString)) {
        //    connection.Open();
        //    using (SqlCommand command = new SqlCommand(sql, connection)) {
        //        command.ExecuteNonQuery();
        //    }
        //    connection.Close();
        //}


    }


     public double GetTotalLoan(NewLoan x) {
        string sql = string.Format(@"SELECT SUM(CONVERT(decimal, l.loan)) FROM Loan l WHERE CONVERT(datetime, l.loanDate) <= CONVERT(datetime, '{0}') AND l.userId = '{1}'"
                                , x.loanDate, x.user.id);
        double amount = 0;
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        amount = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        return amount;
    }

    public double GetTotalLoanRepayed(NewLoan x) {
        string sql = string.Format(@"SELECT SUM(CONVERT(decimal, a.amount)) FROM Account a LEFT OUTER JOIN Loan l ON l.id = a.loanId
                                    WHERE CONVERT(datetime, l.loanDate) <= CONVERT(datetime, '{0}') AND (a.recordType = '{1}' OR a.recordType = '{2}') AND a.userId = '{3}'"
                                    , x.loanDate, g.repayment, g.loan, x.user.id);
        double amount = 0;
        using (SqlConnection connection = new SqlConnection(g.connectionString)) {
            connection.Open();
            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        amount = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetDecimal(0));
                    }
                }
            }
            connection.Close();
        }
        return amount;
    }
    // public double GetTotalLoan(string loanDate) {
    //    string sql = string.Format(@"SELECT SUM(CONVERT(decimal, l.loan)) FROM Loan l WHERE CONVERT(datetime, l.loanDate) < CONVERT(datetime, '{0}') ", loanDate);
    //    double x = 0;
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

    //public double GetTotalLoanRepayed(string loanDate) {
    //    string sql = string.Format(@"SELECT SUM(CONVERT(decimal, a.amount)) FROM Account a
    //                                LEFT OUTER JOIN Loan l ON l.id = a.loanId
    //                                WHERE CONVERT(datetime, l.loanDate) < CONVERT(datetime, '{0}') AND a.recordType = '{1}'", loanDate, g.repayment);
    //    double x = 0;
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

}
