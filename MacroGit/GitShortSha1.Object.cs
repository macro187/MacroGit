namespace MacroGit
{
    public partial class GitShortSha1
    {

        public override string ToString()
        {
            return value;
        }


        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (!(obj is GitShortSha1 shortSha1)) return false;
            return Equals(shortSha1);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = typeof(GitShortSha1).GetHashCode();
                hash = hash * 23 + value.GetHashCode();
                return hash;
            }
        }

    }
}
