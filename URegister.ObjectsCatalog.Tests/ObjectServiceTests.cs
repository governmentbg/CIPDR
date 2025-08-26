using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MockQueryable.Moq;
using Moq;
using Moq.EntityFrameworkCore;
using URegister.Infrastructure.Extensions;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.ObjectsCatalog.Data;
using URegister.ObjectsCatalog.Data.Models;
using URegister.ObjectsCatalog.Services;

[TestFixture]
public class ObjectServiceTests
{
    private Mock<ILogger<ObjectService>> _mockLogger;
    private Mock<IObjectCatalogRepository> _repoMock;
    private ObjectService _objectService;

    [SetUp]
    public void SetUp()
    {
        _repoMock = new Mock<IObjectCatalogRepository>();
        _mockLogger = new Mock<ILogger<ObjectService>>();

        _objectService = new ObjectService(
            _repoMock.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task GetFieldDataAsync_FieldDataNotFound_ReturnsEmptyJson_AndLogsInfo()
    {
        // Arrange
        var fieldType = "NonExistentType";
        var fields = new List<Field>
        {
            new Field
            {
                FieldType = new FieldType { Name = "OtherType" },
                IsCurrent = true
            }
        };

        var mockDbContext = new Mock<DbContext>();
        mockDbContext.Setup(x => x.Set<Field>()).ReturnsDbSet(fields);

        _repoMock.Setup(r => r.AllReadonly<Field>())
                 .Returns(mockDbContext.Object.Set<Field>());

        // Act
        var result = await _objectService.GetFieldDataAsync(fieldType);

        // Assert
        Assert.AreEqual(string.Empty.ToJson(), result);
        _mockLogger.Verify(log => log.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Не е намерено поле от тип {fieldType}")),
            It.IsAny<Exception>(),
            (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Test]
    public async Task GetFieldDataAsync_ValidFieldData_ReturnsJson()
    {
        // Arrange
        var fieldType = "ValidType";
        var formField = new FormField { Label = "Test", Type = "text" }; // Adapt this to your real FormField structure
        var serializedFormField = formField.ToJson();

        var fields = new List<Field>
    {
        new Field
        {
            Id = new Guid(),
            IsCurrent = true,
            FieldType = new FieldType { Name = fieldType },
            FieldData = serializedFormField
        }
    };

        var mockDbContext = new Mock<DbContext>();
        mockDbContext.Setup(x => x.Set<Field>()).ReturnsDbSet(fields);

        _repoMock.Setup(r => r.AllReadonly<Field>())
                 .Returns(mockDbContext.Object.Set<Field>());

        // Act
        var resultJson = await _objectService.GetFieldDataAsync(fieldType);

        // Assert
        var deserialized = resultJson.FromJson<FormField>();
        Assert.NotNull(deserialized);
        Assert.That(deserialized.Label, Is.EqualTo(formField.Label));
        Assert.That(deserialized.Type, Is.EqualTo(formField.Type));
    }

    [Test]
    public async Task GetServiceType_ReturnsCorrectMessage_WithStepValuesSet()
    {
        // Arrange
        int serviceTypeId = 1;
        var serviceTypeSteps = new List<ServiceTypeStep>
    {
        new ServiceTypeStep { StepId = 101 },
        new ServiceTypeStep { StepId = 102 }
    };

        var serviceType = new ServiceType
        {
            Id = serviceTypeId,
            Name = "Test Service",
            ServiceTypeSteps = serviceTypeSteps
        };

        var steps = new List<Step>
    {
        new Step { Id = 101, Name = "Step A" },
        new Step { Id = 102, Name = "Step B" },
        new Step { Id = 103, Name = "Step C" }
    };

        var mockServiceTypeDbSet = new List<ServiceType> { serviceType }
            .BuildMockDbSet();

        var mockStepDbSet = steps.BuildMockDbSet();

        _repoMock.Setup(r => r.AllReadonly<ServiceType>())
                 .Returns(mockServiceTypeDbSet.Object);

        _repoMock.Setup(r => r.AllReadonly<Step>())
                 .Returns(mockStepDbSet.Object);

        // Act
        var result = await _objectService.GetServiceType(serviceTypeId);

        // Assert
        Assert.AreEqual(serviceTypeId, result.Id);
        Assert.AreEqual("Test Service", result.Name);
        Assert.AreEqual(3, result.Steps.Count);

        Assert.IsTrue(result.Steps.First(s => s.Id == 101).Value);
        Assert.IsTrue(result.Steps.First(s => s.Id == 102).Value);
        Assert.IsFalse(result.Steps.First(s => s.Id == 103).Value);
    }

    [Test]
    public async Task GetStep_ReturnsStepMessage_WhenStepExists()
    {
        // Arrange
        int stepId = 1;
        var expectedStep = new Step
        {
            Id = stepId,
            Name = "Step Name",
            RoleId = Guid.NewGuid(),
            Type = "StepType",
            Method = "StepMethod"
        };

        _repoMock.Setup(r => r.GetByIdAsync<Step>(stepId))
                 .ReturnsAsync(expectedStep);

        // Act
        var result = await _objectService.GetStep(stepId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedStep.Id, result.Id);
        Assert.AreEqual(expectedStep.Name, result.Name);
        Assert.AreEqual(expectedStep.RoleId.ToString(), result.RoleId);
        Assert.AreEqual(expectedStep.Type, result.Type);
        Assert.AreEqual(expectedStep.Method, result.Method);
    }

}