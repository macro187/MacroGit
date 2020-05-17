namespace MacroGit
{
    public partial class GitRefNameComponent
    {

        public override string ToString()
        {
            return value;
        }


        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (!(obj is GitRefNameComponent refNameComponent)) return false;
            return Equals(refNameComponent);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = typeof(GitRefNameComponent).GetHashCode();
                hash = hash * 23 + value.GetHashCode();
                return hash;
            }
        }

    }
}
