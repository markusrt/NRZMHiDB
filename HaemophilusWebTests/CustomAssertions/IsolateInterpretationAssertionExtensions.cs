using FluentAssertions.Execution;
using HaemophilusWeb.Domain;

namespace HaemophilusWeb.CustomAssertions;

public static class IsolateInterpretationAssertionExtensions
{
    public static IsolateInterpretationAssertion Should(this IsolateInterpretation actualValue)
    {
        return new IsolateInterpretationAssertion(actualValue, AssertionChain.GetOrCreate());
    }
}
