namespace MacroGit
{
    public partial class GitRepositoryName
    {

        public override string ToString()
        {
            return value;
        }


        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (!(obj is GitRepositoryName repositoryName)) return false;
            return Equals(repositoryName);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = typeof(GitRepositoryName).GetHashCode();
                hash = hash * 23 + value.GetHashCode();
                return hash;
            }
        }

    }
}
