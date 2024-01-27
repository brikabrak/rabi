namespace Rabi.References;

public class ExclusionReference
{
    public ProjectReference ProjectRef { get; }
    public string PathRef { get; }

    public ExclusionReference(ProjectReference reference, string pathRef) {
        ProjectRef = reference;
        PathRef = pathRef;

        if (!string.IsNullOrEmpty(pathRef)) {
            var path = new ProjectPath(pathRef)
            {
                AlreadyExcluded = true
            };

            ProjectRef.Paths.Add(path);
        }
        else
            ProjectRef.AlreadyExcluded = true;
    }
}
