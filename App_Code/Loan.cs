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
[WebService(Namespace = "http://janaf.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Loan : System.Web.Services.WebService {
    string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
    DataBase db = new DataBase();
    Global g = new Global();
    double manipulativeCostsCoeff = Convert.ToDouble(ConfigurationManager.AppSettings["manipulativeCostsCoeff"]);
    double defaultDedline = Convert.ToDouble(ConfigurationManager.AppSettings["defaultDedline"]);  // ***** Months to repayment *****
    public Loan() {
    }

    public class NewLoan {
        public string id;
        public User.NewUser user;
        public double loan;
        public string loanDate;
        public double repayment;
        public double manipulativeCosts;
        public double actualLoan;
        public double dedline;
        public double restToRepayment;  //TODO
        public int isRepaid;
        public string note;
        public double manipulativeCostsCoeff;
        public BuisinessUnit.NewUnit buisinessUnit;
    }

    public class Total {
        public double loan;
        public double repayment;
        public double manipulativeCosts;
        public double actualLoan;
        public double restToRepayment;
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
        x.id = Guid.NewGuid().ToString();
        x.user = new User.NewUser();
        x.loan = 0;
        x.loanDate = null;
        x.repayment = 0;
        x.manipulativeCosts = 0;
        x.actualLoan = 0;
        x.dedline = defaultDedline;
        x.isRepaid = 0;
        x.note = null;
        x.manipulativeCostsCoeff = manipulativeCostsCoeff;
        x.buisinessUnit = new BuisinessUnit.NewUnit();
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string Save(NewLoan x) {
        try {
            db.Loan();
            string sql = string.Format(@"BEGIN TRAN
                                            IF EXISTS (SELECT * from Loan WITH (updlock,serializable) WHERE id = '{0}')
                                                BEGIN
                                                   UPDATE Loan SET userId = '{1}', loan = '{2}', loanDate = '{3}', repayment = '{4}', manipulativeCosts = '{5}', dedline = '{6}', isRepaid = '{7}', note = '{8}' WHERE id = '{0}'
                                                END
                                            ELSE
                                                BEGIN
                                                   INSERT INTO Loan (id, userId, loan, loanDate, repayment, manipulativeCosts, dedline, isRepaid, note)
                                                   VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}')
                                                END
                                        COMMIT TRAN", x.id, x.user.id, x.loan, x.loanDate, x.repayment, x.manipulativeCosts, x.dedline, x.isRepaid, x.note);
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
    public string Load(int? month, int year, string buisinessUnitCode) {
        try {
            db.Loan();
            string sql = string.Format(@"SELECT l.id, l.userId, l.loan, l.loanDate, l.repayment, l.manipulativeCosts, l.dedline, l.isRepaid, l.note, u.firstName, u.lastName, b.code, b.title FROM Loan l
                        LEFT OUTER JOIN Users u
                        ON l.userId = u.id
                        LEFT OUTER JOIN BuisinessUnit b
                        ON u.buisinessUnitCode = b.code
                        WHERE {0} {1}"
                            , string.Format("CONVERT(datetime, l.loanDate) >= '{0}' AND CONVERT(datetime, l.loanDate) < '{1}'", g.ReffDate(month == null ? 1 : month, year), g.ReffDate(month == null ? 12 : month + 1, year))
                            , string.IsNullOrEmpty(buisinessUnitCode) ? "" : string.Format("AND u.buisinessUnitCode = '{0}'", buisinessUnitCode));
            Loans xx = new Loans();
            xx.data = new List<NewLoan>();
            using (SqlConnection connection = new SqlConnection(connectionString)) {
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

            xx.total = new Total();
            xx.total.loan = xx.data.Sum(a => a.loan);
            xx.total.repayment = xx.data.Sum(a => a.repayment);
            xx.total.manipulativeCosts = xx.data.Sum(a => a.manipulativeCosts);
            xx.total.actualLoan = xx.data.Sum(a => a.actualLoan);
            xx.total.restToRepayment = xx.data.Sum(a => a.restToRepayment);

            xx.monthlyTotal = new List<MonthlyTotal>();
            xx.monthlyTotal = GetMonthlyTotal(xx.data);

            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    [WebMethod]
    public string Delete(string id) {
        try {
            string sql = string.Format("DELETE FROM Loan WHERE id = '{0}'", id);
            using (SqlConnection connection = new SqlConnection(connectionString)) {
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

    NewLoan ReadData(SqlDataReader reader) {
        NewLoan x = new NewLoan();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.user = new User.NewUser();
        x.user.id = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.loan = reader.GetValue(2) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(2));
        x.loanDate = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
        x.repayment = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
        x.manipulativeCosts = reader.GetValue(5) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(5));
        x.actualLoan = x.loan - x.manipulativeCosts;
        //TODO:  x.restToRepayment;
        x.dedline = reader.GetValue(6) == DBNull.Value ? defaultDedline : Convert.ToDouble(reader.GetString(6));
        x.isRepaid = reader.GetValue(7) == DBNull.Value ? 0 : reader.GetInt32(7);
        x.note = reader.GetValue(8) == DBNull.Value ? null : reader.GetString(8);
        x.user.firstName = reader.GetValue(9) == DBNull.Value ? null : reader.GetString(9);
        x.user.lastName = reader.GetValue(10) == DBNull.Value ? null : reader.GetString(10);
        x.user.buisinessUnit = new BuisinessUnit.NewUnit();
        x.user.buisinessUnit.code = reader.GetValue(11) == DBNull.Value ? null : reader.GetString(11);
        x.user.buisinessUnit.title = reader.GetValue(12) == DBNull.Value ? null : reader.GetString(12);
        return x;
    }

    public NewLoan GetRecord(string userId, int month, int year) {
        string sql = string.Format("SELECT * FROM Loan WHERE userId = '{0}' AND mo = '{1}' AND yr = '{2}'", userId, month, year);
        NewLoan x = new NewLoan();
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
            x.total.restToRepayment = aa.Sum(a => a.restToRepayment);
            xx.Add(x);
        }
        return xx;
    }

}
