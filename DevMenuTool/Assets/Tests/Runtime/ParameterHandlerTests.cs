using System;
using NUnit.Framework;
using DevMenuTool.Runtime.Handlers;
using DevMenuTool.Runtime.Interfaces;

namespace DevMenuTool.Tests.Runtime
{
    [TestFixture]
    public class ParameterHandlerTests
    {
        private StringParameterHandler _stringHandler;
        private NumericParameterHandler _numericHandler;
        private BooleanParameterHandler _booleanHandler;
        private EnumParameterHandler _enumHandler;

        [SetUp]
        public void Setup()
        {
            _stringHandler = new StringParameterHandler();
            _numericHandler = new NumericParameterHandler();
            _booleanHandler = new BooleanParameterHandler();
            _enumHandler = new EnumParameterHandler();
        }

        [TestFixture]
        public class StringParameterHandlerTests
        {
            private StringParameterHandler _handler;

            [SetUp]
            public void Setup()
            {
                _handler = new StringParameterHandler();
            }

            [Test]
            public void CanHandle_WithStringType_ShouldReturnTrue()
            {
                Assert.That(_handler.CanHandle(typeof(string)), Is.True);
            }

            [Test]
            public void CanHandle_WithNonStringType_ShouldReturnFalse()
            {
                Assert.That(_handler.CanHandle(typeof(int)), Is.False);
            }

            [Test]
            public void Parse_WithValidString_ShouldReturnSameString()
            {
                var result = _handler.Parse("test", typeof(string));
                Assert.That(result, Is.EqualTo("test"));
            }

            [Test]
            public void ToString_WithStringValue_ShouldReturnString()
            {
                var result = _handler.ToString("test");
                Assert.That(result, Is.EqualTo("test"));
            }

            [Test]
            public void ToString_WithNullValue_ShouldReturnEmptyString()
            {
                var result = _handler.ToString(null);
                Assert.That(result, Is.EqualTo(string.Empty));
            }
        }

        [TestFixture]
        public class NumericParameterHandlerTests
        {
            private NumericParameterHandler _handler;

            [SetUp]
            public void Setup()
            {
                _handler = new NumericParameterHandler();
            }

            [Test]
            public void CanHandle_WithIntType_ShouldReturnTrue()
            {
                Assert.That(_handler.CanHandle(typeof(int)), Is.True);
            }

            [Test]
            public void CanHandle_WithFloatType_ShouldReturnTrue()
            {
                Assert.That(_handler.CanHandle(typeof(float)), Is.True);
            }

            [Test]
            public void CanHandle_WithNonNumericType_ShouldReturnFalse()
            {
                Assert.That(_handler.CanHandle(typeof(string)), Is.False);
            }

            [Test]
            public void Parse_WithValidInt_ShouldReturnInt()
            {
                var result = _handler.Parse("42", typeof(int));
                Assert.That(result, Is.EqualTo(42));
            }

            [Test]
            public void Parse_WithValidFloat_ShouldReturnFloat()
            {
                var result = _handler.Parse("3.14", typeof(float));
                Assert.That(result, Is.EqualTo(3.14f));
            }

            [Test]
            public void Parse_WithInvalidNumber_ShouldThrowException()
            {
                Assert.Throws<FormatException>(() => _handler.Parse("invalid", typeof(int)));
            }

            [Test]
            public void Parse_WithUnsupportedType_ShouldThrowException()
            {
                Assert.Throws<NotSupportedException>(() => _handler.Parse("test", typeof(string)));
            }
        }

        [TestFixture]
        public class BooleanParameterHandlerTests
        {
            private BooleanParameterHandler _handler;

            [SetUp]
            public void Setup()
            {
                _handler = new BooleanParameterHandler();
            }

            [Test]
            public void CanHandle_WithBoolType_ShouldReturnTrue()
            {
                Assert.That(_handler.CanHandle(typeof(bool)), Is.True);
            }

            [Test]
            public void CanHandle_WithNonBoolType_ShouldReturnFalse()
            {
                Assert.That(_handler.CanHandle(typeof(int)), Is.False);
            }

            [Test]
            public void Parse_WithTrueString_ShouldReturnTrue()
            {
                var result = _handler.Parse("true", typeof(bool));
                Assert.That(result, Is.True);
            }

            [Test]
            public void Parse_WithFalseString_ShouldReturnFalse()
            {
                var result = _handler.Parse("false", typeof(bool));
                Assert.That(result, Is.False);
            }

            [Test]
            public void Parse_WithUpperCaseTrue_ShouldReturnTrue()
            {
                var result = _handler.Parse("TRUE", typeof(bool));
                Assert.That(result, Is.True);
            }

            [Test]
            public void Parse_WithInvalidBoolean_ShouldThrowException()
            {
                Assert.Throws<FormatException>(() => _handler.Parse("invalid", typeof(bool)));
            }
        }

        [TestFixture]
        public class EnumParameterHandlerTests
        {
            private EnumParameterHandler _handler;

            [SetUp]
            public void Setup()
            {
                _handler = new EnumParameterHandler();
            }

            [Test]
            public void CanHandle_WithEnumType_ShouldReturnTrue()
            {
                Assert.That(_handler.CanHandle(typeof(TestEnum)), Is.True);
            }

            [Test]
            public void CanHandle_WithNonEnumType_ShouldReturnFalse()
            {
                Assert.That(_handler.CanHandle(typeof(int)), Is.False);
            }

            [Test]
            public void Parse_WithValidEnumValue_ShouldReturnEnum()
            {
                var result = _handler.Parse("Value1", typeof(TestEnum));
                Assert.That(result, Is.EqualTo(TestEnum.Value1));
            }

            [Test]
            public void Parse_WithLowerCaseEnumValue_ShouldReturnEnum()
            {
                var result = _handler.Parse("value1", typeof(TestEnum));
                Assert.That(result, Is.EqualTo(TestEnum.Value1));
            }

            [Test]
            public void Parse_WithInvalidEnumValue_ShouldThrowException()
            {
                Assert.Throws<ArgumentException>(() => _handler.Parse("InvalidValue", typeof(TestEnum)));
            }
        }

        public enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }
    }
} 