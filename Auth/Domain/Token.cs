namespace Domain
{
    public class Token
    {
        public string Value { get; }

        public Token(string value)
        {
            Value = value;
        }

        public static implicit operator string(Token token) 
        {
            return token.Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}