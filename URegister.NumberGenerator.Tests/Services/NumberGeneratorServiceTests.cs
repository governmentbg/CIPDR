using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using URegister.Infrastructure.Data.Common;
using URegister.NumberGenerator.Data;
using URegister.NumberGenerator.Data.Models;
using URegister.NumberGenerator.Services;

namespace URegister.NumberGenerator.Tests.Services
{
    [TestFixture]
    public class NumberGeneratorServiceTests
    {
        private Mock<INumberGeneratorRepository> _repoMock;
        private Mock<ILogger<NumberGeneratorService>> _loggerMock;
        private Mock<IConfiguration> _configMock;
        private NumberGeneratorService _service;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<INumberGeneratorRepository>();
            _loggerMock = new Mock<ILogger<NumberGeneratorService>>();
            _configMock = new Mock<IConfiguration>();
        }

        [TestCase()]
        [TestCase(6200, 5, 888888, 2)]
        [TestCase(200, 5, 88888, 2)]
        [TestCase(200, 8, 88888, 2)]
        [TestCase(200, 10, 22, 42)]
        [Test]
        public async Task GenerateNumber_ValidInputs_ReturnsGeneratedNumber(int ebk = 7500,
            int sequenceLength = 6,
            int maxValue = 999999,
            int sequence = 1)
        {
            // Arrange      

            var inMemorySettings = new Dictionary<string, string> {
            {"Number:EBK", ebk.ToString() },
            {"Number:SequenceLength", sequenceLength.ToString() },
            {"Number:MaxValue", maxValue.ToString() }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _service = new NumberGeneratorService(
                _repoMock.Object,
                _loggerMock.Object,
                configuration
            );

            var initialDocumentId = Guid.NewGuid();

            _repoMock.Setup(r => r.ExecuteProc<Numerator>(ProcedureType.GetNumeratorSequence, It.IsAny<int>()))
                .ReturnsAsync(new[] { new Numerator { Sequence = sequence } });
            _repoMock.Setup(r => r.AddAsync(It.IsAny<NumberArchive>())).Returns(Task.CompletedTask);

            string sequenceFormat = $"D{sequenceLength}";
            long number;
            string prefix = $"{DateTime.Now:yy}{ebk:D4}{DateTime.Now.DayOfYear:D3}";
            int dbPrefix = int.Parse(prefix);

            if (sequence > maxValue)
            {
                sequence = sequence - maxValue;
                prefix = $"{DateTime.Now:yy}{ebk:D4}{(DateTime.Now.DayOfYear + 500).ToString("D3")}";
            }

            number = long.Parse($"{prefix}{sequence.ToString(sequenceFormat)}");

            int controlNumber = CalculateControllNumber(number, sequenceLength);
            number = number * 10 + controlNumber;

            // Act
            var result = await _service.GenerateNumber("TestRegister", initialDocumentId);

            // Assert
            Assert.AreEqual(number, result);
            _repoMock.Verify(r => r.AddAsync(It.Is<NumberArchive>(a => a.Register == "TestRegister" && a.Number == number)), Times.Once);
        }

        private int CalculateControllNumber(long number, int sequenceLength)
        {
            // 9 - дължина на префикса
            int numberLength = 9 + sequenceLength;

            if (number.ToString().Length != numberLength)
            {
                throw new ArgumentException($"Номера трябва да е {numberLength} цифри (без контролна сума)");
            }

            long tempNumber = number;
            int sum = 0;

            do
            {
                sum += (int)(tempNumber % 1000);
                tempNumber /= 1000;
            } while (tempNumber > 0);

            return sum % 10;
        }

        //[Test]
        //public async Task GenerateNumberForExternalSystem_ValidInputs_ReturnsGeneratedNumber()
        //{
        //    // Arrange
        //    const int ebk = 1234;
        //    const long expectedNumber = 123456789012345L;
        //    var systemName = "ExternalSystem";
        //    var initialDocumentNumber = "DOC123";

        //    _repoMock.Setup(r => r.ExecuteProc<Numerator>(ProcedureType.GetNumeratorSequence, It.IsAny<int>()))
        //        .ReturnsAsync(new[] { new Numerator { Sequence = 1 } });
        //    _repoMock.Setup(r => r.AddAsync(It.IsAny<ExternalNumberArchive>())).Returns(Task.CompletedTask);

        //    // Act
        //    var result = await _service.GenerateNumberForExternalSystem(ebk, systemName, initialDocumentNumber);

        //    // Assert
        //    Assert.AreEqual(expectedNumber, result);
        //    _repoMock.Verify(r => r.AddAsync(It.Is<ExternalNumberArchive>(a => a.Register == systemName && a.Number == expectedNumber)), Times.Once);
        //}

        //[TestCase(2575000220000010, 6)]
        //[TestCase(256200022000029, 5)]
        //[TestCase(250200022000029, 5)]
        //[TestCase(250200022000000029, 8)]
        //[TestCase(6573308126290448593, 10)]
        //[Test]
        //public async Task ValidateNumber_ValidNumber_ReturnsTrue(long number, int sequenceLength)
        //{
        //    //Arrange
        //    var numberArchiveData = new List<NumberArchive>
        //    {
        //        new NumberArchive { Number = number }
        //    }.AsQueryable();

        //    var asyncTestData = new TestAsyncEnumerable<NumberArchive>(numberArchiveData);

        //    _repoMock.Setup(repo => repo.AllReadonly<NumberArchive>().AnyAsync(It.IsAny<Expression>())       
        //.Returns(() => Task.FromResult(foo)));
            

        //    var inMemorySettings = new Dictionary<string, string> {           
        //    {"Number:SequenceLength", sequenceLength.ToString() },
        //    };

        //    IConfiguration configuration = new ConfigurationBuilder()
        //        .AddInMemoryCollection(inMemorySettings)
        //        .Build();

        //    _service = new NumberGeneratorService(
        //        _repoMock.Object,
        //        _loggerMock.Object,
        //        configuration
        //    );
           
        //    // Act
        //    var result = await _service.ValidateNumber(number);

        //    // Assert
        //    Assert.IsTrue(result);
        //}

        //[Test]
        //public void ValidateNumber_InvalidNumberLength_ReturnsFalse()
        //{
        //    // Arrange
        //    const long invalidNumber = 12345L;

        //    _configMock.Setup(c => c.GetValue<int>("Number:SequenceLength")).Returns(6);

        //    // Act & Assert
        //    Assert.ThrowsAsync<ArgumentException>(async () => await _service.ValidateNumber(invalidNumber));
        //}
    }
}