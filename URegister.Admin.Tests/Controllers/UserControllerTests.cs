using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using URegister.Admin.Controllers;
using URegister.Core.Contracts;
using URegister.Core.Models.User;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Helper;
using URegister.RegistersCatalog;
using URegister.Users;

namespace URegister.Admin.Tests.Controllers
{
    [TestFixture]
    public class UserControllerTests
    {
        private Mock<AppUserManager.AppUserManagerClient> _mockAppUserManagerClient;
        private Mock<ILogger<UserController>> _mockLogger;
        private Mock<IRegisterClientService> _mockRegisterClient;
        private Mock<IHttpRequester> _mockHttpRequester;
        private Mock<IConfiguration> _mockConfig;
        private Mock<IDataTablesRequest> _mockDataTablesRequest;
        private UserController _controller;

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [SetUp]
        public void SetUp()
        {
            _mockAppUserManagerClient = new Mock<AppUserManager.AppUserManagerClient>();
            _mockLogger = new Mock<ILogger<UserController>>();
            _mockRegisterClient = new Mock<IRegisterClientService>();
            _mockHttpRequester = new Mock<IHttpRequester>();
            _mockConfig = new Mock<IConfiguration>();
            _mockDataTablesRequest = new Mock<IDataTablesRequest>();

            _controller = new UserController(
                _mockAppUserManagerClient.Object,
                _mockLogger.Object,
                _mockRegisterClient.Object
            );
        }

        [Test]
        public async Task Index_WhenAdministrationIdIsNull_SelectsFirstAdministration()
        {

            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var reply = new AppAdministrations
            {
                Status = CommonGrpcHelper.CreateStatusOK()

            };
            reply.Administrations.Add(new AppAdministration
            {
                Id = id1.ToString(),
                Name = "Admin1"
            });
            reply.Administrations.Add(new AppAdministration
            {
                Id = id2.ToString(),
                Name = "Admin2"
            });
            _mockRegisterClient
                .Setup(client => client.GetAllAdministrations())
                .ReturnsAsync(reply);

            // Act
            var result = await _controller.Index(null) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as AdministrationViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(id1.ToString(), model.SelectedAdministrationId);
            Assert.AreEqual(2, model.Administrations.Count);
            Assert.IsTrue(model.Administrations.Any(a => a.Value == id1.ToString() && a.Selected));
        }

        [Test]
        public async Task Index_WhenAdministrationIdIsProvided_SelectsProvidedAdministration()
        {
            // Arrange
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var reply = new AppAdministrations
            {
                Status = CommonGrpcHelper.CreateStatusOK()

            };
            reply.Administrations.Add(new AppAdministration
            {
                Id = id1.ToString(),
                Name = "Admin1"
            });
            reply.Administrations.Add(new AppAdministration
            {
                Id = id2.ToString(),
                Name = "Admin2"
            });
            _mockRegisterClient
                .Setup(client => client.GetAllAdministrations())
                .ReturnsAsync(reply);

            // Act
            var result = await _controller.Index(id2.ToString()) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as AdministrationViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(id2.ToString(), model.SelectedAdministrationId);
            Assert.AreEqual(2, model.Administrations.Count);
            Assert.IsTrue(model.Administrations.Any(a => a.Value == id2.ToString() && a.Selected));
        }

        [Test]
        public async Task Index_WhenNoAdministrationsExist_ReturnsEmptyList()
        {
            // Arrange
            var administrations = new AppAdministrations();

            _mockRegisterClient
                .Setup(client => client.GetAllAdministrations())
                .ReturnsAsync(administrations);

            // Act
            var result = await _controller.Index(null) as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            var model = result.Model as AdministrationViewModel;
            Assert.IsNotNull(model);
            Assert.IsNull(model.SelectedAdministrationId);
            Assert.AreEqual(0, model.Administrations.Count);
        }
    }
}
