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

        public string ReffDate(int month, int year) {
            int nextMonth = month == 12 ? 1 : month + 1;
            string month_ = nextMonth < 10 ? string.Format("0{0}", nextMonth) : nextMonth.ToString();
            int year_ = month == 12 ? year + 1 : year;
            return string.Format("{0}-{1}-01", year_, month_);
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
            return string.Format("{0:N} {1}", value.ToString(), currency);
        }

        public string Month(int x) {
           return x < 10 ? string.Format("0{0}", x) : x.ToString();
        }
    }
}