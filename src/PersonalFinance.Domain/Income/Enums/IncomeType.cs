namespace PersonalFinance.Domain.Income.Enums;

/// <summary>
/// Rule I4: Prima/bonus = Active. Rule I5: Extra work = Active.
/// Rule I6: Freelance for another country = Active (multi-currency).
/// Passive = money earned without active participation.
/// </summary>
public enum IncomeType
{
    /// <summary>Salary, wages — the main pillar income.</summary>
    Salary,
    /// <summary>Rule I1: bonus, commission, overtime.</summary>
    Bonus,
    /// <summary>Rule I1 + I5: extra work, freelance gig.</summary>
    Freelance,
    /// <summary>Rule I4: prima (aguinaldo, 13th salary).</summary>
    Prima,
    /// <summary>Rule I6: freelance from another country.</summary>
    InternationalFreelance,
    /// <summary>Rental income — passive.</summary>
    Rental,
    /// <summary>Dividends from investments — passive.</summary>
    Dividend,
    /// <summary>Interest earned on savings/CDTs.</summary>
    Interest,
    /// <summary>Any other source.</summary>
    Other
}
