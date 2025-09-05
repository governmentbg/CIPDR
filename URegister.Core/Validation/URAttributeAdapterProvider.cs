using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace URegister.Core.Validation;

// <snippet_Class>
public class URAttributeAdapterProvider : IValidationAttributeAdapterProvider
{
    private readonly IValidationAttributeAdapterProvider baseProvider =
        new ValidationAttributeAdapterProvider();

    public IAttributeAdapter? GetAttributeAdapter(
        ValidationAttribute attribute, IStringLocalizer? stringLocalizer)
    {
        if (attribute is URStateDateAttribute urStateDateAttribute)
        {
            return new URStateDateAttributeAdapter(urStateDateAttribute, stringLocalizer);
        }
        if (attribute is UrEikAttribute urEikAttribute)
        {
            return new UrEikAttributeAdapter(urEikAttribute, stringLocalizer);
        }
        if (attribute is UrEgnAttribute urEgnAttribute)
        {
            return new UrEgnAttributeAdapter(urEgnAttribute, stringLocalizer);
        }
        return baseProvider.GetAttributeAdapter(attribute, stringLocalizer);
    }
}
// </snippet_Class>
