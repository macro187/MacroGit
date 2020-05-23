namespace MacroGit
{

    /// <summary>
    /// A full, unambiguous refname path
    /// </summary>
    ///
    /// <remarks>
    /// https://git-scm.com/docs/gitrevisions
    /// </remarks>
    ///
    public class GitFullRefName : GitRefName
    {

        public GitFullRefName(string value)
            : base(value)
        {
        }

    }
}
