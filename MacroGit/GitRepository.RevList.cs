using System;
using System.Collections.Generic;
using System.Linq;
using MacroDiagnostics;
using MacroGuards;

namespace MacroGit
{
    public partial class GitRepository
    {

        public IEnumerable<GitCommitInfo> RevList(GitRev rev)
        {
            return RevList(-1, rev);
        }


        public IEnumerable<GitCommitInfo> RevList(int maxCount, GitRev rev)
        {
            return RevList(maxCount, new[]{rev});
        }


        public IEnumerable<GitCommitInfo> RevList(int maxCount, IEnumerable<GitRev> revs)
        {
            Guard.NotNull(revs, nameof(revs));
            revs = revs.ToList();
            if (!revs.Any())
            {
                throw new ArgumentException("Empty", nameof(revs));
            }
            if (revs.Any(rev => rev == null))
            {
                throw new ArgumentException("Null item", nameof(revs));
            }

            if (maxCount < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxCount));
            }

            var args =
                new List<string>()
                {
                    "-C", Path, "rev-list", "--format=fuller", "--date=iso-strict", "--parents",
                };

            if (maxCount >= 0)
            {
                args.Add($"--max-count={maxCount}");
            }

            args.AddRange(revs.Select(rev => rev.ToString()));

            try
            {
                return
                    ParseRevList(
                        ProcessExtensions.ExecuteAndRead(null, false, true, null, GitProgram, args.ToArray())
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
                var sha1s =
                    RemovePrefix("commit ", lines.Current)
                        .Split(' ')
                        .Select(s => new GitSha1(s))
                        .ToList();
                var sha1 = sha1s.First();
                var parentSha1s = sha1s.Skip(1);

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

                yield return
                    new GitCommitInfo(sha1, parentSha1s, author, authorDate, committer, commitDate, messageLines);
            }
        }

    }
}
