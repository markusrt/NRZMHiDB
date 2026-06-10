using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using HaemophilusWeb.Domain;

namespace HaemophilusWeb.CustomAssertions;

public class MeningoIsolateInterpretationAssertion(MeningoIsolateInterpretation instance, AssertionChain assertionChain)
    : ReferenceTypeAssertions<MeningoIsolateInterpretation, MeningoIsolateInterpretationAssertion>(instance, assertionChain)
{
    protected override string Identifier => "MeningoIsolateInterpretation";

    public AndConstraint<MeningoIsolateInterpretationAssertion> HaveTyping(
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

        return new AndConstraint<MeningoIsolateInterpretationAssertion>(this);
    }

    public AndConstraint<MeningoIsolateInterpretationAssertion> HaveTypingContaining(
        string attribute, string expectedSubstring,
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
                .ForCondition(typing.Value.Contains(expectedSubstring))
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected typing with Attribute {0} to have a Value containing {1}{reason}, but found {2}.",
                    attribute, expectedSubstring, typing.Value);
        }

        return new AndConstraint<MeningoIsolateInterpretationAssertion>(this);
    }

    public AndConstraint<MeningoIsolateInterpretationAssertion> HaveTypingMatching(
        string attribute, Func<string, bool> predicate,
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
                .ForCondition(predicate(typing.Value))
                .BecauseOf(because, becauseArgs)
                .FailWith("Expected typing with Attribute {0} to have a Value matching the predicate{reason}, but found {1}.",
                    attribute, typing.Value);
        }

        return new AndConstraint<MeningoIsolateInterpretationAssertion>(this);
    }
}
