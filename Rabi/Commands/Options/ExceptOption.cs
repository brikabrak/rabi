using System;
using System.Collections.Generic;
using System.Linq;
using Rabi.Commands.Interfaces;
using Rabi.References;
using Rabi.Utility;

namespace Rabi.Commands.Options;

public class ExceptOption : ICommandOption
{
    private string[] data;
    private IConsole console;

    public bool CanApply => data.Length > 0;

    public ExceptOption(string[]? ignoreList, IConsole consoleWrapper)
    {
        data = ignoreList ?? [];
        console = consoleWrapper;
    }

    public List<ProjectReference> ApplyOption(List<ProjectReference> projectReferences, List<ExclusionReference> exclusionReferences)
    {
        if (!CanApply)
            return projectReferences;
        
        if (CommandHandler.IsVerbose)
            console.WriteLine($"-e - Matching assemblies from exclude list:");

        var splitItems = OptionUtility.SplitAssemblyPaths(data);

        return projectReferences.GroupJoin(
            exclusionReferences,
            pRef => pRef,
            e => e.ProjectRef,
            (p, e) => new { projRef = p, exclusionRef = e }
        )
        .GroupJoin(
            splitItems,
            j => j.projRef.ProjectAssembly,
            i => i.Item1,
            (j, i) => new { j.projRef, j.exclusionRef, split = i.DefaultIfEmpty() }
        )
        .Select(gj =>
        {
            foreach (var s in gj.split)
            {
                if (s == null) continue;

                var projRef = gj.projRef;
                var hasPath = false;
                if (!string.IsNullOrEmpty(s.Item2) && projRef.TryGetPath(s.Item2, out var projectPath)) {
                    hasPath = true;
                    projRef.Paths.Remove(projectPath);
                }
                else
                    projRef.DoNotExclude = true;

                if (CommandHandler.IsVerbose)
                    console.WriteLine($"\tProject: {s.Item1}{(hasPath ? $" Path: {s.Item2}" : "")}");
            }

            return gj.projRef;
        })
        .ToList();
    }
}