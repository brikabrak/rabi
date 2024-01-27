using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Rabi.Commands;
using Rabi.Commands.Options;
using Rabi.References;

namespace Rabi.Tests.Commands.Options;

public class IgnoreOptionTests
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

        CommandHandler.IsVerbose = false;
        
        var option = new IgnoreOption(Array.Empty<string>(), consoleMock.Object);
        var result = option.ApplyOption(pRefs, null);
        Assert.That(result, Is.EqualTo(pRefs));
        consoleMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
        consoleMock.Reset();

        option = new IgnoreOption(null, consoleMock.Object);
        Assert.That(result, Is.EqualTo(pRefs));
        consoleMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
        consoleMock.Reset();
    }

    [Test]
    public void AdjustProjectReferenceWithIgnoreList()
    {
        var data = new string[] {"SecondAssembly"};
        var pRefs = new List<ProjectReference>()
        {
            new ProjectReference("SecondAssembly", "SecondAssembly", "someguid")
        };
        var exclusions = new List<ExclusionReference>();
    
        CommandHandler.IsVerbose = false;
    
        var option = new IgnoreOption(data, consoleMock.Object);
        var result = option.ApplyOption(pRefs, exclusions);
        Assert.That(result, Is.EqualTo(pRefs));
        Assert.That(result[0].DoNotExclude, Is.False);
        Assert.That(result[0].ShouldExclude, Is.True);
        consoleMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Never);
    }
    
    [Test]
    public void AdjustPathWithIgnoreList()
    {
        var path = "Some\\Folder\\Place";
        var data = new string[] {$"SecondAssembly\\{path}", "SecondAssembly\\Thing", "Blurb"};
        var pRefs = new List<ProjectReference>()
        {
            new ProjectReference("SecondAssembly", "SecondAssembly", "someguid")
        };
        var exclusions = new List<ExclusionReference>();
        pRefs[0].Paths.Add(new ProjectPath(path));
        
        CommandHandler.IsVerbose = true;
    
        Assert.That(pRefs[0].TryGetPath(path, out _), Is.True);
        var option = new IgnoreOption(data, consoleMock.Object);
        var result = option.ApplyOption(pRefs, exclusions);
        Assert.That(result, Is.EqualTo(pRefs));
        Assert.That(result[0].DoNotExclude, Is.False);
    
        var pathResult = result[0].TryGetPath(path, out var subPath);
        Assert.That(pathResult, Is.True);
        Assert.That(subPath.DoNotExclude, Is.False);
        Assert.That(subPath.ShouldExclude, Is.True);
        consoleMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Exactly(7));
    }
}