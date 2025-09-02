using System;
using NUnit.Framework;
using DevMenuTool.Runtime;
using DevMenuTool.Runtime.Handlers;

namespace DevMenuTool.Tests.Runtime
{
    [TestFixture]
    public class ParameterHandlerRegistryTests
    {
        private ParameterHandlerRegistry _registry;

        [SetUp]
        public void Setup()
        {
            _registry = new ParameterHandlerRegistry();
        }

        [Test]
        public void GetHandler_WithStringType_ShouldReturnStringHandler()
        {
            var handler = _registry.GetHandler(typeof(string));
            Assert.That(handler, Is.InstanceOf<StringParameterHandler>());
        }

        [Test]
        public void GetHandler_WithIntType_ShouldReturnNumericHandler()
        {
            var handler = _registry.GetHandler(typeof(int));
            Assert.That(handler, Is.InstanceOf<NumericParameterHandler>());
        }

        [Test]
        public void GetHandler_WithFloatType_ShouldReturnNumericHandler()
        {
            var handler = _registry.GetHandler(typeof(float));
            Assert.That(handler, Is.InstanceOf<NumericParameterHandler>());
        }

        [Test]
        public void GetHandler_WithBoolType_ShouldReturnBooleanHandler()
        {
            var handler = _registry.GetHandler(typeof(bool));
            Assert.That(handler, Is.InstanceOf<BooleanParameterHandler>());
        }

        [Test]
        public void GetHandler_WithEnumType_ShouldReturnEnumHandler()
        {
            var handler = _registry.GetHandler(typeof(TestEnum));
            Assert.That(handler, Is.InstanceOf<EnumParameterHandler>());
        }

        [Test]
        public void GetHandler_WithUnsupportedType_ShouldReturnNull()
        {
            var handler = _registry.GetHandler(typeof(DateTime));
            Assert.That(handler, Is.Null);
        }

        [Test]
        public void IsSupported_WithSupportedType_ShouldReturnTrue()
        {
            Assert.That(_registry.IsSupported(typeof(string)), Is.True);
            Assert.That(_registry.IsSupported(typeof(int)), Is.True);
            Assert.That(_registry.IsSupported(typeof(float)), Is.True);
            Assert.That(_registry.IsSupported(typeof(bool)), Is.True);
            Assert.That(_registry.IsSupported(typeof(TestEnum)), Is.True);
        }

        [Test]
        public void IsSupported_WithUnsupportedType_ShouldReturnFalse()
        {
            Assert.That(_registry.IsSupported(typeof(DateTime)), Is.False);
            Assert.That(_registry.IsSupported(typeof(object)), Is.False);
        }

        [Test]
        public void Parse_WithValidString_ShouldParseCorrectly()
        {
            var result = _registry.Parse("test", typeof(string));
            Assert.That(result, Is.EqualTo("test"));
        }

        [Test]
        public void Parse_WithValidInt_ShouldParseCorrectly()
        {
            var result = _registry.Parse("42", typeof(int));
            Assert.That(result, Is.EqualTo(42));
        }

        [Test]
        public void Parse_WithValidFloat_ShouldParseCorrectly()
        {
            var result = _registry.Parse("3.14", typeof(float));
            Assert.That(result, Is.EqualTo(3.14f));
        }

        [Test]
        public void Parse_WithValidBool_ShouldParseCorrectly()
        {
            var result = _registry.Parse("true", typeof(bool));
            Assert.That(result, Is.True);
        }

        [Test]
        public void Parse_WithValidEnum_ShouldParseCorrectly()
        {
            var result = _registry.Parse("Value1", typeof(TestEnum));
            Assert.That(result, Is.EqualTo(TestEnum.Value1));
        }

        [Test]
        public void Parse_WithUnsupportedType_ShouldThrowException()
        {
            Assert.Throws<NotSupportedException>(() => _registry.Parse("test", typeof(DateTime)));
        }

        [Test]
        public void ToString_WithStringValue_ShouldReturnString()
        {
            var result = _registry.ToString("test");
            Assert.That(result, Is.EqualTo("test"));
        }

        [Test]
        public void ToString_WithIntValue_ShouldReturnString()
        {
            var result = _registry.ToString(42);
            Assert.That(result, Is.EqualTo("42"));
        }

        [Test]
        public void ToString_WithFloatValue_ShouldReturnString()
        {
            var result = _registry.ToString(3.14f);
            Assert.That(result, Is.EqualTo("3.14"));
        }

        [Test]
        public void ToString_WithBoolValue_ShouldReturnString()
        {
            var result = _registry.ToString(true);
            Assert.That(result, Is.EqualTo("True"));
        }

        [Test]
        public void ToString_WithEnumValue_ShouldReturnString()
        {
            var result = _registry.ToString(TestEnum.Value1);
            Assert.That(result, Is.EqualTo("Value1"));
        }

        [Test]
        public void ToString_WithNullValue_ShouldReturnEmptyString()
        {
            var result = _registry.ToString(null);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public void ToString_WithUnsupportedType_ShouldReturnToString()
        {
            var dateTime = new DateTime(2023, 1, 1);
            var result = _registry.ToString(dateTime);
            Assert.That(result, Is.EqualTo(dateTime.ToString()));
        }

        public enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }
    }
} 