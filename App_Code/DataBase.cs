using System;
using System.Web;
using System.Configuration;
using System.IO;
using System.Data;
using System.Data.SqlClient;

/// <summary>
/// DataBase
/// </summary>
namespace Igprog {
    public class DataBase {
        //string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
        string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

        public DataBase() {
        }

        #region CreateTable
        public void Users() {
            string sql = string.Format(@"{0})
                        CREATE TABLE Users
                        (id NVARCHAR (50) PRIMARY KEY,
                        buissinesUnitCode NVARCHAR (50),
                        firstName NVARCHAR (50),
                        lastName NVARCHAR (50),
                        pin NVARCHAR (50),
                        birthDate NVARCHAR (50),
                        accessDate NVARCHAR (50),
                        terminationDate NVARCHAR (50),
                        isActive INTEGER,
                        monthlyPayment NVARCHAR (50))", CheckTbl("Users"));
            CreateTable(sql);
        }

        public void BuisinessUnit() {
            string sql = string.Format(@"{0})
                        CREATE TABLE BuisinessUnit
                        (id NVARCHAR (50),
                        code NVARCHAR (50) PRIMARY KEY,
                        title NVARCHAR (50))", CheckTbl("BuisinessUnit"));
            CreateTable(sql);
        }

        private string CheckTbl(string tbl) {
            return string.Format("IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{0}' and xtype='U'", tbl);
        }
        #endregion


        private void CreateTable(string sql) {
            try{
                using (SqlConnection connection = new SqlConnection(connectionString)) {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(sql, connection)) {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception e) { }
        }




        //public void CreateDataBase(string userId, string table) {
        //    try {
        //        string path = GetDataBasePath(userId, dataBase);
        //        string dir = Path.GetDirectoryName(path);
        //        if (!Directory.Exists(dir)) {
        //            Directory.CreateDirectory(dir);
        //        }
        //        if (!File.Exists(path)) {
        //            SQLiteConnection.CreateFile(path);
        //        }
        //        CreateTables(table, path);
        //    } catch (Exception e) { }
        //}

        //public void CreateGlobalDataBase(string path, string table) {
        //    try {
        //        string dir = Path.GetDirectoryName(path);
        //        if (!Directory.Exists(dir)) {
        //            Directory.CreateDirectory(dir);
        //        }
        //        if (!File.Exists(path)) {
        //            SQLiteConnection.CreateFile(path);
        //        }
        //        CreateTables(table, path);
        //    } catch (Exception e) { }
        //}

        //private void CreateTables(string table, string path) {
        //    switch (table) {
        //        case "users":
        //            Users(path);
        //            break;
        //        default:
        //            break;
        //    }
        //}

        //private void CreateTable(string path, string sql) {
        //    try {
        //        if (File.Exists(path)){
        //            //SQLiteConnection connection = new SQLiteConnection("Data Source=" + path);
        //            SqlConnection connection = new SqlConnection(connectionString);
        //            connection.Open();
        //            SqlCommand command = new SqlCommand(sql, connection);
        //            //SQLiteCommand command = new SQLiteCommand(sql, connection);
        //            command.ExecuteNonQuery();
        //            connection.Close();
        //        };
        //    } catch (Exception e) { }
        //}

        //public string GetDataBasePath(string userId, string dataBase) {
        //    return HttpContext.Current.Server.MapPath("~/App_Data/users/" + userId + "/" + dataBase);
        //}

        //public void AddColumn(string userId, string path, string table, string column) {
        //    if(!CheckColumn(userId, table, column)) {
        //        string sql = string.Format("ALTER TABLE {0} ADD COLUMN {1} VARCHAR (50)", table, column);
        //        CreateTable(path, sql);
        //    }
        //}

        /************** Check if column exists ***********/
        //private bool CheckColumn(string userId, string table, string column) {
        //    try {
        //        string dataBase = ConfigurationManager.AppSettings["UserDataBase"];
        //        DataBase db = new DataBase();
        //        bool exists = false;
        //        string name = null;
        //        SQLiteConnection connection = new SQLiteConnection("Data Source=" + db.GetDataBasePath(userId, dataBase));
        //        connection.Open();
        //        string sql = string.Format("pragma table_info('{0}')", table);
        //        SQLiteCommand command = new SQLiteCommand(sql, connection);
        //        SQLiteDataReader reader = command.ExecuteReader();
        //        while (reader.Read()) {
        //            name = reader.GetValue(1) == DBNull.Value ? "" : reader.GetString(1);
        //            if (name == column) {
        //                exists = true;
        //            }
        //        }
        //        connection.Close();
        //        return exists;
        //    } catch (Exception e) { return false; }
        //}
        /*************************************************/

    }

}
