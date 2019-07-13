using System;
using System.Configuration;
using System.Data.SqlClient;

/// <summary>
/// DataBase
/// </summary>
namespace Igprog {
    public class DataBase {
        string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

        public DataBase() {
        }

        #region TableSQL
        public void Users() {
            string tbl = "Users";
            string sql = string.Format(@"{0}
                        CREATE TABLE {1}
                        (id NVARCHAR (50) PRIMARY KEY,
                        buissinesUnitCode NVARCHAR (50),
                        firstName NVARCHAR (50),
                        lastName NVARCHAR (50),
                        pin NVARCHAR (50),
                        birthDate NVARCHAR (50),
                        accessDate NVARCHAR (50),
                        terminationDate NVARCHAR (50),
                        isActive INTEGER,
                        monthlyPayment NVARCHAR (50))", CheckTbl(tbl), tbl);
            CreateTable(sql);
        }

        public void BuisinessUnit() {
            string tbl = "BuisinessUnit";
            string sql = string.Format(@"{0}
                        CREATE TABLE {1}
                        (id NVARCHAR (50),
                        code NVARCHAR (50) PRIMARY KEY,
                        title NVARCHAR (50))", CheckTbl(tbl), tbl);
            CreateTable(sql);
        }

        public void Account() {
            string tbl = "Account";
            string sql = string.Format(@"{0}
                        CREATE TABLE {1}
                        (id NVARCHAR (50) PRIMARY KEY,
                        userId NVARCHAR (50),
                        mo INTEGER,
                        yr INTEGER,
                        input NVARCHAR (50),
                        loan NVARCHAR (50),
                        loanDate NVARCHAR (50),
                        repayment NVARCHAR (50),
                        repaymentDate NVARCHAR (50),
                        restToRepayment NVARCHAR (50),
                        accountBalance NVARCHAR (50),
                        note NVARCHAR (50))", CheckTbl(tbl), tbl);
            CreateTable(sql);
        }

        public void Loan() {
            string tbl = "Loan";
            string sql = string.Format(@"{0}
                        CREATE TABLE {1}
                        (id NVARCHAR (50) PRIMARY KEY,
                        userId NVARCHAR (50),
                        loan NVARCHAR (50),
                        loanDate NVARCHAR (50),
                        repayment NVARCHAR (50),
                        manipulativeCosts NVARCHAR (50),
                        dedline NVARCHAR (50),
                        note NVARCHAR (50))", CheckTbl(tbl), tbl);
            CreateTable(sql);
        }
        #endregion

        #region Methods
        private string CheckTbl(string tbl) {
            return string.Format("IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{0}' and xtype='U')", tbl);
        }

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
        #endregion

    }

}
