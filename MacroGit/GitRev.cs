using System;
using MacroGuards;

namespace MacroGit
{

    /// <summary>
    /// A string that specifies a particular commit in any way
    /// </summary>
    ///
    /// <remarks>
    /// https://git-scm.com/docs/gitrevisions
    /// </remarks>
    ///
    public partial class GitRev
    {

        public static implicit operator string(GitRev rev)
        {
            if (rev == null) return null;
            return rev.ToString();
        }


        public static bool operator ==(GitRev a, GitRev b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }


        public static bool operator !=(GitRev a, GitRev b)
        {
            return !(a == b);
        }


        public GitRev(string value)
        {
            Guard.NotNull(value, nameof(value));

            if (string.IsNullOrEmpty(value))
            {
                throw new FormatException("Empty");
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new FormatException("Whitespace-only");
            }

            this.value = value;
        }


        readonly string value;

    }
}
