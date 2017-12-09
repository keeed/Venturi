using System;

namespace Domain
{
    public class InventoryId : IEquatable<InventoryId>
    {
        public Guid Value { get; }

        public InventoryId(Guid value)
        {
            Value = value;
        }

        public bool Equals(InventoryId other)
        {
            if(other == null)
            {
                return false;
            }
            
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InventoryId);
        }

        public static bool operator ==(InventoryId obj1, InventoryId obj2)
        {
            if (ReferenceEquals(obj1, null) && ReferenceEquals(obj2, null))
            {
                return true;
            }

            if (ReferenceEquals(obj1, null) || ReferenceEquals(obj2, null))
            {
                return false;
            }

            return obj1.Equals(obj2);
        }

        public static bool operator !=(InventoryId obj1, InventoryId obj2)
        {
            return !(obj1 == obj2);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}