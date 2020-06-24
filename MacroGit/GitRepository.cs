using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MacroDiagnostics;
using MacroGuards;
using IOPath = System.IO.Path;

namespace MacroGit
{
    public partial class GitRepository
    {

        /// <summary>
        /// Determine whether a directory is a Git repository
        /// </summary>
        ///
        public static bool IsRepository(string path)
        {
            Guard.Required(path, nameof(path));
            if (!Directory.Exists(path)) return false;
            if (!Directory.Exists(IOPath.Combine(path, ".git"))) return false;
            return true;
        }


        /// <summary>
        /// Locate the Git repository containing the specified <paramref name="path"/>
        /// </summary>
        ///
        /// <returns>
        /// The Git repository that <paramref name="path"/> is in
        /// - OR -
        /// <c>null</c> if it is not in a Git repository
        /// </returns>
        ///
        public static GitRepository FindContainingRepository(string path)
        {
            var dir = new DirectoryInfo(path);
            while (true)
            {
                if (dir.Parent == null) return null;
                if (IsRepository(dir.FullName)) break;
                dir = dir.Parent;
            }
            return new GitRepository(dir.FullName);
        }


        /// <summary>
        /// Clone a Git repository
        /// </summary>
        ///
        public static GitRepository Clone(string parentPath, GitUrl url)
        {
            Guard.Required(parentPath, nameof(parentPath));
            if (!Directory.Exists(parentPath))
            {
                throw new ArgumentException("Parent path doesn't exist", nameof(parentPath));
            }
            Guard.NotNull(url, nameof(url));

            var directoryName = IOPath.GetFileName(url.AbsolutePath);
            if (directoryName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                directoryName = directoryName.Substring(0, directoryName.Length - 4);
            }

            var path = IOPath.Combine(parentPath, directoryName);

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", parentPath, "clone", url);
            if (r.ExitCode != 0) throw new GitException("Cloning repository failed", r);

            return new GitRepository(path);
        }


        /// <summary>
        /// Initialise a new Git repository
        /// </summary>
        ///
        /// <remarks>
        /// If the specified <paramref name="path"/> doesn't exist, it is created
        /// </remarks>
        ///
        /// <param name="path">
        /// Path to new repository
        /// </param>
        ///
        /// <returns>
        /// The newly-initialised repository
        /// </returns>
        ///
        /// <exception cref="InvalidOperationException">
        /// The specified <paramref name="path"/> is already a Git repository
        /// </exception>
        ///
        public static GitRepository Init(string path)
        {
            Guard.Required(path, nameof(path));
            path = IOPath.GetFullPath(path);

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (IsRepository(path))
                throw new InvalidOperationException("Path is already a Git repository");

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", path, "init");
            if (r.ExitCode != 0) throw new GitException("Initialising repository failed", r);

            return new GitRepository(path);
        }


        public GitRepository(string path)
        {
            Guard.Required(path, nameof(path));
            path = IOPath.GetFullPath(path);
            if (!IsRepository(path)) throw new ArgumentException("Not a path to a Git repository", nameof(path));
            Path = path;
            Name = new GitRepositoryName(IOPath.GetFileName(path));
        }


        /// <summary>
        /// Absolute path to the repository
        /// </summary>
        ///
        public string Path
        {
            get;
        }


        /// <summary>
        /// Name of the repository (according to its local directory name)
        /// </summary>
        ///
        public GitRepositoryName Name
        {
            get;
        }


        /// <summary>
        /// Determine whether a rev resolves to a commit in the repository
        /// </summary>
        ///
        public bool Exists(GitRev rev)
        {
            return TryGetCommitId(rev, out var _);
        }


        /// <summary>
        /// Try resolving a rev to a commit sha1
        /// </summary>
        ///
        public bool TryGetCommitId(GitRev rev, out GitSha1 sha1)
        {
            Guard.NotNull(rev, nameof(rev));

            try
            {
                sha1 = GetCommitId(rev);
                return true;
            }
            catch (GitException)
            {
                sha1 = default;
                return false;
            }
        }


        /// <summary>
        /// Get the globally-unique identifier of the currently-checked-out commit
        /// </summary>
        ///
        public GitSha1 GetCommitId()
        {
            return GetCommitId(new GitRev("HEAD"));
        }


        /// <summary>
        /// Resolve a rev to a commit sha1
        /// </summary>
        ///
        public GitSha1 GetCommitId(GitRev rev)
        {
            Guard.NotNull(rev, nameof(rev));

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path,
                "rev-parse", "-q", "--verify", $"{rev}^{{commit}}");

            if (r.ExitCode != 0)
            {
                throw new GitException("Resolve rev to commit sha1 failed", r);
            }

            return new GitSha1(r.StandardOutput.Trim());
        }


        /// <summary>
        /// Resolve a rev to a shortened commit sha1
        /// </summary>
        ///
        /// <param name="minimumLength">
        /// Minimum length of the shortened sha1, or <c>0</c> for Git to choose automatically
        /// </param>
        ///
        public GitShortSha1 GetShortCommitId(GitRev rev, int minimumLength = 0)
        {
            Guard.NotNull(rev, nameof(rev));

            if (minimumLength < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLength));
            }
            if (0 < minimumLength && minimumLength < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumLength));
            }

            var length = minimumLength == 0 ? "auto" : minimumLength.ToString();

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path,
                "rev-parse", $"--short={length}", rev);

            if (r.ExitCode != 0)
            {
                throw new GitException("Resolve rev to short commit sha1 failed", r);
            }

            return new GitShortSha1(r.StandardOutput.Trim());
        }


        /// <summary>
        /// Are there any uncommitted changes?
        /// </summary>
        ///
        /// <remarks>
        /// "Uncommitted changes" includes staged or unstaged changes of any kind.
        /// </remarks>
        ///
        /// <exception cref="GitException">
        /// The check for uncommitted changes failed
        /// </exception>
        ///
        public bool HasUncommittedChanges()
        {
            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "status", "--porcelain");

            if (r.ExitCode != 0)
                throw new GitException("Uncommitted changes check failed", r);

            return r.CombinedOutput.Trim() != "";
        }


        /// <summary>
        /// Stage uncommitted changes
        /// </summary>
        ///
        public void StageChanges()
        {
            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "add", "-A");

            if (r.ExitCode != 0)
                throw new GitException("Stage uncommitted changes failed", r);
        }


        /// <summary>
        /// Commit staged changes
        /// </summary>
        ///
        public void Commit(string message)
        {
            Guard.Required(message, nameof(message));

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "commit", "-m", message);

            if (r.ExitCode != 0)
                throw new GitException("Commit failed", r);
        }


        /// <summary>
        /// Check out a particular commit
        /// </summary>
        ///
        /// <exception cref="InvalidOperationException">
        /// The repository has uncommitted changes
        /// </exception>
        ///
        /// <exception cref="GitException">
        /// The checkout operation failed
        /// </exception>
        ///
        public void Checkout(GitRev rev)
        {
            Guard.NotNull(rev, nameof(rev));

            if (HasUncommittedChanges())
            {
                throw new InvalidOperationException("Repository contains uncommitted changes");
            }

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "checkout", rev);
            if (r.ExitCode != 0)
            {
                throw new GitException("Checkout failed", r);
            }
        }


        /// <summary>
        /// Push branches or tags to a remote
        /// </summary>
        ///
        /// <param name="refs">
        /// Names of branches and/or tags to push
        /// </param>
        ///
        /// <param name="remote">
        /// Name of remote (default "origin")
        /// </param>
        ///
        /// <param name="dryRun">
        /// Don't actually do anything, just show what would be done
        /// </param>
        ///
        /// <param name="echoOutput">
        /// Echo Git command output as it runs
        /// </param>
        ///
        /// <returns>
        /// Git command output
        /// </returns>
        ///
        public string Push(
            IEnumerable<GitFullRefName> refs,
            string remote = "origin",
            bool dryRun = false,
            bool echoOutput = false)
        {
            Guard.NotNull(refs, nameof(refs));
            Guard.NotNull(remote, nameof(remote));
            Guard.NotWhiteSpaceOnly(remote, nameof(remote));

            if (!refs.Any()) return "";

            var args = new List<string>(){ "-C", Path, "push", "--atomic" };
            if (dryRun) args.Add("--dry-run");
            args.Add(remote);
            args.AddRange(refs.Select(r => r.ToString()));

            var result = ProcessExtensions.ExecuteCaptured(false, echoOutput, null, "git", args.ToArray());

            if (result.ExitCode != 0)
            {
                throw new GitException("Push failed", result);
            }

            return result.CombinedOutput;
        }


        /// <summary>
        /// Determine whether a file or directory within the repository is .gitignore'd
        /// </summary>
        ///
        public bool IsIgnored(string path)
        {
            Guard.NotNull(path, nameof(path));
            Guard.NotWhiteSpaceOnly(path, nameof(path));

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "check-ignore", "-q", path);

            switch (r.ExitCode)
            {
                case 0:
                    return true;
                case 1:
                    return false;
                default:
                    throw new GitException("check-ignore failed", r);
            }
        }


        /// <summary>
        /// Is one commit the ancestor of another?
        /// </summary>
        ///
        public bool IsAncestor(GitRev ancestor, GitRev descendent)
        {
            Guard.NotNull(ancestor, nameof(ancestor));
            Guard.NotNull(descendent, nameof(descendent));

            var r = ProcessExtensions.ExecuteCaptured(false, false, null,
                "git", "-C", Path, "merge-base", "--is-ancestor", ancestor, descendent);

            switch (r.ExitCode)
            {
                case 0:
                    return true;
                case 1:
                    return false;
                default:
                    throw new GitException("merge-base --is-ancestor failed", r);
            }
        }


        /// <summary>
        /// Calculate the distance in commits from the beginning of revision history
        /// </summary>
        ///
        /// <param name="to">
        /// The commit to measure to
        /// </param>
        ///
        public int Distance(GitRev to)
        {
            return Distance(null, to);
        }


        /// <summary>
        /// Calculate the distance in commits from one commit to another
        /// </summary>
        ///
        /// <param name="from">
        /// The commit to measure from, or <c>null</c> to measure from the beginning of history
        /// </param>
        ///
        /// <param name="to">
        /// The commit to measure to
        /// </param>
        ///
        public int Distance(GitRev from, GitRev to)
        {
            return ListCommits(from, to).Count();
        }


        /// <summary>
        /// List commit IDs from one commit to another
        /// </summary>
        ///
        /// <remarks>
        /// <paramref name="from"/> is not included in the output, but <paramref name="to"/> is.
        /// </remarks>
        ///
        /// <param name="from">
        /// The commit to list from, or <c>null</c> to list from the beginning of history
        /// </param>
        ///
        /// <param name="to">
        /// The commit to list to
        /// </param>
        ///
        /// <returns>
        /// The list of IDs of commits between <paramref name="from"/> and <paramref name="to"/> in (more or less)
        /// chronological order
        /// </returns>
        ///
        public IEnumerable<GitSha1> ListCommits(GitRev from, GitRev to)
        {
            Guard.NotNull(to, nameof(to));

            var rev = new GitRev(from != null ? $"{from}..{to}" : to);
            return
                RevList(-1, rev)
                    .Select(commit => commit.Sha1)
                    .Reverse();
        }


        /// <summary>
        /// Get a commit's commit date
        /// </summary>
        ///
        public DateTimeOffset GetCommitterDate(GitRev rev) =>
            RevList(1, rev).Single().CommitDate;


        /// <summary>
        /// Get a commit's message
        /// </summary>
        ///
        public string GetCommitMessage(GitRev rev) =>
            RevList(1, rev).Single().Message;

    }
}
