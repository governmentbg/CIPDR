using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using URegister.Common;
using URegister.NumberGenerator.Contracts;
using URegister.NumberGenerator.Services;

namespace URegister.NumberGenerator.Tests.Services
{
    [TestFixture]
    public class NumberServiceTests
    {
        private Mock<INumberGeneratorService> _mockNumberGeneratorService;
        private Mock<ILogger<NumberService>> _mockLogger;
        private NumberService _numberService;

        [SetUp]
        public void SetUp()
        {
            _mockNumberGeneratorService = new Mock<INumberGeneratorService>();
            _mockLogger = new Mock<ILogger<NumberService>>();
            _numberService = new NumberService(_mockLogger.Object, _mockNumberGeneratorService.Object);
        }

        [Test]
        public async Task CheckNumber_ValidNumber_ReturnsValidResponse()
        {
            // Arrange
            var request = new CheckNumberRequest { Number = 12345 };
            _mockNumberGeneratorService.Setup(s => s.ValidateNumber(request.Number)).ReturnsAsync(true);

            // Act
            var result = await _numberService.CheckNumber(request, It.IsAny<ServerCallContext>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsValid);
            Assert.That(result.Status.Code, Is.EqualTo(ResultCodes.Ok));
        }

        [Test]
        public async Task CheckNumber_InvalidNumber_ReturnsInvalidResponse()
        {
            // Arrange
            var request = new CheckNumberRequest { Number = 0 };
            _mockNumberGeneratorService.Setup(s => s.ValidateNumber(request.Number)).ReturnsAsync(false);

            // Act
            var result = await _numberService.CheckNumber(request, It.IsAny<ServerCallContext>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Status.Code, Is.EqualTo(ResultCodes.Ok));
        }

        [Test]
        public async Task CheckNumber_ThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var request = new CheckNumberRequest { Number = -1 };
            _mockNumberGeneratorService.Setup(s => s.ValidateNumber(request.Number)).ThrowsAsync(new ArgumentException("Invalid number format"));

            // Act
            var result = await _numberService.CheckNumber(request, It.IsAny<ServerCallContext>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Status.Code, Is.EqualTo(ResultCodes.BadRequest));
            Assert.That(result.Status.Message, Is.EqualTo("Invalid number format"));
        }

        [Test]
        public async Task CheckNumber_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new CheckNumberRequest { Number = 99999 };
            _mockNumberGeneratorService.Setup(s => s.ValidateNumber(request.Number)).ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _numberService.CheckNumber(request, It.IsAny<ServerCallContext>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.IsValid);
            Assert.That(result.Status.Code, Is.EqualTo(ResultCodes.InternalServerError));
            Assert.That(result.Status.Message, Is.EqualTo("Unexpected error"));
        }

        [Test]
        public async Task GetNumber_ValidRequest_ReturnsGeneratedNumber()
        {
            // Arrange
            var request = new NumberRequest { InitialDocumentId = Guid.NewGuid().ToString(), Register = "TestRegister" };
            long generatedNumber = 123456;
            _mockNumberGeneratorService.Setup(s => s.GenerateNumber(request.Register, Guid.Parse(request.InitialDocumentId))).ReturnsAsync(generatedNumber);

            // Act
            var result = await _numberService.GetNumber(request, It.IsAny<ServerCallContext>());

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Number, Is.EqualTo(generatedNumber));
            Assert.That(result.Status.Code, Is.EqualTo(ResultCodes.Ok));
        }
   
        [Test]
        public async Task GetNumber_ThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var request = new NumberRequest { InitialDocumentId = Guid.NewGuid().ToString(), Register = "TestRegister" };

            _mockNumberGeneratorService
                .Setup(s => s.GenerateNumber(It.IsAny<string>(), It.IsAny<Guid>()))
                .ThrowsAsync(new ArgumentException("Invalid document ID"));

            // Act
            var result = await _numberService.GetNumber(request, It.IsAny<ServerCallContext>());

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Number, Is.EqualTo(0));
            Assert.That(result.Status.Code, Is.EqualTo(ResultCodes.BadRequest), "Expected BadRequest but got a different response");
            Assert.That(result.Status.Message, Is.EqualTo("Invalid document ID"));
        }

        [Test]
        public async Task GetNumber_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new NumberRequest { InitialDocumentId = Guid.NewGuid().ToString(), Register = "TestRegister" };
            _mockNumberGeneratorService.Setup(s => s.GenerateNumber(request.Register, Guid.Parse(request.InitialDocumentId)))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _numberService.GetNumber(request, It.IsAny<ServerCallContext>());

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Number, Is.EqualTo(0));
            Assert.That(result.Status.Code, Is.EqualTo(ResultCodes.InternalServerError));
            Assert.That(result.Status.Message, Is.EqualTo("Unexpected error"));
        }

        [Test]
        public async Task GetNumberForExternalSystem_ValidRequest_ReturnsGeneratedNumber()
        {
            // Arrange
            var request = new ExternalNumberRequest { InitialDocumentNumber = "12345", SystemName = "TestSystem", Ebk = 1 };
            long generatedNumber = 987654;
            _mockNumberGeneratorService.Setup(s => s.GenerateNumberForExternalSystem(request.Ebk, request.SystemName, request.InitialDocumentNumber))
                .ReturnsAsync(generatedNumber);

            // Act
            var result = await _numberService.GetNumberForExternalSystem(request, It.IsAny<ServerCallContext>());

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Number, Is.EqualTo(generatedNumber));
            Assert.That(result.Status.Code, Is.EqualTo(ResultCodes.Ok));
        }

        [Test]
        public async Task GetNumberForExternalSystem_ThrowsArgumentException_ReturnsBadRequest()
        {
            // Arrange
            var request = new ExternalNumberRequest { InitialDocumentNumber = "12345", SystemName = "TestSystem", Ebk = 1 };
            _mockNumberGeneratorService.Setup(s => s.GenerateNumberForExternalSystem(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ArgumentException("Invalid external system input"));

            // Act
            var result = await _numberService.GetNumberForExternalSystem(request, It.IsAny<ServerCallContext>());

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Number, Is.EqualTo(0));
            Assert.That(result.Status.Code, Is.EqualTo(ResultCodes.BadRequest));
            Assert.That(result.Status.Message, Is.EqualTo("Invalid external system input"));
        }

        [Test]
        public async Task GetNumberForExternalSystem_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var request = new ExternalNumberRequest { InitialDocumentNumber = "12345", SystemName = "TestSystem", Ebk = 1 };
            _mockNumberGeneratorService.Setup(s => s.GenerateNumberForExternalSystem(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Unexpected error"));

            // Act
            var result = await _numberService.GetNumberForExternalSystem(request, It.IsAny<ServerCallContext>());

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result.Number, Is.EqualTo(0));
            Assert.That(result.Status.Code, Is.EqualTo(ResultCodes.InternalServerError));
            Assert.That(result.Status.Message, Is.EqualTo("Unexpected error"));
        }
    }

}
