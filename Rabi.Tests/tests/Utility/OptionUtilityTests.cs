using System;
using System.Linq;
using NUnit.Framework;
using Rabi.Utility;

namespace Rabi.Tests.Utility;

public class OptionUtilityTests
{
    [Test]
    public void SplitSetOfAssemblies()
    {
        var lines = new[]
        {
            "# something fake",
            "Some.csproj/Path/To/File.cs # Something else to talk about",
            "Some.csproj/Folder/Path",
            "Fake.csproj"
        };
        var expected = new Tuple<string, string>[]
        {
            new("Some.csproj", "Path\\To\\File.cs"),
            new("Some.csproj", "Folder\\Path"),
            new("Fake.csproj", string.Empty)
        }.Order().ToArray();
        var result = OptionUtility.SplitAssemblyPaths(lines);

        Assert.That(result, Is.EqualTo(expected));
    }
}
