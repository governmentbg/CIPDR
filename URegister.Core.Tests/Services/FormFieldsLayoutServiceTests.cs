using URegister.Core.Services;

namespace URegister.Core.Tests.Services
{
    [TestFixture]
    public class FormFieldsLayoutServiceTests
    {
        [Test]
        [TestCase("1:12345", "1:12***")]
        [TestCase("112345", "112345")]
        [TestCase("1:6401241266", "1:64********")]
        [TestCase("3:6401241266", "3:64********")]
        [TestCase("3:64", "3:64")]
        [TestCase("3:643", "3:64*")]
        [TestCase("3:6", "3:6")]
        public void MaskAfterColonKeepingFirstTwoTest(string originalValue, string expectedResult)
        {
            Assert.AreEqual(expectedResult, FormFieldsLayoutService.MaskAfterColonKeepingFirstTwo(originalValue));
        }
    }
}
