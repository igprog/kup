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
/// Entry
/// </summary>
[WebService(Namespace = "http://janaf.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Entry : System.Web.Services.WebService {
    string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
    DataBase db = new DataBase();

    public Entry() {
    }

    public class NewEntry {
        public double monthlyFee;
        public double loan;
        public double repayment;
        public double repaid;
        public double restToRepayment;
        public double totalObligation;
        public double manipulativeCosts;
    }

    public class Entries {
        public List<NewEntry> data;
        public NewEntry total;
    }

    [WebMethod]
    public string Load(int month, int year) {
        try {
            db.Account();
            Entries xx = new Entries();
            xx.data = new List<NewEntry>();
            string sql = string.Format(@"SELECT a.monthlyFee, a.loan, a.repayment, a.repaid, a.restToRepayment, l.manipulativeCosts FROM account a
                                        LEFT OUTER JOIN Loan l 
                                        ON a.loanId = l.id
                                        WHERE a.mo = {0} and a.yr = {1}
                                        GROUP BY a.monthlyFee, a.loan, a.repayment, a.repaid, a.restToRepayment, l.manipulativeCosts", month, year);
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewEntry x = ReadData(reader);
                            xx.data.Add(x);
                        }
                    }
                }
                connection.Close();
            }
            xx.total = new NewEntry();
            xx.total.monthlyFee = xx.data.Sum(a => a.monthlyFee);
            xx.total.loan = xx.data.Sum(a => a.loan);
            xx.total.repayment = xx.data.Sum(a => a.repayment);
            xx.total.repaid = xx.data.Sum(a => a.repaid);
            xx.total.restToRepayment = xx.data.Sum(a => a.restToRepayment);
            xx.total.totalObligation = xx.data.Sum(a => a.totalObligation);
            xx.total.manipulativeCosts = xx.data.Sum(a => a.manipulativeCosts);
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        } catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }


    NewEntry ReadData(SqlDataReader reader) {
        NewEntry x = new NewEntry();
        x.monthlyFee = reader.GetValue(0) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(0));
        x.loan = reader.GetValue(1) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(1));
        x.repayment = reader.GetValue(2) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(2));
        x.repaid = reader.GetValue(3) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(3));
        x.restToRepayment = reader.GetValue(4) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(4));
        x.totalObligation = x.monthlyFee + x.repaid;
        x.manipulativeCosts = reader.GetValue(5) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(5));
        return x;
    }
}
