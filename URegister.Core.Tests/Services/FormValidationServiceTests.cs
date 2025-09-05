using System.Globalization;
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
        [TestCase("3C 3F 78 6D 6C 20", ".xml")]
        [TestCase("25 50 44 46 2D", ".pdf")]
        [TestCase("D0 CF 11 E0 A1 B1 1A E1", ".doc")]
        [TestCase("50 4B 03 04", ".sxw")]
        [TestCase("EF BB BF", ".txt")]
        [TestCase("FF FE", ".txt")]
        [TestCase("FE FF", ".txt")]
        [TestCase("00 00 FE FF", ".txt")]
        [TestCase("AA 00 FE FF", ".txt")] // not BOM
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
        [TestCase("3C 3F 79 6D 6C 20", ".xml")]
        [TestCase("25 50 45 46 2D", ".pdf")]
        [TestCase("D0 CF 12 E0 A1 B1 1A E1", ".doc")]
        [TestCase("50 4B 04 04", ".sxw")]
        //[TestCase("EF BB B0", ".txt")]
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

        [TestCase("", false)]
        [TestCase(" ", false)]
        [TestCase(null, false)]
        [TestCase("f7e793d6-902b-4d8c-a703-09932b661772", true)]
        [TestCase("83067062-68f1-41a0-b5f8-d5518d2aecf7", true)]
        [TestCase("f7e793d6902b4d8ca70309932b661773", true)]
        public async Task ValidateFileFieldKey_Valid(string? key, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "File",
                Value = key,
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

        [TestCase("", true)]
        [TestCase(" ", true)]
        [TestCase(null, true)]
        [TestCase("83067062", true)]
        [TestCase("текст", true)]
        public async Task ValidateFileFieldKey_Invalid(string? key, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "File",
                Value = key,
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

        [TestCase(".txt", ".txt")]
        [TestCase(".txt,.jpg", ".txt")]
        [TestCase("", ".txt")]
        
        public async Task ValidateUploadedFileType_Valid(string allowedFileExtensions, string fileExtension)
        {
            var fileHeader = "EF BB BF" + " FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
            var sign = fileHeader.Split(' '); //"FF D8 FF DB" го правим в list с име sign и в случая с 4 елемента. FF D8 FF DB - това e hex формат
            var fileBytes = new byte[sign.Length];  // arr е временен byte масив в десетичен вид, който приема hex стойностите от sign
            for (int i = 0; i < fileBytes.Length; i++)
            {
                fileBytes[i] = (byte)Convert.ToInt32(sign[i], 16);
            }

            var memoryStream = new MemoryStream(fileBytes);
            var fileMock = new Mock<IFormFile>();

            // Create a fake 2 MB memory stream
            var fileSizeInBytes = 2 * 1024 * 1024;

            fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(f => f.Length).Returns(fileSizeInBytes);
            fileMock.Setup(f => f.FileName).Returns("testfile" + fileExtension);
            fileMock.Setup(f => f.ContentType).Returns("text/plain");

            FormField testSubject = new FormField()
            {
                Type = "File",
                IsRequired = false,
                AllowedFileSizeInMB = 2,
                AllowedFileExtensions = allowedFileExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries)
            };

            bool result = await _formValidationService!.ValidateFile(testSubject, fileMock.Object);

            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrWhiteSpace(testSubject.ValidationError));
        }

        [TestCase(".txt", ".jpg")]
        [TestCase(".txt,.jpg", ".png")]

        public async Task ValidateUploadedFileType_Invalid(string allowedFileExtensions, string fileExtension)
        {
            var fileHeader = "EF BB BF" + " FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
            var sign = fileHeader.Split(' '); //"FF D8 FF DB" го правим в list с име sign и в случая с 4 елемента. FF D8 FF DB - това e hex формат
            var fileBytes = new byte[sign.Length];  // arr е временен byte масив в десетичен вид, който приема hex стойностите от sign
            for (int i = 0; i < fileBytes.Length; i++)
            {
                fileBytes[i] = (byte)Convert.ToInt32(sign[i], 16);
            }

            var memoryStream = new MemoryStream(fileBytes);
            var fileMock = new Mock<IFormFile>();

            // Create a fake 2 MB memory stream
            var fileSizeInBytes = 2 * 1024 * 1024;

            fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(f => f.Length).Returns(fileSizeInBytes);
            fileMock.Setup(f => f.FileName).Returns("testfile" + fileExtension);
            fileMock.Setup(f => f.ContentType).Returns("text/plain");

            var allowedExtensionsAsArray = 
                allowedFileExtensions.Split(',', StringSplitOptions.RemoveEmptyEntries);

            FormField testSubject = new FormField()
            {
                Type = "File",
                IsRequired = false,
                AllowedFileSizeInMB = 2,
                AllowedFileExtensions = allowedExtensionsAsArray
            };

            bool result = await _formValidationService!.ValidateFile(testSubject, fileMock.Object);

            Assert.IsFalse(result);
            Assert.AreEqual(string.Format(MessageConstant.Values.FileTypeRejected, string.Join("; ", allowedExtensionsAsArray)), 
                testSubject.ValidationError);
        }

        [TestCase(2, 5, false)]
        [TestCase(2, 2, false)]
        public async Task ValidateUploadedFileSize_Valid(int fileSizeInMB, int fileLimitInMB, bool isRequired = false)
        {
            var fileHeader = "EF BB BF" + " FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
            var sign = fileHeader.Split(' '); //"FF D8 FF DB" го правим в list с име sign и в случая с 4 елемента. FF D8 FF DB - това e hex формат
            var fileBytes = new byte[sign.Length];  // arr е временен byte масив в десетичен вид, който приема hex стойностите от sign
            for (int i = 0; i < fileBytes.Length; i++)
            {
                fileBytes[i] = (byte)Convert.ToInt32(sign[i], 16);
            }

            var memoryStream = new MemoryStream(fileBytes);
            var fileMock = new Mock<IFormFile>();

            // Create a fake 2 MB memory stream
            var fileSizeInBytes = fileSizeInMB * 1024 * 1024;

            fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(f => f.Length).Returns(fileSizeInBytes);
            fileMock.Setup(f => f.FileName).Returns("testfile.txt");
            fileMock.Setup(f => f.ContentType).Returns("text/plain");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>(async (stream, token) =>
                {
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(stream, 81920, token);
                });

            FormField testSubject = new FormField()
            {
                Type = "File",
                IsRequired = isRequired,
                AllowedFileSizeInMB = fileLimitInMB,
            };

            bool result = await _formValidationService!.ValidateFile(testSubject, fileMock.Object);
            
            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrWhiteSpace(testSubject.ValidationError));
        }

        [TestCase(5, 2, false)]
        [TestCase(20, 1, true)]
        public async Task ValidateUploadedFileSize_Invalid(int fileSizeInMB, int fileLimitInMB, bool isRequired = false)
        {
            var fileHeader = "EF BB BF" + " FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
            var sign = fileHeader.Split(' '); //"FF D8 FF DB" го правим в list с име sign и в случая с 4 елемента. FF D8 FF DB - това e hex формат
            var fileBytes = new byte[sign.Length];  // arr е временен byte масив в десетичен вид, който приема hex стойностите от sign
            for (int i = 0; i < fileBytes.Length; i++)
            {
                fileBytes[i] = (byte)Convert.ToInt32(sign[i], 16);
            }

            var memoryStream = new MemoryStream(fileBytes);
            var fileMock = new Mock<IFormFile>();

            // Create a fake 2 MB memory stream
            var fileSizeInBytes = fileSizeInMB * 1024 * 1024;

            fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(f => f.Length).Returns(fileSizeInBytes);
            fileMock.Setup(f => f.FileName).Returns("testfile.txt");
            fileMock.Setup(f => f.ContentType).Returns("text/plain");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns<Stream, CancellationToken>(async (stream, token) =>
                {
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(stream, 81920, token);
                });

            FormField testSubject = new FormField()
            {
                Type = "File",
                IsRequired = isRequired,
                AllowedFileSizeInMB = fileLimitInMB,
            };

            bool result = await _formValidationService!.ValidateFile(testSubject, fileMock.Object);
            
            Assert.IsFalse(result);
            Assert.AreEqual(string.Format(MessageConstant.Values.FileExceedsLimit, fileLimitInMB),
                testSubject.ValidationError);
        }

        [Test]
        [TestCase("1:3602021988")]//ЕГН
        [TestCase("1:8302218810")]//ЕГН
        [TestCase("1:1242057554 ")]//ЕГН
        [TestCase("2:7000900051")]//ЛНЧ
        [TestCase("2:1004227747")]//ЛНЧ
        [TestCase("2:1004227747", true)]//ЛНЧ
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
                    var mockResponse = CreateMockAreNomenclatureCodesAllowedResponse(request);

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
                    var mockResponse = CreateMockAreNomenclatureCodesAllowedResponse(request);

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

        [Test]
        [TestCase("1:177208082")]//ЕИК
        [TestCase("1:175210420")]//ЕИК
        [TestCase("1:1218173091476")]//ЕИК 13
        [TestCase("2:202846766")]//БУЛСТАТ
        [TestCase("2:000818022")]//БУЛСТАТ
        [TestCase("2:000818022", true)]//БУЛСТАТ
        [TestCase("2:1218173090381")]//БУЛСТАТ 13
        [TestCase(" 1 :  1218173090797 ")]//ЕИК
        [TestCase("", false)]
        [TestCase(" ", false)]
        [TestCase(null, false)]
        public async Task ValidateFieldCid_Valid(string? fieldValue, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.IsAny<AreNomenclatureCodesAllowedRequest>(), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockAreNomenclatureCodesAllowedResponse(request);

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
                Type = "CompanyIdentifier",
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
        [TestCase("1:177208083", true)]//ЕИК
        [TestCase("1::177208082", true)]
        [TestCase("1::177208082:true", true)]
        [TestCase("177208082")]
        [TestCase("1;177208082")]
        [TestCase("", true)]
        [TestCase(" ", true)]
        [TestCase(null, true)]
        [TestCase("1:1:177208082")]
        [TestCase("2:177208083", true)]//БУЛСТАТ
        [TestCase("2:1772080820", true)]//БУЛСТАТ
        [TestCase("1:1004227747")]
        [TestCase("2:1004227748")]
        [TestCase("2:ZXCVBNMAS")]
        [TestCase("1:36020219880")]
        [TestCase("1:360202198")]
        [TestCase("1:", true)]
        [TestCase("1::, true", true)]
        [TestCase(":177208082", true)]
        [TestCase("177208082")]
        [TestCase("99:177208082")]
        [TestCase("6:", true)]
        [TestCase("1:177208082:истина")]
        [TestCase("1:1218173090798")]
        public async Task ValidateFieldCid_Invalid(string? fieldValue, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.IsAny<AreNomenclatureCodesAllowedRequest>(), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockAreNomenclatureCodesAllowedResponse(request);

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
                Type = "CompanyIdentifier",
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

        [Test]
        [TestCase("1:123123123", "Компания", "", "", true)]//ЕИК
        public async Task ValidateCompany_Invalid(string cid, string name, string eikForm, string bulstatForm, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.IsAny<AreNomenclatureCodesAllowedRequest>(), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockAreNomenclatureCodesAllowedResponse(request);

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
                Type = SimpleFormFieldType.Company.ToString(),
                IsRequired = isRequired,
                Name = "comp",
                Fields = new List<FormField>()
                {
                    new FormField()
                    {
                        Name = "comp_companyNameImmutable",
                        Value = name,
                        Type = SimpleFormFieldType.Text.ToString()
                    },
                    new FormField()
                    {
                        Name = "comp_legalFormBulstatImmutable",
                        Value = bulstatForm,
                        Type = SimpleFormFieldType.Autocomplete.ToString()
                    },
                    new FormField()
                    {
                        Name = "comp_legalFormEIKImmutable",
                        Value = bulstatForm,
                        Type = SimpleFormFieldType.Select.ToString()
                    },
                    new FormField()
                    {
                        Name = "comp_companyNumberImmutable",
                        Value = bulstatForm,
                        Type = SimpleFormFieldType.CompanyIdentifier.ToString()
                    },
                }
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
        [TestCase("1:123123123", "Компания", "", "", false)]//ЕИК
        public async Task ValidateCompany_Valid(string cid, string name, string eikForm, string bulstatForm, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.IsAny<AreNomenclatureCodesAllowedRequest>(), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockAreNomenclatureCodesAllowedResponse(request);

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
                Type = SimpleFormFieldType.Company.ToString(),
                IsRequired = isRequired,
                Name = "comp",
                Fields = new List<FormField>()
                {
                    new FormField()
                    {
                        Name = "comp_companyNameImmutable",
                        Value = name,
                        Type = SimpleFormFieldType.Text.ToString()
                    },
                    new FormField()
                    {
                        Name = "comp_legalFormBulstatImmutable",
                        Value = bulstatForm,
                        Type = SimpleFormFieldType.Autocomplete.ToString()
                    },
                    new FormField()
                    {
                        Name = "comp_legalFormEIKImmutable",
                        Value = bulstatForm,
                        Type = SimpleFormFieldType.Select.ToString()
                    },
                    new FormField()
                    {
                        Name = "comp_companyNumberImmutable",
                        Value = bulstatForm,
                        Type = SimpleFormFieldType.CompanyIdentifier.ToString()
                    },
                }
            };

            var viewModel = new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            };
            bool result = await _formValidationService!.ValidateViewModel(viewModel, mockNomenclatureClient.Object, 0);

            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
            Assert.IsEmpty(await _formValidationService.GetValidatedFormFieldsErrors(viewModel));
        }

        //private static NomenclaturePublicResponse CreateMockPidNomenclaturePublicResponse()
        //{
        //    // Create the response object
        //    var nomenclatureType = new NomenclaturePublicResponse
        //    {
        //        ResultStatus = new ResultStatus { Code = ResultCodes.Ok },
        //        NomenclatureTypes = { new NomenclatureTypePublicResponse()
        //            {
        //                Type = NomenclatureTypes.PidType,
        //                Name = "Тип на идентификатор на физическо лице",
        //                CodeableConcepts =
        //                {
        //                    new CodeableConceptPublicResponse { Code = "1", Value = "ЕГН", ValueEn = "EGN" },
        //                    new CodeableConceptPublicResponse { Code = "2", Value = "ЛНЧ", ValueEn = "LNZ" },
        //                    new CodeableConceptPublicResponse { Code = "3", Value = "Паспорт № ", ValueEn = "PASS" },
        //                    new CodeableConceptPublicResponse { Code = "4", Value = "Друг", ValueEn = "Other" },
        //                    new CodeableConceptPublicResponse { Code = "5", Value = "ЕИК", ValueEn = "NBN" }
        //                }
        //            }
        //        }
        //    };
        //    return nomenclatureType;
        //}

        private static AreNomenclatureCodesAllowedResponse CreateMockAreNomenclatureCodesAllowedResponse(AreNomenclatureCodesAllowedRequest request)
        {
            // Create the response object
            var result = new AreNomenclatureCodesAllowedResponse
            {
                ResultStatus = new ResultStatus { Code = ResultCodes.Ok },
                AreAllowed = int.TryParse(request.NomenclatureCodes.First(), out int parsedInt) && parsedInt >= 1 && parsedInt <= 6
            };
            return result;
        }

        private static AreNomenclatureCodesAllowedResponse CreateMockCountryAreNomenclatureCodesAllowedResponse(AreNomenclatureCodesAllowedRequest request)
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
        [TestCase(null, "", false)]
        [TestCase(null, " ", false)]
        [TestCase(null, null, false)]
        [TestCase(null, "19.12.2024", false)]
        [TestCase(null, "19.12.2024", true)]
        [TestCase(null, "19.12.2023", true, true, true)]
        [TestCase(null, "19.12.2023", true, true, false)]
        [TestCase(null, "19.12.2123", true, false, true)]
        [TestCase(null, null, false, false, false)]

        [TestCase("13.06.2025",  "", false)]
        [TestCase("13.06.2025", " ", false)]
        [TestCase("13.06.2025", null, false)]
        [TestCase("13.06.2025", "19.12.2024", false)]
        [TestCase("13.06.2025", "19.12.2024", true)]
        [TestCase("13.06.2025", "19.12.2023", true, true, true)]
        [TestCase("13.06.2025", "19.12.2023", true, true, false)]
        [TestCase("13.06.2025", "19.12.2123", true, false, true)]
        [TestCase("13.06.2025", "13.06.2025", true)]
        [TestCase("13.06.2025", "13.06.2025", true, false, false)]
        //[TestCase(" 19.12.2024 ", false)]
        public async Task ValidateFieldDate_Valid(string? processCreationDateString, string? fieldValue, bool isRequired = false, bool allowPastDates = true, bool allowFutureDates = true)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            DateTime? processCreationDate = null;
            if (processCreationDateString != null)
            {
                processCreationDate = DateTime.ParseExact(processCreationDateString,
                    FormattingConstant.NormalDateFormat, CultureInfo.InvariantCulture);
            }

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
            }, mockNomenclatureClient.Object, 0, processCreationDate);

            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase(null, "", true)]
        [TestCase(null, " ", true)]
        [TestCase(null, null, true)]
        [TestCase(null, "19.12.2024 г.", false)]
        [TestCase(null, "19.12.2024 г. 8:52:57", false)]
        [TestCase(null, "19.12.2024 г. 8:52", false)]
        [TestCase(null, "19.12.2024 г. 08:52:57", false)]
        [TestCase(null, "19.12.2024 г. 08:52", false)]
        [TestCase(null, "19.12.2024 08:52:57", false)]
        [TestCase(null, " 19.12.2024 ", false)]
        [TestCase(null, "12/19/2024 08:52:57", false)]
        [TestCase(null, "12/19/2024 8:52:57", false)]
        [TestCase(null, "12/19/2024 08:52", false)]
        [TestCase(null, "12/19/2024 8:52", false)]
        [TestCase(null, "12/19/2024", false)]
        [TestCase(null, "01/01/2024", false)]
        [TestCase(null, "19.12.2023", true, false, false)]
        [TestCase(null, "19.12.2023", true, false, true)]
        [TestCase(null, "19.12.2123", true, true, false)]

        [TestCase("20.12.2023", "19.12.2023", true, false, false)]
        [TestCase("20.12.2023", "19.12.2023", true, false, true)]
        [TestCase("20.12.2123", "19.12.2123", true, false, true)]
        public async Task ValidateFieldDate_Invalid(string? processCreationDateString, string? fieldValue, bool isRequired = false, bool allowPastDates = true, bool allowFutureDates = true)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            DateTime? processCreationDate = null;

            if (processCreationDateString != null)
            {
                processCreationDate = DateTime.ParseExact(processCreationDateString,
                    FormattingConstant.NormalDateFormat, CultureInfo.InvariantCulture);
            }

            FormField testSubject = new FormField()
            {
                Type = SimpleFormFieldType.Date.ToString(),
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
            }, mockNomenclatureClient.Object, 0, processCreationDate);

            Assert.IsFalse(result);
            Assert.IsFalse(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase(120, true, false)]
        [TestCase(120, false, true)]
        [TestCase(-120, true, false)]
        [TestCase(-120, false, true)]
        public async Task ValidateFieldDateTimePastFuture_Valid(int userMinutesUtcOffset, bool allowPastDates = true, bool allowFutureDates = true)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            var utcNow = DateTime.UtcNow;
            string fieldValue = string.Empty;

            if (allowPastDates)
            {
                fieldValue = utcNow.AddMinutes(-1).AddMinutes(-userMinutesUtcOffset).ToString(FormattingConstant.DateTimeFormat);
            }
            else if (allowFutureDates)
            {
                fieldValue = utcNow.AddMinutes(10).AddMinutes(-userMinutesUtcOffset).ToString(FormattingConstant.DateTimeFormat);
            }

            FormField testSubject = new FormField()
            {
                Type = SimpleFormFieldType.DateTime.ToString(),
                Value = fieldValue,
                AllowPastDates = allowPastDates,
                AllowFutureDates = allowFutureDates
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject,
                },
                UserTimeZoneOffsetInMinutes = userMinutesUtcOffset
            }, mockNomenclatureClient.Object, 0);

            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase(120, true, false)]
        [TestCase(120, false, true)]
        [TestCase(-120, true, false)]
        [TestCase(-120, false, true)]
        public async Task ValidateFieldDatePastFuture_Valid(int userMinutesUtcOffset, bool allowPastDates = true, bool allowFutureDates = true)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            var utcNow = DateTime.UtcNow;
            string fieldValue = string.Empty;

            if (allowPastDates)
            {
                fieldValue = utcNow.AddMinutes(-1).AddMinutes(-userMinutesUtcOffset).Date.ToString("dd.MM.yyyy");
            }
            else if (allowFutureDates)
            {
                fieldValue = utcNow.AddMinutes(10).AddMinutes(-userMinutesUtcOffset).Date.ToString("dd.MM.yyyy");
            }

            FormField testSubject = new FormField()
            {
                Type = SimpleFormFieldType.Date.ToString(),
                Value = fieldValue,
                AllowPastDates = allowPastDates,
                AllowFutureDates = allowFutureDates
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject,
                },
                UserTimeZoneOffsetInMinutes = userMinutesUtcOffset
            }, mockNomenclatureClient.Object, 0);

            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase(null, "", false)]
        [TestCase(null, " ", false)]
        [TestCase(null, null, false)]
        [TestCase(null, "19.12.2024 08:52", false)]
        [TestCase(null, "19.12.2024 08:52", true)]
        [TestCase(null, "19.12.2023 08:52", true, true, true)]
        [TestCase(null, "19.12.2023 08:52", true, true, false)]
        [TestCase(null, "19.12.2123 08:52", true, false, true)]

        [TestCase("19.12.2024 08:52", "", false)]
        [TestCase("19.12.2024 08:52", " ", false)]
        [TestCase("19.12.2024 08:52", null, false)]
        [TestCase("19.12.2024 08:52", "19.12.2024 08:52", false)]
        [TestCase("19.12.2024 08:52", "19.12.2024 08:52", true)]
        [TestCase("19.12.2014 08:52", "19.12.2023 08:52", true, true, true)]
        [TestCase("19.12.2014 08:52", "19.12.2023 08:52", true, true, true)]
        [TestCase("19.12.2123 08:52", "20.12.2123 08:52", true, false, true)]
        [TestCase("19.12.2123 08:52", "19.12.2123 08:52", true, false, false)]
        [TestCase("19.12.2123 08:52", "19.12.2123 08:52", true)]
        //[TestCase(" 19.12.2024 ", false)]
        public async Task ValidateFieldDateTime_Valid(string? processCreationDateString, string? fieldValue, bool isRequired = false, bool allowPastDates = true, bool allowFutureDates = true)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            DateTime? processCreationDate = null;

            if (processCreationDateString != null)
            {
                processCreationDate = DateTime.ParseExact(processCreationDateString,
                    FormattingConstant.DateTimeFormat, CultureInfo.InvariantCulture);
            }

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
            }, mockNomenclatureClient.Object, 0, processCreationDate);

            Assert.IsTrue(result);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase(null, "", true)]
        [TestCase(null, " ", true)]
        [TestCase(null, null, true)]
        [TestCase(null, "19.12.2024 г.", false)]
        [TestCase(null, "19.12.2024 г. 8:52:57", false)]
        [TestCase(null, "19.12.2024 г. 8:52", false)]
        [TestCase(null, "19.12.2024 г. 08:52:57", false)]
        [TestCase(null, "19.12.2024 г. 08:52", false)]
        [TestCase(null, "19.12.2024 08:52:57", false)]
        [TestCase(null, " 19.12.2024 ", false)]
        [TestCase(null, "12/19/2024 08:52:57", false)]
        [TestCase(null, "12/19/2024 8:52:57", false)]
        [TestCase(null, "12/19/2024 08:52", false)]
        [TestCase(null, "12/19/2024 8:52", false)]
        [TestCase(null, "12/19/2024 08:52", false)]
        [TestCase(null, "01/01/2024 08:52", false)]
        [TestCase(null, "19.12.2023 08:52", true, false, false)]
        [TestCase(null, "19.12.2023 08:52", true, false, true)]
        [TestCase(null, "19.12.2123 08:52", true, true, false)]

        [TestCase("19.12.2023 08:53", "19.12.2023 08:52", true, false, false)]
        [TestCase("19.12.2023 08:53", "19.12.2023 08:52", true, false, true)]
        [TestCase("19.12.2123 08:53", "19.12.2123 08:52", true, false, true)]
        public async Task ValidateFieldDateTime_Invalid(string? processCreationDateString, string? fieldValue, bool isRequired = false, bool allowPastDates = true, bool allowFutureDates = true)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            DateTime? processCreationDate = null;

            if (processCreationDateString != null)
            {
                processCreationDate = DateTime.ParseExact(processCreationDateString,
                    FormattingConstant.DateTimeFormat, CultureInfo.InvariantCulture);
            }

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
            }, mockNomenclatureClient.Object, 0, processCreationDate);

            Assert.IsFalse(result);
            Assert.IsFalse(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        [TestCase("", false)]
        [TestCase(" ", false)]
        [TestCase(null, false)]
        [TestCase("2025-04-01T00:00:00+03:00", false, "01.04.2025")]
        [TestCase("2025-04-01T00:00:00+03:00", true, "01.04.2025")]
        //[TestCase(" 19.12.2024 ", false)]
        public async Task ValidateFieldDateEFormImport_Valid(string? fieldValue, bool isRequired = false, string reformatedDate = "")
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "Date",
                Value = fieldValue,
                IsRequired = isRequired,
                AllowFutureDates = true,
                AllowPastDates = true
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

            if (!string.IsNullOrWhiteSpace(reformatedDate))
            {
                Assert.AreEqual(reformatedDate, testSubject.Value);
            }
        }

        [Test]
        [TestCase("", true)]
        [TestCase(" ", true)]
        [TestCase(null, true)]
        [TestCase("19.12.2024 г.", false)]
        [TestCase("2025-04-01T00:00:00+43:00", false)]
        [TestCase("2025-04-41T00:00:00+03:00", false)]
        public async Task ValidateFieldDateEFormImport_Invalid(string? fieldValue, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "Date",
                Value = fieldValue,
                IsRequired = isRequired,
                AllowFutureDates = true,
                AllowPastDates = true
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
        [TestCase("2025-04-01T00:00:00+03:00", false, "01.04.2025 00:00")]
        [TestCase("2025-04-01T01:00:00+03:00", true, "01.04.2025 01:00")]
        [TestCase("2025-04-01T23:01:00+03:00", true, "01.04.2025 23:01")]
        //[TestCase("2025-04-14T12:41:43.063Z", true, "01.04.2025 23:01")]
        //[TestCase(" 19.12.2024 ", false)]
        public async Task ValidateFieldDateTimeEFormImport_Valid(string? fieldValue, bool isRequired = false, string reformatedDate = "")
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "DateTime",
                Value = fieldValue,
                IsRequired = isRequired,
                AllowFutureDates = true,
                AllowPastDates = true
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

            if (!string.IsNullOrWhiteSpace(reformatedDate))
            {
                Assert.AreEqual(reformatedDate, testSubject.Value);
            }
        }

        [Test]
        [TestCase("", true)]
        [TestCase(" ", true)]
        [TestCase(null, true)]
        [TestCase("19.12.2024 г.", false)]
        [TestCase("2025-04-01T00:00:00+43:00", false)]
        [TestCase("2025-04-41T00:00:00+03:00", false)]
        public async Task ValidateFieldDateTimeEFormImport_Invalid(string? fieldValue, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "DateTime",
                Value = fieldValue,
                IsRequired = isRequired,
                AllowFutureDates = true,
                AllowPastDates = true
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
                    var mockResponse = CreateMockCountryAreNomenclatureCodesAllowedResponse(request);

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
                    var mockResponse = CreateMockCountryAreNomenclatureCodesAllowedResponse(request);

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

        [Test]
        [TestCase("BG", "PER", "", true)]
        [TestCase("FIN", "", "Kotka", true)]
        [TestCase("BG", "", "", false)]
        [TestCase("FIN", "", "", false)]
        public async Task ValidateFieldAddress_Valid(string countryCode, string cityBg, string addressAbroad, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.Is<AreNomenclatureCodesAllowedRequest>(req => req.NomenclatureType == "EK009"), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockCountryAreNomenclatureCodesAllowedResponse(request);

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
                Type = "Address",
                IsRequired = isRequired,
                Name = "address"
            };

            testSubject.Fields.Add(new FormField()
            {
                Type = SimpleFormFieldType.Autocomplete.ToString(),
                Name = "address_countryImmutable",
                Value = countryCode,
                IsRequired = false,
                NomenclatureType = "EK009"
            });

            testSubject.Fields.Add(new FormField()
            {
                //Type = SimpleFormFieldType.City.ToString(),
                Type = SimpleFormFieldType.Text.ToString(),
                Name = "settlementImmutable",
                Value = cityBg,
            });

            if (!string.IsNullOrWhiteSpace(addressAbroad))
            {
                testSubject.Fields.Add(new FormField()
                {
                    Type = SimpleFormFieldType.TextArea.ToString(),
                    Name = "addressAbroadImmutable",
                    Value = addressAbroad
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
        [TestCase("BG", "", "PER", true)]
        [TestCase("FIN", "Kotka", "", true)]
        [TestCase("BG", "", "", true)]
        [TestCase("FIN", "", "", true)]
        public async Task ValidateFieldAddress_Invalid(string countryCode, string cityBg, string addressAbroad, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.Is<AreNomenclatureCodesAllowedRequest>(req => req.NomenclatureType == "EK009"), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockCountryAreNomenclatureCodesAllowedResponse(request);

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
                Type = "Address",
                IsRequired = isRequired,
                Name = "address"
            };

            testSubject.Fields.Add(new FormField()
            {
                Type = SimpleFormFieldType.Autocomplete.ToString(),
                Name = "address_countryImmutable",
                Value = countryCode,
                IsRequired = false,
                NomenclatureType = "EK009"
            });

            testSubject.Fields.Add(new FormField()
            {
                //Type = SimpleFormFieldType.City.ToString(),
                Type = SimpleFormFieldType.Text.ToString(),
                Name = "settlementImmutable",
                Value = cityBg,
            });

            testSubject.Fields.Add(new FormField()
            {
                Type = SimpleFormFieldType.TextArea.ToString(),
                Name = "addressAbroadImmutable",
                Value = addressAbroad
            });
            
            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsFalse(result);
            if (countryCode == "BG")
            {
                Assert.AreEqual(MessageConstant.FieldIsRequired, 
                    testSubject.Fields.Single(f => f.Name == "settlementImmutable").ValidationError);
            }
            else
            {
                Assert.AreEqual(MessageConstant.FieldIsRequired,
                    testSubject.Fields.Single(f => f.Name == "addressAbroadImmutable").ValidationError);
            }
        }

        [Test]
        [TestCase("BG", "PER", "", true)]
        [TestCase("FIN", "", "Kotka", true)]
        public async Task ValidateFieldAddressStructure_Invalid(string countryCode, string cityBg, string addressAbroad, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();
            mockNomenclatureClient
                .Setup(m => m.AreNomenclatureCodesAllowedAsync(
                    It.Is<AreNomenclatureCodesAllowedRequest>(req => req.NomenclatureType == "EK009"), null, null, CancellationToken.None))
                .Returns((AreNomenclatureCodesAllowedRequest request, Metadata headers, DateTime? deadline, CancellationToken cancellationToken) =>
                {
                    var mockResponse = CreateMockCountryAreNomenclatureCodesAllowedResponse(request);

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
                Type = "Address",
                IsRequired = isRequired,
                Name = "address"
            };

            testSubject.Fields.Add(new FormField()
            {
                Type = SimpleFormFieldType.TextArea.ToString(),
                Name = "addressAbroadImmutable",
                Value = addressAbroad
            });

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsFalse(result);
            Assert.AreEqual(MessageConstant.InvalidFieldConfig,
                testSubject.ValidationError);
        }


        [Test]
        public async Task RepeatedFieldsAreAlsoValidated()
        {
            var viewModelWithRepetitions = new FormViewModel{FormFields = new List<FormField>()};

            viewModelWithRepetitions.FormFields.Add(new FormField()
            {
                Type = "Text",
                IsRequired = true,
                Value = null,
                Repetitions = new List<FormField>()
                {
                    new FormField()
                    {
                        Type = "Text",
                        IsRequired = true,
                        Value = null
                    }
                }
            });

            bool result = await _formValidationService!.ValidateViewModel(viewModelWithRepetitions, null, 1);

            Assert.IsFalse(result);
            Assert.IsNotNull(viewModelWithRepetitions.FormFields.First().ValidationError);
            Assert.IsNotEmpty(viewModelWithRepetitions.FormFields.First().ValidationError);
            Assert.IsNotNull(viewModelWithRepetitions.FormFields.First().Repetitions.First().ValidationError);
            Assert.IsNotEmpty(viewModelWithRepetitions.FormFields.First().Repetitions.First().ValidationError);
        }

        [Test]
        public async Task ComplexFieldChildrenAreAlsoValidated()
        {
            var viewModelWithRepetitions = new FormViewModel { FormFields = new List<FormField>() };

            viewModelWithRepetitions.FormFields.Add(new FormField()
            {
                Type = "ComplexType",
                IsRequired = true,
                Value = null,
                Fields = new List<FormField>()
                {
                    new FormField()
                    {
                        Type = "Text",
                        IsRequired = true,
                        Value = null
                    }
                }
            });

            bool result = await _formValidationService!.ValidateViewModel(viewModelWithRepetitions, null, 1);

            Assert.IsFalse(result);
            Assert.IsNotNull(viewModelWithRepetitions.FormFields.First().Fields.First().ValidationError);
            Assert.IsNotEmpty(viewModelWithRepetitions.FormFields.First().Fields.First().ValidationError);
        }

        [Test]
        public async Task ValidateEmptyFileErrorMessage()
        {
            var fileHeader = "EF BB BF" + " FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF FF";
            var sign = fileHeader.Split(' '); //"FF D8 FF DB" го правим в list с име sign и в случая с 4 елемента. FF D8 FF DB - това e hex формат
            var fileBytes = new byte[sign.Length];  // arr е временен byte масив в десетичен вид, който приема hex стойностите от sign
            for (int i = 0; i < fileBytes.Length; i++)
            {
                fileBytes[i] = (byte)Convert.ToInt32(sign[i], 16);
            }

            var memoryStream = new MemoryStream(fileBytes);
            var fileMock = new Mock<IFormFile>();


            fileMock.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            fileMock.Setup(f => f.Length).Returns(0);
            fileMock.Setup(f => f.FileName).Returns("testfile.txt");
            fileMock.Setup(f => f.ContentType).Returns("text/plain");

            FormField testSubject = new FormField()
            {
                Type = "File",
                IsRequired = false,
                AllowedFileSizeInMB = 2,
            };

            bool result = await _formValidationService!.ValidateFile(testSubject, fileMock.Object);

            Assert.IsFalse(result);
            Assert.AreEqual(MessageConstant.Values.FileIsEmpty, testSubject.ValidationError);
        }

        [Test]
        [TestCase("", false)]
        [TestCase(" ", false)]
        [TestCase(null, false)]
        [TestCase("08:52", false)]
        [TestCase("08:52", true)]      
        [TestCase("18:52", true)]      
        [TestCase("00:00", true)]      
        public async Task ValidateFieldTime_Valid(string? fieldValue, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "Time",
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
        [TestCase("", true)]
        [TestCase(" ", true)]
        [TestCase(null, true)]
        [TestCase("8:52:00", false)]
        [TestCase("0:52", false)]
        [TestCase("8/52/00", false)]
        [TestCase("0/52", false)]
        [TestCase("8:52", false)]
        [TestCase("38:52", false)]
        [TestCase("18:62", false)]
        [TestCase("19.12.2023", false)]
        public async Task ValidateFieldTime_Invalid(string? fieldValue, bool isRequired = false)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = "Time",
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

        [Test]
        public async Task ValidateComplexRequiredFieldChildrenAreValidatedForIsRequired_Valid()
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = SimpleFormFieldType.Text.ToString(),
                IsRequired = true,
                Fields = new List<FormField>()
                {
                    new FormField
                    {
                        Type = SimpleFormFieldType.Text.ToString(),
                        IsRequired = true,
                        Value = "Text"
                    }
                }
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
        public async Task ValidateComplexRequiredFieldChildrenAreNonRequired_Valid()
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = SimpleFormFieldType.Text.ToString(),
                IsRequired = true,
                Fields = new List<FormField>()
                {
                    new FormField
                    {
                        Type = SimpleFormFieldType.Text.ToString(),
                        IsRequired = false,
                    }
                }
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsTrue(true);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
        }

        [Test]
        public async Task ValidateComplexRequiredFieldChildrenAreValidatedForIsRequired_Invalid()
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = SimpleFormFieldType.Text.ToString(),
                IsRequired = true,
                Fields = new List<FormField>()
                {
                    new FormField
                    {
                        Type = SimpleFormFieldType.Text.ToString(),
                        IsRequired = true
                    }
                }
            };

            bool result = await _formValidationService!.ValidateViewModel(new FormViewModel()
            {
                FormFields = new List<FormField>()
                {
                    testSubject
                }
            }, mockNomenclatureClient.Object, 0);

            Assert.IsFalse(result);
            Assert.IsTrue(string.IsNullOrEmpty(testSubject.ValidationError));
            Assert.IsFalse(string.IsNullOrEmpty(testSubject.Fields.First().ValidationError));
            Assert.AreEqual(MessageConstant.FieldIsRequiredNoParam, testSubject.Fields.First().ValidationError);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public async Task ValidateComplexNonRequiredFieldChildrenAreValidatedForIsRequired_Valid(bool isChildRequired)
        {
            var mockNomenclatureClient = new Mock<NomenclatureGrpc.NomenclatureGrpcClient>();

            FormField testSubject = new FormField()
            {
                Type = SimpleFormFieldType.Text.ToString(),
                IsRequired = false,
                Fields = new List<FormField>()
                {
                    new FormField
                    {
                        Type = SimpleFormFieldType.Text.ToString(),
                        IsRequired = isChildRequired
                    }
                }
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
    }
}
