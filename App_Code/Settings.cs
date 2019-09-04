using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.IO;
using Newtonsoft.Json;
using Igprog;

/// <summary>
/// Settings
/// </summary>
[WebService(Namespace = "http://janaf.hr/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Settings : System.Web.Services.WebService {
    string path = "~/App_Data/json/settings.json";
    public Settings() { 
    }

    public class NewSettings {
        public double startAccountBalance;
        public string currency;
        public double monthlyFee;
        public double manipulativeCostsCoeff;
        public int defaultDedline;
    }

    public NewSettings Data() {
        return GetSettings();
    }

    [WebMethod]
    public string Load() {
        try {
            return JsonConvert.SerializeObject(GetSettings());
        } catch (Exception e) { return ("Error: " + e); }
    }

    [WebMethod]
    public string Save(string x) {
        try {
            WriteFile(path, x);
            return Load();
        } catch(Exception e) { return ("Error: " + e); }
    }

    protected void WriteFile(string path, string value) {
        File.WriteAllText(Server.MapPath(path), value);
    }

    public NewSettings GetSettings() {
        NewSettings x = new NewSettings();
        string json = ReadFile();
        return JsonConvert.DeserializeObject<NewSettings>(json);
    }

    public string ReadFile() {
        if (File.Exists(Server.MapPath(path))) {
            return File.ReadAllText(Server.MapPath(path));
        } else {
            return null;
        }
    }

}
