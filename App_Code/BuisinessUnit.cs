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
/// BuisinessUnit
/// </summary>
[WebService(Namespace = "http://janaf.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class BuisinessUnit : System.Web.Services.WebService {
    string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
    DataBase db = new DataBase();
    public BuisinessUnit() { 
    }

    public class NewUnit {
        public string id;
        public string title;
    }

    [WebMethod]
    public string Init() {
        NewUnit x = new NewUnit();
        x.id = null;
        x.title = null;
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string Save(NewUnit x) {
        try {
            db.BuisinessUnit();
            //TODO insert or update
            string sql = string.Format(@"INSERT INTO BuisinessUnit VALUES ('{0}', '{1}')", x.id, x.title);
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
            string sql = "SELECT * FROM BuisinessUnit";
            List<NewUnit> xx = new List<NewUnit>();
            using (SqlConnection connection = new SqlConnection(connectionString)) {
                connection.Open();
                using (SqlCommand command = new SqlCommand(sql, connection)) {
                    using (SqlDataReader reader = command.ExecuteReader()) {
                        while (reader.Read()) {
                            NewUnit x = ReadData(reader);
                            xx.Add(x);
                        }
                    }
                }
                connection.Close();
            }
            return JsonConvert.SerializeObject(xx, Formatting.Indented);
        }catch (Exception e) {
            return JsonConvert.SerializeObject(e.Message, Formatting.Indented);
        }
    }

    NewUnit ReadData(SqlDataReader reader) {
        NewUnit x = new NewUnit();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.title = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        return x;
    }

}
