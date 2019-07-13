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
/// Users
/// </summary>
[WebService(Namespace = "http://janaf.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Users : System.Web.Services.WebService {
    string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
    DataBase db = new DataBase();
    double monthlyPayment = Convert.ToDouble(ConfigurationManager.AppSettings["monthlyPayment"]);
    public Users() {
    }

    public class NewUser {
        public string id;
        public string buissinesUnitCode;
        public string firstName;
        public string lastName;
        public string pin;
        public string birthDate;
        public string accessDate;
        public string terminationDate;
        public int isActive;
        public double monthlyPayment;
    }

    [WebMethod]
    public string Init() {
        NewUser x = new NewUser();
        x.id = null;
        x.buissinesUnitCode = null;
        x.firstName = null;
        x.lastName = null;
        x.pin = null;
        x.birthDate = null;
        x.accessDate = DateTime.Now.ToShortDateString();
        x.terminationDate = null;
        x.isActive = 1;
        x.monthlyPayment = monthlyPayment;
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string Save(NewUser x) {
        try {
            db.Users();
            string sql = string.Format(@"INSERT INTO Users VALUES  
                       ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')"
                        , x.id, x.buissinesUnitCode, x.firstName, x.lastName, x.pin, x.birthDate, x.accessDate, x.terminationDate, x.isActive, x.monthlyPayment);
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
            return JsonConvert.SerializeObject(GetUsers(), Formatting.Indented);
        }catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    NewUser ReadData(SqlDataReader reader) {
        NewUser x = new NewUser();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.buissinesUnitCode = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.firstName = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
        x.lastName = reader.GetValue(3) == DBNull.Value ? null : reader.GetString(3);
        x.pin = reader.GetValue(4) == DBNull.Value ? null : reader.GetString(4);
        x.birthDate = reader.GetValue(5) == DBNull.Value ? null : reader.GetString(5);
        x.accessDate = reader.GetValue(6) == DBNull.Value ? null : reader.GetString(6);
        x.terminationDate = reader.GetValue(7) == DBNull.Value ? null : reader.GetString(7);
        x.isActive = reader.GetValue(8) == DBNull.Value ? 1 : reader.GetInt32(8);
        x.monthlyPayment = reader.GetValue(9) == DBNull.Value ? 0 : Convert.ToDouble(reader.GetString(9));
        return x;
    }

    public List<NewUser> GetUsers() {
        db.Users();
        string sql = "SELECT * FROM Users";
        List<NewUser> xx = new List<NewUser>();
        using (SqlConnection connection = new SqlConnection(connectionString)) {
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

}
