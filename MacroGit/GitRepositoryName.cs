using System;
using System.Text.RegularExpressions;
using MacroGuards;

namespace MacroGit
{

    /// <summary>
    /// A Git repository name
    /// </summary>
    ///
    /// <remarks>
    /// Case-insensitive
    /// </remarks>
    ///
    public partial class GitRepositoryName : IEquatable<GitRepositoryName>
    {

        public static implicit operator string(GitRepositoryName repositoryName)
        {
            if (repositoryName == null) return null;
            return repositoryName.ToString();
        }


        /// <summary>
        /// Initialise a new repository name
        /// </summary>
        ///
        /// <param name="repositoryNameString">
        /// The repository name
        /// </param>
        ///
        /// <exception cref="ArgumentNullException">
        /// <paramref name="repositoryNameString"/> was <c>null</c>
        /// </exception>
        ///
        /// <exception cref="FormatException">
        /// <paramref name="repositoryNameString"/> was not a valid repository name
        /// </exception>
        ///
        public GitRepositoryName(string repositoryNameString)
        {
            Guard.NotNull(repositoryNameString, nameof(repositoryNameString));
            if (!Regex.IsMatch(repositoryNameString, @"^[A-Za-z0-9_.-]+$"))
                throw new FormatException("Contains invalid characters");

            _value = repositoryNameString;
        }


        string _value;

    }
}
