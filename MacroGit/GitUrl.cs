using System;
using System.IO;
using System.Linq;

namespace MacroGit
{

    /// <summary>
    /// A Git repository URL
    /// </summary>
    ///
    /// <remarks>
    /// See <c>https://git-scm.com/docs/git-fetch</c> for details
    /// </remarks>
    ///
    public class GitUrl : Uri
    {

        public static implicit operator string(GitUrl gitUrl)
        {
            if (gitUrl == null) return null;
            return gitUrl.ToString();
        }


        /// <summary>
        /// Valid Git URL schemes
        /// </summary>
        ///
        static readonly string[] GitSchemes = new[] { "FILE", "SSH", "GIT", "HTTP", "HTTPS" };


        static string CheckGitUrlString(string gitUrlString)
        {
            if (gitUrlString == null) throw new ArgumentNullException("gitUrlString");
            return gitUrlString;
        }


        /// <summary>
        /// Initialise a new Git URL
        /// </summary>
        ///
        /// <param name="gitUrlString">
        /// A Git URL string
        /// </param>
        ///
        /// <exception cref="ArgumentNullException">
        /// <paramref name="gitUrlString"/> was <c>null</c>
        /// </exception>
        ///
        /// <exception cref="FormatException">
        /// <paramref name="gitUrlString"/> was not a valid Git URL
        /// </exception>
        ///
        public GitUrl(string gitUrlString)
            : base(CheckGitUrlString(gitUrlString), UriKind.Absolute)
        {
            if (!GitSchemes.Contains(Scheme.ToUpperInvariant()))
                throw new FormatException("Invalid Git URL scheme");
            if (Query != "")
                throw new FormatException("Query components are not permitted in Git URLs");
            if (Fragment != "")
                throw new FormatException("Fragment components are not permitted in Git URLs");

            RepositoryName = new GitRepositoryName(Path.GetFileNameWithoutExtension(AbsolutePath));
        }


        /// <summary>
        /// The final path component, minus any filename extension
        /// </summary>
        ///
        public GitRepositoryName RepositoryName
        {
            get;
            private set;
        }

    }
}
