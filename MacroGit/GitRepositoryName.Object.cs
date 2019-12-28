namespace MacroGit
{
    public partial class GitRepositoryName
    {

        /// <summary>
        /// Get the repository name as a string
        /// </summary>
        ///
        public override string ToString()
        {
            return _value;
        }


        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return Equals(obj as GitRepositoryName);
        }


        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ToString().ToUpperInvariant().GetHashCode();
        }

    }
}
