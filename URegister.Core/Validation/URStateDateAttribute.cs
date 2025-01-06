// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using URegister.Core.Models.Nomenclature;

namespace URegister.Core.Validation
{
    public class URStateDateAttribute : ValidationAttribute
    {
         protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance == null)
                return new ValidationResult(ErrorMessage);
            var codeableConcept = validationContext.ObjectInstance as CodeableConceptVM;
            if (codeableConcept == null)
                return new ValidationResult(ErrorMessage);
            if (codeableConcept.DateFrom < codeableConcept.DateFromInit)
                return new ValidationResult(ErrorMessage);
            if (codeableConcept.DateTo != null && codeableConcept.DateFrom < codeableConcept.DateTo)
                return new ValidationResult(ErrorMessage);
            return ValidationResult.Success;
        }
    }
}
