namespace MacroGit
{
    public partial class GitSha1
    {

        public override string ToString()
        {
            return value;
        }


        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (!(obj is GitSha1 sha1)) return false;
            return Equals(sha1);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = typeof(GitSha1).GetHashCode();
                hash = hash * 23 + value.GetHashCode();
                return hash;
            }
        }

    }
}
