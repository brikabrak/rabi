# Rabi

AKA **Rider Analysis Bulk Ignore** - an adjustment tool for ignoring projects, libraries, and files from analysis in `DotSettings` files.

## About

This tool is a way to get around a very annoying [issue](https://youtrack.jetbrains.com/issue/RIDER-43746/Inspection-Settings.-Elements-to-skip.-Only-one-item-can-be-added-without-reopening-the-settings) with [JetBrains Rider](https://www.jetbrains.com/rider/), an extremely powerful IDE whose quirks with settings files leave a bit to be desired at times.

Rider has a code analysis tool that users can enable or disable, which will scan a project and highlight various issues based on that project's severity settings. This process scans all assemblies linked to the project's solution file, and includes their issues in its analysis. Not only can this bog down a system, it can effectively give a developer information from a closed library or assembly they cannot hope to fix, which just turns into noise.

**Rabi**, or the **Rider Analysis Bulk Ignore** tool, is a simple command-line tool that adds all assemblies listed in a solution file to the project's DotSettings file under `Inspection Settings -> Elements to Skip -> Files and Folders`.

Could this have been a Rider/Resharper plugin? Yea, probably. It could be forked or modified to fit that use case still, but for now here is a working version.

## How to Use

**Note**: Requires .NET 8.0.

### Commands:

| Comand             |Optional Parameters| Example      | Description         |
| :-------------------- | :---| :---------- | :------------------ |
| `path` | `-u`<br>`--user-first` | `rabi path C:/ProjectRoot`<br>`rabi path C:/ProjectRoot -u` | The target path that houses both the `DotSettings` file and the project solution `sln` file. This will grab the first occurence of both.<br>If `-u\|--user-first` is provided, rabi will attempt to use the `.DotSettings.user` file located in the given path first before attempting to find any other files. |

Rabi's output will match a `DotSettings` file, ignoring ALL assemblies listed in the provided/found solution file. This is the standard use-case and should allow users to go into their settings and explicitly remove the few assemblies they may want to have analysis of. Ignored entries are unique; if an entry already exists in the `DotSettings` file, it will stay that way.
**Note**: Output can be piped or copied to a file. User can then keep their original settings file as a backup before switching.

### Command Parameters:

These parameters adjust the final result of the `DotSettings` file based on their criteria. They cannot be combined and will bypass the default command action:

| Parameter             | Default      | Description         |
| :-------------------- | :---------- | :------------------ |
| `-i\|--ignore` || The target file path of assemblies and internal file paths that should be ignored, one entry per line. If the referenced projects exist in the project solution file, they will be appended into the `DotSettings` file. |
| `-e\|--except` || The target file path of assemblies and internal file paths that should not be ignored, one entry per line. These assemblies will be removed from the `DotSettings` file. No other assembly entries will be modified. |
| `-n\|--only-ignore` || The target file path of the only assemblies and internal file paths that should be ignored, one entry per line. Using this argument will clear all existing entries in your `DotSolution` file. |

### Optional Global Parameters:

All of these can be used with any of the commands above unless otherwise specified:

| Parameter             | Default      | Description         |
| :-------------------- | :---------- | :------------------ |
| `-v\|--verbose` || A flag that will cause all actions to be output to the console window, for those who are curious. |

* * *

&copy; 2023 Blake Farrugia, see LICENSE.md for details.
