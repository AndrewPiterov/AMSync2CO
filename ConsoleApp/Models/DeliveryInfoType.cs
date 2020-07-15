using Newtonsoft.Json;

namespace ConsoleApp.Models
{
    public class DeliveryInfoType
    {
        [JsonProperty("Description")]
        public object Description { get; set; }

        [JsonProperty("Codes")]
        public Code[] Codes { get; set; }
    }
}