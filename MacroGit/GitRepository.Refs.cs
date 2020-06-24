using System;
using System.Collections.Generic;
using System.Linq;
using MacroDiagnostics;
using MacroGuards;
using MacroSystem;

namespace MacroGit
{
    public partial class GitRepository
    {

        /// <summary>
        /// Get the name of the currently-checked-out branch
        /// </summary>
        ///
        /// <returns>
        /// The name of the currently-checked out branch
        /// - OR -
        /// <c>null</c> if no branch is checked out
        /// </returns>
        ///
        public GitRefNameComponent GetBranch()
        {
            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "rev-parse", "--abbrev-ref", "HEAD");

            if (r.ExitCode != 0)
                throw new GitException("Get current branch failed", r);

            var branchName = r.StandardOutput.Trim();
            if (branchName == "HEAD") return null;

            return new GitRefNameComponent(branchName);
        }


        /// <summary>
        /// Get list of remote branches and the commit ids they point to
        /// </summary>
        ///
        public IEnumerable<GitRef> GetRemoteBranches()
        {
            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "ls-remote", "--heads");

            switch (r.ExitCode)
            {
                case 0:
                    return ParseRefLines(StringExtensions.SplitLines(r.StandardOutput));
                default:
                    throw new GitException("Get remote branches failed", r);
            }
        }


        /// <summary>
        /// Get list of branches and the commit ids they point to
        /// </summary>
        ///
        public IEnumerable<GitRef> GetBranches()
        {
            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "show-ref", "--heads");

            switch (r.ExitCode)
            {
                case 0:
                case 1:
                    return ParseRefLines(StringExtensions.SplitLines(r.StandardOutput));
                default:
                    throw new GitException("Get branches failed", r);
            }
        }


        /// <summary>
        /// Get all refs
        /// </summary>
        ///
        public IEnumerable<GitRef> GetRefs()
        {
            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path,
                "show-ref", "--head", "--dereference");

            switch (r.ExitCode)
            {
                case 0:
                case 1:
                    return ParseRefLines(StringExtensions.SplitLines(r.StandardOutput));
                default:
                    throw new GitException("Get refs failed", r);
            }
        }


        /// <summary>
        /// Get all remote refs
        /// </summary>
        ///
        public IEnumerable<GitRef> GetRemoteRefs()
        {
            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "ls-remote");

            switch (r.ExitCode)
            {
                case 0:
                    return ParseRefLines(StringExtensions.SplitLines(r.StandardOutput));
                default:
                    throw new GitException("Get remote refs failed", r);
            }
        }


        /// <summary>
        /// Create a branch
        /// </summary>
        ///
        public void CreateBranch(GitRefNameComponent name)
        {
            Guard.NotNull(name, nameof(name));

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "branch", name);

            if (r.ExitCode != 0)
            {
                throw new GitException("Create branch failed", r);
            }
        }


        /// <summary>
        /// Create or move a branch to the currently-checked-out commit
        /// </summary>
        ///
        public void CreateOrMoveBranch(GitRefNameComponent name)
        {
            Guard.NotNull(name, nameof(name));

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "branch", "-f", name, "HEAD");

            if (r.ExitCode != 0)
                throw new GitException("Create or move branch failed", r);
        }


        /// <summary>
        /// Create a symbolic reference branch that points to another
        /// </summary>
        ///
        public void CreateSymbolicBranch(GitRefNameComponent name, GitRefNameComponent target)
        {
            Guard.NotNull(name, nameof(name));
            Guard.NotNull(target, nameof(target));

            var refName = new GitFullRefName($"refs/heads/{name}");
            var targetRefName = new GitFullRefName($"refs/heads/{target}");

            //
            // CreateBranch() fails if the branch already exists, so match that behaviour
            //
            if (GetBranches().Any(@ref => @ref.FullName == refName))
            {
                throw new GitException($"A branch named '{name}' already exists");
            }

            CreateSymbolicReference(refName, targetRefName);
        }


        /// <summary>
        /// Is a branch actually a symbolic reference?
        /// </summary>
        ///
        public bool IsSymbolicBranch(GitRefNameComponent name)
        {
            Guard.NotNull(name, nameof(name));

            var refName = new GitFullRefName($"refs/heads/{name}");
            return IsSymbolicReference(refName);
        }


        /// <summary>
        /// Delete a branch
        /// </summary>
        ///
        /// <remarks>
        /// If <paramref name="name"/> is a symbolic ref, it is deleted using
        /// <see cref="DeleteSymbolicReference(GitFullRefName)"/>.  Otherwise, it is treated as a regular branch and
        /// deleted using <c>git branch -D</c>.
        /// </remarks>
        ///
        public void DeleteBranch(GitRefNameComponent name)
        {
            Guard.NotNull(name, nameof(name));

            var refName = new GitFullRefName($"refs/heads/{name}");
            if (IsSymbolicReference(refName))
            {
                DeleteSymbolicReference(refName);
                return;
            }

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "branch", "-D", name);

            if (r.ExitCode != 0)
            {
                throw new GitException("Delete branch failed", r);
            }
        }


        /// <summary>
        /// Get list of remote tags and the commit ids they point to
        /// </summary>
        ///
        public IEnumerable<GitRef> GetRemoteTags()
        {
            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "ls-remote", "--tags");

            switch (r.ExitCode)
            {
                case 0:
                    return ParseRefLines(StringExtensions.SplitLines(r.StandardOutput));
                default:
                    throw new GitException("Get remote tags failed", r);
            }
        }


        /// <summary>
        /// Get list of tags and the commit ids they point to
        /// </summary>
        ///
        public IEnumerable<GitRef> GetTags()
        {
            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path,
                "show-ref", "--tags", "-d");

            switch (r.ExitCode)
            {
                case 0:
                case 1:
                    return ParseRefLines(StringExtensions.SplitLines(r.StandardOutput));
                default:
                    throw new GitException("Get tags failed", r);
            }
        }


        /// <summary>
        /// Create a tag
        /// </summary>
        ///
        public void CreateTag(GitRefNameComponent name)
        {
            Guard.NotNull(name, nameof(name));

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "tag", name);

            if (r.ExitCode != 0)
                throw new GitException("Create tag failed", r);
        }


        /// <summary>
        /// Delete a tag
        /// </summary>
        ///
        public void DeleteTag(GitRefNameComponent name)
        {
            Guard.NotNull(name, nameof(name));

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "tag", "-d", name);

            if (r.ExitCode != 0)
                throw new GitException("Delete tag failed", r);
        }


        /// <summary>
        /// Create a symbolic reference
        /// </summary>
        ///
        public void CreateSymbolicReference(GitFullRefName name, GitFullRefName target)
        {
            Guard.NotNull(name, nameof(name));
            Guard.NotNull(target, nameof(target));

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "symbolic-ref", name, target);

            if (r.ExitCode != 0)
                throw new GitException("Create symbolic ref failed", r);
        }


        /// <summary>
        /// Delete a symbolic reference
        /// </summary>
        ///
        public void DeleteSymbolicReference(GitFullRefName name)
        {
            Guard.NotNull(name, nameof(name));

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "symbolic-ref", "--delete", name);

            if (r.ExitCode != 0)
            {
                throw new GitException("Delete symbolic ref failed", r);
            }
        }


        /// <summary>
        /// Is a commit name a symbolic reference?
        /// </summary>
        ///
        public bool IsSymbolicReference(GitFullRefName name)
        {
            Guard.NotNull(name, nameof(name));

            var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "symbolic-ref", name);

            return r.ExitCode == 0;
        }


        /// <summary>
        /// Parse refs output from ls-remote or show-ref
        /// </summary>
        ///
        IEnumerable<GitRef> ParseRefLines(IEnumerable<string> lines)
        {
            var entries =
                lines
                    // "08c471b4f1c4c1f1fcdd506bc291d1c3e7e383d8        refs/tags/1.7.0"
                    .Select(s => s.Trim())
                    .Where(s => s != "")
                    // "08c471b4f1c4c1f1fcdd506bc291d1c3e7e383d8        refs/tags/1.7.0"
                    .Select(s => s.Split(new[]{' ', '\t'}, 2))
                    // ["08c471b4f1c4c1f1fcdd506bc291d1c3e7e383d8", "       refs/tags/1.7.0"]
                    .Select(a => (Name: a[1].Trim(), Target: a[0].Trim()))
                    // ("refs/tags/1.7.0", "08c471b4f1c4c1f1fcdd506bc291d1c3e7e383d8")
                    .ToList();

            var lookup = entries.ToDictionary(t => t.Name, t => t.Target);

            string Dereference(string name) =>
                lookup.TryGetValue($"{name}^{{}}", out var sha1) ? sha1 : null;

            return
                entries
                    .Where(t => !t.Name.EndsWith("^{}", StringComparison.Ordinal))
                    .Select(t => (t.Name, Target: Dereference(t.Name) ?? t.Target))
                    .Select(t => new GitRef(new GitFullRefName(t.Name), new GitSha1(t.Target)));
        }

    }
}
