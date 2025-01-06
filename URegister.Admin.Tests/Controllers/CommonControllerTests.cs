using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using Microsoft.AspNetCore.Mvc;
using URegister.Admin.Controllers;

namespace URegister.Admin.Tests.Controllers
{
    [TestFixture]
    public class CommonControllerTests
    {
        [Test]
        public void AllHttpPostAndDeleteMethodsHaveValidateAntiForgeryToken()
        {
            // Get all controller types in the assembly
            var controllerTypes = Assembly.GetAssembly(typeof(UserController))
                .GetTypes()
                .Where(type => typeof(Controller).IsAssignableFrom(type));

            foreach (var controllerType in controllerTypes)
            {
                // Get all methods with the [HttpPost] attribute
                var httpPostMethods = controllerType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                    .Where(method => method.GetCustomAttributes(typeof(HttpPostAttribute), false).Any());

                // Get all methods with the [HttpDelete] attribute
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
