using System.ComponentModel.DataAnnotations;
using URegister.Core.Validation;

namespace URegister.Core.Tests.Validation
{
    public class UrEikAttributeTests
    {
        [Test]
        [TestCase("177208082")]
        [TestCase("1218173091476")]
        public void IsValid_CallsValidateCompanyId(string input)
        {
            // Arrange
            var attribute = new TestableUrEikAttribute();
            var validationContext = new ValidationContext(new { });
            var errorMessage = "errorMessage";

            // Act
            var result = attribute.TestIsValid(input, validationContext, errorMessage);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        [TestCase("177208083")]
        [TestCase("ZXCVBNMASD")]
        [TestCase("")]
        [TestCase(null)]
        [TestCase("2434")]
        [TestCase("36020219899")]
        [TestCase("177208082 ")]
        [TestCase(" 177208082")]
        [TestCase("1772 08082")]
        [TestCase("1772080 82 ")]
        public void IsInvalid_CallsValidateCompanyId(string? input)
        {
            // Arrange
            var attribute = new TestableUrEikAttribute();
            var validationContext = new ValidationContext(new { });
            var errorMessage = "errorMessage";

            // Act
            var result = attribute.TestIsValid(input, validationContext, errorMessage);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(errorMessage, result.ErrorMessage);
        }
    }

    public class TestableUrEikAttribute : UrEikAttribute
    {
        public ValidationResult? TestIsValid(object? value, ValidationContext validationContext, string errorMessage)
        {
            ErrorMessage = errorMessage;
            return base.IsValid(value, validationContext);
        }
    }
}
