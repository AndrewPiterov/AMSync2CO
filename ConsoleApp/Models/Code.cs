using Newtonsoft.Json;

namespace ConsoleApp.Models
{
    public class Code
    {
        [JsonProperty("Code")]
        public string CodeCode { get; set; }

        [JsonProperty("File")]
        public object File { get; set; }

        [JsonProperty("Description")]
        public object Description { get; set; }

        [JsonProperty("ExtraInfo")]
        public object ExtraInfo { get; set; }
    }
}