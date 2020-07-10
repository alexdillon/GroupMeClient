using System.Reflection;

// AssemblyVersion = full version info, major.minor.patch
[assembly: AssemblyVersion(ThisAssembly.SimpleVersion)]

// FileVersion = full version info, major.minor.patch
[assembly: AssemblyFileVersion(ThisAssembly.SimpleVersion)]

// InformationalVersion = full version + branch + commit sha.
[assembly: AssemblyInformationalVersion(ThisAssembly.InformationalVersion)]

/// <summary>
/// Defines Version Numbers used for automatic assembly versioning.
/// </summary>
public partial class ThisAssembly
{
    /// <summary>
    /// Simple release-like version number, like 4.0.1.0.
    /// </summary>
    public const string SimpleVersion = Git.BaseVersion.Major + "." + Git.BaseVersion.Minor + "." + Git.BaseVersion.Patch + "." + Git.Commits;

    /// <summary>
    /// Full version, plus branch and commit short sha, like 4.0.1.0-39cf84e-branch.
    /// </summary>
    public const string InformationalVersion = SimpleVersion + "-" + Git.Commit + "+" + Git.Branch;
}
