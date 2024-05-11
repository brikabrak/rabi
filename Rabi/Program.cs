using System;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Rabi.Commands;
using Rabi.Utility;

namespace Rabi;

[ExcludeFromCodeCoverage]
public static class Program
{
    private static ParserUtility parserUtility = new ParserUtility();
    private static IConsole console = new ConsoleWrapper();
    private static FileDataProvider dataProvider = new FileDataProvider();
    private static CommandHandler commandHandler = new CommandHandler(console);

    private static async Task<int> Main(string[] args) {
        var globalOpts = new Option[] {
            new Option<bool>(
                aliases: new string[] { "-v", "--verbose" },
                description: "All actions to be output to the console window, for those who are curious."
            )
        };

        var commandOpts = new Option[] {
            new Option<string[]?>(
                aliases: new string[] { "-i", "--ignore" },
                description: "The target file path of assemblies and internal file paths that should be ignored, one entry per line.\nIf the referenced projects exist in the project solution file, they will be appended into the DotSettings file.",
                parseArgument: parserUtility.ParseFileArgument
            ),
            new Option<string[]?>(
                aliases: new string[] { "-e", "--except" },
                description: "The target file path of assemblies and internal file paths that should not be ignored, one entry per line.\nThese entries will be removed from the DotSettings file. No other entries will be modified.\nThese values override values passed as part of --ignore.",
                parseArgument: parserUtility.ParseFileArgument
            ),
            new Option<string[]?>(
                aliases: new string[] { "-n", "--only-ignore" },
                description: "The target file path of the only assemblies and internal file paths that should be ignored, one entry per line.\nUsing this argument will clear all existing entries in your DotSolution file.\nThese values override values passed as part of --ignore and --except.\nCannot be used with default command mode.",
                parseArgument: parserUtility.ParseFileArgument
            ),
        };

        var baseCommand = new RootCommand("Rabi - Rider Analysis Bulk Ignore tool");

        SetPathCommand(baseCommand, commandOpts, globalOpts);

        foreach (var o in globalOpts)
            baseCommand.AddGlobalOption(o);

        return await baseCommand.InvokeAsync(args);
    }

    private static void SetPathCommand(RootCommand root, Option[] commandOptions, Option[] globalOptions) {
        var pathArg = new Argument<DirectoryInfo?>
        (
            name: "directory",
            description: "The target path that houses both the DotSettings file and the project solution .sln file. This will grab the first occurence of both.",
            parse: parserUtility.ParsePathArgument

        );
        var pathSub = new Command("path", "Find both the DotSettings and project solution files and apply changes.");
        pathSub.AddArgument(pathArg);

        var userOption = new Option<bool>(
            aliases: new string[] { "-u", "--user-first" },
            description: "Attempt to use the .DotSettings.user file located in the given path first before attempting to find any other files."
        );
        pathSub.AddOption(userOption);

        foreach (var o in commandOptions)
            pathSub.AddOption(o);

        pathSub.SetHandler(
            (pathArgVal, commandOpts, pathOpts, _) => {
                try
                {
                    if (pathArgVal == null)
                        throw new Exception("Directory passed does not exist. Please check your input.");

                    var settingLines = dataProvider.GetDataFromPath(
                        pathArgVal,
                        "*.DotSettings*",
                        pathOpts.UserFirst ? ".DotSettings.user" : string.Empty);
                    var solutionLines = dataProvider.GetDataFromPath(pathArgVal, "*.sln");

                    commandHandler.ApplyIgnore(settingLines, solutionLines, commandOpts);
                } catch (Exception e) {
                    Console.WriteLine($"Could not continue with the operation:\n{e.Message}");

                    return Task.FromResult(1);
                }

                return Task.FromResult(0);
            },
            pathArg,
            new CommandOptionsBinder(commandOptions[0], commandOptions[1], commandOptions[2], console),
            new PathOptionsBinder(userOption),
            new GlobalOptionsBinder(globalOptions[0])
        );

        root.AddCommand(pathSub);
    }
}
