using System;
using System.Text.RegularExpressions;
using MacroGuards;

namespace MacroGit
{

    /// <summary>
    /// A Git repository name
    /// </summary>
    ///
    /// <remarks>
    /// https://git-scm.com/docs/gitrevisions
    /// </remarks>
    ///
    public partial class GitRepositoryName
    {

        public static implicit operator string(GitRepositoryName repositoryName)
        {
            if (repositoryName == null) return null;
            return repositoryName.ToString();
        }


        public static bool operator ==(GitRepositoryName a, GitRepositoryName b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }


        public static bool operator !=(GitRepositoryName a, GitRepositoryName b)
        {
            return !(a == b);
        }


        public GitRepositoryName(string value)
        {
            Guard.NotNull(value, nameof(value));

            if (string.IsNullOrEmpty(value))
            {
                throw new FormatException("Empty");
            }

            if (!Regex.IsMatch(value, @"^[A-Za-z0-9_.-]+$"))
            {
                throw new FormatException("Contains invalid characters");
            }

            this.value = value;
        }


        readonly string value;

    }
}
