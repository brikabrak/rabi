using System.Diagnostics.CodeAnalysis;

namespace Rabi.Commands;

[ExcludeFromCodeCoverage]
public class PathOptions {
    public bool UserFirst { get; }

    public PathOptions(bool userFirst) {
        UserFirst = userFirst;
    }
}