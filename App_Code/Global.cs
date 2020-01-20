using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
using Ionic.Zip;
using Igprog;


/// <summary>
/// Global
/// </summary>
namespace Igprog {
    public class Global {
        Settings s = new Settings();
        public Global() {
        }

        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;

        #region RecordType
        public string repayment = "repayment";
        public string manipulativeCosts = "manipulativeCosts";
        public string withdraw = "withdraw";
        public string monthlyFee = "monthlyFee";
        public string bankFee = "bankFee";
        public string interest = "interest";
        public string otherFee = "otherFee";
        public string terminationWithdraw = "terminationWithdraw";
        public string userPayment = "userPayment";
        public string userRepayment = "userRepayment";
        #endregion RecordType
        public string giroaccount = "giroaccount";
        public string loan = "loan";  // otplata novom pozajmicom ?
        public string income = "income";
        public string expense = "expense";
        public string incomeExpenseDiff = "incomeExpenseDiff";
        public string entry_I = "entry_I";
        public string entry_II = "entry_II";
        public string allUnitsTitle = "JANAF ukupno";

        #region Date
        public string ReffDate(int? month, int year) {
            if(month == 13) {
                month = 1;
                year = year + 1;
            }
            string month_ = month < 10 ? string.Format("0{0}", month) : month.ToString();
            return string.Format("{0}-{1}-01", year, month_);
        }

        public string Date(DateTime date) {
            int day = date.Day;
            int month = date.Month;
            int year = date.Year;
            return SetDate(day, month, year);
        }

        public string SetDate(int day, int month, int year){
            string day_ = day < 10 ? string.Format("0{0}", day) : day.ToString();
            string month_ = month < 10 ? string.Format("0{0}", month) : month.ToString();
            return string.Format("{0}-{1}-{2}", year, month_, day_);
        }

        public string SetDayMonthDate(int day, int month){
            string day_ = day < 10 ? string.Format("0{0}", day) : day.ToString();
            string month_ = month < 10 ? string.Format("0{0}", month) : month.ToString();
            return string.Format("{0}.{1}", day_, month_);
        }

        public int GetLastDayInMonth(int year, int month) {
            return Convert.ToInt32(DateTime.DaysInMonth(year, month));
        }

        public string LastDayInMonth(int year, int month) {
            return SetDayMonthDate(GetLastDayInMonth(year, month), month);
        }

        public string ConvertDate(string date) {
            string date_ = null;
            if(!string.IsNullOrEmpty(date)) {
                date_ = SetDate(Convert.ToInt32(date.Substring(0, 2)), Convert.ToInt32(date.Substring(3, 2)), Convert.ToInt32(date.Substring(6, 4)));
            }
            return date_;
        }

        public string Month(int month) {
           return month < 10 ? string.Format("0{0}", month) : month.ToString();
        }

        public string GetMonth(string date) {
            return date.Substring(5, 2);
        }

        public string GetYear(string date) {
            return date.Substring(0, 4);
        }

        public int DateDiff(DateTime from, DateTime to, bool abs) {
            try {
                return Convert.ToInt32(abs == true ? Math.Abs((from - to).TotalDays) : (from - to).TotalDays);
            } catch (Exception e) {
                return 0;
            }
        }

        public int DateDiff(string from, string to, bool abs) {
            try {
                DateTime from_ = Convert.ToDateTime(from);
                DateTime to2_ = Convert.ToDateTime(to);
                return DateDiff(from_, to2_, abs);
            } catch (Exception e) {
                return 0;
            }
        }

        public int DateDiff(string date) {
            try {
                return DateDiff(Convert.ToDateTime(date), DateTime.UtcNow, true);
            } catch (Exception e) {
                return 0;
            }
        }
        #endregion Date

        public void CreateFolder(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        public void DeleteFolder(string path) {
            if (Directory.Exists(path)) {
                Directory.Delete(path, true);
            }
        }

        public string Format(double value) {
            return string.Format("{0:N}", value);
        }

        public string Currency(double value) {
            return string.Format("{0} {1}", Format(value), s.Data().currency);
        }

        public string manipulativeCostsPerc() {
            return string.Format("{0}", Math.Round(s.Data().manipulativeCostsCoeff * 100), 0);
        }

        public double ConvertToDouble(string val) {
            double val_ = 0;
            if (!string.IsNullOrWhiteSpace(val)) {
                val_ = Convert.ToDouble(val);
            }
            return val_;
        }

        public string Zip(string fileName) {
            try {
                string[] filePaths = Directory.GetFiles(HttpContext.Current.Server.MapPath("~/upload/pdf/temp/cards/"));
                using (ZipFile zip = new ZipFile()) {
                    foreach (string filePath in filePaths) {
                        zip.AddFile(filePath);
                    }
                    zip.Save(HttpContext.Current.Server.MapPath(string.Format("~/upload/pdf/temp/{0}.zip", fileName)));
                }
                return string.Format("{0}.zip", fileName);
            } catch(Exception e) {
                return e.Message;
            }
            
        }

    }
}