using System;
using System.Collections.Generic;
using System.Globalization;
using MacroDiagnostics;
using MacroGuards;

namespace MacroGit
{
    public partial class GitRepository
    {

        public IEnumerable<GitCommitInfo> RevList(GitRev rev)
        {
            return RevList(rev, -1);
        }


        public IEnumerable<GitCommitInfo> RevList(GitRev rev, int maxCount)
        {
            Guard.NotNull(rev, nameof(rev));
            if (maxCount < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCount));
            }

            var args =
                new List<string>()
                {
                    "-C", Path, "rev-list", "--format=fuller", "--date=iso-strict",
                };

            if (maxCount >= 0)
            {
                args.Add($"--max-count={maxCount}");
            }

            args.Add(rev);

            try
            {
                return
                    ParseRevList(
                        ProcessExtensions.ExecuteAndRead(
                            null, false, true, null,
                            "git", args.ToArray())
                        .GetEnumerator());
            }

            catch (ProcessExecuteException pex)
            {
                throw new GitException(pex.Result);
            }
        }


        IEnumerable<GitCommitInfo> ParseRevList(IEnumerator<string> lines)
        {
            string RemovePrefix(string prefix, string line)
            {
                if(!line.StartsWith(prefix))
                {
                    throw new GitException($"Expected '{prefix}' from rev-list but got '{line}'");
                }
                return line.Substring(prefix.Length);
            }

            void UnexpectedEnd()
            {
                throw new GitException("Unexpected end of rev-list output");
            }

            while (true)
            {
                if (!lines.MoveNext()) break;
                var sha1 = new GitSha1(RemovePrefix("commit ", lines.Current));

                if (!lines.MoveNext()) UnexpectedEnd();
                while (lines.Current.StartsWith("Merge:")) if (!lines.MoveNext()) UnexpectedEnd();

                var author = RemovePrefix("Author:", lines.Current).Trim();

                if (!lines.MoveNext()) UnexpectedEnd();
                var authorDate =
                    DateTimeOffset.ParseExact(
                        RemovePrefix("AuthorDate:", lines.Current).Trim(),
                        "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz",
                        null);

                if (!lines.MoveNext()) UnexpectedEnd();
                var committer = RemovePrefix("Commit:", lines.Current).Trim();

                if (!lines.MoveNext()) UnexpectedEnd();
                var commitDate =
                    DateTimeOffset.ParseExact(
                        RemovePrefix("CommitDate:", lines.Current).Trim(),
                        "yyyy'-'MM'-'dd'T'HH':'mm':'sszzz",
                        null);

                if (!lines.MoveNext()) UnexpectedEnd();
                if (lines.Current != "")
                {
                    throw new GitException($"Expected blank line from rev-list but got '{lines.Current}'");
                }

                var messageLines = new List<string>();
                while (true)
                {
                    if (!lines.MoveNext()) UnexpectedEnd();
                    if (!lines.Current.StartsWith("    ")) break;
                    messageLines.Add(lines.Current.Substring(4));
                }

                if (lines.Current != "")
                {
                    throw new GitException($"Expected blank line from rev-list but got '{lines.Current}'");
                }

                yield return new GitCommitInfo(sha1, author, authorDate, committer, commitDate, messageLines);
            }
        }

    }
}
