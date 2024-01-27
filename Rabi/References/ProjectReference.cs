using System.Collections.Generic;
namespace Rabi.References;

/// <summary>
/// A representation of a project reference inside a solution file.
/// </summary>
public class ProjectReference
{
    private bool doNotExclude;
    private HashSet<ProjectPath> paths;

    /// <summary>
    /// The base project name.
    /// </summary>
    public string Project { get; }
    /// <summary>
    /// The base project file path.
    /// </summary>
    public string ProjectFile { get; }
    /// <summary>
    /// Assembly naming scheme built from ProjectFile.
    /// </summary>
    public string ProjectAssembly { get; }
    /// <summary>
    /// Unique project reference stored in the Solution file.
    /// </summary>
    public string Guid { get; }
    /// <summary>
    /// Specific to Unity-based projects; notes that the project reference is a base Unity assembly for the main game project.
    /// </summary>
    public bool IsRootAssembly { get; }
    /// <summary>
    /// Assembly is already excluded in the DotSettings file. Useful to determine next steps for Should/DoNotExclude.
    /// </summary>
    public bool AlreadyExcluded { get; set; }
    /// <summary>
    /// Should add as an exclusion entry to the DotSettings file.
    /// </summary>
    public bool ShouldExclude { get; set; }
    /// <summary>
    /// Do not add as an exclusion entry to the DotSettings file.
    /// </summary>
    public bool DoNotExclude {
        get => doNotExclude;
        set {
            doNotExclude = value;

            if (!doNotExclude) return;

            AlreadyExcluded = false;
            ShouldExclude = false;
        }
    }
    /// <summary>
    /// Determined that this project reference is not using the GUID reference, but is using the Project name reference.
    /// </summary>
    public bool UseProjectFileAlias { get; set; }
    /// <summary>
    /// File and folder paths that are being excluded from analysis.
    /// </summary>
    public HashSet<ProjectPath> Paths => paths;

    public ProjectReference(string project, string projectFile, string guidString) {
        Project = project;
        ProjectFile = projectFile;
        Guid = guidString;

        ProjectAssembly = ProjectFile.Replace(".csproj", "");
        IsRootAssembly = ProjectFile.Contains("Assembly-CSharp");
        DoNotExclude = IsRootAssembly;

        paths = new HashSet<ProjectPath>();
    }

    public bool TryGetPath(string path, out ProjectPath projectPath)
    {
        foreach (var p in paths)
        {
            if (p.Path != path) continue;

            projectPath = p;
            return true;
        }

        projectPath = ProjectPath.Empty;
        return false;
    }
}
