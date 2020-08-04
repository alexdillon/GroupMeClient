using System.Reflection;

// AssemblyVersion = full version info, major.minor.patch
[assembly: AssemblyVersion(GroupMeClient.Core.GlobalAssemblyInfo.SimpleVersion)]

// FileVersion = full version info, major.minor.patch
[assembly: AssemblyFileVersion(GroupMeClient.Core.GlobalAssemblyInfo.SimpleVersion)]

// InformationalVersion = full version + branch + commit sha.
[assembly: AssemblyInformationalVersion(GroupMeClient.Core.GlobalAssemblyInfo.InformationalVersion)]

namespace GroupMeClient.Core
{
    /// <summary>
    /// <see cref="GlobalAssemblyInfo"/> documents the build version of the GroupMe Desktop Client Core.
    /// </summary>
    /// <remarks>
    /// This wrapper class is needed to export the information into the GroupMeClient.Core namespace.
    /// Otherwise it is assembly-local to the core component from GitInfo, under ThisAssembly.Git.
    /// </remarks>
    public class GlobalAssemblyInfo
    {
        /// <summary>
        /// Simple release-like version number, like 4.0.1.0.
        /// </summary>
        public const string SimpleVersion = ThisAssembly.Git.BaseVersion.Major + "." + ThisAssembly.Git.BaseVersion.Minor + "." + ThisAssembly.Git.BaseVersion.Patch + "." + ThisAssembly.Git.Commits;

        /// <summary>
        /// Full version, plus branch and commit short sha, like 4.0.1.0-39cf84e-branch.
        /// </summary>
        public const string InformationalVersion = SimpleVersion + "-" + ThisAssembly.Git.Commit + "+" + ThisAssembly.Git.Branch;
    }
}