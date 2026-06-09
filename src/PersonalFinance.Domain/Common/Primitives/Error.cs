namespace PersonalFinance.Domain.Common.Primitives;

/// <summary>Represents a domain error without throwing exceptions.</summary>
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string entity, Guid id) =>
        new($"{entity}.NotFound", $"{entity} '{id}' was not found.");

    public static Error Validation(string field, string message) =>
        new($"Validation.{field}", message);

    public static Error Conflict(string code, string message) =>
        new($"Conflict.{code}", message);

    public static Error BusinessRule(string code, string message) =>
        new($"BusinessRule.{code}", message);

    public override string ToString() => $"[{Code}] {Description}";
}
