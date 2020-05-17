using System;
using System.Text.RegularExpressions;
using MacroGuards;

namespace MacroGit
{

    /// <summary>
    /// A 40-character hexadecimal sha1 object name
    /// </summary>
    ///
    /// <remarks>
    /// https://git-scm.com/docs/gitrevisions
    /// </remarks>
    ///
    public partial class GitShortSha1
    {

        public static implicit operator string(GitShortSha1 shortSha1)
        {
            if (shortSha1 == null) return null;
            return shortSha1.ToString();
        }


        public static bool operator ==(GitShortSha1 a, GitShortSha1 b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }


        public static bool operator !=(GitShortSha1 a, GitShortSha1 b)
        {
            return !(a == b);
        }


        public GitShortSha1(string value)
        {
            Guard.NotNull(value, nameof(value));

            if (string.IsNullOrEmpty(value))
            {
                throw new FormatException("Empty");
            }

            if (!Regex.IsMatch(value, @"^[a-f0-9]{4,40}$"))
            {
                throw new FormatException("Contains invalid characters");
            }

            this.value = value;
        }


        readonly string value;

    }
}
