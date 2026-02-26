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

}