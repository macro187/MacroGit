using System;

namespace MacroGit
{

    /// <summary>
    /// An individual refname path component
    /// </summary>
    ///
    /// <remarks>
    /// https://git-scm.com/docs/gitrevisions
    /// </remarks>
    ///
    public class GitRefNameComponent : GitRefName
    {

        public GitRefNameComponent(string value)
            : base(value)
        {
            if (value.Contains("/"))
            {
                throw new FormatException("Invalid characters");
            }
        }

    }
}
