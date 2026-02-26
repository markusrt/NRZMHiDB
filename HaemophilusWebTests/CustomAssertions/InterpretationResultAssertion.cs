using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using HaemophilusWeb.Domain;

namespace HaemophilusWeb.CustomAssertions;

public class InterpretationResultAssertion(InterpretationResult instance, AssertionChain assertionChain)
    : ReferenceTypeAssertions<InterpretationResult, InterpretationResultAssertion>(instance, assertionChain)
{
    protected override string Identifier => "InterpretationResult";


    public AndConstraint<InterpretationResultAssertion> BePreliminary(string because = "", params object[] becauseArgs)
    {
        assertionChain
            .ForCondition(Subject.Preliminary)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected interpretation result to be preliminary", true, Subject);

        return new AndConstraint<InterpretationResultAssertion>(this);
    }

    public AndConstraint<InterpretationResultAssertion> NotBePreliminary(string because = "", params object[] becauseArgs)
    {
        assertionChain
            .ForCondition(Subject.Preliminary == false)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected interpretation result not to be preliminary", true, Subject);

        return new AndConstraint<InterpretationResultAssertion>(this);
    }

    public AndConstraint<InterpretationResultAssertion> ContainReportLine(
        string expectedSubstring, string because = "", params object[] becauseArgs)
    {
        assertionChain
            .ForCondition(Subject.Report != null)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected Report not to be null{reason}.");

        if (Subject.Report != null)
        {
            assertionChain
                .ForCondition(Subject.Report.Any(s => s.Contains(expectedSubstring)))
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected Report to contain a line matching {0}{reason}, but none was found.",
                    expectedSubstring);
        }

        return new AndConstraint<InterpretationResultAssertion>(this);
    }

}