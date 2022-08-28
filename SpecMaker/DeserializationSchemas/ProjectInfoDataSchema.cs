using System.IO;
using System.Text.Json.Serialization;

namespace SpecMaker
{
    public record class ProjectInfoDataSchema
    {
        public string Designer { get; set; }
        public string Checker { get; set; }
        public string TeamLead { get; set; }
        public string NormController { get; set; }
        public string Approver { get; set; }
        public string Customer { get; set; }
        public string Plant { get; set; }
        public string Stage { get; set; }
        public string ProjectCode { get; set; }
        public string Date { get; set; }

        [JsonIgnore]
        public static readonly string ProjectInfoFilePath = Path.Join("Settings", "ProjectInfoData.json");
    }
}
