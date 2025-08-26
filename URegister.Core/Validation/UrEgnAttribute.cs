// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using URegister.Core.Services;
using System.ComponentModel.DataAnnotations;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Validation
{
    public class UrEgnAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance == null)
                return new ValidationResult(ErrorMessage);

            if (PidValidateService.ValidatePersonalId((string)value, (int)PidTypes.EGN))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}