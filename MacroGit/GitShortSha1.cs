using System;
using System.Text.RegularExpressions;

namespace MacroGit
{

    /// <summary>
    /// A possibly-shortened hexadecimal sha1 object name
    /// </summary>
    ///
    /// <remarks>
    /// https://git-scm.com/docs/gitrevisions
    /// </remarks>
    ///
    public class GitShortSha1 : GitRev
    {

        public GitShortSha1(string value)
            : base(value)
        {
            if (value.Length < 4)
            {
                throw new FormatException("Too short");
            }

            if (value.Length > 40)
            {
                throw new FormatException("Too long");
            }

            if (!Regex.IsMatch(value, @"^[a-f0-9]$"))
            {
                throw new FormatException("Invalid characters");
            }
        }

    }
}
