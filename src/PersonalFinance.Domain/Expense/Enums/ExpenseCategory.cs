namespace PersonalFinance.Domain.Expense.Enums;

/// <summary>Categories matching the design: obligatory debts, leisure, etc.</summary>
public enum ExpenseCategory
{
    // Obligatory
    Housing,       // rent, mortgage
    Utilities,     // electricity, water, internet
    Debt,          // loan payments, credit card minimum
    Insurance,
    Transport,
    // Variable
    Food,
    Health,
    Education,
    // Leisure
    Entertainment,
    Dining,
    Travel,
    Shopping,
    // Other
    Subscriptions,
    Other
}
