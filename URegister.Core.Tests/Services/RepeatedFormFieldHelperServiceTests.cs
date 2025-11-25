using URegister.Core.Services;

namespace URegister.Core.Tests.Services
{
    [TestFixture]
    public class RepeatedFormFieldHelperServiceTests
    {
        [Test]
        [TestCase("abc", 2, "abc#2")]
        [TestCase("abc", 22, "abc#22")]
        [TestCase("abc", 1, "abc#1")]
        [TestCase("abc_def", 6, "abc#6_def")]
        public void InsertBeforeFirstUnderscore_Test(string originalName, int index, string expectedName)
        {
            string result = RepeatedFormFieldHelperService.InsertBeforeFirstUnderscore(originalName, '#' + index.ToString());

            Assert.AreEqual(expectedName, result);
        }


        [Test]
        [TestCase("abc#2", 2)]
        [TestCase("abc#22", 22)]
        [TestCase("abc#1", 1)]
        [TestCase("abc#6_def", 6)]
        [TestCase("abc6_def", 0)]
        public void GetRepetitionIndex_Test(string input, int expectedIndex)
        {
            int result = RepeatedFormFieldHelperService.GetRepetitionIndex(input);

            Assert.AreEqual(expectedIndex, result);
        }
    }
}
