namespace Sproutopia.Utilities
{
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        public static bool operator ==(ValueObject? a, ValueObject? b)
        {
            if (a is null && b is null)
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public static bool operator !=(ValueObject? a, ValueObject? b) =>
            !(a == b);

        public virtual bool Equals(ValueObject? other) =>
            other is not null && ValuesAreEqual(other);

        public override bool Equals(object? obj) =>
            obj is ValueObject valueObject && ValuesAreEqual(valueObject);

        public override int GetHashCode() =>
            GetAtomicValues().Aggregate(
                default(int),
                (hashcode, value) =>
                    HashCode.Combine(hashcode, value.GetHashCode()));

        protected abstract IEnumerable<object> GetAtomicValues();

        private bool ValuesAreEqual(ValueObject valueObject) =>
            GetAtomicValues().SequenceEqual(valueObject.GetAtomicValues());
    }
}
