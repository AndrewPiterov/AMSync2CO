using System.Linq;
using Newtonsoft.Json;

namespace ConsoleApp.Models
{
    public class ProductType
    {
        [JsonProperty("ProductCode")]
        public string ProductCode { get; set; }

        [JsonProperty("ProductId")]
        public string ProductId { get; set; }

        [JsonProperty("ProductName")]
        public string ProductName { get; set; }

        [JsonProperty("ProductVersion")]
        public string ProductVersion { get; set; }

        [JsonProperty("ProductQuantity")]
        public long ProductQuantity { get; set; }

        public int RealQty
        {
            get
            {
                return PriceOptionCodes.Where(c => c?.StartsWith("cmpqty=") == true)
                    .Select(c => c.Substring("cmpqty=".Length, c.Length - "cmpqty=".Length))
                    .Select(int.Parse)
                    .Single();
            }
        }

        [JsonProperty("PriceOptionCodes")]
        public string[] PriceOptionCodes { get; set; }
    }
}