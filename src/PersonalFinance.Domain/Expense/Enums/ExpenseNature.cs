namespace PersonalFinance.Domain.Expense.Enums;

public enum ExpenseNature
{
    Obligatory,   // fixed, must pay (rent, utilities)
    Variable,     // necessary but varies (food, health)
    Leisure       // discretionary
}
