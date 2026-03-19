namespace GestAI.Api.Configuration;

public sealed class ApiCorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];
}
