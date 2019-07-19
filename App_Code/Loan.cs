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
        public int isRepaid;
        public string note;
        public double manipulativeCostsCoeff;
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
    public string Load() {
        try {
            db.Loan();
            string sql = "SELECT * FROM Loan";
            List<NewLoan> xx = new List<NewLoan>();
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewLoan x = ReadData(reader);
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
        x.dedline = reader.GetValue(6) == DBNull.Value ? defaultDedline : Convert.ToDouble(reader.GetString(6));
        x.isRepaid = reader.GetValue(7) == DBNull.Value ? 0 : reader.GetInt32(7);
        x.note = reader.GetValue(8) == DBNull.Value ? null : reader.GetString(8);
        return x;
    }

}
