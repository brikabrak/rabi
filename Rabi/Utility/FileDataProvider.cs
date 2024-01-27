using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Rabi.Utility;

[ExcludeFromCodeCoverage]
public class FileDataProvider
{
    public string[] GetDataFromPath(DirectoryInfo path, string searchPattern, string fileEndsWith = "")
    {
        var files = path.GetFiles(searchPattern, SearchOption.TopDirectoryOnly);

        if (files.Length == 0)
            throw new Exception($"No {searchPattern} files available at the given path.");

        FileInfo targetFile;
        if (!string.IsNullOrEmpty(fileEndsWith))
            targetFile = files.FirstOrDefault(f => f.FullName.EndsWith(fileEndsWith)) ?? files[0];
        else
            targetFile = files[0];

        return File.ReadLines(targetFile.FullName).ToArray();
    }
}