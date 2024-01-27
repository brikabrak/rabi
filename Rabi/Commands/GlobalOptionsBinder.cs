using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Rabi.Commands.Interfaces;

namespace Rabi.Commands;

[ExcludeFromCodeCoverage]
public class GlobalOptionsBinder : BinderBase<ICommandOption[]>
{
    private readonly List<Option> cache;
    private ParseResult? parseResult;

    public GlobalOptionsBinder(
        Option isVerboseOpt
    ) {
        cache = new List<Option>
        {
            isVerboseOpt
        };
    }

    protected override ICommandOption[] GetBoundValue(BindingContext bindingContext)
    {
        parseResult = bindingContext.ParseResult;

        CommandHandler.IsVerbose = cache[0] is Option<bool> a && parseResult.GetValueForOption(a);

        return Array.Empty<ICommandOption>();
    }
}
