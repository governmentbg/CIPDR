using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using URegister.Core.Services;
using URegister.Infrastructure.Model.RegisterForms;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace URegister.Core.Tests.Services
{
    public class FormConfigurationPersistenceServiceTests
    {
        [Test]
        [TestCase("one,two,three", "one,two,three", "one,two,three")]
        [TestCase("one,two,three", "three,two,one", "one,two,three")]
        [TestCase("one,two,three", "a,b,c", "a,b,c")]
        [TestCase("one,two,three", "one,b,three", "one,three,b")]
        [TestCase("one,two,twoAndAHalf,three", "three,two,one", "one,two,three")]
        [TestCase("one,two,three", "three,two,twoAndAHalf,one", "one,two,three,twoAndAHalf")]
        [TestCase("one,two,three", "one,three", "one,three")]
        [TestCase("one,two,three", "three,one", "one,three")]
        [TestCase("one,three", "three,two,one", "one,three,two")]
        [TestCase("one,two,three", "", "")]
        [TestCase("", "one,two,three", "one,two,three")]
        public void ArrangeRepeatingFieldsSubfieldsInTheCorrectOrderTest(string originalOrderOfFields, string clonedOrderOfFields,
            string expectedOrderAfterSort)
        {
            FormViewModel viewModel = new FormViewModel()
            {
                FormFields = new List<FormField>()
            };

            FormField original = new FormField()
            {
                Name = "Original",
                Label = "Оригинал",
                Fields = originalOrderOfFields.Split(',').Select(f => new FormField() { Label = f, Name = f }).ToList(),
                Repetitions = new List<FormField>()
                {
                    new FormField()
                    {
                        Name = "Clone",
                        Label = "Клонинг",
                        Fields = clonedOrderOfFields.Split(',').Select(f => new FormField() { Label = f, Name = f })
                            .ToList(),
                    }
                }
            };

            viewModel.FormFields.Add(original);

            //FormConfigurationPersistenceService.ArrangeRepeatingFieldsSubfieldsInTheCorrectOrder(viewModel);

            Type type = typeof(FormConfigurationPersistenceService);

            // Get the MethodInfo for the private static method
            MethodInfo methodInfo = type.GetMethod("ArrangeRepeatingFieldsSubfieldsInTheCorrectOrder", BindingFlags.NonPublic | BindingFlags.Static);

            if (methodInfo != null)
            {
                // Invoke the private static method
                methodInfo.Invoke(null, new[] { viewModel}); // Pass null for instance (static method) and parameters
            }

            var cloneFieldsAfterSort = viewModel.FormFields.First().Repetitions.First().Fields.Select(f => f.Label);
            var stringAfterSort = string.Join(',', cloneFieldsAfterSort);

            Assert.AreEqual(expectedOrderAfterSort, stringAfterSort);
        }
    }
}
