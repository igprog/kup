using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Igprog;


/// <summary>
/// Admin
/// </summary>
[WebService(Namespace = "http://janaf.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
 [System.Web.Script.Services.ScriptService]
public class Admin : System.Web.Services.WebService {
    string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
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
            using (SqlConnection connection = new SqlConnection(connectionString)) {
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


}
