using System;

namespace MacroGit
{

    /// <summary>
    /// A full 40-character hexadecimal sha1 object name
    /// </summary>
    ///
    /// <remarks>
    /// https://git-scm.com/docs/gitrevisions
    /// </remarks>
    ///
    public class GitSha1 : GitShortSha1
    {

        public GitSha1(string value)
            : base(value)
        {
            if (value.Length < 40)
            {
                throw new FormatException("Too short");
            }
        }

    }
}
