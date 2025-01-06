// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;


namespace URegister.Helper
{
    public static class Utils
    {
        public static string GetDropDownDdlPath(string htmlFieldPrefix)
        {
            return htmlFieldPrefix.Replace(".", "_") + "_ddl";
        }
        public static string GetDropDownDdlPathGlobal(string htmlFieldPrefix)
        {
            var pos = htmlFieldPrefix.IndexOf(".");
            while (pos >= 0)
            {
                htmlFieldPrefix = htmlFieldPrefix.Substring(pos + 1, htmlFieldPrefix.Length - pos - 1);
                pos = htmlFieldPrefix.IndexOf(".");
            }
            return $"{htmlFieldPrefix}_ddl";
        }
        public static List<SelectListItem> GetDropDownDdl<T>(ViewDataDictionary<T> viewData)
        {
            if (viewData == null)
                return new List<SelectListItem>();
            var ddl = (List<SelectListItem>)viewData["Ddl"];
            if (ddl == null)
            {
                var fieldName = Utils.GetDropDownDdlPath(viewData.TemplateInfo.HtmlFieldPrefix);
                ddl = (List<SelectListItem>)viewData[fieldName];
            }
            if (ddl == null)
            {
                var fieldName = Utils.GetDropDownDdlPathGlobal(viewData.TemplateInfo.HtmlFieldPrefix);
                ddl = (List<SelectListItem>)viewData[fieldName];
            }
            ddl = ddl ?? new List<SelectListItem>();
            return ddl;
        }

        public static string EditorAsView(string editorName)
        {
            return $"~/Views/Shared/EditorTemplates/{editorName}.cshtml";
        }
       
        public static bool AddRequiredModelError(ModelMetadata modelMetadata, ModelBindingContext bindingContext)
        {
            var member = modelMetadata.ContainerType?.GetMember(modelMetadata.PropertyName);
            if (member != null)
            {
                // RequiredAttribute
                object[] attribs = member[0].GetCustomAttributes(typeof(RequiredAttribute), false);
                if (attribs.Length > 0)
                {
                    // TODO: ТранслатЕ
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Въведете {bindingContext.ModelName}");
                    return true;
                }
            }
            return false;
        }
        public static bool AddRangeModelError(ModelMetadata modelMetadata, ModelBindingContext bindingContext, object actualValue)
        {
            var member = modelMetadata.ContainerType?.GetMember(modelMetadata.PropertyName);
            if (member != null)
            {
                var attribs = member[0].GetCustomAttributes(typeof(RangeAttribute), false);
                if (attribs.Length > 0)
                {
                    var rangeAttribute = (RangeAttribute)attribs[0];
                    if (!rangeAttribute.IsValid(actualValue))
                    {
                        // TODO: ТранслатЕ 
                        bindingContext.ModelState.AddModelError(bindingContext.ModelName, $"Въведете {bindingContext.ModelName}");
                        return true;
                    }
                }
            }
            return false;
        }
        public static string GetIndexPath(string fieldPrefix)
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
        public static string ResetApplyNumber(string applyNumber)
        {
            if (applyNumber?.Length == 13)
            {
                var year = applyNumber.Substring(0, 4);
                var yearInt = 0;
                int.TryParse(year, out yearInt);
                if (2018 <= yearInt && yearInt <= 2023)
                {
                    applyNumber += "0";
                }
            }
            return applyNumber;
        }
        
    }
}