namespace SteveTheTradeBot.Dal.Models.Base
{
    public abstract class BaseReferenceWithName : BaseReference
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id} {nameof(Name)}: {Name}";
        }

        #region Equality members

        protected bool Equals(BaseReferenceWithName other)
        {
            return base.Equals(other) && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BaseReferenceWithName) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        #endregion
    }
}