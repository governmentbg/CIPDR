using System.Reflection;

namespace URegister.Infrastructure.Model.RegisterForms
{
    public static class FormFieldExtensions
    {
        /// <summary>
        /// Прави копие на полето за повтарящ се елемент
        /// </summary>
        /// <param name="original"></param>
        /// <param name="pairKey"></param>
        /// <param name="assignedValue"></param>
        /// <returns></returns>
        public static FormField CreateRepeaterClone(this FormField original, string name, string? assignedValue = null)
        {
            if (original == null)
            {
                return null;
            }

            return new FormField
            {
                Identifier = new Guid(), //
                FieldId = original.FieldId,
                Type = original.Type,
                Name = name, //
                Label = original.Label,
                Placeholder = original.Placeholder,
                Value =  assignedValue,// original.Value,
                Columns = original.Columns,
                ClassName = original.ClassName,
                Pattern = original.Pattern,
                ValidationMessage = original.ValidationMessage,
                IsRequired = original.IsRequired,
                IsPublic = original.IsPublic,
                IsReadonly = original.IsReadonly,
                Tooltip = original.Tooltip,
                NomenclatureType = original.NomenclatureType,
                Fields = new List<FormField>(),// original.Fields?.Select(f => f.CreateRepeaterClone(f.Name)).ToArray(),
                MaxValue = original.MaxValue,
                MinValue = original.MinValue,
                AllowPastDates = original.AllowPastDates,
                AllowFutureDates = original.AllowFutureDates,
                NumberOfDigitsAfterDelimiter = original.NumberOfDigitsAfterDelimiter,
                IsHidden = original.IsHidden,
                ValidationError = original.ValidationError,
                AllowedFileSizeInMB = original.AllowedFileSizeInMB,
                AllowedFileExtensions = original.AllowedFileExtensions?.ToArray(), // Clone string array
                //File = original.File, // Shallow copy; IFormFile is likely reference type managed outside this clone
                CanBeRepeated = false, //original.CanBeRepeated,
                Repetitions =
                    new List<FormField>() //original.Repetitions?.Select(r => r.Clone()).ToList() // Recursively clone nested list
            };
        }
    }
}
