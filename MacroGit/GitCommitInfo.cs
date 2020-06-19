using System;
using System.Collections.Generic;
using System.Linq;
using MacroGuards;

namespace MacroGit
{

    /// <summary>
    /// Basic information about a commit
    /// </summary>
    ///
    public class GitCommitInfo
    {

        string message;


        public GitCommitInfo(
            GitSha1 sha1,
            IEnumerable<GitSha1> parentSha1s,
            string author,
            DateTimeOffset authorDate,
            string committer,
            DateTimeOffset commitDate,
            IReadOnlyList<string> messageLines)
        {
            Guard.NotNull(sha1, nameof(sha1));
            Guard.NotNull(parentSha1s, nameof(parentSha1s));
            Guard.NotNull(author, nameof(author));
            Guard.NotNull(committer, nameof(committer));
            Guard.NotNull(messageLines, nameof(messageLines));

            Sha1 = sha1;
            ParentSha1s = parentSha1s.ToList();
            Author = author;
            AuthorDate = authorDate;
            Committer = committer;
            CommitDate = commitDate;
            MessageLines = messageLines;
        }


        public GitSha1 Sha1 { get; }
        public IReadOnlyList<GitSha1> ParentSha1s { get; }
        public string Author { get; }
        public DateTimeOffset AuthorDate { get; }
        public string Committer { get; }
        public DateTimeOffset CommitDate { get; }
        public IReadOnlyList<string> MessageLines { get; }


        public string Message => 
            message ?? (message = string.Concat(MessageLines.Select(line => line + Environment.NewLine)));

    }
}
