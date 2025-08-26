using System.Text.RegularExpressions;
using URegister.Infrastructure.Constants;

namespace URegister.Infrastructure.Tests.Constants
{
    [TestFixture]
    public class RegexPatternsTests
    {
        [Test]
        [TestCase("Иван")]
        [TestCase("иван")]
        [TestCase("ИВАН")]
        [TestCase("ИВАН-СТОЯН")]
        [TestCase("иван-стоян")]
        [TestCase("ИВАН СТОЯН")]
        [TestCase("ИВАН СТОЯН 1")]
        [TestCase("(ИВАН) СТОЯН №1")]
        [TestCase("ИВАН О'СТОЯН №1")]
        [TestCase("!,.1234567890")]
        [TestCase("!,.1234567890§\"")]
        public void CyrillicTextPattern_Valid(string input)
        {
            Assert.IsTrue(Regex.IsMatch(input, RegexPatterns.CyrillicTextPattern));
        }

        [Test]
        [TestCase("Ивaн")]//a е латинско
        [TestCase("иванV")]
        [TestCase("ИВАНv")]
        [TestCase("IVAN")]
        [TestCase("ivan")]
        [TestCase("Ivan")]
        [TestCase("ИВАН ")]
        [TestCase("ИВАН ")]
        [TestCase(" ИВАН ")]
        [TestCase(" ИВАН")]
        [TestCase(" ИВАН")]
        public void CyrillicTextPattern_Invalid(string input)
        {
            Assert.IsFalse(Regex.IsMatch(input, RegexPatterns.CyrillicTextPattern));
        }
        
        [Test]
        [TestCase("Ivan")]
        [TestCase("Ivan(7)")]
        [TestCase("(Ivan(7))")]
        public void LatinTextWithNumbersPatternPattern_Valid(string input)
        {
            Assert.IsTrue(Regex.IsMatch(input, RegexPatterns.LatinTextWithNumbersPattern));
        }

        [Test]
        [TestCase(" Ivan")]
        [TestCase(" Ivan ")]
        [TestCase("Ivan ")]
        [TestCase(" Ivan")]
        [TestCase("Иван")]
        public void LatinTextWithNumbersPatternPattern_Invalid(string input)
        {
            Assert.IsFalse(Regex.IsMatch(input, RegexPatterns.LatinTextWithNumbersPattern));
        }

        [Test]
        [TestCase("1000")]
        [TestCase("3700")]
        [TestCase("1309")]
        public void PostCodePattern_Valid(string input)
        {
            Assert.IsTrue(Regex.IsMatch(input, RegexPatterns.PostCode));
        }

        [Test]
        [TestCase("")]
        [TestCase("ABCD")]
        [TestCase("АБВГ")]
        [TestCase("100")]
        [TestCase("10000")]
        //TODO : [TestCase("0000")]        
        public void PostCodePattern_Invalid(string input)
        {
            Assert.IsFalse(Regex.IsMatch(input, RegexPatterns.PostCode));
        }

        //[Test]
        //[TestCase("123456789")]
        //[TestCase("113344556")]
        //[TestCase("541233534")]
        //[TestCase("OO5412335")]
        //[TestCase("TK5412335")]
        //public void IDCardNumberPattern_Valid(string input)
        //{
        //    Assert.IsTrue(Regex.IsMatch(input, RegexPatterns. IDCardNumberPattern));
        //}

        //[Test]
        //[TestCase("")]
        //[TestCase("5412!3534")]
        //[TestCase("ABCDABCDE")]
        //[TestCase("АБВГАБВГД")]
        //[TestCase("12345678")]
        //[TestCase("1234567890")]
        //[TestCase("6К5412335")]
        //[TestCase("К15412335")]
        //[TestCase("К1541233Т")]
        //[TestCase("КО541233Т")]
        //[TestCase("ТЮ5412335")]
        //[TestCase("ЮT5412335")]
        //[TestCase("ЮЮ5412335")]
        //[TestCase("Тk5412335")]
        //[TestCase("TTK5412335")]
        ////TODO : [TestCase("000000000")]        
        //public void IDCardNumberPattern_Invalid(string input)
        //{
        //    Assert.IsFalse(Regex.IsMatch(input, RegexPatternConstants.IDCardNumberPattern));
        //}

        [Test]
        [TestCase("00014")]
        [TestCase("00028")]
        [TestCase("00881")]
        [TestCase("00165")]
        [TestCase("00179")]
        [TestCase("00182")]
        [TestCase("00196")]
        [TestCase("00206")]
        [TestCase("00215")]
        [TestCase("00223")]
        [TestCase("00254")]
        [TestCase("87597")]
        public void SettlementEkattePattern_Valid(string input)
        {
            Assert.IsTrue(Regex.IsMatch(input, RegexPatterns.EkatteCode));
        }

        [Test]
        [TestCase("")]
        [TestCase("1234")]
        [TestCase("123456")]
        [TestCase("12Т45")]
        [TestCase("АБВГА")]
        [TestCase("ABCDEF")]
        [TestCase("!@#$%")]
        //TODO : [TestCase("00000")]        
        public void SettlementEkattePatternPattern_Invalid(string input)
        {
            Assert.IsFalse(Regex.IsMatch(input, RegexPatterns.EkatteCode));
        }

        //[Test]
        //[TestCase("Ивaн")]//a е латинско
        //[TestCase("ИванQ")]
        //[TestCase("иванQ")]
        //[TestCase("ИВАНQ")]
        //[TestCase("ИВАН-СТОЯНQ")]
        //[TestCase("ИВАН СТОЯНQ")]
        //[TestCase("ИВАН СТОЯН 1Q")]
        //[TestCase("(ИВАН) СТОЯН №1Q")]
        //[TestCase("ИВАН О'СТОЯН №1Q")]
        //[TestCase("!,.1234567890QЩ")]
        //[TestCase("QWEeREщ!,.1234567890")]
        //public void ContainsCyrillicTextPattern_Valid(string input)
        //{
        //    Assert.IsTrue(Regex.IsMatch(input, RegexPatterns.CyrillicTextPattern));
        //}

        //[Test]
        //[TestCase("IVAN")]
        //[TestCase("ivan")]
        //[TestCase("Ivan")]
        //[TestCase("12345")]
        //[TestCase("!@№$")]
        //public void ContainsCyrillicTextPattern_Invalid(string input)
        //{
        //    Assert.IsFalse(Regex.IsMatch(input, RegexPatterns.CyrillicTextPattern));
        //}

        //[Test]
        //[TestCase("")]
        //[TestCase("Ивaн")]//a е латинско
        //[TestCase("ИванQ")]
        //[TestCase("иванQ")]
        //[TestCase("ИВАНQ")]
        //[TestCase("ИВАН-СТОЯНQ")]
        //[TestCase("ИВАН СТОЯНQ")]
        //[TestCase("ИВАН СТОЯН 1Q")]
        //[TestCase("(ИВАН) СТОЯН №1Q")]
        //[TestCase("ИВАН О'СТОЯН №1Q")]
        //[TestCase("!,.1234567890QЩ")]
        //[TestCase("QWEeREщ!,.1234567890")]
        //public void EmptyOrContainsCyrillicTextPattern_Valid(string input)
        //{
        //    Assert.IsTrue(Regex.IsMatch(input, RegexPatterns. EmptyOrContainsCyrillicTextPattern));
        //}

        //[Test]
        //[TestCase("IVAN")]
        //[TestCase("ivan")]
        //[TestCase("Ivan")]
        //[TestCase("12345")]
        //[TestCase("!@№$")]
        //public void EmptyOrContainsCyrillicTextPattern_Invalid(string input)
        //{
        //    Assert.IsFalse(Regex.IsMatch(input, RegexPatternConstants.EmptyOrContainsCyrillicTextPattern));
        //}

        [Test]
        [TestCase("+359877123123")]
        [TestCase("0877123123")]
        public void PhoneNumberPattern_Valid(string input)
        {
            Assert.IsTrue(Regex.IsMatch(input, RegexPatterns.PhoneNumber));
        }

        [Test]
        [TestCase("359877123123")]
        [TestCase("877123123")]
        [TestCase("+0359877123123")]
        [TestCase("0877A23123")]
        [TestCase("00877123123")]
        public void PhoneNumberPattern_Invalid(string input)
        {
            Assert.IsFalse(Regex.IsMatch(input, RegexPatterns.PhoneNumber));
        }

        [Test]
        [TestCase("Иван")]
        [TestCase("иван")]
        [TestCase("ИВАН")]
        [TestCase("ИВАН-СТОЯН")]
        [TestCase("ИВАН СТОЯН")]
        [TestCase("ИВАН О'СТОЯН")]
        [TestCase("Ю")]
        public void CyrillicPersonNamePattern_Valid(string input)
        {
            Assert.That(Regex.IsMatch(input, RegexPatterns.CyrillicPersonNamePattern));
        }

        [Test]
        [TestCase("Ивaн")]//a е латинско
        [TestCase("иванV")]
        [TestCase("ИВАНv")]
        [TestCase("IVAN")]
        [TestCase("ivan")]
        [TestCase("Ivan")]
        [TestCase("ИВАН О'СТОЯН №1")]
        [TestCase("ИВАН СТОЯН 1")]
        [TestCase(" ИВАН")]
        [TestCase("ИВАН ")]
        [TestCase(" ИВАН ")]
        [TestCase("ИВАН ")]
        public void CyrillicPersonNamePattern_Invalid(string input)
        {
            Assert.That(Regex.IsMatch(input, RegexPatterns.CyrillicPersonNamePattern), Is.False);
        }

        [Test]
        [TestCase("123456789")]
        [TestCase("113344556")]
        [TestCase("541233534")]
        [TestCase("OO5412335")]
        [TestCase("TK5412335")]
        public void IDCardNumberPattern_Valid(string input)
        {
            Assert.IsTrue(Regex.IsMatch(input, RegexPatterns.IDCardNumberPattern));
        }

        [Test]
        [TestCase("")]
        [TestCase("5412!3534")]
        [TestCase("ABCDABCDE")]
        [TestCase("АБВГАБВГД")]
        [TestCase("12345678")]
        [TestCase("1234567890")]
        [TestCase("6К5412335")]
        [TestCase("К15412335")]
        [TestCase("К1541233Т")]
        [TestCase("КО541233Т")]
        [TestCase("ТЮ5412335")]
        [TestCase("ЮT5412335")]
        [TestCase("ЮЮ5412335")]
        [TestCase("Тk5412335")]
        [TestCase("TTK5412335")]
        //TODO : [TestCase("000000000")]        
        public void IDCardNumberPattern_Invalid(string input)
        {
            Assert.IsFalse(Regex.IsMatch(input, RegexPatterns.IDCardNumberPattern));
        }

        [Test]
        [TestCase("zzz@zz.zz")]
        [TestCase("z.z.z@zz.zz")]
        [TestCase("zzz@z.z.zz")]
        [TestCase("ЮЮЮ@юю.юю")]
        [TestCase("113@hh.hh")]
        [TestCase("أَبْجَدِيّ@أَبْجَدِيّ.أَبْجَدِيّ")]
        public void EmailPattern_Valid(string input)
        {
            Assert.IsTrue(Regex.IsMatch(input, RegexPatterns.Email));
        }

        [Test]
        [TestCase("z@zz@zz.zz")]
        [TestCase("zzzzz.zz")]
        [TestCase("zzzzz@zz")]
        [TestCase("123456")]
        //[TestCase("___@zz.zz")]
        //[TestCase("zzz@__.zz")]
        //[TestCase("ЮЮЮ@юю.__")]
        public void EmailNumberPattern_Invalid(string input)
        {
            Assert.IsFalse(Regex.IsMatch(input, RegexPatterns.Email));
        }
    }
}
