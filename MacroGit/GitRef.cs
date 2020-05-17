using System;
using System.Linq;
using MacroGuards;

namespace MacroGit
{

    /// <summary>
    /// A refname plus the sha1 of the object it refers to
    /// </summary>
    ///
    public class GitRef
    {

        public GitRef(GitRefName fullName, GitSha1 target)
        {
            Guard.NotNull(fullName, nameof(fullName));
            Guard.NotNull(target, nameof(target));

            FullName = fullName;
            Name = new GitRefNameComponent(fullName.ToString().Split('/').Last());
            Target = target;
            IsBranch = FullName.ToString().StartsWith("refs/heads/", StringComparison.Ordinal);
            IsTag = FullName.ToString().StartsWith("refs/tags/", StringComparison.Ordinal);
        }


        public GitRefNameComponent Name { get; }
        public GitRefName FullName { get; }
        public GitSha1 Target { get; }
        public bool IsBranch { get; }
        public bool IsTag { get; }

    }
}
