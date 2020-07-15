using Newtonsoft.Json;

namespace ConsoleApp.Models
{
    public class Subscription
    {
        [JsonProperty("SubscriptionReference")]
        public string SubscriptionReference { get; set; }

        [JsonProperty("StartDate")]
        public string StartDate { get; set; }

        [JsonProperty("ExpirationDate")]
        public string ExpirationDate { get; set; }

        [JsonProperty("RecurringEnabled")]
        public bool RecurringEnabled { get; set; }

        [JsonProperty("SubscriptionEnabled")]
        public bool SubscriptionEnabled { get; set; }

        [JsonProperty("Product")]
        public ProductType Product { get; set; }

        [JsonProperty("EndUser")]
        public EndUserType EndUser { get; set; }

        [JsonProperty("SKU")]
        public string Sku { get; set; }

        [JsonProperty("DeliveryInfo")]
        public DeliveryInfoType DeliveryInfo { get; set; }

        [JsonProperty("ReceiveNotifications")]
        public bool ReceiveNotifications { get; set; }

        [JsonProperty("Lifetime")]
        public bool Lifetime { get; set; }

        [JsonProperty("PartnerCode")]
        public string PartnerCode { get; set; }

        [JsonProperty("AvangateCustomerReference")]
        public string AvangateCustomerReference { get; set; }

        [JsonProperty("ExternalCustomerReference")]
        public object ExternalCustomerReference { get; set; }

        [JsonProperty("TestSubscription")]
        public bool TestSubscription { get; set; }

        [JsonProperty("IsTrial")]
        public bool IsTrial { get; set; }
    }
}