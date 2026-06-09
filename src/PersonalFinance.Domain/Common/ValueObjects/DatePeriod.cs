using PersonalFinance.Domain.Common.Primitives;

namespace PersonalFinance.Domain.Common.ValueObjects;

/// <summary>Represents the month+year period used to organise money month-by-month.</summary>
public sealed class DatePeriod : ValueObject
{
    public int Year { get; }
    public int Month { get; }

    private DatePeriod(int year, int month) { Year = year; Month = month; }

    public static Result<DatePeriod> Create(int year, int month)
    {
        if (year < 2000 || year > 2100)
            return Result.Failure<DatePeriod>(Error.Validation("Year", "Year must be between 2000 and 2100."));
        if (month < 1 || month > 12)
            return Result.Failure<DatePeriod>(Error.Validation("Month", "Month must be between 1 and 12."));
        return Result.Success(new DatePeriod(year, month));
    }

    public static DatePeriod Current => new(DateTime.UtcNow.Year, DateTime.UtcNow.Month);
    public static DatePeriod FromDate(DateTime date) => new(date.Year, date.Month);

    public bool Contains(DateTime date) => date.Year == Year && date.Month == Month;

    protected override IEnumerable<object?> GetComponents() { yield return Year; yield return Month; }
    public override string ToString() => $"{Year:D4}-{Month:D2}";
}
