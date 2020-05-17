namespace MacroGit
{
    public partial class GitRefName
    {

        public override string ToString()
        {
            return value;
        }


        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (!(obj is GitRefName refName)) return false;
            return Equals(refName);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = typeof(GitRefName).GetHashCode();
                hash = hash * 23 + value.GetHashCode();
                return hash;
            }
        }

    }
}
