using System;
using FluentAssertions;
using NUnit.Framework;

namespace HaemophilusWeb.Utils
{
    public class ExceptionUtilsTests
    {
        private const string MessageContent = "Message Content";
        
        private static readonly Exception SingleException = new Exception(MessageContent); 

        private static readonly Exception WrappedException = new Exception("Foo", SingleException);

        
        [Test]
        public void AnyMessageMentions_NullException_DoesNotThrow()
        {
            ExceptionUtils.AnyMessageMentions(null, MessageContent).Should().BeFalse();
        }

        [Test]
        public void AnyMessageMentions_SingleException_FindsValue()
        {
            SingleException.AnyMessageMentions(MessageContent).Should().BeTrue();
        }

        [Test]
        public void AnyMessageMentions_WrappedException_FindsValue()
        {
            SingleException.AnyMessageMentions(MessageContent).Should().BeTrue();
        }
    }
}