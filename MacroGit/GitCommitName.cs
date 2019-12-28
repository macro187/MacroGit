using System;
using System.Linq;
using System.Text.RegularExpressions;
using MacroGuards;


namespace
MacroGit
{


/// <summary>
/// Name of a Git commit
/// </summary>
///
public class
GitCommitName
    : IEquatable<GitCommitName>
{


public static
implicit operator string(GitCommitName gitCommit)
{
    if (gitCommit == null) return null;
    return gitCommit.ToString();
}


public static bool
operator ==(GitCommitName a, GitCommitName b)
{
    if (ReferenceEquals(a, null) && ReferenceEquals(b, null)) return true;
    if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
    return a.Equals(b);
}


public static bool
operator !=(GitCommitName a, GitCommitName b)
{
    return !(a == b);
}


/// <summary>
/// Initialise a new Git commit name
/// </summary>
///
/// <param name="gitCommitString">
/// A Git commit string
/// </param>
///
/// <exception cref="ArgumentNullException">
/// <paramref name="gitCommitString"/> was <c>null</c>
/// </exception>
///
/// <exception cref="FormatException">
/// <paramref name="gitCommitString"/> was not a valid Git commit reference
/// </exception>
///
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Naming",
    "CA1720:IdentifiersShouldNotContainTypeNames",
    MessageId = "string",
    Justification = "Don't care")]
public
GitCommitName(string gitCommitString)
{
    Guard.NotNull(gitCommitString, nameof(gitCommitString));
    if (string.IsNullOrEmpty(gitCommitString))
        throw new FormatException("Empty string");
    if (gitCommitString.Contains(' '))
        throw new FormatException("Contains whitespace");
    if (!Regex.IsMatch(gitCommitString, @"^[-+A-Za-z0-9_./]+$"))
        throw new FormatException("Contains invalid characters");

    _value = gitCommitString;
}


string
_value;



#region IEquatable<GitCommitName>

public bool
Equals(GitCommitName that)
{
    if (that == null) return false;
    return that.ToString() == ToString();
}

#endregion



#region object

/// <summary>
/// Get the Git commit name as a string
/// </summary>
///
public override string
ToString()
{
    return _value;
}


public override bool
Equals(object obj)
{
    if (obj == null) return false;
    var objAsGitCommitName = obj as GitCommitName;
    if (objAsGitCommitName == null) return false;
    return Equals(objAsGitCommitName);
}


public override int
GetHashCode()
{
    return typeof(GitCommitName).GetHashCode() ^ ToString().GetHashCode();
}

#endregion


}
}
