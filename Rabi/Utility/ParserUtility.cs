using System;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace Rabi.Utility;

[ExcludeFromCodeCoverage]
public class ParserUtility {
    public string[]? ParseFileArgument(ArgumentResult result) {
        if (result.Tokens.Count == 0) {
            result.ErrorMessage = "No path provided.";
            return null;
        }

        string? filePath = result.Tokens.SingleOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath)) {
            result.ErrorMessage = $"The argument given is null, empty, or does not exist.\nArgument: {filePath}";
            return null;
        }

        return File.ReadAllLines(filePath);
    }

    public DirectoryInfo? ParsePathArgument(ArgumentResult result) {
        if (result.Tokens.Count == 0) {
            result.ErrorMessage = "No path provided.";
            return null;
        }

        string? filePath = result.Tokens.SingleOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(filePath) || !Directory.Exists(filePath)) {
            result.ErrorMessage = $"The argument given is null, empty, or does not exist.\nArgument: {filePath}";
            return null;
        }

        DirectoryInfo? info;
        try {
            info = new DirectoryInfo(filePath);
        } catch (Exception e) {
            Console.WriteLine(e.Message);
            return null;
        }

        return info;
    }

    public FileInfo? ParseFilePathArgument(ArgumentResult result) {
        if (result.Tokens.Count == 0) {
            result.ErrorMessage = "No path provided.";
            return null;
        }

        string? filePath = result.Tokens.SingleOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(filePath)) {
            result.ErrorMessage = $"The argument given is null or empty.\nArgument: {filePath}";
            return null;
        }
        
        FileInfo? info;
        try {
            info = new FileInfo(filePath);
        } catch (Exception e) {
            Console.WriteLine(e.Message);
            return null;
        }

        return info;
    }
}
