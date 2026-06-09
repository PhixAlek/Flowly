namespace PersonalFinance.Infrastructure.Security;

public sealed class CorsSettings
{
    public const string SectionName = "Cors";
    public string PolicyName { get; init; } = "PersonalFinancePolicy";
    public string[] AllowedOrigins { get; init; } = [];
}
