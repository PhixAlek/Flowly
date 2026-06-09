using System.Text.Json.Serialization;

namespace PersonalFinance.Infrastructure.Adapters.Currency;

internal sealed class ExchangeRateApiResponse
{
    [JsonPropertyName("result")]
    public string Result { get; init; } = string.Empty;

    [JsonPropertyName("conversion_rates")]
    public Dictionary<string, decimal> ConversionRates { get; init; } = [];
}
