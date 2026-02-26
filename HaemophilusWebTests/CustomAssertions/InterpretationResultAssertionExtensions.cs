using FluentAssertions.Execution;
using HaemophilusWeb.Domain;

namespace HaemophilusWeb.CustomAssertions;

public static class InterpretationResultAssertionExtensions
{
    public static InterpretationResultAssertion Should(this InterpretationResult actualValue)
    {
        return new InterpretationResultAssertion(actualValue, AssertionChain.GetOrCreate());
    }
}