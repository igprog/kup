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
    DataBase db = new DataBase();
    Global g = new Global();
    public BuisinessUnit() { 
    }

    public class NewUnit {
        public string id;
        public string code;
        public string title;
    }

    [WebMethod]
    public string Init() {
        NewUnit x = new NewUnit();
        x.id = Guid.NewGuid().ToString();
        x.code = null;
        x.title = null;
        return JsonConvert.SerializeObject(x, Formatting.Indented);
    }

    [WebMethod]
    public string Save(NewUnit x) {
        try {
            db.BuisinessUnit();
            string sql = string.Format(@"BEGIN TRAN
                                            IF EXISTS (SELECT * from BuisinessUnit WITH (updlock,serializable) WHERE id = '{0}')
                                                BEGIN
                                                   UPDATE BuisinessUnit SET code = '{1}', title = '{2}' WHERE id = '{0}'
                                                END
                                            ELSE
                                                BEGIN
                                                   INSERT INTO BuisinessUnit (id, code, title)
                                                   VALUES ('{0}', '{1}', '{2}')
                                                END
                                        COMMIT TRAN", x.id, x.code, x.title);
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
    public string Load() {
        try {
            db.BuisinessUnit();
            string sql = "SELECT * FROM BuisinessUnit";
            List<NewUnit> xx = new List<NewUnit>();
            NewUnit all = new NewUnit();
            all.code = null;
            all.id = null;
            all.title = g.allUnitsTitle;
            xx.Add(all);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
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

    [WebMethod]
    public string Delete(string id) {
        try {
            string sql = string.Format("DELETE FROM BuisinessUnit WHERE id = '{0}'", id);
            using (SqlConnection connection = new SqlConnection(g.connectionString)) {
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

    NewUnit ReadData(SqlDataReader reader) {
        NewUnit x = new NewUnit();
        x.id = reader.GetValue(0) == DBNull.Value ? null : reader.GetString(0);
        x.code = reader.GetValue(1) == DBNull.Value ? null : reader.GetString(1);
        x.title = reader.GetValue(2) == DBNull.Value ? null : reader.GetString(2);
        return x;
    }

    public NewUnit Get(string code) {
        NewUnit x = new NewUnit();
        try {
            db.BuisinessUnit();
            string sql = string.Format("SELECT * FROM BuisinessUnit WHERE code = '{0}'", code);
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
            if(string.IsNullOrEmpty(code)) {
                x.code = null;
                x.id = null;
                x.title = g.allUnitsTitle;
            }
            return x;
        } catch (Exception e) {
            return x;
        }
    }

}
