using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using Rabi.Commands.Interfaces;
using Rabi.Commands.Options;

namespace Rabi.Commands;

[ExcludeFromCodeCoverage]
public class CommandOptionsBinder : BinderBase<ICommandOption[]>
{
    private readonly List<Option> cache;
    private readonly List<ICommandOption> commandOptions;
    private ParseResult? parseResult;
    private IConsole console;

    public CommandOptionsBinder(
        Option acceptAllOpt,
        Option verboseOpt, 
        Option dryrunOpt,
        IConsole consoleWrapper
    ) {
        commandOptions = new List<ICommandOption>();
        cache = new List<Option>
        {
            acceptAllOpt,
            verboseOpt, 
            dryrunOpt
        };

        console = consoleWrapper;            
    }

    protected override ICommandOption[] GetBoundValue(BindingContext bindingContext)
    {
        parseResult = bindingContext.ParseResult;

        AddCommandOption<string[]>(cache[0], data => new IgnoreOption(data, console));
        AddCommandOption<string[]>(cache[1], data => new ExceptOption(data, console));
        AddCommandOption<string[]>(cache[2], data => new ExplicitIgnoreOption(data, console));

        return commandOptions.ToArray();
    }

    private void AddCommandOption<T1>(Option option, Func<T1, ICommandOption> optionFactory)
    {
        if (option is Option<T1> optionData && parseResult != null)
        {
            var data = parseResult.GetValueForOption(optionData);
            if (data != null)
                commandOptions.Add(optionFactory(data));
        }
    }
}
