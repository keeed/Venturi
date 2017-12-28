using System;

namespace Domain
{
    public class UserId
    {
        public Guid Value { get; }

        public UserId(Guid value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}