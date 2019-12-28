using System;
using System.Text.RegularExpressions;
using MacroGuards;


namespace
MacroGit
{


/// <summary>
/// A Git repository name
/// </summary>
///
/// <remarks>
/// Case-insensitive
/// </remarks>
///
public class
GitRepositoryName
    : IEquatable<GitRepositoryName>
{


public static
implicit operator string(GitRepositoryName repositoryName)
{
    if (repositoryName == null) return null;
    return repositoryName.ToString();
}


/// <summary>
/// Initialise a new repository name
/// </summary>
///
/// <param name="repositoryNameString">
/// The repository name
/// </param>
///
/// <exception cref="ArgumentNullException">
/// <paramref name="repositoryNameString"/> was <c>null</c>
/// </exception>
///
/// <exception cref="FormatException">
/// <paramref name="repositoryNameString"/> was not a valid repository name
/// </exception>
///
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Microsoft.Naming",
    "CA1720:IdentifiersShouldNotContainTypeNames",
    MessageId = "string",
    Justification = "Don't care")]
public
GitRepositoryName(string repositoryNameString)
{
    Guard.NotNull(repositoryNameString, nameof(repositoryNameString));
    if (!Regex.IsMatch(repositoryNameString, @"^[A-Za-z0-9_.-]+$"))
        throw new FormatException("Contains invalid characters");

    _value = repositoryNameString;
}


string
_value;



#region IEquatable<RepositoryName>

public bool
Equals(GitRepositoryName other)
{
    if (other == null) return false;
    return ToString().Equals(other.ToString(), StringComparison.OrdinalIgnoreCase);
}


public static bool
operator ==(GitRepositoryName oneName, GitRepositoryName anotherName)
{
    if (object.ReferenceEquals(oneName, null) && object.ReferenceEquals(anotherName, null)) return true;
    if (object.ReferenceEquals(oneName, null) || object.ReferenceEquals(anotherName, null)) return false;
    return oneName.Equals(anotherName);
}


public static bool
operator !=(GitRepositoryName oneName, GitRepositoryName anotherName)
{
    return !(oneName == anotherName);
}

#endregion



#region object

/// <summary>
/// Get the repository name as a string
/// </summary>
///
public override string
ToString()
{
    return _value;
}


/// <inheritdoc/>
public override bool
Equals(object obj)
{
    if (obj == null) return false;
    return Equals(obj as GitRepositoryName);
}


/// <inheritdoc/>
public override int
GetHashCode()
{
    return ToString().ToUpperInvariant().GetHashCode();
}

#endregion


}
}
