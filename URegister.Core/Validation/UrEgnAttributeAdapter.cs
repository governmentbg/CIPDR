// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace URegister.Core.Validation
{
    public class UrEgnAttributeAdapter : URAttributeAdapterBase<UrEgnAttribute>
    {
        public UrEgnAttributeAdapter(
            UrEgnAttribute attribute, IStringLocalizer? stringLocalizer)
            : base(attribute, stringLocalizer)
        {

        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-ur-egn", GetErrorMessage(context));
        }
    }
}