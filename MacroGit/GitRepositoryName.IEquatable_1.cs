using System;

namespace MacroGit
{
    public partial class GitRepositoryName : IEquatable<GitRepositoryName>
    {

        public bool Equals(GitRepositoryName repositoryName)
        {
            if (repositoryName is null) return false;
            return repositoryName.ToString() == value;
        }

    }
}
