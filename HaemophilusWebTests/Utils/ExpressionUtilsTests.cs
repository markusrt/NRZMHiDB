using FluentAssertions;
using HaemophilusWeb.TestDoubles;
using NUnit.Framework;

namespace HaemophilusWeb.Utils
{
    public class ExpressionUtilsTests
    {
        [Test]
        public void GetDisplayName_DisplayAttribute_ReturnsDisplayName()
        {
            ExpressionUtils.GetDisplayName<Isolate, object>(x => x.SamplingLocation).Should().Be("Entnahmeort");
            ExpressionUtils.GetDisplayName<Isolate, object>(x => x.Patient).Should().Be("Der Patient");
            ExpressionUtils.GetDisplayName<Isolate, object>(x => x.Patient.Age).Should().Be("Alter");
        }

        [Test]
        public void GetDisplayName_NoAttribute_ReturnsMemberName()
        {
            ExpressionUtils.GetDisplayName<Isolate, object>(x => x.Invasive).Should().Be("Invasive");
            ExpressionUtils.GetDisplayName<Isolate, object>(x => x.Patient.Initials).Should().Be("Initials");
        }

        [Test]
        public void GetDisplayName_MemberMethodCall_ReturnsMemberName()
        {
            ExpressionUtils.GetDisplayName<Isolate, object>(x => x.Invasive.ToLower()).Should().Be("Invasive");
            ExpressionUtils.GetDisplayName<Isolate, object>(x => x.SamplingLocation.ToLower())
                .Should().Be("Entnahmeort");
        }

        [Test]
        public void GetDisplayName_WrappedMethodCall_ReturnsMemberName()
        {
            ExpressionUtils.GetDisplayName<Isolate, object>(x => Identity(x.Invasive.ToLower())).Should().Be("Invasive");
            ExpressionUtils.GetDisplayName<Isolate, object>(x => Identity(x.SamplingLocation.ToLower()))
                .Should().Be("Entnahmeort");
        }

        [Test]
        public void GetDisplayName_DoubleWrappedMethodCall_ReturnsMemberName()
        {
            ExpressionUtils.GetDisplayName<Isolate, object>(x => Identity((double) x.Patient.Age))
                .Should().Be("Alter");
        }

        [Test]
        public void GetDisplayName_WrappedMethodCallWithTwoArguments_ReturnsMemberName()
        {
            ExpressionUtils.GetDisplayName<Isolate, object>(x => ConditionalReturn(x.Patient.Initials, x))
                .Should()
                .Be("Initials");
        }

        private static object Identity(object value)
        {
            return value;
        }

        private static string ConditionalReturn(string initials, Isolate isolate)
        {
            if (isolate.Invasive == "Yes")
            {
                return initials.ToLower();
            }
            return initials;
        }
    }
}