using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpecMaker
{
    public class AppSettingsSchema
    {
        //class for application settings to be read from config (.json) like supported persons, paths to excel files...
        public string[] SupportedPersons { get; set; }
        public string SpecExcelTemplatePath { get; set; }
        
        [JsonIgnore]
        public static string PathToAppSettings { get; } = Path.Join("Settings", "AppSettings.json");

    }
}
