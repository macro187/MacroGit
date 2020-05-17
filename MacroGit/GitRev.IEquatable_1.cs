using System;

namespace MacroGit
{
    public partial class GitRev : IEquatable<GitRev>
    {

        public bool Equals(GitRev rev)
        {
            if (rev is null) return false;
            return rev.ToString() == value;
        }

    }
}
