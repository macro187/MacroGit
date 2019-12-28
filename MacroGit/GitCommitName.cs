using System;
using System.Linq;
using System.Text.RegularExpressions;
using MacroGuards;

namespace MacroGit
{

    /// <summary>
    /// Name of a Git commit
    /// </summary>
    ///
    public partial class GitCommitName
    {

        public static implicit operator string(GitCommitName gitCommit)
        {
            if (gitCommit == null) return null;
            return gitCommit.ToString();
        }


        public static bool operator ==(GitCommitName a, GitCommitName b)
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.Equals(b);
        }


        public static bool operator !=(GitCommitName a, GitCommitName b)
        {
            return !(a == b);
        }


        /// <summary>
        /// Initialise a new Git commit name
        /// </summary>
        ///
        /// <param name="gitCommitString">
        /// A Git commit string
        /// </param>
        ///
        /// <exception cref="ArgumentNullException">
        /// <paramref name="gitCommitString"/> was <c>null</c>
        /// </exception>
        ///
        /// <exception cref="FormatException">
        /// <paramref name="gitCommitString"/> was not a valid Git commit reference
        /// </exception>
        ///
        public GitCommitName(string gitCommitString)
        {
            Guard.NotNull(gitCommitString, nameof(gitCommitString));
            if (string.IsNullOrEmpty(gitCommitString))
                throw new FormatException("Empty string");
            if (gitCommitString.Contains(' '))
                throw new FormatException("Contains whitespace");
            if (!Regex.IsMatch(gitCommitString, @"^[-+A-Za-z0-9_./]+$"))
                throw new FormatException("Contains invalid characters");

            _value = gitCommitString;
        }


        string _value;

    }
}
