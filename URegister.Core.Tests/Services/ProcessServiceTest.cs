using Grpc.Core;
using IO.SignTools.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MockQueryable;
using MockQueryable.Moq;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using System.ComponentModel;
using URegister.Common;
using URegister.Core.Contracts;
using URegister.Core.Data;
using URegister.Core.Data.Models.Common;
using URegister.Core.Data.Models.Process;
using URegister.Core.Models.CurrentRegister;
using URegister.Core.Models.Process;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Contracts;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.NomenclaturesCatalog;
using URegister.NumberGenerator;
using URegister.ObjectsCatalog;
using URegister.RegistersCatalog;
using static URegister.IntegrationsCatalog.IntegrationGrpc;
using static URegister.ObjectsCatalog.ObjectsCatalogGrpc;
using static URegister.Users.AppUserManager;

namespace URegister.Core.Tests.Services
{
    [TestFixture]
    public class ProcessServiceTest
    {
        private readonly Mock<IApplicationRepository> _repoMock = new();
        private readonly Mock<NumberGenerator.NumberGenerator.NumberGeneratorClient> _numberGeneratorMock = new();
        private readonly Mock<RegistersCatalogGrpc.RegistersCatalogGrpcClient> _registerGrpcMock = new();
        private readonly Mock<IFormConfigurationPersistenceService> _formConfigMock = new();
        private readonly Mock<IRegisterService> _registerServiceMock = new();
        private readonly Mock<NomenclatureGrpc.NomenclatureGrpcClient> _nomenclatureGrpcMock = new();
        private readonly Mock<ILogger<BaseService>> _loggerMock = new();
        private readonly Mock<IObjectStoreService> _objectStoreMock = new();
        private readonly Mock<IConfiguration> _configMock = new();
        private readonly Mock<IIOSignToolsService> _signToolsMock = new();
        private readonly Mock<ObjectsCatalogGrpcClient> _objectsCatalogMock = new();
        private readonly Mock<AppUserManagerClient> _appUserManagerClient = new();
        private readonly Mock<IntegrationGrpcClient> _integrationGrpcClient = new();
        private readonly Mock<IHttpRequester> _httpRequester = new();
        private readonly Mock<IUserContext> _userContextMock = new();
        private readonly Mock<IProcessTemplateService> _processTemplateService = new();
        private readonly Mock<IServiceService> _serviceService = new();
        private ProcessService _processService;

        [SetUp]
        public void SetUp()
        {
            _processService = new ProcessService(
                _repoMock.Object,
                _numberGeneratorMock.Object,
                _registerGrpcMock.Object,
                _formConfigMock.Object,
                _registerServiceMock.Object,
                _nomenclatureGrpcMock.Object,
                _loggerMock.Object,
                _objectStoreMock.Object,
                _configMock.Object,
                _signToolsMock.Object,
                _objectsCatalogMock.Object,
                _appUserManagerClient.Object,
                _integrationGrpcClient.Object,
                _httpRequester.Object,
                _userContextMock.Object,
                _serviceService.Object
            );
        }

        [Test]
        public async Task AddStep_CreatesNewProcess_WhenProcessIdIsEmpty()
        {
            // Arrange
            var model = new ProcessStepVM
            {
                ProcessId = Guid.Empty,
                FromProcessId = null,
                ServiceId = 1,
                ServiceStepId = 100,
                OrderNum = 1,
                IncomingDate = new DateTime(2020, 1, 1),
                IncomingNumber = "OLD-123",
                FileId = Guid.NewGuid(),
                FormFields = new List<FormField>(),
            };

            _registerServiceMock
                .Setup(x => x.GetCurrentRegister())
                .ReturnsAsync(new RegisterVM { Code = "REG-001" });

            _numberGeneratorMock
                .Setup(x => x.GetNumberAsync(It.IsAny<NumberRequest>(), null, null, It.IsAny<CancellationToken>()))
                .Returns(new AsyncUnaryCall<NumberReply>(
                    Task.FromResult(new NumberReply
                    {
                        Number = 12345,
                        Status = new ResultStatus { Code = ResultCodes.NotFound, Message = "Mock error" }
                    }),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }
                ));

            // Act
            Exception ex = null;

            try
            {
                await _processService.AddStep(model);
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.IsNotNull(ex);
            Assert.That(ex.Message, Does.Contain("Проблем при номериране"));
        }

        [Test]
        public async Task AddStep_UsesExistingProcess_WhenProcessIdIsNotEmpty()
        {
            Guid responseGuid = new Guid("79b1e404-8aa1-4245-9137-6d23215786c0");
            var fakeResponse = new MPRIListMessage
            {
                Items = 
                { 
                    new MPRILisItemMessage()
                    {
                        Id = responseGuid.ToString()
                    }
                }
            };
            // Simulate gRPC response with AsyncUnaryCall<T>
            var asyncUnaryCall = new AsyncUnaryCall<MPRIListMessage>(
                Task.FromResult(fakeResponse), // Task with fake response
                Task.FromResult(new Metadata()), // Metadata (empty)
                () => Status.DefaultSuccess, // Status (OK)
                () => new Metadata(), // Trailers (empty)
                () => { } // Dispose
            );

            _registerGrpcMock
                .Setup(x => x.AddMasterPersonRecordIndexAsync(It.IsAny<MasterPersonRecordIndexAddMessage>(), null, null, It.IsAny<CancellationToken>()))
                .Returns(new AsyncUnaryCall<MasterPersonRecordIndexAddResponse>(
                    Task.FromResult(new MasterPersonRecordIndexAddResponse
                    {
                        Id = responseGuid.ToString(),
                        Status = new ResultStatus { Code = ResultCodes.Ok }
                    }),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }
                ));


            // Arrange
            var existingProcessId = Guid.Parse("de2de41b-a8a7-4ffa-a7a4-849fa89d4ebd");
            var formParentId = Guid.NewGuid();
            var existingFormId = Guid.NewGuid();
            var formFields = new List<FormField>
            {
                new FormField
                {
                    Name = "companyNameImmutable",

                    IsBatchOwner = true,
                    IsSubmitter = true,
                    Type = SimpleFormFieldType.Company.ToString(),
                    Fields = new List<FormField>
                    {
                        new FormField
                        {
                            Name = "xxxcompanyNumberImmutable",
                            Value = "a:b"
                        },
                        new FormField
                        {
                            Name = "companyNameImmutable",
                            Value = "a:b"
                        }
                    },
                    Value = "c"
                }
            };

            var model = new ProcessStepVM
            {
                ProcessId = existingProcessId,
                FromProcessId = null,
                ServiceId = 1,
                ServiceStepId = 200,
                OrderNum = 2,
                IncomingDate = new DateTime(2021, 1, 1),
                IncomingNumber = "OLD-456",
                FileId = Guid.NewGuid(),
                FormFields = formFields,
                FormParentId = 1
            };

            var existingProcess = new Process
            {
                Id = existingProcessId,
                ServiceId = model.ServiceId,
                IncomingNumber = "IN-999",
                IncomingDate = DateTime.Today.AddDays(-1),
                StatusId = 3,
                RegisterNumber = "1",
                // RegisteredStepId = existingProcessId,
                FromProcessId = existingProcessId
            };

            var existingForm = new Form
            {
                Id = 2,
                ParentId = 1
            };

            // Create in-memory lists
            var processes = new List<Process> { existingProcess };
            var forms = new List<Form> { existingForm };
            var serviceSteps = new List<ServiceStep>
            {
                new ServiceStep
                {
                    Id = model.ServiceStepId,
                    StatusId = (int)ProcessStatus.Registered // or Certificate, etc.
                }
            };
            var instructionResponses = new List<InstructionResponse>
            {
            };

            var mockProcessDbSet = processes.BuildMockDbSet();
            _repoMock.Setup(r => r.All<Process>()).Returns(mockProcessDbSet.Object);
            var mockFormDbSet = forms.BuildMockDbSet();
            _repoMock.Setup(r => r.All<Form>()).Returns(mockFormDbSet.Object);
            var mockServiceStepDbSet = serviceSteps.BuildMockDbSet();
            _repoMock.Setup(r => r.AllReadonly<ServiceStep>()).Returns(mockServiceStepDbSet.Object);
            var mockInstructionResponseDbSet = instructionResponses.BuildMockDbSet();
            _repoMock.Setup(r => r.All<InstructionResponse>()).Returns(mockInstructionResponseDbSet.Object);
            
            _repoMock.Setup(r => r.AddAsync(It.IsAny<ProcessStep>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            _registerServiceMock
                .Setup(x => x.GetCurrentRegister())
                .ReturnsAsync(new RegisterVM { Code = "REG-001" });

            _numberGeneratorMock
                .Setup(x => x.GetNumberAsync(It.IsAny<NumberRequest>(), null, null, It.IsAny<CancellationToken>()))
                .Returns(new AsyncUnaryCall<NumberReply>(
                    Task.FromResult(new NumberReply
                    {
                        Number = 12345,
                        Status = new ResultStatus { Code = ResultCodes.Ok, Message = "Mock error" }
                    }),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }
                ));

            _objectsCatalogMock
                .Setup(x => x.GetFieldsListAsync(It.IsAny<Google.Protobuf.WellKnownTypes.Empty>(), null, null, It.IsAny<CancellationToken>()))
                .Returns(new AsyncUnaryCall<CatalogFieldsListReply>(
                    Task.FromResult(new CatalogFieldsListReply
                    {
                        // fill any properties inside FieldsListReply if needed
                    }),
                    Task.FromResult(new Metadata()),
                    () => Status.DefaultSuccess,
                    () => new Metadata(),
                    () => { }
                ));

            // Act
            (_,var result) = await _processService.AddStep(model);
            
            // Assert
            Assert.AreEqual("IN-999", result.IncomingNumber);
            _repoMock.Verify(r => r.AddAsync(It.IsAny<ProcessStep>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task GetProcessForCertificateForPersonTest()
        {
            List<FormField> formList = new List<FormField>();

            formList.Add(new FormField()
            {
                Type = SimpleFormFieldType.Person.ToString(),
                IsBatchOwner = true,
                Fields = new List<FormField>()
                {
                    new FormField()
                    {
                        Type = SimpleFormFieldType.PersonIdentifier.ToString(),
                        Value = "1:0848038103",
                        IsBatchOwner = true,
                        Name = "PersonIdentifierImmutable"
                    },
                    new FormField()
                    {
                        Type = SimpleFormFieldType.Text.ToString(),
                        Value = "Карло",
                        Name = "test_firstNameImmutable"
                    }, new FormField()
                    {
                        Type = SimpleFormFieldType.Text.ToString(),
                        Value = "Джонсънсон",
                        Name = "test_middleNameImmutable"
                    }, new FormField()
                    {
                        Type = SimpleFormFieldType.Text.ToString(),
                        Value = "Педерсоли",
                        Name = "test_lastNameImmutable"
                    }
                }
            });

            Guid responseGuid = new Guid("77953de4-789b-45d8-8677-d69e9d475bfe");
            var fakeResponse = new MPRIListMessage
            {
                Items = { new URegister.RegistersCatalog.MPRILisItemMessage()
                {
                    Id = responseGuid.ToString()
                } }
            };


            // Simulate gRPC response with AsyncUnaryCall<T>
            var asyncUnaryCall = new AsyncUnaryCall<MPRIListMessage>(
                Task.FromResult(fakeResponse), // Task with fake response
                Task.FromResult(new Metadata()), // Metadata (empty)
                () => Status.DefaultSuccess, // Status (OK)
                () => new Metadata(), // Trailers (empty)
                () => { } // Dispose
            );

            _registerGrpcMock
                .Setup(client => client.GetMasterPersonRecordIndexAsync(
                    It.Is<GetMasterPersonRecordIndexMessage>(msg => msg.Pid == "0848038103" && msg.PidType == "1"),
                    null, // CallOptions (can be null)
                    default,
                    default(CancellationToken)// CancellationToken
                ))
                .Returns(asyncUnaryCall); // Return the properly wrapped response

            var testProcess = new Process
            {
                Id = new Guid(),
                MpriId = responseGuid,
                Service = new Service { ServiceTypeId = (int)ServiceTypes.Register },
                RegisterItems = new List<URegister.Core.Data.Models.Process.RegisterItem>
                    { new URegister.Core.Data.Models.Process.RegisterItem()
                    {
                        MpriId = responseGuid
                    }}
            };

            // Create mock DbContext
            var mockDbContext = new Mock<DbContext>();
            var processes = new List<Process> { testProcess };

            // Setup DbSet with async support
            mockDbContext.Setup(x => x.Set<Process>())
                .ReturnsDbSet(processes);


            _repoMock.Setup(r => r.AllReadonly<Process>())
                .Returns(mockDbContext.Object.Set<Process>());


            Process? result = await _processService.GetProcessForCertificate(formList);
            Assert.NotNull(result);
            Assert.AreEqual(responseGuid, result.MpriId);
            Assert.AreEqual(1, result.RegisterItems.Count);
            Assert.AreEqual(responseGuid, result.RegisterItems.Single().MpriId);
        }

        [Test]
        public async Task GetProcessForCertificateForCompanyTest()
        {
            List<FormField> formList = new List<FormField>();

            formList.Add(new FormField()
            {
                Type = SimpleFormFieldType.Company.ToString(),
                IsBatchOwner = true,
                Fields = new List<FormField>()
                {
                    new FormField()
                    {
                        Type = SimpleFormFieldType.PersonIdentifier.ToString(),
                        Value = "1:000694749",
                        IsBatchOwner = true,
                        Name = "test_companyNumberImmutable"
                    },
                    new FormField()
                    {
                        Type = SimpleFormFieldType.Text.ToString(),
                        Value = "ОЧЗ",
                        Name = "test_companyNameImmutable"
                    }
                }
            });

            Guid responseGuid = new Guid("77953de4-789b-45d8-8677-d69e9d475bfe");
            var fakeResponse = new MPRIListMessage
            {
                Items = { new URegister.RegistersCatalog.MPRILisItemMessage()
                {
                    Id = responseGuid.ToString()
                } }
            };


            // Simulate gRPC response with AsyncUnaryCall<T>
            var asyncUnaryCall = new AsyncUnaryCall<MPRIListMessage>(
                Task.FromResult(fakeResponse), // Task with fake response
                Task.FromResult(new Metadata()), // Metadata (empty)
                () => Status.DefaultSuccess, // Status (OK)
                () => new Metadata(), // Trailers (empty)
                () => { } // Dispose
            );

            _registerGrpcMock
                .Setup(client => client.GetMasterPersonRecordIndexAsync(
                    It.Is<GetMasterPersonRecordIndexMessage>(msg => msg.Pid == "000694749" && msg.PidType == "1"),
                    null, // CallOptions (can be null)
                    default,
                    default(CancellationToken)// CancellationToken
                ))
                .Returns(asyncUnaryCall); // Return the properly wrapped response

            var testProcess = new Process
            {
                Id = new Guid(),
                MpriId = responseGuid,
                Service = new Service { ServiceTypeId = (int)ServiceTypes.Register },
                RegisterItems = new List<URegister.Core.Data.Models.Process.RegisterItem>
                    { new URegister.Core.Data.Models.Process.RegisterItem()
                    {
                        MpriId = responseGuid
                    }}
            };

            // Create mock DbContext
            var mockDbContext = new Mock<DbContext>();
            var processes = new List<Process> { testProcess };

            // Setup DbSet with async support
            mockDbContext.Setup(x => x.Set<Process>())
                .ReturnsDbSet(processes);


            _repoMock.Setup(r => r.AllReadonly<Process>())
                .Returns(mockDbContext.Object.Set<Process>());


            Process? result = await _processService.GetProcessForCertificate(formList);

            Assert.NotNull(result);
            Assert.AreEqual(responseGuid, result.MpriId);
            Assert.AreEqual(1, result.RegisterItems.Count);
            Assert.AreEqual(responseGuid, result.RegisterItems.Single().MpriId);
        }
    }
}
