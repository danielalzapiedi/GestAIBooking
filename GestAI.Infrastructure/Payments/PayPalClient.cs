using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GestAI.Infrastructure.Payments
{
    public class PayPalClient
    {
        private readonly HttpClient _http;
        private readonly PayPalOptions _options;

        public PayPalClient(HttpClient http, PayPalOptions options)
        {
            _http = http;
            _options = options;
            _http.BaseAddress = new Uri(_options.Environment?.ToLowerInvariant() == "live"
                ? "https://api-m.paypal.com/"
                : "https://api-m.sandbox.paypal.com/");
        }

        private async Task<string> GetAccessTokenAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Post, "v1/oauth2/token");
            var byteArray = Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.Secret}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            req.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString()!;
        }

        public async Task<JsonDocument> GetSubscriptionAsync(string subscriptionId)
        {
            var token = await GetAccessTokenAsync();
            var req = new HttpRequestMessage(HttpMethod.Get, $"v1/billing/subscriptions/{subscriptionId}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var json = await res.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json);
        }

        public async Task<bool> VerifyWebhookAsync(string transmissionId, string transmissionTime, string certUrl, string authAlgo, string transmissionSig, string eventBody)
        {
            var token = await GetAccessTokenAsync();
            var payload = new
            {
                auth_algo = authAlgo,
                cert_url = certUrl,
                transmission_id = transmissionId,
                transmission_sig = transmissionSig,
                transmission_time = transmissionTime,
                webhook_id = _options.WebhookId,
                webhook_event = JsonSerializer.Deserialize<JsonElement>(eventBody)
            };

            var json = JsonSerializer.Serialize(payload);
            var req = new HttpRequestMessage(HttpMethod.Post, "v1/notifications/verify-webhook-signature");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var res = await _http.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var resJson = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(resJson);
            var status = doc.RootElement.GetProperty("verification_status").GetString();
            return string.Equals(status, "SUCCESS", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(status, "VERIFIED", StringComparison.OrdinalIgnoreCase);
        }

        public PayPalOptions Options => _options;
    }
}
