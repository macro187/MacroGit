using System;

namespace MacroGit
{
    public partial class GitSha1 : IEquatable<GitSha1>
    {

        public bool Equals(GitSha1 sha1)
        {
            if (sha1 is null) return false;
            return sha1.ToString() == value;
        }

    }
}
