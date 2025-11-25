using DataTables.AspNet.Core;
using Grpc.Core;
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
    }
}
