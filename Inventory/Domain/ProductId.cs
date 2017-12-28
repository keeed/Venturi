using System;

namespace Domain
{
    public class ProductId : IEquatable<ProductId>
    {
        public Guid Value { get; }

        public ProductId(Guid value)
        {
            Value = value;
        }

        public bool Equals(ProductId other)
        {
            if(other == null)
            {
                return false;
            }
            
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProductId);
        }

        public static bool operator ==(ProductId obj1, ProductId obj2)
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

        public static bool operator !=(ProductId obj1, ProductId obj2)
        {
            return !(obj1 == obj2);
        }


        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}