using Newtonsoft.Json;

namespace ConsoleApp.Models
{
    public class AvangateApiError
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }
    }
}