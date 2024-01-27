using System.Collections.Generic;
using System.Linq;
using Rabi.Commands.Interfaces;
using Rabi.References;
using Rabi.Utility;

namespace Rabi.Commands.Options;

public class ExplicitIgnoreOption : ICommandOption
{
    private string[] data;
    private IConsole console;

    public bool CanApply => data.Length > 0;

    public ExplicitIgnoreOption(string[]? ignoreList, IConsole consoleWrapper)
    {
        data = ignoreList ?? [];
        console = consoleWrapper;
    }

    public List<ProjectReference> ApplyOption(List<ProjectReference> projectReferences, List<ExclusionReference> exclusionReferences)
    {
        if (!CanApply)
            return projectReferences;

        var splitItems = OptionUtility.SplitAssemblyPaths(data);

        if (CommandHandler.IsVerbose)
        {
            console.WriteLine("-e - Explicit Ignore list passed in:");

            for (var i = 0; i < splitItems.Length; i++)
            {
                var split = splitItems[i];
                console.WriteLine($"\tProject: {split.Item1}{(!string.IsNullOrEmpty(split.Item2) ? $" Path: {split.Item2}" : "")}");
            }
            
            console.WriteLine($"-e - Matching assemblies from exclude list:");
        }
        
        return projectReferences.GroupJoin(
                exclusionReferences,
                pRef => pRef,
                e => e.ProjectRef,
                (p, e) => new { projRef = p, exclusionRefs = e.ToArray() }
            )
            .GroupJoin(
                splitItems,
                j => j.projRef.ProjectAssembly,
                i => i.Item1,
                (j, i) => new { j.projRef, j.exclusionRefs, splits = i.ToArray() }
            )
            .Select(gj =>
            {
                var projRef = gj.projRef;

                if (gj.splits.Length == 0)
                {
                    projRef.DoNotExclude = true;
                    return projRef;
                }
                
                foreach (var s in gj.splits)
                {
                    var hasPath = false;
                    if (!string.IsNullOrEmpty(s.Item2))
                    {
                        if (projRef.TryGetPath(s.Item2, out var projPath))
                            projPath.ShouldExclude = true;
                        else
                        {
                            var newPath = new ProjectPath(s.Item2)
                            {
                                ShouldExclude = true
                            };
                            projRef.Paths.Add(newPath);
                        }
                    }
                    else if (!projRef.ShouldExclude)
                        projRef.ShouldExclude = true;

                    if (CommandHandler.IsVerbose)
                        console.WriteLine($"\tProject: {s.Item1}{(hasPath ? $" Path: {s.Item2}" : "")}");
                }

                return projRef;
            })
            .ToList();
    }
}