using System;

namespace MacroGit
{
    public partial class GitRefName : IEquatable<GitRefName>
    {

        public bool Equals(GitRefName refName)
        {
            if (refName is null) return false;
            return refName.ToString() == value;
        }

    }
}
