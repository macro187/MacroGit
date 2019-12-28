using System;

namespace MacroGit
{
    public partial class GitCommitName : IEquatable<GitCommitName>
    {

        public bool Equals(GitCommitName that)
        {
            if (that == null) return false;
            return that.ToString() == ToString();
        }

    }
}
