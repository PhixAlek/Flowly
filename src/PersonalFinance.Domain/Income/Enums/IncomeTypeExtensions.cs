namespace PersonalFinance.Domain.Income.Enums;

public static class IncomeTypeExtensions
{
    public static IncomeNature GetDefaultNature(this IncomeType type) =>
        type switch
        {
            IncomeType.Rental or
            IncomeType.Dividend or
            IncomeType.Interest => IncomeNature.Passive,
            _ => IncomeNature.Active
        };
}
