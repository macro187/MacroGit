using System;

namespace MacroGit
{
    public partial class GitRefNameComponent : IEquatable<GitRefNameComponent>
    {

        public bool Equals(GitRefNameComponent refNameComponent)
        {
            if (refNameComponent is null) return false;
            return refNameComponent.ToString() == value;
        }

    }
}
