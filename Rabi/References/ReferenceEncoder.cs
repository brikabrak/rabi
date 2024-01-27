using System;

namespace Rabi.References;

public static class ReferenceEncoder
{
    public const string PATH_DECODED_SEPARATOR = "\\";
    private const string PATH_ENCODED_SEPARATOR = "_002Fd_003A";
    private const string PATH_FILE_ENCODED_SEPARATOR = "_002Ff_003A";

    /// <summary>
    /// Interesting hex values that Rider stores for GUID dashes and file separators.
    /// </summary>
    private static readonly Tuple<string, string>[] CharacterReplacements = {
        new("_002D", "-"),
        new("_002E", "."),
        new(PATH_ENCODED_SEPARATOR, PATH_DECODED_SEPARATOR),
        new(PATH_FILE_ENCODED_SEPARATOR, PATH_DECODED_SEPARATOR)
    };

    public static string Decode(string value) {
        foreach (var c in CharacterReplacements)
            value = value.Replace(c.Item1, c.Item2);

        return value;
    }

    public static string Encode(string value) {
        foreach (var c in CharacterReplacements) {
            if (c.Item1 == PATH_FILE_ENCODED_SEPARATOR) {
                var index = value.LastIndexOf(c.Item2, StringComparison.Ordinal);
                if (index == -1) continue;
                value = value.Remove(index, c.Item2.Length).Insert(index, c.Item2);
            }
            else
                value = value.Replace(c.Item2, c.Item1);
        }

        return value;
    }
}
