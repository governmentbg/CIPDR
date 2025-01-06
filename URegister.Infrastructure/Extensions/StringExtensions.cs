using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RulesEngine.HelperFunctions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using URegister.Infrastructure.Attributes;


namespace URegister.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ToPaternSearch(this string model)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                return "%";
            }
            return $"%{model.Replace(" ", "%")}%";
        }
        public static string ToShortCaseNumber(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            if (value.Length < 5)
            {
                return value.PadLeft(5, '0');
            }
            else
            {
                return value;
            }
        }

        public static string ToCasePaternSearch(this string model)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                return "%";
            }
            return "%" + model.ToShortCaseNumber();
        }

        public static string ConcatenateFullName(string givenName, string middleName, string familyName)
        {
            string result = givenName;
            if (!string.IsNullOrEmpty(middleName))
            {
                result += " " + middleName.Trim();
            }
            if (!string.IsNullOrEmpty(familyName))
            {
                result += " " + familyName.Trim();
            }
            return result;
        }

        
        public static string Decode(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return System.Web.HttpUtility.HtmlDecode(value);
        }

        public static string Encode(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            return System.Web.HttpUtility.HtmlEncode(value);
        }

        public static bool ToBooleanEpzeu(this string value)
        {
            return (string.IsNullOrEmpty(value) || value == "0" || value.ToLower() == "false") ? false : true;
        }

        public static DateTime? ToDateEpzeu(this string value)
        {
            DateTime tmpDate;
            DateTime? _date;

            if (string.IsNullOrEmpty(value) == false && DateTime.TryParseExact(value, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out tmpDate))
                _date = tmpDate;
            else
                _date = null;

            return _date;
        }

        public static string ToStringBooleanEpzeu(this bool value)
        {
            return Convert.ToInt32(value).ToString();
        }

        public static string GetIndexPath(this string fieldPrefix)
        {
            if (!string.IsNullOrEmpty(fieldPrefix))
            {
                var pos = 0;
                while (fieldPrefix.IndexOf("[", pos + 1) > 0)
                {
                    pos = fieldPrefix.IndexOf("[", pos + 1);
                }
                if (pos > 0)
                    return fieldPrefix.Substring(0, pos) + ".Index";
            }
            return fieldPrefix;
        }

        public static string GetDropDownDdlPath(this string htmlFieldPrefix)
        {
            return htmlFieldPrefix.Replace(".", "_") + "_ddl";
        }
        public static string GetDropDownDdlPathGlobal(this string htmlFieldPrefix)
        {
            var pos = htmlFieldPrefix.IndexOf(".");
            while (pos >= 0)
            {
                htmlFieldPrefix = htmlFieldPrefix.Substring(pos + 1, htmlFieldPrefix.Length - pos - 1);
                pos = htmlFieldPrefix.IndexOf(".");
            }
            return $"{htmlFieldPrefix}_ddl";
        }
        public static List<SelectListItem> GetDropDownDdl<T>(this ViewDataDictionary<T> viewData)
        {
            if (viewData == null)
                return new List<SelectListItem>();
            var ddl = (List<SelectListItem>)viewData["Ddl"];
            if (ddl == null)
            {
                var fieldName = viewData.TemplateInfo.HtmlFieldPrefix.GetDropDownDdlPath();
                ddl = (List<SelectListItem>)viewData[fieldName];
            }
            if (ddl == null)
            {
                var fieldName = viewData.TemplateInfo.HtmlFieldPrefix.GetDropDownDdlPathGlobal();
                ddl = (List<SelectListItem>)viewData[fieldName];
            }
            ddl = ddl ?? new List<SelectListItem>();
            return ddl;
        }

        public static string GetIOReqClass(this Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata model)
        {
            try
            {
                if (model.ContainerType != null)
                {
                    var Member = model.ContainerType.GetMember(model.PropertyName);
                    var reqTypes = new[] {
                        typeof(RequiredAttribute),
                        typeof(RangeAttribute),
                        typeof(IORequiredAttribute)
                    };
                    var hasIOreq = Member[0].CustomAttributes.Any(a => reqTypes.Contains(a.AttributeType));
                    if (hasIOreq)
                    {
                        return "required";
                    }
                }
            }
            catch { }
            try
            {
                if (model.ContainerType != null)
                {
                    var Member = model.ContainerType.GetMember(model.PropertyName);
                    var reqTypes = new[] {
                        typeof(IOconditionallyRequiredAttribute)
                    };
                    var hasIOreq = Member[0].CustomAttributes.Any(a => reqTypes.Contains(a.AttributeType));
                    if (hasIOreq)
                    {
                        return "io-conditionally-req";
                    }
                }
            }
            catch { }
            return string.Empty;
        }
        

        public static string CombineTemplatePrefix(this string prefix, string add)
        {
            return (string.IsNullOrEmpty(prefix) ? string.Empty : prefix + ".") + add;
        }
    }
}
