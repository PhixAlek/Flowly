using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PersonalFinance.Application.Income.Ports;
using System.Text.Json;

namespace PersonalFinance.Infrastructure.Adapters.Currency;

/// <summary>
/// Adapter: implements ICurrencyConverter using ExchangeRate-API v6.
/// Results are cached per base currency to reduce API calls.
/// Rule I6: freelancers can convert USD/EUR → COP.
/// </summary>
public sealed class ExchangeRateApiAdapter(
    HttpClient httpClient,
    IMemoryCache cache,
    IOptions<ExchangeRateSettings> options,
    ILogger<ExchangeRateApiAdapter> logger)
    : ICurrencyConverter
{
    private readonly ExchangeRateSettings _settings = options.Value;

    public async Task<decimal> GetRateAsync(string from, string to, CancellationToken ct = default)
    {
        from = from.ToUpperInvariant();
        to = to.ToUpperInvariant();

        if (from == to) return 1m;

        var cacheKey = $"exchange_rates_{from}";
        if (!cache.TryGetValue(cacheKey, out Dictionary<string, decimal>? rates))
        {
            rates = await FetchRatesAsync(from, ct);
            cache.Set(cacheKey, rates, TimeSpan.FromMinutes(_settings.CacheDurationMinutes));
        }

        if (rates is null || !rates.TryGetValue(to, out var rate))
            throw new InvalidOperationException($"Exchange rate not found for {from} → {to}.");

        return rate;
    }

    public async Task<decimal> ConvertAsync(decimal amount, string from, string to, CancellationToken ct = default)
    {
        var rate = await GetRateAsync(from, to, ct);
        return Math.Round(amount * rate, 2);
    }

    private async Task<Dictionary<string, decimal>> FetchRatesAsync(string baseCurrency, CancellationToken ct)
    {
        var url = $"{_settings.BaseUrl}/{_settings.ApiKey}/latest/{baseCurrency}";
        logger.LogInformation("Fetching exchange rates for base currency {Currency}", baseCurrency);

        var response = await httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var data = JsonSerializer.Deserialize<ExchangeRateApiResponse>(json);

        if (data?.Result != "success" || data.ConversionRates.Count == 0)
            throw new InvalidOperationException($"ExchangeRate-API returned an unexpected response for {baseCurrency}.");

        return data.ConversionRates;
    }
}
