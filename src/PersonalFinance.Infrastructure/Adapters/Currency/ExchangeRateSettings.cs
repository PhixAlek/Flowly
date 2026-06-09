namespace PersonalFinance.Infrastructure.Adapters.Currency;

public sealed class ExchangeRateSettings
{
    public const string SectionName = "ExchangeRateApi";
    public string BaseUrl { get; init; } = "https://v6.exchangerate-api.com/v6";
    public string ApiKey { get; init; } = string.Empty;
    public int CacheDurationMinutes { get; init; } = 60;
}
