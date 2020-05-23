using System;
using System.Text.RegularExpressions;

namespace MacroGit
{

    /// <summary>
    /// A refname
    /// </summary>
    ///
    /// <remarks>
    /// Refnames can be full paths, partial paths, or individual path components, and may be ambiguous.
    /// </remarks>
    ///
    /// <remarks>
    /// https://git-scm.com/docs/gitrevisions
    /// </remarks>
    ///
    public class GitRefName : GitRev
    {

        public GitRefName(string value)
            : base(value)
        {
            if (!Regex.IsMatch(value, @"^[A-Za-z0-9/_.-]+$"))
            {
                throw new FormatException("Invalid characters");
            }

            if (value.StartsWith("/"))
            {
                throw new FormatException("Starts with path separator");
            }

            if (value.EndsWith("/"))
            {
                throw new FormatException("Ends with path separator");
            }

            if (value.Contains("//"))
            {
                throw new FormatException("Multiple consecutive path separators");
            }
        }

    }
}
