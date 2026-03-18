namespace DriverBase.DTOs
{
    using System;

    public class OptionDTO : IEquatable<OptionDTO>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public object Value { get; set; }
        public string[] Examples { get; set; }

        public bool Equals(OptionDTO other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name
                   && Description == other.Description
                   && Equals(Value, other.Value)
                   && Equals(Examples, other.Examples);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((OptionDTO)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Description, Value, Examples);
        }
    }
}