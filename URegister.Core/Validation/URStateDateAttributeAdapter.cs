// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace URegister.Core.Validation
{
    public class URStateDateAttributeAdapter : URAttributeAdapterBase<URStateDateAttribute>
    {
        public URStateDateAttributeAdapter(
            URStateDateAttribute attribute, IStringLocalizer? stringLocalizer)
            : base(attribute, stringLocalizer)
        {

        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-ur-state-date", GetErrorMessage(context));
        }
    }
}
