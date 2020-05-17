using System;
using System.Text.RegularExpressions;
using MacroGuards;

namespace MacroGit
{

    /// <summary>
    /// A refname component
    /// </summary>
    ///
    /// <remarks>
    /// https://git-scm.com/docs/gitrevisions
    /// </remarks>
    ///
    public partial class GitRefNameComponent
    {

        public static implicit operator string(GitRefNameComponent refNameComponent)
        {
            if (refNameComponent == null) return null;
            return refNameComponent.ToString();
        }


        public static bool operator ==(GitRefNameComponent a, GitRefNameComponent b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }


        public static bool operator !=(GitRefNameComponent a, GitRefNameComponent b)
        {
            return !(a == b);
        }


        public GitRefNameComponent(string value)
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
