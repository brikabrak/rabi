using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Rabi.Commands.Interfaces;
using Rabi.References;

namespace Rabi.Commands;

public class CommandHandler
{
    public const string ENTRY_TEMPLATE = @"<s:String x:Key=""/Default/CodeInspection/ExcludedFiles/FilesAndFoldersToSkip2/=_VALUE_/@EntryIndexedValue"">ExplicitlyExcluded</s:String>";
    private const string DS_FILE_END = "</wpf:ResourceDictionary>";

    /// <summary>
    /// Command will log all information happening as part of the process.
    /// </summary>
    public static bool IsVerbose = false;

    private IConsole console;

    public CommandHandler(IConsole consoleWrapper)
    {
        console = consoleWrapper;
    }

    public void ApplyIgnore(string[] settingsLines, string[] solutionLines, ICommandOption[] commandOptions)
    {
        if (IsVerbose)
            console.WriteLine("rabi - Performing ignore operations\nSolution Project References:");

        // Get all lines from solution and strip down to the project references
        var projectRefs = GetExistingSolutionReferences(solutionLines);

        // Get existing excluded project references from DotSettings file along with all settings lines
        var settingLines = settingsLines.ToList();
        var exclusionRefs = GetExistingExclusionReferences(settingLines, projectRefs);

        settingLines.RemoveAll(IsSettingLineExclusion);
        SeparateSettingsFileEnding(settingLines);

        // Apply first available command option, or bulk ignore all existing references
        var opt = commandOptions.FirstOrDefault(o => o.CanApply);
        if (opt != null)
            projectRefs = opt.ApplyOption(projectRefs, exclusionRefs);
        else
        {
            foreach (var pRef in projectRefs)
                pRef.ShouldExclude = true;
        }

        var removalCount = CountProjectRemovals(projectRefs);
        var newExclusionCount = CountProjectExclusions(projectRefs);

        // Modify Settings with Exclusions
        var lastIndex = settingLines.Count - 1;
        foreach (var pRef in projectRefs)
        {
            if (pRef.IsRootAssembly && pRef.Paths.Count == 0) continue;

            var basePath = pRef.UseProjectFileAlias
                ? pRef.ProjectFile.Split(Path.PathSeparator).Last()
                : pRef.Guid.ToUpper();

            if (pRef.Paths.Count > 0)
            {
                foreach (var path in pRef.Paths)
                {
                    if (!path.DoNotExclude && (path.ShouldExclude || path.AlreadyExcluded))
                        settingLines.Insert(++lastIndex, $"\t{ENTRY_TEMPLATE.Replace("_VALUE_", ReferenceEncoder.Encode($"{basePath}\\{path.Path}"))}");
                }
            }

            if (!pRef.DoNotExclude && (pRef.ShouldExclude || pRef.AlreadyExcluded))
                settingLines.Insert(++lastIndex, $"\t{ENTRY_TEMPLATE.Replace("_VALUE_", ReferenceEncoder.Encode(basePath))}");
        }

        settingLines.Add(DS_FILE_END);

        if (IsVerbose)
        {
            console.WriteLine($"Would add {newExclusionCount} reference(s) to DotSettings output.");
            console.WriteLine($"Would remove {removalCount} reference(s) from DotSettings output.");
            console.WriteLine("\n--- FINAL OUTPUT ---\n");
        }

        foreach (var l in settingLines)
            console.WriteLine(l);
    }

    private List<ProjectReference> GetExistingSolutionReferences(IEnumerable<string> solutionLines)
    {
        var pattern = new Regex(@"^Project\("".+\s=\s""([\w\.\-]+)"",\s""([\w\.\-\\]+)"".+\{([\w\-]+)\}.+$");
        var projectRefs = solutionLines.Where(l => l.StartsWith("Project(\""))
            .Order()
            .Select(l => {
                var match = pattern.Match(l);
                var proj = match.Groups[1].Captures[0].Value;
                var projFile = match.Groups[2].Captures[0].Value;
                var refer = match.Groups[3].Captures[0].Value.ToUpper();

                if (IsVerbose)
                    console.WriteLine($"\tProject: {proj}, File: {projFile} <{refer}>");

                var pRef = new ProjectReference(proj, projFile, refer);

                return pRef;
            }).ToList();

        return projectRefs;
    }

    private List<ExclusionReference> GetExistingExclusionReferences(
        List<string> settingsLines,
        List<ProjectReference> projectRefs
    ) {
        var excludePart = ENTRY_TEMPLATE[..ENTRY_TEMPLATE.IndexOf("_VALUE_", StringComparison.Ordinal)];
        var pattern = new Regex($@"^\s*{excludePart}([0-9a-zA-Z_\.]+)");
        var exclusionRefs = settingsLines
            .Where(IsSettingLineExclusion)
            .Select(l =>
            {
                // Pull matching excluded assembly references
                var match = pattern.Match(l);

                // Replace characters for GUID and path parsing
                var pulledRef = ReferenceEncoder.Decode(match.Groups[1].Captures[0].Value);
                var pathRef = string.Empty;

                var splitRef = pulledRef.Split(ReferenceEncoder.PATH_DECODED_SEPARATOR, 2);
                if (splitRef.Length > 1)
                {
                    pulledRef = splitRef[0];
                    pathRef = splitRef[1];
                }

                var useProjectFile = false;
                // check if reference is project name or guid from sln file
                var foundRef = projectRefs.FirstOrDefault(p =>
                {
                    if (p.Guid == pulledRef)
                        return true;
                    if (p.Project == pulledRef)
                    {
                        useProjectFile = true;
                        return true;
                    }

                    return false;
                });

                if (foundRef == null)
                    return null;

                foundRef.UseProjectFileAlias = useProjectFile;

                // If a ref isn't found, disregard
                return new ExclusionReference(foundRef, pathRef);
            })
            .OfType<ExclusionReference>()
            .OrderBy(r => r.ProjectRef.ProjectFile)
            .ToList();

        if (IsVerbose) {
            console.WriteLine("Existing Project Exclusions");
            for (var i = 0; i < exclusionRefs.Count; i++)
            {
                var exclusionRef = exclusionRefs[i];
                var projRef = exclusionRef.ProjectRef;
                var pathRef = exclusionRef.PathRef;
                console.WriteLine($"\tFile: {projRef.ProjectFile} <{projRef.Guid}{(!string.IsNullOrEmpty(pathRef) ? $"\\{pathRef}" : "")}>");
            }
        }

        return exclusionRefs;
    }

    private int CountProjectRemovals(List<ProjectReference> projectReferences)
    {
        var modifications = projectReferences.Sum(p => {
            var count = p.Paths.Sum(path => path.DoNotExclude ? 1 : 0);
            if (p.DoNotExclude && !p.IsRootAssembly)
                count++;

            return count;
        });

        return modifications;
    }

    private int CountProjectExclusions(List<ProjectReference> projectReferences)
    {
        var modifications = projectReferences.Sum(p => {
            var count = p.Paths.Sum(path => path.ShouldExclude && !path.AlreadyExcluded ? 1 : 0);
            if (p.ShouldExclude && !p.AlreadyExcluded)
                count++;

            return count;
        });

        return modifications;
    }

    private void SeparateSettingsFileEnding(List<string> settingLines)
    {
        var lastSetting = settingLines[^1];
        if (lastSetting.Count(c => c == '<') > 1)
        {
            lastSetting = lastSetting[..lastSetting.IndexOf(DS_FILE_END, StringComparison.Ordinal)];
            settingLines.RemoveAt(settingLines.Count - 1);
            settingLines.Add(lastSetting);
        }
    }

    private bool IsSettingLineExclusion(string line)
        => line.Contains("CodeInspection/ExcludedFiles") && line.Contains("ExplicitlyExcluded");
}
