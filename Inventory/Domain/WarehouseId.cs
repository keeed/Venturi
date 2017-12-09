using System;

namespace Domain
{
    public class WarehouseId : IEquatable<WarehouseId>
    {
        public Guid Value { get; }

        public WarehouseId(Guid value)
        {
            Value = value;
        } 

        public bool Equals(WarehouseId other)
        {
            if(other == null)
            {
                return false;
            }
            
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as WarehouseId);
        }

        public static bool operator ==(WarehouseId obj1, WarehouseId obj2)
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

        public static bool operator !=(WarehouseId obj1, WarehouseId obj2)
        {
            return !(obj1 == obj2);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}