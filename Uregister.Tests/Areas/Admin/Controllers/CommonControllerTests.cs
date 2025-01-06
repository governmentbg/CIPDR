using System.Reflection;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using URegister.Areas.Admin.Controllers;

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
                .Where(type => typeof(Controller).IsAssignableFrom(type));

            foreach (var controllerType in controllerTypes)
            {
                // Get all methods with the [HttpPost] attribute
                var httpPostMethods = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(method => method.GetCustomAttributes(typeof(HttpPostAttribute), false).Any());

                var httpDeleteMethods = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(method => method.GetCustomAttributes(typeof(HttpDeleteAttribute), false).Any());

                foreach (var method in httpPostMethods.Concat(httpDeleteMethods))
                {
                    // Check for exceptions
                    //if (method.Name == nameof(AccountController.ExternalLogin))
                    //{
                    //    continue;
                    //}

                    // Skip methods where the first parameter is of type IDataTablesRequest
                    var parameters = method.GetParameters();
                    if (parameters.Length > 0 && parameters[0].ParameterType == typeof(IDataTablesRequest))
                    {
                        continue; // Skip methods with the first parameter of type IDataTablesRequest
                    }

                    // Check if the method has the [ValidateAntiForgeryToken] attribute
                    var hasAntiForgeryToken = method.GetCustomAttributes(typeof(ValidateAntiForgeryTokenAttribute), false).Any();

                    Assert.That(hasAntiForgeryToken,
                        $"Method '{method.Name}' in controller '{controllerType.Name}' does not have the [ValidateAntiForgeryToken] attribute.");
                }
            }
        }
    }
}
