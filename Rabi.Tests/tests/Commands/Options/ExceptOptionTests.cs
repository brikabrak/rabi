using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Rabi.Commands;
using Rabi.Commands.Options;
using Rabi.References;

namespace Rabi.Tests.Commands.Options;

[TestFixture]
public class ExceptOptionTests
{
    private Mock<IConsole> consoleMock = new Mock<IConsole>();

    [TearDown]
    public void TearDown()
    {
        consoleMock.Reset();
        CommandHandler.IsVerbose = false;
    }

    [Test]
    public void CanApplyBasedOffDataPassed()
    {
        var pRefs = new List<ProjectReference>
        {
            new ProjectReference("a", "b", Guid.NewGuid().ToString())
        };

        var option = new ExceptOption(Array.Empty<string>(), consoleMock.Object);
        var result = option.ApplyOption(pRefs, null);
        Assert.That(result, Is.EqualTo(pRefs));
        consoleMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
        consoleMock.Reset();

        option = new ExceptOption(null, consoleMock.Object);
        Assert.That(result, Is.EqualTo(pRefs));
        consoleMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
        consoleMock.Reset();
    }

    [Test]
    public void AdjustProjectReferenceWithExceptionList()
    {
        var data = new string[] {"FirstAssembly", "SecondAssembly"};
        var pRefs = new List<ProjectReference>()
        {
            new ProjectReference("SecondAssembly", "SecondAssembly", "someguid"),
            new ProjectReference("ExtraAssembly", "ExtraAssembly", "someotherguid")
        };
        var exclusions = new List<ExclusionReference>()
        {
            new ExclusionReference(pRefs[0], null)
        };

        CommandHandler.IsVerbose = true;

        var option = new ExceptOption(data, consoleMock.Object);
        var result = option.ApplyOption(pRefs, exclusions);
        Assert.That(result, Is.EqualTo(pRefs));
        Assert.That(result[0].DoNotExclude, Is.True);
        consoleMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Exactly(2));
    }

    [Test]
    public void AdjustPathWithExceptionList()
    {
        var path = "Some\\Folder\\Place";
        var data = new string[] {$"SecondAssembly\\{path}"};
        var pRefs = new List<ProjectReference>()
        {
            new ProjectReference("SecondAssembly", "SecondAssembly", "someguid"),
            new ProjectReference("ExtraAssembly", "ExtraAssembly", "someotherguid")
        };
        var exclusions = new List<ExclusionReference>()
        {
            new ExclusionReference(pRefs[0], path)
        };

        CommandHandler.IsVerbose = true;

        Assert.That(pRefs[0].TryGetPath(path, out _), Is.True);
        var option = new ExceptOption(data, consoleMock.Object);
        var result = option.ApplyOption(pRefs, exclusions);
        Assert.That(result, Is.EqualTo(pRefs));
        Assert.That(result[0].DoNotExclude, Is.False);
        Assert.That(result[0].TryGetPath(path, out _), Is.False);
        consoleMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Exactly(2));
    }
}