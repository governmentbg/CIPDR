using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;

using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Register;
using URegister.Core.Services;
using URegister.RegistersCatalog;
using r = URegister.Core.Models.Register;
using cur = URegister.Core.Models.CurrentRegister;
using URegister.NomenclaturesCatalog;
using URegister.Common;
using URegister.Infrastructure.Constants;


[TestFixture]
public class RegisterServiceTests
{
    private Mock<IApplicationRepository> _repoMock;
    private Mock<INomenclatureClientService> _nomenclatureClientMock;
    private Mock<RegistersCatalogGrpc.RegistersCatalogGrpcClient> _registerGrpcClientMock;
    private Mock<IRegisterClientService> _registerClientMock;
    private Mock<ILogger<RegisterService>> _mockLogger;
    private RegisterService _registerService;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IApplicationRepository>();
        _nomenclatureClientMock = new Mock<INomenclatureClientService>();
        _registerGrpcClientMock = new Mock<RegistersCatalogGrpc.RegistersCatalogGrpcClient>();
        _registerClientMock = new Mock<IRegisterClientService>();
        _mockLogger = new Mock<ILogger<RegisterService>>();

        _registerService = new RegisterService(
            _repoMock.Object,
            _mockLogger.Object,
            _nomenclatureClientMock.Object,
            _registerGrpcClientMock.Object,
            _registerClientMock.Object
        );
    }

    [Test]
    public async Task GetCurrentRegister_ReturnsExpectedRegisterVM()
    {
        // Arrange: Define test values

        var testRegister = new Register
        {
            Id = 42,
            Code = "Reg1",
            Name = "Register 1",
            Description = "Test description",
            LegalBasis = "Test Law",
            Type = "General",
            IdentitySecurityLevel = "2",
            TypeEntry = "3"
        };
        var testRegisterVM = new r.RegisterVM
        {
            Id = 42,
            Code = "Reg1",
            Name = "Register 1",
            Description = "Test description",
            LegalBasis = "Test Law",
            Type = "General",
            IdentitySecurityLevel = "2",
            TypeEntry = "3"
        };

        var testRegisters = new List<Register> { testRegister };
        var mockDbContext = new Mock<DbContext>();

        mockDbContext.Setup(x => x.Set<Register>())
            .ReturnsDbSet(testRegisters);
        _repoMock.Setup(r => r.AllReadonly<Register>())
            .Returns(mockDbContext.Object.Set<Register>());
        _registerClientMock.Setup(r => r.GetRegister(It.IsAny<int>(), It.IsAny<Guid>())).Returns(Task.FromResult(testRegisterVM));

        // Act: Call the method being tested
        var result = await _registerService.GetCurrentRegister();

        // Assert: Check if returned RegisterVM matches expected values

        Assert.AreEqual(testRegister.Id, result.Id);
        Assert.AreEqual(testRegister.Code, result.Code);
        Assert.AreEqual(testRegister.Name, result.Name);
        Assert.AreEqual(testRegister.Description, result.Description);
        Assert.AreEqual(testRegister.LegalBasis, result.LegalBasis);
        Assert.AreEqual(testRegister.Type, result.Type);
        Assert.AreEqual(testRegister.IdentitySecurityLevel, result.IdentitySecurityLevel);
        Assert.AreEqual(testRegister.TypeEntry, result.TypeEntry);

        // Verify that the mocked methods were called only once
        _registerClientMock.Verify(r => r.GetRegister(It.IsAny<int>(), It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public async Task SaveRegister_CallsEditRegisterWithCorrectData()
    {
        var inputModel = new cur.RegisterVM
        {
            Id = 42,
            Name = "Test Register",
            Code = "T123",
            Type = "General",
            LegalBasis = "Law 42",
            TypeEntry = "Manual",
            Description = "Test Desc",
            IdentitySecurityLevel = "3"
        };

        // Act
        await _registerService.SaveRegister(inputModel);

        // Assert
        _registerClientMock.Verify(r => r.EditRegister(It.Is<r.RegisterVM>(reg =>
            reg.Id == inputModel.Id &&
            reg.Name == inputModel.Name
        )), Times.Once);
    }
    [Test]
    public async Task GetCurrentRegisterId_ReturnsCorrectId_WhenOneRegisterExists()
    {
        var register = new Register { Id = 42 };
        var testRegisters = new List<Register> { register };
        var mockDbContext = new Mock<DbContext>();
        mockDbContext.Setup(x => x.Set<Register>())
                     .ReturnsDbSet(testRegisters);
        _repoMock.Setup(r => r.AllReadonly<Register>())
                     .Returns(mockDbContext.Object.Set<Register>());

        var result = await _registerService.GetCurrentRegisterId();

        Assert.AreEqual(42, result);
    }

    [Test]
    public void GetCurrentRegisterId_Throws_WhenNoRegistersExist()
    {
        var mockDbContext = new Mock<DbContext>();
        mockDbContext.Setup(x => x.Set<Register>())
                     .ReturnsDbSet(new List<Register>());
        _repoMock.Setup(r => r.AllReadonly<Register>())
                 .Returns(mockDbContext.Object.Set<Register>());

        Assert.ThrowsAsync<InvalidOperationException>(() => _registerService.GetCurrentRegisterId());
    }

    [Test]
    public void GetCurrentRegisterId_Throws_WhenMultipleRegistersExist()
    {
        var testRegisters = new List<Register>
    {
        new Register { Id = 1 },
        new Register { Id = 2 }
    };

        var mockDbContext = new Mock<DbContext>();
        mockDbContext.Setup(x => x.Set<Register>())
                     .ReturnsDbSet(testRegisters);
        _repoMock.Setup(r => r.AllReadonly<Register>())
                 .Returns(mockDbContext.Object.Set<Register>());

        Assert.ThrowsAsync<InvalidOperationException>(() =>
            _registerService.GetCurrentRegisterId());
    }

    [Test]
    public async Task GetPersonTypes_ReturnsListOfPersonTypes_WhenServiceReturnsData()
    {
        // Arrange: Fake current register id needed by GetNomenclaturePublic
        var testRegisters = new List<Register>
        {
            new Register { Id = 42 }
        };

        var mockDbContext = new Mock<DbContext>();
        mockDbContext.Setup(x => x.Set<Register>()).ReturnsDbSet(testRegisters);

        _repoMock.Setup(r => r.AllReadonly<Register>())
            .Returns(mockDbContext.Object.Set<Register>());

        var mockData = new List<NomenclatureTypePublicResponse>
        {
            new NomenclatureTypePublicResponse { Type = "PersonType1", Name = "Person1" },
            new NomenclatureTypePublicResponse { Type = "Organization", Name = "Company" }
        };

        _nomenclatureClientMock
            .Setup(n => n.GetNomenclaturePublic(42, It.IsAny<string[]>()))
            .ReturnsAsync(mockData);

        // Act
        var result = await _registerService.GetPersonTypes();

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("PersonType1", result[0].Type);
        Assert.AreEqual("Person1", result[0].Name);
        Assert.AreEqual("Organization", result[1].Type);
        Assert.AreEqual("Company", result[1].Name);
    }
    [Test]
    public async Task GetPersonTypes_HandlesErrorResponse()
    {
        // Arrange: One register so GetCurrentRegisterId works
        var testRegisters = new List<Register> { new Register { Id = 42 } };

        // Mock DbContext
        var mockDbContext = new Mock<DbContext>();
        mockDbContext.Setup(x => x.Set<Register>()).ReturnsDbSet(testRegisters);

        // Mock repo to return that DbSet
        _repoMock.Setup(r => r.AllReadonly<Register>())
            .Returns(mockDbContext.Object.Set<Register>());


        // Empty list simulating error/empty nomenclature response
        var response = new List<NomenclatureTypePublicResponse>();

        _nomenclatureClientMock
            .Setup(x => x.GetNomenclaturePublic(It.IsAny<int>(), It.IsAny<string[]>()))
            .ReturnsAsync(response);

        // Act
        var result = await _registerService.GetPersonTypes();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }
    [Test]
    public async Task InitPerson_ReturnsCorrectPersonVM()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var personType = "type-1";

        // Mock data for Nomenclature types
        var mockNomenclatureTypes = new List<NomenclatureTypePublicResponse>
        {
            new NomenclatureTypePublicResponse
            {
                Type = InternalNomenclatureTypes.PersonType,
                Name = "Person Type",
                CodeableConcepts =
                {
                    new CodeableConceptPublicResponse
                    {
                        Code = "type-1",
                        Value = "Type One"
                    }
                }
            }
        };

        // Mock the nomenclatureClientService that _registerService uses
        _nomenclatureClientMock
            .Setup(x => x.GetNomenclaturePublic(It.IsAny<int>(), It.IsAny<string[]>()))
            .ReturnsAsync(mockNomenclatureTypes);

        _nomenclatureClientMock
            .Setup(x => x.GetNomenclatureValue(mockNomenclatureTypes, InternalNomenclatureTypes.PersonType, personType))
            .Returns("Type: Person");

        // Mock DbContext and repository
        var testRegisters = new List<Register>
        {
            new Register { Id = 42, Code = "Reg1", Name = "Register 1" }
        };

        var mockDbContext = new Mock<DbContext>();

        // Use ReturnsDbSet to handle IQueryable properly with async operations
        mockDbContext.Setup(x => x.Set<Register>()).ReturnsDbSet(testRegisters);

        // Set up _repoMock to return the DbSet
        _repoMock.Setup(r => r.AllReadonly<Register>())
            .Returns(mockDbContext.Object.Set<Register>());

        // Act
        var result = await _registerService.InitPerson(adminId, personType);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(adminId, result.AdministrationId);
        Assert.AreEqual(personType, result.Type);
        Assert.AreEqual("Type: Person", result.TypeName);

        _nomenclatureClientMock.Verify(x =>
            x.GetNomenclatureValue(mockNomenclatureTypes, InternalNomenclatureTypes.PersonType, personType),
            Times.Once);
    }
}
