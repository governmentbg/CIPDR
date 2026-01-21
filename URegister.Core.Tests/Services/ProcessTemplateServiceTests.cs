using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Services;

namespace URegister.Core.Tests.Services
{
    [TestFixture]
    public class ProcessTemplateServiceTests
    {
        [Test]
        [TestCase(",,,,,", ",")]
        [TestCase(",", ",")]
        [TestCase(", ", ", ")]
        [TestCase(", ,, ,", ",")]
        [TestCase(", ,, , ", ", ")]
        [TestCase("a, ,, ,b", "a,b")]
        [TestCase("a, ,, , b", "a, b")]
        [TestCase("a,b", "a,b")]
        [TestCase("a, b", "a, b")]
        public void ReplaceConsequitiveCommasTest(string input, string expected)
        {
            string processed = ProcessTemplateService.ReplaceConsequitiveCommas(input);

            Assert.AreEqual(expected, processed);
        }
    }
}
