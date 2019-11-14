using System;
using System.IO;
using IOPath = System.IO.Path;
using MacroGuards;
using MacroDiagnostics;
using System.Collections.Generic;
using MacroSystem;
using System.Linq;

namespace
MacroGit
{


public class
GitRepository
{


/// <summary>
/// Determine whether a directory is a Git repository
/// </summary>
///
public static bool
IsRepository(string path)
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
public static GitRepository
FindContainingRepository(string path)
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
public static GitRepository
Clone(string parentPath, GitUrl url)
{
    Guard.Required(parentPath, nameof(parentPath));
    if (!Directory.Exists(parentPath)) throw new ArgumentException("Parent path doesn't exist", nameof(parentPath));
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
public static GitRepository
Init(string path)
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


public
GitRepository(string path)
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
public string
Path
{
    get;
}


/// <summary>
/// Name of the repository (according to its local directory name)
/// </summary>
///
public GitRepositoryName
Name
{
    get;
}


/// <summary>
/// Get the globally-unique identifier of the currently-checked-out commit
/// </summary>
///
public GitCommitName
GetCommitId()
{
    return GetCommitId(new GitCommitName("HEAD"));
}


/// <summary>
/// Resolves a commit name to an exact unique commit identifier
/// </summary>
///
public GitCommitName
GetCommitId(GitCommitName commitName)
{
    Guard.NotNull(commitName, nameof(commitName));

    var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "rev-parse", commitName);

    if (r.ExitCode != 0)
        throw new GitException("Get commit ID failed", r);

    return new GitCommitName(r.StandardOutput.Trim());
}


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
public GitCommitName
GetBranch()
{
    var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "rev-parse", "--abbrev-ref", "HEAD");

    if (r.ExitCode != 0)
        throw new GitException("Get current branch failed", r);

    var branchName = r.StandardOutput.Trim();
    if (branchName == "HEAD") return null;

    return new GitCommitName(branchName);
}


/// <summary>
/// Get names of local branches
/// </summary>
///
public ISet<GitCommitName>
GetBranches()
{
    var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "branch");

    if (r.ExitCode != 0)
        throw new GitException("Get branches failed", r);

    return new HashSet<GitCommitName>(
        StringExtensions.SplitLines(r.StandardOutput)
            .Select(line => CleanGitBranchLine(line))
            .Where(line => !string.IsNullOrEmpty(line))
            .Select(name => new GitCommitName(name)));
}


/// <summary>
/// Create a branch
/// </summary>
///
public void
CreateBranch(GitCommitName name)
{
    Guard.NotNull(name, nameof(name));

    var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "branch", name);

    if (r.ExitCode != 0)
        throw new GitException("Create branch failed", r);
}


/// <summary>
/// Create or move a branch to the currently-checked-out commit
/// </summary>
///
public void
CreateOrMoveBranch(GitCommitName name)
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
public void
CreateSymbolicBranch(GitCommitName name, GitCommitName target)
{
    Guard.NotNull(name, nameof(name));
    Guard.NotNull(target, nameof(target));

    // CreateBranch() fails if the branch already exists, so match that behaviour
    if (GetBranches().Contains(name))
        throw new GitException("A branch named '" + name + "' already exists");

    var symbolicName = new GitCommitName("refs/heads/" + name);
    var symbolicTarget = new GitCommitName("refs/heads/" + target);
    CreateSymbolicReference(symbolicName, symbolicTarget);
}


/// <summary>
/// Is a branch actually a symbolic reference?
/// </summary>
///
public bool
IsSymbolicBranch(GitCommitName name)
{
    Guard.NotNull(name, nameof(name));

    var symbolicName = new GitCommitName("refs/heads/" + name);
    return IsSymbolicReference(symbolicName);
}


/// <summary>
/// Delete a branch
/// </summary>
///
/// <remarks>
/// If <paramref name="name"/> is a symbolic ref, it is deleted using
/// <see cref="DeleteSymbolicReference(GitCommitName)"/>.  Otherwise, it is treated as a regular branch and deleted
/// using <c>git branch -D</c>.
/// </remarks>
///
public void
DeleteBranch(GitCommitName name)
{
    Guard.NotNull(name, nameof(name));

    var symbolicName = new GitCommitName("refs/heads/" + name);
    if (IsSymbolicReference(symbolicName))
    {
        DeleteSymbolicReference(symbolicName);
        return;
    }

    var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "branch", "-D", name);

    if (r.ExitCode != 0)
        throw new GitException("Delete branch failed", r);
}


/// <summary>
/// Get names of tags
/// </summary>
///
public ISet<GitCommitName>
GetTags()
{
    var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "tag");

    if (r.ExitCode != 0)
        throw new GitException("Get tags failed", r);

    return new HashSet<GitCommitName>(
        StringExtensions.SplitLines(r.StandardOutput)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line))
            .Select(name => new GitCommitName(name)));
}


/// <summary>
/// Create a tag
/// </summary>
///
public void
CreateTag(GitCommitName name)
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
public void
DeleteTag(GitCommitName name)
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
public void
CreateSymbolicReference(GitCommitName name, GitCommitName target)
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
public void
DeleteSymbolicReference(GitCommitName name)
{
    Guard.NotNull(name, nameof(name));

    var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "symbolic-ref", "--delete", name);

    if (r.ExitCode != 0)
        throw new GitException("Delete symbolic ref failed", r);
}


/// <summary>
/// Is a commit name a symbolic reference?
/// </summary>
///
public bool
IsSymbolicReference(GitCommitName name)
{
    Guard.NotNull(name, nameof(name));

    var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "symbolic-ref", name);

    return r.ExitCode == 0;
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
public bool
HasUncommittedChanges()
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
public void
StageChanges()
{
    var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "add", "-A");

    if (r.ExitCode != 0)
        throw new GitException("Stage uncommitted changes failed", r);
}


/// <summary>
/// Commit staged changes
/// </summary>
///
public void
Commit(string message)
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
public void
Checkout(GitCommitName commit)
{
    Guard.NotNull(commit, nameof(commit));
    if (HasUncommittedChanges())
        throw new InvalidOperationException("Repository contains uncommitted changes");

    var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "checkout", commit);
    if (r.ExitCode != 0)
        throw new GitException("Checkout failed", r);
}


/// <summary>
/// Determine whether a file or directory within the repository is .gitignore'd
/// </summary>
///
public bool
IsIgnored(string path)
{
    Guard.NotNull(path, nameof(path));
    Guard.NotWhiteSpaceOnly(path, nameof(path));

    var r = ProcessExtensions.ExecuteCaptured(false, false, null, "git", "-C", Path, "check-ignore", "-q", path);

    switch(r.ExitCode)
    {
        case 0:
            return true;
        case 1:
            return false;
        default:
            throw new GitException("check-ignore failed", r);
    }
}


static string CleanGitBranchLine(string gitBranchLine)
{
    var s = gitBranchLine;
    s = s.Trim().TrimStart('*').Trim();
    var i = s.IndexOf("->");
    if (i >= 0) s = s.Substring(i + 2).Trim();
    return s;
}


}
}
