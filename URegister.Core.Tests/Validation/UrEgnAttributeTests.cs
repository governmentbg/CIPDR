using System.ComponentModel.DataAnnotations;
using URegister.Core.Validation;

namespace URegister.Core.Tests.Validation
{
    public class UrEgnAttributeTests
    {
        [Test]
        [TestCase("3602021988")]
        [TestCase("8302218810")]
        [TestCase("1242057554")]
        public void IsValid_CallsValidatePersonalId(string input)
        {
            // Arrange
            var attribute = new TestableUrEgnAttribute();
            var validationContext = new ValidationContext(new { });
            var errorMessage = "errorMessage";

            // Act
            var result = attribute.TestIsValid(input, validationContext, errorMessage);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [TestCase("3602021989")]
        [TestCase("ZXCVBNMASD")]
        [TestCase("")]
        [TestCase(null)]
        [TestCase("2434")]
        [TestCase("36020219899")]
        [TestCase("3602021989 ")]
        [TestCase(" 1242057554")]
        [TestCase("124 2057554")]
        [TestCase("124 2057554 ")]
        public void IsInvalid_CallsValidatePersonalId(string? input)
        {
            // Arrange
            var attribute = new TestableUrEgnAttribute();
            var validationContext = new ValidationContext(new { });
            var errorMessage = "errorMessage";

            // Act
            var result = attribute.TestIsValid(input, validationContext, errorMessage);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(errorMessage, result.ErrorMessage);
        }
    }

    public class TestableUrEgnAttribute : UrEgnAttribute
    {
        public ValidationResult? TestIsValid(object? value, ValidationContext validationContext, string errorMessage)
        {
            ErrorMessage = errorMessage;
            return base.IsValid(value, validationContext);
        }
    }
}
