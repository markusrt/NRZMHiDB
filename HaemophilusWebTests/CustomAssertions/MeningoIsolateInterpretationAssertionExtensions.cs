using FluentAssertions.Execution;
using HaemophilusWeb.Domain;

namespace HaemophilusWeb.CustomAssertions;

public static class MeningoIsolateInterpretationAssertionExtensions
{
    public static MeningoIsolateInterpretationAssertion Should(this MeningoIsolateInterpretation actualValue)
    {
        return new MeningoIsolateInterpretationAssertion(actualValue, AssertionChain.GetOrCreate());
    }
}
