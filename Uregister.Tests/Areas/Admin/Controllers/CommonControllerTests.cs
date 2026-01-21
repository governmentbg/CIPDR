using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using URegister.Areas.Admin.Controllers;
using URegister.Areas.Public.Controllers;
using AccountController = URegister.Areas.Admin.Controllers.AccountController;

namespace URegister.Tests.Areas.Admin.Controllers
{
    [TestFixture]
    public class CommonControllerTests
    {
        [Test]
        public void AllHttpPostAndDeleteMethodsHaveValidateAntiForgeryToken()
        {
            // Get all controller types in the assembly
            var controllerTypes = Assembly.GetAssembly(typeof(ServiceController))
                .GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type) && type.Name != nameof(OldDataController));

            foreach (var controllerType in controllerTypes)
            {
                // Get all methods with the [HttpPost] attribute
                var httpPostMethods = controllerType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(method => method.GetCustomAttributes(typeof(HttpPostAttribute), false).Any());

                var httpDeleteMethods = controllerType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(method => method.GetCustomAttributes(typeof(HttpDeleteAttribute), false).Any());

                foreach (var method in httpPostMethods.Concat(httpDeleteMethods))
                {
                    // Check for exceptions
                    if (method.Name is
                        nameof(AccountController.ExternalLogin) or
                        nameof(URegister.Areas.Public.Controllers.ImportController.ImportApplication) or
                        nameof(URegister.Areas.Public.Controllers.ImportController.ImportJson) or 
                        //nameof(OldDataController.ImportExcelFileForR00001) or 
                        nameof(URegister.Areas.Public.Controllers.ImportController.ImportEDeliveryFile) or
                        nameof(URegister.Controllers.FilesController.PreparePdfForSignature))
                    {
                        continue;
                    }

                    // Skip methods where the first parameter is of type IDataTablesRequest
                    var parameters = method.GetParameters();
                    if (parameters.Length > 0 && parameters[0].ParameterType == typeof(IDataTablesRequest))
                    {
                        continue; // Skip methods with the first parameter of type IDataTablesRequest
                    }

                    // Check if the method has the [ValidateAntiForgeryToken] attribute
                    var hasAntiForgeryToken =
                        method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false).Any();

                    Assert.That(hasAntiForgeryToken,
                        $"Method '{method.Name}' in controller '{controllerType.Name}' does not have the [ValidateAntiForgeryToken] attribute.");
                }
            }
        }

        [Test]
        public void AllActionsHaveDisplayAttributeTest()
        {
            // Get all controller types in the assembly
            var controllerTypes = Assembly.GetAssembly(typeof(ServiceController))
                .GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type));

            StringBuilder problems = new StringBuilder();

            foreach (var controllerType in controllerTypes)
            {
                var allMethods =
                    controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);

                foreach (var method in allMethods)
                {
                    // Check for exceptions
                    //if (method.Name is
                    //    nameof(AccountController.ExternalLogin) or
                    //    nameof(ImportController.ImportApplication))
                    //{
                    //    continue;
                    //}

                    // Check if the method has the [DisplayAttribute] attribute
                    var hasDisplayAttribute = method.GetCustomAttributes(typeof(DisplayAttribute), false).Any();

                    if (!hasDisplayAttribute)
                    {

                        problems.Append(
                            $"Method '{method.Name}' in controller '{controllerType.Name}' does not have the [Display] attribute.");

                        problems.Append(Environment.NewLine);
                    }
                }
            }

            string report = problems.ToString();
            Assert.That(string.IsNullOrEmpty(report), report);
        }
    }
}
