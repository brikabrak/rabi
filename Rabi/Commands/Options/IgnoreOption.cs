using System.Collections.Generic;
using System.Linq;
using Rabi.Commands.Interfaces;
using Rabi.References;
using Rabi.Utility;

namespace Rabi.Commands.Options;

public class IgnoreOption : ICommandOption
{
    private string[] data;
    private IConsole console;

    public bool CanApply => data.Length > 0;

    public IgnoreOption(string[]? ignoreList, IConsole consoleWrapper)
    {
        data = ignoreList ?? [];
        console = consoleWrapper;
    }

    public List<ProjectReference> ApplyOption(List<ProjectReference> projectReferences, List<ExclusionReference> exclusionReferences)
    {
        if (!CanApply)
            return projectReferences;

        // Split ignore paths between assembly and any available folder/filepath
        var splitItems = OptionUtility.SplitAssemblyPaths(data);

        if (CommandHandler.IsVerbose)
        {
            console.WriteLine("-i - Ignore list passed in:");
            for (var i = 0; i < splitItems.Length; i++)
            {
                var split = splitItems[i];
                console.WriteLine($"\tProject: {split.Item1}{(!string.IsNullOrEmpty(split.Item2) ? $" Path: {split.Item2}" : "")}");
            }

            console.WriteLine($"-i - Matching assemblies from ignore list:");
        }

        // Left outer join between reference assemblies and split assembly matches
        var filteredRefs = projectReferences.GroupJoin(
            splitItems,
            r => r.ProjectAssembly,
            i => i.Item1,
            (r, i) => new { r, split = i.DefaultIfEmpty() }
        )
        .Select(gj => {
            var r = gj.r;

            foreach (var s in gj.split)
            {
                if (s == null) continue;

                var hasPath = false;
                if (!string.IsNullOrEmpty(s.Item2))
                {
                    hasPath = true;

                    if (r.TryGetPath(s.Item2, out var projPath))
                        projPath.ShouldExclude = true;
                    else
                    {
                        var newPath = new ProjectPath(s.Item2) {
                            ShouldExclude = true
                        };
                        r.Paths.Add(newPath);
                    }
                }
                else if (!r.ShouldExclude)
                    r.ShouldExclude = true;

                if (CommandHandler.IsVerbose)
                    console.WriteLine($"\tProject: {s.Item1}{(hasPath ? $" Path: {s.Item2}" : "")}");
            }

            return r;
        })
        .ToList();

        return filteredRefs;
    }
}