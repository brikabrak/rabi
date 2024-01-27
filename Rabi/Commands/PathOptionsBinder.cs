using System.CommandLine;
using System.CommandLine.Binding;
using System.Diagnostics.CodeAnalysis;

namespace Rabi.Commands;

[ExcludeFromCodeCoverage]
public class PathOptionsBinder : BinderBase<PathOptions>
{
    private readonly Option userFirst;

    public PathOptionsBinder(Option userFirstOpt) => userFirst = userFirstOpt;

    protected override PathOptions GetBoundValue(BindingContext bindingContext)
    {
        return new PathOptions(
            userFirst is Option<bool> userFirstOpt && bindingContext.ParseResult.GetValueForOption(userFirstOpt)
            );
    }
}