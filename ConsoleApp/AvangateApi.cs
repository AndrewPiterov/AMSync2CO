using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApp.Models;
using EnsureThat;
using Newtonsoft.Json;
using Polly;

namespace ConsoleApp
{
    internal sealed class AvangateApi
    {
        public AvangateApi(IHashUtils hashUtils, AvangateSettings avangateSettings)
        {
            EnsureArg.IsNotNull(hashUtils, nameof(hashUtils));
            EnsureArg.IsNotNull(avangateSettings, nameof(avangateSettings));

            this.hashUtils = hashUtils;
            this.avangateSettings = avangateSettings;

            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMediaType));
        }

        private const string JsonMediaType = "application/json";
        private const string PartnerCoupon = "&COUPON=PARTNER35";
        private const string AgCompaniesOptionTemplate = "cmpqty={0}";
        private const string CleanCartParameter = "CLEAN_CART=1";
        private const string AgSubscriptionEndpointTemplate = "https://api.2checkout.com/rest/5.0/subscriptions/{0}/";
        private const string AgSubscriptionHistoryEndpointTemplate = "https://api.2checkout.com/rest/5.0/subscriptions/{0}/history/";
        private const string AgOrderEndpointTemplate = "https://api.2checkout.com/rest/5.0/orders/{0}/";
        private const string AgCustomerEndpointTemplate = "https://api.2checkout.com/rest/5.0/customers/{0}/";
        private const string AgCustomerDetailsEndpointTemplate = "https://api.2checkout.com/rest/5.0/customers/{0}/?endUserUpdate=true";
        private const string AgPaymentInfoEndpointTemplate = "https://api.2checkout.com/rest/5.0/subscriptions/{0}/payment/";
        private const string AgNextRenewalPriceEndpointTemplate = "https://api.2checkout.com/rest/5.0/subscriptions/{0}/renewal/price/{1}/";
        private const string AgInvoiceEndpointTemplate = "https://api.2checkout.com/rest/5.0/invoices/{0}/";
        private const string AgRenewalNotificationsEndpointTemplate = "https://api.2checkout.com/rest/5.0/subscriptions/{0}/renewal/notification/";
        private const string AgRenewalEndpointTemplate = "https://api.2checkout.com/rest/5.0/subscriptions/{0}/renewal/";

        private const string MyAccountRenewalEndpoint = "https://myaccount.approvalmax.com/renewal/";

        private static readonly HttpClient HttpClient = new HttpClient();
        private readonly AvangateSettings avangateSettings;

        private readonly IHashUtils hashUtils;

        public async Task<Subscription> FetchSubscriptionAsync(string id, CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNullOrWhiteSpace(id, nameof(id));

            var url = string.Format(AgSubscriptionEndpointTemplate, id);
            var responseJson = await this.MakeGetAvangateApiCallAsync(url, cancellationToken).ConfigureAwait(false);

            var subscription = JsonConvert.DeserializeObject<Subscription>(responseJson);
            return subscription;
        }

        public async Task UpdateSubscriptionAsync(
            Subscription subscription,
            int newCompaniesCount,
            string newProductName,
            string newProductId,
            CancellationToken cancellationToken)
        {
            EnsureArg.IsNotNull(subscription, nameof(subscription));
            EnsureArg.IsGte(newCompaniesCount, 1, nameof(newCompaniesCount));

            var url = string.Format(AgSubscriptionEndpointTemplate, subscription.SubscriptionReference);
            var priceOption = string.Format(AgCompaniesOptionTemplate, newCompaniesCount);
            subscription.Product.PriceOptionCodes = new [] {priceOption};
            if (!string.IsNullOrWhiteSpace(newProductName) && !string.IsNullOrWhiteSpace(newProductId))
            {
                subscription.Product.ProductName = newProductName;
                subscription.Product.ProductId = newProductId;
            }

            await this.MakeAvangateApiCallAsync(
                url,
                HttpMethod.Put,
                cancellationToken,
                subscription).ConfigureAwait(false);
        }


        private async Task<string> MakeAvangateApiCallAsync(
            string url,
            HttpMethod method,
            CancellationToken cancellationToken,
            object payload = default)
        {
            HttpResponseMessage responseMessage;

            try
            {
                responseMessage = await Policy
                    .Handle<HttpRequestException>()
                    .OrResult<HttpResponseMessage>(response => response.StatusCode != HttpStatusCode.OK)
                    .WaitAndRetryAsync(
                        new[]
                        {
                            TimeSpan.FromMilliseconds(200),
                            TimeSpan.FromMilliseconds(200),
                            TimeSpan.FromMilliseconds(200)
                        })
                    .ExecuteAsync(
                        ct => HttpClient.SendAsync(
                            CreateHttpRequestMessage(
                                url,
                                method,
                                payload,
                                this.avangateSettings.HmacKey,
                                this.avangateSettings.MerchantCode),
                            ct),
                        cancellationToken);
            }
            catch (HttpRequestException exception)
            {
                throw new InvalidOperationException($"Failed to {method} {url} AG API Error.", exception);
            }

            if (responseMessage.Content.Headers.ContentType.MediaType == JsonMediaType)
            {
                var responseJson = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    return responseJson;
                }

                if (responseMessage.StatusCode == HttpStatusCode.BadRequest)
                {
                    var error = JsonConvert.DeserializeObject<AvangateApiError>(responseJson);
                    throw new AvangateApiErrorException(
                        $"Failed to {method} {url} AG API error code {error.ErrorCode}, message {error.Message}",
                        method.ToString(),
                        url,
                        error);
                }
            }

            throw new InvalidOperationException(
                $"Failed to {method} {url} AG API Access Error Code {responseMessage.StatusCode}");
        }

        private async Task<string> MakeGetAvangateApiCallAsync(
            string url,
            CancellationToken cancellationToken,
            object payload = default)
        {
            return await this.MakeAvangateApiCallAsync(
                url,
                HttpMethod.Get,
                cancellationToken,
                payload);
        }

        private static HttpRequestMessage CreateHttpRequestMessage(
            string url,
            HttpMethod method,
            object payload,
            string hmacKey,
            string merchantCode)
        {
            var httpRequestMessage = new HttpRequestMessage(method, url) { Headers = { { "Accept", JsonMediaType } } };
            var authHeaderValue = MakeAuthHeader(hmacKey, merchantCode);
            httpRequestMessage.Headers.Add("X-Avangate-Authentication", authHeaderValue);

            if (payload != null)
            {
                var jsonPayload = JsonConvert.SerializeObject(payload);
                httpRequestMessage.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            }

            return httpRequestMessage;
        }

        private static string MakeAuthHeader(string hmacKey, string merchantCode)
        {
            var date = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            var hash = HashUtils.CalcHmac($"{merchantCode.Length}{merchantCode}{date.Length}{date}", hmacKey);

            return $"code=\"{merchantCode}\" date=\"{date}\" hash=\"{hash}\"";
        }
    }
}