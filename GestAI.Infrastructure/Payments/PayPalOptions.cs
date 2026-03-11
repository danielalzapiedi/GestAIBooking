namespace GestAI.Infrastructure.Payments
{
    public class PayPalOptions
    {
        public string Environment { get; set; } = "Sandbox"; // "Live" or "Sandbox"
        public string ClientId { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public string WebhookId { get; set; } = string.Empty;
        public PlanIds Plans { get; set; } = new();

        public class PlanIds
        {
            public string Basic { get; set; } = string.Empty;    // USD 5
            public string Standard { get; set; } = string.Empty; // USD 10
            public string Premium { get; set; } = string.Empty;  // USD 15
        }
    }
}
