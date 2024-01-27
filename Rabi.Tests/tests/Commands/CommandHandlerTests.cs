using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Rabi.Commands;
using Rabi.Commands.Interfaces;
using Rabi.References;

namespace Rabi.Tests.Commands;

public class CommandHandlerTests
{
    private Mock<IConsole> consoleMock = new Mock<IConsole>();

    private string[] solutionLines = new[]
    {
        "some line we don't care about",
        "Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79JGOG}\") = \"SomethingElse\", \"SomethingElse.csproj\", \"{96AC2222-744B-4506-BD5D-27CFA69E8948}\"",
        "Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"SomeSubProj\", \"SomeSubProj\\SomeSubProj.csproj\", \"{96AC2851-744B-4506-BD5D-27CFA69E8948}\"",
        "Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79AAAA}\") = \"Assembly-CSharp\", \"Assembly-CSharp.csproj\", \"{12345678-744B-4506-BD5D-27CFA69E8948}\"",
    };

    [SetUp]
    public void SetUp()
    {
        consoleMock.Reset();
        consoleMock.Setup(x => x.WriteLine(It.IsAny<string>())).Callback<string>(TestContext.WriteLine);

        CommandHandler.IsVerbose = false;
    }

    [Test]
    public void ApplyBasicIgnore()
    {
        CommandHandler.IsVerbose = true;

        var handler = new CommandHandler(consoleMock.Object);
        var settingLines = new[]
        {
            "Starting line",
            "<someline /></wpf:ResourceDictionary>"
        };

        handler.ApplyIgnore(settingLines, solutionLines, []);
        consoleMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.AtLeast(9));
        consoleMock.Verify(c => c.WriteLine(It.IsRegex("96AC2851.*ExplicitlyExcluded")), Times.Once);
    }

    [Test]
    public void ApplyIgnoreViaOption()
    {
        CommandHandler.IsVerbose = false;

        var optionMock = new Mock<ICommandOption>();
        optionMock.Setup(o => o.ApplyOption(It.IsAny<List<ProjectReference>>(), It.IsAny<List<ExclusionReference>>()))
            .Returns((List<ProjectReference> a, List<ExclusionReference> _) =>
            {
                a[^1].DoNotExclude = true;
                return a;
            });
        optionMock.Setup(o => o.CanApply).Returns(true);

        var handler = new CommandHandler(consoleMock.Object);
        var settingLines = new[]
        {
            "Starting line",
            "<someline /></wpf:ResourceDictionary>"
        };

        handler.ApplyIgnore(settingLines, solutionLines, [optionMock.Object]);
        consoleMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.AtLeast(3));
        consoleMock.Verify(c => c.WriteLine(It.IsRegex("96AC2851.*ExplicitlyExcluded")), Times.Never);
    }

    [Test]
    public void ApplyIgnoreViaPaths()
    {
        CommandHandler.IsVerbose = true;

        var handler = new CommandHandler(consoleMock.Object);
        var settingLines = new[]
        {
            "Starting line",
            CommandHandler.ENTRY_TEMPLATE.Replace("_VALUE_", ReferenceEncoder.Encode("fake\\something")),
            CommandHandler.ENTRY_TEMPLATE.Replace("_VALUE_", ReferenceEncoder.Encode("96AC2851-744B-4506-BD5D-27CFA69E8948\\AThing")),
            CommandHandler.ENTRY_TEMPLATE.Replace("_VALUE_", ReferenceEncoder.Encode("SomeSubProj\\SomeFolder")),
            CommandHandler.ENTRY_TEMPLATE.Replace("_VALUE_", ReferenceEncoder.Encode("SomeSubProj\\SomeFile.cs")),
            CommandHandler.ENTRY_TEMPLATE.Replace("_VALUE_", ReferenceEncoder.Encode("SomeSubProj\\SomeNestedFolder\\SomeOtherFile.cs")),
            "<someline /></wpf:ResourceDictionary>"
        };

        handler.ApplyIgnore(settingLines, solutionLines, []);
        consoleMock.Verify(c => c.WriteLine(It.IsAny<string>()), Times.Exactly(21));
    }
}