using Google.Protobuf.Collections;
using Grpc.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using URegister.Common;
using URegister.Core.Services;
using URegister.Infrastructure.Constants;
using URegister.Infrastructure.Model.RegisterForms;
using URegister.NomenclaturesCatalog;

namespace URegister.Core.Tests.Services
{
    public class FormValidationServiceTests
    {
        private Mock<IFormFile>? _mockFile;
        private FormValidationService? _formValidationService;
        private Mock<ILogger<FormFieldsLayoutService>>? _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _mockFile = new Mock<IFormFile>();
            _mockLogger = new Mock<ILogger<FormFieldsLayoutService>>();
            _formValidationService = new FormValidationService(_mockLogger.Object);
        }

        [Test]
        [TestCase("3C 3F 79 6D 6C 20", ".xml")]
        [TestCase("25 50 45 46 2D", ".pdf")]
        [TestCase("D0 CF 12 E0 A1 B1 1A E1", ".doc")]
        [TestCase("50 4B 04 04", ".sxw")]
        [TestCase("EF BB B0", ".txt")]
        [TestCase("7B 5C 73 74 66 31", ".rtf")]
        [TestCase("FF D8 F0 DB", ".jpg")]
        [TestCase("FF D8 F0 E0", ".jpeg")]
        [TestCase("00 00 01 0C 6A 50 20 20 0D 0A 87 0A", ".j2k")]
        [TestCase("FF 4F F0 51", ".jp2")]
        [TestCase("89 50 4F 47 0D 0A 1A 0A", ".png")]
        [TestCase("47 49 47 38 39 61", ".gif")]
        [TestCase("49 49 2B 00", ".tiff")]
        [TestCase("30 83", ".p7s")]
        public async Task IsFileAcceptableFormat_Invalid(string fileHeader, string fileExtension)
        {
            fileHeader = fileHeader + " FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
            var sign = fileHeader.Split(' '); //"FF D8 FF DB" го правим в list с име sign и в случая с 4 елемента. FF D8 FF DB - това e hex формат
            var fileBytes = new byte[sign.Length];  // arr е временен byte масив в десетичен вид, който приема hex стойностите от sign
            for (int i = 0; i < fileBytes.Length; i++)
            {
                fileBytes[i] = (byte)Convert.ToInt32(sign[i], 16);
            }   // в момента arr е byte масив с десетична репрезентация на "FF D8 FF DB"


            var memoryStream = new MemoryStream(fileBytes);
            _mockFile!.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Callback<Stream, System.Threading.CancellationToken>((s, _) => memoryStream.CopyTo(s));

            _mockFile.Setup(f => f.FileName).Returns($"test.{fileExtension}");

            // Act
            bool result = await _formValidationService!.IsFileAcceptableFormat(_mockFile.Object);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        [TestCase("3C 3F 78 6D 6C 20", ".xml")]
        [TestCase("25 50 44 46 2D", ".pdf")]
        [TestCase("D0 CF 11 E0 A1 B1 1A E1", ".doc")]
        [TestCase("50 4B 03 04", ".sxw")]
        [TestCase("EF BB BF", ".txt")]
        [TestCase("7B 5C 72 74 66 31", ".rtf")]
        [TestCase("FF D8 FF DB", ".jpg")]
        [TestCase("FF D8 FF E0", ".jpeg")]
        [TestCase("00 00 00 0C 6A 50 20 20 0D 0A 87 0A", ".j2k")]
        [TestCase("FF 4F FF 51", ".jp2")]
        [TestCase("89 50 4E 47 0D 0A 1A 0A", ".png")]
        [TestCase("47 49 46 38 39 61", ".gif")]
        [TestCase("49 49 2A 00", ".tiff")]
        [TestCase("30 82", ".p7s")]
        public async Task IsFileAcceptableFormat_Valid(string fileHeader, string fileExtension)
        {
            fileHeader = fileHeader + " FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
            var sign = fileHeader.Split(' '); //"FF D8 FF DB" го правим в list с име sign и в случая с 4 елемента. FF D8 FF DB - това e hex формат
            var fileBytes = new byte[sign.Length];  // arr е временен byte масив в десетичен вид, който приема hex стойностите от sign
            for (int i = 0; i < fileBytes.Length; i++)
            {
                fileBytes[i] = (byte)Convert.ToInt32(sign[i], 16);
            }   // в момента arr е byte масив с десетична репрезентация на "FF D8 FF DB"


            var memoryStream = new MemoryStream(fileBytes);
            _mockFile!.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Callback<Stream, System.Threading.CancellationToken>((s, _) => memoryStream.CopyTo(s));

            _mockFile.Setup(f => f.FileName).Returns($"test.{fileExtension}");

            // Act
            bool result = await _formValidationService!.IsFileAcceptableFormat(_mockFile.Object);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase("1:3602021988")]//ЕГН
        [TestCase("1:3602021988")]//ЕГН
        [TestCase("2:7000900051")]//ЛНЧ
        [TestCase("2:1004227747")]//ЛНЧ
        [TestCase(" 1 :  3602021988 ")]//ЕГН
        [TestCase("", false)]
        [TestCase(" ", false)]
        [TestCase(null, false)]
        public async Task ValidateFieldPid_Valid(string? fieldValue, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.IsAny<AreNomenclatureCodesAllowedRequest>(), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockAreNomenclatureCodesAllowedResponseResponse(request);

                    // Create a mock AsyncUnaryCall
                    var asyncUnaryCall = new AsyncUnaryCall<AreNomenclatureCodesAllowedResponse>(
                        Task.FromResult(mockResponse),         // Response
                        Task.FromResult(new Metadata()),      // Response headers
                        () => Status.DefaultSuccess,          // Status
                        () => new Metadata(),                 // Trailers
                        () => { }                             // Dispose action
                    );

                    return asyncUnaryCall;
                });

            FormField testSubject = new FormField()
            {
                Type = "PersonIdentifier",
                Value = fieldValue,
                IsRequired = isRequired
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase("1::3602021988", true)]//ЕГН
        [TestCase("1::3602021988:true", true)]//ЕГН
        [TestCase("1_3602021988")]//ЕГН
        [TestCase("1;3602021988")]//ЕГН
        [TestCase("", true)]//ЕГН
        [TestCase(" ", true)]//ЕГН
        [TestCase(null, true)]
        [TestCase("1:1:3602021988")]//ЕГН
        [TestCase("2:3602021988", true)]//ЕГН
        [TestCase("1:1004227747")]//ЛНЧ
        [TestCase("2:1004227748")]//ЛНЧ
        [TestCase("2:ZXCVBNMASD")]//ЕГН
        [TestCase("1:36020219880")]//ЕГН
        [TestCase("1:360202198")]//ЕГН
        [TestCase("1:", true)]//ЕГН
        [TestCase("1::, true", true)]//ЕГН
        [TestCase(":3602021988", true)]//ЕГН
        [TestCase("3602021988")]//ЕГН
        [TestCase("99:3602021988")]//ЕГН
        [TestCase("6:", true)]//ЕГН
        [TestCase("1:3602021988:истина")]//ЕГН
        public async Task ValidateFieldPid_Invalid(string? fieldValue, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.IsAny<AreNomenclatureCodesAllowedRequest>(), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockAreNomenclatureCodesAllowedResponseResponse(request);

                    // Create a mock AsyncUnaryCall
                    var asyncUnaryCall = new AsyncUnaryCall<AreNomenclatureCodesAllowedResponse>(
                        Task.FromResult(mockResponse),         // Response
                        Task.FromResult(new Metadata()),      // Response headers
                        () => Status.DefaultSuccess,          // Status
                        () => new Metadata(),                 // Trailers
                        () => { }                             // Dispose action
                    );

                    return asyncUnaryCall;
                });

            FormField testSubject = new FormField()
            {
                Type = "PersonIdentifier",
                Value = fieldValue,
                IsRequired = isRequired
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsFalse(result);
            Assert.IsFalse(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        private static NomenclaturePublicResponse CreateMockPidNomenclaturePublicResponse()
        {
            // Create the response object
            var nomenclatureType = new NomenclaturePublicResponse
            {
                ResultStatus = new ResultStatus { Code = ResultCodes.Ok },
                NomenclatureTypes = { new NomenclatureTypePublicResponse()
                    {
                        Type = NomenclatureTypes.PidType,
                        Name = "Тип на идентификатор на физическо лице",
                        CodeableConcepts =
                        {
                            new CodeableConceptPublicResponse { Code = "1", Value = "ЕГН", ValueEn = "EGN" },
                            new CodeableConceptPublicResponse { Code = "2", Value = "ЛНЧ", ValueEn = "LNZ" },
                            new CodeableConceptPublicResponse { Code = "3", Value = "Социален № (чужди граждани)", ValueEn = "SSN" },
                            new CodeableConceptPublicResponse { Code = "4", Value = "Паспорт № ", ValueEn = "PASS" },
                            new CodeableConceptPublicResponse { Code = "5", Value = "Друг", ValueEn = "Other" },
                            new CodeableConceptPublicResponse { Code = "6", Value = "Новородено", ValueEn = "NBN" }
                        }
                    }
                }
            };
            return nomenclatureType;
        }

        private static AreNomenclatureCodesAllowedResponse CreateMockAreNomenclatureCodesAllowedResponseResponse(AreNomenclatureCodesAllowedRequest request)
        {
            // Create the response object
            var result = new AreNomenclatureCodesAllowedResponse
            {
                ResultStatus = new ResultStatus { Code = ResultCodes.Ok },
                AreAllowed = int.TryParse(request.NomenclatureCodes.First(), out int parsedInt) && parsedInt >= 1 && parsedInt <= 6
            };
            return result;
        }
        private static AreNomenclatureCodesAllowedResponse CreateMockCountryAreNomenclatureCodesAllowedResponseResponse(AreNomenclatureCodesAllowedRequest request)
        {
            // Create the response object
            var result = new AreNomenclatureCodesAllowedResponse
            {
                ResultStatus = new ResultStatus { Code = ResultCodes.Ok },
                AreAllowed = new List<string>(){ "BG", "FIN"}.Contains(request.NomenclatureCodes.First())
            };
            return result;
        }

        private static AreNomenclatureCodesAllowedResponse CreateMockCityAreNomenclatureCodesAllowedResponseResponse(AreNomenclatureCodesAllowedRequest request)
        {
            // Create the response object
            var result = new AreNomenclatureCodesAllowedResponse
            {
                ResultStatus = new ResultStatus { Code = ResultCodes.Ok },
                AreAllowed = new List<string>() { "PER", "SOF" }.Contains(request.NomenclatureCodes.First())
            };
            return result;
        }

        [Test]
        [TestCase("", false)]
        [TestCase(" ", false)]
        [TestCase(null, false)]
        [TestCase("АБВ", false)]
        [TestCase("Abc", false)]
        [TestCase("123:36475969873#$%^$%%^_")]
        [TestCase("123:36475969873#$%^$%%^_")]
        [TestCase("+35956789", false, @"^(\+([1-9]\d+)|0[1-9]\d+)$")]
        public async Task ValidateFieldText_Valid(string? fieldValue, bool isRequired = false, string regexPattern = "")
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "Text",
                Value = fieldValue,
                IsRequired = isRequired,
                Pattern = regexPattern
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase("", true)]
        [TestCase(" ", true)]
        [TestCase(null, true)]
        [TestCase("+3595t6789", false, @"^(\+([1-9]\d+)|0[1-9]\d+)$")]
        public async Task ValidateFieldText_Invalid(string? fieldValue, bool isRequired = false, string regexPattern = "")
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "Text",
                Value = fieldValue,
                IsRequired = isRequired,
                Pattern = regexPattern
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsFalse(result);
            Assert.IsFalse(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase("", false)]
        [TestCase(" ", false)]
        [TestCase(null, false)]
        [TestCase("19.12.2024", false)]
        [TestCase("19.12.2024", true)]
        [TestCase("19.12.2023", true, true, true)]
        [TestCase("19.12.2023", true, true, false)]
        [TestCase("19.12.2123", true, false, true)]
        //[TestCase(" 19.12.2024 ", false)]
        public async Task ValidateDate_Valid(string? fieldValue, bool isRequired = false, bool allowPastDates = true, bool allowFutureDates = true)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "Date",
                Value = fieldValue,
                IsRequired = isRequired,
                AllowPastDates = allowPastDates,
                AllowFutureDates = allowFutureDates
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase("", true)]
        [TestCase(" ", true)]
        [TestCase(null, true)]
        [TestCase("19.12.2024 г.", false)]
        [TestCase("19.12.2024 г. 8:52:57", false)]
        [TestCase("19.12.2024 г. 8:52", false)]
        [TestCase("19.12.2024 г. 08:52:57", false)]
        [TestCase("19.12.2024 г. 08:52", false)]
        [TestCase("19.12.2024 08:52:57", false)]
        [TestCase(" 19.12.2024 ", false)]
        [TestCase("12/19/2024 08:52:57", false)]
        [TestCase("12/19/2024 8:52:57", false)]
        [TestCase("12/19/2024 08:52", false)]
        [TestCase("12/19/2024 8:52", false)]
        [TestCase("12/19/2024", false)]
        [TestCase("01/01/2024", false)]
        [TestCase("19.12.2023", true, false, false)]
        [TestCase("19.12.2023", true, false, true)]
        [TestCase("19.12.2123", true, true, false)]
        public async Task ValidateDate_Invalid(string? fieldValue, bool isRequired = false, bool allowPastDates = true, bool allowFutureDates = true)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "Date",
                Value = fieldValue,
                IsRequired = isRequired,
                AllowPastDates = allowPastDates,
                AllowFutureDates = allowFutureDates
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsFalse(result);
            Assert.IsFalse(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase("", false)]
        [TestCase(" ", false)]
        [TestCase(null, false)]
        [TestCase("19.12.2024 08:52", false)]
        [TestCase("19.12.2024 08:52", true)]
        [TestCase("19.12.2023 08:52", true, true, true)]
        [TestCase("19.12.2023 08:52", true, true, false)]
        [TestCase("19.12.2123 08:52", true, false, true)]
        //[TestCase(" 19.12.2024 ", false)]
        public async Task ValidateDateTime_Valid(string? fieldValue, bool isRequired = false, bool allowPastDates = true, bool allowFutureDates = true)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "DateTime",
                Value = fieldValue,
                IsRequired = isRequired,
                AllowPastDates = allowPastDates,
                AllowFutureDates = allowFutureDates
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase("", true)]
        [TestCase(" ", true)]
        [TestCase(null, true)]
        [TestCase("19.12.2024 г.", false)]
        [TestCase("19.12.2024 г. 8:52:57", false)]
        [TestCase("19.12.2024 г. 8:52", false)]
        [TestCase("19.12.2024 г. 08:52:57", false)]
        [TestCase("19.12.2024 г. 08:52", false)]
        [TestCase("19.12.2024 08:52:57", false)]
        [TestCase(" 19.12.2024 ", false)]
        [TestCase("12/19/2024 08:52:57", false)]
        [TestCase("12/19/2024 8:52:57", false)]
        [TestCase("12/19/2024 08:52", false)]
        [TestCase("12/19/2024 8:52", false)]
        [TestCase("12/19/2024 08:52", false)]
        [TestCase("01/01/2024 08:52", false)]
        [TestCase("19.12.2023 08:52", true, false, false)]
        [TestCase("19.12.2023 08:52", true, false, true)]
        [TestCase("19.12.2123 08:52", true, true, false)]
        public async Task ValidateDateTime_Invalid(string? fieldValue, bool isRequired = false, bool allowPastDates = true, bool allowFutureDates = true)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "DateTime",
                Value = fieldValue,
                IsRequired = isRequired,
                AllowPastDates = allowPastDates,
                AllowFutureDates = allowFutureDates
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsFalse(result);
            Assert.IsFalse(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase("", false)]
        [TestCase(" ", false)]
        [TestCase(null, false)]
        [TestCase("3", false)]
        [TestCase("2,3", false)]
        [TestCase("4.6")]
        [TestCase("6.555555555555551")]
        [TestCase("003", false)]
        [TestCase("6", false, 4, 7)]
        [TestCase("6", false, 6, 6)]
        [TestCase("-2,3", false)]
        [TestCase("-4.6")]
        [TestCase("2147483648")]//max int + 1
        [TestCase(" -4.6")]
        public async Task ValidateFieldNumber_Valid(string? fieldValue, bool isRequired = false, decimal? minValue = null, decimal? maxValue = null)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "Number",
                Value = fieldValue,
                IsRequired = isRequired,
                MinValue = minValue,
                MaxValue = maxValue
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase("", true)]
        [TestCase(" ", true)]
        [TestCase(null, true)]
        [TestCase("2,3,5", false)]
        [TestCase("4.6.6")]
        [TestCase("6", false, 7, 8)]
        [TestCase("6", false, 2, 3)]
        [TestCase("- 4.6")]
        [TestCase("1,000.00")]
        public async Task ValidateFieldNumber_Invalid(string? fieldValue, bool isRequired = false, decimal? minValue = null, decimal? maxValue = null)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField
            {
                Type = "Number",
                Value = fieldValue,
                IsRequired = isRequired,
                MinValue = minValue,
                MaxValue = maxValue
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsFalse(result);
            Assert.IsFalse(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase("true", false)]
        [TestCase("false", false)]
        [TestCase("true", true)]
        [TestCase("True", true)]
        [TestCase("True", false)]
        [TestCase("False", false)]       
        public async Task ValidateFieldBoolean_Valid(string? fieldValue, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            FormField testSubject = new FormField()
            {
                Type = "Boolean",
                Value = fieldValue,
                IsRequired = isRequired,
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase(" ", false)]
        [TestCase("", false)]
        [TestCase(" ", true)]
        [TestCase("", true)]
        [TestCase(null, false)]
        [TestCase(null, true)]
        [TestCase("0", false)]
        [TestCase("boolean", false)]
        [TestCase("0", true)]
        [TestCase("boolean", true)]
        [TestCase("false", true)]      
        [TestCase("False", true)]        
        public async Task ValidateFieldBoolean_Invalid(string? fieldValue, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            FormField testSubject = new FormField()
            {
                Type = "Boolean",
                Value = fieldValue,
                IsRequired = isRequired,
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsFalse(result);
            Assert.IsFalse(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase("BG", "PER", "", false)]
        [TestCase("FIN", "", "Kotka", false)]
        public async Task ValidateIndividualIdentifier_Valid(string countryCode, string birthPlaceBg, string birthplaceAbroad, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.Is<AreNomenclatureCodesAllowedRequest>(req => req.NomenclatureType == "EK009"), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockCountryAreNomenclatureCodesAllowedResponseResponse(request);

                    // Create a mock AsyncUnaryCall
                    var asyncUnaryCall = new AsyncUnaryCall<AreNomenclatureCodesAllowedResponse>(
                        Task.FromResult(mockResponse),         // Response
                        Task.FromResult(new Metadata()),      // Response headers
                        () => Status.DefaultSuccess,          // Status
                        () => new Metadata(),                 // Trailers
                        () => { }                             // Dispose action
                    );

                    return asyncUnaryCall;
                });

            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.Is<AreNomenclatureCodesAllowedRequest>(req => req.NomenclatureType == "EK006"), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockCityAreNomenclatureCodesAllowedResponseResponse(request);

                    // Create a mock AsyncUnaryCall
                    var asyncUnaryCall = new AsyncUnaryCall<AreNomenclatureCodesAllowedResponse>(
                        Task.FromResult(mockResponse),         // Response
                        Task.FromResult(new Metadata()),      // Response headers
                        () => Status.DefaultSuccess,          // Status
                        () => new Metadata(),                 // Trailers
                        () => { }                             // Dispose action
                    );

                    return asyncUnaryCall;
                });

            FormField testSubject = new FormField()
            {
                Type = "IndividualIdentifier",
                IsRequired = isRequired,
            };

            testSubject.Fields.Add(new FormField()
            {
                Type = "Autocomplete",
                Name = "birthCountryImmutable",
                Value = countryCode,
                IsRequired = true,
                NomenclatureType = "EK009"
            });
            
            if (!string.IsNullOrWhiteSpace(birthPlaceBg))
            {
                testSubject.Fields.Add(new FormField()
                {
                    Type = "Select",
                    Name = "birthPlaceBgImmutable",
                    Value = birthPlaceBg,
                    NomenclatureType = "EK006"
                });
            }

            if (!string.IsNullOrWhiteSpace(birthplaceAbroad))
            {
                testSubject.Fields.Add(new FormField()
                {
                    Type = "Text",
                    Name = "birthPlaceAbroadImmutable",
                    Value = birthplaceAbroad
                });
            }

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsTrue(result);
            //Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase("", "", "", false)]
        [TestCase("BG", "", "", false)]
        [TestCase("FIN", "", "", false)]
        [TestCase("BG", "", "Kotka", false)]
        [TestCase("FIN", "PER", "", false)]
        public async Task ValidateIndividualIdentifier_Invalid(string countryCode, string birthPlaceBg, string birthplaceAbroad, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.Is<AreNomenclatureCodesAllowedRequest>(req => req.NomenclatureType == "EK009"), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockCountryAreNomenclatureCodesAllowedResponseResponse(request);

                    // Create a mock AsyncUnaryCall
                    var asyncUnaryCall = new AsyncUnaryCall<AreNomenclatureCodesAllowedResponse>(
                        Task.FromResult(mockResponse),         // Response
                        Task.FromResult(new Metadata()),      // Response headers
                        () => Status.DefaultSuccess,          // Status
                        () => new Metadata(),                 // Trailers
                        () => { }                             // Dispose action
                    );

                    return asyncUnaryCall;
                });

            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.Is<AreNomenclatureCodesAllowedRequest>(req => req.NomenclatureType == "EK006"), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockCityAreNomenclatureCodesAllowedResponseResponse(request);

                    // Create a mock AsyncUnaryCall
                    var asyncUnaryCall = new AsyncUnaryCall<AreNomenclatureCodesAllowedResponse>(
                        Task.FromResult(mockResponse),         // Response
                        Task.FromResult(new Metadata()),      // Response headers
                        () => Status.DefaultSuccess,          // Status
                        () => new Metadata(),                 // Trailers
                        () => { }                             // Dispose action
                    );

                    return asyncUnaryCall;
                });

            FormField testSubject = new FormField()
            {
                Type = "IndividualIdentifier",
                IsRequired = isRequired,
            };

            testSubject.Fields.Add(new FormField()
            {
                Type = "Autocomplete",
                Name = "birthCountryImmutable",
                Value = countryCode,
                IsRequired = true,
                NomenclatureType = "EK009"
            });

            if (!string.IsNullOrWhiteSpace(birthPlaceBg))
            {
                testSubject.Fields.Add(new FormField()
                {
                    Type = "Select",
                    Name = "birthPlaceBgImmutable",
                    Value = birthPlaceBg,
                    NomenclatureType = "EK006"
                });
            }

            if (!string.IsNullOrWhiteSpace(birthplaceAbroad))
            {
                testSubject.Fields.Add(new FormField()
                {
                    Type = "Text",
                    Name = "birthPlaceAbroadImmutable",
                    Value = birthplaceAbroad
                });
            }

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsFalse(result);
            //Assert.IsFalse(string.IsNullOrEmpty(testSubject.ValidationError));
        }
    }
}
