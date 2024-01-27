using System;
using System.Linq;
using Rabi.References;

namespace Rabi.Utility;

public static class OptionUtility
{
    public static Tuple<string, string>[] SplitAssemblyPaths(string[] data)
    {
        return data
        .Distinct()
        .Select(i => {
            // Remove or split out comments
            var commentPos = i.IndexOf('#');
            if (commentPos == 0)
                return null;
            if (commentPos > 0)
                i = i[..commentPos];

            if (i.Contains('/'))
                i = i.Replace("/", ReferenceEncoder.PATH_DECODED_SEPARATOR);

            i = i.TrimEnd();

            var split = !string.IsNullOrWhiteSpace(i) ? i.Split(ReferenceEncoder.PATH_DECODED_SEPARATOR, 2) : [];

            if (split.Length == 1)
                return new Tuple<string, string>(split[0], string.Empty);

            return split.Length > 1 ? new Tuple<string, string>(split[0], split[1]) : null;
        })
        .Order()
        .OfType<Tuple<string, string>>()
        .ToArray();
    }
}