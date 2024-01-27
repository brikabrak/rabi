using System.Collections.Generic;
using Rabi.References;

namespace Rabi.Commands.Interfaces;

public interface ICommandOption
{
    bool CanApply { get; }
    List<ProjectReference> ApplyOption(List<ProjectReference> projectReferences, List<ExclusionReference> exclusionReferences);
}