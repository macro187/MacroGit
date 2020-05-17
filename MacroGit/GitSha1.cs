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
    public partial class GitSha1
    {

        public static implicit operator string(GitSha1 sha1)
        {
            if (sha1 == null) return null;
            return sha1.ToString();
        }


        public static bool operator ==(GitSha1 a, GitSha1 b)
        {
            if (a is null && b is null) return true;
            if (a is null || b is null) return false;
            return a.Equals(b);
        }


        public static bool operator !=(GitSha1 a, GitSha1 b)
        {
            return !(a == b);
        }


        public GitSha1(string value)
        {
            Guard.NotNull(value, nameof(value));

            if (string.IsNullOrEmpty(value))
            {
                throw new FormatException("Empty");
            }

            if (!Regex.IsMatch(value, @"^[a-f0-9]{40}$"))
            {
                throw new FormatException("Contains invalid characters");
            }

            this.value = value;
        }


        readonly string value;

    }
}
