using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Configuration;


/// <summary>
/// Global
/// </summary>
namespace Igprog {
    public class Global {
        public Global() {
        }

        public string connectionString = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
        public double manipulativeCostsCoeff = Convert.ToDouble(ConfigurationManager.AppSettings["manipulativeCostsCoeff"]);
        public double defaultDedline = Convert.ToDouble(ConfigurationManager.AppSettings["defaultDedline"]);  // ***** Broj mjeseci za otplatu rata pozajmice *****
        public double startAccountBalance = Convert.ToDouble(ConfigurationManager.AppSettings["startAccountBalance"]);  // ***** Pocetno stanje na racunu *****

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

        public string Currency(double value) {
            string currency = ConfigurationManager.AppSettings["currency"];
            return string.Format("{0:N} {1}", value, currency);
        }

        public string manipulativeCostsPerc() {
            double manipulativeCostsCoeff = Convert.ToDouble(ConfigurationManager.AppSettings["manipulativeCostsCoeff"]);
            return string.Format("{0}", Math.Round(manipulativeCostsCoeff * 100), 0);
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