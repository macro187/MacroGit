namespace MacroGit
{
    public partial class GitRev
    {

        public override string ToString()
        {
            return value;
        }


        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (!(obj is GitRev rev)) return false;
            return Equals(rev);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = typeof(GitRev).GetHashCode();
                hash = hash * 23 + value.GetHashCode();
                return hash;
            }
        }

    }
}
