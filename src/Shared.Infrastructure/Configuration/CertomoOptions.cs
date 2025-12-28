namespace Shared.Infrastructure.Configuration;

public class CertomoOptions
{
    public const string SectionName = "Certomo";

    public string BaseUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
