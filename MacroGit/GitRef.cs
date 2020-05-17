using MacroGuards;

namespace MacroGit
{

    /// <summary>
    /// A refname plus the sha1 of the object it refers to
    /// </summary>
    ///
    public class GitRef
    {

        public GitRef(GitRefName name, GitSha1 target)
        {
            Guard.NotNull(name, nameof(name));
            Guard.NotNull(target, nameof(target));

            Name = name;
            Target = target;
        }


        public GitRefName Name { get; }
        public GitSha1 Target { get; }

    }
}
