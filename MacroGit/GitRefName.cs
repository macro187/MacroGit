using System;
using System.Text.RegularExpressions;
using MacroGuards;

namespace MacroGit
{

    /// <summary>
    /// A full refname, possibly including path separators
    /// </summary>
    ///
    /// <remarks>
    /// https://git-scm.com/docs/gitrevisions
    /// </remarks>
    ///
    public partial class GitRefName
    {

        public static implicit operator string(GitRefName refName)
        {
            if (refName == null) return null;
            return refName.ToString();
        }


        public static bool operator ==(GitRefName a, GitRefName b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }


        public static bool operator !=(GitRefName a, GitRefName b)
        {
            return !(a == b);
        }


        public GitRefName(string value)
        {
            Guard.NotNull(value, nameof(value));

            if (string.IsNullOrEmpty(value))
            {
                throw new FormatException("Empty");
            }

            if (!Regex.IsMatch(value, @"^[A-Za-z0-9/_.-]+$"))
            {
                throw new FormatException("Contains invalid characters");
            }

            this.value = value;
        }


        readonly string value;

    }
}
