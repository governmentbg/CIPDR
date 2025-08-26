// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using URegister.Core.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using URegister.Infrastructure.Constants;

namespace URegister.Core.Validation
{
    public class UrEikAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance == null)
                return new ValidationResult(ErrorMessage);

            if (PidValidateService.ValidateCompanyId((string)value, (int)CidTypes.EIK))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult(ErrorMessage);
        }
    }
}
