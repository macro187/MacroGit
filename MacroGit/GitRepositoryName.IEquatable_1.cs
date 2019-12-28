using System;

namespace MacroGit
{
    public partial class GitRepositoryName : IEquatable<GitRepositoryName>
    {

        public bool Equals(GitRepositoryName other)
        {
            if (other == null) return false;
            return ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
        }


        public static bool operator ==(GitRepositoryName oneName, GitRepositoryName anotherName)
        {
            if (ReferenceEquals(oneName, null) && ReferenceEquals(anotherName, null)) return true;
            if (ReferenceEquals(oneName, null) || ReferenceEquals(anotherName, null)) return false;
            return oneName.Equals(anotherName);
        }


        public static bool operator !=(GitRepositoryName oneName, GitRepositoryName anotherName)
        {
            return !(oneName == anotherName);
        }

    }
}
