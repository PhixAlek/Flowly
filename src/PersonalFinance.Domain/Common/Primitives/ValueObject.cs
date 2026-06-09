namespace PersonalFinance.Domain.Common.Primitives;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetComponents();
    public bool Equals(ValueObject? other) =>
        other is not null && GetType() == other.GetType() &&
        GetComponents().SequenceEqual(other.GetComponents());
    public override bool Equals(object? obj) => Equals(obj as ValueObject);
    public override int GetHashCode() =>
        GetComponents().Aggregate(0, (h, c) => HashCode.Combine(h, c));
}
