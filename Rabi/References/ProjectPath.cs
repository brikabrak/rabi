namespace Rabi.References;

/// <summary>
/// Representation of a specific folder or file path that has exclusion properties.
/// </summary>
public class ProjectPath
{
    public static readonly ProjectPath Empty = new ProjectPath("") {
        ShouldExclude = false,
        AlreadyExcluded = false,
        DoNotExclude = false
    };

    public bool ShouldExclude { get; set; }
    public bool AlreadyExcluded { get; set; }
    public bool DoNotExclude { get; set; }
    public string Path { get; }

    public ProjectPath(string path) {
        Path = path;
    }
}
