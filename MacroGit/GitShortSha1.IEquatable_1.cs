using System;

namespace MacroGit
{
    public partial class GitShortSha1 : IEquatable<GitShortSha1>
    {

        public bool Equals(GitShortSha1 shortSha1)
        {
            if (shortSha1 is null) return false;
            return shortSha1.ToString() == value;
        }

    }
}
