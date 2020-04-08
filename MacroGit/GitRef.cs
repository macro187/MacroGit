using MacroGuards;

namespace MacroGit
{

    /// <summary>
    /// A commit name plus the unique identifier of the commit it refers to
    /// </summary>
    ///
    public class GitRef
    {

        public GitRef(GitCommitName name, GitCommitName id)
        {
            Guard.NotNull(name, nameof(name));
            Guard.NotNull(id, nameof(id));

            Name = name;
            Id = id;
        }


        public GitCommitName Name { get; }
        public GitCommitName Id { get; }

    }
}
