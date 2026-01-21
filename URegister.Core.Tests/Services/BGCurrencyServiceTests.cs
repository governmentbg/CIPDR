using URegister.Core.Services;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Tests.Services
{
    public class BGCurrencyServiceTests
    {
        [Test]
        [TestCase("", "")]
        [TestCase("2:2", "2,00 €")]
        [TestCase("2:2.222", "2,22 €")]
        [TestCase("2:-2.222", "-2,22 €")]
        [TestCase("2:-1000000.01", "-1 000 000,01 €")]
        public void RegistryItemValueToPublicTextTest(string fieldValue, string expectedFormattedText)
        {
            string result = BGCurrencyService.RegistryItemValueToPublicText(fieldValue);

            if(DateTime.Now < ValueConstants.EuroDate)
            {
                expectedFormattedText = expectedFormattedText.Replace("€", "лв.");
            }

            Assert.AreEqual(expectedFormattedText, result);
        }
    }
}
