using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using HaemophilusWeb.Domain;

namespace HaemophilusWeb.CustomAssertions;

public class IsolateInterpretationAssertion(IsolateInterpretation instance, AssertionChain assertionChain)
    : ReferenceTypeAssertions<IsolateInterpretation, IsolateInterpretationAssertion>(instance, assertionChain)
{
    protected override string Identifier => "IsolateInterpretation";

    public AndConstraint<IsolateInterpretationAssertion> HaveTyping(
        string attribute, string value,
        string because = "", params object[] becauseArgs)
    {
        var typing = Subject.Typings.FirstOrDefault(t => t.Attribute == attribute);

        assertionChain
            .ForCondition(typing != null)
            .BecauseOf(because, becauseArgs)
            .FailWith("Expected typings to contain an entry with Attribute {0}{reason}, but it was not found.", attribute);

        if (typing != null)
        {
            assertionChain
                .ForCondition(typing.Value == value)
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected typing with Attribute {0} to have Value {1}{reason}, but found {2}.",
                    attribute, value, typing.Value);
        }

        return new AndConstraint<IsolateInterpretationAssertion>(this);
    }
}
