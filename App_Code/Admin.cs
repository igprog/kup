using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Configuration;
using Newtonsoft.Json;


/// <summary>
/// Admin
/// </summary>
[WebService(Namespace = "http://janaf.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
 [System.Web.Script.Services.ScriptService]
public class Admin : System.Web.Services.WebService {
    public Admin() {
    }

    [WebMethod]
    public bool Login(string username, string password) {
        string supervisorUserName = ConfigurationManager.AppSettings["AdminUserName"];
        string supervisorPassword = ConfigurationManager.AppSettings["AdminPassword"];
        if (username == supervisorUserName && password == supervisorPassword) {
            return true;
        } else {
            return false;
        }
    }


}
