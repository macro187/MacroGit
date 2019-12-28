namespace MacroGit
{
    public partial class GitCommitName
    {

        /// <summary>
        /// Get the Git commit name as a string
        /// </summary>
        ///
        public override string ToString()
        {
            return _value;
        }


        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var objAsGitCommitName = obj as GitCommitName;
            if (objAsGitCommitName == null) return false;
            return Equals(objAsGitCommitName);
        }


        public override int GetHashCode()
        {
            return typeof(GitCommitName).GetHashCode() ^ ToString().GetHashCode();
        }

    }
}
