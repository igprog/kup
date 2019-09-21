using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;
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
        #endregion RecordType
        public string giroaccount = "giroaccount";
        public string allUnitsTitle = "JANAF ukupno";

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
            //return string.Format("{0:N} {1}", value, s.Data().currency);
        }

        public string manipulativeCostsPerc() {
            return string.Format("{0}", Math.Round(s.Data().manipulativeCostsCoeff * 100), 0);
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

    }
}